using System;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

namespace SkinColorSwapNs
{
    /// <summary>
    /// Copies only donor preset Skin Color HSV onto recipient Person skin
    /// storable. Leaves morphs, hair, gloss, face/nails, subsurface/spec.
    /// </summary>
    public class SkinColorSwap : MVRScript
    {
        private const string SkinStorableId = "skin";

        private const string SkinColorParamName = "Skin Color";

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
                    "[SkinColorSwap] Init failed: ",
                    e.ToString()));
            }
        }

        private void BuildUi()
        {
            _donorPresetPath = new JSONStorableUrl(
                "donorPresetPath_skin_color",
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
                "applySkinColorFromPreset",
                ApplyFromCurrentPreset);
            RegisterAction(_applyAction);

            UIDynamicButton applyButton =
                CreateButton("Apply Skin Color");
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
                "statusLine_skin_color",
                InitialStatusText());
            RegisterString(_statusLine);

            UIDynamicTextField statusField =
                CreateTextField(_statusLine, false);
            if (statusField != null)
            {
                statusField.height = 100f;
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
                    "Attach this plugin to the Person that receives the ",
                    "donor skin tone.");
            }

            return string.Concat(
                "Recipient: ",
                containingAtom.uid,
                ". Pick a Saves/Person preset; only Skin Color ",
                "(HSV) is copied.");
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
                ApplyFromPresetPath(presetPath);
            }
        }

        private void ApplyFromCurrentPreset()
        {
            string presetPath = NormalizePresetPathInput(
                _donorPresetPath != null ? _donorPresetPath.val : string.Empty);
            ApplyFromPresetPath(presetPath);
        }

        private void ApplyFromPresetPath(string presetRelativePath)
        {
            Atom recipientAtom;
            string recipientErr;
            if (!TryGetRecipientPerson(out recipientAtom, out recipientErr))
            {
                SetStatus(recipientErr);
                return;
            }

            if (presetRelativePath == null || presetRelativePath.Length == 0)
            {
                SetStatus("Pick a donor .json first.");
                return;
            }

            string readErr;
            JSONClass presetRoot;
            if (!TryReadPresetRoot(presetRelativePath, out presetRoot, out readErr))
            {
                SetStatus(readErr);
                LogErrorShowHud(string.Concat("[SkinColorSwap] ", readErr));
                return;
            }

            JSONClass donorSkinStorableJson = FindSkinStorableJson(presetRoot);
            if (donorSkinStorableJson == null)
            {
                string msg =
                    "Preset has no storables[\"skin\"] Person section.";
                SetStatus(msg);
                LogErrorShowHud(string.Concat("[SkinColorSwap] ", msg));
                return;
            }

            JSONNode colorNode = donorSkinStorableJson[SkinColorParamName];
            JSONClass hsvObj = colorNode != null ? colorNode.AsObject : null;
            if (hsvObj == null)
            {
                string msg =
                    "Donor preset skin block has no Skin Color HSV.";
                SetStatus(msg);
                LogErrorShowHud(string.Concat("[SkinColorSwap] ", msg));
                return;
            }

            float hVal;
            float sVal;
            float vVal;
            if (!TryReadHsv(hsvObj, out hVal, out sVal, out vVal))
            {
                string msg =
                    "Skin Color in preset is missing h/s/v numbers.";
                SetStatus(msg);
                LogErrorShowHud(string.Concat("[SkinColorSwap] ", msg));
                return;
            }

            JSONStorable recvSkin =
                recipientAtom.GetStorableByID(SkinStorableId);
            if (recvSkin == null)
            {
                SetStatus(
                    "Recipient Person has no storables[\"skin\"] yet.");
                return;
            }

            JSONStorableColor recvSkinTone =
                recvSkin.GetColorJSONParam(SkinColorParamName);
            if (recvSkinTone == null)
            {
                string msg = string.Concat(
                    "Recipient skin has no param \"",
                    SkinColorParamName,
                    "\".");
                SetStatus(msg);
                LogErrorShowHud(string.Concat("[SkinColorSwap] ", msg));
                return;
            }

            recvSkinTone.SetVal(hVal, sVal, vVal);

            string ok = string.Concat(
                "Skin Color set from ",
                presetRelativePath,
                " (H=",
                hVal.ToString("F4"),
                " S=",
                sVal.ToString("F4"),
                " V=",
                vVal.ToString("F4"),
                ").");
            SetStatus(ok);
            if (SuperController.singleton != null)
            {
                SuperController.singleton.Message(ok);
            }
        }

        private void SetStatus(string message)
        {
            if (_statusLine != null)
            {
                _statusLine.val = message;
            }
        }

        private bool TryGetRecipientPerson(out Atom recipientAtom, out string err)
        {
            recipientAtom = containingAtom;
            err = string.Empty;

            if (recipientAtom == null || recipientAtom.type != "Person")
            {
                err =
                    "Attach SkinColorSwap to the target Person.";
                return false;
            }

            JSONStorable skinProbe =
                recipientAtom.GetStorableByID(SkinStorableId);
            if (skinProbe == null)
            {
                err = string.Concat(
                    "Recipient has no storables[\"",
                    SkinStorableId,
                    "\"] loaded yet.");
                return false;
            }

            return true;
        }

        private static bool TryReadPresetRoot(
            string presetRelativePath,
            out JSONClass rootJson,
            out string err)
        {
            rootJson = null;
            err = string.Empty;

            if (SuperController.singleton == null)
            {
                err = "SuperController unavailable.";
                return false;
            }

            string text =
                SuperController.singleton.ReadFileIntoString(presetRelativePath);
            if (text == null || text.Length == 0)
            {
                err = string.Concat(
                    "Empty / unreadable: ",
                    presetRelativePath);
                return false;
            }

            JSONNode node = JSON.Parse(text);
            rootJson = node != null ? node.AsObject : null;
            if (rootJson == null)
            {
                err = "Not valid JSON object.";
                return false;
            }

            return true;
        }

        private static JSONClass FindSkinStorableJson(JSONClass presetRoot)
        {
            if (presetRoot == null)
            {
                return null;
            }

            JSONClass fromPersonStorables =
                PickSkinFromPersonAtomsFirst(presetRoot);
            if (fromPersonStorables != null)
            {
                return fromPersonStorables;
            }

            JSONArray rootSt =
                presetRoot["storables"] != null ?
                presetRoot["storables"].AsArray :
                null;
            return FindSkinInStorableArray(rootSt);
        }

        private static JSONClass PickSkinFromPersonAtomsFirst(
            JSONClass presetRoot)
        {
            JSONArray atoms = presetRoot["atoms"] != null ?
                presetRoot["atoms"].AsArray :
                null;
            if (atoms == null)
            {
                return null;
            }

            for (int i = 0; i < atoms.Count; i++)
            {
                JSONClass atomJo = atoms[i].AsObject;
                if (atomJo == null)
                {
                    continue;
                }

                string tp = atomJo["type"];
                if (tp != "Person")
                {
                    continue;
                }

                JSONArray stArr = atomJo["storables"] != null ?
                    atomJo["storables"].AsArray :
                    null;

                JSONClass skin = FindSkinInStorableArray(stArr);
                if (skin != null)
                {
                    return skin;
                }
            }

            for (int j = 0; j < atoms.Count; j++)
            {
                JSONClass atomJo2 = atoms[j].AsObject;
                if (atomJo2 == null)
                {
                    continue;
                }

                JSONArray stArr2 = atomJo2["storables"] != null ?
                    atomJo2["storables"].AsArray :
                    null;

                JSONClass skin2 = FindSkinInStorableArray(stArr2);
                if (skin2 != null)
                {
                    return skin2;
                }
            }

            return null;
        }

        private static JSONClass FindSkinInStorableArray(JSONArray storables)
        {
            if (storables == null)
            {
                return null;
            }

            for (int i = 0; i < storables.Count; i++)
            {
                JSONClass st = storables[i].AsObject;
                if (st == null)
                {
                    continue;
                }

                JSONNode idNode = st["id"];
                if (idNode != null &&
                    string.Equals(idNode.Value,
                        SkinStorableId,
                        StringComparison.Ordinal))
                {
                    return st;
                }
            }

            return null;
        }

        private static bool TryReadHsv(
            JSONClass hsv,
            out float hOut,
            out float sOut,
            out float vOut)
        {
            hOut = 0f;
            sOut = 0f;
            vOut = 0f;

            if (hsv == null)
            {
                return false;
            }

            JSONNode hn = hsv["h"];
            JSONNode sn = hsv["s"];
            JSONNode vn = hsv["v"];
            if (hn == null || sn == null || vn == null)
            {
                return false;
            }

            hOut = hn.AsFloat;
            sOut = sn.AsFloat;
            vOut = vn.AsFloat;
            return true;
        }
    }
}
