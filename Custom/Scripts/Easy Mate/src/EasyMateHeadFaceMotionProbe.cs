using System;

namespace geesp0t
{
    /// <summary>
    /// Detects motion-animation clips on a Person targeting head / neck / gaze.
    /// Skips auto-merging E-Motion Original when scene mocap already drives those
    /// rigs (<see cref="Atom.motionAnimationControls"/>).
    /// </summary>
    internal static class EasyMateHeadFaceMotionProbe
    {
        private const float MinClipLength = 0.0005f;

        /// <summary>Lowercase storables VaM Persons use for head pose and gaze.</summary>
        private static readonly string[] HeadFaceLowerStoreIds =
        {
            "headcontrol",
            "neckcontrol",
            "eyetargetcontrol"
        };

        /// <summary>
        /// True when <paramref name="person"/> has a motion clip with positive length tied
        /// to head, neck, or eye-target free controllers on that atom.
        /// </summary>
        public static bool PersonHasHeadFaceMotionClip(Atom person)
        {
            if (person == null || person.type != "Person")
                return false;

            MotionAnimationControl[] macs = person.motionAnimationControls;
            if (macs == null || macs.Length == 0)
                return false;

            for (int i = 0; i < macs.Length; i++)
            {
                MotionAnimationControl mac = macs[i];
                if (mac == null)
                    continue;
                MotionAnimationClip clip = mac.clip;
                if (clip == null || clip.clipLength < MinClipLength)
                    continue;
                FreeControllerV3 fc = mac.controller;
                if (fc == null)
                    continue;
                string sid = fc.storeId;
                if (string.IsNullOrEmpty(sid))
                    continue;
                string low = sid.ToLowerInvariant();
                for (int k = 0; k < HeadFaceLowerStoreIds.Length; k++)
                {
                    if (low == HeadFaceLowerStoreIds[k])
                        return true;
                }
            }

            return false;
        }
    }
}
