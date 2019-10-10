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
    // public Vector3 Color;
}

public class ParticleEffect : MonoBehaviour
{
    [SerializeField]
    private ComputeShader _computeShader = null;

    [SerializeField]
    private Shader _shader = null;

    #region ### Particle Parameters ###
    [Header("== Particle parameters ==")]
    [SerializeField]
    private int _maxParticleNum = 1000;

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

    private Material _material = null;
    private ComputeBuffer _particlesBuf = null;
    private CommandBuffer _commandBuf = null;
    private int _kernelIndex = 0;

    private void Start()
    {
        Initialize();
        Camera.main.AddCommandBuffer(CameraEvent.BeforeImageEffects, _commandBuf);
    }

    private void Update()
    {
        UpdatePosition();
    }

    private void OnDestroy()
    {
        _particlesBuf.Release();
    }

    private void Initialize()
    {
        _material = new Material(_shader);
        _material.color = new Color(0.2f, 0.5f, 1f);

        _commandBuf = new CommandBuffer();
        _commandBuf.DrawProcedural(Matrix4x4.identity, _material, 0, MeshTopology.Points, _maxParticleNum);

        _particlesBuf = new ComputeBuffer(_maxParticleNum, Marshal.SizeOf(typeof(Particle)));

        Particle[] particles = GenerateParticles(_maxParticleNum);
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

        _computeShader.Dispatch(_kernelIndex, _maxParticleNum / 8, 1, 1);

        _material.SetFloat("_Size", _size * (1.0f - _progress));
        _material.SetBuffer("_Particles", _particlesBuf);
    }

    private Particle[] GenerateParticles(int particleNum)
    {
        Particle[] particles = new Particle[particleNum];

        for (int i = 0; i < particleNum; i++)
        {
            float x = Random.Range(-0.1f, 0.1f);
            float y = Random.Range(-0.1f, 0.1f);
            float z = Random.Range(-0.1f, 0.1f);

            Particle p = new Particle
            {
                Position = new Vector3(x, y, z),
            };

            particles[i] = p;
        }

        return particles;
    }
}
