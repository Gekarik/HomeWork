using Newtonsoft.Json;
using RestSharp;

namespace Home_Work.Scripts
{
    internal class VakSms: IDisposable
    { 
        public string Tel { get; set; }        
        public string SmsCode { get; set; }        
        public string IdNum { get; set; }
        private readonly string _apikey = ConfigLoader.GetConfiguration()["VakApiKeyMain"];        
        private readonly string _service = "dr";
        private readonly string _oper = ConfigLoader.GetConfiguration()["Operator"];
        private readonly string _country = ConfigLoader.GetConfiguration()["Country"];
        public string ApiKey
        {
            get { return _apikey; }
        }

        public string Service
        {
            get { return _service; }
        }

        public string Operator
        {
            get { return _oper; }
        }

        public string Country
        {
            get { return _country; }
        }
        private readonly string _url = "https://vak-sms.com/api/";        
        private static IRestResponse ExecuteRequestWithRetry(RestClient client, RestRequest request)
        {
            int maxRetries = 3;
            int retryDelayMilliseconds = 5000;
            IRestResponse response = null;

            for (int i = 0; i < maxRetries; i++)
            {
                response = client.Execute(request);
                if (response.IsSuccessful)
                {
                    return response;
                }
                Thread.Sleep(retryDelayMilliseconds);
            }

            return response;
        }
        public void BuyNumber()
        {
            int counter = 0;
            var client = new RestClient(_url);
            var request = new RestRequest("getNumber/", Method.GET);
            request.AddParameter("apiKey", _apikey);
            request.AddParameter("service", _service);
            request.AddParameter("country", _country);
            request.AddParameter("operator", _oper);

            while (counter < 5)
            {
                var response = ExecuteRequestWithRetry(client, request);
                if (response.IsSuccessful)
                {
                    var jsonObj = JsonConvert.DeserializeObject<VakSms>(response.Content);//обработать пустые
                    if (jsonObj != null)
                    {
                        Tel = jsonObj.Tel[2..];
                        IdNum = jsonObj.IdNum;
                        break;
                    }
                }
                else
                {                    
                    Console.WriteLine($"Error_VakSms: Request failed with status code {response.StatusCode}");
                }

                Console.WriteLine("No answer from Vak-Sms. Repeating...");
                Thread.Sleep(5000);
                counter++;
            }

            if (counter >= 5)
            {
                Console.WriteLine("Vak-Sms is dead... :( ");
            }
        }       

        public void ReceiveSmsCode()
        {
            int counter = 0;
            var client = new RestClient(_url);
            var request = new RestRequest("getSmsCode/", Method.GET);
            request.AddParameter("apiKey", _apikey);
            request.AddParameter("idNum", IdNum);

            VakSms jsonObj;

            while (SmsCode == null || SmsCode.Length == 0)
            {
                if (counter < 9)
                {
                    Thread.Sleep(10000);
                    var response = client.Execute(request);
                    if (response.IsSuccessful)
                    {
                        jsonObj = JsonConvert.DeserializeObject<VakSms>(response.Content);
                        if (jsonObj != null && !string.IsNullOrEmpty(jsonObj.SmsCode))
                        {
                            SmsCode = jsonObj.SmsCode;
                            break;
                        }
                    }
                    else
                    {                        
                        Console.WriteLine($"Error: Request failed with status code {response.StatusCode}");
                    }
                    counter++;
                }
                else
                {
                    KillNumber();                    
                    break;
                }
            }
        }
        public void KillNumber()
        {
            var client = new RestClient(_url);
            var request = new RestRequest("setStatus/", Method.GET);
            request.AddParameter("apiKey", _apikey);
            request.AddParameter("idNum", IdNum);
            request.AddParameter("status", "bad");
            client.Execute(request);
        }
        public void Dispose()
        {
            Tel = null;
            SmsCode = null;
            IdNum = null;
        }

    }
}
