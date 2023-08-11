using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;


namespace Home_Work.Scripts
{
    internal class TempMail : IDisposable
    {
        public string Email { get; set; }
        public string Apikey { get; set; }
        public List<string> Domains { get; set; }
        public string Domain;
        public string Hash { get; set; }
        public string Url { get; set; }
        readonly string pattern = @"https://auth0\.openai\.com\S+";
        public void Dispose()
        {
            Email = null;
            Apikey = null;
            Domains = null;
            Domain = null;
            Hash = null;
            Url = null;
        }
        public void ChooseDomain()
        {
            var rand = new Random();
            if (Domains != null)
                Domain = Domains[rand.Next(0, Domains.Count - 1)];
            else
                throw new Exception("Domains was null");
        }

        public void GenerateDomains()
        {
            int counter = 0;
            List<string>? exceptions = ConfigLoader.GetConfiguration().GetSection("TempMailDomainsExceptions").Get<List<string>>();
            while (true)
            {
                List<string>? tempMailKeys = ConfigLoader.GetConfiguration().GetSection("TempMailApiKeys").Get<List<string>>();
                if (tempMailKeys != null)
                    Apikey = tempMailKeys[counter];
                var client = new RestClient("https://api.apilayer.com/temp_mail/domains");

                var request = new RestRequest(Method.GET);
                request.AddHeader("apikey", $"{Apikey}");

                IRestResponse response = client.Execute(request);
                if (!response.IsSuccessful)
                    counter++;
                else
                {
                    Domains = JsonConvert.DeserializeObject<List<string>>(response.Content).Except(exceptions).ToList();
                    ChooseDomain();
                    break;
                }
            }
        }
        public void EmailToHash()
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(Email);
            byte[] hashBytes = MD5.HashData(inputBytes);
            var sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            Hash = sb.ToString();
        }
        public void ExtractVerifyUrl()
        {
            EmailToHash();

            var client = new RestClient($"https://api.apilayer.com/temp_mail/mail/id/{Hash}")
            {
                Timeout = -1
            };

            var request = new RestRequest(Method.GET);
            request.AddHeader("apikey", $"{Apikey}");
            var response = client.Execute(request);

            while (response.Content.Contains("There are no emails yet"))
            {
                Thread.Sleep(2000);
                response = client.Execute(request);
            }
            MatchCollection matches = Regex.Matches(response.Content, pattern);
            Url = matches[0].Value;
        }
    }
}
