using System.Text;
using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Scena 20: warstwy Animatora. Spacja – waga warstwy 1 (0 ↔ 1, płynnie), LPM – waga 0,5.
    /// Robot 1: tylko warstwa bazowa (chód). Robot 2: + warstwa Override z maską (górna część ciała macha).
    /// Robot 3: + warstwa Additive (kiwanie głową i tułowiem dodawane do chodu).
    /// </summary>
    public class LayersDemo : DemoActionBase
    {
        public Animator[] layered;
        float m_Target = 1f;
        float m_Weight = 1f;

        public override void OnMainAction()
        {
            m_Target = m_Target > 0.5f ? 0f : 1f;
            DemoHud.Log("SetLayerWeight(1, " + m_Target + ")");
        }

        public override void OnSecondaryAction()
        {
            m_Target = 0.5f;
            DemoHud.Log("SetLayerWeight(1, 0.5)");
        }

        void Update()
        {
            m_Weight = Mathf.MoveTowards(m_Weight, m_Target, 1.5f * Time.deltaTime);
            foreach (var a in layered)
                if (a != null && a.layerCount > 1) a.SetLayerWeight(1, m_Weight);
            var sb = new StringBuilder();
            sb.Append("waga warstwy 1 = <b>").Append(m_Weight.ToString("0.00")).Append("</b>\n\n");
            foreach (var a in layered)
            {
                if (a == null) continue;
                sb.Append("<b>").Append(a.name).Append("</b>\n");
                for (int l = 0; l < a.layerCount; l++)
                    sb.Append("   warstwa ").Append(l).Append(" \"").Append(a.GetLayerName(l)).Append("\" waga ")
                      .Append(a.GetLayerWeight(l).ToString("0.00")).Append('\n');
            }
            DemoHud.SetStatus("layers", sb.ToString());
        }
    }
}
