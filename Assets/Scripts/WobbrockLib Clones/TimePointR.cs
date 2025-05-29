/* 
 * Cloned from WobbrockLib.dll
 * C# .NET 2.0 library of application-agnostic formulae, widgets, data structures, and algorithms
 * 
 *      Jacob O. Wobbrock, Ph.D.
 * 		The Information School
 *		University of Washington
 *		Mary Gates Hall, Box 352840
 *		Seattle, WA 98195-2840
 *		wobbrock@uw.edu
 */
using System.Collections.Generic;
using System.Drawing;
using TimeL = System.Int64;

public struct TimePointR
{
    public static readonly TimePointR Empty;

    private double _x;

    private double _y;

    private TimeL _t;

    public double X
    {
        get
        {
            return _x;
        }
        set
        {
            _x = value;
        }
    }

    public double Y
    {
        get
        {
            return _y;
        }
        set
        {
            _y = value;
        }
    }

    public TimeL Time
    {
        get
        {
            return _t;
        }
        set
        {
            _t = value;
        }
    }

    public TimePointR(double x, double y, long ms)
    {
        _x = x;
        _y = y;
        _t = ms;
    }

    public TimePointR(PointR pt, long ms)
    {
        _x = pt.X;
        _y = pt.Y;
        _t = ms;
    }

    public TimePointR(TimePointR pt)
    {
        _x = pt.X;
        _y = pt.Y;
        _t = pt.Time;
    }

    public override string ToString()
    {
        return $"{{X={_x}, Y={_y}, Time={_t}}}";
    }

    public static TimePointR FromString(string s)
    {
        TimePointR result = Empty;
        try
        {
            int num = s.IndexOf('X');
            int num2 = s.IndexOf(',', num + 2);
            double x = double.Parse(s.Substring(num + 2, num2 - (num + 2)));
            int num3 = s.IndexOf('Y');
            num2 = s.IndexOf(',', num3 + 2);
            double y = double.Parse(s.Substring(num3 + 2, num2 - (num3 + 2)));
            int num4 = s.IndexOf('T');
            num2 = s.IndexOf('}', num4 + 5);
            long ms = long.Parse(s.Substring(num4 + 5, num2 - (num4 + 5)));
            result = new TimePointR(x, y, ms);
        }
        catch
        {
        }

        return result;
    }

    public static explicit operator PointR(TimePointR pt)
    {
        return new PointR(pt.X, pt.Y);
    }

    public static implicit operator TimePointR(PointR pt)
    {
        return new TimePointR(pt.X, pt.Y, 0L);
    }

    public static explicit operator PointF(TimePointR pt)
    {
        return new PointF((float)pt.X, (float)pt.Y);
    }

    public static implicit operator TimePointR(PointF pt)
    {
        return new TimePointR(pt.X, pt.Y, 0L);
    }

    public static List<PointR> ConvertList(List<TimePointR> pts)
    {
        List<PointR> list = new List<PointR>(pts.Count);
        foreach (TimePointR pt in pts)
        {
            list.Add((PointR)pt);
        }

        return list;
    }

    public static List<TimePointR> ConvertList(List<PointR> pts)
    {
        List<TimePointR> list = new List<TimePointR>(pts.Count);
        foreach (PointR pt in pts)
        {
            list.Add(pt);
        }

        return list;
    }

    public static bool operator ==(TimePointR tp1, TimePointR tp2)
    {
        if (tp1.X == tp2.X && tp1.Y == tp2.Y)
        {
            return tp1.Time == tp2.Time;
        }

        return false;
    }

    public static bool operator !=(TimePointR tp1, TimePointR tp2)
    {
        if (tp1.X == tp2.X && tp1.Y == tp2.Y)
        {
            return tp1.Time != tp2.Time;
        }

        return true;
    }

    public bool EqualsIgnoreTime(TimePointR tp)
    {
        if (X == tp.X)
        {
            return Y == tp.Y;
        }

        return false;
    }

    public override bool Equals(object obj)
    {
        if (obj is TimePointR timePointR)
        {
            if (X == timePointR.X && Y == timePointR.Y)
            {
                return Time == timePointR.Time;
            }

            return false;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }
}