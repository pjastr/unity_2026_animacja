using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// StateMachineBehaviour – skrypt podpięty do STANU w Animator Controllerze (a nie do GameObjectu).
    /// Unity wywołuje jego metody przy wejściu, w trakcie i przy wyjściu ze stanu.
    /// </summary>
    public class StateLogger : StateMachineBehaviour
    {
        public string label = "stan";

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            DemoHud.Log($"<color=#7CFC7C>OnStateEnter</color> {label}");
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            DemoHud.Log($"<color=#FF9F7B>OnStateExit</color> {label}");
        }
    }
}
