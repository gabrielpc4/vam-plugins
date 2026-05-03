using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SimpleJSON;
/// <summary>
/// Copies head / face morphs from Saves/Person .json presets OR from another
/// Person in the scene. If a preset path is set it wins over the scene donor.
/// </summary>
public class HeadMorphSwap : MVRScript
{
    private const string DonorNoneChoice = "--- pick donor ---";

    private JSONStorableUrl donorPersonPresetPathUrl;

    private JSONStorableStringChooser recipientChooser;

    private JSONStorableStringChooser donorChooser;

    private JSONStorableString regionSubstringsCsv;

    private JSONStorableBool requireSameSex;

    private JSONStorableBool copyHairFromDonor;

    private JSONStorableString statusLine;

    private JSONStorableAction applyAction;

    private void SyncRecipientChoices()
    {
        List<string> choices = PersonUidChoices(
            recipientChooser != null ? recipientChooser.val : null);
        recipientChooser.choices = choices;
        NormalizeRecipientChooser(recipientChooser, choices);
    }

    private void SyncDonorChoices()
    {
        string prior = donorChooser != null ? donorChooser.val : null;
        List<string> personsOnly = PersonUidChoices(
            IsRealPersonUid(prior) ? prior : null);
        List<string> withSentinel = new List<string>();
        withSentinel.Add(DonorNoneChoice);
        foreach (string uid in personsOnly)
        {
            withSentinel.Add(uid);
        }
        donorChooser.choices = withSentinel;
        if (prior == DonorNoneChoice)
        {
            donorChooser.valNoCallback = DonorNoneChoice;
        }
        else if (IsRealPersonUid(prior) &&
            personsOnly.Exists(delegate(string uid) { return uid.Equals(prior); }))
        {
            donorChooser.valNoCallback = prior;
        }
        else
        {
            donorChooser.valNoCallback = DonorNoneChoice;
        }
    }

    private static bool IsRealPersonUid(string uid)
    {
        if (uid == null ||
            uid == string.Empty ||
            uid.Equals(DonorNoneChoice))
        {
            return false;
        }
        return true;
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
    /// VaM forbids referencing MVR.FileManagement in script plugins.
    /// We only unify slashes and strip SELF: (VAR package rewriting is omitted).
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

    private static bool PresetMorphUidMatchesMorph(
        string presetUidRaw,
        string morphUidRaw)
    {
        string presetCanon = CanonicalMorphOrHairId(presetUidRaw);
        string morphCanon = CanonicalMorphOrHairId(morphUidRaw);
        if (presetCanon.Length == 0 ||
            morphCanon.Length == 0)
        {
            return false;
        }
        if (string.Equals(presetCanon, morphCanon, StringComparison.Ordinal))
        {
            return true;
        }
        return string.Equals(
            presetCanon,
            morphCanon,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <returns>Preset morph block for tm or null.</returns>
    private static JSONClass FindPresetMorphForTarget(
        JSONArray presetMorphArray,
        DAZMorph tm)
    {
        if (presetMorphArray == null || tm == null)
        {
            return null;
        }
        foreach (JSONNode presetNode in presetMorphArray)
        {
            JSONClass presetMorphJson = presetNode.AsObject;
            if (presetMorphJson == null)
            {
                continue;
            }
            JSONNode uidNode = presetMorphJson["uid"];
            if (uidNode != null &&
                !(uidNode.Value == null || uidNode.Value == string.Empty))
            {
                if (PresetMorphUidMatchesMorph(uidNode.Value, tm.uid))
                {
                    return presetMorphJson;
                }
            }
        }
        foreach (JSONNode presetNodeSecond in presetMorphArray)
        {
            JSONClass presetMorphJson2 = presetNodeSecond.AsObject;
            if (presetMorphJson2 == null)
            {
                continue;
            }
            JSONNode nameNode = presetMorphJson2["name"];
            if (nameNode != null &&
                tm.resolvedDisplayName != null &&
                string.Equals(
                    nameNode.Value,
                    tm.resolvedDisplayName,
                    StringComparison.Ordinal))
            {
                return presetMorphJson2;
            }
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
        bool hasFemale = low.IndexOf("female", StringComparison.Ordinal) >= 0;
        bool hasMale = low.IndexOf("male", StringComparison.Ordinal) >= 0;
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
        if (!(copyHairFromDonor.val &&
            presetGeomJson != null &&
            recipientGeom != null))
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

    private void CopyHairSlotsFromLivingDonor(
        DAZCharacterSelector recipientGeom,
        DAZCharacterSelector donorGeom,
        ref int synced,
        ref int skipped)
    {
        if (!(copyHairFromDonor.val &&
            recipientGeom != null &&
            donorGeom != null))
        {
            return;
        }
        recipientGeom.RemoveAllHair();
        DAZHairGroup[] donorHairItems = donorGeom.hairItems;
        if (donorHairItems == null)
        {
            return;
        }
        foreach (DAZHairGroup dh in donorHairItems)
        {
            if (dh == null || !dh.active || dh.uid == null)
            {
                continue;
            }
            if (recipientGeom.GetHairItem(dh.uid) != null)
            {
                recipientGeom.SetActiveHairItem(dh.uid, true, false);
                synced++;
            }
            else
            {
                skipped++;
            }
        }
    }

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
            SuperController.LogError(
                string.Concat("[HeadMorphSwap] ", presetErr));
            return;
        }

        if (requireSameSex.val)
        {
            DAZCharacter rc =
                recipientAtom.GetComponentInChildren<DAZCharacter>();
            if (RecipientSexFailsPresetCharacterField(rc, presetGeomJson))
            {
                statusLine.val = "Sex mismatch preset vs recipient; loosen check.";
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
        List<DAZMorph> tgtList = morphCtrl.GetMorphs();
        if (tgtList == null)
        {
            statusLine.val = "Recipient morph list missing.";
            return;
        }

        int copied = 0;
        int noMatch = 0;
        foreach (DAZMorph tmorph in tgtList)
        {
            if (tmorph == null || tmorph.isPoseControl || tmorph.disable)
            {
                continue;
            }
            if (!RegionMatchesMorph(tmorph, regionTokens))
            {
                continue;
            }
            JSONClass presetMorphJC =
                FindPresetMorphForTarget(presetMorphArr, tmorph);
            if (presetMorphJC == null)
            {
                noMatch++;
                continue;
            }
            tmorph.RestoreFromJSON(presetMorphJC);
            copied++;
        }

        SmoothMorphBanks(recipientGeom);

        int hairSynced = 0;
        int hairSkipped = 0;
        CopyHairSlotsFromPresetJson(
            recipientGeom,
            presetGeomJson,
            ref hairSynced,
            ref hairSkipped);

        PushStatusAfterApply(
            "preset",
            copied,
            noMatch,
            hairSynced,
            hairSkipped);
    }

    private void PushStatusAfterApply(
        string modeLabel,
        int copied,
        int noMatch,
        int hairSynced,
        int hairSkipped)
    {
        if (copyHairFromDonor.val)
        {
            statusLine.val = string.Concat(
                "(", modeLabel, ") Morphs ",
                copied.ToString(),
                ". Missing source ",
                noMatch.ToString(),
                ". Hair ",
                hairSynced.ToString(),
                "/",
                hairSkipped.ToString(),
                " synced/missing.");

        }
        else
        {
            statusLine.val = string.Concat(
                "(", modeLabel, ") Morphs ",
                copied.ToString(),
                ". Missing source ",
                noMatch.ToString(),
                ".");
        }
        SuperController.singleton.Message(statusLine.val);
    }

    private void ApplyMorphsFromSceneDonor(string rid, string did)
    {
        if (did == null ||
            did.Length == 0 ||
            did.Equals(DonorNoneChoice))
        {
            statusLine.val = "Select scene donor Person or-browse Saves/Person preset.";
            return;
        }
        if (rid == did)
        {
            statusLine.val = "Pick two different Persons.";
            return;
        }
        if (SuperController.singleton == null)
        {
            statusLine.val = "SuperController unavailable.";
            return;
        }
        Atom recipientAtom = SuperController.singleton.GetAtomByUid(rid);
        Atom donorAtom = SuperController.singleton.GetAtomByUid(did);
        if (!IsPersonReady(recipientAtom) || !IsPersonReady(donorAtom))
        {
            statusLine.val = "Recipient & donor Persons must finish loading.";
            return;
        }

        if (requireSameSex.val)
        {
            DAZCharacter rc = recipientAtom.GetComponentInChildren<DAZCharacter>();
            DAZCharacter dcDonor =
                donorAtom.GetComponentInChildren<DAZCharacter>();
            bool rMale = rc.isMale;
            bool dMale = dcDonor.isMale;
            if (rMale != dMale)
            {
                statusLine.val = "Sex differs; disable same-sex gate or swap.";
                return;
            }
        }

        List<string> regionTokens = ParseRegionTokens(regionSubstringsCsv.val);
        if (regionTokens.Count == 0)
        {
            statusLine.val = "Comma-separate morph region substring list.";
            return;
        }

        DAZCharacterSelector rGeom = GetGeometry(recipientAtom);
        DAZCharacterSelector dGeom = GetGeometry(donorAtom);
        GenerateDAZMorphsControlUI rm = rGeom.morphsControlUI;
        GenerateDAZMorphsControlUI dm = dGeom.morphsControlUI;

        List<DAZMorph> tgtList = rm.GetMorphs();
        if (tgtList == null)
        {
            statusLine.val = "Recipient morph list missing.";
            return;
        }

        int copied = 0;
        int noMatch = 0;
        foreach (DAZMorph tmorph in tgtList)
        {
            if (tmorph == null || tmorph.isPoseControl || tmorph.disable)
            {
                continue;
            }
            if (!RegionMatchesMorph(tmorph, regionTokens))
            {
                continue;
            }
            DAZMorph donorMorph =
                dm.GetMorphByDisplayName(tmorph.resolvedDisplayName);
            if (donorMorph == null ||
                donorMorph.isPoseControl ||
                donorMorph.disable)
            {
                noMatch++;
                continue;
            }
            tmorph.morphValue = donorMorph.morphValue;
            copied++;
        }

        SmoothMorphBanks(rGeom);

        int hairSynced = 0;
        int hairSkipped = 0;
        CopyHairSlotsFromLivingDonor(rGeom, dGeom, ref hairSynced, ref hairSkipped);

        PushStatusAfterApply(
            "scene",
            copied,
            noMatch,
            hairSynced,
            hairSkipped);
    }

    private void ApplyMorphsFromDonor()
    {
        string rid = recipientChooser.val;
        if (rid == null || rid.Length == 0)
        {
            statusLine.val = "Select recipient Person.";
            return;
        }

        string presetPathCandidate = TrimPresetPathInput(
            donorPersonPresetPathUrl != null ? donorPersonPresetPathUrl.val : "");
        if (presetPathCandidate.Length > 0)
        {
            ApplyMorphsFromPresetFile(presetPathCandidate);
            return;
        }

        ApplyMorphsFromSceneDonor(rid, donorChooser.val);
    }

    public override void Init()
    {
        try
        {
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

            recipientChooser =
                new JSONStorableStringChooser(
                    "recipientPerson",
                    new List<string>(),
                    "",
                    "Recipient Person")
                ;
            RegisterStringChooser(recipientChooser);
            SyncRecipientChoices();

            UIDynamicPopup recipientPopup = CreateScrollablePopup(
                recipientChooser,
                false);
            recipientPopup.popupPanelHeight = 700f;
            recipientPopup.popup.onOpenPopupHandlers += SyncRecipientChoices;

            donorChooser =
                new JSONStorableStringChooser(
                    "donorPerson",
                    new List<string>(),
                    DonorNoneChoice,
                    "Scene donor")
                ;
            RegisterStringChooser(donorChooser);
            SyncDonorChoices();

            UIDynamicPopup donorPopup =
                CreateScrollablePopup(donorChooser, false);
            donorPopup.popupPanelHeight = 700f;
            donorPopup.popup.onOpenPopupHandlers += SyncDonorChoices;

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

            copyHairFromDonor =
                new JSONStorableBool("copyHairFromDonorIfIdsMatch", false);

            RegisterBool(copyHairFromDonor);

            CreateToggle(copyHairFromDonor, false);

            applyAction = new JSONStorableAction(
                "applyHeadMorphsFromDonor",
                ApplyMorphsFromDonor);

            RegisterAction(applyAction);

            UIDynamicButton goButton =
                CreateButton(
                    "Apply head morphs");

            if (goButton != null && goButton.button != null)
            {
                goButton.button.onClick.AddListener(
                    delegate() { ApplyMorphsFromDonor(); });

            }

            statusLine = new JSONStorableString(
                "lastSwapStatus",
                string.Concat(
                    "Preset JSON path overrides scene donor.",
                    " Use Browse under Saves/Person (for example Saves/Person/full)."));

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
            SuperController.LogError("HeadMorphSwap Init " + e);
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
                "");

        statusLine.val = p.Length > 0 ?
            string.Concat("Preset armed: ", p) :

            string.Concat(
                "Preset cleared: using scene donor if selected.",
                " Use Saves/Person .json.");

    }
}
