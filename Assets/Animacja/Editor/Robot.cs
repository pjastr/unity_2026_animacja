using System.Collections.Generic;
using UnityEngine;

namespace Animacja.EditorTools
{
    /// <summary>Nazwy stawów robota i ich ścieżki względem korzenia (Animator) – ścieżki trafiają do krzywych klipów.</summary>
    public static class J
    {
        public const string Hips = "Biodra";
        public const string Torso = "Tulow";
        public const string Head = "Glowa";
        public const string ArmL = "RamieL";
        public const string ForeL = "PrzedramieL";
        public const string HandL = "DlonL";
        public const string ArmR = "RamieP";
        public const string ForeR = "PrzedramieP";
        public const string HandR = "DlonP";
        public const string ThighL = "UdoL";
        public const string ShinL = "GolenL";
        public const string FootL = "StopaL";
        public const string ThighR = "UdoP";
        public const string ShinR = "GolenP";
        public const string FootR = "StopaP";

        public static readonly string[] All =
        {
            Hips, Torso, Head, ArmL, ForeL, HandL, ArmR, ForeR, HandR, ThighL, ShinL, FootL, ThighR, ShinR, FootR,
        };

        public static readonly Dictionary<string, string> Path = new Dictionary<string, string>
        {
            { Hips, "Biodra" },
            { Torso, "Biodra/Tulow" },
            { Head, "Biodra/Tulow/Glowa" },
            { ArmL, "Biodra/Tulow/RamieL" },
            { ForeL, "Biodra/Tulow/RamieL/PrzedramieL" },
            { HandL, "Biodra/Tulow/RamieL/PrzedramieL/DlonL" },
            { ArmR, "Biodra/Tulow/RamieP" },
            { ForeR, "Biodra/Tulow/RamieP/PrzedramieP" },
            { HandR, "Biodra/Tulow/RamieP/PrzedramieP/DlonP" },
            { ThighL, "Biodra/UdoL" },
            { ShinL, "Biodra/UdoL/GolenL" },
            { FootL, "Biodra/UdoL/GolenL/StopaL" },
            { ThighR, "Biodra/UdoP" },
            { ShinR, "Biodra/UdoP/GolenP" },
            { FootR, "Biodra/UdoP/GolenP/StopaP" },
        };

        /// <summary>Położenie stawu względem rodzica w pozie spoczynkowej.</summary>
        public static readonly Dictionary<string, Vector3> Rest = new Dictionary<string, Vector3>
        {
            { Hips, new Vector3(0f, 1f, 0f) },
            { Torso, new Vector3(0f, 0.08f, 0f) },
            { Head, new Vector3(0f, 0.72f, 0f) },
            { ArmL, new Vector3(-0.33f, 0.62f, 0f) },
            { ForeL, new Vector3(0f, -0.32f, 0f) },
            { HandL, new Vector3(0f, -0.3f, 0f) },
            { ArmR, new Vector3(0.33f, 0.62f, 0f) },
            { ForeR, new Vector3(0f, -0.32f, 0f) },
            { HandR, new Vector3(0f, -0.3f, 0f) },
            { ThighL, new Vector3(-0.12f, -0.06f, 0f) },
            { ShinL, new Vector3(0f, -0.44f, 0f) },
            { FootL, new Vector3(0f, -0.42f, 0f) },
            { ThighR, new Vector3(0.12f, -0.06f, 0f) },
            { ShinR, new Vector3(0f, -0.44f, 0f) },
            { FootR, new Vector3(0f, -0.42f, 0f) },
        };
    }

    /// <summary>Robot zbudowany z prymitywów: hierarchia stawów (puste obiekty) + siatki jako dzieci stawów.</summary>
    public class RobotRig
    {
        public GameObject root;
        public readonly Dictionary<string, Transform> joints = new Dictionary<string, Transform>();
        public readonly List<Renderer> bodyRenderers = new List<Renderer>();
        public Transform headMesh;

        public Transform this[string joint] => joints[joint];

        public Animator AddAnimator(RuntimeAnimatorController controller)
        {
            var a = root.GetComponent<Animator>();
            if (a == null) a = root.AddComponent<Animator>();
            a.runtimeAnimatorController = controller;
            a.applyRootMotion = false;
            a.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            return a;
        }

        /// <summary>Ustawia pozę statycznie (bez Animatora) – np. "duszki" póz kluczowych.</summary>
        public void ApplyPose(Pose p)
        {
            foreach (var j in J.All)
                joints[j].localRotation = Quaternion.Euler(p.Get(j));
            joints[J.Hips].localPosition = J.Rest[J.Hips] + p.hips;
            joints[J.Torso].localScale = p.torsoScale;
        }
    }

    public static class RobotFactory
    {
        public static RobotRig Build(string name, Vector3 feet, float yaw, Material body, Material joint, Material accent, Transform parent = null)
        {
            var rig = new RobotRig();
            rig.root = new GameObject(name);
            if (parent != null) rig.root.transform.SetParent(parent, false);
            rig.root.transform.position = feet;
            rig.root.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            var hips = Joint(rig, J.Hips, rig.root.transform);
            var torso = Joint(rig, J.Torso, hips);
            var head = Joint(rig, J.Head, torso);
            var armL = Joint(rig, J.ArmL, torso);
            var foreL = Joint(rig, J.ForeL, armL);
            var handL = Joint(rig, J.HandL, foreL);
            var armR = Joint(rig, J.ArmR, torso);
            var foreR = Joint(rig, J.ForeR, armR);
            var handR = Joint(rig, J.HandR, foreR);
            var thighL = Joint(rig, J.ThighL, hips);
            var shinL = Joint(rig, J.ShinL, thighL);
            var footL = Joint(rig, J.FootL, shinL);
            var thighR = Joint(rig, J.ThighR, hips);
            var shinR = Joint(rig, J.ShinR, thighR);
            var footR = Joint(rig, J.FootR, shinR);

            Mesh(rig, PrimitiveType.Cube, "Miednica", hips, new Vector3(0, 0, 0), new Vector3(0.42f, 0.18f, 0.24f), joint, true);
            Mesh(rig, PrimitiveType.Cube, "Korpus", torso, new Vector3(0, 0.36f, 0), new Vector3(0.52f, 0.62f, 0.3f), body, true);
            rig.headMesh = Mesh(rig, PrimitiveType.Sphere, "Glowa_siatka", head, new Vector3(0, 0.2f, 0), Vector3.one * 0.38f, body, true).transform;
            Mesh(rig, PrimitiveType.Cube, "Wizjer", head, new Vector3(0, 0.23f, 0.16f), new Vector3(0.26f, 0.08f, 0.06f), accent, false);
            Mesh(rig, PrimitiveType.Cylinder, "Szyja", head, new Vector3(0, 0.0f, 0), new Vector3(0.12f, 0.05f, 0.12f), joint, false);
            foreach (var s in new[] { new[] { armL, foreL, handL }, new[] { armR, foreR, handR } })
            {
                Mesh(rig, PrimitiveType.Sphere, "Bark", s[0], Vector3.zero, Vector3.one * 0.16f, joint, false);
                Mesh(rig, PrimitiveType.Capsule, "Ramie_siatka", s[0], new Vector3(0, -0.16f, 0), new Vector3(0.13f, 0.16f, 0.13f), body, true);
                Mesh(rig, PrimitiveType.Capsule, "Przedramie_siatka", s[1], new Vector3(0, -0.15f, 0), new Vector3(0.11f, 0.15f, 0.11f), body, true);
                Mesh(rig, PrimitiveType.Sphere, "Dlon_siatka", s[2], new Vector3(0, -0.05f, 0), Vector3.one * 0.13f, joint, false);
            }
            foreach (var s in new[] { new[] { thighL, shinL, footL }, new[] { thighR, shinR, footR } })
            {
                Mesh(rig, PrimitiveType.Capsule, "Udo_siatka", s[0], new Vector3(0, -0.22f, 0), new Vector3(0.16f, 0.22f, 0.16f), body, true);
                Mesh(rig, PrimitiveType.Sphere, "Kolano", s[1], Vector3.zero, Vector3.one * 0.14f, joint, false);
                Mesh(rig, PrimitiveType.Capsule, "Golen_siatka", s[1], new Vector3(0, -0.21f, 0), new Vector3(0.14f, 0.21f, 0.14f), body, true);
                Mesh(rig, PrimitiveType.Cube, "Stopa_siatka", s[2], new Vector3(0, -0.04f, 0.06f), new Vector3(0.15f, 0.08f, 0.3f), joint, false);
            }
            return rig;
        }

        static Transform Joint(RobotRig rig, string n, Transform p)
        {
            var t = new GameObject(n).transform;
            t.SetParent(p, false);
            t.localPosition = J.Rest[n];
            rig.joints[n] = t;
            return t;
        }

        static GameObject Mesh(RobotRig rig, PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat, bool body)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = mat;
            if (body) rig.bodyRenderers.Add(r);
            return go;
        }
    }
}
