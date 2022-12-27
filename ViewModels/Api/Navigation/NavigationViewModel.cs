using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Api.Navigation
{
    public class NavigationViewModel
    {
        public List<NavigationProperties> Navigation { get; set; }
    }

    public class NavigationProperties
    {
        public int ID { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
    }
}