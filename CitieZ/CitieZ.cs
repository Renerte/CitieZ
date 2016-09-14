using System;
using System.Data;
using System.IO;
using System.Reflection;
using CitieZ.Db;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace CitieZ
{
    [ApiVersion(1, 24)]
    public class CitieZ : TerrariaPlugin
    {
        public CitieZ(Main game) : base(game)
        {
        }

        public static Config Config { get; private set; }
        public static IDbConnection Db { get; private set; }
        public static CityManager Cities { get; private set; }

        public override string Author => "Renerte";
        public override string Name => "CitieZ";
        public override string Description => "Configurable cities system for your TShock server!";
        public override Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeneralHooks.ReloadEvent -= OnReload;
                PlayerHooks.PlayerCommand -= OnPlayerCommand;
                RegionHooks.RegionEntered -= OnRegionEntered;

                ServerApi.Hooks.GameInitialize.Deregister(this, OnGameInitialize);
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
            }
            base.Dispose(disposing);
        }

        public override void Initialize()
        {
            GeneralHooks.ReloadEvent += OnReload;
            PlayerHooks.PlayerCommand += OnPlayerCommand;
            RegionHooks.RegionEntered += OnRegionEntered;

            ServerApi.Hooks.GameInitialize.Register(this, OnGameInitialize);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
        }

        private void OnPlayerCommand(PlayerCommandEventArgs e)
        {
            if (e.Handled || (e.Player == null))
                return;
        }

        private async void OnRegionEntered(RegionHooks.RegionEnteredEventArgs e)
        {
            var city = await Cities.FindByRegionAsync(e.Region.Name);
            if ((city != null) && !city.Discovered.Contains(e.Player.User.ID) &&
                await Cities.DiscoverAsync(e.Region.Name, e.Player))
                e.Player.SendInfoMessage(string.Format(Config.DiscoveredCity, city.Name));
        }

        private async void OnReload(ReloadEventArgs e)
        {
            var path = Path.Combine(TShock.SavePath, "citiez.json");
            Config = Config.Read(path);
            if (!File.Exists(path))
                Config.Write(path);
            await Cities.ReloadAsync();
            e.Player.SendSuccessMessage("[CitieZ] Reloaded config and homes!");
        }

        private void OnGameInitialize(EventArgs e)
        {
            #region Config

            var path = Path.Combine(TShock.SavePath, "citiez.json");
            Config = Config.Read(path);
            if (!File.Exists(path))
                Config.Write(path);

            #endregion

            #region Database

            if (TShock.Config.StorageType.Equals("mysql", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(Config.MySqlHost) ||
                    string.IsNullOrWhiteSpace(Config.MySqlDbName))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(
                        "[CitieZ] MySQL is enabled, but the Essentials+ MySQL Configuration has not been set.");
                    Console.WriteLine(
                        "[CitieZ] Please configure your MySQL server information in citiez.json, then restart the server.");
                    Console.WriteLine("[CitieZ] This plugin will now disable itself...");
                    Console.ResetColor();

                    Dispose(true);

                    return;
                }

                var host = Config.MySqlHost.Split(':');
                Db = new MySqlConnection
                {
                    ConnectionString =
                        $"Server={host[0]}; Port={(host.Length == 1 ? "3306" : host[1])}; Database={Config.MySqlDbName}; Uid={Config.MySqlUsername}; Pwd={Config.MySqlPassword};"
                };
            }
            else if (TShock.Config.StorageType.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
            {
                Db = new SqliteConnection(
                    "uri=file://" + Path.Combine(TShock.SavePath, "citiez.sqlite") + ",Version=3");
            }
            else
            {
                throw new InvalidOperationException("Invalid storage type!");
            }

            #endregion

            #region Commands

            //Allows overriding of already created commands.
            Action<Command> Add = c =>
            {
                //Finds any commands with names and aliases that match the new command and removes them.
                TShockAPI.Commands.ChatCommands.RemoveAll(c2 => c2.Names.Exists(s2 => c.Names.Contains(s2)));
                //Then adds the new command.
                TShockAPI.Commands.ChatCommands.Add(c);
            };

            Add(new Command(Commands.City, "city")
            {
                HelpText = "Teleports to city, if player has discovered it"
            });

            Add(new Command(Commands.Manage, "citiez")
            {
                HelpText = "Manages cities"
            });

            #endregion
        }

        private void OnGamePostInitialize(EventArgs e)
        {
            Cities = new CityManager(Db);
        }
    }
}