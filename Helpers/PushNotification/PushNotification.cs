using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NowBuySell.Web.Helpers.FCMNotifications;

namespace NowBuySell.Web.Helpers.PushNotification
{
    public class PushNotification
    {
        
        private static Uri FireBasePushNotificationsURL = new Uri("https://fcm.googleapis.com/fcm/send");
        private static string ServerKey = "AAAAin0vtMM:APA91bFtpXqzPwWHVGThNpZH0gD2Xr95I5YKNnXE8wA1Rf8ZTE2tF9nafdY-LmcLJa1HhJJcDXypl_hsdmdQpOFzwnTyFxm7bQW3YkKDl4R-6WUV4QXaD6rSoMMuGuR-0pJBh-wMiq-2";
        private static string ServerKeyForVendor = "AAAAHh2CWPM:APA91bG-Bp3ZdeyJ1OCInItYcIUQ8ijP0mwTlaHIww0RydxX9wmOZdXpyCY_yWMU4E-IJ5BSXz19IRI4_iyJxR4CyFoLvowCyNH9KAhlpY8ly9BW1UXV9W47tRo5QsO48hJZS06ewL7m";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceTokens">List of all devices assigned to a user</param>
        /// <param name="title">Title of notification</param>
        /// <param name="body">Description of notification</param>
        /// <param name="data">Object with all extra information you want to send hidden in the notification</param>
        /// <returns></returns>
        public static async Task<bool> SendPushNotification(string[] deviceTokens, string title, string body, object data,bool forCustomer = true)
        {
            bool sent = false;

            if (deviceTokens.Count() > 0)
            {
                //Object creation

                var messageInformation = new Message()
                {
                    notification = new Notification()
                    {
                        title = title,
                        body = body,
						sound = "default",
					},
                    data = data,
					sound = "default",
					registration_ids = deviceTokens
                };

                //Object to JSON STRUCTURE => using Newtonsoft.Json;
                string jsonMessage = JsonConvert.SerializeObject(messageInformation);

                /*
                 ------ JSON STRUCTURE ------
                 {
                    notification: {
                                    title: "",
                                    text: ""
                                    },
                    data: {
                            action: "Play",
                            playerId: 5
                            },
                    registration_ids = ["id1", "id2"]
                 }
                 ------ JSON STRUCTURE ------
                 */

                //Create request to Firebase API
                var request = new HttpRequestMessage(HttpMethod.Post, FireBasePushNotificationsURL);
                if (forCustomer)
                {
                    request.Headers.TryAddWithoutValidation("Authorization", "key=" + ServerKey);
                }

                else
                {
                    request.Headers.TryAddWithoutValidation("Authorization", "key=" + ServerKeyForVendor);
                }
                
                request.Content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

                HttpResponseMessage result;
                using (var client = new HttpClient())
                {
                    result = await client.SendAsync(request);
                    sent = sent && result.IsSuccessStatusCode;
                }
            }

            return sent;
        }
    }
}