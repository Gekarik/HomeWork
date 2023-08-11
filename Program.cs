using OpenQA.Selenium;
using System.Diagnostics;

namespace Home_Work.Scripts
{
    public class CustomException : Exception
    {
        public CustomException(string message) : base(message)
        {

        }
    }

    internal class Program
    {
        static void Main()
        {     
            ProcessKiller();
            for (int i = 0; i < 10; i++)
            {
                using var db = new DBService();
                using var openAi = new OpenAi();
                using var tempMail = new TempMail();
                using var phone = new VakSms();
                using var browser = SeleniumService.CreateBrowser();
                try
                {
                    ((IJavaScriptExecutor)browser).ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined});");

                    tempMail.GenerateDomains();
                    Console.WriteLine("Domains generated");
                    openAi.Registration(browser, tempMail);                    
                    OpenAi.VerifyPhone(browser, phone);                    
                    openAi.ExtractKey(browser);                    
                    db.AddInformation(openAi, phone);
                }
                catch (CustomException ex)
                {
                    if (ex.Message == "Signup Error")
                    {
                        Console.WriteLine("Error: " + ex.Message);
                        browser.Quit();
                        Environment.Exit(0);
                    }
                    Console.WriteLine("Error: " + ex.Message);
                    continue;
                }
            }
        }
        public static void ProcessKiller()
        {
            ProcessStartInfo p;
            p = new ProcessStartInfo("cmd.exe", "/C " + "taskkill /f /im chromedriver.exe");
            using Process proc = new();
            proc.StartInfo = p;
            proc.Start();
            proc.WaitForExit();
            proc.Close();
        }
    }

}

