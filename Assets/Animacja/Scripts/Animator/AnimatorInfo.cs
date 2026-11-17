using System.Text;
using UnityEngine;

namespace Animacja
{
    /// <summary>Pomocnicze opisy stanu Animatora (nazwy stanów po hash'ach, przejścia, parametry).</summary>
    public static class AnimatorInfo
    {
        public static string StateName(AnimatorStateInfo info, string[] names)
        {
            foreach (var n in names)
                if (info.IsName(n)) return n;
            return "?";
        }

        public static string Describe(Animator a, string[] names, int layer = 0)
        {
            var sb = new StringBuilder();
            var cur = a.GetCurrentAnimatorStateInfo(layer);
            sb.Append("stan: <b>").Append(StateName(cur, names)).Append("</b>  normalizedTime = ")
              .Append(cur.normalizedTime.ToString("0.00"));
            if (a.IsInTransition(layer))
            {
                var next = a.GetNextAnimatorStateInfo(layer);
                var tr = a.GetAnimatorTransitionInfo(layer);
                sb.Append("\n   przejście → <b>").Append(StateName(next, names)).Append("</b>  postęp ")
                  .Append((tr.normalizedTime * 100f).ToString("0")).Append('%');
            }
            return sb.ToString();
        }

        public static string Parameters(Animator a)
        {
            var sb = new StringBuilder();
            foreach (var p in a.parameters)
            {
                sb.Append("   ").Append(p.name).Append(" (").Append(p.type).Append(") = ");
                switch (p.type)
                {
                    case AnimatorControllerParameterType.Float: sb.Append(a.GetFloat(p.nameHash).ToString("0.00")); break;
                    case AnimatorControllerParameterType.Int: sb.Append(a.GetInteger(p.nameHash)); break;
                    case AnimatorControllerParameterType.Bool: sb.Append(a.GetBool(p.nameHash)); break;
                    default: sb.Append(a.GetBool(p.nameHash) ? "ustawiony" : "-"); break;
                }
                sb.Append('\n');
            }
            return sb.ToString();
        }
    }
}
