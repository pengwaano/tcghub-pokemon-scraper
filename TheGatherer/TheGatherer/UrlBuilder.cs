using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TheGatherer
{
    class UrlBuilder
    {
        public string series { get; set; }
        public string setCode { get; set; }
        public string seriesCode { get; set; }
        public string set { get; set; }
        public string setUnderscore { get; set; }
        public string prefix { get; set; }
        public string pkmncardAbbrev { get; set; }

        string pokemonBaseUrl = "https://www.pokemon.com/us/pokemon-tcg/pokemon-cards/";
        string bulbapediaBaseUrl = "https://bulbapedia.bulbagarden.net/wiki/";

        public UrlBuilder(string series, string setCode, string seriesCode, string set, string prefix, string pkmncardAbbrev)
        {
            this.series = series;
            this.setCode = setCode;
            this.seriesCode = seriesCode;
            this.set = set;
            this.setUnderscore = set.Replace(" ", "_");
            this.prefix = prefix;
            this.pkmncardAbbrev = pkmncardAbbrev;
        }

        public string getPokemonCardUrl(string id)
        {
            return pokemonBaseUrl + seriesCode + "-series/" + setCode + "/" + prefix + id + "/";
        }

        public string getBulbapediaListUrl()
        {
            return bulbapediaBaseUrl + setUnderscore + "_(TCG)";
        }

        public string getBulbapediaCardUrl(BasicCard bCard)
        {
            return bulbapediaBaseUrl + bCard.bulbapediaUrl;
        }

        public string getPkmnCardsUrl(BasicCard bCard)
        {
            return ("https://pkmncards.com/wp-content/uploads/" + hyphenateName(bCard.name) + "-" + hyphenateSet(set) + "-" + pkmncardAbbrev + "-" + bCard.number + ".png").ToLower();
        }

        private string hyphenateName(String name) {
            Regex rgx = new Regex("[^a-zA-Z0-9\\s -]");
            name = rgx.Replace(name, "");
            name = name.Replace(" ", "-");

            return name;
        }

        private string hyphenateSet(String set)
        {
            return set.Replace(" ", "-");
        }
    }
}
