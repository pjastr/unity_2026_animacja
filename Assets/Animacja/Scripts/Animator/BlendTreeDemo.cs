using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Scena 19: Blend Tree 1D (Speed) i 2D Simple Directional (MoveX, MoveY).
    /// W / S – prędkość robota 1D, strzałki/WASD – kierunek robota 2D (ten sam Move), Spacja – automatyczna zmiana prędkości.
    /// Panel pokazuje wagi klipów (GetCurrentAnimatorClipInfo).
    /// </summary>
    public class BlendTreeDemo : DemoActionBase
    {
        public Animator blend1D;
        public Animator blend2D;
        public float maxSpeed = 4f;
        bool m_Auto = true;
        float m_Speed;
        Vector2 m_Dir;
        readonly List<AnimatorClipInfo> m_Clips = new List<AnimatorClipInfo>();

        public override void OnMainAction()
        {
            m_Auto = !m_Auto;
            DemoHud.Log(m_Auto ? "Prędkość automatyczna" : "Prędkość z klawiatury (W / S)");
        }

        void Update()
        {
            Vector2 mv = DemoInput.ReadVector2(DemoInput.Move);
            if (m_Auto) m_Speed = (Mathf.Sin(Time.time * 0.5f) * 0.5f + 0.5f) * maxSpeed;
            else m_Speed = Mathf.Clamp(m_Speed + mv.y * 2.5f * Time.deltaTime, 0f, maxSpeed);
            blend1D.SetFloat("Speed", m_Speed);

            m_Dir = Vector2.MoveTowards(m_Dir, mv, 3f * Time.deltaTime);
            blend2D.SetFloat("MoveX", m_Dir.x);
            blend2D.SetFloat("MoveY", m_Dir.y);

            var sb = new StringBuilder();
            sb.Append("<b>1D</b> – Speed = ").Append(m_Speed.ToString("0.00")).Append(m_Auto ? " (auto)" : " (W/S)").Append('\n');
            sb.Append("   progi: Idle 0, Walk 1.5, Run 4\n");
            AppendWeights(sb, blend1D);
            sb.Append("\n<b>2D Simple Directional</b> – MoveX = ").Append(m_Dir.x.ToString("0.00"))
              .Append(", MoveY = ").Append(m_Dir.y.ToString("0.00")).Append('\n');
            sb.Append("   pozycje: Idle (0,0), Walk (0,1), WalkBack (0,−1), StrafeL (−1,0), StrafeR (1,0)\n");
            AppendWeights(sb, blend2D);
            DemoHud.SetStatus("blend", sb.ToString());
        }

        void AppendWeights(StringBuilder sb, Animator a)
        {
            a.GetCurrentAnimatorClipInfo(0, m_Clips);
            foreach (var c in m_Clips)
            {
                int bars = Mathf.RoundToInt(c.weight * 20f);
                sb.Append("   ").Append(c.clip.name).Append(": ").Append(c.weight.ToString("0.00")).Append("  ")
                  .Append(new string('|', bars)).Append('\n');
            }
        }
    }
}
