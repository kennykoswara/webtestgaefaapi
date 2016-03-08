using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebTest.PortalList
{
    public class GaefaFilter
    {
        public string titleOrLocation { get; set; }
        public Boolean? include_flight { get; set; }
        public Boolean? include_inn { get; set; }
        
    }
}