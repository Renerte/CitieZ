using System.Collections.Generic;
using CitieZ.Util;

namespace CitieZ.Db
{
    public class City
    {
        public string RegionName;
        public Position Warp;

        public City(string name, string regionName, Position warp, List<int> discovered)
        {
            Name = name;
            RegionName = regionName;
            Warp = warp;
            Discovered = discovered;
        }

        public string Name { get; }
        public List<int> Discovered { get; }
    }
}