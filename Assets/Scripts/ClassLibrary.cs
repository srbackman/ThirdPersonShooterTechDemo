using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassLibrary : MonoBehaviour
{
    public GenericFunctions genericFunctions;
    public PoolingManager poolingManager;
    public StatisticsMonitor statisticsMonitor;

    private void Awake()
    {
        genericFunctions = FindObjectOfType<GenericFunctions>();
        poolingManager = FindObjectOfType<PoolingManager>();
        statisticsMonitor = FindObjectOfType<StatisticsMonitor>();
    }
}
