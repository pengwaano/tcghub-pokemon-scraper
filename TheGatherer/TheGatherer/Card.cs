using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGatherer
{
    class BasicCard
    {
        public string name { get; set; }
        public string bulbapediaUrl { get; set; }
        public string rarity { get; set; }
        public string number { get; set; }

        public BasicCard()
        {

        }

        public override string ToString()
        {
            return $"{name}, {number}, {rarity}, {bulbapediaUrl}";
        }
    }

    class Card
    {
        public string id { get; set; }
        public string name { get; set; }
        public string imageUrl { get; set; }
        public string subtype { get; set; }
        public string supertype { get; set; }
        public string evolvesFrom { get; set; }
        public Ability ability { get; set; }
        public string hp { get; set; }
        public List<String> retreatCost { get; set; }
        public int convertedRetreatCost { get; set; }
        public string number { get; set; }
        public string artist { get; set; }
        public string rarity { get; set; }
        public string series { get; set; }
        public string set { get; set; }
        public string setCode { get; set; }
        public List<string> types { get; set; }
        public List<Attack> attacks { get; set; }
        public List<Effect> weaknesses { get; set; }
        public List<Effect> resistances { get; set; }
        public string imageUrlHiRes { get; set; }
        public int nationalPokedexNumber { get; set; }

        public Card()
        {

        }

        public bool isCardComplete()
        {
            return id == null || name == null || imageUrl == null || subtype == null || supertype == null || ability.isNull() || hp == null || retreatCost == null || number == null || artist == null ||
                rarity == null || series == null || set == null || setCode == null || types == null || imageUrlHiRes == null;
        }
    }

    class Ability
    {
        public string name { get; set; }
        public string text { get; set; }
        public string type { get; set; }

        public bool isNull()
        {
            return name == null || text == null || type == null;
        }
    }
    class Attack
    {
        public string name { get; set; }
        public List<string> cost { get; set; }
        public int convertedEnergyCost { get; set; }
        public string damage { get; set; }
        public string text { get; set; }
    }
    class Effect
    {
        public string type { get; set; }
        public string value { get; set; }
    }
}