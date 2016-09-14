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

        public Position(int[] coords)
        {
            if (coords.Length != 2)
                throw new WrongArraySizeException("Expected 2 coords: x and y");
            X = coords[0];
            Y = coords[1];
        }

        public override string ToString() => $"{X},{Y}";
    }
}