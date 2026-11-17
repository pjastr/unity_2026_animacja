using UnityEngine;

namespace Animacja
{
    /// <summary>Zasada 12 – mruganie: powieki zamykają się na chwilę w losowych odstępach czasu.</summary>
    public class Blink : MonoBehaviour
    {
        public Transform[] eyelids;
        public float minInterval = 1.5f;
        public float maxInterval = 4f;
        public float blinkTime = 0.15f;

        float m_Next, m_T = -1f;
        Vector3[] m_Open;

        void Start()
        {
            m_Open = new Vector3[eyelids.Length];
            for (int i = 0; i < eyelids.Length; i++) m_Open[i] = eyelids[i].localScale;
            m_Next = Time.time + Random.Range(minInterval, maxInterval);
        }

        void Update()
        {
            if (m_T < 0f && Time.time >= m_Next) m_T = 0f;
            float closed = 0f;
            if (m_T >= 0f)
            {
                m_T += Time.deltaTime;
                float k = m_T / blinkTime;
                closed = k < 1f ? Mathf.Sin(k * Mathf.PI) : 0f;
                if (k >= 1f) { m_T = -1f; m_Next = Time.time + Random.Range(minInterval, maxInterval); }
            }
            for (int i = 0; i < eyelids.Length; i++)
            {
                Vector3 s = m_Open[i];
                s.y = Mathf.Lerp(s.y, s.x, closed);   // powieka "zjeżdża" na oko
                eyelids[i].localScale = s;
            }
        }
    }
}
