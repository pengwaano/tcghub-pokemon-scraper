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
            HttpClient client = new HttpClient();
            string html = null;
            try
            {
                html = await client.GetStringAsync(urlBuilder.getPokemonCardUrl(basicCard.number));
            }
            catch (Exception ex)
            {
                return null;
            }

            Console.WriteLine(html);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            
            return new Card();
        }
    }
}
