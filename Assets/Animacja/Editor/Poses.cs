using System;
using System.Collections.Generic;
using UnityEngine;

namespace Animacja.EditorTools
{
    /// <summary>
    /// Poza robota: kąty Eulera stawów (w stopniach), przesunięcie bioder, skala tułowia, root motion.
    /// Konwencje (robot patrzy w +Z): ramię/udo "do przodu" = ujemny kąt X, kolano zginane = dodatni X,
    /// łokieć zginany do przodu = ujemny X, pochylenie tułowia/głowy do przodu = dodatni X.
    /// </summary>
    public class Pose
    {
        public readonly Dictionary<string, Vector3> rot = new Dictionary<string, Vector3>();
        public Vector3 hips;
        public Vector3 torsoScale = Vector3.one;
        public bool hasRootMotion;
        public Vector3 rootPosition;
        public float rootYaw;

        public Vector3 Get(string j) { Vector3 v; return rot.TryGetValue(j, out v) ? v : Vector3.zero; }
        public void Set(string j, Vector3 e) { rot[j] = e; }
        public void Add(string j, Vector3 e) { rot[j] = Get(j) + e; }

        public Pose Arm(bool left, float forward, float outward, float elbow, float twist = 0f)
        {
            Set(left ? J.ArmL : J.ArmR, new Vector3(-forward, twist, left ? -outward : outward));
            Set(left ? J.ForeL : J.ForeR, new Vector3(-elbow, 0f, 0f));
            return this;
        }

        public Pose Leg(bool left, float forward, float knee, float foot = 0f, float outward = 0f)
        {
            Set(left ? J.ThighL : J.ThighR, new Vector3(-forward, 0f, left ? -outward : outward));
            Set(left ? J.ShinL : J.ShinR, new Vector3(knee, 0f, 0f));
            Set(left ? J.FootL : J.FootR, new Vector3(foot, 0f, 0f));
            return this;
        }

        public Pose Torso(float lean, float twist = 0f, float side = 0f)
        {
            Set(J.Torso, new Vector3(lean, twist, side));
            return this;
        }

        public Pose Head(float nod, float turn = 0f, float tilt = 0f)
        {
            Set(J.Head, new Vector3(nod, turn, tilt));
            return this;
        }

        public Pose HipsRot(float x, float y, float z)
        {
            Set(J.Hips, new Vector3(x, y, z));
            return this;
        }

        public static Pose Lerp(Pose a, Pose b, float t)
        {
            var p = new Pose();
            foreach (var j in J.All)
                p.Set(j, Vector3.Lerp(a.Get(j), b.Get(j), t));
            p.hips = Vector3.Lerp(a.hips, b.hips, t);
            p.torsoScale = Vector3.Lerp(a.torsoScale, b.torsoScale, t);
            p.hasRootMotion = a.hasRootMotion || b.hasRootMotion;
            p.rootPosition = Vector3.Lerp(a.rootPosition, b.rootPosition, t);
            p.rootYaw = Mathf.Lerp(a.rootYaw, b.rootYaw, t);
            return p;
        }
    }

    /// <summary>
    /// Funkcje póz w czasie – "przepisy" na klipy animacji. Generator próbkuje je (30 kl./s) i zapisuje jako krzywe.
    /// </summary>
    public static class Poses
    {
        const float TwoPi = Mathf.PI * 2f;

        /// <summary>Interpolacja po kluczach (czas, wartość, czas, wartość…) z wygładzeniem slow in / slow out.</summary>
        public static float K(float t, params float[] tv)
        {
            if (t <= tv[0]) return tv[1];
            for (int i = 0; i + 3 < tv.Length; i += 2)
            {
                if (t <= tv[i + 2])
                {
                    float u = (t - tv[i]) / Mathf.Max(1e-5f, tv[i + 2] - tv[i]);
                    return Mathf.Lerp(tv[i + 1], tv[i + 3], Mathf.SmoothStep(0f, 1f, u));
                }
            }
            return tv[tv.Length - 1];
        }

        public static Pose Neutral()
        {
            var p = new Pose();
            p.Arm(true, 0f, 6f, 8f).Arm(false, 0f, 6f, 8f).Leg(true, 0f, 0f).Leg(false, 0f, 0f).Torso(0f).Head(0f);
            return p;
        }

        // ------------------------------------------------------------------ cykle ruchu

        public static Pose Idle(float t)
        {
            float s = Mathf.Sin(TwoPi * t / 2f);
            var p = Neutral();
            p.Torso(1.5f * s).Head(-2f * s, 8f * Mathf.Sin(TwoPi * t / 2f + 1f));
            p.Arm(true, 3f * s, 7f, 10f).Arm(false, 3f * s, 7f, 10f);
            p.Leg(true, 0f, 2f).Leg(false, 0f, 2f);
            p.hips = new Vector3(0f, -0.01f + 0.01f * s, 0f);
            return p;
        }

        /// <summary>Chód – cykl 1 s. stiff = bez ruchów rąk, tułowia i głowy (porównanie z zasadą 8).</summary>
        public static Pose Walk(float t, bool stiff = false)
        {
            float ph = TwoPi * t;
            float s = Mathf.Sin(ph), c = Mathf.Cos(ph);
            var p = new Pose();
            p.Leg(true, 28f * s, 8f + 40f * Mathf.Max(0f, c), -6f * s);
            p.Leg(false, -28f * s, 8f + 40f * Mathf.Max(0f, -c), 6f * s);
            if (stiff)
            {
                p.Arm(true, 0f, 6f, 5f).Arm(false, 0f, 6f, 5f).Torso(0f).Head(0f);
            }
            else
            {
                p.Arm(true, -24f * s, 7f, 15f + 12f * Mathf.Max(0f, -s));
                p.Arm(false, 24f * s, 7f, 15f + 12f * Mathf.Max(0f, s));
                p.Torso(4f, 7f * s).Head(-3f + 2f * Mathf.Cos(2f * ph), -5f * s);
                p.HipsRot(0f, -6f * s, 0f);
            }
            p.hips = new Vector3(0f, -0.03f + 0.03f * Mathf.Cos(2f * ph), 0f);
            return p;
        }

        public static Pose Run(float t)
        {
            float ph = TwoPi * t / 0.7f;
            float s = Mathf.Sin(ph), c = Mathf.Cos(ph);
            var p = new Pose();
            p.Leg(true, 45f * s, 15f + 85f * Mathf.Max(0f, c), -10f * s);
            p.Leg(false, -45f * s, 15f + 85f * Mathf.Max(0f, -c), 10f * s);
            p.Arm(true, -45f * s, 10f, 80f).Arm(false, 45f * s, 10f, 80f);
            p.Torso(12f, 10f * s).Head(-8f);
            p.HipsRot(0f, -8f * s, 0f);
            p.hips = new Vector3(0f, -0.06f + 0.05f * Mathf.Cos(2f * ph), 0f);
            return p;
        }

        public static Pose WalkBack(float t) => Walk(1f - Mathf.Repeat(t, 1f));

        /// <summary>Krok w bok (dir = +1 w prawo, −1 w lewo) – cykl 1 s.</summary>
        public static Pose Strafe(float t, float dir)
        {
            float ph = TwoPi * t;
            float s = Mathf.Sin(ph), c = Mathf.Cos(ph);
            var p = new Pose();
            p.Leg(true, 0f, 10f + 30f * Mathf.Max(0f, c), 0f, -14f * s * dir);
            p.Leg(false, 0f, 10f + 30f * Mathf.Max(0f, -c), 0f, 14f * s * dir);
            p.Arm(true, 0f, 10f + 6f * s, 15f).Arm(false, 0f, 10f - 6f * s, 15f);
            p.Torso(3f, 0f, 3f * s * dir).Head(-2f, 10f * dir);
            p.hips = new Vector3(0f, -0.03f + 0.03f * Mathf.Cos(2f * ph), 0f);
            return p;
        }

        /// <summary>Machanie prawą ręką – cykl 1,2 s.</summary>
        public static Pose Wave(float t)
        {
            var p = Neutral();
            float b = Mathf.Sin(TwoPi * t / 1.2f);
            p.Arm(true, 3f * b, 7f, 10f);
            p.hips = new Vector3(0f, -0.01f + 0.01f * b, 0f);
            float w = Mathf.Sin(TwoPi * t / 0.6f);
            p.Set(J.ArmR, new Vector3(-15f, 0f, 150f));
            p.Set(J.ForeR, new Vector3(-10f, 0f, 25f * w));
            p.Head(-4f, 12f, 6f);
            p.Torso(0f, 0f, -3f);
            return p;
        }

        /// <summary>Kiwanie głową i tułowiem – do warstwy Additive (pierwsza klatka = poza odniesienia).</summary>
        public static Pose Nod(float t)
        {
            var p = Neutral();
            float s = Mathf.Sin(TwoPi * t / 0.8f);
            p.Head(18f * s, 0f, 6f * s);
            p.Torso(6f * s);
            return p;
        }

        // ------------------------------------------------------------------ akcje

        /// <summary>
        /// Skok (1,6 s). anticipation 0…1 – przysiad przed wybiciem; exaggeration – skala przesady
        /// (głębokość przysiadu, wysokość, zamach rąk, rozciąganie/zgniatanie tułowia).
        /// </summary>
        public static Pose Jump(float t, float anticipation, float exaggeration)
        {
            float a = anticipation, k = exaggeration;
            float drop;
            if (t < 0.55f) drop = K(t, 0f, 0f, 0.15f, 0f, 0.45f, -0.32f * a * Mathf.Max(0.3f, k), 0.55f, 0f);
            else if (t < 1.0f) drop = 0f;
            else drop = K(t, 1.0f, 0f, 1.1f, -0.25f * Mathf.Max(0.3f, k), 1.45f, 0f);
            float height = (0.35f + 0.45f * k);
            float air = 0f;
            if (t > 0.55f && t < 1.0f)
            {
                float u = (t - 0.55f) / 0.45f;
                air = height * 4f * u * (1f - u);
            }
            float knee = -drop * 230f + (air > 0f ? 25f * Mathf.Min(1.5f, k) * Mathf.Sin((t - 0.55f) / 0.45f * Mathf.PI) : 0f);
            float thigh = knee * 0.5f;
            var p = new Pose();
            p.Leg(true, thigh, knee, -knee * 0.5f).Leg(false, thigh, knee, -knee * 0.5f);
            float lean = -drop * 70f;
            p.Torso(lean).Head(-lean * 0.5f);
            float arms = K(t, 0f, 0f, 0.15f, 0f, 0.45f, -50f * a * Mathf.Min(1.5f, k), 0.62f, 150f * Mathf.Clamp(k, 0.3f, 1.2f),
                1.0f, 60f, 1.1f, 40f, 1.45f, 0f);
            p.Arm(true, arms, 10f, 20f).Arm(false, arms, 10f, 20f);
            p.hips = new Vector3(0f, drop + air, 0f);
            float st = Mathf.Max(0f, k - 0.5f);
            float sy = 1f + K(t, 0f, 0f, 0.5f, 0f, 0.6f, 0.22f * st, 0.8f, 0f, 1.0f, 0f, 1.06f, -0.18f * st, 1.3f, 0f);
            float sxz = 1f / Mathf.Sqrt(sy);
            p.torsoScale = new Vector3(sxz, sy, sxz);
            return p;
        }

        /// <summary>Ciosy kombinacji: 1 – prawa ręka, 2 – lewa ręka, 3 – kopnięcie. Długość 0,6 s.</summary>
        public static Pose Attack(float t, int which)
        {
            var p = Neutral();
            p.Leg(true, 12f, 15f, -6f).Leg(false, -12f, 15f, 6f);
            p.hips = new Vector3(0f, -0.05f, 0f);
            float hit = K(t, 0f, 0f, 0.24f, 1f, 0.36f, 1f, 0.6f, 0f);
            float wind = K(t, 0f, 0f, 0.12f, 1f, 0.24f, 0f);
            switch (which)
            {
                case 1:
                    p.Arm(false, 85f * hit - 20f * wind, 5f, 80f * (1f - hit) + 20f, 0f);
                    p.Arm(true, 40f, 10f, 100f);
                    p.Torso(5f, 25f * wind - 25f * hit);
                    break;
                case 2:
                    p.Arm(true, 85f * hit - 20f * wind, 5f, 80f * (1f - hit) + 20f, 0f);
                    p.Arm(false, 40f, 10f, 100f);
                    p.Torso(5f, -25f * wind + 25f * hit);
                    break;
                default:
                    p.Leg(false, 85f * hit, 70f * (1f - hit) + 10f * wind, 20f * hit);
                    p.Leg(true, 0f, 12f, 0f);
                    p.Arm(true, 30f, 35f, 60f).Arm(false, -20f, 35f, 40f);
                    p.Torso(-10f * hit);
                    break;
            }
            return p;
        }

        // ------------------------------------------------------------------ root motion

        public static Pose WalkRootMotion(float t)
        {
            var p = Walk(t);
            p.hasRootMotion = true;
            p.rootPosition = new Vector3(0f, 0f, 1.4f * t);
            return p;
        }

        public static Pose TurnRootMotion(float t)
        {
            var p = Walk(t);
            p.hasRootMotion = true;
            float theta = 40f * Mathf.Deg2Rad * t;          // 40° na cykl
            const float r = 1.4f / (40f * Mathf.Deg2Rad);   // długość łuku = 1,4 m na cykl
            p.rootPosition = new Vector3(r * (1f - Mathf.Cos(theta)), 0f, r * Mathf.Sin(theta));
            p.rootYaw = 40f * t;
            return p;
        }

        // ------------------------------------------------------------------ pozy kluczowe

        public static Pose[] KeyPoses()
        {
            var p0 = Neutral();
            var p1 = Neutral();
            p1.Arm(true, 165f, 10f, 5f).Arm(false, 165f, 10f, 5f).Torso(-10f).Head(-20f);
            p1.Leg(true, 0f, 0f, 15f).Leg(false, 0f, 0f, 15f);
            p1.hips = new Vector3(0f, 0.06f, 0f);
            var p2 = Neutral();
            p2.Leg(true, 70f, 110f, -40f).Leg(false, 70f, 110f, -40f);
            p2.Arm(true, 70f, 15f, 30f).Arm(false, 70f, 15f, 30f).Torso(30f).Head(-25f);
            p2.hips = new Vector3(0f, -0.38f, 0f);
            var p3 = Neutral();
            p3.Arm(false, 90f, 20f, 0f).Arm(true, -40f, 15f, 30f).Torso(-5f, 25f).Head(0f, 30f, -8f);
            p3.Leg(true, 25f, 15f, -8f).Leg(false, -15f, 5f, 8f);
            return new[] { p0, p1, p2, p3 };
        }

        /// <summary>Poza do pozy: klucze co 1 s, pomiędzy – interpolacja z wygładzeniem; cykl 4 s.</summary>
        public static Pose PoseToPose(float t)
        {
            var keys = KeyPoses();
            float u = Mathf.Repeat(t, 4f);
            int i = Mathf.Min(3, Mathf.FloorToInt(u));
            return Pose.Lerp(keys[i], keys[(i + 1) % 4], Mathf.SmoothStep(0f, 1f, u - i));
        }

        /// <summary>Symetryczna, sztywna poza ("twinning") – zasada 11, przykład negatywny.</summary>
        public static Pose Twinning()
        {
            var p = Neutral();
            p.Arm(true, 30f, 20f, 30f).Arm(false, 30f, 20f, 30f);
            return p;
        }

        /// <summary>Kontrapost – ciężar na jednej nodze, przeciwne pochylenie bioder i barków.</summary>
        public static Pose Contrapposto()
        {
            var p = Neutral();
            p.HipsRot(0f, 8f, -8f);
            p.Leg(false, 2f, 4f, 0f, -6f);
            p.Leg(true, 18f, 28f, -10f, -12f);
            p.Torso(-2f, -12f, 14f);
            p.Head(4f, 20f, -10f);
            p.Arm(false, -10f, 18f, 20f);
            p.Arm(true, 20f, 35f, 95f, 30f);
            p.hips = new Vector3(0.03f, -0.03f, 0f);
            return p;
        }
    }
}
