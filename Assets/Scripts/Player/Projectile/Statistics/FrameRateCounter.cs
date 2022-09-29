using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FrameRateCounter : MonoBehaviour
{
    public int _avgFrameRate;
    public int _maxFrameRate = 1000;
    public TMP_Text _displayFpsText;

    private void Awake()
    {
        Application.targetFrameRate = _maxFrameRate;
        QualitySettings.vSyncCount = 0;
    }

    public void Update()
    {
        float current = 0;
        current = (int)(1f / Time.unscaledDeltaTime);
        _avgFrameRate = (int)current;
        _displayFpsText.text = "fps: " + _avgFrameRate.ToString();
    }
}
