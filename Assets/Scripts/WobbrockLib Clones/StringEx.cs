using System;
using System.Drawing;
using System.IO;

public static class StringEx
{
    public static string Array2String(Array array)
    {
        string text = "[";
        int num = 0;
        while (array != null && num < array.Length)
        {
            text = text + array.GetValue(num).ToString() + ",";
            num++;
        }

        text = text.TrimEnd(',');
        return text + "]";
    }

    public static int[] String2IntArray(string s)
    {
        if (s == "[]")
        {
            return null;
        }

        int num = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == ',')
            {
                num++;
            }
        }

        int num2 = 1;
        int num3 = 0;
        int[] array = new int[num + 1];
        for (int j = 0; j < array.Length; j++)
        {
            num3 = s.IndexOf((j == array.Length - 1) ? ']' : ',', num2);
            try
            {
                array[j] = int.Parse(s.Substring(num2, num3 - num2));
            }
            catch
            {
                return null;
            }

            num2 = num3 + 1;
        }

        return array;
    }

    public static double[] String2DoubleArray(string s)
    {
        if (s == "[]")
        {
            return null;
        }

        int num = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == ',')
            {
                num++;
            }
        }

        int num2 = 1;
        int num3 = 0;
        double[] array = new double[num + 1];
        for (int j = 0; j < array.Length; j++)
        {
            num3 = s.IndexOf((j == array.Length - 1) ? ']' : ',', num2);
            try
            {
                array[j] = double.Parse(s.Substring(num2, num3 - num2));
            }
            catch
            {
                return null;
            }

            num2 = num3 + 1;
        }

        return array;
    }

    public static decimal[] String2DecimalArray(string s)
    {
        if (s == "[]")
        {
            return null;
        }

        int num = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == ',')
            {
                num++;
            }
        }

        int num2 = 1;
        int num3 = 0;
        decimal[] array = new decimal[num + 1];
        for (int j = 0; j < array.Length; j++)
        {
            num3 = s.IndexOf((j == array.Length - 1) ? ']' : ',', num2);
            try
            {
                array[j] = decimal.Parse(s.Substring(num2, num3 - num2));
            }
            catch
            {
                return null;
            }

            num2 = num3 + 1;
        }

        return array;
    }

    public static Rectangle String2Rectangle(string s)
    {
        Rectangle result = Rectangle.Empty;
        try
        {
            int num = s.IndexOf('X');
            int num2 = s.IndexOf(',', num + 2);
            int x = int.Parse(s.Substring(num + 2, num2 - (num + 2)));
            int num3 = s.IndexOf('Y');
            num2 = s.IndexOf(',', num3 + 2);
            int y = int.Parse(s.Substring(num3 + 2, num2 - (num3 + 2)));
            int num4 = s.IndexOf('W');
            num2 = s.IndexOf(',', num4 + 6);
            int width = int.Parse(s.Substring(num4 + 6, num2 - (num4 + 6)));
            int num5 = s.IndexOf('H');
            num2 = s.IndexOf('}', num5 + 7);
            int height = int.Parse(s.Substring(num5 + 7, num2 - (num5 + 7)));
            result = new Rectangle(x, y, width, height);
        }
        catch
        {
        }

        return result;
    }

    public static Point String2Point(string s)
    {
        Point result = Point.Empty;
        try
        {
            int num = s.IndexOf('X');
            int num2 = s.IndexOf(',', num + 2);
            int x = int.Parse(s.Substring(num + 2, num2 - (num + 2)));
            int num3 = s.IndexOf('Y');
            num2 = s.IndexOf('}', num3 + 2);
            int y = int.Parse(s.Substring(num3 + 2, num2 - (num3 + 2)));
            result = new Point(x, y);
        }
        catch
        {
        }

        return result;
    }

    public static Size String2Size(string s)
    {
        Size result = Size.Empty;
        try
        {
            int num = s.IndexOf('W');
            int num2 = s.IndexOf(',', num + 6);
            int width = int.Parse(s.Substring(num + 6, num2 - (num + 6)));
            int num3 = s.IndexOf('H');
            num2 = s.IndexOf('}', num3 + 7);
            int height = int.Parse(s.Substring(num3 + 7, num2 - (num3 + 7)));
            result = new Size(width, height);
        }
        catch
        {
        }

        return result;
    }

    public static string MakeValidFilename(string name)
    {
        char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
        for (int num = name.IndexOfAny(invalidFileNameChars); num != -1; num = name.IndexOfAny(invalidFileNameChars))
        {
            name = name.Remove(num, 1);
        }

        return name;
    }

    // RandomEx¿¡¼­ °¡Á®¿È.
    public static string RandomString(int minlen, int maxlen)
    {
        int num = UnityEngine.Random.Range(minlen, maxlen + 1);
        string text = string.Empty;
        while (text.Length < num)
        {
            text += Path.GetRandomFileName();
            int startIndex = text.IndexOf('.');
            text = text.Remove(startIndex, 1);
        }

        return text.Substring(0, num);
    }
}