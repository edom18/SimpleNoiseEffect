using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace SNE
{
    /// <summary>
    /// Particle information.
    /// </summary>
    public struct Particle
    {
        public Vector3 Position;
        public Vector3 OutPosition;
        public float Scale;
        public Vector2 UV;
    }

    /// <summary>
    /// Particle effect contorller.
    /// 
    /// This component will control all of particle effects.
    /// Set up buffers, update particle infos, and clean up them.
    /// </summary>
    public class ParticleEffect : MonoBehaviour
    {
        struct PropertyIdDef
        {
            public int NoiseScale;
            public int Progress;
            public int Intensity;
            public int Rotate;
            public int ParticleNumPerRow;
            public int Particles;
            public int Size;
        }

        [SerializeField]
        private ComputeShader _computeShader = null;

        [SerializeField]
        private Shader _shader = null;


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
        private Dictionary<Camera, CommandBuffer> _camBuffers = new Dictionary<Camera, CommandBuffer>();

        private PropertyIdDef _propertyIdDef = default;
        private Mesh _targetMesh = null;

        private int ParticleNum => _targetMesh.vertexCount;

        private int _kernelIndex = 0;
        private int _particleNumRoot = 0;

        #region ### MonoBehaviour ###
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

        private void OnWillRenderObject()
        {
            if (_targetMesh == null)
            {
                return;
            }

            if (_camBuffers.ContainsKey(Camera.current))
            {
                return;
            }

            Camera cam = Camera.current;

            CommandBuffer buf = CreateCommandBuffer();
            cam.AddCommandBuffer(CameraEvent.BeforeImageEffects, buf);

            _camBuffers.Add(cam, buf);
        }
        #endregion ### MonoBehaviour ###

        /// <summary>
        /// Set a mesh to visualize vertices.
        /// </summary>
        /// <param name="mesh">Visualize target.</param>
        public void SetMesh(Mesh mesh)
        {
            _targetMesh = mesh;
            ResetBuffers();
        }

        /// <summary>
        /// Set a texture to paint for vertices.
        /// </summary>
        /// <param name="texture">Paint texture.</param>
        public void SetTexture(Texture texture)
        {
            _material.mainTexture = texture;
        }

        /// <summary>
        /// Control progress. This method can take 0 - 1 value.
        /// </summary>
        /// <param name="t">Progress.</param>
        public void Progres(float t)
        {
            t = Mathf.Clamp01(t);
            _progress = t;
        }

        /// <summary>
        /// Clean up command buffers and compute buffers.
        /// </summary>
        private void CleanUp()
        {
            foreach (var cam in _camBuffers)
            {
                if (cam.Key)
                {
                    cam.Key.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, cam.Value);
                }
            }

            _camBuffers.Clear();

            if (_particlesBuf != null)
            {
                _particlesBuf.Release();
            }
        }

        /// <summary>
        /// Initialize this component.
        /// 
        /// Create buffers and get kernel indices.
        /// </summary>
        private void Initialize()
        {
            CreatePropertyId();
            GetKernelIndies();
            ResetBuffers();
        }

        /// <summary>
        /// Reset buffers.
        /// 
        /// If the buffers already exist, it will be released for next allocating buffers.
        /// </summary>
        private void ResetBuffers()
        {
            CleanUp();
            CreateBuffers();
        }

        /// <summary>
        /// Create all property IDs for using the particles
        /// </summary>
        private void CreatePropertyId()
        {
            _propertyIdDef = new PropertyIdDef
            {
                NoiseScale = Shader.PropertyToID("_NoiseScale"),
                Progress = Shader.PropertyToID("_Progress"),
                Intensity = Shader.PropertyToID("_Intensity"),
                Rotate = Shader.PropertyToID("_Rotate"),
                ParticleNumPerRow = Shader.PropertyToID("_ParticleNumPerRow"),
                Particles = Shader.PropertyToID("_Particles"),
                Size = Shader.PropertyToID("_Size"),
            };
        }

        /// <summary>
        /// Get all kernel indices.
        /// </summary>
        private void GetKernelIndies()
        {
            _kernelIndex = _computeShader.FindKernel("CurlNoiseMain");
        }

        /// <summary>
        /// Caluculate number from particle count to a root value.
        /// </summary>
        private void CaluculateNum()
        {
            _particleNumRoot = (int)Mathf.Ceil(Mathf.Sqrt(ParticleNum));
        }

        /// <summary>
        /// Create buffers for using particles.
        /// </summary>
        private void CreateBuffers()
        {
            if (_targetMesh == null)
            {
                return;
            }

            CaluculateNum();

            _particlesBuf = new ComputeBuffer(_particleNumRoot * _particleNumRoot, Marshal.SizeOf(typeof(Particle)));

            Particle[] particles = GenerateParticles();
            _particlesBuf.SetData(particles);
        }

        /// <summary>
        /// Create a command buffer for drawing points.
        /// </summary>
        /// <returns>The command buffer.</returns>
        private CommandBuffer CreateCommandBuffer()
        {
            CommandBuffer buf = new CommandBuffer();
            buf.DrawProcedural(transform.localToWorldMatrix, _material, 0, MeshTopology.Points, ParticleNum);
            return buf;
        }

        /// <summary>
        /// Update the particles position.
        /// </summary>
        private void UpdatePosition()
        {
            if (_targetMesh == null)
            {
                return;
            }

            _computeShader.SetFloat(_propertyIdDef.NoiseScale, _noiseScale);
            _computeShader.SetFloat(_propertyIdDef.Progress, _progress);
            _computeShader.SetFloat(_propertyIdDef.Intensity, _intensity);
            _computeShader.SetFloat(_propertyIdDef.Rotate, _rotation);
            _computeShader.SetInt(_propertyIdDef.ParticleNumPerRow, _particleNumRoot);
            _computeShader.SetBuffer(_kernelIndex, _propertyIdDef.Particles, _particlesBuf);

            _computeShader.Dispatch(_kernelIndex, _particleNumRoot / 8, _particleNumRoot / 8, 1);

            _material.SetFloat(_propertyIdDef.Size, _size * (1.0f - _progress));
            _material.SetBuffer(_propertyIdDef.Particles, _particlesBuf);
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
}

