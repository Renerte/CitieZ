using System.Reflection;
using CitieZ.Util;
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
            var city = await CitieZ.Cities.GetAsync(e.Parameters[0]);
            if ((city != null) && (city.Discovered.Contains(e.Player.User.ID) || e.Player.HasPermission("citiez.all")))
            {
                e.Player.SendInfoMessage(string.Format(CitieZ.Config.TeleportingToCity, city.Name));
                e.Player.Teleport(city.Warp.X*16, city.Warp.Y*16);
            }
            else
            {
                e.Player.SendErrorMessage(string.Format(CitieZ.Config.NoSuchCity, e.Parameters[0]));
            }
        }

        public static async void Manage(CommandArgs e)
        {
            if (e.Parameters.Count == 0)
            {
                e.Player.SendErrorMessage(
                    $"CitieZ v{Assembly.GetExecutingAssembly().GetName().Version} by Renerte - the best city system for TShock!");
                return;
            }
            switch (e.Parameters[0])
            {
                case "add":
                    if (e.Parameters.Count < 3)
                    {
                        e.Player.SendErrorMessage("Use: /citiez add name region");
                        break;
                    }
                    if (
                        await
                            CitieZ.Cities.AddAsync(e.Parameters[1], e.Parameters[2],
                                new Position(e.Player.TileX, e.Player.TileY)))
                        e.Player.SendInfoMessage($"Added city {e.Parameters[1]} with region {e.Parameters[2]}.");
                    break;
                case "setwarp":
                    if (e.Parameters.Count < 2)
                    {
                        e.Player.SendErrorMessage("Use: /citiez setwarp name");
                        break;
                    }
                    if (await CitieZ.Cities.SetWarpAsync(e.Parameters[1], new Position(e.Player.TileX, e.Player.TileY)))
                        e.Player.SendInfoMessage($"Successfully set warp for city {e.Parameters[1]}");
                    break;
                case "del":
                    if (e.Parameters.Count < 2)
                        e.Player.SendErrorMessage("Use: /citiez del name");
                    if (await CitieZ.Cities.DeleteAsync(e.Parameters[1]))
                        e.Player.SendInfoMessage($"Successfully deleted city {e.Parameters[1]}");
                    else
                        e.Player.SendErrorMessage($"Could not remove city {e.Parameters[1]}");
                    break;
            }
        }
    }
}