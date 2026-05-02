using System;
using System.Collections;
using System.Collections.Generic;
using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Lightweight facial animation: brow + mouth morphs only (patterned after VRAdultFun E-Motion face state targets).
    /// Does not use head / neck / eye target / eyelid controls or body — only allowed DAZ face morphs.
    /// </summary>
    public class EasyMotionLite : MVRScript
    {
        public const string pluginName = "EasyMotionLite";

        public const string pluginVersion = "1.0.0";

        private static readonly string[] AllowedMorphDisplayNames =
        {
            "Brow Down", "Brow Up", "Brow Inner Up", "Brow Outer Up Left", "Brow Outer Up Right", "Concentrate",
            "Smile Full Face", "Smile Open Full Face", "Glare", "Flirting", "Happy", "Huge Smile",
            "Mouth Open", "Mouth Open Wide", "Lip Bite", "Mouth Smile Simple Left", "Mouth Smile Simple Right",
            "Lips Pucker", "Lips Pucker Wide", "Mouth Side-Side Left", "Mouth Side-Side Right",
            "Lips Part", "Lips Close", "Cheeks Sink Lower", "Tongue In-Out", "Mouth Narrow"
        };

        private GenerateDAZMorphsControlUI _morphUi;

        private readonly List<DAZMorph> _morphs = new List<DAZMorph>();

        private readonly List<string> _morphDisplayNames = new List<string>();

        private readonly List<float> _goals = new List<float>();

        private JSONStorableBool _run;

        private JSONStorableFloat _intensity;

        private JSONStorableFloat _smooth;

        private float _nextPresetTime;

        private bool _morphsReady;

        public override void Init()
        {
            try
            {
                pluginLabelJSON.val = pluginName + " " + pluginVersion;

                _run = new JSONStorableBool("Run facial idle", true);
                RegisterBool(_run);
                CreateToggle(_run, false);

                _intensity = new JSONStorableFloat("Expression intensity", 1f, 0f, 1.25f, true);
                RegisterFloat(_intensity);
                CreateSlider(_intensity, false);

                _smooth = new JSONStorableFloat("Ease speed", 3f, 0.5f, 10f, true);
                RegisterFloat(_smooth);
                CreateSlider(_smooth, false);
            }
            catch (Exception e)
            {
                SuperController.LogError("EasyMotionLite Init: " + e.Message);
            }
        }

        public void Start()
        {
            StartCoroutine(CoSetupMorphs());
        }

        private IEnumerator CoSetupMorphs()
        {
            yield return null;
            yield return null;

            try
            {
                JSONStorable geo = containingAtom != null ? containingAtom.GetStorableByID("geometry") : null;
                DAZCharacterSelector dcs = geo as DAZCharacterSelector;
                if (dcs == null)
                {
                    SuperController.LogError("EasyMotionLite: no DAZCharacterSelector on geometry.");
                    yield break;
                }

                _morphUi = dcs.morphsControlUI;
                if (_morphUi == null)
                {
                    SuperController.LogError("EasyMotionLite: morphsControlUI missing.");
                    yield break;
                }

                _morphs.Clear();
                _morphDisplayNames.Clear();
                _goals.Clear();

                for (int i = 0; i < AllowedMorphDisplayNames.Length; i++)
                {
                    string dn = AllowedMorphDisplayNames[i];
                    if (dn.IndexOf("Eye", StringComparison.OrdinalIgnoreCase) >= 0)
                        continue;
                    DAZMorph m = _morphUi.GetMorphByDisplayName(dn);
                    if (m == null)
                        continue;
                    _morphs.Add(m);
                    _morphDisplayNames.Add(dn);
                    _goals.Add(0f);
                }

                if (_morphs.Count == 0)
                {
                    SuperController.LogError("EasyMotionLite: no allowed morphs found on this person.");
                    yield break;
                }

                _morphsReady = true;
                _nextPresetTime = Time.time + UnityEngine.Random.Range(2f, 5f);
                PickRandomPreset();
            }
            catch (Exception e)
            {
                SuperController.LogError("EasyMotionLite CoSetupMorphs: " + e.Message);
            }
        }

        private void Update()
        {
            if (!_morphsReady || _morphs.Count == 0)
                return;

            float intensity = _intensity != null ? _intensity.val : 1f;
            float smooth = _smooth != null ? _smooth.val : 3f;
            float dt = Time.deltaTime;

            if (_run != null && _run.val && Time.time >= _nextPresetTime)
            {
                PickRandomPreset();
                _nextPresetTime = Time.time + UnityEngine.Random.Range(3f, 8f);
            }

            if (_run == null || !_run.val)
            {
                for (int i = 0; i < _goals.Count; i++)
                    _goals[i] = 0f;
            }

            for (int i = 0; i < _morphs.Count; i++)
            {
                DAZMorph morph = _morphs[i];
                if (morph == null)
                    continue;
                float goal = _goals[i] * intensity;
                float cur = morph.morphValue;
                float alpha = 1f - Mathf.Exp(-smooth * dt);
                float next = Mathf.Lerp(cur, goal, alpha);
                morph.SetValue(next);
            }
        }

        private void PickRandomPreset()
        {
            for (int i = 0; i < _goals.Count; i++)
                _goals[i] = 0f;

            int roll = UnityEngine.Random.Range(0, 6);
            if (roll == 0)
                PresetNeutralSoft();
            else if (roll == 1)
                PresetSmile();
            else if (roll == 2)
                PresetOpenMouth();
            else if (roll == 3)
                PresetBrowInterest();
            else if (roll == 4)
                PresetPuckerSubtle();
            else
                PresetMicroExpression();
        }

        private void SetGoal(string displayName, float v)
        {
            for (int i = 0; i < _morphDisplayNames.Count; i++)
            {
                if (_morphDisplayNames[i] == displayName)
                {
                    _goals[i] = Mathf.Clamp01(v);
                    return;
                }
            }
        }

        private void PresetNeutralSoft()
        {
            SetGoal("Mouth Open", UnityEngine.Random.Range(0f, 0.12f));
            SetGoal("Lips Part", UnityEngine.Random.Range(0f, 0.15f));
        }

        private void PresetSmile()
        {
            SetGoal("Smile Full Face", UnityEngine.Random.Range(0.18f, 0.45f));
            SetGoal("Mouth Smile Simple Left", UnityEngine.Random.Range(0.2f, 0.55f));
            SetGoal("Mouth Smile Simple Right", UnityEngine.Random.Range(0.2f, 0.55f));
            SetGoal("Brow Up", UnityEngine.Random.Range(0f, 0.18f));
            SetGoal("Happy", UnityEngine.Random.Range(0f, 0.25f));
        }

        private void PresetOpenMouth()
        {
            SetGoal("Mouth Open", UnityEngine.Random.Range(0.15f, 0.45f));
            SetGoal("Lips Part", UnityEngine.Random.Range(0.1f, 0.35f));
            SetGoal("Smile Open Full Face", UnityEngine.Random.Range(0f, 0.2f));
        }

        private void PresetBrowInterest()
        {
            SetGoal("Brow Inner Up", UnityEngine.Random.Range(0.08f, 0.28f));
            SetGoal("Brow Outer Up Left", UnityEngine.Random.Range(0f, 0.12f));
            SetGoal("Brow Outer Up Right", UnityEngine.Random.Range(0f, 0.12f));
            SetGoal("Concentrate", UnityEngine.Random.Range(0.1f, 0.35f));
            SetGoal("Lip Bite", UnityEngine.Random.Range(0f, 0.25f));
            SetGoal("Mouth Open", UnityEngine.Random.Range(0f, 0.12f));
        }

        private void PresetPuckerSubtle()
        {
            SetGoal("Lips Pucker", UnityEngine.Random.Range(0.1f, 0.35f));
            SetGoal("Flirting", UnityEngine.Random.Range(0f, 0.22f));
            SetGoal("Brow Up", UnityEngine.Random.Range(0f, 0.12f));
        }

        private void PresetMicroExpression()
        {
            SetGoal("Glare", UnityEngine.Random.Range(0f, 0.15f));
            SetGoal("Cheeks Sink Lower", UnityEngine.Random.Range(0f, 0.12f));
            SetGoal("Mouth Side-Side Left", UnityEngine.Random.Range(0f, 0.12f));
        }

        private void OnDestroy()
        {
            try
            {
                if (!_morphsReady || _morphs == null)
                    return;
                for (int i = 0; i < _morphs.Count; i++)
                {
                    DAZMorph morph = _morphs[i];
                    if (morph != null)
                        morph.SetValue(0f);
                }
            }
            catch
            {
            }
        }
    }
}
