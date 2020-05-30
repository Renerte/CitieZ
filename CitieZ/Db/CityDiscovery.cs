using TShockAPI.DB;

namespace CitieZ.Db
{
    public class CityDiscovery
    {
        public CityDiscovery(string cityName, UserAccount user)
        {
            CityName = cityName;
            User = user;
        }

        public string CityName { get; private set; }
        public UserAccount User { get; private set; }
    }
}