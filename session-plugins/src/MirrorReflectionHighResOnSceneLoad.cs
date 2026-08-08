using System.Collections.Generic;
using MeshVR;

namespace geesp0t
{
    /// <summary>
    /// After a scene change, if any <c>Glass</c> or <c>ReflectiveSlate</c> atom
    /// exposes VaM&apos;s <c>MirrorRender</c> (<see cref="MirrorReflection"/>),
    /// set reflection <see cref="MirrorReflection.textureSize"/> to 4096 on
    /// each such atom so mirrors / glass are sharp (user preference on load).
    /// </summary>
    public static class MirrorReflectionHighResOnSceneLoad
    {
        private const int TargetReflectionTextureSize = 4096;

        private static bool IsMirrorHostAtomType(string atomType)
        {
            if (string.IsNullOrEmpty(atomType))
            {
                return false;
            }

            return atomType == "Glass" || atomType == "ReflectiveSlate";
        }

        /// <summary>
        /// Runs after SceneControlSuite detects a scene change; cheap scan of the atom list.
        /// </summary>
        public static void ApplyIfSceneHasMirrorHosts(SuperController sc)
        {
            if (sc == null)
            {
                return;
            }

            List<Atom> atoms = sc.GetAtoms();
            bool needHighRes;
            needHighRes = false;
            int i;
            for (i = 0; i < atoms.Count; i++)
            {
                Atom a = atoms[i];
                if (a == null || !IsMirrorHostAtomType(a.type))
                {
                    continue;
                }

                JSONStorable st = a.GetStorableByID("MirrorRender");
                if (st is MirrorReflection)
                {
                    needHighRes = true;
                    break;
                }
            }

            if (!needHighRes)
            {
                return;
            }

            for (i = 0; i < atoms.Count; i++)
            {
                Atom a = atoms[i];
                if (a == null || !IsMirrorHostAtomType(a.type))
                {
                    continue;
                }

                MirrorReflection mr =
                    a.GetStorableByID("MirrorRender") as MirrorReflection;
                if (mr == null)
                {
                    continue;
                }

                if (mr.textureSize != TargetReflectionTextureSize)
                {
                    mr.textureSize = TargetReflectionTextureSize;
                }
            }
        }
    }
}
