using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Scena 16: odbiorca zdarzeń animacji. Zdarzenie z klipu wywołuje metodę o podanej nazwie
    /// na komponentach obiektu, na którym jest Animator (tu: korzeń robota).
    /// </summary>
    public class AnimationEventReceiver : MonoBehaviour
    {
        public Transform leftFoot;
        public Transform rightFoot;
        public Transform rightHand;
        public Material dustMaterial;
        public Renderer hitTarget;
        public Material hitMaterial;

        Material m_TargetDefault;
        float m_HitTimer;
        int m_Steps;

        void Start()
        {
            if (hitTarget != null) m_TargetDefault = hitTarget.sharedMaterial;
        }

        /// <summary>Wywoływane przez AnimationEvent "OnFootstep" (intParameter: 0 = lewa, 1 = prawa).</summary>
        public void OnFootstep(int foot)
        {
            m_Steps++;
            Transform f = foot == 0 ? leftFoot : rightFoot;
            if (f != null) DustPuff.Spawn(f.position, dustMaterial);
            DemoHud.Log($"OnFootstep({foot}) – {(foot == 0 ? "lewa" : "prawa")} stopa, krok {m_Steps}");
        }

        /// <summary>Wywoływane przez AnimationEvent "OnHit" w klipie ataku.</summary>
        public void OnHit(string what)
        {
            m_HitTimer = 0.3f;
            if (rightHand != null) DustPuff.Spawn(rightHand.position, dustMaterial, 0.8f);
            DemoHud.Log($"OnHit(\"{what}\") – w klatce, w której dłoń jest najdalej");
        }

        void Update()
        {
            if (hitTarget == null) return;
            m_HitTimer -= Time.deltaTime;
            hitTarget.sharedMaterial = m_HitTimer > 0f ? hitMaterial : m_TargetDefault;
        }
    }
}
