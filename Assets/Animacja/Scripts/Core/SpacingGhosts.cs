using System.Collections.Generic;
using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// "Duszki": co stały odstęp czasu zostawia półprzezroczystą kopię kształtu obiektu.
    /// Odstępy między duszkami pokazują rozkład ruchu w czasie (spacing) – gęsto = wolno, rzadko = szybko.
    /// </summary>
    public class SpacingGhosts : MonoBehaviour
    {
        public static bool Visible = true;

        [Tooltip("Odstęp czasu między duszkami [s] (czas gry)")]
        public float interval = 1f / 12f;
        public int maxGhosts = 14;
        public Material ghostMaterial;
        public float sizeFactor = 0.6f;

        readonly Queue<GameObject> m_Ghosts = new Queue<GameObject>();
        float m_Timer;
        Mesh m_Mesh;

        void Start()
        {
            var mf = GetComponent<MeshFilter>();
            m_Mesh = mf != null ? mf.sharedMesh : null;
        }

        void Update()
        {
            m_Timer += Time.deltaTime;
            if (m_Timer < interval) return;
            m_Timer -= interval;
            if (!Visible || m_Mesh == null)
            {
                Clear();
                return;
            }
            var g = new GameObject("Duszek");
            g.transform.SetPositionAndRotation(transform.position, transform.rotation);
            g.transform.localScale = transform.lossyScale * sizeFactor;
            g.AddComponent<MeshFilter>().sharedMesh = m_Mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = ghostMaterial;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_Ghosts.Enqueue(g);
            while (m_Ghosts.Count > maxGhosts)
                Destroy(m_Ghosts.Dequeue());
        }

        public void Clear()
        {
            while (m_Ghosts.Count > 0)
                Destroy(m_Ghosts.Dequeue());
        }

        void OnDisable() => Clear();
    }
}
