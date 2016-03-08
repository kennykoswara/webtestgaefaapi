using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebTest.PortalList
{
    public class GaefaPackageSort
    {
        public enum sort_type
        {
            location = 0,
            price_per_pack = 1,
            start_package = 2,
            cdate = 3,
            title = 4,
        }

        public enum sort_mode
        {
            ASCENDING = 0,
            DESCENDING = 1,
        }

        public sort_type sortType { get; set; }
        public sort_mode sortMode { get; set; }
        
    }
}