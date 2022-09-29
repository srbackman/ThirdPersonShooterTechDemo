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

        _totalTimeText.text = "Total: " + totalTime.ToString() + "ms";
        _aimTimeText.text = "Aiming: " + _aimTimeMs.ToString() + "ms";
        _shootingTimeText.text = "Shooting: " + _shootingTimeMs.ToString() + "ms";
        _renderTimeText.text = "Render: " + _renderTimeMs.ToString() + "ms";


        _aimTimeMs = 0;
        _shootingTimeMs = 0;
        _renderTimeMs = 0;
    }
}
