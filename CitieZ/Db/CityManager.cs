using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CitieZ.Util;
using MySql.Data.MySqlClient;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace CitieZ.Db
{
    public class CityManager
    {
        private readonly List<City> cities = new List<City>();
        private readonly IDbConnection db;
        private readonly List<CityDiscovery> discoveries = new List<CityDiscovery>();
        private readonly object syncLock = new object();

        public CityManager(IDbConnection db)
        {
            this.db = db;

            var sqlCreator = new SqlTableCreator(db,
                db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder) new SqliteQueryCreator() : new MysqlQueryCreator());
            sqlCreator.EnsureTableStructure(new SqlTable("Cities",
                new SqlColumn("ID", MySqlDbType.Int32) {AutoIncrement = true, Primary = true},
                new SqlColumn("Name", MySqlDbType.VarChar, 32) {Unique = true, Length = 32},
                new SqlColumn("Region", MySqlDbType.Text),
                new SqlColumn("Warp", MySqlDbType.Text),
                new SqlColumn("Discovered", MySqlDbType.Text),
                new SqlColumn("WorldID", MySqlDbType.Int32)));

            sqlCreator.EnsureTableStructure(new SqlTable("CityDiscoveries",
                new SqlColumn("ID", MySqlDbType.Int32) {AutoIncrement = true, Primary = true},
                new SqlColumn("City", MySqlDbType.VarChar, 32) {Unique = true, Length = 32},
                new SqlColumn("Player", MySqlDbType.VarChar, 32) {Length = 32},
                new SqlColumn("WorldID", MySqlDbType.Int32)));

            using (
                var result = db.QueryReader("SELECT * FROM Cities WHERE WorldID = @0", Main.worldID))
            {
                while (result.Read())
                    cities.Add(new City(
                        result.Get<string>("Name"),
                        result.Get<string>("Region"),
                        new Position(result.Get<string>("Warp").Split(',').Select(int.Parse).ToArray()),
                        string.IsNullOrWhiteSpace(result.Get<string>("Discovered"))
                            ? new List<int>()
                            : result.Get<string>("Discovered").Split(',').Select(int.Parse).ToList()));
            }

            TShock.Log.ConsoleInfo($"[CitieZ] Loaded {cities.Count} cities.");

            using (var result = db.QueryReader("SELECT * FROM CityDiscoveries WHERE WorldID = @0", Main.worldID))
            {
                while (result.Read())
                    discoveries.Add(new CityDiscovery(
                        result.Get<string>("City"),
                        TShock.Users.GetUserByName(result.Get<string>("Player")).Name));
            }

            TShock.Log.ConsoleInfo($"[CitieZ] {discoveries.Count} cities have been discovered!");
        }

        public async Task<bool> ReloadAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    lock (syncLock)
                    {
                        cities.Clear();
                        using (var result = db.QueryReader("SELECT * FROM Cities WHERE WorldID = @0", Main.worldID))
                        {
                            while (result.Read())
                                cities.Add(new City(
                                    result.Get<string>("Name"),
                                    result.Get<string>("Region"),
                                    new Position(result.Get<string>("Warp").Split(',').Select(int.Parse).ToArray()),
                                    string.IsNullOrWhiteSpace(result.Get<string>("Discovered"))
                                        ? new List<int>()
                                        : result.Get<string>("Discovered").Split(',').Select(int.Parse).ToList()));
                        }

                        discoveries.Clear();
                        using (
                            var result = db.QueryReader("SELECT * FROM CityDiscoveries WHERE WorldID = @0", Main.worldID)
                        )
                        {
                            while (result.Read())
                                discoveries.Add(new CityDiscovery(
                                    result.Get<string>("City"),
                                    TShock.Users.GetUserByName(result.Get<string>("Player")).Name));
                        }
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error(ex.ToString());
                    return false;
                }
            });
        }

        public async Task<bool> AddAsync(string name, string regionName, Position warpPosition)
        {
            return await Task.Run(() =>
            {
                try
                {
                    lock (syncLock)
                    {
                        cities.Add(new City(name, regionName, warpPosition, new List<int>()));
                        return
                            db.Query(
                                "INSERT INTO Cities (Name, Region, Warp, WorldID) VALUES (@0, @1, @2, @3)",
                                name,
                                regionName,
                                warpPosition,
                                Main.worldID) > 0;
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error(ex.ToString());
                    return false;
                }
            });
        }

        public async Task<City> GetAsync(string name)
        {
            return await Task.Run(() =>
            {
                lock (syncLock)
                {
                    return
                        cities.Find(
                            c =>
                                    c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                }
            });
        }

        public async Task<bool> SetWarpAsync(string name, Position warpPosition)
        {
            var query = db.GetSqlType() == SqlType.Mysql
                ? "UPDATE Cities SET Warp = @0 WHERE Name = @1"
                : "UPDATE Cities SET Warp = @0 WHERE Name = @1 COLLATE NOCASE";

            return await Task.Run(() =>
            {
                var city = GetAsync(name).Result;
                try
                {
                    lock (syncLock)
                    {
                        city.Warp = warpPosition;
                        return db.Query(query,
                                   warpPosition,
                                   name) > 0;
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error(ex.ToString());
                    return false;
                }
            });
        }

        public async Task<bool> SetRegionAsync(string name, string regionName)
        {
            var query = db.GetSqlType() == SqlType.Mysql
                ? "UPDATE Cities SET Region = @0 WHERE Name = @1"
                : "UPDATE Cities SET Region = @0 WHERE Name = @1 COLLATE NOCASE";

            return await Task.Run(() =>
            {
                var city = GetAsync(name).Result;
                try
                {
                    lock (syncLock)
                    {
                        city.RegionName = regionName;
                        return db.Query(query,
                                   regionName,
                                   name) > 0;
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error(ex.ToString());
                    return false;
                }
            });
        }

        public async Task<bool> DeleteAsync(string name)
        {
            var query = db.GetSqlType() == SqlType.Mysql
                ? "DELETE FROM Cities WHERE Name = @0"
                : "DELETE FROM Cities WHERE Name = @0 COLLATE NOCASE";

            return await Task.Run(() =>
            {
                try
                {
                    lock (syncLock)
                    {
                        cities.RemoveAll(k => k.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                        return db.Query(query, name) > 0;
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error(ex.ToString());
                    return false;
                }
            });
        }

        public async Task<City> FindByRegionAsync(string regionName)
        {
            return await Task.Run(() =>
            {
                lock (syncLock)
                {
                    return cities.Find(c => c.RegionName.Equals(regionName, StringComparison.InvariantCultureIgnoreCase));
                }
            });
        }

        public async Task<bool> DiscoverAsync(string name, TSPlayer player)
        {
            var query = db.GetSqlType() == SqlType.Mysql
                ? "UPDATE Cities SET Discovered = @0 WHERE Name = @1"
                : "UPDATE Cities SET Discovered = @0 WHERE Name = @1 COLLATE NOCASE";

            return await Task.Run(() =>
            {
                try
                {
                    lock (syncLock)
                    {
                        var city = cities.Find(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                        city.Discovered.Add(player.User.ID);
                        return db.Query(query,
                                   string.Join(",", city.Discovered),
                                   name) > 0;
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error(ex.ToString());
                    return false;
                }
            });
        }

        public async Task<CityDiscovery> GetDiscoveryAsync(string name)
        {
            return await Task.Run(() =>
            {
                lock (syncLock)
                {
                    return discoveries.Find(d => d.CityName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                }
            });
        }

        public async Task<bool> AddDiscoveryAsync(string name, TSPlayer player)
        {
            return await Task.Run(() =>
            {
                try
                {
                    lock (syncLock)
                    {
                        discoveries.Add(new CityDiscovery(name, player.User.Name));
                        return
                            db.Query("INSERT INTO CityDiscoveries (City, Player, WorldID) VALUES (@0, @1, @2)",
                                name,
                                player.Name,
                                Main.worldID) > 0;
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error(ex.ToString());
                    return false;
                }
            });
        }
    }
}