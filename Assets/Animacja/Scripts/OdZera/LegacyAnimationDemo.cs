using System.Text;
using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Scena 15: komponent Animation (legacy) – tryby zawijania (WrapMode) – oraz Animator dla porównania.
    /// Spacja – Play od początku, LPM – odtwarzanie wstecz (speed = −1), PPM – CrossFade na ostatnim sześcianie.
    /// </summary>
    public class LegacyAnimationDemo : DemoActionBase
    {
        public Animation[] legacy;
        public WrapMode[] modes;
        public string clipName;
        public Animation crossFadeTarget;
        public string crossFadeA;
        public string crossFadeB;
        public Animator animator;

        bool m_Reverse;
        bool m_B;

        void Start()
        {
            for (int i = 0; i < legacy.Length; i++)
            {
                var st = legacy[i][clipName];
                st.wrapMode = modes[i];
                legacy[i].Play(clipName);
            }
        }

        public override void OnMainAction()
        {
            m_Reverse = false;
            for (int i = 0; i < legacy.Length; i++)
            {
                var st = legacy[i][clipName];
                st.speed = 1f;
                st.time = 0f;
                legacy[i].Play(clipName);
            }
            if (animator != null) animator.Play(0, 0, 0f);
            DemoHud.Log("Animation.Play(\"" + clipName + "\") – od początku");
        }

        public override void OnSecondaryAction()
        {
            m_Reverse = !m_Reverse;
            foreach (var a in legacy)
            {
                var st = a[clipName];
                st.speed = m_Reverse ? -1f : 1f;
                if (!a.isPlaying) a.Play(clipName);
            }
            DemoHud.Log(m_Reverse ? "AnimationState.speed = −1 (wstecz)" : "AnimationState.speed = 1");
        }

        public override void OnTertiaryAction()
        {
            if (crossFadeTarget == null) return;
            m_B = !m_B;
            crossFadeTarget.CrossFade(m_B ? crossFadeB : crossFadeA, 0.6f);
            DemoHud.Log($"Animation.CrossFade(\"{(m_B ? crossFadeB : crossFadeA)}\", 0.6f)");
        }

        void Update()
        {
            var sb = new StringBuilder("<b>Animation (legacy)</b> – ten sam klip, różne WrapMode\n");
            for (int i = 0; i < legacy.Length; i++)
            {
                var st = legacy[i][clipName];
                sb.Append("   ").Append(modes[i]).Append(": time = ").Append(st.time.ToString("0.00"))
                  .Append(" s, normalizedTime = ").Append(st.normalizedTime.ToString("0.00"))
                  .Append(", isPlaying = ").Append(legacy[i].isPlaying).Append('\n');
            }
            if (crossFadeTarget != null)
            {
                sb.Append("\n<b>CrossFade</b> – wagi stanów:\n");
                foreach (AnimationState s in crossFadeTarget)
                    sb.Append("   ").Append(s.name).Append(": weight = ").Append(s.weight.ToString("0.00")).Append('\n');
            }
            if (animator != null)
            {
                var info = animator.GetCurrentAnimatorStateInfo(0);
                sb.Append("\n<b>Animator</b> (klip z Loop Time): normalizedTime = ").Append(info.normalizedTime.ToString("0.00"));
            }
            DemoHud.SetStatus("legacy", sb.ToString());
        }
    }
}
