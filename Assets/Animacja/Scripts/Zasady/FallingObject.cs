using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Zasada 9 – czas (timing): ten sam spadek z tej samej wysokości, ale inny czas i odbicia
    /// mówią widzowi, czy obiekt jest ciężki (kula do kręgli), sprężysty (piłka) czy lekki (balon).
    /// stepFps > 0 – obraz zmienia się tylko co 1/stepFps s (animacja "na dwójkach" = 12 kl./s).
    /// </summary>
    public class FallingObject : DemoActionBase
    {
        public float startHeight = 4f;
        public float gravity = 9.81f;
        [Range(0f, 0.95f)] public float restitution = 0.6f;
        [Tooltip("Opór powietrza (balon – duży)")]
        public float drag;
        public float radius = 0.5f;
        public float restTime = 1.2f;
        public float stepFps;

        Vector3 m_Base;
        float m_Y, m_Vy, m_Rest, m_StepTimer, m_Elapsed;
        float m_LastFallTime;

        void Start()
        {
            m_Base = transform.position;
            Restart();
        }

        public override void OnMainAction() => Restart();

        void Restart()
        {
            m_Y = startHeight;
            m_Vy = 0f;
            m_Rest = 0f;
            m_Elapsed = 0f;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            m_Elapsed += dt;
            m_Vy -= gravity * dt;
            m_Vy -= m_Vy * drag * dt;
            m_Y += m_Vy * dt;
            if (m_Y <= 0f)
            {
                m_Y = 0f;
                if (Mathf.Abs(m_Vy) < 0.6f) { m_Vy = 0f; m_Rest += dt; }
                else
                {
                    if (m_LastFallTime <= 0f) m_LastFallTime = m_Elapsed;
                    m_Vy = -m_Vy * restitution;
                }
            }
            if (m_Rest > restTime) { m_LastFallTime = 0f; Restart(); }

            if (stepFps > 0f)
            {
                m_StepTimer += dt;
                if (m_StepTimer < 1f / stepFps) return;
                m_StepTimer %= 1f / stepFps;
            }
            transform.position = m_Base + Vector3.up * (m_Y + radius);
        }
    }
}
