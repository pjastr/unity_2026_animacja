using System.Text;
using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Scena 22: Root Motion. Spacja – przywróć pozycje, LPM – Animator.speed 1 ↔ 0,5 (ruch też zwalnia),
    /// PPM – Apply Root Motion wł./wył. na robocie 2.
    /// </summary>
    public class RootMotionDemo : DemoActionBase
    {
        public Animator[] robots;
        public float laneLength = 14f;

        Vector3[] m_Start;
        Quaternion[] m_StartRot;
        float m_Speed = 1f;

        void Start()
        {
            m_Start = new Vector3[robots.Length];
            m_StartRot = new Quaternion[robots.Length];
            for (int i = 0; i < robots.Length; i++)
            {
                m_Start[i] = robots[i].transform.position;
                m_StartRot[i] = robots[i].transform.rotation;
            }
        }

        public override void OnMainAction()
        {
            for (int i = 0; i < robots.Length; i++)
                robots[i].transform.SetPositionAndRotation(m_Start[i], m_StartRot[i]);
            DemoHud.Log("Pozycje przywrócone");
        }

        public override void OnSecondaryAction()
        {
            m_Speed = m_Speed > 0.99f ? 0.5f : 1f;
            foreach (var r in robots) r.speed = m_Speed;
            DemoHud.Log("Animator.speed = " + m_Speed);
        }

        public override void OnTertiaryAction()
        {
            if (robots.Length < 2) return;
            robots[1].applyRootMotion = !robots[1].applyRootMotion;
            DemoHud.Log("Robot 2: applyRootMotion = " + robots[1].applyRootMotion);
        }

        void Update()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < robots.Length; i++)
            {
                var r = robots[i];
                // pas ruchu: po przejściu laneLength wracamy na start (tylko roboty idące prosto)
                if (i < 3 && Vector3.Distance(r.transform.position, m_Start[i]) > laneLength)
                    r.transform.SetPositionAndRotation(m_Start[i], m_StartRot[i]);
                var custom = r.GetComponent<CustomRootMotion>();
                sb.Append("<b>").Append(i + 1).Append(". ").Append(r.name).Append("</b>\n   applyRootMotion = ").Append(r.applyRootMotion)
                  .Append(custom != null ? "  (OnAnimatorMove ×" + custom.multiplier + ")" : "")
                  .Append("\n   pozycja = ").Append(r.transform.position.ToString("0.0"))
                  .Append("\n   deltaPosition (klatka) = ").Append(r.deltaPosition.ToString("0.000")).Append('\n');
            }
            DemoHud.SetStatus("rm", sb.ToString());
        }
    }
}
