namespace Worldescape.App.Core;

public class Coordinate
{
    public Coordinate()
    {

    }

    public Coordinate(double x, double y, int z = 9999)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public double X { get; set; }

    public double Y { get; set; }

    public int Z { get; set; }
}

