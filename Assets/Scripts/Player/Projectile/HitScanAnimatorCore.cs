using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HitScanAnimatorCore : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    [SerializeField] private Texture[] _textures;
    [Range(0.01f, 0.5f)]
    [SerializeField] private float _tickTime = 0.5f;
    private float _timer;
    private int _currentTextureSlot = 0;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        while (_timer < 0) { _timer += _tickTime; GoThroughSprite(); }
    }

    private void GoThroughSprite()
    {
        _currentTextureSlot++;
        if (_currentTextureSlot >= _textures.Length) _currentTextureSlot = 0;
        Material lineMaterial = _lineRenderer.material;
        lineMaterial.SetTexture("_MainTex", _textures[_currentTextureSlot]);
        _lineRenderer.material = lineMaterial;
    }
}
