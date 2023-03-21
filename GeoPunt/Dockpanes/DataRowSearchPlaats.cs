using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPunt.Dockpanes
{
    internal class DataRowSearchPlaats
    {
        public int id { get; set; }
        public string Thema { get; set; }
        public string Categorie { get; set; }
        public string Type { get; set; }
        public string Naam { get; set; }
        //public string Omschrijving { get; set; }
        public string Straat { get; set; }
        public string Huisnummer { get; set; }
        public string busnr { get; set; }
        public string Gemeente { get; set; }
        public string Postcode { get; set; }
    }
}
