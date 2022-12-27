using System.Configuration;
using System.Web.Mvc;

namespace NowBuySell.Web
{
    public static class UrlHelperExtensions
    {
        public static string ContentVersioned(this UrlHelper self, string contentPath)
        {
            var BuildVersion = ConfigurationManager.AppSettings["BuildVersion"];
            string versionedContentPath = contentPath + "?v=" + BuildVersion;
            return self.Content(versionedContentPath);
        }
    }
}