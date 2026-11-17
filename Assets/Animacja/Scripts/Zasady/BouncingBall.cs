using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Zasada 1 (i 9): piłka odbijająca się od podłoża, liczona w kodzie.
    /// Bez zgniatania – sztywna kula; ze zgniataniem – rozciąga się w locie zgodnie z prędkością
    /// i spłaszcza przy uderzeniu, ZACHOWUJĄC OBJĘTOŚĆ (sx · sy · sz = 1).
    /// </summary>
    public class BouncingBall : DemoActionBase
    {
        public float height = 3f;
        [Tooltip("Czas jednego odbicia [s]")]
        public float period = 1f;
        public float diameter = 1f;
        public bool squashAndStretch = true;
        [Range(0f, 0.8f)] public float squash = 0.45f;
        [Range(0f, 0.8f)] public float stretch = 0.35f;
        [Tooltip("Część okresu, przez którą piłka jest w kontakcie z podłożem")]
        [Range(0.01f, 0.3f)] public float contact = 0.08f;

        Vector3 m_Base;
        float m_Time;

        void Start()
        {
            m_Base = transform.position;
        }

        public override void OnMainAction()
        {
            m_Time = 0f;
        }

        void Update()
        {
            m_Time += Time.deltaTime;
            float p = Mathf.Repeat(m_Time / period, 1f);
            float sy = 1f;
            float y;
            float half = contact * 0.5f;
            if (p < half || p > 1f - half)
            {
                // kontakt z podłożem
                float d = p < half ? p / half : (1f - p) / half;     // 0 w chwili uderzenia, 1 na początku/końcu kontaktu
                sy = squashAndStretch ? Mathf.Lerp(1f - squash, 1f, d) : 1f;
                y = 0f;
            }
            else
            {
                float q = (p - half) / (1f - contact);                  // 0..1 lot
                y = height * 4f * q * (1f - q);
                float speed = Mathf.Abs(1f - 2f * q);                   // 1 przy ziemi, 0 w szczycie
                sy = squashAndStretch ? 1f + stretch * speed * speed : 1f;
            }
            float sxz = 1f / Mathf.Sqrt(sy);                            // zachowanie objętości
            transform.localScale = new Vector3(sxz, sy, sxz) * diameter;
            transform.position = m_Base + Vector3.up * (y + diameter * sy * 0.5f);
        }
    }
}
