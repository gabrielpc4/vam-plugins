using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utils.MorphsPreset;
using UnityEngine.UI;
using Utils.UIUtils;

namespace MorphMAS
{
    public class MorphMAS : MVRScript
    {
        private static int[] _shouldersVertices = new int[] { 13522, 187, 186, 11185, 11113, 11139, 211, 11114, 185, 257, 2710, 11115 };
        private List<ScannedMorph> _scannedMainMorphs;
        private List<ScannedMorph> _scannedGenitalMorphs;
        private List<UIDynamic> _currentUI;
        private List<UIDynamic> _morphPickingUI;
        private List<UIDynamic> _exportMorphsUI;
        private JSONStorableBool _splitHeadBodyStorable;
        private JSONStorableBool _autoFixHeadHeightStorable;
        private JSONStorableBool _exportHeadMorphStorable;
        private JSONStorableBool _exportBodyMorphStorable;
        private JSONStorableBool _exportWholeBodyMorphStorable;
        private JSONStorableBool _exportGenitalMorphStorable;
        private JSONStorableBool _exportAsPoseMorphStorable;
        private JSONStorableString _morphsNameStorable;
        private JSONStorableString _morphsCategoryStorable;
        private JSONStorableString _exportLogStorable;

        public override void Init()
        {
            InitStorables();
            ShowStartUI();
        }

        private void InitStorables()
        {
            _splitHeadBodyStorable = new JSONStorableBool("Splitted head/body morphs", false);
            _splitHeadBodyStorable.setCallbackFunction += (bool newValue) => UpdateExportUI();
            _autoFixHeadHeightStorable = new JSONStorableBool("Auto fix head height", false);
            _exportHeadMorphStorable = new JSONStorableBool("Export head morph", false);
            _exportBodyMorphStorable = new JSONStorableBool("Export body morph", false);
            _exportWholeBodyMorphStorable = new JSONStorableBool("Export whole body morph", false);
            _exportGenitalMorphStorable = new JSONStorableBool("Export genital morph", false);
            _exportAsPoseMorphStorable = new JSONStorableBool("Export as pose morph", false);

            _morphsNameStorable = new JSONStorableString("Morphs name:", "");
            _morphsCategoryStorable = new JSONStorableString("Morphs category:", "");
            _exportLogStorable = new JSONStorableString("Export log", "");
        }

        private void ResetUI()
        {
            UIUtils.RemoveUI(this, _currentUI);
            UIUtils.RemoveUI(this, _morphPickingUI);
            UIUtils.RemoveUI(this, _exportMorphsUI);
            _currentUI = new List<UIDynamic>();
        }

        public void ShowStartUI()
        {
            ResetUI();

            _currentUI.Add(UIUtils.CreateHeader(this, "Merge and split:", false, 30, Color.black, .1f));

            JSONStorableBool disableAutoBehaviorsStorable = new JSONStorableBool("Disable auto behaviours", true);
            UIDynamicToggle disableAutoBehaviorsToggle = CreateToggle(disableAutoBehaviorsStorable);
            _currentUI.Add(disableAutoBehaviorsToggle);

            JSONStorableBool disablePoseMorphsStorable = new JSONStorableBool("Disable pose morphs", true);
            UIDynamicToggle disablePoseMorphsToggle = CreateToggle(disablePoseMorphsStorable);
            _currentUI.Add(disablePoseMorphsToggle);

            _currentUI.Add(UIUtils.CreateSpacer(this, 20));

            UIDynamicButton nextStepButton = CreateButton("Next step →");
            _currentUI.Add(nextStepButton);

            nextStepButton.button.onClick.AddListener(() =>
            {
                StartCoroutine(MorphPickingStep(disableAutoBehaviorsStorable.val, disablePoseMorphsStorable.val));
            });

            _currentUI.Add(UIUtils.CreateHeader(this, "Other:", true, 30, Color.black, .1f));

            UIDynamicButton openMorphsFolderButton = CreateButton("Open morphs folder in explorer", true);
            _currentUI.Add(openMorphsFolderButton);
            openMorphsFolderButton.button.onClick.AddListener(() => SuperController.singleton.OpenFolderInExplorer("Custom/Atom/Person/Morphs"));
        }

        private void DisableAutoBehaviours()
        {
            DisableAutoBehaviour("AutoExpressions", "enabled");
            DisableAutoBehaviour("AutoJawMouthMorph", "enabled");
            DisableAutoBehaviour("BendFix", "enabled");
            DisableAutoBehaviour("BreastInOut", "enabled");
            DisableAutoBehaviour("EyelidControl", "blinkEnabled");
            DisableAutoBehaviour("EyelidControl", "eyelidLookMorphsEnabled");
            DisableAutoBehaviour("FemaleAnatomy", "enabled");
            DisableAutoBehaviour("MaleAnatomy", "enabled");

            // Eyelids morph don't get properly disabled :(
            DAZCharacterSelector characterSelector = containingAtom.GetComponentInChildren<DAZCharacterSelector>();
            DAZMorphBank bank1 = characterSelector.morphBank1;
            DefaultMorphByUID(bank1, "Eyelids Bottom Up Left");
            DefaultMorphByUID(bank1, "Eyelids Bottom Up Right");
        }

        private void DisableAutoBehaviour(string storableID, string boolParamName)
        {
            JSONStorableBool isAutoBehaviourEnabled = containingAtom.GetStorableByID(storableID)?.GetBoolJSONParam(boolParamName);
            if (isAutoBehaviourEnabled != null && isAutoBehaviourEnabled.val == true)
                isAutoBehaviourEnabled.val = false;
        }

        private void DefaultMorphByUID(DAZMorphBank bank, string morphUID)
        {
            DAZMorph morph = bank.GetMorphByUid(morphUID);
            if (morph != null) morph.morphValue = morph.startValue;
        }

        private IEnumerator MorphPickingStep(bool disableAutoBehaviors, bool disablePoseMorphs)
        {
            if (disableAutoBehaviors)
            {
                DisableAutoBehaviours();
                yield return new WaitForFixedUpdate();
                yield return new WaitForEndOfFrame();
            }
            ShowMorphPickingUI();
            ScanActiveMorphs();
            if (disablePoseMorphs)
            {
                SetPoseMorphsState(false);
            }
        }

        private void ShowMorphPickingUI()
        {
            ResetUI();

            UIDynamicButton previousStepButton = CreateButton("← Previous step");
            _currentUI.Add(previousStepButton);
            previousStepButton.button.onClick.AddListener(ShowStartUI);

            _currentUI.Add(UIUtils.CreateSpacer(this, 20));

            UIDynamicButton rescanActiveMorphsButton = CreateButton("Relist active morphs");
            _currentUI.Add(rescanActiveMorphsButton);
            rescanActiveMorphsButton.button.onClick.AddListener(ScanActiveMorphs);

            UIDynamicButton disablePoseMorphsButton = CreateButton("Disable pose morphs");
            _currentUI.Add(disablePoseMorphsButton);
            disablePoseMorphsButton.button.onClick.AddListener(() => SetPoseMorphsState(false));

            UIDynamicButton enablePoseMorphsButton = CreateButton("Enable pose morphs");
            _currentUI.Add(enablePoseMorphsButton);
            enablePoseMorphsButton.button.onClick.AddListener(() => SetPoseMorphsState(true));

            _currentUI.Add(UIUtils.CreateSpacer(this, 20));

            UIDynamicButton nextStepButton = CreateButton("Next step →");
            _currentUI.Add(nextStepButton);
            nextStepButton.button.onClick.AddListener(() => ExportStep(true));
        }

        private void ScanActiveMorphs()
        {
            DAZCharacterSelector characterSelector = containingAtom.GetComponentInChildren<DAZCharacterSelector>();
            DAZMorphBank bank1 = characterSelector.morphBank1;
            DAZMorphBank bank2 = characterSelector.morphBank2;
            DAZMorphBank bank3 = characterSelector.morphBank3;

            _scannedMainMorphs = ScanActiveBankMorphs(new DAZMorphBank[] { bank1 });
            _scannedGenitalMorphs = ScanActiveBankMorphs(new DAZMorphBank[] { bank2, bank3 });

            UpdateMorphPickingUI();
        }

        private void UpdateMorphPickingUI()
        {
            UIUtils.RemoveUI(this, _morphPickingUI);
            _morphPickingUI = new List<UIDynamic>();

            _morphPickingUI.Add(UIUtils.CreateHeader(this, "Main morphs:", true, 30, Color.black, .1f));
            List<UIDynamic> mainToggles = BuildMorphPickingToggles(_scannedMainMorphs);
            _morphPickingUI.AddRange(mainToggles);

            _morphPickingUI.Add(UIUtils.CreateSpacer(this, 25, true));
            _morphPickingUI.Add(UIUtils.CreateHeader(this, "Genital morphs:", true, 30, Color.black, .1f));
            List<UIDynamic> genitalToggles = BuildMorphPickingToggles(_scannedGenitalMorphs);
            _morphPickingUI.AddRange(genitalToggles);
        }

        private List<UIDynamic> BuildMorphPickingToggles(List<ScannedMorph> scannedMorphs)
        {
            List<UIDynamic> morphToggles = new List<UIDynamic>();
            foreach (ScannedMorph scannedMorph in scannedMorphs)
            {
                DAZMorph morph = scannedMorph.morph;

                scannedMorph.enabled.setCallbackFunction = (bool isOn) =>
                {
                    if (isOn)
                        scannedMorph.morph.morphValue = scannedMorph.value;
                    else
                        scannedMorph.morph.morphValue = scannedMorph.morph.startValue;
                };
                UIDynamic morphToggle = CreateToggle(scannedMorph.enabled, true);
                morphToggles.Add(morphToggle);
            }
            return morphToggles;
        }

        private List<ScannedMorph> ScanActiveBankMorphs(DAZMorphBank[] morphBanks)
        {
            List<DAZMorph> activeBankMorphs = GetActiveBankMorphs(morphBanks);
            List<ScannedMorph> scannedBankMorphs = new List<ScannedMorph>();

            foreach (DAZMorph activeMorph in activeBankMorphs)
            {
                scannedBankMorphs.Add(new ScannedMorph
                {
                    morph = activeMorph,
                    value = activeMorph.morphValue,
                    enabled = new JSONStorableBool(activeMorph.displayName, true),
                });
            }

            return scannedBankMorphs;
        }

        private List<DAZMorph> GetActiveBankMorphs(DAZMorphBank[] morphBanks)
        {
            List<DAZMorph> morphList = new List<DAZMorph>();
            foreach (DAZMorphBank morphBank in morphBanks)
            {
                if (morphBank == null) continue;
                foreach (DAZMorph morph in morphBank.morphs)
                {
                    if (morph.active && morph.visible && morph.appliedValue != morph.jsonFloat.defaultVal)
                    {
                        morphList.Add(morph);
                    }
                }
            }
            return morphList;
        }

        private void SetPoseMorphsState(bool enabled)
        {
            DisableScannedPoseMorphs(_scannedMainMorphs, enabled);
            DisableScannedPoseMorphs(_scannedGenitalMorphs, enabled);
        }

        private void DisableScannedPoseMorphs(List<ScannedMorph> scannedMorphs, bool enabled)
        {
            foreach (ScannedMorph scannedMorph in scannedMorphs)
            {
                if (scannedMorph.morph.isPoseControl)
                    scannedMorph.enabled.val = enabled;
            }
        }

        private void ExportStep(bool resetStorables = true)
        {
            if (resetStorables)
            {
                _splitHeadBodyStorable.val = true;
                _autoFixHeadHeightStorable.val = true;
                _exportHeadMorphStorable.val = true;
                _exportBodyMorphStorable.val = true;
                _exportWholeBodyMorphStorable.val = true;
                _exportGenitalMorphStorable.val = true;
                _exportAsPoseMorphStorable.val = false;

                _morphsNameStorable.val = containingAtom.name;
                _morphsCategoryStorable.val = "Characters";
            }

            ShowExportUI();
            UpdateExportUI();
        }

        private void ShowExportUI()
        {
            ResetUI();

            UIDynamicButton previousStepButton = CreateButton("← Previous step");
            _currentUI.Add(previousStepButton);
            previousStepButton.button.onClick.AddListener(() => StartCoroutine(MorphPickingStep(false, false)));

            _currentUI.Add(UIUtils.CreateSpacer(this, 20));

            _currentUI.Add(CreateToggle(_splitHeadBodyStorable));

            _currentUI.Add(UIUtils.CreateHeader(this, "Morphs name:", true, 30, Color.black, .1f));
            _currentUI.Add(UIUtils.CreateTextInput(this, _morphsNameStorable, true));

            _currentUI.Add(UIUtils.CreateHeader(this, "Morphs category:", true, 30, Color.black, .1f));
            _currentUI.Add(UIUtils.CreateTextInput(this, _morphsCategoryStorable, true));

        }

        private void UpdateExportUI()
        {
            UIUtils.RemoveUI(this, _exportMorphsUI);
            _exportMorphsUI = new List<UIDynamic>();

            if (_splitHeadBodyStorable.val)
                _exportMorphsUI.Add(CreateToggle(_autoFixHeadHeightStorable));
            _exportMorphsUI.Add(UIUtils.CreateSpacer(this, 10));

            if (_splitHeadBodyStorable.val)
            {
                _exportMorphsUI.Add(CreateToggle(_exportHeadMorphStorable));
                _exportMorphsUI.Add(CreateToggle(_exportBodyMorphStorable));
            }
            else
            {
                _exportMorphsUI.Add(CreateToggle(_exportWholeBodyMorphStorable));
            }
            _exportMorphsUI.Add(CreateToggle(_exportGenitalMorphStorable));

            // _exportMorphsUI.Add(UIUtils.CreateSpacer(this, 10));
            // _exportMorphsUI.Add(CreateToggle(_exportAsPoseMorphStorable));

            _exportMorphsUI.Add(UIUtils.CreateSpacer(this, 20));

            UIDynamicButton exportButton = CreateButton("Export morphs");
            _exportMorphsUI.Add(exportButton);
            exportButton.button.onClick.AddListener(() =>
            {
                ExportMorphs();
                ShowResultUI();
            });
        }

        void ExportMorphs()
        {
            List<DAZMorph> exportedMorphs = new List<DAZMorph>();
            DAZCharacterSelector characterSelector = containingAtom.GetComponentInChildren<DAZCharacterSelector>();
            DAZMorphBank bank1 = characterSelector.morphBank1;
            DAZMorphBank bank2 = characterSelector.morphBank2;

            MorphBuffer mainBuffer = CreateScannedMorphBuffer(_scannedMainMorphs);
            MorphBuffer genitalBuffer = CreateScannedMorphBuffer(_scannedGenitalMorphs);

            if (_splitHeadBodyStorable.val)
            {
                MorphBuffer headBuffer;
                MorphBuffer bodyBuffer;
                mainBuffer.ApplyFilter(MorphFilters.HeadFilter, out headBuffer, out bodyBuffer);

                if (_autoFixHeadHeightStorable.val)
                {
                    float meanY = 0;
                    foreach (int shoulderVertex in _shouldersVertices)
                    {
                        if (headBuffer.deltas.ContainsKey(shoulderVertex))
                            meanY += headBuffer.deltas[shoulderVertex].y;
                    }
                    meanY = meanY / _shouldersVertices.Length;
                    Vector3 headOffset = Vector3.up * meanY;

                    headBuffer = headBuffer.ApplyOffsetOn(Vector3.up * -meanY, MorphFilters.HeadFilter);
                    bodyBuffer = bodyBuffer.ApplyOffsetOn(Vector3.up * meanY, MorphFilters.HeadFilter);
                }

                if (_exportHeadMorphStorable.val)
                    exportedMorphs.Add(headBuffer.ExportMorph(
                        _morphsNameStorable.val + " - Head",
                        _morphsCategoryStorable.val,
                        bank1,
                        _exportAsPoseMorphStorable.val
                    ));
                if (_exportBodyMorphStorable.val)
                    exportedMorphs.Add(bodyBuffer.ExportMorph(
                        _morphsNameStorable.val + " - Body",
                        _morphsCategoryStorable.val,
                        bank1,
                        _exportAsPoseMorphStorable.val
                    ));
            }
            else if (_exportWholeBodyMorphStorable.val)
            {
                exportedMorphs.Add(mainBuffer.ExportMorph(
                    _morphsNameStorable.val + " - Complete",
                    _morphsCategoryStorable.val,
                    bank1,
                    _exportAsPoseMorphStorable.val
                ));
            }

            if (_exportGenitalMorphStorable.val)
                exportedMorphs.Add(genitalBuffer.ExportMorph(
                    _morphsNameStorable.val + " - Genital",
                    _morphsCategoryStorable.val,
                    bank2,
                    _exportAsPoseMorphStorable.val
                ));

            SaveMorphs(exportedMorphs);
        }

        private MorphBuffer CreateScannedMorphBuffer(List<ScannedMorph> scannedMorphs)
        {
            MorphBuffer buffer = new MorphBuffer();

            foreach (ScannedMorph scannedMorph in scannedMorphs)
            {
                if (!scannedMorph.enabled.val) continue;

                DAZMorph morph = scannedMorph.morph;

                foreach (DAZMorphVertex morphVertex in morph.deltas)
                {
                    buffer.AddDelta(morphVertex.vertex, morphVertex.delta, morph.morphValue);
                }
                foreach (DAZMorphFormula morphFormula in morph.formulas)
                {
                    if (
                        morphFormula.targetType != DAZMorphFormulaTargetType.MCM
                        && morphFormula.targetType != DAZMorphFormulaTargetType.MCMMult
                        && morphFormula.targetType != DAZMorphFormulaTargetType.MorphValue
                    )
                        buffer.AddFormula(
                            morphFormula.target,
                            morphFormula.targetType,
                            morphFormula.multiplier,
                            morph.morphValue
                        );
                }
            }

            return buffer;
        }

        private void SaveMorphs(List<DAZMorph> morphs)
        {
            ResetLog();
            foreach (DAZMorph morph in morphs)
            {
                if (morph.numDeltas == 0 && morph.formulas.Length == 0)
                {
                    Log($"\"{morph.morphName}\" was empty and has not been exported.");
                    continue;
                }

                string morphExportFolder = morph.morphBank.autoImportFolder;
                string metaFileName = morph.morphName + ".vmi";
                string deltasFileName = morph.morphName + ".vmb";

                // VaM exposes SaveJSON(JSONClass, path) only; no dialog/callback overload.
                SaveJSON(morph.GetMetaJSON(), morphExportFolder + "/" + metaFileName);
                morph.SaveDeltasToBinaryFile(morphExportFolder + "/" + deltasFileName);
                Log($"\"{morph.morphName}\" exported to: {morphExportFolder}");
            }
        }

        private void ShowResultUI()
        {
            ResetUI();

            UIDynamicButton previousStepButton = CreateButton("← Previous step");
            _currentUI.Add(previousStepButton);
            previousStepButton.button.onClick.AddListener(() => ExportStep(false));

            _currentUI.Add(UIUtils.CreateSpacer(this, 20));

            _currentUI.Add(UIUtils.CreateHeader(this, "Export result:", false, 30, Color.black, .1f));
            UIDynamicTextField logField = CreateTextField(_exportLogStorable);
            _currentUI.Add(logField);

            logField.height = 500;

            _currentUI.Add(UIUtils.CreateSpacer(this, 20));

            UIDynamicButton nextStepButton = CreateButton("Done");
            _currentUI.Add(nextStepButton);
            nextStepButton.button.onClick.AddListener(() => ShowStartUI());

            UIDynamicButton openMorphsFolderButton = CreateButton("Open morphs folder in explorer");
            _currentUI.Add(openMorphsFolderButton);
            openMorphsFolderButton.button.onClick.AddListener(() => SuperController.singleton.OpenFolderInExplorer("Custom/Atom/Person/Morphs"));

            _currentUI.Add(UIUtils.CreateHeader(this, "Note: created morphs will not appear in the morphs tab until you hard reset or restart the game.", false, 30, new Color(.3f, 0, 0), 5f));
        }

        void ResetLog()
        {
            _exportLogStorable.val = "";
        }

        void Log(string message)
        {
            if (_exportLogStorable.val != "") message = "\n" + message;
            _exportLogStorable.val += message;
        }
    }
}
