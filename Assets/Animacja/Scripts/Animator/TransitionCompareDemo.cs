using System.Text;
using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Scena 18: te same stany, różne ustawienia przejścia (czas trwania, Has Exit Time, Interruption Source).
    /// Spacja – trigger Akcja we wszystkich, LPM – drugi trigger w trakcie przejścia (test przerwania).
    /// </summary>
    public class TransitionCompareDemo : DemoActionBase
    {
        public Animator[] animators;
        [TextArea(1, 3)] public string[] descriptions;
        public string[] stateNames = { "Idle", "Wave", "Jump" };

        public override void OnMainAction()
        {
            foreach (var a in animators) a.SetTrigger("Akcja");
            DemoHud.Log("SetTrigger(\"Akcja\") – Idle → Wave");
        }

        public override void OnSecondaryAction()
        {
            foreach (var a in animators) a.SetTrigger("Skok");
            DemoHud.Log("SetTrigger(\"Skok\") – przejście do Jump (z Any State)");
        }

        void Update()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < animators.Length; i++)
            {
                sb.Append("<b>").Append(i + 1).Append(". ").Append(descriptions[i]).Append("</b>\n   ")
                  .Append(AnimatorInfo.Describe(animators[i], stateNames)).Append("\n\n");
            }
            DemoHud.SetStatus("tr", sb.ToString());
        }
    }
}
