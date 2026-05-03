using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Copies head / face morph values from another Person onto the recipient.
/// Body morphs, clothing, skin texture presets, and plugins stay as-is unless
/// you enable optional donor hair-slot matching.
/// </summary>
public class HeadMorphSwap : MVRScript
{
    private const string DonorNoneChoice = "--- pick donor ---";

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
        if (uid == null || uid == string.Empty ||
            uid.Equals(DonorNoneChoice))
        {
            return false;
        }
        return true;
    }

    /// <summary>Pick a usable uid whenever the popup list changes.</summary>
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

    private void ApplyMorphsFromDonor()
    {
        string rid = recipientChooser.val;
        string did = donorChooser.val;
        if (rid == null || rid.Length == 0)
        {
            statusLine.val = "Select recipient Person.";
            return;
        }
        if (did == null ||
            did.Length == 0 ||
            did.Equals(DonorNoneChoice))
        {
            statusLine.val = "Select donor Person.";
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
            statusLine.val = "Recipient and donor must be loaded Person atoms.";
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
                statusLine.val = "Sex differs; disable \"same sex\" or swap atoms.";
                return;
            }
        }

        List<string> regionTokens = ParseRegionTokens(regionSubstringsCsv.val);
        if (regionTokens.Count == 0)
        {
            statusLine.val = "Add comma-separated region substrings.";
            return;
        }

        DAZCharacterSelector rGeom = GetGeometry(recipientAtom);
        DAZCharacterSelector dGeom = GetGeometry(donorAtom);
        GenerateDAZMorphsControlUI rMorphs = rGeom.morphsControlUI;
        GenerateDAZMorphsControlUI dMorphs = dGeom.morphsControlUI;

        List<DAZMorph> tgtList = rMorphs.GetMorphs();
        if (tgtList == null)
        {
            statusLine.val = "Recipient morph bank not ready.";
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
                dMorphs.GetMorphByDisplayName(tmorph.resolvedDisplayName);
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

        int hairSynced = 0;
        int hairSkipped = 0;
        if (copyHairFromDonor.val)
        {
            rGeom.RemoveAllHair();
            DAZHairGroup[] donorHairItems = dGeom.hairItems;
            if (donorHairItems != null)
            {
                foreach (DAZHairGroup dh in donorHairItems)
                {
                    if (dh == null || !dh.active || dh.uid == null)
                    {
                        continue;
                    }
                    DAZHairGroup onReceiver = rGeom.GetHairItem(dh.uid);
                    if (onReceiver != null)
                    {
                        rGeom.SetActiveHairItem(dh.uid, true, false);
                        hairSynced++;
                    }
                    else
                    {
                        hairSkipped++;
                    }
                }
            }
        }

        if (copyHairFromDonor.val)
        {
            statusLine.val = string.Concat(
                "Morphs copied: ",
                copied.ToString(),
                ". Missing donor morph: ",
                noMatch.ToString(),
                ". Hair slots matched ",
                hairSynced.ToString(),
                "; not on recipient ",
                hairSkipped.ToString(),
                ".");
        }
        else
        {
            statusLine.val = string.Concat(
                "Morphs copied: ",
                copied.ToString(),
                ". Missing donor morph: ",
                noMatch.ToString(),
                ".");
        }
        SuperController.singleton.Message(statusLine.val);
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
                    "Recipient")
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
                    "Donor")
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
                    defaultCsv)
                ;
            RegisterString(regionSubstringsCsv);
            CreateTextField(regionSubstringsCsv, false);

            requireSameSex = new JSONStorableBool(
                "requireSameSexMorphBanks",
                true)
                ;
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
                CreateButton("Apply head morphs from donor");
            if (goButton != null && goButton.button != null)
            {
                goButton.button.onClick.AddListener(
                    delegate() { ApplyMorphsFromDonor(); });
            }

            statusLine = new JSONStorableString(
                "lastSwapStatus",
                "Substring list matches morph region text (comma-separated).");

            RegisterString(statusLine);
            CreateTextField(statusLine, false);

            if (containingAtom != null &&
                containingAtom.type == "Person" &&
                recipientChooser != null &&
                recipientChooser.choices != null &&
                recipientChooser.choices.Exists(
                    delegate(string uid) { return uid.Equals(containingAtom.uid); }))
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
}
