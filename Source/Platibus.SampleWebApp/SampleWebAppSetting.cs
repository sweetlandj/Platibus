using System.Configuration;

namespace Platibus.SampleWebApp
{
    public static class SampleWebAppSetting
    {
        public const string OwinMiddleware = "UseOwinMiddleware";

        public static bool IsSet(this string setting) => setting.IsEnabled();

        public static bool IsEnabled(this string setting)
        {
            var value = ConfigurationManager.AppSettings[setting]?.Trim().ToLower();
            switch (value)
            {
                    case "true":
                    case "t":
                    case "yes": 
                    case "y": 
                    case "on":
                    case "enabled": 
                        return true;
                    default:
                        return false;
            }
        }
    }
}