using Microsoft.Extensions.Configuration;

namespace Home_Work.Scripts
{
    internal class ConfigLoader
    {
        private static readonly IConfiguration _config;

        static ConfigLoader()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile(Path.GetFullPath("../../../appsettings.json"))
                .Build();
        }

        public static IConfiguration GetConfiguration()
        {
            return _config;
        }
    }
}
