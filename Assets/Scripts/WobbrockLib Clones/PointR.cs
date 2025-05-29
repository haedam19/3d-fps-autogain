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
/// <summary>
/// <para> 2차원 x, y좌표를 double로 저장합니다. </para>
/// <para> 좌표 표현에 쓰일 때는 lefttop을 원점으로 취급합니다. </para>
/// </summary>
using System;
using UnityEngine;
using System.Collections.Generic;

public struct PointR
{
    public static readonly PointR Empty;

    private double _x;

    private double _y;

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

    public PointR(double x, double y)
    {
        _x = x;
        _y = y;
    }

    public PointR(PointR pt)
    {
        _x = pt._x;
        _y = pt._y;
    }

    public override string ToString()
    {
        return $"{{X={_x}, Y={_y}}}";
    }

    public static PointR FromString(string s)
    {
        PointR result = Empty;
        try
        {
            int num = s.IndexOf('X');
            int num2 = s.IndexOf(',', num + 2);
            double x = double.Parse(s.Substring(num + 2, num2 - (num + 2)));
            int num3 = s.IndexOf('Y');
            num2 = s.IndexOf('}', num3 + 2);
            double y = double.Parse(s.Substring(num3 + 2, num2 - (num3 + 2)));
            result = new PointR(x, y);
        }
        catch
        {
        }

        return result;
    }

    public static double Distance(PointR p1, PointR p2)
    {
        double num = p2.X - p1.X;
        double num2 = p2.Y - p1.Y;
        return Math.Sqrt(num * num + num2 * num2);
    }

    /// <summary>
    /// start -> end 사이의 각도를 구합니다.
    /// </summary>
    /// <returns> angle in radians (double) </returns>
    public static double Angle(PointR start, PointR end, bool positiveOnly)
    {
        double num = 0.0;
        if (start.X != end.X)
        {
            num = Math.Atan2(end.Y - start.Y, end.X - start.X);
        }
        else if (end.Y < start.Y)
        {
            num = -Math.PI / 2.0;
        }
        else if (end.Y > start.Y)
        {
            num = Math.PI / 2.0;
        }

        if (positiveOnly && num < 0.0)
        {
            num += Math.PI * 2.0;
        }

        return num;
    }

    /// <summary>
    /// p를 c를 중심으로 radians 만큼 회전시킨 점(PointR)을 반환합니다.
    /// </summary>
    public static PointR RotatePoint(PointR p, PointR c, double radians)
    {
        PointR empty = Empty;
        empty.X = (p.X - c.X) * Math.Cos(radians) - (p.Y - c.Y) * Math.Sin(radians) + c.X;
        empty.Y = (p.X - c.X) * Math.Sin(radians) + (p.Y - c.Y) * Math.Cos(radians) + c.Y;
        return empty;
    }

    /// <summary> 점들의 무게중심을 구합니다. </summary>
    public static PointR Centroid(List<PointR> points)
    {
        double num = 0.0;
        double num2 = 0.0;
        foreach (PointR point in points)
        {
            num += point.X;
            num2 += point.Y;
        }

        return new PointR(num / (double)points.Count, num2 / (double)points.Count);
    }

    /// <summary>
    /// 점들의 무게중심을 중심으로 하여 radians 만큼 회전시킨 점(PointR) 리스트를 반환합니다.
    /// </summary>
    public static List<PointR> RotatePoints(List<PointR> points, double radians)
    {
        List<PointR> list = new List<PointR>(points.Count);
        PointR pointR = Centroid(points);
        double x = pointR.X;
        double y = pointR.Y;
        double num = Math.Cos(radians);
        double num2 = Math.Sin(radians);
        for (int i = 0; i < points.Count; i++)
        {
            double num3 = points[i].X - x;
            double num4 = points[i].Y - y;
            PointR empty = PointR.Empty;
            empty.X = num3 * num - num4 * num2 + x;
            empty.Y = num3 * num2 + num4 * num + y;
            list.Add(empty);
        }

        return list;
    }

    /// <summary>
    /// PointR를 Vector2로 변환합니다. y좌표는 유니티 좌표계에 맞게 변환됩니다.
    /// </summary>
    public static explicit operator PointR(Vector2 v)
    {
        return new PointR(v.x, Screen.height - v.y);
    }

    /// <summary>
    /// Vector2를 PointR로 변환합니다. y좌표는 화면 좌표계에 맞게 변환됩니다.
    /// </summary>
    public static explicit operator Vector2(PointR p)
    {
        return new Vector2((float)p.X, Screen.height - (float)p.Y);
    }

    public static bool operator ==(PointR p1, PointR p2)
    {
        if (p1._x == p2._x)
        {
            return p1._y == p2._y;
        }

        return false;
    }

    public static bool operator !=(PointR p1, PointR p2)
    {
        if (p1._x == p2._x)
        {
            return p1._y != p2._y;
        }

        return true;
    }

    public override bool Equals(object obj)
    {
        if (obj is PointR pointR)
        {
            if (X == pointR.X)
            {
                return Y == pointR.Y;
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