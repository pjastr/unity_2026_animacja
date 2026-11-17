using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animacja.EditorTools
{
    public static partial class SceneBuilder
    {
        /// <summary>Lista scen: część I – 12 zasad animacji, część II – animacja od zera, część III – Animator.</summary>
        public static readonly SceneDef[] Scenes =
        {
            new SceneDef("01_Zgniatanie_i_rozciaganie", P01_SquashStretch),
            new SceneDef("02_Antycypacja", P02_Anticipation),
            new SceneDef("03_Inscenizacja", P03_Staging),
            new SceneDef("04_Na_wprost_i_poza_do_pozy", P04_StraightAhead),
            new SceneDef("05_Kontynuacja_i_nakladanie", P05_FollowThrough),
            new SceneDef("06_Spowolnienie_i_przyspieszenie", P06_SlowInOut),
            new SceneDef("07_Luki", P07_Arcs),
            new SceneDef("08_Dzialanie_drugoplanowe", P08_Secondary),
            new SceneDef("09_Czas_timing", P09_Timing),
            new SceneDef("10_Przesada", P10_Exaggeration),
            new SceneDef("11_Solidne_rysowanie", P11_SolidDrawing),
            new SceneDef("12_Atrakcyjnosc", P12_Appeal),
            new SceneDef("13_Klatki_kluczowe_krzywe", Z13_Keyframes),
            new SceneDef("14_Hierarchia_i_wlasciwosci", Z14_Properties),
            new SceneDef("15_Animation_legacy_WrapMode", Z15_Legacy),
            new SceneDef("16_Zdarzenia_animacji", Z16_Events),
            new SceneDef("17_Animator_maszyna_stanow", A17_StateMachine),
            new SceneDef("18_Animator_przejscia", A18_Transitions),
            new SceneDef("19_Animator_Blend_Tree", A19_BlendTree),
            new SceneDef("20_Animator_warstwy_maski", A20_Layers),
            new SceneDef("21_Animator_podmaszyna_Behaviour", A21_SubStateMachine),
            new SceneDef("22_Animator_Root_Motion", A22_RootMotion),
        };

        // =====================================================================
        //  KLIPY WSPÓLNE (tworzone raz przy generowaniu)
        // =====================================================================

        static readonly Dictionary<string, AnimationClip> s_Clips = new Dictionary<string, AnimationClip>();

        static AnimationClip Clip(string name)
        {
            AnimationClip c;
            if (s_Clips.TryGetValue(name, out c) && c != null) return c;
            c = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{ClipFactory.ClipDir}/{name}.anim");
            if (c == null)
            {
                CreateSharedAssets();
                s_Clips.TryGetValue(name, out c);
            }
            return c;
        }

        static void Reg(AnimationClip c) { s_Clips[c.name] = c; }

        static void CreateSharedAssets()
        {
            ClipFactory.EnsureDirs();
            s_Clips.Clear();
            Reg(ClipFactory.Robot("Stoj", 2f, true, Poses.Idle));
            Reg(ClipFactory.Robot("Chod", 1f, true, t => Poses.Walk(t)));
            Reg(ClipFactory.Robot("Chod_sztywny", 1f, true, t => Poses.Walk(t, true)));
            Reg(ClipFactory.Robot("Bieg", 0.7f, true, Poses.Run));
            Reg(ClipFactory.Robot("Chod_tyl", 1f, true, Poses.WalkBack));
            Reg(ClipFactory.Robot("Krok_w_lewo", 1f, true, t => Poses.Strafe(t, -1f)));
            Reg(ClipFactory.Robot("Krok_w_prawo", 1f, true, t => Poses.Strafe(t, 1f)));
            Reg(ClipFactory.Robot("Machanie", 1.2f, true, Poses.Wave));
            Reg(ClipFactory.Robot("Kiwanie", 0.8f, true, Poses.Nod));
            Reg(ClipFactory.Robot("Skok_bez_antycypacji", 1.6f, false, t => Poses.Jump(t, 0f, 1f)));
            Reg(ClipFactory.Robot("Skok_z_antycypacja", 1.6f, false, t => Poses.Jump(t, 1f, 1f)));
            Reg(ClipFactory.Robot("Skok_przesada_0", 1.6f, false, t => Poses.Jump(t, 1f, 0.35f)));
            Reg(ClipFactory.Robot("Skok_przesada_1", 1.6f, false, t => Poses.Jump(t, 1f, 1f)));
            Reg(ClipFactory.Robot("Skok_przesada_2", 1.6f, false, t => Poses.Jump(t, 1f, 1.8f)));
            Reg(ClipFactory.Robot("Atak1", 0.6f, false, t => Poses.Attack(t, 1)));
            Reg(ClipFactory.Robot("Atak2", 0.6f, false, t => Poses.Attack(t, 2)));
            Reg(ClipFactory.Robot("Atak3", 0.6f, false, t => Poses.Attack(t, 3)));
            Reg(ClipFactory.Robot("Poza_do_pozy", 4f, true, Poses.PoseToPose));
            Reg(ClipFactory.Robot("Chod_RootMotion", 1f, true, Poses.WalkRootMotion));
            Reg(ClipFactory.Robot("Skret_RootMotion", 1f, true, Poses.TurnRootMotion));

            // klipy ze zdarzeniami (scena 16) – osobne kopie, żeby zdarzenia nie wymagały odbiorcy w innych scenach
            var walkEv = ClipFactory.Robot("Chod_zdarzenia", 1f, true, t => Poses.Walk(t));
            ClipFactory.Events(walkEv, ClipFactory.Event(0.25f, "OnFootstep", 0), ClipFactory.Event(0.75f, "OnFootstep", 1));
            Reg(walkEv);
            var runEv = ClipFactory.Robot("Bieg_zdarzenia", 0.7f, true, Poses.Run);
            ClipFactory.Events(runEv, ClipFactory.Event(0.175f, "OnFootstep", 0), ClipFactory.Event(0.525f, "OnFootstep", 1));
            Reg(runEv);
            var atkEv = ClipFactory.Robot("Atak_zdarzenia", 0.6f, false, t => Poses.Attack(t, 1));
            ClipFactory.Events(atkEv, ClipFactory.Event(0.24f, "OnHit", 0, "cios"));
            Reg(atkEv);
            AssetDatabase.SaveAssets();
        }

        // =====================================================================
        //  POMOCNICZE
        // =====================================================================

        static Material M_Ghost => TransparentMat("Duszek", new Color(0.75f, 0.85f, 1f, 0.28f));
        static Material M_GhostRobot => TransparentMat("Duszek_robot", new Color(0.6f, 0.75f, 1f, 0.22f));
        static Material M_Joint => Mat("Robot_staw", new Color(0.18f, 0.19f, 0.22f));
        static Material M_Visor => Mat("Robot_wizjer", new Color(0.3f, 0.9f, 1f), 0.9f);

        static RobotRig Robot(string name, Vector3 feet, Material body, float yaw = 180f, Transform parent = null)
        {
            return RobotFactory.Build(name, feet, yaw, body, M_Joint, M_Visor, parent);
        }

        static TrailRenderer Trail(GameObject go, Color c, float time = 1.5f, float width = 0.06f)
        {
            var tr = go.AddComponent<TrailRenderer>();
            tr.time = time;
            tr.widthMultiplier = width;
            tr.minVertexDistance = 0.02f;
            tr.sharedMaterial = UnlitMat("Slad_" + ColorUtility.ToHtmlStringRGB(c), c);
            tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return tr;
        }

        static SpacingGhosts Ghosts(GameObject go, float interval = 1f / 12f, int max = 14)
        {
            var g = go.AddComponent<SpacingGhosts>();
            g.interval = interval;
            g.maxGhosts = max;
            g.ghostMaterial = M_Ghost;
            g.sizeFactor = 0.55f;
            return g;
        }

        /// <summary>Napisy "BEZ ZASADY" / "Z ZASADĄ" nad dwoma polami porównania.</summary>
        static void CompareLabels(string left, string right, float x = 3f, float y = 0.05f, float z = -1.6f)
        {
            SectionLabel(left, new Vector3(-x, y, z));
            SectionLabel(right, new Vector3(x, y, z));
        }

        static void Pedestal(Vector3 pos, float size = 2.2f)
        {
            Prim(PrimitiveType.Cylinder, "Podest", pos + Vector3.up * 0.025f, new Vector3(size, 0.025f, size), M_Gray);
        }

        const string PrincipleKeys = "<b>LPM</b> – zwolnione tempo ×0,25   <b>PPM</b> – ślady wł./wył.";
    }
}
