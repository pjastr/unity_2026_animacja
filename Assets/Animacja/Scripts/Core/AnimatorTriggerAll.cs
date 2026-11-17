using UnityEngine;

namespace Animacja
{
    /// <summary>Spacja – ustawia ten sam trigger we wszystkich podanych Animatorach (porównanie wariantów animacji).</summary>
    public class AnimatorTriggerAll : DemoActionBase
    {
        public Animator[] animators;
        public string trigger = "Akcja";

        public override void OnMainAction()
        {
            foreach (var a in animators)
                if (a != null) a.SetTrigger(trigger);
            DemoHud.Log($"SetTrigger(\"{trigger}\") × {animators.Length}");
        }
    }
}
