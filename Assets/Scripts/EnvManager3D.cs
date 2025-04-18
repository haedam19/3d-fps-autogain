using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Condition
{
    public int A; // Diameter of the circle which targets are aligned along
    public int W; // Target width

    public Condition(int a, int w) { A = a; W = w; }
}

public class EnvManager3D : MonoBehaviour
{
    [SerializeField] int[] Aset;
    [SerializeField] int[] Wset;
    public int trialPerCondition;
    public int blocks; // iterations (1 block = set of all conditions. 0 to blocks - 1)

    public List<Condition> conditionSequence;
    public ushort conditionIndex;
    public int blockIndex;

    public void Init()
    {
        conditionSequence = CreateConditionSequence(true);
        conditionIndex = 0;
        blockIndex = 0;
    }

    public List<Condition> CreateConditionSequence(bool shuffle)
    {
        List<Condition> conditionList = new List<Condition>();
        foreach (int A in Aset)
        {
            foreach (int W in Wset)
                conditionList.Add(new Condition(A, W));
        }

        // Shuffle Condition Sequence
        if (shuffle)
        {
            Condition temp;
            int length = conditionList.Count;
            int i, j;
            for (i = 0; i < length; i++)
            {
                j = UnityEngine.Random.Range(i, length);
                temp = conditionList[i];
                conditionList[i] = conditionList[j];
                conditionList[j] = temp;
            }
        }
        
        return conditionList;
    }
}
