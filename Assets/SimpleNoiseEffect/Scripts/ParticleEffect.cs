using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public struct Particle
{
    public Vector3 Position;
    public Vector3 OutPosition;
    public float Scale;
    public Vector2 UV;
    // public Vector3 Color;
}

public class ParticleEffect : MonoBehaviour
{
    [SerializeField]
    private ComputeShader _computeShader = null;

    [SerializeField]
    private Shader _shader = null;

    [SerializeField]
    private Mesh _targetMesh = null;

    #region ### Particle Parameters ###
    [Header("== Particle parameters ==")]
    [SerializeField]
    private Material _material = null;

    [SerializeField]
    private float _size = 3f;
    #endregion ### Particle Parameters ###

    #region ### Noise Parameters ###
    [Header("== Noise parameters ==")]
    [SerializeField]
    private float _noiseScale = 1f;

    [SerializeField]
    private float _rotation = 0.1f;

    [SerializeField]
    private float _intensity = 1f;
    #endregion ### Noise Parameters ###

    [Header("== Control ==")]
    [SerializeField, Range(0, 1f)]
    private float _progress = 0;

    private ComputeBuffer _particlesBuf = null;
    private int _kernelIndex = 0;
    private Dictionary<Camera, CommandBuffer> _camBuffers = new Dictionary<Camera, CommandBuffer>();

    private int ParticleNum => _targetMesh.vertexCount;
    private int _particleNumRoot = 0;

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        UpdatePosition();
    }

    private void OnDestroy()
    {
        CleanUp();
    }

    private void CleanUp()
    {
        foreach (var cam in _camBuffers)
        {
            if (cam.Key)
            {
                cam.Key.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, cam.Value);
            }
        }

        if (_particlesBuf != null)
        {
            _particlesBuf.Release();
        }
    }

    private void OnWillRenderObject()
    {
        if (_camBuffers.ContainsKey(Camera.current))
        {
            return;
        }

        Camera cam = Camera.current;

        CommandBuffer buf = CreateCommandBuffer();
        cam.AddCommandBuffer(CameraEvent.BeforeImageEffects, buf);

        _camBuffers.Add(cam, buf);
    }

    private void Initialize()
    {
        CaluculateNum();

        _particlesBuf = new ComputeBuffer(_particleNumRoot * _particleNumRoot, Marshal.SizeOf(typeof(Particle)));

        Particle[] particles = GenerateParticles();
        _particlesBuf.SetData(particles);

        _kernelIndex = _computeShader.FindKernel("CurlNoiseMain");
    }

    private void CaluculateNum()
    {
        _particleNumRoot = (int)Mathf.Ceil(Mathf.Sqrt((float)ParticleNum));
    }

    private void UpdatePosition()
    {
        _computeShader.SetFloat("_NoiseScale", _noiseScale);
        _computeShader.SetFloat("_Progress", _progress);
        _computeShader.SetFloat("_Intensity", _intensity);
        _computeShader.SetFloat("_Rotate", _rotation);
        _computeShader.SetInt("_PrticleNumPerRow", _particleNumRoot);
        _computeShader.SetBuffer(_kernelIndex, "_Particles", _particlesBuf);

        _computeShader.Dispatch(_kernelIndex, _particleNumRoot / 8, _particleNumRoot / 8, 1);

        _material.SetFloat("_Size", _size * (1.0f - _progress));
        _material.SetBuffer("_Particles", _particlesBuf);
    }

    private CommandBuffer CreateCommandBuffer()
    {
        CommandBuffer buf = new CommandBuffer();
        buf.DrawProcedural(transform.localToWorldMatrix, _material, 0, MeshTopology.Points, ParticleNum);
        return buf;
    }

    /// <summary>
    /// Generate particles.
    /// 
    /// This method may create over the vertex count. Because the buffer will be created by square value from the vertex count.
    /// So the particles length will be power of the value.
    /// </summary>
    /// <returns></returns>
    private Particle[] GenerateParticles()
    {
        Particle[] particles = new Particle[_particleNumRoot * _particleNumRoot];

        for (int i = 0; i < particles.Length; i++)
        {
            int idx = i % _targetMesh.vertexCount;

            Particle p = new Particle
            {
                Position = _targetMesh.vertices[idx],
                UV = _targetMesh.uv[idx],
            };

            particles[i] = p;
        }

        return particles;
    }
}
