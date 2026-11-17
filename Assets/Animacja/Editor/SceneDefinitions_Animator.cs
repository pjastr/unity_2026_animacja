using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Animacja.EditorTools
{
    /// <summary>Część III – Animator: maszyna stanów, przejścia, Blend Tree, warstwy, podmaszyny, root motion.</summary>
    public static partial class SceneBuilder
    {
        // =====================================================================
        // 17 – MASZYNA STANÓW
        // =====================================================================
        static void A17_StateMachine()
        {
            Begin("17 · Animator – maszyna stanów",
                "<b>Animator Controller</b> to maszyna stanów: każdy <b>stan</b> odtwarza ruch (klip lub Blend Tree), " +
                "a <b>przejścia</b> między stanami uruchamiają <b>warunki</b> na <b>parametrach</b>.\n\n" +
                "Parametry (ustawiane z kodu):\n" +
                "• <b>Float</b> Speed – SetFloat (prędkość),\n" +
                "• <b>Trigger</b> Jump – SetTrigger (jednorazowy, kasowany po użyciu),\n" +
                "• <b>Bool</b> Wave – SetBool (stan włączony/wyłączony),\n" +
                "• (także <b>Int</b>).\n\n" +
                "Węzły specjalne: <b>Entry</b> (stan domyślny – pomarańczowy w edytorze), <b>Any State</b> (przejście z dowolnego stanu), " +
                "<b>Exit</b>.\n\n" +
                "Tablice z tyłu: aktywny stan – zielony, stan docelowy przejścia – żółty. " +
                "Otwórz okno <b>Animator</b> (Window > Animation > Animator) i zaznacz robota, żeby widzieć graf na żywo.",
                "<b>W / S</b> – prędkość   <b>Spacja</b> – skok   <b>LPM</b> – machanie", "", statusWidth: 560f);
            Floor();
            Cam(new Vector3(0f, 7.5f, -12.5f), new Vector3(0f, 1f, 2f));
            var c = ClipFactory.NewController("A17_Maszyna_stanow");
            c.AddParameter("Speed", AnimatorControllerParameterType.Float);
            c.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            c.AddParameter("Wave", AnimatorControllerParameterType.Bool);
            var sm = c.layers[0].stateMachine;
            var idle = ClipFactory.State(sm, "Idle", Clip("Stoj"), 300, 120);
            var walk = ClipFactory.State(sm, "Walk", Clip("Chod"), 550, 120);
            var run = ClipFactory.State(sm, "Run", Clip("Bieg"), 800, 120);
            var jump = ClipFactory.State(sm, "Jump", Clip("Skok_z_antycypacja"), 550, 300);
            var wave = ClipFactory.State(sm, "Wave", Clip("Machanie"), 300, -60);
            sm.defaultState = idle;
            sm.anyStatePosition = new Vector3(300, 300, 0);
            ClipFactory.Go(idle, walk, 0.2f, AnimatorConditionMode.Greater, 0.1f, "Speed");
            ClipFactory.Go(walk, idle, 0.2f, AnimatorConditionMode.Less, 0.1f, "Speed");
            ClipFactory.Go(walk, run, 0.25f, AnimatorConditionMode.Greater, 2.5f, "Speed");
            ClipFactory.Go(run, walk, 0.25f, AnimatorConditionMode.Less, 2.5f, "Speed");
            var any = sm.AddAnyStateTransition(jump);
            any.AddCondition(AnimatorConditionMode.If, 0f, "Jump");
            any.duration = 0.1f;
            any.canTransitionToSelf = false;
            ClipFactory.AfterExit(jump, idle, 0.9f, 0.2f);
            ClipFactory.Go(idle, wave, 0.25f, AnimatorConditionMode.If, 0f, "Wave");
            ClipFactory.Go(wave, idle, 0.25f, AnimatorConditionMode.IfNot, 0f, "Wave");

            var r = Robot("Robot", new Vector3(0f, 0f, 4f), M_Orange);
            var animator = r.AddAnimator(c);
            var demo = Manager<StateMachineDemo>();
            demo.animator = animator;
            demo.center = new Vector3(0f, 0f, 0f);
            demo.radius = 4f;
            Prim(PrimitiveType.Cylinder, "Tor", new Vector3(0f, 0.01f, 0f), new Vector3(8.8f, 0.01f, 8.8f), M_Gray);
            Prim(PrimitiveType.Cylinder, "Srodek_toru", new Vector3(0f, 0.02f, 0f), new Vector3(7.2f, 0.01f, 7.2f), Mat("Trawa", new Color(0.35f, 0.5f, 0.32f)));

            string[] names = { "Idle", "Walk", "Run", "Jump", "Wave" };
            var nodes = new List<Renderer>();
            for (int i = 0; i < names.Length; i++)
            {
                var n = Box("Stan_" + names[i], new Vector3(-6f + i * 3f, 0.6f, 7.5f), new Vector3(2.4f, 1.2f, 0.3f), M_Gray);
                NoCollider(n);
                Label(n, names[i], 0.9f, 18);
                nodes.Add(n.GetComponent<Renderer>());
            }
            demo.stateNodes = nodes.ToArray();
            demo.nodeIdle = M_Gray;
            demo.nodeActive = M_Green;
            demo.nodeNext = M_Yellow;
        }

        // =====================================================================
        // 18 – PRZEJŚCIA
        // =====================================================================
        static void A18_Transitions()
        {
            Begin("18 · Animator – ustawienia przejść",
                "Każde przejście (strzałka w Animatorze) ma ustawienia:\n" +
                "• <b>Has Exit Time</b> – przejście może nastąpić dopiero w chwili <b>Exit Time</b> " +
                "(ułamek długości klipu, np. 1 = koniec cyklu). Bez warunków działa jak „po zakończeniu”.\n" +
                "• <b>Transition Duration</b> – czas mieszania (blend) obu stanów; <b>Fixed Duration</b> – w sekundach " +
                "albo w ułamku klipu.\n" +
                "• <b>Transition Offset</b> – od którego miejsca startuje stan docelowy.\n" +
                "• <b>Interruption Source</b> – czy inne przejście może przerwać trwające przejście.\n\n" +
                "Trzy roboty, ten sam trigger „Akcja” (Idle → Wave):\n" +
                "1. czas przejścia 0 – natychmiastowy przeskok pozy,\n" +
                "2. czas 0,3 s – płynne zmieszanie,\n" +
                "3. Has Exit Time = 1 – czeka na koniec cyklu Idle (do 2 s): mniejsza responsywność.\n\n" +
                "LPM – trigger „Skok” z <b>Any State</b> (przerywa każdy stan).",
                "<b>Spacja</b> – trigger Akcja (Idle → Wave)   <b>LPM</b> – trigger Skok", "", statusWidth: 520f);
            Floor();
            PrincipleCamera();
            string[] desc = { "Duration 0, bez Exit Time", "Duration 0,3 s, bez Exit Time", "Has Exit Time = 1, Duration 0,3 s" };
            Material[] mats = { M_Blue, M_Green, M_Red };
            var anims = new List<Animator>();
            for (int i = 0; i < 3; i++)
            {
                var c = ClipFactory.NewController("A18_Przejscie_" + (i + 1));
                c.AddParameter("Akcja", AnimatorControllerParameterType.Trigger);
                c.AddParameter("Skok", AnimatorControllerParameterType.Trigger);
                var sm = c.layers[0].stateMachine;
                var idle = ClipFactory.State(sm, "Idle", Clip("Stoj"), 300, 100);
                var wave = ClipFactory.State(sm, "Wave", Clip("Machanie"), 550, 100);
                var jump = ClipFactory.State(sm, "Jump", Clip("Skok_z_antycypacja"), 420, 260);
                sm.defaultState = idle;
                var t = idle.AddTransition(wave);
                t.AddCondition(AnimatorConditionMode.If, 0f, "Akcja");
                t.hasFixedDuration = true;
                t.duration = i == 0 ? 0f : 0.3f;
                t.hasExitTime = i == 2;
                t.exitTime = 1f;
                ClipFactory.AfterExit(wave, idle, 2f, 0.3f);      // dwa machnięcia (2 cykle klipu)
                var any = sm.AddAnyStateTransition(jump);
                any.AddCondition(AnimatorConditionMode.If, 0f, "Skok");
                any.duration = 0.1f;
                any.canTransitionToSelf = false;
                ClipFactory.AfterExit(jump, idle, 0.9f, 0.2f);
                float x = -3.5f + i * 3.5f;
                var r = Robot("Robot_" + (i + 1), new Vector3(x, 0f, 0f), mats[i]);
                anims.Add(r.AddAnimator(c));
                SectionLabel((i + 1) + ". " + desc[i], new Vector3(x, 0.05f, -1.6f), null, 15);
            }
            var demo = Manager<TransitionCompareDemo>();
            demo.animators = anims.ToArray();
            demo.descriptions = desc;
        }

        // =====================================================================
        // 19 – BLEND TREE
        // =====================================================================
        static void A19_BlendTree()
        {
            Begin("19 · Animator – Blend Tree",
                "<b>Blend Tree</b> to stan, który <b>miesza kilka klipów naraz</b> według wartości parametrów – " +
                "zamiast wielu stanów i przejść (Idle, Walk, Run) jest jeden stan z płynną zmianą.\n\n" +
                "• <b>1D</b> (lewy robot) – parametr Speed i progi (thresholds): 0 → Idle, 1,5 → Walk, 4 → Run. " +
                "Dla Speed = 2,75 waga Walk = Run = 0,5.\n" +
                "• <b>2D Simple Directional</b> (prawy robot) – dwa parametry (MoveX, MoveY) i klipy rozmieszczone " +
                "na płaszczyźnie: przód, tył, w lewo, w prawo, środek – Idle.\n" +
                "• Inne typy: 2D Freeform Directional / Cartesian (wiele klipów w tym samym kierunku), Direct (wagi wprost).\n\n" +
                "Klipy w drzewie powinny mieć <b>tę samą długość cyklu</b> lub zgodne fazy (stopa stawiana w tym samym ułamku cyklu) – " +
                "inaczej nogi „plączą się” przy mieszaniu. Unity normalizuje czas klipów w drzewie.",
                "<b>Spacja</b> – prędkość auto / W-S   <b>WASD</b> – kierunek (robot 2D)", "", statusWidth: 560f);
            Floor();
            PrincipleCamera();

            var c1 = ClipFactory.NewController("A19_Blend1D");
            c1.AddParameter("Speed", AnimatorControllerParameterType.Float);
            BlendTree t1;
            var s1 = c1.CreateBlendTreeInController("Ruch", out t1, 0);
            t1.blendType = BlendTreeType.Simple1D;
            t1.blendParameter = "Speed";
            t1.useAutomaticThresholds = false;
            t1.AddChild(Clip("Stoj"), 0f);
            t1.AddChild(Clip("Chod"), 1.5f);
            t1.AddChild(Clip("Bieg"), 4f);
            c1.layers[0].stateMachine.defaultState = s1;

            var c2 = ClipFactory.NewController("A19_Blend2D");
            c2.AddParameter("MoveX", AnimatorControllerParameterType.Float);
            c2.AddParameter("MoveY", AnimatorControllerParameterType.Float);
            BlendTree t2;
            var s2 = c2.CreateBlendTreeInController("Ruch2D", out t2, 0);
            t2.blendType = BlendTreeType.SimpleDirectional2D;
            t2.blendParameter = "MoveX";
            t2.blendParameterY = "MoveY";
            t2.AddChild(Clip("Stoj"), Vector2.zero);
            t2.AddChild(Clip("Chod"), new Vector2(0f, 1f));
            t2.AddChild(Clip("Chod_tyl"), new Vector2(0f, -1f));
            t2.AddChild(Clip("Krok_w_lewo"), new Vector2(-1f, 0f));
            t2.AddChild(Clip("Krok_w_prawo"), new Vector2(1f, 0f));
            c2.layers[0].stateMachine.defaultState = s2;

            var r1 = Robot("Robot_1D", new Vector3(-3f, 0f, 0f), M_Blue, 150f);
            var r2 = Robot("Robot_2D", new Vector3(3f, 0f, 0f), M_Red, 210f);
            var demo = Manager<BlendTreeDemo>();
            demo.blend1D = r1.AddAnimator(c1);
            demo.blend2D = r2.AddAnimator(c2);
            CompareLabels("1D – Speed", "2D – MoveX, MoveY");
        }

        // =====================================================================
        // 20 – WARSTWY I MASKI
        // =====================================================================
        static void A20_Layers()
        {
            Begin("20 · Animator – warstwy i maski",
                "<b>Warstwy</b> (Layers) pozwalają odtwarzać kilka animacji jednocześnie na różnych częściach ciała " +
                "lub nakładać je na siebie. Warstwy liczone są od góry (Base Layer) do dołu.\n\n" +
                "• <b>Override</b> + <b>Avatar Mask</b> (robot 2): maska wybiera stawy, na które warstwa działa – tu tułów, " +
                "ręce i głowa. Nogi nadal chodzą z warstwy bazowej, a górna część ciała macha.\n" +
                "• <b>Additive</b> (robot 3): warstwa <b>dodaje</b> różnicę względem pozy odniesienia (pierwszej klatki klipu) – " +
                "kiwanie głową i tułowiem nakłada się na chód.\n" +
                "• <b>Weight</b> – waga warstwy 0…1 (SetLayerWeight), zmieniana płynnie.\n\n" +
                "Dla rigu Generic maskę tworzy się z listy ścieżek obiektów (Transform), dla Humanoid – zaznaczając części " +
                "sylwetki. Typowe użycie: strzelanie/przeładowanie górną częścią ciała podczas biegu.",
                "<b>Spacja</b> – waga warstwy 1: 0 ↔ 1   <b>LPM</b> – waga 0,5", "", statusWidth: 520f);
            Floor();
            PrincipleCamera();
            string[] labels = { "1. tylko Base Layer", "2. + Override z maską", "3. + Additive" };
            Material[] mats = { M_Blue, M_Green, M_Red };
            var layered = new List<Animator>();
            for (int i = 0; i < 3; i++)
            {
                float x = -3.5f + i * 3.5f;
                var r = Robot("Robot_" + (i + 1), new Vector3(x, 0f, 0f), mats[i]);
                var c = ClipFactory.NewController("A20_Warstwy_" + (i + 1));
                var sm = c.layers[0].stateMachine;
                sm.defaultState = ClipFactory.State(sm, "Chod", Clip("Chod"), 300, 100);
                if (i > 0)
                {
                    c.AddLayer(i == 1 ? "Gorna_czesc" : "Kiwanie");
                    var layers = c.layers;
                    layers[1].defaultWeight = 1f;
                    if (i == 1)
                    {
                        layers[1].blendingMode = AnimatorLayerBlendingMode.Override;
                        layers[1].avatarMask = ClipFactory.Mask("Gorna_czesc", r.root, "Biodra/Tulow");
                    }
                    else
                    {
                        layers[1].blendingMode = AnimatorLayerBlendingMode.Additive;
                    }
                    c.layers = layers;
                    var sm1 = c.layers[1].stateMachine;
                    sm1.defaultState = ClipFactory.State(sm1, i == 1 ? "Machanie" : "Kiwanie", Clip(i == 1 ? "Machanie" : "Kiwanie"), 300, 100);
                }
                var a = r.AddAnimator(c);
                if (i > 0) layered.Add(a);
                SectionLabel(labels[i], new Vector3(x, 0.05f, -1.6f), null, 16);
            }
            Manager<LayersDemo>().layered = layered.ToArray();
        }

        // =====================================================================
        // 21 – PODMASZYNA STANÓW I STATEMACHINEBEHAVIOUR
        // =====================================================================
        static void A21_SubStateMachine()
        {
            Begin("21 · Podmaszyna stanów i StateMachineBehaviour",
                "Rozbudowany graf porządkuje się w <b>podmaszyny stanów</b> (Sub-State Machine) – węzeł „Walka” zawiera " +
                "stany Atak1, Atak2, Atak3. Wejście do podmaszyny prowadzi do jej stanu domyślnego, wyjście (Exit) – " +
                "z powrotem do węzła Entry warstwy (tu: Idle albo Chod, zależnie od parametru Chodzi).\n\n" +
                "<b>Kombinacja ciosów</b>: przejście Atak1 → Atak2 ma warunek trigger Atak i <b>Has Exit Time 0,6</b> – " +
                "naciśnięcie w trakcie ciosu jest „zapamiętane” i kolejny cios zaczyna się pod koniec bieżącego.\n\n" +
                "<b>StateMachineBehaviour</b> to skrypt podpięty do stanu (Add Behaviour w Inspectorze stanu). " +
                "Metody OnStateEnter / OnStateUpdate / OnStateExit – tu StateLogger wypisuje wejścia i wyjścia w dzienniku.\n\n" +
                "<b>Tag</b> stanu („Atak”) pozwala w kodzie sprawdzić grupę stanów: stateInfo.IsTag(\"Atak\").",
                "<b>Spacja</b> – atak (wciskaj w rytm ciosów)   <b>LPM</b> – chód wł./wył.", "", statusWidth: 520f);
            Floor();
            Cam(new Vector3(-1.2f, 2.8f, -6.5f), new Vector3(0f, 1.1f, 0f));
            var c = ClipFactory.NewController("A21_Kombinacja");
            c.AddParameter("Atak", AnimatorControllerParameterType.Trigger);
            c.AddParameter("Chodzi", AnimatorControllerParameterType.Bool);
            var sm = c.layers[0].stateMachine;
            var idle = ClipFactory.State(sm, "Idle", Clip("Stoj"), 300, 60);
            var walk = ClipFactory.State(sm, "Chod", Clip("Chod"), 300, 200);
            sm.defaultState = idle;
            var entryWalk = sm.AddEntryTransition(walk);
            entryWalk.AddCondition(AnimatorConditionMode.If, 0f, "Chodzi");
            ClipFactory.Go(idle, walk, 0.2f, AnimatorConditionMode.If, 0f, "Chodzi");
            ClipFactory.Go(walk, idle, 0.2f, AnimatorConditionMode.IfNot, 0f, "Chodzi");

            var combat = sm.AddStateMachine("Walka", new Vector3(600, 130, 0));
            var a1 = ClipFactory.State(combat, "Atak1", Clip("Atak1"), 300, 60);
            var a2 = ClipFactory.State(combat, "Atak2", Clip("Atak2"), 300, 160);
            var a3 = ClipFactory.State(combat, "Atak3", Clip("Atak3"), 300, 260);
            combat.defaultState = a1;
            foreach (var st in new[] { a1, a2, a3 })
            {
                st.tag = "Atak";
                var log = st.AddStateMachineBehaviour<StateLogger>();
                log.label = st.name;
                EditorUtility.SetDirty(log);
            }
            foreach (var st in new[] { idle, walk })
            {
                var log = st.AddStateMachineBehaviour<StateLogger>();
                log.label = st.name;
                EditorUtility.SetDirty(log);
                var t = st.AddTransition(combat);
                t.AddCondition(AnimatorConditionMode.If, 0f, "Atak");
                t.hasExitTime = false;
                t.duration = 0.08f;
            }
            var t12 = a1.AddTransition(a2);
            t12.AddCondition(AnimatorConditionMode.If, 0f, "Atak");
            t12.hasExitTime = true; t12.exitTime = 0.6f; t12.duration = 0.1f;
            var t23 = a2.AddTransition(a3);
            t23.AddCondition(AnimatorConditionMode.If, 0f, "Atak");
            t23.hasExitTime = true; t23.exitTime = 0.6f; t23.duration = 0.1f;
            foreach (var st in new[] { a1, a2, a3 })
            {
                var ex = st.AddExitTransition();
                ex.hasExitTime = true;
                ex.exitTime = 0.95f;
                ex.duration = 0.15f;
            }

            var r = Robot("Robot", Vector3.zero, M_Purple, 200f);
            var demo = Manager<ComboDemo>();
            demo.animator = r.AddAnimator(c);
            Box("Manekin", new Vector3(-0.6f, 1f, -1.3f), new Vector3(0.5f, 2f, 0.5f), M_Wood);
        }

        // =====================================================================
        // 22 – ROOT MOTION
        // =====================================================================
        static void A22_RootMotion()
        {
            Begin("22 · Animator – Root Motion",
                "<b>Root motion</b>: przesunięcie postaci w świecie pochodzi z <b>animacji</b>, a nie ze skryptu. " +
                "Klip zawiera trajektorię korzenia (dla rigu bez Avatara – krzywe <b>MotionT / MotionQ</b>, tworzone opcją " +
                "„Generate Root Motion Curves”; dla modeli – węzeł Root Node).\n\n" +
                "Wszystkie roboty odtwarzają klipy z root motion:\n" +
                "1. <b>Apply Root Motion = wył.</b> – nogi idą, ale robot stoi w miejscu (ruch trzeba dodać w kodzie),\n" +
                "2. <b>Apply Root Motion = wł.</b> – Animator przesuwa obiekt o deltaPosition w każdej klatce; stopy nie ślizgają się,\n" +
                "3. <b>OnAnimatorMove</b> – skrypt sam stosuje deltaPosition (tu ×2 – stopy się ślizgają: tak NIE robić),\n" +
                "4. klip ze <b>skrętem</b> (MotionQ) – robot chodzi po okręgu.\n\n" +
                "Zaleta: ruch idealnie zgodny z animacją. Wada: mniejsza kontrola i responsywność sterowania – " +
                "w grach akcji ruch często liczy kod, a root motion stosuje się do ataków, przewrotów, cutscenek.",
                "<b>Spacja</b> – pozycje od nowa   <b>LPM</b> – speed 1 ↔ 0,5   <b>PPM</b> – Apply Root Motion robota 2",
                "", statusWidth: 520f);
            Floor();
            Cam(new Vector3(1.5f, 9f, -10f), new Vector3(1.5f, 0f, 3f));
            var cWalk = ClipFactory.Single("A22_Chod_RootMotion", Clip("Chod_RootMotion"));
            var cTurn = ClipFactory.Single("A22_Skret_RootMotion", Clip("Skret_RootMotion"));
            string[] names = { "Robot_1_bez_root_motion", "Robot_2_apply_root_motion", "Robot_3_OnAnimatorMove", "Robot_4_skret" };
            Material[] mats = { M_Blue, M_Green, M_Red, M_Purple };
            var robots = new List<Animator>();
            for (int i = 0; i < 4; i++)
            {
                float x = -4.5f + i * 2.4f;
                Vector3 start = i < 3 ? new Vector3(x, 0f, -2f) : new Vector3(5.5f, 0f, 0f);
                if (i < 3) Box("Pas_" + (i + 1), new Vector3(x, 0.01f, 5f), new Vector3(1.6f, 0.02f, 14.5f), M_Dark);
                var r = Robot(names[i], start, mats[i], 0f);
                var a = r.AddAnimator(i < 3 ? cWalk : cTurn);
                a.applyRootMotion = i != 0;
                if (i == 2) r.root.AddComponent<CustomRootMotion>().multiplier = 2f;
                robots.Add(a);
                Label(r.root, (i + 1).ToString(), 2.3f, 18);
            }
            var demo = Manager<RootMotionDemo>();
            demo.robots = robots.ToArray();
            demo.laneLength = 13.5f;
        }
    }
}
