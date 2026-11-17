using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Zasada 4 – "straight ahead": każda klatka powstaje z poprzedniej (bez klatek kluczowych).
    /// Łańcuch segmentów (płomień / macka): kąt segmentu goni kąt poprzednika z opóźnieniem, do tego szum.
    /// Ruch jest organiczny i nieprzewidywalny – trudno go zaplanować co do klatki.
    /// </summary>
    public class StraightAheadChain : MonoBehaviour
    {
        public Transform[] segments;
        public float follow = 7f;
        public float noise = 55f;
        public float noiseSpeed = 1.3f;

        float[] m_AngleX;
        float[] m_AngleZ;
        float m_Seed;

        void Start()
        {
            m_AngleX = new float[segments.Length];
            m_AngleZ = new float[segments.Length];
            m_Seed = Random.value * 100f;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            float t = Time.time * noiseSpeed + m_Seed;
            // segment 0 – "pomysł animatora" w tej klatce (szum), kolejne – reakcja na poprzednika
            float tx = (Mathf.PerlinNoise(t, 0.3f) - 0.5f) * noise;
            float tz = (Mathf.PerlinNoise(0.7f, t) - 0.5f) * noise;
            for (int i = 0; i < segments.Length; i++)
            {
                float k = 1f - Mathf.Exp(-follow * dt);
                m_AngleX[i] = Mathf.Lerp(m_AngleX[i], tx, k);
                m_AngleZ[i] = Mathf.Lerp(m_AngleZ[i], tz, k);
                segments[i].localRotation = Quaternion.Euler(m_AngleX[i], 0f, m_AngleZ[i]);
                tx = m_AngleX[i] * 1.15f;
                tz = m_AngleZ[i] * 1.15f;
            }
        }
    }
}
