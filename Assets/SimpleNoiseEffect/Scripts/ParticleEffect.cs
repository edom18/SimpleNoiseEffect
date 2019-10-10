using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public struct Particle
{
    public Vector3 Position;
    public Vector3 OutPosition;
    public float Scale;
    // public Vector3 Color;
}

public class ParticleEffect : MonoBehaviour
{
    private const int MAX_VERTICES_IN_MESH = 65534;

    [SerializeField]
    private ComputeShader _computeShader = null;

    [SerializeField]
    private Shader _shader = null;

    [SerializeField]
    private Mesh _mesh = null;

    #region ### Noise Parameters ###
    [Header("Noise parameters")]
    [SerializeField]
    private int _maxParticleNum = 1000;

    [SerializeField]
    private float _scale = 1f;

    [SerializeField]
    private float _rotation = 0.1f;

    [SerializeField]
    private float _intensity = 1f;

    [SerializeField, Range(0, 1f)]
    private float _progress = 0;
    #endregion ### Noise Parameters ###

    private Material _material = null;
    private Mesh _combinedMesh = null;
    private ComputeBuffer _particlesBuf = null;
    private int _kernelIndex = 0;

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
        _particlesBuf.Release();
    }

    private void Initialize()
    {
        _material = new Material(_shader);
        _material.SetFloat("_Scale", 0.1f);
        _material.color = new Color(0.2f, 0.5f, 1f);

        // Calculate how many mesh can be included in one mesh.
        int particleNumPerMesh = MAX_VERTICES_IN_MESH / _mesh.vertexCount;
        int meshNum = (int)Mathf.Ceil((float)_maxParticleNum / particleNumPerMesh);

        _combinedMesh = GenerateCombinedMesh(_mesh, particleNumPerMesh);

        // Allocate a buffer for the compute shader.
        _particlesBuf = new ComputeBuffer(_maxParticleNum, Marshal.SizeOf(typeof(Particle)));

        Particle[] particles = GenerateParticles(_maxParticleNum);
        _particlesBuf.SetData(particles);

        _kernelIndex = _computeShader.FindKernel("CurlNoiseMain");
    }

    private void UpdatePosition()
    {
        _computeShader.SetFloat("_Scale", _scale);
        _computeShader.SetFloat("_Progress", _progress);
        _computeShader.SetFloat("_Intensity", _intensity);
        _computeShader.SetFloat("_Rotate", _rotation);
        _computeShader.SetBuffer(_kernelIndex, "_Particles", _particlesBuf);

        _computeShader.Dispatch(_kernelIndex, _maxParticleNum / 8, 1, 1);

        _material.SetBuffer("_Particles", _particlesBuf);
        Graphics.DrawMesh(_combinedMesh, transform.position, transform.rotation, _material, 0);
    }

    private Mesh GenerateCombinedMesh(Mesh baseMesh, int num)
    {
        // UnityEngine.Assertions.Assert.IsTrue(baseMesh.vertexCount * _maxParticleNum <= MAX_VERTICES_IN_MESH);

        int[] meshIndices = baseMesh.GetIndices(0);
        int indNum = meshIndices.Length;

        int[] indices = new int[num * indNum];

        List<Vector2> uv0 = new List<Vector2>();
        List<Vector2> uv1 = new List<Vector2>();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();

        for (int id = 0; id < num; id++)
        {
            vertices.AddRange(baseMesh.vertices);
            tangents.AddRange(baseMesh.tangents);
            normals.AddRange(baseMesh.normals);
            uv0.AddRange(baseMesh.uv);

            for (int n = 0; n < indNum; n++)
            {
                indices[id * indNum + n] = id * baseMesh.vertexCount + meshIndices[n];
            }

            for (int u = 0; u < _mesh.uv.Length; u++)
            {
                uv1.Add(new Vector2(id, id));
            }
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uv0);
        mesh.SetUVs(1, uv1);
        mesh.SetTangents(tangents);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        return mesh;
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
