using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Scena 22: własna obsługa root motion. Gdy skrypt ma metodę OnAnimatorMove, Animator NIE przesuwa obiektu sam –
    /// dostajemy deltaPosition / deltaRotation z klipu i decydujemy, co z nimi zrobić (np. CharacterController.Move).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class CustomRootMotion : MonoBehaviour
    {
        public float multiplier = 2f;
        Animator m_Animator;
        public Vector3 lastDelta;

        void Awake() => m_Animator = GetComponent<Animator>();

        void OnAnimatorMove()
        {
            lastDelta = m_Animator.deltaPosition;
            transform.position += m_Animator.deltaPosition * multiplier;
            transform.rotation = m_Animator.deltaRotation * transform.rotation;
        }
    }
}
