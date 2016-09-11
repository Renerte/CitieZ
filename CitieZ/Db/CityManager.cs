﻿using System;
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

            TShock.Log.ConsoleInfo($"[CitieZ] Loaded {cities.Count} cities.");
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
    }
}