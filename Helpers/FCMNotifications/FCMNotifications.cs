namespace NowBuySell.Web.Helpers.FCMNotifications
{
    public class Message
    {
        public string[] registration_ids { get; set; }
        public Notification notification { get; set; }
        public object data { get; set; }
		public string sound { get; set; }

	}
    public class Notification
    {
        public string title { get; set; }
        public string body { get; set; }
		public string sound { get; set; }
	}
}