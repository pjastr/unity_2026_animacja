using System.Text;
using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Scena 14: jeden klip animuje wiele właściwości różnych obiektów w hierarchii.
    /// Spacja – pauza (Animator.speed = 0), LPM – zwolnione tempo (Animator.speed = 0,25), PPM – od początku.
    /// </summary>
    public class PropertyClipDemo : DemoActionBase
    {
        public Animator animator;
        public string stateName = "Pokaz";
        [Tooltip("Opis ścieżek (path) i właściwości zapisanych w klipie – wypełnia generator")]
        [TextArea(3, 20)] public string[] bindings;

        float m_Speed = 1f;
        bool m_Paused;

        public override void OnMainAction()
        {
            m_Paused = !m_Paused;
            Apply();
            DemoHud.Log(m_Paused ? "animator.speed = 0" : "animator.speed = " + m_Speed);
        }

        public override void OnSecondaryAction()
        {
            m_Speed = m_Speed > 0.99f ? 0.25f : 1f;
            Apply();
            DemoHud.Log("animator.speed = " + m_Speed);
        }

        public override void OnTertiaryAction()
        {
            animator.Play(stateName, 0, 0f);
            DemoHud.Log($"animator.Play(\"{stateName}\", 0, 0f)");
        }

        void Apply() => animator.speed = m_Paused ? 0f : m_Speed;

        void Update()
        {
            var info = animator.GetCurrentAnimatorStateInfo(0);
            var sb = new StringBuilder();
            sb.Append("<b>Klip</b>: czas ").Append((Mathf.Repeat(info.normalizedTime, 1f) * info.length).ToString("0.00"))
              .Append(" / ").Append(info.length.ToString("0.00")).Append(" s   speed = ").Append(animator.speed).Append("\n\n");
            sb.Append("<b>Krzywe w klipie</b> (ścieżka : komponent.właściwość)\n");
            if (bindings != null)
                foreach (var b in bindings) sb.Append(b).Append('\n');
            DemoHud.SetStatus("props", sb.ToString());
        }
    }
}
