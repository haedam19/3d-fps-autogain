using UnityEngine;
using System.IO;

public class ProjectPaths
{
    /*
     * <Application.dataPath>
     * 입력 및 에디터 출력 디렉터리로 사용
     * - Editor: Assets
     * - StandAlone(Windows): (빌드 디렉터리)/<product name>_Data
     * 
     * Application.persistentDataPath
     * 스탠드얼론 출력 디렉터리로 사용
     * C:\Users\<username>\AppData\LocalLow\<company name>\<product name>
     */

    public static string ConfigPath => Path.Combine(Application.dataPath, "_config");
    public static string LogPath
    {
        get
        {
#if UNITY_EDITOR
            return Path.Combine(Application.dataPath, "_log");
#elif UNITY_STANDALONE_WIN
            return Path.Combine(Application.persistentDataPath, "_log");
#endif
        }
    } 

    public static string FittsLogPath => Path.Combine(LogPath, "Fitts");

    public static string AutoGainLogPath => Path.Combine(LogPath, "AutoGain");
}