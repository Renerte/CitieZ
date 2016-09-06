using System.IO;
using Newtonsoft.Json;

namespace CitieZ
{
    public class Config
    {
        public string MySqlDbName = "";
        public string MySqlHost = "";
        public string MySqlPassword = "";
        public string MySqlUsername = "";
        public string NoSuchCity = "Could not find city '{0}' - maybe it doesn't exist or you did not discover it yet.";
        public string TeleportingToCity = "You are teleported to city {0}.";

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