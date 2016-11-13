namespace CitieZ.Db
{
    public class CityDiscovery
    {
        public CityDiscovery(string cityName, string playerName)
        {
            CityName = cityName;
            PlayerName = playerName;
        }

        public string CityName { get; private set; }
        public string PlayerName { get; private set; }
    }
}