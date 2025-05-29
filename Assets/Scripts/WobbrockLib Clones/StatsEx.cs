using System;
using System.Collections.Generic;

public static class StatsEx
{
    private const double EXACT_TEST_BIAS = 1.0339757656912846E-25;

    private const double ONE_PLUS_EPSILON = 1.0339757656912949E-25;

    private const double ONE_MINUS_EPSILON = 1.0339757656912743E-25;

    public static double Mean(double[] d)
    {
        double num = 0.0;
        for (int i = 0; i < d.Length; i++)
        {
            num += d[i];
        }

        return num / (double)d.Length;
    }

    public static double StdDev(double[] d)
    {
        if (d.Length == 0)
        {
            return double.NaN;
        }

        double num = 0.0;
        double num2 = Mean(d);
        for (int i = 0; i < d.Length; i++)
        {
            double num3 = d[i] - num2;
            num += num3 * num3;
        }

        return Math.Sqrt(num / (double)(d.Length - 1));
    }

    public static double StdDev2D(List<PointR> points)
    {
        if (points.Count == 0)
        {
            return double.NaN;
        }

        double num = 0.0;
        PointR p = PointR.Centroid(points);
        for (int i = 0; i < points.Count; i++)
        {
            double num2 = PointR.Distance(p, points[i]);
            num += num2 * num2;
        }

        return Math.Sqrt(num / (double)(points.Count - 1));
    }

    /// <summary>
    /// 두 데이터 집합 x와 y의 선형 회귀 모델의 기울기 계산. (https://en.wikipedia.org/wiki/Simple_linear_regression)
    /// </summary>
    public static double Slope(double[] x, double[] y)
    {
        double num = 0.0;
        double num2 = 0.0;
        double num3 = 0.0;
        double num4 = 0.0;
        for (int i = 0; i < x.Length; i++)
        {
            num += x[i] * x[i];
            num2 += x[i];
            num3 += x[i] * y[i];
            num4 += y[i];
        }

        double num5 = (double)x.Length * num - num2 * num2;
        num3 = (double)x.Length * num3 - num2 * num4;
        return num3 / num5;
    }

    /// <summary>
    /// 두 데이터 집합 x와 y의 선형 회귀 모델의 절편 계산. (https://en.wikipedia.org/wiki/Simple_linear_regression)
    /// </summary> 
    public static double Intercept(double[] x, double[] y)
    {
        return Mean(y) - Slope(x, y) * Mean(x);
    }

    public static double CDF(double mean, double stdev, double z)
    {
        double[] array = new double[5] { 2.2352520354606837, 161.02823106855587, 1067.6894854603709, 18154.981253343562, 0.065682337918207448 };
        double[] array2 = new double[4] { 47.202581904688245, 976.09855173777669, 10260.932208618979, 45507.789335026733 };
        double[] array3 = new double[9] { 0.39894151208813466, 8.8831497943883768, 93.506656132177852, 597.27027639480025, 2494.5375852903726, 6848.1904505362827, 11602.65143764735, 9842.7148383839776, 1.0765576773720192E-08 };
        double[] array4 = new double[8] { 22.266688044328117, 235.387901782625, 1519.3775994075547, 6485.5582982667611, 18615.571640885097, 34900.952721145979, 38912.00328609327, 19685.429676859992 };
        double[] array5 = new double[6] { 0.215898534057957, 0.12740116116024736, 0.022235277870649807, 0.0014216191932278934, 2.9112874951168793E-05, 0.023073441764940174 };
        double[] array6 = new double[5] { 1.2842600961449111, 0.46823821248086511, 0.065988137868928556, 0.0037823963320275824, 7.2975155508396618E-05 };
        double num = (z - mean) / stdev;
        double num2 = double.Epsilon;
        double num3 = num;
        double num4 = Math.Abs(num3);
        double num8;
        double num10;
        if (num4 <= 0.66291)
        {
            double num5 = 0.0;
            if (num4 > 1E-12)
            {
                num5 = num3 * num3;
            }

            double num6 = array[4] * num5;
            double num7 = num5;
            for (int i = 0; i < 3; i++)
            {
                num6 = (num6 + array[i]) * num5;
                num7 = (num7 + array2[i]) * num5;
            }

            num8 = num3 * (num6 + array[3]) / (num7 + array2[3]);
            double num9 = num8;
            num8 = 0.5 + num9;
            num10 = 0.5 - num9;
        }
        else if (num4 <= 5.656854248)
        {
            double num6 = array3[8] * num4;
            double num7 = num4;
            for (int i = 0; i < 7; i++)
            {
                num6 = (num6 + array3[i]) * num4;
                num7 = (num7 + array4[i]) * num4;
            }

            num8 = (num6 + array3[7]) / (num7 + array4[7]);
            double num5 = Math.Floor(num4 * 1.6) / 1.6;
            double num11 = (num4 - num5) * (num4 + num5);
            num8 = Math.Exp(0.0 - num5 * num5 * 0.5) * Math.Exp(0.0 - num11 * 0.5) * num8;
            num10 = 1.0 - num8;
            if (num3 > 0.0)
            {
                double num9 = num8;
                num8 = num10;
                num10 = num9;
            }
        }
        else
        {
            num8 = 0.0;
            double num5 = 1.0 / (num3 * num3);
            double num6 = array5[5] * num5;
            double num7 = num5;
            for (int i = 0; i < 4; i++)
            {
                num6 = (num6 + array5[i]) * num5;
                num7 = (num7 + array6[i]) * num5;
            }

            num8 = num5 * (num6 + array5[4]) / (num7 + array6[4]);
            num8 = (0.3989422804014327 - num8) / num4;
            num5 = Math.Floor(num3 * 1.6) / 1.6;
            double num11 = (num3 - num5) * (num3 + num5);
            num8 = Math.Exp(0.0 - num5 * num5 * 0.5) * Math.Exp(0.0 - num11 * 0.5) * num8;
            num10 = 1.0 - num8;
            if (num3 > 0.0)
            {
                double num9 = num8;
                num8 = num10;
                num10 = num9;
            }
        }

        if (num8 < num2)
        {
            num8 = 0.0;
        }

        if (num10 < num2)
        {
            num10 = 0.0;
        }

        return num8;
    }

    public static double Covariance(double[] d1, double[] d2)
    {
        double num = 0.0;
        double num2 = Mean(d1);
        double num3 = Mean(d2);
        for (int i = 0; i < d1.Length; i++)
        {
            num += (d1[i] - num2) * (d2[i] - num3);
        }

        return num / (double)(d1.Length - 1);
    }

    /// <summary> 피어슨 상관계수 계산 </summary>
    public static double Pearson(double[] d1, double[] d2)
    {
        double num = Covariance(d1, d2);
        double num2 = StdDev(d1);
        double num3 = StdDev(d2);
        return num / (num2 * num3);
    }
}
