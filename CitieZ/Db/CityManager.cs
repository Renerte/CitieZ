using System.Collections.Generic;
using System.Data;
using System.Linq;
using CitieZ.Util;
using MySql.Data.MySqlClient;
using Terraria;
using TShockAPI.DB;

namespace CitieZ.Db
{
    public class CityManager
    {
        private readonly List<City> cities = new List<City>();
        private IDbConnection db;
        private object syncLock = new object();

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
                        result.Get<string>("Discovered").Split(',').Select(int.Parse).ToList()));
            }
        }
    }
}