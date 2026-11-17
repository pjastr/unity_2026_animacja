using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Zasada 5: postać rusza (przyspiesza), jedzie i GWAŁTOWNIE się zatrzymuje, potem wraca.
    /// Udostępnia przyspieszenie, z którego korzysta SpringChain (części luźne).
    /// </summary>
    public class MoveStopCycle : DemoActionBase
    {
        public float distance = 6f;
        public float travelTime = 1.2f;
        public float pause = 1.6f;

        public Vector3 Acceleration { get; private set; }

        Vector3 m_Start;
        Vector3 m_PrevPos;
        Vector3 m_PrevVel;
        float m_Time;

        void Start()
        {
            m_Start = transform.position;
            m_PrevPos = m_Start;
        }

        public override void OnMainAction() => m_Time = 0f;

        void Update()
        {
            m_Time += Time.deltaTime;
            float cycle = 2f * (travelTime + pause);
            float u = Mathf.Repeat(m_Time, cycle);
            float x;
            if (u < travelTime) x = Ease.Evaluate(Ease.Type.InQuad, u / travelTime);          // przyspiesza i staje w miejscu
            else if (u < travelTime + pause) x = 1f;
            else if (u < 2f * travelTime + pause) x = 1f - Ease.Evaluate(Ease.Type.InQuad, (u - travelTime - pause) / travelTime);
            else x = 0f;
            transform.position = m_Start + Vector3.right * distance * x;

            float dt = Mathf.Max(Time.deltaTime, 1e-4f);
            Vector3 vel = (transform.position - m_PrevPos) / dt;
            Acceleration = (vel - m_PrevVel) / dt;
            m_PrevVel = vel;
            m_PrevPos = transform.position;
        }
    }
}
