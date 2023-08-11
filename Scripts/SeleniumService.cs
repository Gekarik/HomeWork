using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;

namespace Home_Work.Scripts
{
    internal static class SeleniumService
    {
        public static ChromeDriver CreateBrowser()
        {
            var service = ChromeDriverService.CreateDefaultService(Path.GetFullPath("../../../"));
            service.HideCommandPromptWindow = true;

            var options = SetOptions();
            var browser = new ChromeDriver(service,options);
            browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            return browser;
        }
        public static ChromeOptions SetOptions()
        {
            var options = new ChromeOptions
            {
                PageLoadStrategy = PageLoadStrategy.Normal
            };
            options.AddArguments(
            $"user-agent={ConfigLoader.GetConfiguration()["user-agent"]}",//должен меняться после парса
            "--disable-blink-features=AutomationControlled",
            "--start-maximized",
            "disable-gpu",
            "--no-sandbox",
            //"--headless=new",
            "--disable-extensions",
            "--log-level=0",
            "--no-sandbox",
            "--disable-logging"
            );
            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("profile.password_manager_enabled", false);
            options.AddExcludedArgument("enable-automation");
            options.SetLoggingPreference(LogType.Browser, LogLevel.Off);           

            return options;
        }


    }
}

