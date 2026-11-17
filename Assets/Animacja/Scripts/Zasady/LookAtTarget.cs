using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Zasada 8 – działanie drugoplanowe: głowa śledzi cel PO animacji (LateUpdate nadpisuje pozę z Animatora).
    /// Ograniczenie kąta i wygładzenie, żeby ruch był naturalny.
    /// </summary>
    public class LookAtTarget : MonoBehaviour
    {
        public Transform head;
        public Transform target;
        [Range(0f, 1f)] public float weight = 1f;
        public float maxAngle = 70f;
        public float smoothing = 6f;

        Quaternion m_Current = Quaternion.identity;
        bool m_Init;

        void LateUpdate()
        {
            if (head == null || target == null) return;
            Quaternion animated = head.rotation;   // poza ustawiona przez Animator w tej klatce
            Vector3 dir = target.position - head.position;
            if (dir.sqrMagnitude < 1e-4f) return;
            Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
            Quaternion body = head.parent != null ? head.parent.rotation : Quaternion.identity;
            float angle = Quaternion.Angle(body, look);
            if (angle > maxAngle) look = Quaternion.Slerp(body, look, maxAngle / angle);
            Quaternion wanted = Quaternion.Slerp(animated, look, weight);
            if (!m_Init) { m_Current = wanted; m_Init = true; }
            m_Current = Quaternion.Slerp(m_Current, wanted, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
            head.rotation = m_Current;
        }
    }
}
