using UnityEngine;

namespace Animacja
{
    /// <summary>Prosty ruch po okręgu z falowaniem w pionie (np. motyl, cel do śledzenia).</summary>
    public class OrbitMover : MonoBehaviour
    {
        public Vector3 center;
        public float radius = 2.5f;
        public float speed = 0.8f;
        public float bob = 0.5f;
        public float phase;

        void Update()
        {
            float a = Time.time * speed + phase;
            transform.position = center + new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a * 2.3f) * bob, Mathf.Sin(a) * radius * 0.6f);
            transform.Rotate(0f, 0f, 720f * Time.deltaTime * Mathf.Sin(Time.time * 12f));
        }
    }
}
