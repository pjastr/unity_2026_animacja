using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Zasada 7 – łuki: ruch z A do B po linii prostej albo po łuku (paraboli) o wysokości arcHeight.
    /// Naturalne ruchy (rzut, zamach ręką, obrót głowy) prawie zawsze przebiegają po łukach.
    /// </summary>
    public class ArcMover : DemoActionBase
    {
        public Vector3 from;
        public Vector3 to;
        public float arcHeight = 2.5f;
        public bool useArc = true;
        public float travelTime = 1.4f;
        public float hold = 0.5f;

        float m_Time;

        public override void OnMainAction() { m_Time = 0f; }

        void Update()
        {
            m_Time += Time.deltaTime;
            float cycle = 2f * (travelTime + hold);
            float u = Mathf.Repeat(m_Time, cycle);
            float s;
            bool back = false;
            if (u < travelTime) s = u / travelTime;
            else if (u < travelTime + hold) s = 1f;
            else if (u < 2f * travelTime + hold) { s = (u - travelTime - hold) / travelTime; back = true; }
            else { s = 1f; back = true; }
            float e = Ease.Evaluate(Ease.Type.InOutQuad, s);
            Vector3 a = back ? to : from;
            Vector3 b = back ? from : to;
            Vector3 p = Vector3.Lerp(a, b, e);
            if (useArc) p += Vector3.up * arcHeight * 4f * e * (1f - e);
            transform.position = p;
        }
    }
}
