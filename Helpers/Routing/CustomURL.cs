using System.Configuration;

namespace NowBuySell.Web.Helpers.Routing
{

	public class CustomURL
	{
		public static string GetFormatedURL(string url)
		{
			var ServiceProviderUrl = ConfigurationManager.AppSettings["DomainUrl"];
			return string.Format("{0}{1}", ServiceProviderUrl, url.Replace("~", ""));
		}

		public static string GetImageServer()
		{
			return ConfigurationManager.AppSettings["ImageServer"];
		}
	}
}