using System.Collections.Generic;
using UnityEngine;

namespace Animacja.EditorTools
{
    /// <summary>Część I – 12 zasad animacji (Ollie Johnston, Frank Thomas, „The Illusion of Life”, 1981).</summary>
    public static partial class SceneBuilder
    {
        static void PrincipleCamera() => Cam(new Vector3(0f, 3.4f, -11f), new Vector3(0f, 1.6f, 0f));

        // =====================================================================
        // 01 – SQUASH & STRETCH
        // =====================================================================
        static void P01_SquashStretch()
        {
            Begin("01 · Zgniatanie i rozciąganie (squash & stretch)",
                "Najważniejsza zasada: obiekt zmienia kształt pod wpływem ruchu i sił, co nadaje mu <b>ciężar i elastyczność</b>.\n\n" +
                "Piłka po prawej:\n" +
                "• w locie <b>rozciąga się</b> wzdłuż kierunku ruchu – tym bardziej, im szybciej leci,\n" +
                "• przy uderzeniu o podłoże <b>spłaszcza się</b>,\n" +
                "• <b>objętość pozostaje stała</b>: gdy wysokość maleje, szerokość rośnie (sx = sz = 1 / √sy).\n\n" +
                "Piłka po lewej porusza się identycznie, ale jest sztywna – wygląda jak kula bilardowa, nie jak gumowa piłka.\n\n" +
                "Ta sama zasada dotyczy postaci: mimika, napięcie mięśni, przysiad przed skokiem.\n" +
                "Duszki (co 1/24 s) pokazują rozkład ruchu: gęsto w szczycie, rzadko przy ziemi.",
                "<b>Spacja</b> – od początku   " + PrincipleKeys, "", statusWidth: 420f);
            Floor();
            PrincipleCamera();
            Manager<SlowMotion>();
            CompareLabels("BEZ ZASADY – sztywna kula", "Z ZASADĄ – zgniatanie i rozciąganie");
            for (int i = 0; i < 2; i++)
            {
                var b = Ball(i == 0 ? "Pilka_sztywna" : "Pilka_elastyczna", new Vector3(i == 0 ? -3f : 3f, 0.5f, 0f), 1f, i == 0 ? M_Blue : M_Red);
                NoCollider(b);
                var bb = b.AddComponent<BouncingBall>();
                bb.squashAndStretch = i == 1;
                bb.height = 3.2f;
                bb.period = 1.1f;
                Ghosts(b, 1f / 24f, 26);
            }
        }

        // =====================================================================
        // 02 – ANTICIPATION
        // =====================================================================
        static void P02_Anticipation()
        {
            Begin("02 · Antycypacja (anticipation)",
                "<b>Antycypacja</b> przygotowuje widza na główną akcję: ruch w przeciwnym kierunku przed ruchem właściwym.\n\n" +
                "Przed skokiem postać kuca, odchyla ręce do tyłu i pochyla tułów – widz wie, że zaraz nastąpi skok, " +
                "a sam skok wygląda na wykonany <b>siłą mięśni</b>.\n\n" +
                "Robot po lewej skacze bez przygotowania – wybicie jest nagłe i nienaturalne, jakby uniosła go niewidzialna siła.\n\n" +
                "Przykłady w grach: zamach przed ciosem (gracz widzi, że przeciwnik zaraz uderzy – czas na unik), " +
                "przykucnięcie przed skokiem, cofnięcie ręki przed rzutem. Zbyt długa antycypacja ataku gracza " +
                "pogarsza responsywność sterowania – w grach skraca się ją dla gracza, a wydłuża dla przeciwników.",
                "<b>Spacja</b> – skok (trigger Akcja)   " + PrincipleKeys, "", statusWidth: 420f);
            Floor();
            PrincipleCamera();
            Manager<SlowMotion>();
            CompareLabels("BEZ ANTYCYPACJI", "Z ANTYCYPACJĄ");
            var c1 = ClipFactory.IdleAction("P02_Skok_bez", Clip("Stoj"), Clip("Skok_bez_antycypacji"));
            var c2 = ClipFactory.IdleAction("P02_Skok_z", Clip("Stoj"), Clip("Skok_z_antycypacja"));
            var r1 = Robot("Robot_bez", new Vector3(-3f, 0f, 0f), M_Blue);
            var r2 = Robot("Robot_z", new Vector3(3f, 0f, 0f), M_Red);
            var trig = Manager<AnimatorTriggerAll>();
            trig.animators = new[] { r1.AddAnimator(c1), r2.AddAnimator(c2) };
            trig.trigger = "Akcja";
            Trail(r1[J.Head].gameObject, new Color(0.4f, 0.6f, 1f), 1.6f);
            Trail(r2[J.Head].gameObject, new Color(1f, 0.5f, 0.4f), 1.6f);
        }

        // =====================================================================
        // 03 – STAGING
        // =====================================================================
        static void P03_Staging()
        {
            Begin("03 · Inscenizacja (staging)",
                "<b>Inscenizacja</b> to takie przedstawienie akcji, by widz od razu wiedział, <b>na co patrzeć</b> i co się dzieje.\n\n" +
                "Narzędzia inscenizacji:\n" +
                "• <b>kontrast</b> – postać wyróżnia się kolorem i jasnością od tła,\n" +
                "• <b>światło</b> prowadzi wzrok do miejsca akcji,\n" +
                "• <b>porządek w tle</b> – nic nie konkuruje o uwagę z główną akcją,\n" +
                "• <b>czytelna sylwetka</b> – akcję widać nawet w samym konturze (LPM: test sylwetki).\n\n" +
                "Jedna wyraźna akcja na raz: machający robot ma rękę z dala od tułowia, więc gest jest czytelny w sylwetce.\n\n" +
                "W grach: podświetlenie obiektów interaktywnych, kolor przeciwnika kontrastujący z otoczeniem, " +
                "wyraźne pozy ataków.",
                "<b>Spacja</b> – inscenizacja wł./wył.   <b>LPM</b> – test sylwetki", "", statusWidth: 420f);
            Floor();
            Cam(new Vector3(0f, 4.5f, -12f), new Vector3(0f, 1.2f, 0f));
            var light = Object.FindFirstObjectByType<Light>();
            var robot = Robot("Robot_machajacy", new Vector3(0f, 0f, 0f), M_Gray);
            robot.AddAnimator(ClipFactory.Single("P03_Machanie", Clip("Machanie")));

            var clutter = new List<Renderer>();
            var hide = new List<Transform>();
            var rnd = new System.Random(7);
            var g = Group("Tlo");
            for (int i = 0; i < 26; i++)
            {
                float x = -7f + (float)rnd.NextDouble() * 14f;
                float z = -2.5f + (float)rnd.NextDouble() * 7f;
                if (Mathf.Abs(x) < 1.3f && z < 1.5f) x += x < 0 ? -1.5f : 1.5f;
                float h = 0.5f + (float)rnd.NextDouble() * 2.2f;
                var go = i % 3 == 0
                    ? Prim(PrimitiveType.Cylinder, $"Obiekt_{i}", new Vector3(x, h / 2f, z), new Vector3(0.6f, h / 2f, 0.6f), M_Gray, g)
                    : Box($"Obiekt_{i}", new Vector3(x, h / 2f, z), new Vector3(0.7f, h, 0.7f), M_Gray, g, Quaternion.Euler(0, (float)rnd.NextDouble() * 90f, 0));
                clutter.Add(go.GetComponent<Renderer>());
                if (i % 2 == 0 || (Mathf.Abs(x) < 3f && z < 2f)) hide.Add(go.transform);
            }
            var spotGo = new GameObject("Reflektor");
            spotGo.transform.position = new Vector3(0f, 7f, -3f);
            spotGo.transform.LookAt(new Vector3(0f, 1f, 0f));
            var spot = spotGo.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.spotAngle = 32f;
            spot.range = 20f;
            spot.intensity = 60f;
            spot.color = new Color(1f, 0.95f, 0.85f);
            spot.shadows = LightShadows.Soft;

            var demo = Manager<StagingDemo>();
            demo.characterRenderers = robot.bodyRenderers.ToArray();
            demo.characterPlain = M_Gray;
            demo.characterAccent = M_Orange;
            demo.clutter = clutter.ToArray();
            demo.clutterBusy = M_Gray;
            demo.clutterCalm = Mat("Tlo_wyciszone", new Color(0.3f, 0.32f, 0.36f));
            demo.clutterToHide = hide.ToArray();
            demo.spot = spot;
            demo.mainLight = light;
            demo.silhouetteMaterial = UnlitMat("Sylwetka", new Color(0.05f, 0.05f, 0.06f));
        }

        // =====================================================================
        // 04 – STRAIGHT AHEAD / POSE TO POSE
        // =====================================================================
        static void P04_StraightAhead()
        {
            Begin("04 · Akcja „na wprost” i „poza do pozy”",
                "Dwie metody tworzenia ruchu:\n\n" +
                "<b>Poza do pozy</b> (pose to pose) – animator najpierw ustala <b>pozy kluczowe</b> (półprzezroczyste roboty " +
                "w tle), a potem wypełnia odstępy (in-betweens). W animacji komputerowej klatki pośrednie liczy " +
                "<b>interpolacja</b> – tak działają klatki kluczowe w Unity. Pełna kontrola nad pozami i czasem – metoda " +
                "podstawowa w grach.\n\n" +
                "<b>Na wprost</b> (straight ahead action) – rysowanie klatka po klatce od początku do końca, każda klatka " +
                "wynika z poprzedniej. Ruch jest żywy i nieprzewidywalny (ogień, woda, dym, włosy). W grach odpowiednikiem " +
                "jest <b>symulacja</b> liczona w każdej klatce – tu macka/płomień po prawej.\n\n" +
                "W praktyce łączy się obie metody: pozy kluczowe dla postaci, symulacja dla elementów wtórnych.",
                "" + PrincipleKeys, "", statusWidth: 420f);
            Floor();
            Cam(new Vector3(0f, 3.6f, -11.5f), new Vector3(0f, 1.6f, 1f));
            Manager<SlowMotion>();
            CompareLabels("POZA DO POZY – klatki kluczowe", "NA WPROST – klatka po klatce", 3.2f);
            var r = Robot("Robot_poza_do_pozy", new Vector3(-3.2f, 0f, 0f), M_Blue);
            r.AddAnimator(ClipFactory.Single("P04_Poza_do_pozy", Clip("Poza_do_pozy")));
            var keys = Poses.KeyPoses();
            for (int i = 0; i < keys.Length; i++)
            {
                var gh = Robot($"Poza_kluczowa_{i + 1}", new Vector3(-6.2f + i * 2f, 0f, 3.2f), M_GhostRobot);
                foreach (var rr in gh.root.GetComponentsInChildren<Renderer>()) rr.sharedMaterial = M_GhostRobot;
                gh.ApplyPose(keys[i]);
                Label(gh.root, $"klucz {i + 1} (t = {i} s)", 2.3f, 13);
            }

            // macka – łańcuch segmentów
            var baseGo = Prim(PrimitiveType.Cylinder, "Podstawa_macki", new Vector3(3.2f, 0.15f, 0f), new Vector3(1.2f, 0.15f, 1.2f), M_Dark);
            NoCollider(baseGo);
            Transform parent = baseGo.transform;
            var segs = new List<Transform>();
            Vector3 p = new Vector3(3.2f, 0.3f, 0f);
            for (int i = 0; i < 10; i++)
            {
                var joint = new GameObject($"Segment_{i + 1}").transform;
                joint.SetParent(i == 0 ? null : segs[i - 1], true);
                joint.position = p;
                float w = Mathf.Lerp(0.5f, 0.12f, i / 9f);
                var m = Prim(PrimitiveType.Capsule, "Siatka", p + Vector3.up * 0.18f, new Vector3(w, 0.2f, w), i % 2 == 0 ? M_Orange : M_Red, joint);
                NoCollider(m);
                segs.Add(joint);
                p += Vector3.up * 0.36f;
            }
            var chain = segs[0].gameObject.AddComponent<StraightAheadChain>();
            chain.segments = segs.ToArray();
            Trail(segs[segs.Count - 1].GetChild(0).gameObject, new Color(1f, 0.6f, 0.2f), 1.2f);
        }

        // =====================================================================
        // 05 – FOLLOW THROUGH / OVERLAPPING ACTION
        // =====================================================================
        static void P05_FollowThrough()
        {
            Begin("05 · Kontynuacja i nakładanie się ruchów",
                "<b>Kontynuacja</b> (follow through): gdy ciało się zatrzymuje, jego luźne części (włosy, ubranie, ogon, antena) " +
                "jeszcze przez chwilę <b>poruszają się dalej</b> siłą bezwładności, a potem wracają.\n\n" +
                "<b>Nakładanie się ruchów</b> (overlapping action): różne części ciała poruszają się <b>z różnym opóźnieniem</b> – " +
                "segment bliżej ciała reaguje pierwszy, końcówka ostatnia. Nic nie zatrzymuje się w tej samej klatce.\n\n" +
                "Obie postacie ruszają z przyspieszeniem i stają gwałtownie. Po lewej antena jest sztywna – postać wygląda " +
                "jak zabawka z plastiku. Po prawej antena to łańcuch tłumionych sprężyn (SpringChain): " +
                "przy ruszaniu odchyla się do tyłu, przy zatrzymaniu – wychyla do przodu i drga.\n\n" +
                "W grach: fizyka „kości” (jiggle bones), łańcuchy sprężyn, cloth.",
                "<b>Spacja</b> – od początku   " + PrincipleKeys, "", statusWidth: 420f);
            Floor();
            Cam(new Vector3(0f, 4f, -12f), new Vector3(0f, 1.4f, 0.5f));
            Manager<SlowMotion>();
            SectionLabel("BEZ – sztywna antena", new Vector3(-6.5f, 3.4f, -1.5f));
            SectionLabel("Z ZASADĄ – antena na sprężynach", new Vector3(-6.5f, 0.1f, -1.5f));
            for (int i = 0; i < 2; i++)
            {
                float z = i == 0 ? 2.2f : -0.8f;
                float y = i == 0 ? 1.2f : 0f;
                var body = Box(i == 0 ? "Postac_sztywna" : "Postac_sprezysta", new Vector3(-4f, 0.6f + y, z), new Vector3(1.2f, 1.2f, 1.2f), i == 0 ? M_Blue : M_Red);
                NoCollider(body);
                if (i == 0) Box("Polka", new Vector3(0f, y, z), new Vector3(14f, 0.1f, 2f), M_Wood);
                var mover = body.AddComponent<MoveStopCycle>();
                mover.distance = 7f;
                var segs = new List<Transform>();
                Vector3 p = body.transform.position + new Vector3(0f, 0.6f, 0f);
                for (int s = 0; s < 6; s++)
                {
                    var j = new GameObject($"Antena_{s + 1}").transform;
                    j.SetParent(s == 0 ? body.transform : segs[s - 1], true);
                    j.position = p;
                    var m = Prim(PrimitiveType.Cylinder, "Siatka", p + Vector3.up * 0.15f, new Vector3(0.08f, 0.15f, 0.08f), M_Dark, j);
                    NoCollider(m);
                    segs.Add(j);
                    p += Vector3.up * 0.3f;
                }
                var tip = Ball("Kulka", p, 0.25f, M_Yellow, segs[segs.Count - 1]);
                NoCollider(tip);
                var chain = body.AddComponent<SpringChain>();
                chain.segments = segs.ToArray();
                chain.driver = mover;
                chain.rigid = i == 0;
                Trail(tip, i == 0 ? new Color(0.4f, 0.6f, 1f) : new Color(1f, 0.85f, 0.3f), 0.8f, 0.04f);
            }
        }

        // =====================================================================
        // 06 – SLOW IN / SLOW OUT
        // =====================================================================
        static void P06_SlowInOut()
        {
            Begin("06 · Spowolnienie i przyspieszenie (slow in, slow out)",
                "Obiekty fizyczne nie ruszają i nie stają w miejscu z pełną prędkością: <b>przyspieszają</b> na starcie " +
                "i <b>zwalniają</b> przed zatrzymaniem. W animacji oznacza to więcej klatek blisko póz kluczowych, " +
                "a mniej w środku ruchu.\n\n" +
                "Duszki (co 1/12 s) pokazują <b>rozkład klatek</b> (spacing):\n" +
                "• <b>Linear</b> – równe odstępy, ruch mechaniczny (robot, taśmociąg),\n" +
                "• <b>Ease In</b> – rusza powoli, staje gwałtownie,\n" +
                "• <b>Ease Out</b> – rusza gwałtownie, staje łagodnie,\n" +
                "• <b>Ease In-Out</b> – slow in i slow out: naturalny ruch,\n" +
                "• <b>Out Back</b> – przestrzelenie celu i powrót (sprężystość).\n\n" +
                "W Unity: styczne kluczy w krzywej animacji (Auto / Clamped Auto ≈ ease in-out), AnimationCurve, " +
                "Mathf.SmoothStep, Mathf.SmoothDamp, funkcje easing.",
                "<b>Spacja</b> – od początku   " + PrincipleKeys, "", statusWidth: 420f);
            Floor();
            Cam(new Vector3(0f, 7.5f, -10.5f), new Vector3(0f, 0f, 1.2f));
            Manager<SlowMotion>();
            var types = new[] { Ease.Type.Linear, Ease.Type.InQuad, Ease.Type.OutQuad, Ease.Type.InOutCubic, Ease.Type.OutBack };
            string[] names = { "Linear", "Ease In", "Ease Out", "Ease In-Out", "Out Back" };
            for (int i = 0; i < types.Length; i++)
            {
                float z = 4f - i * 1.6f;
                Box($"Tor_{names[i]}", new Vector3(0f, 0.02f, z), new Vector3(10f, 0.04f, 0.9f), M_Dark);
                SectionLabel(names[i], new Vector3(-6.3f, 0.3f, z), null, 16);
                var b = Ball($"Kula_{names[i]}", new Vector3(-4.5f, 0.45f, z), 0.7f, Palette[i]);
                NoCollider(b);
                var lane = b.AddComponent<EaseLane>();
                lane.from = new Vector3(-4.5f, 0.45f, z);
                lane.to = new Vector3(4.5f, 0.45f, z);
                lane.ease = types[i];
                lane.travelTime = 1.6f;
                lane.hold = 0.6f;
                Ghosts(b, 1f / 12f, 20);
            }
        }

        // =====================================================================
        // 07 – ARCS
        // =====================================================================
        static void P07_Arcs()
        {
            Begin("07 · Łuki (arcs)",
                "Większość naturalnych ruchów przebiega po <b>łukach</b>: rzucona piłka leci po paraboli, ręka i noga obracają się " +
                "wokół stawów, głowa przy obrocie lekko opada i wraca.\n\n" +
                "Ruch po linii prostej z punktu A do B wygląda <b>mechanicznie</b> (dobry dla robota przemysłowego, " +
                "zły dla postaci).\n\n" +
                "Ślady pokazują tor: na górze piłka interpolowana liniowo między A i B, na dole – po łuku. " +
                "Robot macha ręką – dłoń sama rysuje łuk, bo ramię jest obracane w stawie (hierarchia transformacji).\n\n" +
                "Pułapka w Unity: interpolacja <b>pozycji</b> między dwoma kluczami daje linię prostą. Żeby uzyskać łuk, " +
                "animuj <b>obrót</b> w stawie albo dodaj klucz pośredni / styczne.",
                "<b>Spacja</b> – od początku   " + PrincipleKeys, "", statusWidth: 420f);
            Floor();
            Cam(new Vector3(0f, 4f, -12f), new Vector3(0f, 2f, 0f));
            Manager<SlowMotion>();
            for (int i = 0; i < 2; i++)
            {
                float z = i == 0 ? 3f : 0f;
                var b = Ball(i == 0 ? "Pilka_linia_prosta" : "Pilka_luk", new Vector3(-4f, 0.4f, z), 0.6f, i == 0 ? M_Blue : M_Red);
                NoCollider(b);
                var m = b.AddComponent<ArcMover>();
                m.from = new Vector3(-4.5f, 0.4f, z);
                m.to = new Vector3(2f, 0.4f, z);
                m.useArc = i == 1;
                m.arcHeight = 3f;
                Trail(b, i == 0 ? new Color(0.4f, 0.6f, 1f) : new Color(1f, 0.5f, 0.4f), 2.5f);
                SectionLabel(i == 0 ? "BEZ – linia prosta" : "Z ZASADĄ – łuk", new Vector3(-6.2f, 0.2f, z), null, 16);
            }
            var r = Robot("Robot_machajacy", new Vector3(4.5f, 0f, 1f), M_Green);
            r.AddAnimator(ClipFactory.Single("P07_Machanie", Clip("Machanie")));
            Trail(r[J.HandR].gameObject, new Color(0.4f, 1f, 0.5f), 1.2f, 0.05f);
            Label(r.root, "łuk dłoni – obrót w stawie", 2.6f, 14);
        }

        // =====================================================================
        // 08 – SECONDARY ACTION
        // =====================================================================
        static void P08_Secondary()
        {
            Begin("08 · Działanie drugoplanowe (secondary action)",
                "<b>Działanie drugoplanowe</b> to dodatkowy ruch, który <b>wspiera</b> główną akcję i dodaje postaci charakteru, " +
                "ale jej nie zasłania.\n\n" +
                "Oba roboty idą w miejscu (akcja główna: chód).\n" +
                "• Po lewej – sam chód: nogi pracują, reszta ciała jest nieruchoma.\n" +
                "• Po prawej – chód z ruchami drugoplanowymi: zamach rąk, skręt tułowia, kołysanie bioder " +
                "oraz <b>głowa śledząca motyla</b> (skrypt LookAtTarget w LateUpdate nadpisuje pozę z Animatora).\n\n" +
                "Działanie drugoplanowe może być animacją w klipie, osobną warstwą Animatora albo kodem " +
                "(patrzenie na cel, oddech, mruganie).\n\n" +
                "Zasada: ruch drugoplanowy jest mniejszy i podporządkowany głównemu.",
                "" + PrincipleKeys, "", statusWidth: 420f);
            Floor();
            PrincipleCamera();
            Manager<SlowMotion>();
            CompareLabels("BEZ – sam chód", "Z ZASADĄ – ruchy drugoplanowe");
            var r1 = Robot("Robot_sztywny", new Vector3(-3f, 0f, 0f), M_Blue);
            r1.AddAnimator(ClipFactory.Single("P08_Chod_sztywny", Clip("Chod_sztywny")));
            var r2 = Robot("Robot_zywy", new Vector3(3f, 0f, 0f), M_Red);
            r2.AddAnimator(ClipFactory.Single("P08_Chod", Clip("Chod")));
            var butterfly = Box("Motyl", new Vector3(3f, 2.6f, -1.5f), new Vector3(0.35f, 0.04f, 0.2f), M_Yellow);
            NoCollider(butterfly);
            var orbit = butterfly.AddComponent<OrbitMover>();
            orbit.center = new Vector3(3f, 2.4f, -0.5f);
            orbit.radius = 2.2f;
            orbit.speed = 0.9f;
            var look = r2.root.AddComponent<LookAtTarget>();
            look.head = r2[J.Head];
            look.target = butterfly.transform;
        }

        // =====================================================================
        // 09 – TIMING
        // =====================================================================
        static void P09_Timing()
        {
            Begin("09 · Czas (timing)",
                "<b>Liczba klatek</b> (czas) przeznaczona na ruch decyduje o tym, jak widz odbiera <b>masę, siłę i nastrój</b>.\n\n" +
                "Cztery obiekty spadają z tej samej wysokości:\n" +
                "• <b>kula do kręgli</b> – szybki spadek, prawie bez odbicia: ciężka,\n" +
                "• <b>piłka</b> – kilka odbić o malejącej wysokości: sprężysta,\n" +
                "• <b>balon</b> – duży opór powietrza, powolne opadanie: lekki,\n" +
                "• <b>piłka 12 kl./s</b> – ten sam ruch co piłka, ale obraz zmienia się co 1/12 s " +
                "(animacja „na dwójkach”): ruch staje się skokowy, stylizowany.\n\n" +
                "Na dole: ten sam przesuw w 0,25 s, 0,5 s i 1 s – szybki ruch jest gwałtowny i energiczny, wolny – spokojny lub ciężki.\n\n" +
                "W Unity czas zmieniasz rozsuwając klucze na osi czasu, parametrem Speed stanu w Animatorze lub Animator.speed.",
                "<b>Spacja</b> – od początku   " + PrincipleKeys, "", statusWidth: 420f);
            Floor();
            Cam(new Vector3(0f, 5f, -12.5f), new Vector3(0f, 2f, 0.5f));
            Manager<SlowMotion>();
            string[] names = { "Kula do kręgli", "Piłka", "Balon", "Piłka 12 kl./s" };
            float[] gravity = { 14f, 9.81f, 9.81f, 9.81f };
            float[] rest = { 0.15f, 0.7f, 0.35f, 0.7f };
            float[] drag = { 0f, 0f, 2.6f, 0f };
            float[] size = { 0.8f, 0.6f, 1.0f, 0.6f };
            float[] fps = { 0f, 0f, 0f, 12f };
            Material[] mats = { M_Dark, M_Red, M_Purple, M_Orange };
            for (int i = 0; i < 4; i++)
            {
                float x = -4.5f + i * 3f;
                var b = Ball($"Obiekt_{i + 1}", new Vector3(x, 4f, 2f), size[i], mats[i]);
                NoCollider(b);
                var f = b.AddComponent<FallingObject>();
                f.startHeight = 4f;
                f.gravity = gravity[i];
                f.restitution = rest[i];
                f.drag = drag[i];
                f.radius = size[i] * 0.5f;
                f.stepFps = fps[i];
                f.restTime = 1.5f;
                Ghosts(b, 1f / 12f, 12);
                SectionLabel(names[i], new Vector3(x, 0.1f, 0.8f), null, 15);
            }
            float[] times = { 0.25f, 0.5f, 1f };
            for (int i = 0; i < 3; i++)
            {
                float z = -1.6f - i * 1.1f;
                var c = Box($"Klocek_{times[i]}s", new Vector3(-4f, 0.3f, z), new Vector3(0.6f, 0.6f, 0.6f), Palette[i + 3]);
                NoCollider(c);
                var lane = c.AddComponent<EaseLane>();
                lane.from = new Vector3(-4f, 0.3f, z);
                lane.to = new Vector3(4f, 0.3f, z);
                lane.travelTime = times[i];
                lane.hold = 1.2f;
                lane.ease = Ease.Type.InOutQuad;
                SectionLabel($"{times[i]} s", new Vector3(-5.6f, 0.1f, z), null, 14);
            }
        }

        // =====================================================================
        // 10 – EXAGGERATION
        // =====================================================================
        static void P10_Exaggeration()
        {
            Begin("10 · Przesada (exaggeration)",
                "<b>Przesada</b> wzmacnia to, co w ruchu najważniejsze – pozy są bardziej skrajne, akcje bardziej wyraziste. " +
                "Nie chodzi o deformację wszystkiego, tylko o podkreślenie idei ruchu.\n\n" +
                "Trzy wersje tego samego skoku (ten sam kod pozy, inny parametr przesady k):\n" +
                "• <b>k = 0,35</b> – realistyczny, ale „bez życia”,\n" +
                "• <b>k = 1</b> – wyraźny przysiad i zamach rąk,\n" +
                "• <b>k = 1,8</b> – głęboki przysiad, wysoki skok, tułów <b>rozciąga się</b> przy wybiciu " +
                "i <b>zgniata</b> przy lądowaniu (połączenie z zasadą 1).\n\n" +
                "Stopień przesady zależy od stylu gry: kreskówkowe platformówki przesadzają mocno, gry realistyczne – subtelnie, " +
                "ale nawet tam ruchy są nieco mocniejsze niż w nagraniach motion capture, żeby były czytelne z daleka.",
                "<b>Spacja</b> – skok (trigger Akcja)   " + PrincipleKeys, "", statusWidth: 420f);
            Floor();
            PrincipleCamera();
            Manager<SlowMotion>();
            string[] ks = { "0", "1", "2" };
            string[] labels = { "k = 0,35", "k = 1", "k = 1,8" };
            Material[] mats = { M_Blue, M_Green, M_Red };
            var anims = new List<Animator>();
            for (int i = 0; i < 3; i++)
            {
                float x = -3.5f + i * 3.5f;
                var c = ClipFactory.IdleAction($"P10_Przesada_{ks[i]}", Clip("Stoj"), Clip("Skok_przesada_" + ks[i]));
                var r = Robot($"Robot_{labels[i]}", new Vector3(x, 0f, 0f), mats[i]);
                anims.Add(r.AddAnimator(c));
                Trail(r[J.Head].gameObject, mats[i].color, 1.6f);
                SectionLabel(labels[i], new Vector3(x, 0.05f, -1.6f));
            }
            var trig = Manager<AnimatorTriggerAll>();
            trig.animators = anims.ToArray();
            trig.trigger = "Akcja";
        }

        // =====================================================================
        // 11 – SOLID DRAWING
        // =====================================================================
        static void P11_SolidDrawing()
        {
            Begin("11 · Solidne rysowanie (solid drawing)",
                "Postać ma być <b>bryłą w przestrzeni</b>: z wiarygodną objętością, ciężarem i równowagą, czytelną z każdej strony. " +
                "W animacji 3D model zapewnia objętość, ale nadal trzeba dbać o <b>pozę</b>.\n\n" +
                "• Po lewej – <b>twinning</b> (bliźniaczość): lewa i prawa strona ciała robią to samo. Poza jest symetryczna, " +
                "sztywna i płaska.\n" +
                "• Po prawej – <b>kontrapost</b>: ciężar na jednej nodze, biodra i barki pochylone przeciwnie, " +
                "głowa skręcona, ręce w różnych pozycjach. Poza ma rytm, równowagę i wygląda naturalnie z każdej strony.\n\n" +
                "Obrotowe podstawy pokazują postacie ze wszystkich stron – solidna poza jest czytelna z każdego kąta, " +
                "co w grach 3D ma szczególne znaczenie (gracz ogląda postać z dowolnej strony).",
                "<b>Spacja</b> – obrót wł./wył.", "", statusWidth: 420f);
            Floor();
            PrincipleCamera();
            Manager<TurntableToggle>();
            CompareLabels("BEZ – symetryczna poza (twinning)", "Z ZASADĄ – kontrapost");
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -3f : 3f;
                var table = new GameObject("Podstawa_obrotowa");
                table.transform.position = new Vector3(x, 0f, 0f);
                var disc = Prim(PrimitiveType.Cylinder, "Dysk", new Vector3(x, 0.05f, 0f), new Vector3(2.4f, 0.05f, 2.4f), M_Dark, table.transform);
                NoCollider(disc);
                var r = Robot(i == 0 ? "Robot_twinning" : "Robot_kontrapost", new Vector3(x, 0.1f, 0f), i == 0 ? M_Blue : M_Red, 180f, table.transform);
                r.ApplyPose(i == 0 ? Poses.Twinning() : Poses.Contrapposto());
                table.AddComponent<Turntable>().degreesPerSecond = 30f;
            }
        }

        // =====================================================================
        // 12 – APPEAL
        // =====================================================================
        static void P12_Appeal()
        {
            Begin("12 · Atrakcyjność (appeal)",
                "<b>Atrakcyjność</b> to cecha, dzięki której chce się na postać patrzeć – nie chodzi o „ładność”, " +
                "lecz o <b>charakter</b>, czytelny projekt i osobowość widoczną w ruchu (także u złoczyńców).\n\n" +
                "Robot po lewej: szary, proporcje „przeciętne”, brak oczu, ruch minimalny.\n" +
                "Robot po prawej:\n" +
                "• <b>wyraziste proporcje</b> – większa głowa, różne wielkości kształtów,\n" +
                "• <b>kolor</b> i kontrast,\n" +
                "• <b>oczy i mruganie</b> (skrypt Blink) – widz szuka twarzy,\n" +
                "• <b>antena na sprężynach</b> – reaguje na każdy ruch,\n" +
                "• skok z <b>przesadą</b> i rozciąganiem (zasady 1 i 10).\n\n" +
                "Atrakcyjność łączy wszystkie pozostałe zasady i projekt postaci.",
                "<b>Spacja</b> – reakcja (trigger Akcja)   " + PrincipleKeys, "", statusWidth: 420f);
            Floor();
            PrincipleCamera();
            Manager<SlowMotion>();
            CompareLabels("BEZ – postać bez charakteru", "Z ZASADĄ – postać z charakterem");
            var bland = Robot("Robot_nijaki", new Vector3(-3f, 0f, 0f), M_Gray);
            bland.root.transform.Find("Biodra/Tulow/Glowa/Wizjer").GetComponent<Renderer>().sharedMaterial = M_Gray;
            var a1 = bland.AddAnimator(ClipFactory.IdleAction("P12_Nijaki", Clip("Stoj"), Clip("Skok_przesada_0")));

            var nice = Robot("Robot_z_charakterem", new Vector3(3f, 0f, 0f), M_Orange);
            var head = nice[J.Head];
            nice.headMesh.localScale = Vector3.one * 0.62f;
            nice.headMesh.localPosition = new Vector3(0f, 0.3f, 0f);
            Object.DestroyImmediate(head.Find("Wizjer").gameObject);
            var lids = new List<Transform>();
            for (int s = -1; s <= 1; s += 2)
            {
                var eye = Ball(s < 0 ? "OkoL" : "OkoP", head.position + new Vector3(0.12f * s, 0.36f, -0.27f), 0.17f, M_White, head);
                NoCollider(eye);
                var pupil = Ball("Zrenica", eye.transform.position + new Vector3(0f, -0.01f, -0.07f), 0.08f, M_Dark, eye.transform);
                NoCollider(pupil);
                var lid = Ball("Powieka", eye.transform.position + new Vector3(0f, 0f, -0.01f), 0.18f, M_Orange, head);
                NoCollider(lid);
                lid.transform.localScale = new Vector3(lid.transform.localScale.x, lid.transform.localScale.x * 0.05f, lid.transform.localScale.z);
                lids.Add(lid.transform);
            }
            var blink = nice.root.AddComponent<Blink>();
            blink.eyelids = lids.ToArray();
            var segs = new List<Transform>();
            Vector3 p = head.position + new Vector3(0f, 0.6f, 0f);
            for (int s = 0; s < 4; s++)
            {
                var j = new GameObject($"Antena_{s + 1}").transform;
                j.SetParent(s == 0 ? head : segs[s - 1], true);
                j.position = p;
                var m = Prim(PrimitiveType.Cylinder, "Siatka", p + Vector3.up * 0.08f, new Vector3(0.05f, 0.08f, 0.05f), M_Dark, j);
                NoCollider(m);
                segs.Add(j);
                p += Vector3.up * 0.16f;
            }
            var tip = Ball("Kulka", p, 0.16f, M_Yellow, segs[segs.Count - 1]);
            NoCollider(tip);
            var chain = head.gameObject.AddComponent<SpringChain>();
            chain.segments = segs.ToArray();
            chain.bendAxis = Vector3.forward;
            chain.inertia = 25f;
            chain.verticalInfluence = 1f;
            var a2 = nice.AddAnimator(ClipFactory.IdleAction("P12_Z_charakterem", Clip("Stoj"), Clip("Skok_przesada_2")));
            var trig = Manager<AnimatorTriggerAll>();
            trig.animators = new[] { a1, a2 };
            trig.trigger = "Akcja";
        }
    }
}
