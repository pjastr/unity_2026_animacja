using UnityEngine;

namespace Animacja
{
    /// <summary>Funkcje wygładzania (easing) – t ∈ [0, 1] → [0, 1].</summary>
    public static class Ease
    {
        public enum Type { Linear, InQuad, OutQuad, InOutQuad, InOutCubic, OutBack, OutBounce, Step }

        public static float Evaluate(Type type, float t)
        {
            t = Mathf.Clamp01(t);
            switch (type)
            {
                case Type.InQuad: return t * t;
                case Type.OutQuad: return 1f - (1f - t) * (1f - t);
                case Type.InOutQuad: return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
                case Type.InOutCubic: return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
                case Type.OutBack:
                {
                    const float c1 = 1.70158f, c3 = c1 + 1f;
                    return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                }
                case Type.OutBounce:
                {
                    const float n1 = 7.5625f, d1 = 2.75f;
                    if (t < 1f / d1) return n1 * t * t;
                    if (t < 2f / d1) { t -= 1.5f / d1; return n1 * t * t + 0.75f; }
                    if (t < 2.5f / d1) { t -= 2.25f / d1; return n1 * t * t + 0.9375f; }
                    t -= 2.625f / d1; return n1 * t * t + 0.984375f;
                }
                case Type.Step: return t < 1f ? 0f : 1f;
                default: return t;
            }
        }

        /// <summary>Ruch tam i z powrotem: 0 → 1 → 0 w czasie period, z przerwą hold na końcach.</summary>
        public static float PingPong01(float time, float travel, float hold)
        {
            float cycle = 2f * (travel + hold);
            float u = Mathf.Repeat(time, cycle);
            if (u < travel) return u / travel;
            if (u < travel + hold) return 1f;
            if (u < 2f * travel + hold) return 1f - (u - travel - hold) / travel;
            return 0f;
        }
    }
}
