using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Animacja.EditorTools
{
    /// <summary>Część II – jak powstaje animacja od zera: klatki kluczowe, krzywe, właściwości, Animation, zdarzenia.</summary>
    public static partial class SceneBuilder
    {
        // =====================================================================
        // 13 – KLATKI KLUCZOWE I KRZYWE
        // =====================================================================
        static void Z13_Keyframes()
        {
            Begin("13 · Klatki kluczowe i krzywe animacji",
                "Klip animacji (<b>AnimationClip</b>) to zbiór <b>krzywych</b>: każda opisuje jedną liczbę (np. pozycję Y) w czasie. " +
                "Krzywą wyznaczają <b>klatki kluczowe</b> (keyframes) – pary (czas, wartość) – a wartości pomiędzy liczy " +
                "<b>interpolacja</b>.\n\n" +
                "Wszystkie pięć sześcianów ma te same klucze: (0 s, 0), (1 s, 1,2), (2 s, 0,4), (3 s, 1,5), (4 s, 0). " +
                "Różnią się tylko <b>stycznymi</b> (tangents) kluczy:\n" +
                "• <b>Constant</b> – wartość trzyma się do następnego klucza (skok),\n" +
                "• <b>Linear</b> – odcinki proste, stała prędkość, ostre zmiany kierunku,\n" +
                "• <b>Clamped Auto</b> – gładko, bez przestrzelenia wartości (domyślne w Unity),\n" +
                "• <b>Auto</b> – gładko, może przestrzelić wartości kluczy,\n" +
                "• <b>Free</b> – styczne ustawione ręcznie (tu strome: wyraźne przestrzelenie).\n\n" +
                "W edytorze: okno <b>Animation</b> (Ctrl+6) – przycisk nagrywania, dodawanie kluczy, zakładka <b>Curves</b>, " +
                "PPM na kluczu → tryb stycznych.",
                "<b>Spacja</b> – od początku   <b>LPM</b> – zwolnione tempo   <b>PPM</b> – pauza", "", statusWidth: 420f);
            Floor();
            Cam(new Vector3(0.5f, 4.2f, -14.5f), new Vector3(0.5f, 3.6f, 0f));
            var demo = Manager<KeyframeDemo>();
            var modes = new[]
            {
                AnimationUtility.TangentMode.Constant, AnimationUtility.TangentMode.Linear, AnimationUtility.TangentMode.ClampedAuto,
                AnimationUtility.TangentMode.Auto, AnimationUtility.TangentMode.Free,
            };
            string[] names = { "Constant", "Linear", "Clamped Auto", "Auto", "Free" };
            float[] tv = { 0f, 0f, 1f, 1.2f, 2f, 0.4f, 3f, 1.5f, 4f, 0f };
            var anims = new List<Animation>();
            for (int i = 0; i < modes.Length; i++)
            {
                int col = i % 2, row = i / 2;
                Vector3 origin = new Vector3(-6f + col * 7.5f, 6.4f - row * 2.9f, 0f);
                if (i == 4) origin.x = -2.25f;
                var curve = ClipFactory.Keys(modes[i], tv);
                if (modes[i] == AnimationUtility.TangentMode.Free)
                {
                    var ks = curve.keys;
                    for (int k = 1; k < ks.Length - 1; k++) { ks[k].inTangent = k % 2 == 1 ? 2.5f : -2.5f; ks[k].outTangent = ks[k].inTangent; }
                    curve.keys = ks;
                    for (int k = 0; k < curve.length; k++)
                    {
                        AnimationUtility.SetKeyBroken(curve, k, false);
                        AnimationUtility.SetKeyLeftTangentMode(curve, k, AnimationUtility.TangentMode.Free);
                        AnimationUtility.SetKeyRightTangentMode(curve, k, AnimationUtility.TangentMode.Free);
                    }
                }

                // klip legacy (komponent Animation) – łatwy odczyt czasu do znacznika na wykresie
                var clip = ClipFactory.GetOrCreate("Krzywa_" + names[i].Replace(" ", "_"), true);
                ClipFactory.Curve(clip, "", typeof(Transform), "m_LocalPosition.x", ClipFactory.Keys(AnimationUtility.TangentMode.Constant, 0f, 0f, 4f, 0f));
                ClipFactory.Curve(clip, "", typeof(Transform), "m_LocalPosition.y", curve);
                ClipFactory.Curve(clip, "", typeof(Transform), "m_LocalPosition.z", ClipFactory.Keys(AnimationUtility.TangentMode.Constant, 0f, 0f, 4f, 0f));
                clip.wrapMode = WrapMode.Loop;
                EditorUtility.SetDirty(clip);

                // wykres krzywej
                var pts = new Vector3[121];
                for (int s = 0; s <= 120; s++)
                {
                    float t = s / 30f;
                    pts[s] = origin + new Vector3(t, curve.Evaluate(t), 0f);
                }
                Line($"Wykres_{names[i]}", pts, Palette[i].color, 0.05f, false);
                Line($"Os_czasu_{names[i]}", new[] { origin, origin + new Vector3(4.2f, 0f, 0f) }, new Color(0.3f, 0.3f, 0.33f), 0.025f, false);
                Line($"Os_wartosci_{names[i]}", new[] { origin, origin + new Vector3(0f, 1.9f, 0f) }, new Color(0.3f, 0.3f, 0.33f), 0.025f, false);
                for (int k = 0; k < tv.Length; k += 2)
                {
                    var key = Box($"Klucz_{k / 2}", origin + new Vector3(tv[k], tv[k + 1], -0.05f), Vector3.one * 0.14f, M_White, null, Quaternion.Euler(0, 0, 45));
                    NoCollider(key);
                }
                var marker = Ball($"Znacznik_{names[i]}", origin, 0.2f, M_Yellow);
                NoCollider(marker);
                SectionLabel(names[i], origin + new Vector3(2f, -0.45f, 0f), null, 16);

                // animowany sześcian obok wykresu
                var holder = Group($"Uchwyt_{names[i]}");
                holder.position = origin + new Vector3(5f, 0.2f, 0f);
                var cube = Box($"Szescian_{names[i]}", holder.position, Vector3.one * 0.4f, Palette[i], holder);
                NoCollider(cube);
                var anim = cube.AddComponent<Animation>();
                anim.AddClip(clip, clip.name);
                anim.clip = clip;
                anim.playAutomatically = true;
                anim.wrapMode = WrapMode.Loop;
                anims.Add(anim);

                var ph = marker.AddComponent<CurvePlayhead>();
                ph.anim = anim;
                ph.clipName = clip.name;
                ph.curve = curve;
                ph.marker = marker.transform;
                ph.graphOrigin = origin + new Vector3(0f, 0f, -0.1f);
                ph.tangentName = names[i];
                Label(marker, "", 0.3f, 12);
            }
            demo.animations = anims.ToArray();
        }

        // =====================================================================
        // 14 – HIERARCHIA I WŁAŚCIWOŚCI
        // =====================================================================
        static void Z14_Properties()
        {
            Begin("14 · Co można animować? Ścieżki i właściwości",
                "Krzywa w klipie jest przypisana do <b>właściwości komponentu</b> obiektu wskazanego <b>ścieżką</b> względem " +
                "obiektu z Animatorem (EditorCurveBinding: path + typ komponentu + nazwa właściwości).\n\n" +
                "Ten klip „Pokaz_wlasciwosci” (4,8 s) animuje jednocześnie:\n" +
                "• <b>Transform</b> – obroty stawów robota (ścieżki Robot/Biodra/Tulow/…),\n" +
                "• <b>Light</b> – natężenie i kolor lampy,\n" +
                "• <b>MeshRenderer</b> – kolor materiału (material._BaseColor),\n" +
                "• <b>GameObject</b> – aktywność tabliczki (m_IsActive, klucze Constant),\n" +
                "• skalę i pozycję kuli.\n\n" +
                "Animować można każde pole serializowane typu liczbowego, bool, Color, Vector – także pola własnych skryptów.\n\n" +
                "<b>Uwaga</b>: zmiana nazwy obiektu w hierarchii zrywa ścieżkę – krzywa świeci na żółto w oknie Animation (Missing).",
                "<b>Spacja</b> – pauza   <b>LPM</b> – speed 0,25   <b>PPM</b> – od początku", "", statusWidth: 520f);
            Floor();
            Cam(new Vector3(0f, 3.4f, -9.5f), new Vector3(0f, 1.4f, 0f));

            var show = new GameObject("Pokaz");
            Robot("Robot", new Vector3(-2.5f, 0f, 0f), M_Blue, 180f, show.transform);
            var lampGo = new GameObject("Lampa");
            lampGo.transform.SetParent(show.transform, false);
            lampGo.transform.position = new Vector3(0f, 3f, -1f);
            var lamp = lampGo.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.range = 8f;
            lamp.intensity = 2f;
            var bulb = Ball("Zarowka", lampGo.transform.position, 0.3f, M_Yellow, lampGo.transform);
            NoCollider(bulb);
            var sign = Box("Tabliczka", new Vector3(0f, 1.2f, 1.5f), new Vector3(2.2f, 1.2f, 0.1f), M_Wood, show.transform);
            NoCollider(sign);
            Label(sign, "Tabliczka (m_IsActive)", 0.8f, 13);
            var ball = Ball("Kula", new Vector3(2.5f, 0.6f, 0f), 1.2f, Mat("Kula_kolor", Color.white), show.transform);
            NoCollider(ball);

            var clip = ClipFactory.GetOrCreate("Pokaz_wlasciwosci");
            ClipFactory.AddRobotCurves(clip, 4.8f, Poses.Wave, "Robot/");
            var lin = AnimationUtility.TangentMode.ClampedAuto;
            ClipFactory.Curve(clip, "Lampa", typeof(Light), "m_Intensity", ClipFactory.Keys(lin, 0f, 0f, 1f, 4.8f, 2f, 1f, 3f, 5f, 4.8f, 0f));
            ClipFactory.Curve(clip, "Lampa", typeof(Light), "m_Color.r", ClipFactory.Keys(lin, 0f, 1f, 2f, 0.2f, 4.8f, 1f));
            ClipFactory.Curve(clip, "Lampa", typeof(Light), "m_Color.g", ClipFactory.Keys(lin, 0f, 0.6f, 2f, 1f, 4.8f, 0.6f));
            ClipFactory.Curve(clip, "Lampa", typeof(Light), "m_Color.b", ClipFactory.Keys(lin, 0f, 0.2f, 2f, 1f, 4.8f, 0.2f));
            ClipFactory.Curve(clip, "Tabliczka", typeof(GameObject), "m_IsActive",
                ClipFactory.Keys(AnimationUtility.TangentMode.Constant, 0f, 1f, 2f, 0f, 3f, 1f, 4.8f, 1f));
            ClipFactory.Curve(clip, "Kula", typeof(MeshRenderer), "material._BaseColor.r", ClipFactory.Keys(lin, 0f, 1f, 1.3f, 0.2f, 2.6f, 0.3f, 4.8f, 1f));
            ClipFactory.Curve(clip, "Kula", typeof(MeshRenderer), "material._BaseColor.g", ClipFactory.Keys(lin, 0f, 0.3f, 1.3f, 0.9f, 2.6f, 0.4f, 4.8f, 0.3f));
            ClipFactory.Curve(clip, "Kula", typeof(MeshRenderer), "material._BaseColor.b", ClipFactory.Keys(lin, 0f, 0.2f, 1.3f, 0.3f, 2.6f, 1f, 4.8f, 0.2f));
            ClipFactory.Curve(clip, "Kula", typeof(MeshRenderer), "material._BaseColor.a", ClipFactory.Keys(lin, 0f, 1f, 4.8f, 1f));
            ClipFactory.Curve(clip, "Kula", typeof(Transform), "m_LocalScale.x", ClipFactory.Keys(lin, 0f, 1.2f, 1f, 1.6f, 2f, 1.2f, 3f, 0.8f, 4.8f, 1.2f));
            ClipFactory.Curve(clip, "Kula", typeof(Transform), "m_LocalScale.y", ClipFactory.Keys(lin, 0f, 1.2f, 1f, 0.9f, 2f, 1.2f, 3f, 1.7f, 4.8f, 1.2f));
            ClipFactory.Curve(clip, "Kula", typeof(Transform), "m_LocalScale.z", ClipFactory.Keys(lin, 0f, 1.2f, 1f, 1.6f, 2f, 1.2f, 3f, 0.8f, 4.8f, 1.2f));
            ClipFactory.SetLoop(clip, true);
            clip.EnsureQuaternionContinuity();
            EditorUtility.SetDirty(clip);

            var c = ClipFactory.NewController("Z14_Pokaz");
            var sm = c.layers[0].stateMachine;
            sm.defaultState = ClipFactory.State(sm, "Pokaz", clip, 300, 100);
            var animator = show.AddComponent<Animator>();
            animator.runtimeAnimatorController = c;

            var demo = Manager<PropertyClipDemo>();
            demo.animator = animator;
            demo.stateName = "Pokaz";
            var desc = new List<string>();
            var seen = new HashSet<string>();
            foreach (var b in AnimationUtility.GetCurveBindings(clip))
            {
                string prop = b.propertyName;
                int dot = prop.LastIndexOf('.');
                if (dot > 0 && prop.Length - dot <= 2) prop = prop.Substring(0, dot) + ".*";
                string path = b.path.StartsWith("Robot/") ? "Robot/… (15 stawów)" : b.path;
                string line = $"   {path} : {b.type.Name}.{prop}";
                if (seen.Add(line)) desc.Add(line);
            }
            demo.bindings = desc.ToArray();
        }

        // =====================================================================
        // 15 – ANIMATION (LEGACY) I WRAP MODE
        // =====================================================================
        static void Z15_Legacy()
        {
            Begin("15 · Komponent Animation (legacy) i tryby zawijania",
                "Klip można odtworzyć na dwa sposoby:\n" +
                "• <b>Animator</b> + Animator Controller – maszyna stanów, mieszanie, warstwy (system Mecanim, zalecany),\n" +
                "• <b>Animation</b> (legacy) – proste odtwarzanie klipów z kodu: Play, CrossFade, Blend. " +
                "Klip musi mieć zaznaczone <b>Legacy</b> (Inspector w trybie Debug).\n\n" +
                "<b>WrapMode</b> – co dzieje się po końcu klipu:\n" +
                "• <b>Once</b> – zatrzymuje się i wraca do pierwszej klatki,\n" +
                "• <b>Loop</b> – zaczyna od początku (skok, jeśli koniec ≠ początek),\n" +
                "• <b>PingPong</b> – odtwarza tam i z powrotem,\n" +
                "• <b>ClampForever</b> – zostaje na ostatniej klatce.\n\n" +
                "W Animatorze odpowiednikiem jest opcja klipu <b>Loop Time</b> (i Loop Pose).\n" +
                "Ostatni sześcian: <b>CrossFade</b> – płynne przejście między dwoma klipami.",
                "<b>Spacja</b> – od początku   <b>LPM</b> – wstecz (speed −1)   <b>PPM</b> – CrossFade", "", statusWidth: 560f);
            Floor();
            Cam(new Vector3(0f, 3.6f, -11f), new Vector3(0f, 1.5f, 0f));

            var legacyClip = ClipFactory.GetOrCreate("Ruch_legacy", true);
            FillMoveClip(legacyClip);
            var spinClip = ClipFactory.GetOrCreate("Obrot_legacy", true);
            var ease = AnimationUtility.TangentMode.ClampedAuto;
            ClipFactory.RotationCurves(spinClip, "", 2f, t => new Vector3(180f * t, 0f, 0f));
            ClipFactory.Curve(spinClip, "", typeof(Transform), "m_LocalPosition.x", ClipFactory.Keys(ease, 0f, 0f, 2f, 0f));
            ClipFactory.Curve(spinClip, "", typeof(Transform), "m_LocalPosition.y", ClipFactory.Keys(ease, 0f, 0.5f, 2f, 0.5f));
            ClipFactory.Curve(spinClip, "", typeof(Transform), "m_LocalPosition.z", ClipFactory.Keys(ease, 0f, 0f, 2f, 0f));
            spinClip.wrapMode = WrapMode.Loop;
            var genericClip = ClipFactory.GetOrCreate("Ruch_animator");
            FillMoveClip(genericClip);
            ClipFactory.SetLoop(genericClip, true);
            EditorUtility.SetDirty(legacyClip);
            EditorUtility.SetDirty(spinClip);
            EditorUtility.SetDirty(genericClip);

            var modes = new[] { WrapMode.Once, WrapMode.Loop, WrapMode.PingPong, WrapMode.ClampForever };
            var anims = new List<Animation>();
            for (int i = 0; i < modes.Length; i++)
            {
                float x = -6f + i * 2.6f;
                var holder = Group("Stanowisko_" + modes[i]);
                holder.position = new Vector3(x, 0f, 0f);
                var cube = Box("Szescian_" + modes[i], holder.position + Vector3.up * 0.5f, Vector3.one * 0.8f, Palette[i], holder);
                NoCollider(cube);
                var anim = cube.AddComponent<Animation>();
                anim.AddClip(legacyClip, legacyClip.name);
                anim.clip = legacyClip;
                anim.playAutomatically = false;
                anims.Add(anim);
                SectionLabel(modes[i].ToString(), new Vector3(x, 0.05f, -1.4f), null, 16);
            }
            var cfHolder = Group("Stanowisko_CrossFade");
            cfHolder.position = new Vector3(4.6f, 0f, 0f);
            var cf = Box("Szescian_CrossFade", cfHolder.position + Vector3.up * 0.5f, Vector3.one * 0.8f, M_White, cfHolder);
            NoCollider(cf);
            var cfAnim = cf.AddComponent<Animation>();
            cfAnim.AddClip(legacyClip, legacyClip.name);
            cfAnim.AddClip(spinClip, spinClip.name);
            cfAnim.clip = legacyClip;
            cfAnim.wrapMode = WrapMode.Loop;
            cfAnim.playAutomatically = true;
            SectionLabel("CrossFade", new Vector3(4.6f, 0.05f, -1.4f), null, 16);

            var animHolder = Group("Stanowisko_Animator");
            animHolder.position = new Vector3(7.2f, 0f, 0f);
            var ac = Box("Szescian_Animator", animHolder.position + Vector3.up * 0.5f, Vector3.one * 0.8f, M_Gray, animHolder);
            NoCollider(ac);
            var animator = ac.AddComponent<Animator>();
            animator.runtimeAnimatorController = ClipFactory.Single("Z15_Ruch", genericClip);
            SectionLabel("Animator", new Vector3(7.2f, 0.05f, -1.4f), null, 16);

            var demo = Manager<LegacyAnimationDemo>();
            demo.legacy = anims.ToArray();
            demo.modes = modes;
            demo.clipName = legacyClip.name;
            demo.crossFadeTarget = cfAnim;
            demo.crossFadeA = legacyClip.name;
            demo.crossFadeB = spinClip.name;
            demo.animator = animator;
        }

        /// <summary>Ruch w górę o 2 m z obrotem o 180° w ciągu 2 s (koniec ≠ początek – widać różnice WrapMode).</summary>
        static void FillMoveClip(AnimationClip clip)
        {
            var e = AnimationUtility.TangentMode.ClampedAuto;
            ClipFactory.Curve(clip, "", typeof(Transform), "m_LocalPosition.x", ClipFactory.Keys(e, 0f, 0f, 2f, 0f));
            ClipFactory.Curve(clip, "", typeof(Transform), "m_LocalPosition.y", ClipFactory.Keys(e, 0f, 0.5f, 2f, 2.5f));
            ClipFactory.Curve(clip, "", typeof(Transform), "m_LocalPosition.z", ClipFactory.Keys(e, 0f, 0f, 2f, 0f));
            ClipFactory.RotationCurves(clip, "", 2f, t => new Vector3(0f, 180f * Mathf.SmoothStep(0f, 1f, t / 2f), 0f));
        }

        // =====================================================================
        // 16 – ZDARZENIA ANIMACJI
        // =====================================================================
        static void Z16_Events()
        {
            Begin("16 · Zdarzenia animacji (Animation Events)",
                "<b>Zdarzenie animacji</b> to znacznik w klipie, który w zadanym momencie wywołuje <b>metodę skryptu</b> " +
                "na obiekcie z Animatorem (lub komponentem Animation). Może przekazać jeden parametr: int, float, string " +
                "albo referencję do obiektu.\n\n" +
                "Zastosowania – zsynchronizowanie efektów z ruchem:\n" +
                "• dźwięk i pył przy <b>kroku</b> – OnFootstep(0/1) w chwili postawienia stopy,\n" +
                "• zadanie obrażeń w klatce, w której cios <b>trafia</b> – OnHit(\"cios\"),\n" +
                "• pojawienie się pocisku, efekt cząsteczkowy, koniec ataku.\n\n" +
                "W edytorze: okno Animation → przycisk <b>Add Event</b> albo Inspector klipu → sekcja Events.\n" +
                "Uwaga: brak metody o podanej nazwie = błąd „AnimationEvent has no receiver”. " +
                "Przy mieszaniu klipów zdarzenia mogą odpalić z obu klipów naraz.",
                "<b>Spacja</b> – chód ↔ bieg   <b>LPM</b> – atak", "", statusWidth: 520f);
            Floor();
            Cam(new Vector3(-1.5f, 2.6f, -6.5f), new Vector3(0f, 1.1f, 0f));
            var r = Robot("Robot", Vector3.zero, M_Teal, 180f);
            var c = ClipFactory.NewController("Z16_Zdarzenia");
            c.AddParameter("Bieg", AnimatorControllerParameterType.Bool);
            c.AddParameter("Atak", AnimatorControllerParameterType.Trigger);
            var sm = c.layers[0].stateMachine;
            var walk = ClipFactory.State(sm, "Chod", Clip("Chod_zdarzenia"), 300, 100);
            var run = ClipFactory.State(sm, "Bieg", Clip("Bieg_zdarzenia"), 550, 100);
            var atk = ClipFactory.State(sm, "Atak", Clip("Atak_zdarzenia"), 420, 250);
            sm.defaultState = walk;
            ClipFactory.Go(walk, run, 0.2f, AnimatorConditionMode.If, 0f, "Bieg");
            ClipFactory.Go(run, walk, 0.2f, AnimatorConditionMode.IfNot, 0f, "Bieg");
            var any = sm.AddAnyStateTransition(atk);
            any.AddCondition(AnimatorConditionMode.If, 0f, "Atak");
            any.duration = 0.1f;
            any.canTransitionToSelf = false;
            ClipFactory.AfterExit(atk, walk, 0.95f, 0.15f);
            var animator = r.AddAnimator(c);

            var target = Box("Cel", new Vector3(-0.33f, 1.7f, -1.0f), new Vector3(0.4f, 0.4f, 0.4f), M_Wood);
            var recv = r.root.AddComponent<AnimationEventReceiver>();
            recv.leftFoot = r[J.FootL];
            recv.rightFoot = r[J.FootR];
            recv.rightHand = r[J.HandR];
            recv.dustMaterial = TransparentMat("Pyl", new Color(0.85f, 0.8f, 0.7f, 0.5f));
            recv.hitTarget = target.GetComponent<Renderer>();
            recv.hitMaterial = M_Red;
            var demo = Manager<EventsDemo>();
            demo.animator = animator;
        }
    }
}
