using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NowBuySell.Web.ViewModels;

namespace NowBuySell.Web.Helpers
{
    public class HTMLTagsSplitter
    {
        public List<HTMLTagsSplitResultView> ToSplitList(string remark)
        {
            List<HTMLTagsSplitResultView> ReturnList = new List<HTMLTagsSplitResultView>();

            if (!string.IsNullOrEmpty(remark))
            {
                string[] seperator = { "<hr />", "<br />" };
                List<string> List = remark.Split(seperator, StringSplitOptions.RemoveEmptyEntries).ToList();

                for (int i = 0; List.Count() % 2 != 0 ? i < List.Count() - 1 : i < List.Count(); i += 2)
                {
                    ReturnList.Add(new HTMLTagsSplitResultView
                    {
                        Date = List[i],
                        Comment = List[i + 1]
                    });
                }
            }

            return ReturnList;
        }
    }
}