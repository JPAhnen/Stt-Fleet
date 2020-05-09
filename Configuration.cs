using Microsoft.Extensions.Configuration;

namespace STTFleet
{
    public class Configuration
    {
        IConfigurationRoot _config;

        public Configuration(string basePath)
        {
            _config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            STTApiConfiguration = new STTApiConfiguration(_config);
            DiscordConfiguration = new DiscordConfiguration(_config);
        }

        public STTApiConfiguration STTApiConfiguration { get; }
        public DiscordConfiguration DiscordConfiguration { get; }
    }

    public class DiscordConfiguration
    {
        private IConfigurationRoot _config;

        public DiscordConfiguration(IConfigurationRoot configuration)
        {
            _config = configuration;
        }

        public string DiscordDailiesWebhookUrl => _config["discord-webhookurl-dailies"];
        public string DiscordEventRanksWebhookUrl => _config["discord-webhookurl-eventranks"];
    }

    public class STTApiConfiguration
    {
        IConfigurationRoot _config;

        public STTApiConfiguration(IConfigurationRoot configuration)
        {
            _config = configuration;
        }

        public string Username => _config["stt-username"];

        public string Password => _config["stt-password"];

        public string FleetId => _config["stt-fleetid"];

        public string ClientId => _config["stt-clientid"];

        public string UrlPlatform => _config["stt-url-platform"];

        public string UrlServer => _config["stt-url-server"];

        public string ClientApiVersion => _config["stt-clientapi-version"];
    }
}