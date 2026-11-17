using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Zasada 3 – inscenizacja: Spacja – inscenizacja wył./wł. (kontrast, światło, porządek w tle),
    /// LPM – test sylwetki (wszystko czarne na jasnym tle).
    /// </summary>
    public class StagingDemo : DemoActionBase
    {
        public Renderer[] characterRenderers;
        public Material characterPlain;
        public Material characterAccent;
        public Renderer[] clutter;
        public Material clutterBusy;
        public Material clutterCalm;
        public Transform[] clutterToHide;
        public Light spot;
        public Light mainLight;
        public Material silhouetteMaterial;

        bool m_Staged;
        bool m_Silhouette;
        Material[][] m_Saved;
        Renderer[] m_All;

        void Start() => Apply();

        public override void OnMainAction()
        {
            m_Staged = !m_Staged;
            DemoHud.Log(m_Staged ? "Inscenizacja: WŁ." : "Inscenizacja: WYŁ.");
            Apply();
        }

        public override void OnSecondaryAction()
        {
            m_Silhouette = !m_Silhouette;
            var cam = Camera.main;
            if (m_Silhouette)
            {
                m_All = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
                m_Saved = new Material[m_All.Length][];
                for (int i = 0; i < m_All.Length; i++)
                {
                    m_Saved[i] = m_All[i].sharedMaterials;
                    var mats = new Material[m_Saved[i].Length];
                    for (int j = 0; j < mats.Length; j++) mats[j] = silhouetteMaterial;
                    m_All[i].sharedMaterials = mats;
                }
                if (cam != null) { cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.95f, 0.95f, 0.92f); }
            }
            else if (m_All != null)
            {
                for (int i = 0; i < m_All.Length; i++)
                    if (m_All[i] != null) m_All[i].sharedMaterials = m_Saved[i];
                if (cam != null) cam.clearFlags = CameraClearFlags.Skybox;
            }
            DemoHud.Log(m_Silhouette ? "Test sylwetki: czy akcja jest czytelna bez kolorów i szczegółów?" : "Test sylwetki wyłączony");
        }

        void Apply()
        {
            foreach (var r in characterRenderers)
                if (r != null) r.sharedMaterial = m_Staged ? characterAccent : characterPlain;
            foreach (var r in clutter)
                if (r != null) r.sharedMaterial = m_Staged ? clutterCalm : clutterBusy;
            foreach (var t in clutterToHide)
                if (t != null) t.gameObject.SetActive(!m_Staged);
            if (spot != null) spot.enabled = m_Staged;
            if (mainLight != null) mainLight.intensity = m_Staged ? 0.45f : 1.2f;
            DemoHud.SetStatus("staging",
                m_Staged
                    ? "<b>Inscenizacja WŁ.</b>\n• postać w kontrastowym kolorze\n• światło punktowe prowadzi wzrok\n• tło wyciszone i uporządkowane\n• poza czytelna w sylwetce"
                    : "<b>Inscenizacja WYŁ.</b>\n• postać w kolorze tła\n• równe, płaskie światło\n• tło pełne obiektów konkurujących o uwagę");
        }
    }
}
