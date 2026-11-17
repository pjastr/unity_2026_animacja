using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Zasada 6 (i 9): ruch A → B → A z wybraną funkcją wygładzania.
    /// Linear – stała prędkość (mechanicznie), InOut – zwolnienie przy starcie i końcu (slow in / slow out).
    /// stepFps > 0 – pozycja zmienia się skokowo (np. 12 kl./s – animacja "na dwójkach").
    /// </summary>
    public class EaseLane : DemoActionBase
    {
        public Vector3 from;
        public Vector3 to;
        public Ease.Type ease = Ease.Type.InOutCubic;
        public float travelTime = 1.5f;
        public float hold = 0.6f;
        public float stepFps;

        float m_Time;
        float m_StepTimer;

        public override void OnMainAction() { m_Time = 0f; }

        void Update()
        {
            m_Time += Time.deltaTime;
            if (stepFps > 0f)
            {
                m_StepTimer += Time.deltaTime;
                if (m_StepTimer < 1f / stepFps) return;
                m_StepTimer %= 1f / stepFps;
            }
            float cycle = 2f * (travelTime + hold);
            float u = Mathf.Repeat(m_Time, cycle);
            Vector3 p;
            if (u < travelTime) p = Vector3.LerpUnclamped(from, to, Ease.Evaluate(ease, u / travelTime));
            else if (u < travelTime + hold) p = to;
            else if (u < 2f * travelTime + hold) p = Vector3.LerpUnclamped(to, from, Ease.Evaluate(ease, (u - travelTime - hold) / travelTime));
            else p = from;
            transform.position = p;
        }
    }
}
