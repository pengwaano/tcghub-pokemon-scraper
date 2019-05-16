using HtmlAgilityPack;
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

            var isTrainer = basicCard.type == null || basicCard.type == "";

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
                                var type = node.SelectSingleNode("a").SelectSingleNode("img").Attributes["alt"].Value;
                                var value = node.InnerText.Replace("resistance", "").Replace("-", "-").Trim();
                                card.resistances = new List<Effect>();
                                card.resistances.Add(new Effect(type, value));
                            }
                        }

                        if (node.InnerText.Contains("retreat"))
                        {
                            var count = node.SelectNodes("img").Count;
                            card.retreatCost = new List<string>();
                            for(int i = 0; i < count; i++)
                            {
                                card.retreatCost.Add("Colorless");
                            }
                            card.convertedRetreatCost = count;
                        }
                    }
                }
            }

            //TEXT

            card.types = new List<string>();
            card.types.Add(basicCard.type);

            //ATTACKS

            //WEAKNESSES

            //RESISTANCES

            //NATIONAL POKEDEX NUMBER

            return card;
        }

        public static async Task<List<BasicCard>> getBasicCardsFromBulbapediaList(UrlBuilder urlBuilder)
        {
            HtmlDocument doc = await HtmlUtils.getHtmlFromLink(urlBuilder.getBulbapediaListUrl());

            return getBasicCardsFromHtmlDocument(doc);
        }

        private static List<BasicCard> getBasicCardsFromHtmlDocument(HtmlDocument doc)
        {
            var node = doc.GetElementbyId("mw-content-text");
            HtmlNode correctTable = null;

            var tables = node.SelectNodes("table");

            foreach (HtmlNode n in tables)
            {
                if (n.GetAttributeValue("cellspacing", "-1") == "0")
                {
                    correctTable = n;
                    break;
                }
            }

            var entries = correctTable.SelectSingleNode("tr").SelectNodes("td").First().SelectSingleNode("table").SelectNodes("tr")[1].SelectSingleNode("td").SelectSingleNode("table").SelectNodes("tr");

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
                } catch(Exception ex){}
                
                basicCard.number = columns[0].InnerText.Split('/')[0].Trim();
                basicCard.name = columns[2].SelectSingleNode("a").GetAttributeValue("title", "").Split('(')[0].Trim().Replace("amp;","");
                basicCard.bulbapediaUrl = columns[2].SelectSingleNode("a").GetAttributeValue("href", "").Replace("/wiki/","");
                basicCard.rarity = columns[3].SelectSingleNode("a").GetAttributeValue("title", "");

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
                    var artist = n.SelectSingleNode("//small").InnerText;
                    var index = artist.IndexOf("Illus.");
                    return artist.Substring(index).Replace("Illus.", "").Trim();
                }
            }
            return null;
        }

        private static string getSubTypeForTrainer(HtmlNode topSection)
        {
            return topSection.SelectSingleNode("//sup").InnerText;
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
    }
}
