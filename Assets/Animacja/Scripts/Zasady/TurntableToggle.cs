using UnityEngine;

namespace Animacja
{
    /// <summary>Spacja (opcjonalnie) – pauza obrotowych podstaw (Turntable).</summary>
    public class TurntableToggle : DemoActionBase
    {
        public override void OnMainAction()
        {
            Turntable.Paused = !Turntable.Paused;
            DemoHud.Log(Turntable.Paused ? "Obrót zatrzymany" : "Obrót wznowiony");
        }

        void OnDestroy() => Turntable.Paused = false;
    }
}
