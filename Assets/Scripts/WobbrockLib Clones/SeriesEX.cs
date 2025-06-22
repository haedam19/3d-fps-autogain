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
using System;
using System.Collections;
using System.Collections.Generic;

public static class SeriesEx
{
    public static List<TimePointR> ResampleInSpace(List<TimePointR> points, double px)
    {
        List<TimePointR> list = new List<TimePointR>(points);
        List<TimePointR> list2 = new List<TimePointR>();
        list2.Add(list[0]);
        double num = 0.0;
        for (int i = 1; i < list.Count; i++)
        {
            TimePointR timePointR = list[i - 1];
            TimePointR timePointR2 = list[i];
            double num2 = PointR.Distance((PointR)timePointR, (PointR)timePointR2);
            if (num + num2 >= px)
            {
                double num3 = (px - num) / num2;
                double x = timePointR.X + num3 * (timePointR2.X - timePointR.X);
                double y = timePointR.Y + num3 * (timePointR2.Y - timePointR.Y);
                double num4 = (double)timePointR.Time + num3 * (double)(timePointR2.Time - timePointR.Time);
                TimePointR item = new TimePointR(x, y, (long)num4);
                list2.Add(item);
                list.Insert(i, item);
                num = 0.0;
            }
            else
            {
                num += num2;
            }
        }

        if (num > 0.0)
        {
            list2.Add(list[list.Count - 1]);
        }

        return list2;
    }

    public static List<TimePointR> ResampleInTime(List<TimePointR> points, int hertz)
    {
        double num = 1000.0 / (double)hertz;
        double num2 = 0.0;
        List<TimePointR> list = new List<TimePointR>(points);
        int num3 = (int)Math.Ceiling((double)(points[points.Count - 1].Time - points[0].Time) / num);
        List<TimePointR> list2 = new List<TimePointR>(num3);
        list2.Add(list[0]);
        for (int i = 1; i < list.Count; i++)
        {
            TimePointR timePointR = list[i - 1];
            TimePointR timePointR2 = list[i];
            double num4 = timePointR2.Time - timePointR.Time;
            if (num2 + num4 >= num)
            {
                double num5 = (num - num2) / num4;
                double x = timePointR.X + num5 * (timePointR2.X - timePointR.X);
                double y = timePointR.Y + num5 * (timePointR2.Y - timePointR.Y);
                double num6 = (double)timePointR.Time + (num - num2);
                TimePointR item = new TimePointR(x, y, (long)num6);
                list2.Add(item);
                list.Insert(i, item);
                num2 = 0.0;
            }
            else
            {
                num2 += num4;
            }
        }

        if (list2.Count == num3 - 1)
        {
            list2.Add(list[list.Count - 1]);
        }

        return list2;
    }

    public static int Max(List<PointR> series, int start, int count)
    {
        int num = start;
        int num2 = ((count > 0) ? Math.Min(start + count, series.Count) : series.Count);
        for (int i = start; i < num2; i++)
        {
            PointR pointR = series[num];
            if (series[i].Y > pointR.Y)
            {
                num = i;
            }
        }

        return num;
    }

    public static int Min(List<PointR> series, int start, int count)
    {
        int num = start;
        int num2 = ((count > 0) ? Math.Min(start + count, series.Count) : series.Count);
        for (int i = start; i < num2; i++)
        {
            PointR pointR = series[num];
            if (series[i].Y <= pointR.Y)
            {
                num = i;
            }
        }

        return num;
    }

    public static int[] Maxima(List<PointR> series, int start, int count)
    {
        List<int> list = new List<int>(series.Count);
        int num = ((count > 0) ? Math.Min(start + count, series.Count) : series.Count);
        for (int i = start + 1; i < num - 1; i++)
        {
            PointR pointR = series[i - 1];
            PointR pointR2 = series[i];
            PointR pointR3 = series[i + 1];
            if (pointR.Y < pointR2.Y && pointR2.Y >= pointR3.Y)
            {
                list.Add(i);
            }
        }

        return list.ToArray();
    }

    public static int[] Minima(List<PointR> series, int start, int count)
    {
        List<int> list = new List<int>(series.Count);
        int num = ((count > 0) ? Math.Min(start + count, series.Count) : series.Count);
        for (int i = start + 1; i < num - 1; i++)
        {
            PointR pointR = series[i - 1];
            PointR pointR2 = series[i];
            PointR pointR3 = series[i + 1];
            if (pointR.Y >= pointR2.Y && pointR2.Y < pointR3.Y)
            {
                list.Add(i);
            }
        }

        return list.ToArray();
    }

    public static void GetRisingAndFalling(List<PointR> series, out List<List<PointR>> rising, out List<List<PointR>> falling)
    {
        int[] minima = Minima(series, 0, -1);
        int[] maxima = Maxima(series, 0, -1);
        GetRisingAndFalling(series, minima, maxima, out rising, out falling);
    }

    public static void GetRisingAndFalling(List<PointR> series, int[] minima, int[] maxima, out List<List<PointR>> rising, out List<List<PointR>> falling)
    {
        rising = new List<List<PointR>>();
        falling = new List<List<PointR>>();
        int num = -1;
        for (int i = 0; i < Math.Max(minima.Length, maxima.Length); i++)
        {
            if (0 <= i && i < maxima.Length)
            {
                int num2 = num + 1;
                num = maxima[i];
                rising.Add(series.GetRange(num2, num - num2 + 1));
            }

            if (0 <= i && i < minima.Length)
            {
                int num2 = num + 1;
                num = minima[i];
                falling.Add(series.GetRange(num2, num - num2 + 1));
            }
        }

        if (minima.Length == 0 && maxima.Length == 0)
        {
            int num2 = 0;
            num = series.Count - 1;
            if (series[num2].Y <= series[num].Y)
            {
                rising.Add(series.GetRange(num2, num - num2 + 1));
            }
            else
            {
                falling.Add(series.GetRange(num2, num - num2 + 1));
            }
        }
        else if ((minima.Length != 0 && maxima.Length == 0) || (minima.Length != 0 && maxima.Length != 0 && minima[minima.Length - 1] > maxima[maxima.Length - 1]))
        {
            int num2 = minima[minima.Length - 1] + 1;
            num = series.Count - 1;
            rising.Add(series.GetRange(num2, num - num2 + 1));
        }
        else if ((maxima.Length != 0 && minima.Length == 0) || (maxima.Length != 0 && minima.Length != 0 && maxima[maxima.Length - 1] > minima[minima.Length - 1]))
        {
            int num2 = maxima[maxima.Length - 1] + 1;
            num = series.Count - 1;
            falling.Add(series.GetRange(num2, num - num2 + 1));
        }
    }

    public static BitArray ValuesWithinTolerance(List<PointR> series, float value, float tolerance, int start, int count)
    {
        BitArray bitArray = new BitArray(series.Count);
        int num = ((count > 0) ? Math.Min(start + count, series.Count) : series.Count);
        for (int i = start; i < num; i++)
        {
            bitArray[i] = Math.Abs(series[i].Y - (double)value) <= (double)tolerance;
        }

        return bitArray;
    }

    public static List<int> Bits2Indices(BitArray bits, int start, int count)
    {
        List<int> list = new List<int>();
        int num = ((count > 0) ? Math.Min(start + count, bits.Count) : bits.Count);
        int num2 = -1;
        for (int i = start; i < num; i++)
        {
            if (bits[i])
            {
                if (num2 == -1)
                {
                    num2 = i;
                }
            }
            else if (num2 != -1)
            {
                list.Add(num2);
                list.Add(i - 1);
                num2 = -1;
            }
        }

        if (num2 != -1)
        {
            list.Add(num2);
            list.Add(bits.Count - 1);
        }

        return list;
    }

    public static BitArray Indices2Bits(List<int> indices, int count)
    {
        BitArray bitArray = new BitArray(count);
        for (int i = 1; i < indices.Count; i += 2)
        {
            for (int j = indices[i - 1]; j <= indices[i]; j++)
            {
                bitArray[j] = true;
            }
        }

        return bitArray;
    }

    public static List<PointR> Derivative(List<TimePointR> twoDSeries)
    {
        // x is time in milliseconds, y is speed in pixels per millisecond
        List<PointR> list = new List<PointR>(twoDSeries.Count);
        for (int i = 1; i < twoDSeries.Count; i++)
        {
            double x = twoDSeries[i].Time - twoDSeries[0].Time;
            double num = twoDSeries[i].Time - twoDSeries[i - 1].Time;
            double y = PointR.Distance((PointR)twoDSeries[i - 1], (PointR)twoDSeries[i]) / num;
            list.Add(new PointR(x, y));
        }

        list.Insert(0, PointR.Empty);
        return list;
    }

    public static List<PointR> Derivative(List<PointR> oneDSeries)
    {
        List<PointR> list = new List<PointR>(oneDSeries.Count);
        for (int i = 1; i < oneDSeries.Count; i++)
        {
            double x = oneDSeries[i].X - oneDSeries[0].X;
            double num = oneDSeries[i].X - oneDSeries[i - 1].X;
            double y = (oneDSeries[i].Y - oneDSeries[i - 1].Y) / num;
            list.Add(new PointR(x, y));
        }

        list.Insert(0, PointR.Empty);
        return list;
    }

    public static List<PointR> Filter(List<PointR> series, double[] filter)
    {
        double[] array = new double[series.Count];
        for (int i = 0; i < series.Count; i++)
        {
            int num = 0;
            for (int j = i - filter.Length / 2; j <= i + filter.Length / 2; j++)
            {
                if (0 <= j && j < series.Count)
                {
                    array[i] += series[j].Y * filter[num];
                }

                num++;
            }
        }

        List<PointR> list = new List<PointR>(array.Length);
        for (int k = 0; k < array.Length; k++)
        {
            list.Add(new PointR(series[k].X, array[k]));
        }

        return list;
    }

    public static double[] GaussianKernel(int stdev)
    {
        if (stdev < 1)
        {
            stdev = 1;
        }

        int num = 3 * stdev * 2 + 1;
        double[] array = new double[num];
        int num2 = 0;
        int num3 = -num / 2;
        while (num2 < num)
        {
            array[num2] = 1.0 / (Math.Sqrt(Math.PI * 2.0) * (double)stdev) * Math.Pow(Math.E, (double)(-(num3 * num3)) / (2.0 * (double)stdev * (double)stdev));
            num2++;
            num3++;
        }

        return array;
    }
}