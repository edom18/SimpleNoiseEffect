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

        private int _index = -1;
        private ParticleEffect _effect = null;
        private Coroutine _coroutine = null;

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

            if (Input.GetKeyDown(KeyCode.F))
            {
                AnimationFoward();
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                AnimationBack();
            }
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(50, 50, 150, 30), "Animation forward (F)"))
            {
                AnimationFoward();
            }

            if (GUI.Button(new Rect(50, 100, 150, 30), "Animation back (B)"))
            {
                AnimationBack();
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

        private void AnimationFoward()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }

            _coroutine = StartCoroutine(AnimationImpl(1f, false));
        }

        private void AnimationBack()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }

            _coroutine = StartCoroutine(AnimationImpl(1f, true));
        }

        private IEnumerator AnimationImpl(float duration, bool reverse)
        {
            float time = 0;

            while (true)
            {
                if (time >= duration)
                {
                    _effect.Progres(reverse ? 0 : 1f);
                    break;
                }

                float t = time / duration;
                t = t * t;

                if (reverse)
                {
                    t = 1f - t;
                }

                _effect.Progres(t);

                yield return null;

                time += Time.deltaTime;
            }
        }
    }
}
