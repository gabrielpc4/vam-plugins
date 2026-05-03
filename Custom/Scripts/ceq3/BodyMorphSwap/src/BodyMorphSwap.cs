using System;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

namespace BodyMorphSwap
{
    /// <summary>
    /// Applies donor Person/appearance preset morphs everywhere except head
    /// regions (same token list as MorphMAS). Does not touch hair slots.
    /// </summary>
    public class BodyMorphSwap : MVRScript
    {
        private static readonly string[] HeadRegionTokens = new string[]
        {
            "head",
            "face",
            "mouth",
            "lips",
            "eyelash",
            "eye",
            "eyebrow",
            "ear",
            "brow",
            "nose",
            "tooth",
            "tongue",
            "cheek",
            "chin",
            "jaw",
            "forehead",
            "temple",
            "scalp",
            "neck",
            "cranium"
        };

        private JSONStorableUrl _donorPresetPath;
        private JSONStorableString _statusLine;
        private JSONStorableAction _applyAction;

        public override void Init()
        {
            try
            {
                BuildUi();
            }
            catch (Exception e)
            {
                LogErrorShowHud(string.Concat(
                    "[BodyMorphSwap] Init failed: ",
                    e.ToString()));
            }
        }

        private void BuildUi()
        {
            _donorPresetPath = new JSONStorableUrl(
                "donorPresetPath_body",
                string.Empty,
                delegate(string newValue)
                {
                    OnPresetPathChanged(newValue);
                },
                "json",
                "Saves");
            _donorPresetPath.showDirs = true;
            RegisterUrl(_donorPresetPath);

            UIDynamicButton browseButton =
                CreateButton("Browse donor Person/appearance .json");
            if (browseButton != null && browseButton.button != null)
            {
                _donorPresetPath.RegisterFileBrowseButton(
                    browseButton.button);
            }

            UIDynamicTextField pathField =
                CreateTextField(_donorPresetPath, false);
            if (pathField != null)
            {
                pathField.height = 42f;
            }

            _applyAction = new JSONStorableAction(
                "applyBodyFromPresetExcludeHead",
                ApplyFromCurrentPreset);
            RegisterAction(_applyAction);

            UIDynamicButton applyButton =
                CreateButton("Apply body (head + hair untouched)");
            if (applyButton != null && applyButton.button != null)
            {
                applyButton.button.onClick.AddListener(
                    delegate()
                    {
                        ApplyFromCurrentPreset();
                    });
            }

            UIDynamicButton openSavesButton =
                CreateButton("Open Saves folder");
            if (openSavesButton != null && openSavesButton.button != null)
            {
                openSavesButton.button.onClick.AddListener(
                    delegate()
                    {
                        if (SuperController.singleton != null)
                        {
                            SuperController.singleton.OpenFolderInExplorer(
                                "Saves");
                        }
                    });
            }

            _statusLine = new JSONStorableString(
                "statusLine_body",
                InitialStatusText());
            RegisterString(_statusLine);

            UIDynamicTextField statusField =
                CreateTextField(_statusLine, false);
            if (statusField != null)
            {
                statusField.height = 120f;
                if (statusField.UItext != null)
                {
                    statusField.UItext.alignment = TextAnchor.UpperLeft;
                }
            }
        }

        private string InitialStatusText()
        {
            if (containingAtom == null || containingAtom.type != "Person")
            {
                return string.Concat(
                    "Attach this plugin to the Person that should receive ",
                    "the donor body morphs.");
            }

            return string.Concat(
                "Recipient: ",
                containingAtom.uid,
                ". Browse a donor .json under Saves. Non-head preset ",
                "morphs apply automatically; recipient hair stays as-is.");
        }

        private static void LogErrorShowHud(string message)
        {
            SuperController sc = SuperController.singleton;
            if (sc != null && !sc.IsMonitorOnly)
            {
                sc.ShowMainHUD(true, false);
            }
            SuperController.LogError(message);
            if (sc != null)
            {
                sc.OpenErrorLogPanel();
                if (sc.errorLogPanel != null)
                {
                    Transform sub = sc.errorLogPanel.Find("Panel");
                    if (sub != null)
                    {
                        sub.gameObject.SetActive(true);
                    }
                }
            }
        }

        private static string NormalizePresetPathInput(string rawVal)
        {
            if (rawVal == null)
            {
                return string.Empty;
            }

            string trimmed = rawVal.Trim().Replace('\\', '/').Trim();
            if (trimmed.Length == 0)
            {
                return string.Empty;
            }

            int savesIndex = trimmed.IndexOf(
                "/Saves/",
                StringComparison.OrdinalIgnoreCase);
            if (savesIndex >= 0)
            {
                return trimmed.Substring(savesIndex + 1).Trim();
            }

            if (trimmed.StartsWith(
                "Saves/",
                StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }

            int customIndex = trimmed.IndexOf(
                "/Custom/",
                StringComparison.OrdinalIgnoreCase);
            if (customIndex >= 0)
            {
                return trimmed.Substring(customIndex + 1).Trim();
            }

            if (trimmed.StartsWith(
                "Custom/",
                StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }

            return trimmed;
        }

        private void OnPresetPathChanged(string newValue)
        {
            string presetPath = NormalizePresetPathInput(newValue);
            if (_statusLine == null)
            {
                return;
            }

            if (presetPath.Length == 0)
            {
                _statusLine.val =
                    "Pick a donor Person/appearance .json under Saves.";
                return;
            }

            _statusLine.val = string.Concat(
                "Preset selected: ",
                presetPath);

            if (presetPath.EndsWith(
                ".json",
                StringComparison.OrdinalIgnoreCase))
            {
                ApplyFromPresetFile(presetPath);
            }
        }

        private void ApplyFromCurrentPreset()
        {
            string presetPath = NormalizePresetPathInput(
                _donorPresetPath != null ? _donorPresetPath.val : string.Empty);
            ApplyFromPresetFile(presetPath);
        }

        private void ApplyFromPresetFile(string presetPath)
        {
            Atom recipientAtom;
            string recipientErr;
            if (!TryGetRecipientAtom(out recipientAtom, out recipientErr))
            {
                SetStatus(recipientErr);
                return;
            }

            if (presetPath == null || presetPath.Length == 0)
            {
                SetStatus("Pick a donor Person/appearance .json first.");
                return;
            }

            JSONArray presetMorphArray;
            string presetErr;

            if (!TryReadPresetGeometry(
                presetPath,
                out presetMorphArray,
                out presetErr))
            {
                SetStatus(presetErr);
                LogErrorShowHud(string.Concat(
                    "[BodyMorphSwap] ",
                    presetErr));
                return;
            }

            DAZCharacterSelector recipientGeom = GetGeometry(recipientAtom);
            GenerateDAZMorphsControlUI morphCtrl =
                recipientGeom != null ? recipientGeom.morphsControlUI : null;
            if (recipientGeom == null || morphCtrl == null)
            {
                SetStatus("Recipient geometry or morph UI is not ready.");
                return;
            }

            DisableAutoBehaviours(recipientAtom);
            ResetPoseMorphs(recipientGeom);

            int appliedBody = 0;
            int skippedHead = 0;
            int skippedNoRegion = 0;
            int missingOnTarget = 0;

            for (int i = 0; i < presetMorphArray.Count; i++)
            {
                JSONClass morphJson = presetMorphArray[i].AsObject;
                if (morphJson == null)
                {
                    continue;
                }

                DAZMorph targetMorph =
                    ResolveMorphFromPresetMorphJson(morphCtrl, morphJson);
                if (targetMorph == null)
                {
                    missingOnTarget++;
                    continue;
                }

                if (targetMorph.disable || targetMorph.isPoseControl)
                {
                    continue;
                }

                string rn = targetMorph.resolvedRegionName;
                if (rn == null || rn.Length == 0)
                {
                    skippedNoRegion++;
                    continue;
                }

                if (RegionMatchesHeadExcludedSet(targetMorph))
                {
                    skippedHead++;
                    continue;
                }

                targetMorph.RestoreFromJSON(morphJson);
                appliedBody++;
            }

            SmoothMorphBanks(recipientGeom);

            string finalStatus = string.Concat(
                "Applied ",
                appliedBody.ToString(),
                " body (non-head) sliders from ",
                presetPath,
                ". Skipped head-region rows: ",
                skippedHead.ToString(),
                "; no morph region: ",
                skippedNoRegion.ToString(),
                ". Missing on target: ",
                missingOnTarget.ToString(),
                ". Hair unchanged.");

            SetStatus(finalStatus);
            if (SuperController.singleton != null)
            {
                SuperController.singleton.Message(finalStatus);
            }
        }

        private void SetStatus(string message)
        {
            if (_statusLine != null)
            {
                _statusLine.val = message;
            }
        }

        private bool TryGetRecipientAtom(out Atom recipientAtom, out string err)
        {
            recipientAtom = containingAtom;
            err = string.Empty;

            if (recipientAtom == null || recipientAtom.type != "Person")
            {
                err = string.Concat(
                    "Attach this plugin to the Person that should receive ",
                    "the donor body morphs.");
                return false;
            }

            if (!IsPersonReady(recipientAtom))
            {
                err = "Recipient morph bank is not ready yet.";
                return false;
            }

            return true;
        }

        private static bool IsPersonReady(Atom person)
        {
            if (person == null || person.type != "Person")
            {
                return false;
            }

            DAZCharacter dc = person.GetComponentInChildren<DAZCharacter>();
            DAZCharacterSelector geom = GetGeometry(person);
            return dc != null &&
                geom != null &&
                geom.morphsControlUI != null;
        }

        private static DAZCharacterSelector GetGeometry(Atom person)
        {
            if (person == null)
            {
                return null;
            }

            return person.GetStorableByID("geometry") as DAZCharacterSelector;
        }

        private static void DisableAutoBehaviours(Atom recipientAtom)
        {
            if (recipientAtom == null)
            {
                return;
            }

            DisableAutoBehaviour(recipientAtom, "AutoExpressions", "enabled");
            DisableAutoBehaviour(recipientAtom, "AutoJawMouthMorph", "enabled");
            DisableAutoBehaviour(recipientAtom, "BendFix", "enabled");
            DisableAutoBehaviour(recipientAtom, "BreastInOut", "enabled");
            DisableAutoBehaviour(recipientAtom, "EyelidControl", "blinkEnabled");
            DisableAutoBehaviour(
                recipientAtom,
                "EyelidControl",
                "eyelidLookMorphsEnabled");
            DisableAutoBehaviour(recipientAtom, "FemaleAnatomy", "enabled");
            DisableAutoBehaviour(recipientAtom, "MaleAnatomy", "enabled");

            DAZCharacterSelector geom = GetGeometry(recipientAtom);
            if (geom != null && geom.morphBank1 != null)
            {
                DefaultMorphByUID(geom.morphBank1, "Eyelids Bottom Up Left");
                DefaultMorphByUID(geom.morphBank1, "Eyelids Bottom Up Right");
            }
        }

        private static void DisableAutoBehaviour(
            Atom recipientAtom,
            string storableId,
            string boolParamName)
        {
            if (recipientAtom == null)
            {
                return;
            }

            JSONStorable storable =
                recipientAtom.GetStorableByID(storableId);
            if (storable == null)
            {
                return;
            }

            JSONStorableBool enabledParam =
                storable.GetBoolJSONParam(boolParamName);
            if (enabledParam != null && enabledParam.val)
            {
                enabledParam.val = false;
            }
        }

        private static void DefaultMorphByUID(DAZMorphBank bank, string morphUid)
        {
            if (bank == null || morphUid == null)
            {
                return;
            }

            DAZMorph morph = bank.GetMorphByUid(morphUid);
            if (morph != null)
            {
                morph.morphValue = morph.startValue;
            }
        }

        private static void ResetPoseMorphs(DAZCharacterSelector geom)
        {
            if (geom == null)
            {
                return;
            }

            ResetPoseMorphsInBank(geom.morphBank1);
            ResetPoseMorphsInBank(geom.morphBank2);
            ResetPoseMorphsInBank(geom.morphBank3);
        }

        private static void ResetPoseMorphsInBank(DAZMorphBank bank)
        {
            if (bank == null || bank.morphs == null)
            {
                return;
            }

            foreach (DAZMorph morph in bank.morphs)
            {
                if (morph != null && morph.isPoseControl)
                {
                    morph.SetDefaultValue();
                }
            }
        }

        private static void SmoothMorphBanks(DAZCharacterSelector geom)
        {
            if (geom == null)
            {
                return;
            }

            DAZCharacterRun run = geom.GetComponentInChildren<DAZCharacterRun>();
            if (run != null)
            {
                run.SmoothApplyMorphs();
                return;
            }

            if (geom.morphBank1 != null)
            {
                geom.morphBank1.ApplyMorphsImmediate();
            }
            if (geom.morphBank2 != null)
            {
                geom.morphBank2.ApplyMorphsImmediate();
            }
            if (geom.morphBank3 != null)
            {
                geom.morphBank3.ApplyMorphsImmediate();
            }
        }

        private static bool RegionMatchesHeadExcludedSet(DAZMorph morph)
        {
            if (morph == null)
            {
                return false;
            }

            string region = morph.resolvedRegionName;
            if (region == null || region.Length == 0)
            {
                return false;
            }

            string lowered = region.ToLowerInvariant();
            for (int i = 0; i < HeadRegionTokens.Length; i++)
            {
                string token = HeadRegionTokens[i];
                if (lowered.IndexOf(token, StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static string CanonicalMorphOrHairId(string rawId)
        {
            if (rawId == null)
            {
                return string.Empty;
            }

            string trimmed = rawId.Trim();
            if (trimmed.StartsWith("SELF:", StringComparison.Ordinal))
            {
                trimmed = trimmed.Substring("SELF:".Length).TrimStart();
            }

            return trimmed.Replace('\\', '/');
        }

        private static DAZMorph ResolveMorphFromPresetMorphJson(
            GenerateDAZMorphsControlUI morphCtrl,
            JSONClass morphJson)
        {
            if (morphCtrl == null || morphJson == null)
            {
                return null;
            }

            JSONNode uidNode = morphJson["uid"];
            if (uidNode != null &&
                uidNode.Value != null &&
                uidNode.Value.Length > 0)
            {
                string rawUid = uidNode.Value.Trim();
                DAZMorph byUid = morphCtrl.GetMorphByUid(rawUid);
                if (byUid != null)
                {
                    return byUid;
                }

                string canonicalUid = CanonicalMorphOrHairId(rawUid);
                DAZMorph byCanonicalUid =
                    morphCtrl.GetMorphByUid(canonicalUid);
                if (byCanonicalUid != null)
                {
                    return byCanonicalUid;
                }
            }

            JSONNode nameNode = morphJson["name"];
            if (nameNode != null &&
                nameNode.Value != null &&
                nameNode.Value.Length > 0)
            {
                return morphCtrl.GetMorphByDisplayName(nameNode.Value);
            }

            return null;
        }

        private static bool TryReadPresetGeometry(
            string relativePathNormalized,
            out JSONArray presetMorphArray,
            out string err)
        {
            presetMorphArray = null;
            err = string.Empty;

            if (SuperController.singleton == null)
            {
                err = "SuperController unavailable.";
                return false;
            }

            string jsonText =
                SuperController.singleton.ReadFileIntoString(
                    relativePathNormalized);
            if (jsonText == null || jsonText.Length == 0)
            {
                err = string.Concat(
                    "Empty or unreadable preset: ",
                    relativePathNormalized);
                return false;
            }

            JSONNode rootNode = JSON.Parse(jsonText);
            JSONClass rootJson = rootNode != null ? rootNode.AsObject : null;
            if (rootJson == null)
            {
                err = "Not valid JSON preset.";
                return false;
            }

            JSONClass presetGeomJson = FindGeometryPreset(rootJson);
            if (presetGeomJson == null)
            {
                err = string.Concat(
                    "No geometry section found in ",
                    relativePathNormalized,
                    ".");
                return false;
            }

            JSONNode morphNode = presetGeomJson["morphs"];
            presetMorphArray = morphNode != null ? morphNode.AsArray : null;
            if (presetMorphArray == null)
            {
                err = "Preset geometry lacks morph array.";
                return false;
            }

            return true;
        }

        private static JSONClass FindGeometryPreset(JSONClass rootJson)
        {
            if (rootJson == null)
            {
                return null;
            }

            JSONNode idNode = rootJson["id"];
            if (idNode != null && idNode.Value == "geometry")
            {
                return rootJson;
            }

            JSONNode rootMorphsNode = rootJson["morphs"];
            if (rootMorphsNode != null && rootMorphsNode.AsArray != null)
            {
                return rootJson;
            }

            JSONClass fromRootStorables = FindGeometryInStorables(
                rootJson["storables"] != null ? rootJson["storables"].AsArray :
                null);
            if (fromRootStorables != null)
            {
                return fromRootStorables;
            }

            JSONArray atoms = rootJson["atoms"] != null ?
                rootJson["atoms"].AsArray :
                null;
            if (atoms == null)
            {
                return null;
            }

            for (int i = 0; i < atoms.Count; i++)
            {
                JSONClass atomJson = atoms[i].AsObject;
                if (atomJson == null)
                {
                    continue;
                }

                string type = atomJson["type"];
                if (type != "Person")
                {
                    continue;
                }

                JSONClass fromPersonStorables = FindGeometryInStorables(
                    atomJson["storables"] != null ?
                        atomJson["storables"].AsArray :
                        null);
                if (fromPersonStorables != null)
                {
                    return fromPersonStorables;
                }
            }

            for (int i = 0; i < atoms.Count; i++)
            {
                JSONClass atomJson = atoms[i].AsObject;
                if (atomJson == null)
                {
                    continue;
                }

                JSONClass fromAnyStorables = FindGeometryInStorables(
                    atomJson["storables"] != null ?
                        atomJson["storables"].AsArray :
                        null);
                if (fromAnyStorables != null)
                {
                    return fromAnyStorables;
                }
            }

            return null;
        }

        private static JSONClass FindGeometryInStorables(JSONArray storables)
        {
            if (storables == null)
            {
                return null;
            }

            for (int i = 0; i < storables.Count; i++)
            {
                JSONClass storableJson = storables[i].AsObject;
                if (storableJson == null)
                {
                    continue;
                }

                JSONNode sid = storableJson["id"];
                if (sid != null && sid.Value == "geometry")
                {
                    return storableJson;
                }
            }

            return null;
        }
    }
}
