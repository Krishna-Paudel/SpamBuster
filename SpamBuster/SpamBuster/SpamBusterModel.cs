using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Outlook = Microsoft.Office.Interop.Outlook;
using log4net;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;

namespace SpamBuster
{
    public class JsonBody
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string SPFFlag { get; set; }
        public string DKIMFlag { get; set; }
        public string DMARCFlag { get; set; }
    }
    public static class SpamBusterModel 
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SpamBusterModel));

        static HttpClient client = new HttpClient();
        public async static Task<int> getPrediction(Outlook.MailItem mailItem)
        {
            try
            {
                
                log.Debug("Generating the JSON body for API request.");
                string subject = mailItem.Subject;
                string body = mailItem.Body;
                string from = mailItem.SenderEmailAddress;

                //insert ML logic here
                Dictionary<string, string> AuthenticationHeaderPropertyTags = new Dictionary<string, string>
                {
                    { "Authentication-Results", "800F" },
                    { "DKIM-Signature", "8005" },
                    { "DomainKey-Signature", "8006" },  
                    { "Received-SPF", "800C" },
                    { "Received", "007D" },
                    { "DMARC-Result", "806F" }
                };

                string transportHeaders = mailItem.PropertyAccessor.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x007D001E");

                string SPFFlag = getEmailHeaders(transportHeaders, @"spf=([\w/]+)");
                string DKIMFlag = getEmailHeaders(transportHeaders, @"dkim=([\w/]+)");
                string DMARCFlag = getEmailHeaders(transportHeaders, @"dmarc=([\w/]+)");


                JsonBody jsonBody = new JsonBody
                {
                    Subject = mailItem.Subject,
                    Body = mailItem.Body,
                    SPFFlag = SPFFlag,
                    DKIMFlag = DKIMFlag,
                    DMARCFlag = DMARCFlag

                };

                log.Info("API Request is fired to fetch response from the ML Model");


                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                
                var jsonbody = JsonConvert.SerializeObject(jsonBody, Formatting.Indented);

                var apiURL = "https://spambusterv2-afyahd5wcq-uc.a.run.app";

                HttpResponseMessage response = await client.PostAsync(apiURL, new StringContent(jsonbody, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();


                string responseBody = await response.Content.ReadAsStringAsync();

                if(responseBody == "Output: Ham")
                {
                    return 0;
                }
                else 
                { 
                    return 1;
                }

            }
            catch (System.Exception ex)
            {                
                log.Error("Error Occurred!!!");
                log.Error(ex.Message);
                return -1;
            }

        }
        public static string getEmailHeaders(string transportHeaders, string flagPattern)
        {
            string flagvalue = "unknown";

            Match match = Regex.Match(transportHeaders, flagPattern, RegexOptions.IgnoreCase);

            // if a match is found, extract the SPF value
            if (match.Success)
            {
                flagvalue = match.Groups[1].Value;                
            }

            return flagvalue;
        }

     }
}
