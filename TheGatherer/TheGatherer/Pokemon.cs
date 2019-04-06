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

            var card = scrape(doc, urlBuilder, basicCard);


            return new Card();
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

            if(card.isTrainerOrEnergy()) {
                //var $retreat = $stats.find(".stat:contains(Retreat Cost)");
                //card.convertedRetreatCost = $retreat.find(".energy").length;

                //var $artist = $(".illustrator");
                //card.artist = $artist.find(".highlight>a").text()

                //card.text = [];
                //card.text.push(formatText($(".pokemon-abilities").text()));

                return card;
            }

            var evolvedFrom = type.SelectSingleNode("h4");

            if (evolvedFrom != null) {
                card.evolvesFrom = evolvedFrom.SelectSingleNode("a").InnerText.Trim();
            }

            var passive_name = doc.DocumentNode.SelectSingleNode("//div[@class='pokemon-abilities']").SelectSingleNode("h3");
            //if(passive_name != null)
            Console.WriteLine(doc.DocumentNode.SelectSingleNode("//div[@class='pokemon-abilities']").InnerHtml);
            //if (passive_name.length > 0 && passive_name.next()[0].name == "p")
            //{
            //    card.ability = {
            //    name: passive_name.find("div:last-child").text(),
            //text: formatText(passive_name.next().text()),
            //type: "Ability"
            //    };
            //}
            Console.WriteLine(card);

            return new Card();
        }
    }
}
