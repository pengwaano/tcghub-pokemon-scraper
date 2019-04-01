using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        string pokemonBaseUrl = "https://www.pokemon.com/us/pokemon-tcg/pokemon-cards/";
        string bulbapediaBaseUrl = "https://bulbapedia.bulbagarden.net/wiki/";

        public UrlBuilder(string series, string setCode, string seriesCode, string set, string prefix)
        {
            this.series = series;
            this.setCode = setCode;
            this.seriesCode = seriesCode;
            this.set = set;
            this.setUnderscore = set.Replace(" ", "_");
            this.prefix = prefix;
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
    }
}
