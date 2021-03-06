﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TheGatherer
{
    class Bulbapedia
    {
        static Dictionary<String, String> rarityMap = new Dictionary<String, String>()
        {
            { "Ultra-Rare Rare", "Rare Ultra" }
        };

        public static async Task<Card> getCard(UrlBuilder urlBuilder, BasicCard basicCard)
        {
            var doc = await HtmlUtils.getHtmlFromLink(urlBuilder.getBulbapediaCardUrl(basicCard));

            if (doc == null)
            {
                return null;
            }
            return scrape(doc, urlBuilder, basicCard);
        }

        private static Card scrape(HtmlDocument doc, UrlBuilder urlBuilder, BasicCard basicCard)
        {
            var topSection = doc.DocumentNode.SelectSingleNode("//div[@class='mw-content-ltr']").FirstChild;

            var card = new Card();
            card.number = basicCard.number;
            card.id = $"{urlBuilder.setCode}-{basicCard.number}";
            card.name = basicCard.name;
            card.series = urlBuilder.series;
            card.set = urlBuilder.set;
            card.setCode = urlBuilder.setCode;
            card.rarity = basicCard.rarity;
            card.imageUrl = "https://images.pokemontcg.io/" + urlBuilder.setCode + "/" + card.number + ".png";
            card.imageUrlHiRes = "https://images.pokemontcg.io/" + urlBuilder.setCode + "/" + card.number + "_hires.png";
            card.artist = getArtist(topSection);

            var isTrainer = basicCard.type == null || basicCard.type == "" || basicCard.type.EndsWith(" E");
            Console.WriteLine("Type: " + basicCard.type);

            if (isTrainer)
            {
                card.subtype = getSubTypeForTrainer(topSection);
                card.supertype = "Trainer";

                card.text = getTrainerText(doc.DocumentNode);

                return card;
            }

            card.supertype = "Pokémon";

            var statsBar = topSection.SelectNodes("//table")[0];
            var evolutionStageIndex = 0;
            for(int i = 0; i < statsBar.ChildNodes.Count; i++)
            {
                if (statsBar.ChildNodes[i].InnerText.Trim().Contains("Evolution") && statsBar.ChildNodes[i].InnerText.Trim().Contains("stage"))
                {
                    evolutionStageIndex = i;
                }
            }

            var statsEntries = statsBar.ChildNodes[evolutionStageIndex].SelectSingleNode("td").SelectSingleNode("table").SelectNodes("tr");

            foreach(HtmlNode n in statsEntries)
            {
                if(n.InnerText.Contains("Evolution") && n.InnerText.Contains("stage"))
                {
                    var subtype = n.SelectSingleNode("td").InnerText;
                    card.subtype = subtype.Substring(0, subtype.IndexOf("Pokémon")).Trim();
                } else if(n.InnerText.Contains("Hit Points"))
                {
                    card.hp = n.SelectSingleNode("td").InnerText.Trim();
                } else if(n.InnerText.Contains("weakness"))
                {
                    var entries = n.SelectSingleNode("td").SelectSingleNode("table").SelectSingleNode("tr").SelectNodes("th");
                    foreach (HtmlNode node in entries)
                    {
                        if (node.InnerText.Contains("weakness"))
                        {
                            if (!node.InnerText.Contains("None"))
                            {
                                var type = node.SelectSingleNode("a").SelectSingleNode("img").Attributes["alt"].Value;
                                var value = node.InnerText.Replace("weakness", "").Replace("×", "x").Trim();
                                card.weaknesses = new List<Effect>();
                                card.weaknesses.Add(new Effect(type, value));
                            }
                        }

                        if (node.InnerText.Contains("resistance"))
                        {
                            if (!node.InnerText.Contains("None"))
                            {
                                Console.WriteLine(node.InnerHtml);
                                card.resistances = new List<Effect>();
                                var types = node.Descendants();
                                foreach(HtmlNode typeNode in types)
                                {
                                    if(typeNode.Name == "a")
                                    {
                                        var type = typeNode.SelectSingleNode("img").Attributes["alt"].Value;
                                        var value = node.InnerText.Replace("resistance", "").Replace("-", "-").Trim();
                                        card.resistances.Add(new Effect(type, value));
                                    }
                                }
                            }
                        }

                        if (node.InnerText.Contains("retreat"))
                        {
                            if (!node.InnerText.Contains("None"))
                            {
                                var count = node.SelectNodes("img").Count;
                                card.retreatCost = new List<string>();
                                for (int i = 0; i < count; i++)
                                {
                                    card.retreatCost.Add("Colorless");
                                }
                                card.convertedRetreatCost = count;
                            } else{
                                card.convertedRetreatCost = 0;
                            }
                        }
                    }
                }
            }

            card.text = new List<string>();

            card.types = new List<string>();
            card.types.Add(basicCard.type);

            card.attacks = new List<Attack>();
            try
            {
                var tables = doc.DocumentNode.SelectSingleNode("//div[@class='mw-content-ltr']").SelectNodes("table[@class='roundy']");
                for (int index = 0; index < tables.Count; index++)
                {
                    if(index == 0) 
                    {
                        var node = tables[index];
                        var text = node.InnerText;
                        var sections = node.SelectNodes("tr");

                        for (int i = 0; i < sections.Count; i++) {
                            if (sections[i].InnerHtml.Contains("<big><i><b>") && sections[i].InnerHtml.Contains("</b></i></big>"))
                                continue;

                            var descendants = sections[i].Descendants();

                            var entryType = "Attack";

                            foreach (HtmlNode child in descendants)
                            {
                                if (child.Name == "a")
                                {
                                    if (child.Attributes["title"].Value == "Ability" || child.Attributes["title"].Value == "Poké-BODY" || child.Attributes["title"].Value == "Poké-POWER" || child.Attributes["title"].Value == "Pokémon Power (TCG)")
                                    {
                                        entryType = child.Attributes["title"].Value;
                                        break;
                                    }
                                }
                            }

                            if(i == sections.Count - 1) {
                                card.text.Add(sections[i].InnerText.Trim());
                                continue;
                            }

                            if(entryType == "Attack") {
                                var attackSections = sections[i].SelectSingleNode("td/table/tr").SelectNodes("th");
                                if (attackSections == null)
                                    continue;
                                var energies = attackSections[0];
                                var energyList = new List<string>();

                                foreach(HtmlNode a in energies.Descendants()) {
                                    if(a.Name == "img") {
                                        energyList.Add(a.Attributes["alt"].Value);
                                    }
                                }

                                var name = attackSections[1];
                                name.SelectSingleNode("small").Remove();
                                var damage = attackSections[2].InnerText.Trim();
                                var attackText = "";
                                if(sections.Count > i + 1) {
                                    attackText = sections[i+1].InnerText.Trim();
                                    i += 1;
                                }
                                card.attacks.Add(new Attack(name.InnerText.Trim(), energyList, damage, attackText));

                            } else {
                                card.ability = getAbility(sections, entryType, i);
                                i++;
                            }
                        }
                    } 
                    else if(index == 1)
                    {
                        var node = tables[index];
                        var text = node.InnerText;
                        if (text.Contains("Height") && text.Contains("Weight") && text.Contains("No."))
                        {
                            var pokedexValue = node.SelectNodes("tr")[2].SelectNodes("td")[0];
                            card.nationalPokedexNumber = Int32.Parse(pokedexValue.InnerText);
                        }
                    }
                }
            } catch(Exception e) {
                Console.WriteLine("WOMP WOMP");
                Console.WriteLine(e.Message + " " + e.StackTrace);
            }

            return card;
        }

        public static async Task<List<BasicCard>> getBasicCardsFromBulbapediaList(UrlBuilder urlBuilder)
        {
            HtmlDocument doc = await HtmlUtils.getHtmlFromLink(urlBuilder.getBulbapediaListUrl());

            return getBasicCardsFromHtmlDocument(doc, urlBuilder.set);
        }

        private static List<BasicCard> getBasicCardsFromHtmlDocument(HtmlDocument doc, string set)
        {
            var node = doc.GetElementbyId("mw-content-text");
            HtmlNode correctTable = null;

            var tables = node.SelectNodes("table");

            foreach (HtmlNode n in tables)
            {
                if (n.InnerText.Contains(set) && n.InnerText.Contains("Card name"))
                {
                    correctTable = n;
                    break;
                }
            }

            HtmlNode correctList = null;

            //Console.WriteLine("List: " + correctTable.InnerHtml);

            foreach (HtmlNode child in correctTable.Descendants())
            {
                if(child.Name == "table")
                {
                    if (child.InnerText.Contains("Card name"))
                    {
                        correctList = child;
                        break;
                    }
                }
            }

            //Console.WriteLine("List: " + correctList.InnerHtml);

            var entries = correctList.SelectNodes("tr");

            //var entries = correctTable.SelectSingleNode("tr").SelectNodes("td").First().SelectSingleNode("table").SelectNodes("tr")[1].SelectSingleNode("td").SelectSingleNode("table").SelectNodes("tr");

            var basicCards = new List<BasicCard>();
            for(int i = 1; i < entries.Count - 1; i++)
            {
                var basicCard = new BasicCard();
                var columns = entries[i].SelectNodes("td");

                try
                {
                    var typeColumn = entries[i].SelectSingleNode("th").SelectSingleNode("a");
                    if(typeColumn == null)
                    {
                        typeColumn = entries[i].SelectSingleNode("th").SelectSingleNode("img");
                    } else
                    {
                        typeColumn = typeColumn.SelectSingleNode("img");
                    }
                    var type = typeColumn.GetAttributeValue("alt", "");
                    basicCard.type = type;

                    if(entries[i].SelectSingleNode("th").InnerText.Trim() != "") {
                        basicCard.type += " " + entries[i].SelectSingleNode("th").InnerText.Trim();
                    }

                } catch(Exception ex){}
                
                basicCard.number = columns[0].InnerText.Split('/')[0].Trim();

                if(basicCard.number == "190") {
                    Console.WriteLine("Type: "+ entries[i].InnerHtml);
                }

                basicCard.name = columns[2].SelectSingleNode("a").GetAttributeValue("title", "").Split('(')[0].Trim().Replace("amp;","");
                basicCard.bulbapediaUrl = columns[2].SelectSingleNode("a").GetAttributeValue("href", "").Replace("/wiki/","");
                try
                {
                    basicCard.rarity = columns[3].SelectSingleNode("a").GetAttributeValue("title", "");
                } catch( Exception e)
                {
                    basicCard.rarity = "Promo";
                }
                

                if(rarityMap.ContainsKey(basicCard.rarity))
                {
                    basicCard.rarity = rarityMap[basicCard.rarity];
                }

                basicCards.Add(basicCard);
            }

            return basicCards;
        }

        private static string getArtist(HtmlNode topSection)
        {
            foreach (HtmlNode n in topSection.ChildNodes)
            {
                if (n.InnerText.Contains("Illus."))
                {
                    var artist = n.Descendants("small").First().InnerText;
                    var index = artist.IndexOf("Illus.");
                    return artist.Substring(index).Replace("Illus.", "").Trim();
                }
            }
            return null;
        }

        private static string getSubTypeForTrainer(HtmlNode topSection)
        {
            var node = topSection.SelectSingleNode("//sup");
            if (node != null)
                return node.InnerText;
            else
                return "Special";
        }

        private static List<string> getTrainerText(HtmlNode doc)
        {
            var list = new List<string>();
            foreach (HtmlNode n in doc.SelectSingleNode("//table[@class='roundy']").ChildNodes)
            {
                if (!n.HasAttributes && n.InnerText.Trim().Length > 0)
                {
                    list.Add(n.InnerText.Trim());
                }
            }
            return list;
        }

        private static Ability getAbility(HtmlNodeCollection sections, String entryType, int index) {
            var abilityHtml = sections[index].SelectSingleNode("td/table/tr");
            var nameNode = abilityHtml.SelectNodes("th")[0].SelectSingleNode("table/tr/td");
            nameNode.SelectSingleNode("small").Remove();
            var type = entryType;
            var name = nameNode.InnerText.Trim();
            index += 1;
            return new Ability(name, sections[index].InnerText.Trim(), type);
        }
    }
}
