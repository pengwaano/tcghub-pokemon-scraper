using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGatherer
{
    class Program
    {
        static List<String> failedIDs = new List<String>();

        static void Main(string[] args)
        {
            Console.WriteLine("Started");
            RunAsync().GetAwaiter().GetResult();
        }

        static async Task RunAsync()
        {
            UrlBuilder urlBuilder = new UrlBuilder("Sun & Moon", "sm9", "sm", "Team Up", "");
            var basicCards = await Bulbapedia.getBasicCardsFromBulbapediaList(urlBuilder);

            var cards = new List<Card>();
            foreach(BasicCard bCard in basicCards.GetRange(13,3))
            {
                var card = await Pokemon.getCard(urlBuilder, bCard);
                Card bulbaCard = null;
                if(card == null || card.isIncomplete())
                {
                    bulbaCard = await Bulbapedia.getCard(urlBuilder, bCard);
                }

                if(card != null && bulbaCard != null) {
                    card.combine(bulbaCard);
                }

                if(card != null && !card.isIncomplete()) {
                    cards.Add(card); 
                } else if(bulbaCard != null && !bulbaCard.isIncomplete()) {
                    cards.Add(bulbaCard);
                } else {
                    failedIDs.Add(urlBuilder.setCode + "-" + bCard.number);
                }
            }

            Console.ReadLine();
        }
    }
}
