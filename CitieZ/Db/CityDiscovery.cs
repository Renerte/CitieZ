using TShockAPI;

namespace CitieZ.Db
{
    public class CityDiscovery
    {
        public CityDiscovery(string cityName, TSPlayer player)
        {
            CityName = cityName;
            Player = player;
        }

        public string CityName { get; private set; }
        public TSPlayer Player { get; private set; }
    }
}