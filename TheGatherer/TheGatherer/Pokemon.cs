using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TheGatherer
{
    class Pokemon
    {
        public static async Task<Card> getCard(UrlBuilder urlBuilder, BasicCard basicCard)
        {
            var doc = await HtmlUtils.getHtmlFromLink(urlBuilder.getPokemonCardUrl(basicCard.number));

            if(doc == null) {
                return null;
            }

            return scrape(doc, urlBuilder, basicCard);
        }

        private static Card scrape(HtmlDocument doc, UrlBuilder urlBuilder, BasicCard basicCard) {
            var card = new Card();

            card.number = basicCard.number;
            card.id = $"{urlBuilder.setCode}-{basicCard.number}";
            card.series = urlBuilder.series;
            card.set = urlBuilder.set;
            card.setCode = urlBuilder.setCode;
            card.rarity = basicCard.rarity;
            card.imageUrl = "https://images.pokemontcg.io/" + urlBuilder.setCode + "/" + card.number + ".png";
            card.imageUrlHiRes = "https://images.pokemontcg.io/" + urlBuilder.setCode + "/" + card.number + "_hires.png";

            var cardDescription = doc.DocumentNode.SelectSingleNode("//div[@class='card-description']");

            card.name = cardDescription.SelectSingleNode("//h1").InnerText;

            var basicInfo = doc.DocumentNode.SelectSingleNode("//div[@class='card-basic-info']");

            var type = basicInfo.SelectSingleNode("//div[@class='card-type']");

            card.addTypes(type.SelectSingleNode("//h2").InnerText);

            var stats = doc.DocumentNode.SelectSingleNode("//div[@class='pokemon-stats']");

            card.artist = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'illustrator')]").ChildNodes[1].ChildNodes[1].InnerText.Trim();

            if (card.isTrainerOrEnergy()) {
                var cardText = doc.DocumentNode.SelectSingleNode("//div[@class='pokemon-abilities']");
                
                foreach(HtmlNode node in cardText.Descendants())
                {
                    if(node.Name == "pre")
                    {
                        card.text = new List<String>();
                        card.text.Add(node.InnerText);
                    }
                }

                return card;
            }

            var evolvedFrom = type.SelectSingleNode("h4");

            if (evolvedFrom != null) {
                card.evolvesFrom = evolvedFrom.SelectSingleNode("a").InnerText.Trim();
            }

            var ability = doc.DocumentNode.SelectSingleNode("//div[@class='pokemon-abilities']").SelectSingleNode("h3");
            if(ability != null)
            {
                var abilityType = ability.ChildNodes[1].InnerText.Trim();
                var abilityName = ability.ChildNodes[3].InnerText.Trim();
                var abilityText = ability.NextSibling.NextSibling.InnerText.Trim();

                card.ability = new Ability(abilityName, abilityText, abilityType);
            }

            card.hp = basicInfo.SelectSingleNode("//div[contains(@class, 'right')]").InnerText.Trim().Replace("\n", "").Replace("HP", "");

            card.types = new List<string>();
            card.types.Add(basicCard.type);

            var weakness = stats.ChildNodes[1];
            if(weakness.ChildNodes.Count > 3)
            {
                foreach (HtmlNode node in weakness.SelectSingleNode("ul").ChildNodes)
                {
                    if (node.Name == "li")
                    {
                        card.weaknesses = new List<Effect>();
                        card.weaknesses.Add(new Effect(node.GetAttributeValue("title",""), node.InnerText.Trim()));
                    }
                }
            }

            var resistance = stats.ChildNodes[3];
            if (resistance.ChildNodes.Count > 3)
            {
                foreach (HtmlNode node in resistance.SelectSingleNode("ul").ChildNodes)
                {
                    if (node.Name == "li")
                    {
                        card.resistances = new List<Effect>();
                        card.resistances.Add(new Effect(node.GetAttributeValue("title", ""), node.InnerText.Trim()));
                    }
                }
            }
            var retreat = stats.ChildNodes[5];
            if (retreat.ChildNodes.Count > 3)
            {
                var count = 0;
                card.retreatCost = new List<String>();
                foreach(HtmlNode node in retreat.ChildNodes[3].ChildNodes)
                {
                    if (node.Name == "li")
                    {
                        card.retreatCost.Add("Colorless");
                        count++;
                    }
                }
                card.convertedRetreatCost = count;
            }

            var pokedexNumber = doc.GetElementbyId("pokedex-find");
            card.nationalPokedexNumber = Int32.Parse(pokedexNumber.GetAttributeValue("data-pokemon-number", ""));

            card.attacks = new List<Attack>();

            var attackBase = doc.DocumentNode.SelectSingleNode("//div[@class='pokemon-abilities']");

            foreach(HtmlNode node in attackBase.ChildNodes)
            {
                if(node.InnerHtml.Contains("span"))
                {
                    var attackNode = node.SelectSingleNode("ul");
                    if (attackNode == null)
                        continue;
                    var attackEnergies = attackNode.SelectNodes("li");
                    var attack = new Attack();
                    attack.name = node.SelectSingleNode("h4").InnerText;
                    attack.text = node.SelectSingleNode("pre").InnerText;
                    if(node.SelectSingleNode("span") != null)
                        attack.damage = node.SelectSingleNode("span").InnerText;

                    attack.convertedEnergyCost = attackEnergies.Count;
                    if(attack.convertedEnergyCost > 0)
                    {
                        attack.cost = new List<string>();
                        foreach (HtmlNode energy in attackEnergies)
                        {
                            var energyType = energy.SelectSingleNode("a").GetAttributeValue("data-energy-type", "");
                            attack.cost.Add(energyType);
                        }
                    }
                    card.attacks.Add(attack);
                }
            }

            //Console.WriteLine(card);

            return card;
        }
    }
}
