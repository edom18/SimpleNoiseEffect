using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseEffect : MonoBehaviour
{
    [SerializeField]
    private float _scale = 0.2f;

    [SerializeField]
    private float _rotation = 0.2f;

    [SerializeField]
    private float _intensity = 1.0f;

    [SerializeField, Range(0, 1f)]
    private float _progress = 0;

    private List<Renderer> _renderers = new List<Renderer>();
    private MaterialPropertyBlock _block = null;

    private int _intensityId = 0;
    private int _progressId = 0;
    private int _scaleId = 0;
    private int _rotateId = 0;

    private void Awake()
    {
        Initialize();
    }

    private void OnValidate()
    {
        UpdateProperties();
    }

    private void UpdateProperties()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (_block == null)
        {
            return;
        }

        _block.SetFloat(_progressId, _progress);
        _block.SetFloat(_intensityId, _intensity);
        _block.SetFloat(_scaleId, _scale);
        _block.SetFloat(_rotateId, _rotation);

        foreach (var ren in _renderers)
        {
            ren.SetPropertyBlock(_block);
        }
    }

    private void Initialize()
    {
        _block = new MaterialPropertyBlock();

        _progressId = Shader.PropertyToID("_Progress");
        _scaleId = Shader.PropertyToID("_Scale");
        _rotateId = Shader.PropertyToID("_Rotate");
        _intensityId = Shader.PropertyToID("_Intensity");

        SkinnedMeshRenderer[] skinRens = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var sren in skinRens)
        {
            sren.sharedMesh.SetIndices(sren.sharedMesh.GetIndices(0), MeshTopology.Points, 0);
        }

        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
        foreach (var f in filters)
        {
            f.mesh.SetIndices(f.mesh.GetIndices(0), MeshTopology.Points, 0);
        }

        MeshRenderer[] rens = GetComponentsInChildren<MeshRenderer>();
        _renderers.AddRange(skinRens);
        _renderers.AddRange(rens);

        UpdateProperties();
    }
}
