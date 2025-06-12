using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViennaDotNet.TileRenderer.Wkb;

public struct Point
{
    public double X;
    public double Y;

    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }

    public Point(BinaryReader reader)
    {
        Load(reader);
    }

    public void Load(BinaryReader reader)
    {
        X = reader.ReadDouble();
        Y = reader.ReadDouble();
    }

    public static Point operator +(Point left, Point right)
        => new Point(left.X + right.X, left.Y + right.Y);

    public static Point operator -(Point left, Point right)
        => new Point(left.X - right.X, left.Y - right.Y);

    public static Point operator *(Point left, Point right)
        => new Point(left.X * right.X, left.Y * right.Y);

    public static Point operator /(Point left, Point right)
        => new Point(left.X / right.X, left.Y / right.Y);

    public static Point operator +(Point left, double right)
        => new Point(left.X + right, left.Y + right);

    public static Point operator -(Point left, double right)
        => new Point(left.X - right, left.Y - right);

    public static Point operator *(Point left, double right)
        => new Point(left.X * right, left.Y * right);

    public static Point operator /(Point left, double right)
        => new Point(left.X / right, left.Y / right);
}
