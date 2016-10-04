using System.IO;
using Newtonsoft.Json;

namespace CitieZ
{
    public class Config
    {
        public string DiscoveredCity = "You discovered city {0}! You can now use '/city {0}' to teleport to it.";
        public string FirstDiscoveredCity = "You are the first person to discover city {0}! Congrats!";
        public string MySqlDbName = "";
        public string MySqlHost = "";
        public string MySqlPassword = "";
        public string MySqlUsername = "";
        public string NoSuchCity = "Could not find city '{0}' - maybe it doesn't exist or you did not discover it yet.";
        public string TeleportingToCity = "You are teleported to city {0}.";
        public string WelcomeMessage = "Welcome to {0} - discovered by {1}";

        public void Write(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static Config Read(string path)
        {
            return File.Exists(path) ? JsonConvert.DeserializeObject<Config>(File.ReadAllText(path)) : new Config();
        }
    }
}