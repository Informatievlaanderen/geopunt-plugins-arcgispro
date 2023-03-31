using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPunt.datacontract
{
    class dhmRequest
    {
        public int Samples { get; set; }
        public int SrsIn { get; set; }
        public int SrsOut { get; set; }
        public geojsonLine LineString { get; set; }
    }
}
