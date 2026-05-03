using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SimpleJSON;

/// <summary>
/// Applies head-region morph sliders and enabled hair-slot entries from
/// Saves/Person (.json person presets). Walks preset morph rows so totals
/// are not inflated by sliders your figure lacks in the preset.
/// </summary>
public class HeadMorphSwap : MVRScript
{
    /// <summary>
    /// Logs to VaM error buffer and opens Error HUD (helps desktop mirror use).
    /// </summary>
    private static void LogErrorShowHud(string message)
    {
        SuperController sc = SuperController.singleton;
        if (sc != null && !sc.IsMonitorOnly)
        {
            sc.ShowMainHUD(setAnchors: true, forceMonitor: false);
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

    private JSONStorableUrl donorPersonPresetPathUrl;

    private JSONStorableStringChooser recipientChooser;

    private JSONStorableString regionSubstringsCsv;

    private JSONStorableBool requireSameSex;

    private JSONStorableString statusLine;

    private JSONStorableAction applyAction;

    private void SyncRecipientChoices()
    {
        List<string> choices = PersonUidChoices(
            recipientChooser != null ? recipientChooser.val : null);
        recipientChooser.choices = choices;
        NormalizeRecipientChooser(recipientChooser, choices);
    }

    private static void NormalizeRecipientChooser(
        JSONStorableStringChooser ch,
        List<string> choices)
    {
        if (ch == null || choices == null || choices.Count == 0)
        {
            return;
        }
        int idx = choices.FindIndex(delegate(string uid)
            { return uid.Equals(ch.val); });
        if (idx < 0)
        {
            ch.valNoCallback = choices[0];
        }
    }

    private static List<string> PersonUidChoices(string preserveIfPresent)
    {
        List<string> list = SuperController.singleton != null ?
            SuperController.singleton.GetAtomUIDs() : new List<string>();
        List<string> persons = list.Where(CheckPersonUid).ToList();
        if (preserveIfPresent != null &&
            !persons.Exists(preserveIfPresent.Equals) &&
            SuperController.singleton != null)
        {
            Atom late = SuperController.singleton.GetAtomByUid(preserveIfPresent);
            if (late != null && late.type == "Person")
            {
                persons.Add(preserveIfPresent);
            }
        }
        persons.Sort(string.CompareOrdinal);
        return persons;
    }

    private static bool CheckPersonUid(string uid)
    {
        if (SuperController.singleton == null)
        {
            return false;
        }
        Atom atom = SuperController.singleton.GetAtomByUid(uid);
        return atom != null && atom.type == "Person";
    }

    private static bool IsPersonReady(Atom person)
    {
        if (person == null || person.type != "Person")
        {
            return false;
        }
        DAZCharacter dc = person.GetComponentInChildren<DAZCharacter>();
        DAZCharacterSelector geom = GetGeometry(person);
        return dc != null && geom != null && geom.morphsControlUI != null;
    }

    private static DAZCharacterSelector GetGeometry(Atom person)
    {
        if (person == null)
        {
            return null;
        }
        return person.GetStorableByID("geometry") as DAZCharacterSelector;
    }

    /// <returns>Lower case trim tokens.</returns>
    private List<string> ParseRegionTokens(string csv)
    {
        List<string> tokens = new List<string>();
        if (csv == null)
        {
            return tokens;
        }
        string trimmed = csv.Trim();
        int len = trimmed.Length;
        int start = 0;
        for (int i = 0; i <= len; i++)
        {
            if (i == len || trimmed[i] == ',')
            {
                int sliceLen = i - start;
                if (sliceLen > 0)
                {
                    string token = trimmed.Substring(start, sliceLen).Trim().
                        ToLowerInvariant();
                    if (token.Length > 0)
                    {
                        tokens.Add(token);
                    }
                }
                start = i + 1;
            }
        }
        return tokens;
    }

    private bool RegionMatchesMorph(DAZMorph morph, List<string> tokensLower)
    {
        if (morph == null)
        {
            return false;
        }
        string region = morph.resolvedRegionName;
        if (region == null || region == string.Empty)
        {
            return false;
        }
        string rn = region.ToLowerInvariant();
        for (int ti = 0; ti < tokensLower.Count; ti++)
        {
            string t = tokensLower[ti];
            if (t.Length == 0)
            {
                continue;
            }
            if (rn.IndexOf(t, StringComparison.Ordinal) >= 0)
            {
                return true;
            }
        }
        return false;
    }

    private static void SmoothMorphBanks(DAZCharacterSelector rGeom)
    {
        if (rGeom == null)
        {
            return;
        }
        DAZCharacterRun run = rGeom.GetComponentInChildren<DAZCharacterRun>();
        if (run != null)
        {
            run.SmoothApplyMorphs();
        }
        else
        {
            if (rGeom.morphBank1 != null)
            {
                rGeom.morphBank1.ApplyMorphsImmediate();
            }
            if (rGeom.morphBank2 != null)
            {
                rGeom.morphBank2.ApplyMorphsImmediate();
            }
            if (rGeom.morphBank3 != null)
            {
                rGeom.morphBank3.ApplyMorphsImmediate();
            }
        }
    }

    /// <summary>
    /// VaM forbids MVR.FileManagement in script plugins — trim, SELF: strip,
    /// slash unify only (no VAR id rewriting).
    /// </summary>
    private static string CanonicalMorphOrHairId(string rawId)
    {
        if (rawId == null)
        {
            return string.Empty;
        }
        string t = rawId.Trim();
        if (t.StartsWith("SELF:", StringComparison.Ordinal))
        {
            string selfPrefix = "SELF:";
            t = t.Substring(selfPrefix.Length).TrimStart();
        }
        return t.Replace('\\', '/');
    }

    /// <returns>Matched morph slider on recipient or null.</returns>
    private static DAZMorph ResolveMorphFromPresetMorphJson(
        GenerateDAZMorphsControlUI morphCtrl,
        JSONClass jc)
    {
        if (morphCtrl == null || jc == null)
        {
            return null;
        }
        JSONNode uidNode = jc["uid"];
        if (uidNode != null &&
            !(uidNode.Value == null || uidNode.Value == string.Empty))
        {
            string rawUid = uidNode.Value.Trim();
            DAZMorph byUidRaw = morphCtrl.GetMorphByUid(rawUid);
            if (byUidRaw != null)
            {
                return byUidRaw;
            }
            string canonUidStr = CanonicalMorphOrHairId(rawUid);
            DAZMorph byUidCanon = morphCtrl.GetMorphByUid(canonUidStr);
            if (byUidCanon != null)
            {
                return byUidCanon;
            }
        }
        JSONNode nameNode = jc["name"];
        if (nameNode != null &&
            !(nameNode.Value == null || nameNode.Value == string.Empty))
        {
            return morphCtrl.GetMorphByDisplayName(nameNode.Value);
        }
        return null;
    }

    private static bool RecipientSexFailsPresetCharacterField(
        DAZCharacter rc,
        JSONClass presetGeom)
    {
        if (rc == null || presetGeom == null)
        {
            return false;
        }
        string chRaw = presetGeom["character"];
        if (chRaw == null || chRaw.Length == 0)
        {
            return false;
        }
        string low = chRaw.ToLowerInvariant();
        bool hasFemale =
            low.IndexOf("female", StringComparison.Ordinal) >= 0;
        bool hasMale =
            low.IndexOf("male", StringComparison.Ordinal) >= 0;
        if (hasFemale && hasMale)
        {
            return false;
        }
        if (hasFemale)
        {
            return rc.isMale;
        }
        if (hasMale)
        {
            return !rc.isMale;
        }
        return false;
    }

    /// <returns>geometry storable JSON or null.</returns>
    private static JSONClass FindFirstGeometryInPersonPreset(JSONClass sceneRoot)
    {
        if (sceneRoot == null)
        {
            return null;
        }
        JSONNode atomsNode = sceneRoot["atoms"];
        JSONArray atoms = atomsNode != null ? atomsNode.AsArray : null;
        if (atoms == null)
        {
            return null;
        }
        for (int ai = 0; ai < atoms.Count; ai++)
        {
            JSONClass atomJson = atoms[ai].AsObject;
            if (atomJson == null)
            {
                continue;
            }
            string tp = atomJson["type"];
            if (tp != "Person")
            {
                continue;
            }
            JSONNode stNode = atomJson["storables"];
            JSONArray storArr =
                stNode != null ? stNode.AsArray : null;
            if (storArr == null)
            {
                continue;
            }
            for (int si = 0; si < storArr.Count; si++)
            {
                JSONClass stjc = storArr[si].AsObject;
                if (stjc == null)
                {
                    continue;
                }
                JSONNode sid = stjc["id"];
                if (sid != null && sid.Value == "geometry")
                {
                    return stjc;
                }
            }
        }
        return null;
    }

    private static bool TryReadPersonPresetGeometry(
        string relativePathNormalized,
        out JSONClass presetGeomJson,
        out JSONArray presetMorphArray,
        out string err)
    {
        presetGeomJson = null;
        presetMorphArray = null;
        err = "";
        string jsonText =
            SuperController.singleton.ReadFileIntoString(relativePathNormalized);
        if (jsonText == null || jsonText.Length == 0)
        {
            err = "Empty or unreadable preset.";
            return false;
        }
        JSONClass rootJC = JSON.Parse(jsonText).AsObject;
        if (rootJC == null)
        {
            err = "Not valid JSON preset.";
            return false;
        }
        presetGeomJson = FindFirstGeometryInPersonPreset(rootJC);
        if (presetGeomJson == null)
        {
            err = "No Person geometry section (Saves/Person .json?).";
            return false;
        }
        JSONNode morNode = presetGeomJson["morphs"];
        presetMorphArray = morNode != null ? morNode.AsArray : null;
        if (presetMorphArray == null)
        {
            err = "Preset geometry lacks morph array.";
            return false;
        }
        return true;
    }

    private static string TrimPresetPathInput(string rawVal)
    {
        if (rawVal == null)
        {
            return string.Empty;
        }
        return rawVal.Trim().Replace('\\', '/').Trim();
    }

    private void CopyHairSlotsFromPresetJson(
        DAZCharacterSelector recipientGeom,
        JSONClass presetGeomJson,
        ref int synced,
        ref int skipped)
    {
        if (presetGeomJson == null || recipientGeom == null)
        {
            return;
        }
        JSONNode hairNode = presetGeomJson["hair"];
        JSONArray ha = hairNode != null ? hairNode.AsArray : null;
        if (ha == null)
        {
            return;
        }
        recipientGeom.RemoveAllHair();
        for (int hi = 0; hi < ha.Count; hi++)
        {
            JSONClass itemJson = ha[hi].AsObject;
            if (itemJson == null)
            {
                continue;
            }
            JSONNode enNode = itemJson["enabled"];
            if (enNode != null && !(enNode.AsBool))
            {
                continue;
            }
            JSONNode idNodePrimary = itemJson["id"];
            string textPrimary =
                idNodePrimary != null ? idNodePrimary.Value : null;

            string itemIdActivation = string.Empty;

            DAZHairGroup hairFound = null;

            if (textPrimary != null && textPrimary != string.Empty)
            {
                string trimmedId = textPrimary.Trim();
                if (trimmedId.Length > 0)
                {
                    hairFound = recipientGeom.GetHairItem(trimmedId);
                    if (hairFound != null)
                    {
                        itemIdActivation = trimmedId;
                    }

                    if (hairFound == null)
                    {
                        string canonicalHairId =
                            CanonicalMorphOrHairId(trimmedId);
                        hairFound = recipientGeom.GetHairItem(
                            canonicalHairId);
                        if (hairFound != null)
                        {
                            itemIdActivation = canonicalHairId;
                        }
                    }
                }
            }

            JSONNode internalNode = itemJson["internalId"];
            string backupId = internalNode != null ?
                internalNode.Value :
                null;
            if (hairFound == null &&
                !(backupId == null || backupId == string.Empty))
            {
                hairFound = recipientGeom.GetHairItem(backupId);
                if (hairFound != null)
                {
                    itemIdActivation = backupId;
                }
            }
            if (hairFound != null &&
                !(itemIdActivation == string.Empty))
            {
                recipientGeom.SetActiveHairItem(itemIdActivation, true, false);
                synced++;
            }
            else
            {
                skipped++;
            }
        }
    }

    /// <summary>Apply morph + hair strings from Morrigan preset (etc.).</summary>
    private void ApplyMorphsFromPresetFile(string presetRelativePath)
    {
        string rid = recipientChooser.val;
        if (rid == null || rid.Length == 0)
        {
            statusLine.val = "Select recipient Person.";
            return;
        }
        if (SuperController.singleton == null)
        {
            statusLine.val = "SuperController unavailable.";
            return;
        }
        Atom recipientAtom = SuperController.singleton.GetAtomByUid(rid);
        if (!IsPersonReady(recipientAtom))
        {
            statusLine.val = "Recipient morph bank not ready.";
            return;
        }

        JSONClass presetGeomJson;
        JSONArray presetMorphArr;
        string presetErr;
        if (!TryReadPersonPresetGeometry(
            presetRelativePath,
            out presetGeomJson,
            out presetMorphArr,
            out presetErr))
        {
            statusLine.val = presetErr;
            LogErrorShowHud(string.Concat("[HeadMorphSwap] ", presetErr));
            return;
        }

        if (requireSameSex.val)
        {
            DAZCharacter rc =
                recipientAtom.GetComponentInChildren<DAZCharacter>();
            if (RecipientSexFailsPresetCharacterField(rc, presetGeomJson))
            {
                statusLine.val = "Sex mismatch preset vs recipient; loosen gate.";
                return;
            }
        }

        List<string> regionTokens = ParseRegionTokens(regionSubstringsCsv.val);
        if (regionTokens.Count == 0)
        {
            statusLine.val = "Add comma-separated region substrings.";
            return;
        }

        DAZCharacterSelector recipientGeom = GetGeometry(recipientAtom);
        GenerateDAZMorphsControlUI morphCtrl = recipientGeom.morphsControlUI;

        int appliedHead = 0;
        int unresolvedPresetMorphs = 0;

        for (int pi = 0; pi < presetMorphArr.Count; pi++)
        {
            JSONClass jc = presetMorphArr[pi].AsObject;
            if (jc == null)
            {
                continue;
            }
            DAZMorph tgtMorph = ResolveMorphFromPresetMorphJson(morphCtrl, jc);
            if (tgtMorph == null)
            {
                unresolvedPresetMorphs++;
                continue;
            }
            if (tgtMorph.isPoseControl || tgtMorph.disable)
            {
                continue;
            }
            if (!RegionMatchesMorph(tgtMorph, regionTokens))
            {
                continue;
            }
            tgtMorph.RestoreFromJSON(jc);
            appliedHead++;
        }

        SmoothMorphBanks(recipientGeom);

        int hairSynced = 0;
        int hairSkipped = 0;
        CopyHairSlotsFromPresetJson(
            recipientGeom,
            presetGeomJson,
            ref hairSynced,
            ref hairSkipped);

        statusLine.val = string.Concat(
            "Applied ",
            appliedHead.ToString(),
            " head sliders from preset rows. ",
            unresolvedPresetMorphs.ToString(),
            " preset morphs not installed on figure (addons). ",
            "Hair sync ",
            hairSynced.ToString(),
            ", unavailable ",
            hairSkipped.ToString(),
            ". Body morph rows preset stay ignored.");
        SuperController.singleton.Message(statusLine.val);
    }

    private void ApplyHeadAndHairFromPersonPresetEntry()
    {
        string rid = recipientChooser.val;
        if (rid == null || rid.Length == 0)
        {
            statusLine.val = "Select recipient Person.";
            return;
        }

        string presetPathCandidate = TrimPresetPathInput(
            donorPersonPresetPathUrl != null ?
                donorPersonPresetPathUrl.val :
                "");
        if (presetPathCandidate.Length == 0)
        {
            statusLine.val = "Browse Saves/Person .json (path required).";
            return;
        }

        ApplyMorphsFromPresetFile(presetPathCandidate);
    }

    public override void Init()
    {
        try
        {
            recipientChooser =
                new JSONStorableStringChooser(
                    "recipientPerson",
                    new List<string>(),
                    "",
                    "Recipient Person");
            RegisterStringChooser(recipientChooser);
            SyncRecipientChoices();

            UIDynamicPopup recipientPopup = CreateScrollablePopup(
                recipientChooser,
                false);
            recipientPopup.popupPanelHeight = 700f;
            recipientPopup.popup.onOpenPopupHandlers += SyncRecipientChoices;

            donorPersonPresetPathUrl = new JSONStorableUrl(
                "donorPersonPresetPath",
                string.Empty,
                delegate(string unusedPathSync)
                {
                    NotifyPresetChosen();
                },
                "json",
                "Saves/Person");

            donorPersonPresetPathUrl.showDirs = true;
            RegisterUrl(donorPersonPresetPathUrl);

            UIDynamicButton browsePresetBtn =
                CreateButton("Browse Saves/Person .json");
            if (browsePresetBtn != null && browsePresetBtn.button != null)
            {
                donorPersonPresetPathUrl.RegisterFileBrowseButton(
                    browsePresetBtn.button);
            }

            UIDynamicTextField presetPathTf =
                CreateTextField(donorPersonPresetPathUrl, false);
            if (presetPathTf != null)
            {
                presetPathTf.height = 40f;
            }

            string defaultCsv =
                "head, face, mouth, lips, eyelash, eye, eyebrow, ear, brow, nose, tooth, tongue, cheek, chin, jaw, forehead, temple, scalp, neck, cranium";
            regionSubstringsCsv =
                new JSONStorableString(
                    "regionSubstringsCsv",
                    defaultCsv);

            RegisterString(regionSubstringsCsv);

            CreateTextField(regionSubstringsCsv, false);

            requireSameSex = new JSONStorableBool(
                "requireSameSexMorphBanks",
                true);

            RegisterBool(requireSameSex);

            CreateToggle(requireSameSex, false);

            applyAction = new JSONStorableAction(
                "applyHeadMorphsFromDonor",
                ApplyHeadAndHairFromPersonPresetEntry);

            RegisterAction(applyAction);

            UIDynamicButton goButton = CreateButton("Apply head + hair preset");

            if (goButton != null && goButton.button != null)
            {
                goButton.button.onClick.AddListener(
                    delegate() { ApplyHeadAndHairFromPersonPresetEntry(); });

            }

            statusLine = new JSONStorableString(
                "lastSwapStatus",
                string.Concat(
                    "Recipient, then Saves/Person .json path ",
                    "(e.g. Saves/Person/full/…). Morph list filters by region substring list. ",
                    "Preset hair replaces current hair slots when preset lists hair."));
            RegisterString(statusLine);

            CreateTextField(statusLine, false);

            if (containingAtom != null &&
                containingAtom.type == "Person" &&
                recipientChooser != null &&
                recipientChooser.choices != null &&
                recipientChooser.choices.Exists(
                    delegate(string uid)
                    {
                        return uid.Equals(containingAtom.uid);
                    }))
            {
                recipientChooser.valNoCallback = containingAtom.uid;
                SyncRecipientChoices();
                recipientChooser.valNoCallback = containingAtom.uid;
            }
        }
        catch (Exception e)
        {
            LogErrorShowHud(string.Concat("HeadMorphSwap Init ", e.ToString()));
        }
    }

    private void NotifyPresetChosen()
    {
        if (statusLine == null)
        {
            return;
        }

        string p = TrimPresetPathInput(
            donorPersonPresetPathUrl != null ?
                donorPersonPresetPathUrl.val :
                string.Empty);

        statusLine.val = p.Length > 0 ?
            string.Concat("Preset armed: ", p) :
            "Pick a Saves/Person .json then Apply.";
    }
}
