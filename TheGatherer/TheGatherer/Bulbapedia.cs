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
            return new Card();
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
    }
}
