using System.Collections.Generic;
using System.Web.Http.ModelBinding;

namespace NowBuySell.Web.Helpers
{
	public class ErrorMessages
	{
		public static string[] GetErrorMessages(ModelStateDictionary ModelState)
		{
			var messages = new List<string>();

			foreach (ModelState modelState in ModelState.Values)
			{
				foreach (ModelError error in modelState.Errors)
				{
					if (error.ErrorMessage != "")
					{
						messages.Add(error.ErrorMessage);
					}
					else
					{
						messages.Add(error.Exception.InnerException != null ? error.Exception.InnerException.Message : error.Exception.Message.Split(',')[0].ToString());
					}
				}
			}
			return messages.ToArray();
		}
	}
}