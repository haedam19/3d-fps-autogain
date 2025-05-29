using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MouseLog;


public class EnvManager3D : MonoBehaviour
{
    [SerializeField] int[] Aset;
    [SerializeField] int[] Wset;
    public int trialPerCondition;
    public int blocks; // iterations (1 block = set of all conditions. 0 to blocks - 1)

    public List<ConditionConfig> conditionSequence;
    public ushort conditionIndex;
    public int blockIndex;

    public void Init()
    {
        conditionSequence = CreateConditionSequence(true);
        conditionIndex = 0;
        blockIndex = 0;
    }

    public List<ConditionConfig> CreateConditionSequence(bool shuffle)
    {
        List<ConditionConfig> conditionList = new List<ConditionConfig>();
        foreach (int A in Aset)
        {
            foreach (int W in Wset)
                conditionList.Add(new ConditionConfig(A, W));
        }

        // Shuffle Condition Sequence
        if (shuffle)
        {
            ConditionConfig temp;
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
