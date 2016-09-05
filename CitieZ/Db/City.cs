using System.Collections.Generic;
using CitieZ.Util;

namespace CitieZ.Db
{
    internal class City
    {
        public City(string name, string regionName, Position warp, List<int> discovered)
        {
            Name = name;
            RegionName = regionName;
            Warp = warp;
            Discovered = discovered;
        }

        public string Name { get; private set; }
        public string RegionName { get; private set; }
        public Position Warp { get; private set; }
        public List<int> Discovered { get; private set; }
    }
}