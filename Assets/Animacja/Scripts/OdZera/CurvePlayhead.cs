using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Scena 13: znacznik na wykresie krzywej – pokazuje bieżący czas klipu (Animation, legacy)
    /// i wartość krzywej w tej chwili. Krzywa jest kopią tej samej AnimationCurve, z której zbudowano klip.
    /// </summary>
    public class CurvePlayhead : MonoBehaviour
    {
        public Animation anim;
        public string clipName;
        public AnimationCurve curve;
        public Transform marker;
        public Vector3 graphOrigin;
        public float timeScale = 1f;   // jednostki świata na sekundę
        public float valueScale = 1f;  // jednostki świata na jednostkę wartości
        public string tangentName;

        WorldLabel m_Label;

        void Start() => m_Label = GetComponent<WorldLabel>();

        void Update()
        {
            if (anim == null || curve == null || marker == null) return;
            var st = anim[clipName];
            if (st == null) return;
            float len = st.length;
            float t = len > 0f ? Mathf.Repeat(st.time, len) : 0f;
            float v = curve.Evaluate(t);
            marker.position = graphOrigin + new Vector3(t * timeScale, v * valueScale, 0f);
            if (m_Label != null)
                m_Label.extra = $"t = {t:0.00} s   y = {v:0.00}";
        }
    }
}
