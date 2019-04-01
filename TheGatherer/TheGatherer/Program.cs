using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGatherer
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
        }

        static async Task RunAsync()
        {
            UrlBuilder urlBuilder = new UrlBuilder("Sun & Moon", "sm10", "sm", "Detective Pikachu", "");
            var basicCards = await Bulbapedia.getBasicCardsFromBulbapediaList(urlBuilder);

            var cards = new List<Card>();
            foreach(BasicCard bCard in basicCards)
            {
                var card = await Pokemon.getCard(urlBuilder, bCard);
                if(card == null)
                {
                    card = await Bulbapedia.getCard(urlBuilder, bCard);
                }
            }

            Console.ReadLine();
        }
    }
}
