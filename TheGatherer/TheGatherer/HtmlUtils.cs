using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace TheGatherer
{
    static class HtmlUtils
    {
        public static async Task<HtmlDocument> getHtmlFromLink(String url) {
            HttpClient client = new HttpClient();
            HtmlDocument doc = new HtmlDocument();
            string html = null;
            try
            {
                html = await client.GetStringAsync(url);
                doc.LoadHtml(html);
            }
            catch (Exception ex)
            {
                return null;
            }
            return doc;
        }

    }
}
