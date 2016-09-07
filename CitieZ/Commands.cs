using TShockAPI;

namespace CitieZ
{
    public static class Commands
    {
        public static async void City(CommandArgs e)
        {
            if (e.Parameters.Count != 1)
            {
                e.Player.SendErrorMessage("Use: /city name");
                return;
            }
            var city = await CitieZ.Cities.GetAsync(e.Player, e.Parameters[0]);
            if ((city != null) && (city.Discovered.Contains(e.Player.User.ID) || e.Player.HasPermission("citiez.all")))
            {
                e.Player.SendInfoMessage(string.Format(CitieZ.Config.TeleportingToCity, city.Name));
                e.Player.Teleport(city.Warp.X, city.Warp.Y);
            }
            else
            {
                e.Player.SendErrorMessage(string.Format(CitieZ.Config.NoSuchCity, e.Parameters[0]));
            }
        }
    }
}