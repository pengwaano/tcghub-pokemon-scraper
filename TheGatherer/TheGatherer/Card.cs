using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TheGatherer
{
    class BasicCard
    {
        public string name { get; set; }
        public string type { get; set; }
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
        public List<String> text { get; set; }
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

        public bool isIncomplete()
        {
            if(supertype != "Trainer" && supertype != "Energy")
            {
                return id == null || name == null || imageUrl == null || subtype == null || supertype == null || hp == null || number == null || artist == null ||
                rarity == null || series == null || set == null || setCode == null || types == null || imageUrlHiRes == null;
            } else
            {
                return id == null || name == null || imageUrl == null || subtype == null || supertype == null || number == null || artist == null ||
                rarity == null || series == null || set == null || setCode == null || imageUrlHiRes == null;
            }
            
        }

        public void combine(Card card) {
            
        }

        public void addTypes(String subtype)
        {
            if (subtype.Contains("Trainer"))
            {
                if (subtype.Contains("Supporter")) {
                    this.subtype = "Supporter";
                }
                if (subtype.Contains("Item") || subtype.Contains("Tool"))
                {
                    this.subtype = "Item";
                }
                if (subtype.Contains("Stadium"))
                {
                    this.subtype = "Stadium";
                }
                supertype = "Trainer";
            }
            else if (subtype.Contains("Energy"))
            {
                this.subtype = subtype.Replace(" Energy", "");
                supertype = "Energy";
            }
            else
            {
                this.subtype = subtype.Replace(" Pokémon", "").Replace("Pokémon-", "").Trim();
                supertype = "Pokémon";
            }
        }

        public bool isTrainerOrEnergy() {
            return supertype == "Trainer" || supertype == "Energy";
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    class Ability
    {
        public string name { get; set; }
        public string text { get; set; }
        public string type { get; set; }

        public Ability(string name, string text, string type)
        {
            this.name = name;
            this.text = text;
            this.type = type;
        }

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

        public Attack() {
            
        }

        public Attack(string name, List<string> cost, string damage, string text)
        {
            this.name = name;
            this.cost= cost;
            this.convertedEnergyCost = cost.Count;
            this.damage = damage;
            this.text = text;
        }
    }
    class Effect
    {
        public string type { get; set; }
        public string value { get; set; }

        public Effect(string type, string value)
        {
            this.type = type;
            this.value = value;
        }
    }
}