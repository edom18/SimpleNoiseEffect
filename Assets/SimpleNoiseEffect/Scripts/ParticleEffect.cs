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
        _particlesBuf = new ComputeBuffer(ParticleNum, Marshal.SizeOf(typeof(Particle)));

        Particle[] particles = GenerateParticles();
        _particlesBuf.SetData(particles);

        _kernelIndex = _computeShader.FindKernel("CurlNoiseMain");
    }

    private void UpdatePosition()
    {
        _computeShader.SetFloat("_NoiseScale", _noiseScale);
        _computeShader.SetFloat("_Progress", _progress);
        _computeShader.SetFloat("_Intensity", _intensity);
        _computeShader.SetFloat("_Rotate", _rotation);
        _computeShader.SetBuffer(_kernelIndex, "_Particles", _particlesBuf);

        _computeShader.Dispatch(_kernelIndex, ParticleNum / 8, 1, 1);

        _material.SetFloat("_Size", _size * (1.0f - _progress));
        _material.SetBuffer("_Particles", _particlesBuf);
    }

    private CommandBuffer CreateCommandBuffer()
    {
        CommandBuffer buf = new CommandBuffer();
        buf.DrawProcedural(Matrix4x4.identity, _material, 0, MeshTopology.Points, ParticleNum);
        return buf;
    }

    private Particle[] GenerateParticles()
    {
        Particle[] particles = new Particle[_targetMesh.vertexCount];

        for (int i = 0; i < _targetMesh.vertexCount; i++)
        {
            Particle p = new Particle
            {
                Position = _targetMesh.vertices[i],
                UV = _targetMesh.uv[i],
            };

            particles[i] = p;
        }

        return particles;
    }
}
