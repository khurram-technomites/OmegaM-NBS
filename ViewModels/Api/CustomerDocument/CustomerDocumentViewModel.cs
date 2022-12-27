using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.ViewModels.Api.CustomerDocument
{
    public class CustomerDocumentViewModel
    {
        public long ID { get; set; }

        public string Type { get; set; }

        public Nullable<bool> IsGCC { get; set; }
    }
}