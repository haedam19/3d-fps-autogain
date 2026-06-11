using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public static class AGCSVImporter
{
    private const string TrialResultHeader = "index,isPractice,startX,startY,endX,endY,duration,targetX,targetY,radius,w,Ae,Dx,ID,Axis,Angle,Overshoot,IsError";
    private const int ColumnCount = 18;

    public static void ImportCSVtoTrials(string source, out List<AGTrialData> trial)
    {
        trial = new List<AGTrialData>();

        if (!File.Exists(source))
            throw new FileNotFoundException("Trial result CSV file does not exist.", source);

        string[] lines = File.ReadAllLines(source);
        if (lines.Length == 0)
            throw new InvalidDataException("Trial result CSV file is empty.");

        if (lines[0] != TrialResultHeader)
            throw new InvalidDataException("Trial result CSV header is invalid.");

        AGTargetData previousTarget = AGTargetData.Empty;

        for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
        {
            if (string.IsNullOrWhiteSpace(lines[lineIndex]))
                continue;

            string[] fields = lines[lineIndex].Split(',');
            if (fields.Length != ColumnCount)
                throw new InvalidDataException($"Invalid column count at line {lineIndex + 1}: {fields.Length}");

            int index = ParseInt(fields[0], lineIndex, "index");
            bool isPractice = ParseBool(fields[1], lineIndex, "isPractice");
            double startX = ParseDouble(fields[2], lineIndex, "startX");
            double startY = ParseDouble(fields[3], lineIndex, "startY");
            double endX = ParseDouble(fields[4], lineIndex, "endX");
            double endY = ParseDouble(fields[5], lineIndex, "endY");
            long duration = ParseLong(fields[6], lineIndex, "duration");
            double targetX = ParseDouble(fields[7], lineIndex, "targetX");
            double targetY = ParseDouble(fields[8], lineIndex, "targetY");
            double radius = ParseDouble(fields[9], lineIndex, "radius");
            float w = ParseFloat(fields[10], lineIndex, "w");

            PointR start = new PointR(startX, startY);
            PointR end = new PointR(endX, endY);
            AGTargetData currentTarget = CreateTargetData(targetX, targetY, radius, w);

            if (lineIndex == 1)
                previousTarget = CreateTargetData(startX, startY, radius, w);

            AGTrialData importedTrial = new AGTrialData(index, isPractice, previousTarget, currentTarget)
            {
                Start = new TimePointR(start, 0L),
                End = new TimePointR(end, duration),
                A = PointR.Distance(start, currentTarget.posR),
                W = w
            };

            // trial_results에는 원본 movement sample 전체가 없으므로 start/end 두 점만 복원한다.
            // 이어하기에서 이전 trial 수와 마지막 target 연결을 복구하는 용도에는 충분하지만,
            // 원본 overshoot/submovement/raw speed 같은 세부 movement 정보는 복원할 수 없다.
            importedTrial.Movement.AddMove(new TimePointR(start, 0L));
            importedTrial.Movement.AddMove(new TimePointR(end, duration));

            trial.Add(importedTrial);
            previousTarget = currentTarget;
        }
    }

    private static AGTargetData CreateTargetData(double x, double y, double radius, float w)
    {
        PointR posR = new PointR(x, y);
        return new AGTargetData
        {
            posWorld = Vector3.zero,
            posRefScreen = new Vector2((float)x, Screen.height - (float)y),
            posR = posR,
            radius = radius,
            w = w
        };
    }

    private static int ParseInt(string value, int lineIndex, string columnName)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            return result;

        throw new InvalidDataException($"Invalid {columnName} at line {lineIndex + 1}: {value}");
    }

    private static long ParseLong(string value, int lineIndex, string columnName)
    {
        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result))
            return result;

        throw new InvalidDataException($"Invalid {columnName} at line {lineIndex + 1}: {value}");
    }

    private static bool ParseBool(string value, int lineIndex, string columnName)
    {
        if (bool.TryParse(value, out bool result))
            return result;

        throw new InvalidDataException($"Invalid {columnName} at line {lineIndex + 1}: {value}");
    }

    private static double ParseDouble(string value, int lineIndex, string columnName)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
            return result;

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out result))
            return result;

        throw new InvalidDataException($"Invalid {columnName} at line {lineIndex + 1}: {value}");
    }

    private static float ParseFloat(string value, int lineIndex, string columnName)
    {
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            return result;

        if (float.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out result))
            return result;

        throw new InvalidDataException($"Invalid {columnName} at line {lineIndex + 1}: {value}");
    }
}
