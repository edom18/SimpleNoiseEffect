using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearTarget : MonoBehaviour
{
    [SerializeField]
    private float _height = 1.5f;

    [SerializeField, Range(0, 1f)]
    private float _fit = 0;

    public float Height => _height;

    private SkinnedMeshRenderer[] _renderers = null;
    private MaterialPropertyBlock _block = null;

    private int _fitId = 0;

    private void Awake()
    {
        Initialize();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        _block.SetFloat(_fitId, _fit);

        foreach (var ren in _renderers)
        {
            ren.SetPropertyBlock(_block);
        }
    }

    private void Initialize()
    {
        _block = new MaterialPropertyBlock();
        _block.SetFloat("_Height", _height);

        _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (var ren in _renderers)
        {
            ren.sharedMesh.SetIndices(ren.sharedMesh.GetIndices(0), MeshTopology.Points, 0);
            ren.SetPropertyBlock(_block);
        }

        _fitId = Shader.PropertyToID("_Fit");
    }
}
