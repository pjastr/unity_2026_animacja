using System.Text;
using UnityEngine;

namespace Animacja
{
    /// <summary>
    /// Scena 17: maszyna stanów Animatora sterowana parametrami.
    /// W / S – prędkość (float Speed), Spacja – skok (trigger Jump), LPM – machanie (bool Wave).
    /// Robot porusza się po okręgu z prędkością równą parametrowi Speed. Tablice stanów podświetlają aktywny stan.
    /// </summary>
    public class StateMachineDemo : DemoActionBase
    {
        public Animator animator;
        public float maxSpeed = 4f;
        public float radius = 4f;
        public Vector3 center;
        public string[] stateNames = { "Idle", "Walk", "Run", "Jump", "Wave" };
        public Renderer[] stateNodes;
        public Material nodeIdle;
        public Material nodeActive;
        public Material nodeNext;

        static readonly int SpeedId = Animator.StringToHash("Speed");
        static readonly int JumpId = Animator.StringToHash("Jump");
        static readonly int WaveId = Animator.StringToHash("Wave");

        float m_Target;
        float m_Speed;
        float m_Angle;

        public override void OnMainAction()
        {
            animator.SetTrigger(JumpId);
            DemoHud.Log("SetTrigger(\"Jump\")");
        }

        public override void OnSecondaryAction()
        {
            bool w = !animator.GetBool(WaveId);
            animator.SetBool(WaveId, w);
            DemoHud.Log($"SetBool(\"Wave\", {w})");
        }

        void Update()
        {
            float input = DemoInput.ReadVector2(DemoInput.Move).y;
            m_Target = Mathf.Clamp(m_Target + input * 2.5f * Time.deltaTime, 0f, maxSpeed);
            m_Speed = Mathf.MoveTowards(m_Speed, m_Target, 4f * Time.deltaTime);
            animator.SetFloat(SpeedId, m_Speed);

            // ruch po okręgu – prędkość liniowa = Speed [m/s]
            m_Angle += m_Speed / radius * Time.deltaTime;
            Vector3 pos = center + new Vector3(Mathf.Sin(m_Angle), 0f, Mathf.Cos(m_Angle)) * radius;
            Vector3 fwd = new Vector3(Mathf.Cos(m_Angle), 0f, -Mathf.Sin(m_Angle));
            animator.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(fwd));

            var cur = animator.GetCurrentAnimatorStateInfo(0);
            var next = animator.IsInTransition(0) ? animator.GetNextAnimatorStateInfo(0) : default(AnimatorStateInfo);
            for (int i = 0; i < stateNodes.Length && i < stateNames.Length; i++)
            {
                Material m = nodeIdle;
                if (cur.IsName(stateNames[i])) m = nodeActive;
                else if (animator.IsInTransition(0) && next.IsName(stateNames[i])) m = nodeNext;
                stateNodes[i].sharedMaterial = m;
            }

            var sb = new StringBuilder();
            sb.Append(AnimatorInfo.Describe(animator, stateNames)).Append("\n\n<b>Parametry</b>\n").Append(AnimatorInfo.Parameters(animator));
            sb.Append("\n<b>Przejścia</b>\n   Idle → Walk: Speed > 0.1      Walk → Idle: Speed < 0.1\n" +
                      "   Walk → Run: Speed > 2.5       Run → Walk: Speed < 2.5\n" +
                      "   Any State → Jump: trigger Jump\n   Jump → Idle: Has Exit Time (0.9)\n" +
                      "   Idle → Wave: Wave = true      Wave → Idle: Wave = false");
            DemoHud.SetStatus("sm", sb.ToString());
        }
    }
}
