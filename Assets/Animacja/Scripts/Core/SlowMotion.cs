using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Sterowanie czasem w scenach "12 zasad": LPM – zwolnione tempo (Time.timeScale 1 ↔ 0,25),
    /// PPM – pokaż / ukryj ślady (duszki i linie toru).
    /// </summary>
    public class SlowMotion : DemoActionBase
    {
        public float slowScale = 0.25f;

        public override void OnSecondaryAction()
        {
            Time.timeScale = Time.timeScale < 0.99f ? 1f : slowScale;
            DemoHud.Log("Time.timeScale = " + Time.timeScale);
        }

        public override void OnTertiaryAction()
        {
            SpacingGhosts.Visible = !SpacingGhosts.Visible;
            foreach (var tr in FindObjectsByType<TrailRenderer>(FindObjectsSortMode.None))
                tr.enabled = SpacingGhosts.Visible;
            DemoHud.Log(SpacingGhosts.Visible ? "Ślady widoczne" : "Ślady ukryte");
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        void Update()
        {
            DemoHud.SetStatus("time", $"Time.timeScale = {Time.timeScale:0.00}" + (Time.timeScale < 1f ? "  (zwolnione tempo)" : ""));
        }
    }
}
