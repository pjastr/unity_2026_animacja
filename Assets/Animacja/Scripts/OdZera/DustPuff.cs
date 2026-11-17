using UnityEngine;

namespace Animacja
{
    /// <summary>Krótki efekt "pyłku": kula rośnie i znika.</summary>
    public class DustPuff : MonoBehaviour
    {
        public float life = 0.5f;
        public float size = 0.5f;
        float m_T;

        public static void Spawn(Vector3 pos, Material mat, float size = 0.5f)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(go.GetComponent<Collider>());
            go.name = "Pylek";
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.05f;
            if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
            go.AddComponent<DustPuff>().size = size;
        }

        void Update()
        {
            m_T += Time.deltaTime;
            float k = m_T / life;
            transform.localScale = Vector3.one * Mathf.Lerp(0.05f, size, Mathf.Sqrt(Mathf.Clamp01(k)));
            if (k >= 1f) Destroy(gameObject);
        }
    }
}
