using UnityEngine;

namespace Animacja
{
    /// <summary>Scena 16: Spacja – chód ↔ bieg (parametr Bieg), LPM – atak (trigger Atak).</summary>
    public class EventsDemo : DemoActionBase
    {
        public Animator animator;
        bool m_Run;

        public override void OnMainAction()
        {
            m_Run = !m_Run;
            animator.SetBool("Bieg", m_Run);
            DemoHud.Log($"SetBool(\"Bieg\", {m_Run})");
        }

        public override void OnSecondaryAction()
        {
            animator.SetTrigger("Atak");
            DemoHud.Log("SetTrigger(\"Atak\")");
        }

        void Update()
        {
            var info = animator.GetCurrentAnimatorStateInfo(0);
            string name = info.IsName("Chod") ? "Chod" : info.IsName("Bieg") ? "Bieg" : info.IsName("Atak") ? "Atak" : "?";
            DemoHud.SetStatus("ev", $"<b>Stan</b>: {name}   normalizedTime = {info.normalizedTime:0.00}\n" +
                                    "Zdarzenia w klipach:\n   Chod_zdarzenia: OnFootstep(0) @ 25%, OnFootstep(1) @ 75%\n" +
                                    "   Bieg_zdarzenia: OnFootstep(0) @ 25%, OnFootstep(1) @ 75%\n   Atak_zdarzenia: OnHit(\"cios\") @ 40%");
        }
    }
}
