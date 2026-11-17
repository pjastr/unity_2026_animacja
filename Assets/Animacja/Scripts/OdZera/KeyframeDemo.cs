using UnityEngine;

namespace Animacja
{
    /// <summary>Scena 13: Spacja – od początku, LPM – zwolnione tempo, PPM – pauza.</summary>
    public class KeyframeDemo : DemoActionBase
    {
        public Animation[] animations;
        bool m_Paused;

        public override void OnMainAction()
        {
            foreach (var a in animations)
            {
                if (a == null) continue;
                a.Stop();
                a.Play();
            }
            DemoHud.Log("Animation.Play() – od początku");
        }

        public override void OnSecondaryAction()
        {
            Time.timeScale = Time.timeScale < 0.99f ? 1f : 0.25f;
            DemoHud.Log("Time.timeScale = " + Time.timeScale);
        }

        public override void OnTertiaryAction()
        {
            m_Paused = !m_Paused;
            foreach (var a in animations)
                if (a != null)
                    foreach (AnimationState s in a) s.speed = m_Paused ? 0f : 1f;
            DemoHud.Log(m_Paused ? "AnimationState.speed = 0 (pauza)" : "AnimationState.speed = 1");
        }

        void OnDestroy() => Time.timeScale = 1f;
    }
}
