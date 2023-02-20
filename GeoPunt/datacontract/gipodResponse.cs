﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPunt.datacontract
{
    public class workassignment
    {
        public int gipodId { get; set; }
        public string owner { get; set; }
        public string description { get; set; }
        public DateTime startDateTime { get; set; }
        public DateTime endDateTime { get; set; }
        public bool importantHindrance { get; set; }
        public geojsonPoint coordinate { get; set; }
        public string detail { get; set; }
        public List<string> cities { get; set; }
    }

    public class gipodResponse : workassignment //workassignment is a subset of manifestation
    {                                           //so manifestation is a generic response contract
        public string initiator { get; set; }
        public string eventType { get; set; }
        public string recurrencePattern { get; set; }
    }
}
