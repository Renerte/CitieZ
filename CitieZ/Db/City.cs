using System.Collections.Generic;

namespace CitieZ.Db
{
    internal class City
    {
        public City(string name, string regionName, string warpName, List<int> discovered)
        {
            Name = name;
            RegionName = regionName;
            WarpName = warpName;
            Discovered = discovered;
        }

        public string Name { get; private set; }
        public string RegionName { get; private set; }
        public string WarpName { get; private set; }
        public List<int> Discovered { get; private set; }
    }
}