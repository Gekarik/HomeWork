using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Faker;
using System.Security.Cryptography;
using OpenQA.Selenium.Support.UI;

namespace Home_Work.Scripts
{
    internal class OpenAi : IDisposable
    {
        public void Dispose()
        {
            FirstName = null;
            LastName = null;
            Email = null;
            Password = null;
            Apikey = null;
            DeathTime = null;
            RegTime = new DateTime();
            Birthday = new DateTime();
        }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }        
        public string? Apikey { get; set; }
        public DateTime Birthday { get; set; }
        public string? DeathTime { get; set; }
        public DateTime RegTime { get; set; }

        private static readonly char[] Punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();
     
        public void GenerateBirthday()
        {
            var gen = new Random();
            var start = new DateTime(1995, 1, 1);
            var end = new DateTime(2005, 12, 28);
            int range = (end - start).Days;
            Birthday = start.AddDays(gen.Next(range));             
        }

        public static void Switcher(ChromeDriver browser)
        {
            browser.Navigate().GoToUrl("https://openai.com/product");
            browser.FindElement(By.XPath("//a[@aria-label='Get started']")).Click();
            Thread.Sleep(5000);
            string originalWindow = browser.CurrentWindowHandle;
            foreach (string window in browser.WindowHandles)
            {
                if (originalWindow != window)
                {
                    browser.SwitchTo().Window(window);
                    break;
                }
            }
            Console.WriteLine("Switched to OpenAi.com");
        }

        public void Registration(ChromeDriver browser, TempMail tempMail)
        {
            Switcher(browser);
            Password = PasswordGenerator(18, 9);
            var rand = new Random();
            Email = Name.First().ToLower() + rand.Next(1000, 10000) + tempMail.Domain;
            tempMail.Email = Email;

            while (true)
            {
                try
                {
                    browser.FindElement(By.XPath("//input[@id='email']")).SendKeys(Email + Keys.Enter);
                    Thread.Sleep(2000);
                    break;
                }
                catch (Exception)
                {

                }
            }
            browser.FindElement(By.XPath("//input[@id='password']")).SendKeys(Password + Keys.Enter);

            HandleMistake(browser, By.XPath("//div[@data-error-code='ip-signup-blocked']"));//Signup error
            HandleMistake(browser, By.CssSelector(".onb-auth-error"));//Email Verified error

            tempMail.ExtractVerifyUrl();
            browser.Navigate().GoToUrl($"{tempMail.Url}");
            try
            {
                if (!browser.FindElement(By.XPath("//h1")).Text.Contains("Tell us about you"))
                    throw new Exception("Antibot detecting worked");                
            }
            catch (NoSuchElementException)
            {

            }
            Console.WriteLine("Registration started...");

            EnterFIO(browser);
        }

        public void EnterFIO(ChromeDriver browser)
        {
            FirstName = Name.First();
            LastName = Name.Last();
            GenerateBirthday();

            browser.FindElement(By.XPath("//input[@placeholder='First name']")).SendKeys(FirstName);
            browser.FindElement(By.XPath("//input[@placeholder='Last name']")).SendKeys(LastName);
            browser.FindElement(By.XPath("//input[@placeholder='Birthday']")).SendKeys
                (
                Birthday.Day.ToString("00") +
                Birthday.Month.ToString("d2") +
                Birthday.Year.ToString()
                );
            browser.FindElement(By.XPath("//button[@type='submit']")).Click();

            Console.WriteLine("FIO Entered...");
        }

        public static void VerifyPhone(ChromeDriver browser, VakSms phone)
        {
            string countryName = ConfigLoader.GetConfiguration()["CountryString"];
            Console.WriteLine("Verifying started");

            phone.BuyNumber();
            Thread.Sleep(2000);
            browser.FindElement(By.XPath("//input[@id='react-select-2-input']")).SendKeys(countryName + Keys.Enter);
            browser.FindElement(By.XPath("//input[@inputmode='numeric']")).SendKeys(phone.Tel);
            HandleMistake(browser, By.XPath("//div[@class='onb-whatsapp-optin']"));//What's Up
            browser.FindElement(By.XPath("//button[@type='submit']")).Click();
            HandleMistake(browser, By.CssSelector(".onb-form-error-msg"));//We couldn't verify your phone number
            try
            {
                if (!browser.FindElement(By.XPath("//h1")).Text.Contains("Enter code"))
                    throw new CustomException("Antibot detecting worked");
            }
            catch (NoSuchElementException)
            {

            }

            phone.ReceiveSmsCode();

            if (phone.SmsCode == null)
                throw new CustomException("Sms field is null...");

            browser.FindElement(By.XPath("//input[@inputmode='numeric']")).SendKeys(phone.SmsCode);
            HandleMistake(browser, By.CssSelector(".onb-form-error-msg"));//Could not verify code
            HandleMistake(browser, By.CssSelector(".onb-credit-warning"));//A not on kredits
        }

        public void ExtractKey(ChromeDriver browser)
        {
            RegTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));

            browser.FindElement(By.XPath("//div[@class='user-details-org']")).Click();
            browser.FindElement(By.XPath("//a[@href='/account/api-keys']")).Click();
            browser.FindElement(By.XPath("//button[@class='btn btn-sm btn-filled btn-neutral']")).Click();
            browser.FindElement(By.XPath("//button[@type='submit']")).Click();

            Thread.Sleep(2000);//Time to key spawning

            Apikey = browser.FindElement(By.XPath("//input[@class='text-input text-input-sm text-input-full']")).GetAttribute("value");
            browser.FindElement(By.XPath("//button[@aria-label='Copy']")).Click();
            browser.FindElement(By.XPath("//button[@type='submit']")).Click();
            browser.FindElement(By.XPath("//a[@href='/account/usage']")).Click();

            int counter = 0;
            while (true)
            {
                try
                {
                    DeathTime = browser.FindElement(By.XPath("//td[3]")).Text;
                    break;
                }
                catch (NoSuchElementException)
                {
                    counter++;
                    Thread.Sleep(10000);
                    browser.Navigate().Refresh();
                }
                if (counter >= 5)
                {
                    DeathTime = null;
                    break;
                }

            }

            Console.WriteLine("apikey grabbed: " + Apikey);
            Thread.Sleep(2000);

        }

        public static string PasswordGenerator(int length, int numberOfNonAlphanumericCharacters)
        {
            if (length < 1 || length > 128)
            {
                throw new ArgumentException(null, nameof(length)); //изменил throw new ArgumentException(nameof(numberOfNonAlphanumericCharacters));
            }

            if (numberOfNonAlphanumericCharacters > length || numberOfNonAlphanumericCharacters < 0)
            {
                throw new ArgumentException(null, nameof(numberOfNonAlphanumericCharacters));//изменил throw new ArgumentException(nameof(numberOfNonAlphanumericCharacters));
            }

            using var rng = RandomNumberGenerator.Create();
            var byteBuffer = new byte[length];

            rng.GetBytes(byteBuffer);

            var count = 0;
            var characterBuffer = new char[length];

            for (var iter = 0; iter < length; iter++)
            {
                var i = byteBuffer[iter] % 87;

                if (i < 10)
                {
                    characterBuffer[iter] = (char)('0' + i);
                }
                else if (i < 36)
                {
                    characterBuffer[iter] = (char)('A' + i - 10);
                }
                else if (i < 62)
                {
                    characterBuffer[iter] = (char)('a' + i - 36);
                }
                else
                {
                    characterBuffer[iter] = Punctuations[i - 62];
                    count++;
                }
            }

            if (count >= numberOfNonAlphanumericCharacters)
            {
                return new string(characterBuffer);
            }

            int j;
            var rand = new Random();

            for (j = 0; j < numberOfNonAlphanumericCharacters - count; j++)
            {
                int k;
                do
                {
                    k = rand.Next(0, length);
                }
                while (!char.IsLetterOrDigit(characterBuffer[k]));

                characterBuffer[k] = Punctuations[rand.Next(0, Punctuations.Length)];
            }

            return new string(characterBuffer);
        }
        static void HandleMistake(IWebDriver browser, By by)
        {
            var wait = new WebDriverWait(browser, TimeSpan.FromSeconds(5));
            try
            {
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(by));
            }
            catch (Exception)
            {
                return; 
            }

            Dictionary<By, int> scenarios = new()
            {
                { By.XPath("//div[@data-error-code='ip-signup-blocked']"), 1 }, //signup error
                { By.CssSelector(".onb-auth-error"), 2 }, //Email Verified error
                { By.XPath("//div[@class='onb-whatsapp-optin']"),3 }, //What's up button
                { By.CssSelector(".onb-form-error-msg"), 4 },//We couldn't verify your phone number
                { By.CssSelector(".onb-credit-warning"), 5 }//A not on credits + //Could not verify code
            };

            if (scenarios.TryGetValue(by, out int state))
            {
                switch (state)
                {
                    case 1:
                        throw new CustomException("Signup Error");

                    case 2:
                        throw new CustomException("Email verified but you are no longer authenticated");

                    case 3:
                        browser.FindElement(By.XPath("//label[@for='whatsapp-opt-in-radio-no']")).Click();
                        break;

                    case 4:
                        while (true)
                        {
                            if (browser.FindElements(by).Count > 0)
                            {
                                var errorString = browser.FindElement(by).Text;

                                if (errorString.Contains("We couldn't verify your phone number"))
                                {
                                    Thread.Sleep(5000);
                                    browser.FindElement(By.XPath("//button[@type='submit']")).Click();
                                }
                                else if (errorString.Contains("Could not verify code."))
                                {
                                    IWebElement input = browser.FindElement(By.XPath("//input[@inputmode='numeric']"));
                                    string inputValue = input.GetAttribute("value");
                                    input.Clear();
                                    input.SendKeys(inputValue);
                                }
                                else
                                    throw new CustomException("Unknown error in handle of mistakes");
                            }
                            else
                                break;
                        }
                        break;
                    case 5:
                        throw new CustomException("Empty account...");

                    default:
                        Console.WriteLine("No handles for mistake. Need to fix");
                        break;
                }
            }
        }
    }
}
