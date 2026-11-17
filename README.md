# Animacja w Unity – sceny demonstracyjne

Projekt Unity **6.3 LTS (6000.3.24f1)** do wykładu 6 „Serializacja i animacja w Unity”. Trzy części:

1. **12 zasad animacji** (Johnston, Thomas – *The Illusion of Life*) – każda zasada w osobnej scenie, zwykle jako porównanie „bez zasady / z zasadą”,
2. **animacja od zera** – klatki kluczowe i krzywe, ścieżki i właściwości w klipie, komponent Animation (legacy) i WrapMode, zdarzenia animacji,
3. **Animator** – maszyna stanów z parametrami, ustawienia przejść, Blend Tree 1D i 2D, warstwy i maski, podmaszyna stanów i StateMachineBehaviour, root motion.

Postać to **robot zbudowany z prymitywów** (hierarchia stawów: Biodra → Tulow → Glowa, RamieL/P → PrzedramieL/P → DlonL/P, UdoL/P → GolenL/P → StopaL/P). Wszystkie **klipy animacji, Animator Controllery i maski są tworzone kodem** (`Assets/Animacja/Editor`) – można je otworzyć w oknach **Animation** i **Animator** i zobaczyć klucze, krzywe, stany i przejścia. Projekt nie zawiera zewnętrznych modeli, grafik ani dźwięków.

- Render pipeline: **URP** (ustawienia z szablonu „Universal 3D”)
- Pakiety: **Input System**, **uGUI** – bez dodatkowych pakietów animacji
- Wejście: wyłącznie akcje z domyślnego pliku `Assets/InputSystem_Actions.inputactions` (Project-wide Actions)

---

## 1. Szybki start

1. Otwórz projekt w Unity Hub (wersja **6000.3.24f1**) albo uruchom `Narzedzia/Otworz_projekt_w_Unity.bat`.
2. Sceny są **generowane kodem**. Przy pierwszym otwarciu pojawi się pytanie „Wygenerować teraz 22 scen?” – wybierz **Generuj** (najpierw powstają klipy w `Assets/Animacja/Data/Klipy`). Można to zrobić także później: menu **Animacja → Generuj wszystkie sceny**.
3. **Animacja → Otwórz pierwszą scenę i uruchom Play**.
4. Klawiszami **1 / 2** przechodzisz do poprzedniej / następnej sceny.

> Jeżeli klawisze nie działają – **Animacja → Napraw ustawienia projektu** i uruchom ponownie edytor.
> W oknie **Game** ustaw proporcje 16:9 (np. *Full HD 1920×1080*).
> Podgląd grafu na żywo: zaznacz robota w Hierarchy w trakcie Play i otwórz **Window → Animation → Animator**.

### Build Windows (opcjonalnie)
`Narzedzia/Generuj_sceny_i_zbuduj_Windows.bat` (edytor z tym projektem musi być zamknięty) – generuje sceny i buduje `Builds/Windows/AnimacjaDemo.exe`.

---

## 2. Menu „Animacja” w edytorze

| Polecenie | Działanie |
|---|---|
| **Generuj wszystkie sceny** | Tworzy klipy, kontrolery, maski i 22 sceny, ustawia Build Settings |
| **Panel scen** | Okno z listą scen: *Otwórz*, ▶ (otwórz i Play), ↻ (wygeneruj tylko tę scenę) |
| **Otwórz pierwszą scenę i uruchom Play** | Skrót do pokazu |
| **Zbuduj wersję Windows (x64)** | Build do `Builds/Windows/AnimacjaDemo.exe` |
| **Napraw ustawienia projektu** | Active Input Handling = Input System, Project-wide Actions, Force Text, Build Settings |
| **Otwórz instrukcję (README.md)** | Otwiera ten plik |

---

## 3. Sterowanie

| Klawisz / mysz | Działanie |
|---|---|
| **Spacja** | akcja główna sceny (od początku, trigger, przełączenie) |
| **LPM** | akcja dodatkowa (w scenach 01–12: zwolnione tempo ×0,25) |
| **PPM** | trzecia akcja (w scenach 01–12: ślady wł./wył.) |
| **WASD / strzałki** | prędkość i kierunek (sceny 17, 19) |
| **C** | pokaż / ukryj panel opisu |
| **przytrzymaj E** | reset sceny |
| **1 / 2** | poprzednia / następna scena |
| **Esc** | wyjście (w buildzie) |

---

## 4. Opis scen

### Część I – 12 zasad animacji

| Scena | Co pokazuje |
|---|---|
| **01_Zgniatanie_i_rozciaganie** | piłka sztywna vs elastyczna (rozciąganie w locie, zgniatanie przy uderzeniu, stała objętość), duszki co 1/24 s |
| **02_Antycypacja** | skok bez przygotowania vs z przysiadem i zamachem rąk (dwa klipy, trigger) |
| **03_Inscenizacja** | kontrast, światło, porządek w tle; test sylwetki (LPM) |
| **04_Na_wprost_i_poza_do_pozy** | klip z 4 pozami kluczowymi (duszki póz) vs łańcuch liczony klatka po klatce |
| **05_Kontynuacja_i_nakladanie** | gwałtowne zatrzymanie: antena sztywna vs łańcuch tłumionych sprężyn |
| **06_Spowolnienie_i_przyspieszenie** | Linear, Ease In, Ease Out, Ease In-Out, Out Back – rozkład klatek (spacing) |
| **07_Luki** | ruch po linii prostej vs po łuku; łuk dłoni przy obrocie w stawie |
| **08_Dzialanie_drugoplanowe** | sam chód vs chód z ruchami rąk, tułowia i głową śledzącą motyla (LateUpdate) |
| **09_Czas_timing** | kula do kręgli / piłka / balon / piłka 12 kl./s; ten sam przesuw w 0,25 / 0,5 / 1 s |
| **10_Przesada** | ten sam skok z parametrem przesady 0,35 / 1 / 1,8 (rozciąganie i zgniatanie tułowia) |
| **11_Solidne_rysowanie** | poza symetryczna (twinning) vs kontrapost na obrotowych podstawach |
| **12_Atrakcyjnosc** | postać nijaka vs postać z charakterem (proporcje, kolor, oczy z mruganiem, antena, przesada) |

### Część II – animacja od zera

| Scena | Co pokazuje |
|---|---|
| **13_Klatki_kluczowe_krzywe** | te same klucze, różne styczne: Constant, Linear, Clamped Auto, Auto, Free; wykresy krzywych ze znacznikiem czasu |
| **14_Hierarchia_i_wlasciwosci** | jeden klip animuje stawy robota, światło (natężenie, kolor), kolor materiału, skalę i aktywność obiektu; lista ścieżek i właściwości |
| **15_Animation_legacy_WrapMode** | komponent Animation: Once, Loop, PingPong, ClampForever, odtwarzanie wstecz, CrossFade; porównanie z Animatorem |
| **16_Zdarzenia_animacji** | AnimationEvent: OnFootstep(0/1) przy krokach, OnHit("cios") w klatce trafienia |

### Część III – Animator

| Scena | Co pokazuje |
|---|---|
| **17_Animator_maszyna_stanow** | stany Idle / Walk / Run / Jump / Wave, parametry Float / Trigger / Bool, Any State, Has Exit Time; tablice stanów |
| **18_Animator_przejscia** | to samo przejście z czasem 0, 0,3 s i z Has Exit Time; przerwanie z Any State |
| **19_Animator_Blend_Tree** | Blend Tree 1D (Speed) i 2D Simple Directional (MoveX, MoveY), wagi klipów na żywo |
| **20_Animator_warstwy_maski** | warstwa Override z Avatar Mask (górna część ciała) i warstwa Additive; SetLayerWeight |
| **21_Animator_podmaszyna_Behaviour** | podmaszyna „Walka” z kombinacją 3 ciosów, Exit, tagi stanów, StateMachineBehaviour (StateLogger) |
| **22_Animator_Root_Motion** | Apply Root Motion wył./wł., OnAnimatorMove, root motion ze skrętem (krzywe MotionT / MotionQ) |

---

## 5. Struktura projektu

```
Assets/
  InputSystem_Actions.inputactions   domyślne akcje (Project-wide Actions)
  Settings/                          ustawienia URP
  Animacja/
    Scripts/Core/      DemoHud, DemoInput, DemoActionBase, WorldLabel, Ease, SlowMotion, SpacingGhosts, …
    Scripts/Zasady/    skrypty scen 01–12 (BouncingBall, SpringChain, EaseLane, ArcMover, LookAtTarget, FallingObject, …)
    Scripts/OdZera/    skrypty scen 13–16 (CurvePlayhead, PropertyClipDemo, LegacyAnimationDemo, AnimationEventReceiver, …)
    Scripts/Animator/  skrypty scen 17–22 (StateMachineDemo, BlendTreeDemo, LayersDemo, StateLogger, CustomRootMotion, …)
    Editor/            Robot (budowa robota), Poses (pozy i cykle ruchu), ClipFactory (klipy, kontrolery, maski),
                       SceneDefinitions_* (sceny), SceneBuilder, panel scen
    Data/Klipy, Data/Kontrolery, Data/Maski, Scenes/, Materials/, Textures/   (tworzone przez generator)
Narzedzia/             skrypty .bat: otwarcie projektu, generowanie scen, build Windows
```
