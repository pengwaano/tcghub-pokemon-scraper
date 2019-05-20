using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace TheGatherer
{
    class Program
    {
        static List<String> failedIDs = new List<String>();

        static void Main(string[] args)
        {
             RunAsync().GetAwaiter().GetResult();
        }

        static async Task RunAsync()
        {
            UrlBuilder urlBuilder = new UrlBuilder("Sun & Moon", "sm10", "sm", "Unbroken Bonds", "", "UNB");
            var basicCards = await Bulbapedia.getBasicCardsFromBulbapediaList(urlBuilder);

            var cards = new List<Card>();
            foreach (BasicCard bCard in basicCards.GetRange(233,1))
            {
                var card = Pokemon.getCard(urlBuilder, bCard).Result;
                Card bulbaCard = null;

                if (shouldBulbapediaBeUsed(card))
                {
                    Console.WriteLine("Bulbapedia needed: " + bCard.number);
                    bulbaCard = await Bulbapedia.getCard(urlBuilder, bCard);
                }

                if(card != null && bulbaCard != null) {
                    card.combine(bulbaCard);
                }

                if(card == null && bulbaCard != null)
                {
                    card = bulbaCard;
                }

                card = checkImageUrlsWork(card, bCard, urlBuilder);

                if(card != null && !card.isIncomplete()) {
                    cards.Add(card);
                } else {
                    failedIDs.Add(urlBuilder.setCode + "-" + bCard.number);
                }

                Console.WriteLine(card);
            }
            Console.WriteLine("COMPLETE");

            Console.ReadLine();
        }

        static bool shouldBulbapediaBeUsed(Card card)
        {
            if (card == null)
                return true;
            if (card.isIncomplete())
                return true;
            return false;
        }

        static Card checkImageUrlsWork(Card card, BasicCard bCard, UrlBuilder urlBuilder) {
            var image = HtmlUtils.DownloadImageFromUrl(card.imageUrl);
            if (image != null)
                return card;

            var pkmnCardImage = HtmlUtils.DownloadImageFromUrl(urlBuilder.getPkmnCardsUrl(bCard));
            if (pkmnCardImage == null)
                return card;

            var tcghubImageUrl = "https://images.tcghub.co.uk/" + urlBuilder.setCode + "/" + card.number + ".png";
            var tcghubHiresImageUrl = "https://images.tcghub.co.uk/" + urlBuilder.setCode + "/" + card.number + "_hires.png";

            card.imageUrl = tcghubImageUrl;
            card.imageUrlHiRes = tcghubHiresImageUrl;

            return card;
        }

        static bool saveImageToAzure(Image image) {
            var lowRes = HtmlUtils.ResizeImage(image, 245, 343);

            return true;
        }
    }
}
