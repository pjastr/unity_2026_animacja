using System.Text;
using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Scena 21: podmaszyna stanów "Walka" (Atak1 → Atak2 → Atak3) i StateMachineBehaviour.
    /// Spacja – atak (trigger Atak; wciśnij ponownie w trakcie ciosu, by przejść do kolejnego), LPM – chód on/off.
    /// </summary>
    public class ComboDemo : DemoActionBase
    {
        public Animator animator;
        public string[] stateNames = { "Idle", "Chod", "Atak1", "Atak2", "Atak3" };

        public override void OnMainAction()
        {
            animator.SetTrigger("Atak");
            DemoHud.Log("SetTrigger(\"Atak\")");
        }

        public override void OnSecondaryAction()
        {
            bool w = !animator.GetBool("Chodzi");
            animator.SetBool("Chodzi", w);
            DemoHud.Log($"SetBool(\"Chodzi\", {w})");
        }

        void Update()
        {
            var info = animator.GetCurrentAnimatorStateInfo(0);
            var sb = new StringBuilder();
            sb.Append(AnimatorInfo.Describe(animator, stateNames)).Append("\n");
            sb.Append("tag stanu: ").Append(info.IsTag("Atak") ? "<b>Atak</b>" : "-").Append("\n\n<b>Parametry</b>\n").Append(AnimatorInfo.Parameters(animator));
            sb.Append("\nKombinacja: Atak1 → (Atak w 2. połowie ciosu) → Atak2 → Atak3\nKażdy cios kończy się wyjściem z podmaszyny (Exit) do Idle / Chod.");
            DemoHud.SetStatus("combo", sb.ToString());
        }
    }
}
