using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SNE
{
    [System.Serializable]
    public struct MeshInfo
    {
        public Mesh Mesh;
        public Texture Texture;
    }

    [RequireComponent(typeof(ParticleEffect))]
    public class ParticleDemo : MonoBehaviour
    {
        [SerializeField]
        private MeshInfo[] _meshInfos = null;

        private ParticleEffect _effect = null;
        private int _index = -1;

        private void Start()
        {
            _effect = GetComponent<ParticleEffect>();
            Next();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                Next();
            }
        }

        private void Next()
        {
            _index = (_index + 1) % _meshInfos.Length;

            Mesh mesh = _meshInfos[_index].Mesh;
            Texture tex = _meshInfos[_index].Texture;

            _effect.SetMesh(mesh);
            _effect.SetTexture(tex);
        }
    }
}
