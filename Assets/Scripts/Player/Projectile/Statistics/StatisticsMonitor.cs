using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatisticsMonitor : MonoBehaviour
{
    [SerializeField] private TMP_Text _totalTimeText;
    [SerializeField] private TMP_Text _aimTimeText;
    [SerializeField] private TMP_Text _shootingTimeText;
    [SerializeField] private TMP_Text _renderTimeText;

    [HideInInspector] public double _aimTimeMs = 0;
    [HideInInspector] public double _shootingTimeMs = 0;
    [HideInInspector] public double _renderTimeMs = 0;

    void LateUpdate()
    {
        double totalTime = 0;
        _renderTimeMs = FrameTimingManager.GetGpuTimerFrequency();
        totalTime = _aimTimeMs + _shootingTimeMs + _renderTimeMs;

        _aimTimeMs = 0;
        _shootingTimeMs = 0;
        _renderTimeMs = 0;
    }
}
