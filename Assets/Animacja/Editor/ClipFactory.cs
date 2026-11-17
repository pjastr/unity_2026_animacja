using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Animacja.EditorTools
{
    /// <summary>
    /// Tworzenie klipów animacji kodem – to samo, co robi okno Animation przy nagrywaniu klatek kluczowych:
    /// AnimationClip + krzywe (EditorCurveBinding: ścieżka, typ komponentu, nazwa właściwości) + ustawienia klipu.
    /// </summary>
    public static class ClipFactory
    {
        public const string ClipDir = SceneBuilder.DataDir + "/Klipy";
        public const string ControllerDir = SceneBuilder.DataDir + "/Kontrolery";
        public const string MaskDir = SceneBuilder.DataDir + "/Maski";
        const int Fps = 30;

        public static void EnsureDirs()
        {
            SceneBuilder.EnsureFolder(ClipDir);
            SceneBuilder.EnsureFolder(ControllerDir);
            SceneBuilder.EnsureFolder(MaskDir);
        }

        /// <summary>Zwraca istniejący klip (zachowuje GUID) albo tworzy nowy; czyści krzywe i zdarzenia.</summary>
        public static AnimationClip GetOrCreate(string name, bool legacy = false)
        {
            string path = $"{ClipDir}/{name}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip { name = name };
                AssetDatabase.CreateAsset(clip, path);
            }
            clip.ClearCurves();
            clip.legacy = legacy;
            clip.frameRate = Fps;
            AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);
            return clip;
        }

        public static void SetLoop(AnimationClip clip, bool loop)
        {
            if (clip.legacy)
            {
                clip.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
                return;
            }
            var s = AnimationUtility.GetAnimationClipSettings(clip);
            s.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, s);
        }

        static AnimationCurve Smooth(List<Keyframe> keys)
        {
            var c = new AnimationCurve(keys.ToArray());
            for (int i = 0; i < c.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(c, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(c, i, AnimationUtility.TangentMode.ClampedAuto);
            }
            return c;
        }

        /// <summary>
        /// Klip robota z funkcji pozy: próbkowanie co 1/30 s, krzywe rotacji (kwaternion) wszystkich stawów,
        /// pozycji bioder, skali tułowia i opcjonalnie root motion (MotionT / MotionQ).
        /// </summary>
        public static AnimationClip Robot(string name, float length, bool loop, Func<float, Pose> fn, bool legacy = false, string prefix = "")
        {
            var clip = GetOrCreate(name, legacy);
            AddRobotCurves(clip, length, fn, prefix);
            SetLoop(clip, loop);
            clip.EnsureQuaternionContinuity();
            EditorUtility.SetDirty(clip);
            return clip;
        }

        public static void AddRobotCurves(AnimationClip clip, float length, Func<float, Pose> fn, string prefix = "")
        {
            int n = Mathf.Max(1, Mathf.RoundToInt(length * Fps));
            var poses = new Pose[n + 1];
            var times = new float[n + 1];
            for (int i = 0; i <= n; i++)
            {
                times[i] = i == n ? length : i / (float)Fps;
                poses[i] = fn(times[i]);
            }

            var bindings = new List<EditorCurveBinding>();
            var curves = new List<AnimationCurve>();
            string[] comp = { "x", "y", "z", "w" };

            foreach (var j in J.All)
            {
                var keys = new List<Keyframe>[4];
                for (int c = 0; c < 4; c++) keys[c] = new List<Keyframe>();
                for (int i = 0; i <= n; i++)
                {
                    Quaternion q = Quaternion.Euler(poses[i].Get(j));
                    keys[0].Add(new Keyframe(times[i], q.x));
                    keys[1].Add(new Keyframe(times[i], q.y));
                    keys[2].Add(new Keyframe(times[i], q.z));
                    keys[3].Add(new Keyframe(times[i], q.w));
                }
                for (int c = 0; c < 4; c++)
                {
                    bindings.Add(EditorCurveBinding.FloatCurve(prefix + J.Path[j], typeof(Transform), "m_LocalRotation." + comp[c]));
                    curves.Add(Smooth(keys[c]));
                }
            }

            AddVector(bindings, curves, prefix + J.Path[J.Hips], typeof(Transform), "m_LocalPosition", times, i => J.Rest[J.Hips] + poses[i].hips);
            AddVector(bindings, curves, prefix + J.Path[J.Torso], typeof(Transform), "m_LocalScale", times, i => poses[i].torsoScale);

            if (poses[0].hasRootMotion)
            {
                // krzywe root motion dla obiektu bez Avatara (to samo daje "Generate Root Motion Curves")
                AddVector(bindings, curves, "", typeof(Animator), "MotionT", times, i => poses[i].rootPosition);
                var qk = new List<Keyframe>[4];
                for (int c = 0; c < 4; c++) qk[c] = new List<Keyframe>();
                for (int i = 0; i <= n; i++)
                {
                    Quaternion q = Quaternion.Euler(0f, poses[i].rootYaw, 0f);
                    qk[0].Add(new Keyframe(times[i], q.x));
                    qk[1].Add(new Keyframe(times[i], q.y));
                    qk[2].Add(new Keyframe(times[i], q.z));
                    qk[3].Add(new Keyframe(times[i], q.w));
                }
                for (int c = 0; c < 4; c++)
                {
                    bindings.Add(EditorCurveBinding.FloatCurve("", typeof(Animator), "MotionQ." + comp[c]));
                    curves.Add(Smooth(qk[c]));
                }
            }

            AnimationUtility.SetEditorCurves(clip, bindings.ToArray(), curves.ToArray());
        }

        static void AddVector(List<EditorCurveBinding> bindings, List<AnimationCurve> curves, string path, Type type, string prop,
            float[] times, Func<int, Vector3> value)
        {
            var kx = new List<Keyframe>();
            var ky = new List<Keyframe>();
            var kz = new List<Keyframe>();
            for (int i = 0; i < times.Length; i++)
            {
                Vector3 v = value(i);
                kx.Add(new Keyframe(times[i], v.x));
                ky.Add(new Keyframe(times[i], v.y));
                kz.Add(new Keyframe(times[i], v.z));
            }
            bindings.Add(EditorCurveBinding.FloatCurve(path, type, prop + ".x")); curves.Add(Smooth(kx));
            bindings.Add(EditorCurveBinding.FloatCurve(path, type, prop + ".y")); curves.Add(Smooth(ky));
            bindings.Add(EditorCurveBinding.FloatCurve(path, type, prop + ".z")); curves.Add(Smooth(kz));
        }

        /// <summary>Krzywe obrotu (kwaternion m_LocalRotation) próbkowane z funkcji kątów Eulera.</summary>
        public static void RotationCurves(AnimationClip clip, string path, float length, Func<float, Vector3> euler)
        {
            int n = Mathf.Max(1, Mathf.RoundToInt(length * Fps));
            var k = new List<Keyframe>[4];
            for (int c = 0; c < 4; c++) k[c] = new List<Keyframe>();
            for (int i = 0; i <= n; i++)
            {
                float t = i == n ? length : i / (float)Fps;
                Quaternion q = Quaternion.Euler(euler(t));
                k[0].Add(new Keyframe(t, q.x));
                k[1].Add(new Keyframe(t, q.y));
                k[2].Add(new Keyframe(t, q.z));
                k[3].Add(new Keyframe(t, q.w));
            }
            string[] comp = { "x", "y", "z", "w" };
            for (int c = 0; c < 4; c++)
                Curve(clip, path, typeof(Transform), "m_LocalRotation." + comp[c], Smooth(k[c]));
            clip.EnsureQuaternionContinuity();
        }

        public static void Curve(AnimationClip clip, string path, Type type, string prop, AnimationCurve curve)
        {
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, type, prop), curve);
        }

        /// <summary>Krzywa z kluczy (czas, wartość…) z jednym trybem stycznych dla wszystkich kluczy.</summary>
        public static AnimationCurve Keys(AnimationUtility.TangentMode mode, params float[] tv)
        {
            var c = new AnimationCurve();
            for (int i = 0; i + 1 < tv.Length; i += 2)
                c.AddKey(new Keyframe(tv[i], tv[i + 1]));
            for (int i = 0; i < c.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(c, i, mode);
                AnimationUtility.SetKeyRightTangentMode(c, i, mode);
            }
            return c;
        }

        public static void Events(AnimationClip clip, params AnimationEvent[] events)
        {
            AnimationUtility.SetAnimationEvents(clip, events);
        }

        public static AnimationEvent Event(float time, string function, int intParam = 0, string stringParam = "")
        {
            return new AnimationEvent { time = time, functionName = function, intParameter = intParam, stringParameter = stringParam };
        }

        // =====================================================================
        //  ANIMATOR CONTROLLER
        // =====================================================================

        public static AnimatorController NewController(string name)
        {
            return AnimatorController.CreateAnimatorControllerAtPath($"{ControllerDir}/{name}.controller");
        }

        public static AnimatorState State(AnimatorStateMachine sm, string name, Motion motion, float x, float y)
        {
            var s = sm.AddState(name, new Vector3(x, y, 0f));
            s.motion = motion;
            return s;
        }

        /// <summary>Przejście sterowane warunkiem (bez czekania na koniec klipu).</summary>
        public static AnimatorStateTransition Go(AnimatorState from, AnimatorState to, float duration,
            AnimatorConditionMode mode, float threshold, string parameter)
        {
            var t = from.AddTransition(to);
            t.hasExitTime = false;
            t.duration = duration;
            t.hasFixedDuration = true;
            t.AddCondition(mode, threshold, parameter);
            return t;
        }

        /// <summary>Przejście po zakończeniu klipu (Has Exit Time).</summary>
        public static AnimatorStateTransition AfterExit(AnimatorState from, AnimatorState to, float exitTime, float duration)
        {
            var t = from.AddTransition(to);
            t.hasExitTime = true;
            t.exitTime = exitTime;
            t.duration = duration;
            t.hasFixedDuration = true;
            return t;
        }

        /// <summary>Kontroler z jednym zapętlonym stanem.</summary>
        public static AnimatorController Single(string name, AnimationClip clip)
        {
            var c = NewController(name);
            var sm = c.layers[0].stateMachine;
            sm.defaultState = State(sm, clip.name, clip, 300, 100);
            return c;
        }

        /// <summary>Idle + akcja uruchamiana triggerem "Akcja", powrót po zakończeniu klipu.</summary>
        public static AnimatorController IdleAction(string name, AnimationClip idle, AnimationClip action)
        {
            var c = NewController(name);
            c.AddParameter("Akcja", AnimatorControllerParameterType.Trigger);
            var sm = c.layers[0].stateMachine;
            var si = State(sm, "Idle", idle, 300, 100);
            var sa = State(sm, "Akcja", action, 300, 220);
            sm.defaultState = si;
            Go(si, sa, 0.1f, AnimatorConditionMode.If, 0f, "Akcja");
            AfterExit(sa, si, 0.95f, 0.15f);
            return c;
        }

        /// <summary>Maska warstwy dla rigu Generic: aktywne są stawy, których ścieżka zawiera fragment "include".</summary>
        public static AvatarMask Mask(string name, GameObject robotRoot, string include)
        {
            var mask = new AvatarMask();
            mask.AddTransformPath(robotRoot.transform, true);
            for (int i = 0; i < mask.transformCount; i++)
            {
                string p = mask.GetTransformPath(i);
                mask.SetTransformActive(i, !string.IsNullOrEmpty(p) && p.Contains(include));
            }
            string path = $"{MaskDir}/{name}.mask";
            if (AssetDatabase.LoadAssetAtPath<AvatarMask>(path) != null) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mask, path);
            return mask;
        }
    }
}
