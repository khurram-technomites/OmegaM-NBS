using System;
using System.IO;
using System.Xml.Serialization;

namespace NowBuySell.Web.Helpers
{
	public class XML
	{

		public static String ObjectToXMLGeneric<T>(T filter)
		{

			string xml = null;
			using (StringWriter sw = new StringWriter())
			{
				XmlSerializer xs = new XmlSerializer(typeof(T));
				xs.Serialize(sw, filter);
				try
				{
					xml = sw.ToString();

				}
				catch (Exception e)
				{
					throw e;
				}
			}
			return xml;
		}
	}
}