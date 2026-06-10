[System.Serializable]
public class MetaData
{
    public string type = "metaData";
    public string timestamp;
    public double binSize;
    public int binCount;
}

[System.Serializable]
public class FullGainCurve
{
    public string type = "fullGainCurve";
    public string timestamp;
    public int updateIndex;
    public float[] gains;
    public int[] binUpdateCounts;
}

[System.Serializable]
public class GainSnapshot
{
    public string type = "gainSnapshot";
    public string timestamp;
    public int updateIndex;
    public float[] gains;
}
