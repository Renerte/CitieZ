using System.Collections.Generic;

namespace CitieZ.Util
{
    public class Position
    {
        public int X;
        public int Y;

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Position(IReadOnlyList<int> coords)
        {
            if (coords.Count != 2)
                throw new WrongArraySizeException("Expected 2 coords: x and y");
            X = coords[0];
            Y = coords[1];
        }

        public override string ToString() => $"{X},{Y}";
    }
}