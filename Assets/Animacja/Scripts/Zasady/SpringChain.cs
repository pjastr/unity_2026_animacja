using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Zasada 5 (i 8): części luźne (antena, ogon, ucho) jako łańcuch tłumionych sprężyn.
    /// Kontynuacja ruchu (follow through) – po zatrzymaniu ciała segmenty jeszcze się wychylają;
    /// nakładanie się ruchów (overlapping action) – każdy kolejny segment reaguje z opóźnieniem.
    /// </summary>
    public class SpringChain : MonoBehaviour
    {
        public Transform[] segments;
        [Tooltip("Źródło przyspieszenia (np. MoveStopCycle). Puste = liczone z ruchu tego obiektu.")]
        public MoveStopCycle driver;
        public bool rigid;
        public float stiffness = 90f;
        public float damping = 6f;
        public float inertia = 60f;
        public float coupling = 1.2f;
        public Vector3 bendAxis = Vector3.forward;
        [Tooltip("Wpływ przyspieszenia pionowego (skok, lądowanie) na wychylenie")]
        public float verticalInfluence;

        float[] m_Angle;
        float[] m_Vel;
        Vector3 m_PrevPos, m_PrevVel;
        Quaternion[] m_Rest;

        void Start()
        {
            m_Angle = new float[segments.Length];
            m_Vel = new float[segments.Length];
            m_Rest = new Quaternion[segments.Length];
            for (int i = 0; i < segments.Length; i++) m_Rest[i] = segments[i].localRotation;
            m_PrevPos = transform.position;
        }

        void LateUpdate()
        {
            float dt = Mathf.Clamp(Time.deltaTime, 1e-4f, 0.05f);
            Vector3 acc;
            if (driver != null) acc = driver.Acceleration;
            else
            {
                Vector3 vel = (transform.position - m_PrevPos) / dt;
                acc = (vel - m_PrevVel) / dt;
                m_PrevVel = vel;
                m_PrevPos = transform.position;
            }
            // przyspieszenie w kierunku prostopadłym do osi zginania wychyla segmenty w przeciwną stronę
            Vector3 bendDir = Vector3.Cross(Vector3.up, transform.TransformDirection(bendAxis));
            float push = Vector3.Dot(acc, bendDir) + acc.y * verticalInfluence;

            for (int i = 0; i < segments.Length; i++)
            {
                if (rigid)
                {
                    m_Angle[i] = 0f;
                    m_Vel[i] = 0f;
                }
                else
                {
                    float target = i == 0 ? 0f : m_Angle[i - 1] * coupling;
                    float force = stiffness * (target - m_Angle[i]) - damping * m_Vel[i] + (i == 0 ? push * inertia : 0f);
                    m_Vel[i] += force * dt;
                    m_Angle[i] = Mathf.Clamp(m_Angle[i] + m_Vel[i] * dt, -75f, 75f);
                }
                segments[i].localRotation = m_Rest[i] * Quaternion.AngleAxis(m_Angle[i], bendAxis);
            }
        }
    }
}
