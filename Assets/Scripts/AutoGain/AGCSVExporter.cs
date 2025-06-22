using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class AGCSVExporter
{
    public static void ExportTrialsToCSV(List<AGTrialData> trials, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // 헤더 작성
            writer.WriteLine("index,isPractice,startX,startY,endX,endY,duration," +
                             "targetX,targetY,radius,w,Ae,Dx,ID,Axis,Angle,Overshoot,IsError");

            for (int i = 0; i < trials.Count; i++)
            {
                AGTrialData trial = trials[i];

                // Practice 여부
                string isPractice = trial.IsPractice.ToString().ToLower();

                // Start
                double startX = trial.Start.X;
                double startY = trial.Start.Y;

                // End
                double endX = trial.End.X;
                double endY = trial.End.Y;

                // duration
                long duration = trial.Movement.Duration;

                // Target posR
                double targetX = trial.ThisTarget.posR.X;
                double targetY = trial.ThisTarget.posR.Y;

                // 기본 파라미터
                double radius = trial.ThisTarget.radius;
                float w = trial.ThisTarget.w;
                double ae = trial.GetAe(true);
                double dx = trial.GetDx(true);
                double id = trial.ID;
                double axis = trial.Axis;
                double angle = trial.Angle;
                int overshoot = trial.TargetOvershoots;
                string isError = trial.IsError.ToString().ToLower();

                // CSV 한 줄 작성
                string line = $"{i},{isPractice},{startX:F1},{startY:F1}," +
                              $"{endX:F1},{endY:F1},{duration}," +
                              $"{targetX:F1},{targetY:F1},{radius:F3},{w:F3},{ae:F3},{dx:F3}," +
                              $"{id:F3},{axis:F3},{angle:F3},{overshoot},{isError}";

                writer.WriteLine(line);
            }
        }

        Debug.Log($"[AGCSVExporter] Trial data exported to CSV: {filePath}");
    }

    public static string GetTimestampedFilename(string prefix = "trial_results", string extension = "csv")
    {
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return $"{prefix}_{timestamp}.{extension}";
    }
}
