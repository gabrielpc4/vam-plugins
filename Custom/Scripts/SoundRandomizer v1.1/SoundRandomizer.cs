using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// SoundRandomizer v1.1.1
/// By bvctr
/// A quick and easy way to play a random sound via an action trigger.
/// </summary>
namespace bvctr {
	public class SoundRandomizer : MVRScript {
		protected JSONStorableAction actionPlayRandom;

		// Audio clips
		protected List<NamedAudioClip> active = new List<NamedAudioClip>();
		protected JSONStorableString selected;

		// Audio source
		protected Atom sourceAtom;
		protected JSONStorable sourceReceiver;

		// UI
		protected List<JSONStorableBool> soundsCustom = new List<JSONStorableBool>();
		protected List<JSONStorableBool> soundsEmbedded = new List<JSONStorableBool>();
		protected List<UIDynamicTextField> categoryLabels = new List<UIDynamicTextField>();
		protected List<UIDynamicButton> categoryButtons = new List<UIDynamicButton>();
		protected JSONStorableStringChooser uiSourceAtom;
		protected JSONStorableStringChooser uiSourceReceiver;
		protected JSONStorableBool uiPlayIfClear;



		/**
		* Create list of available custom sounds.
		*/
		private void BuildCustomSoundList() {
			// Remove any existing sounds
			soundsCustom.ForEach((item) => {
				RemoveToggle(item);
			});

			// Reset list
			soundsCustom.Clear();

			// Get list of all custom scene audio
			List<NamedAudioClip> webAudioClips = URLAudioClipManager.singleton.GetCategoryClips("web");

			if (webAudioClips != null) {
				foreach (NamedAudioClip clip in webAudioClips) {
					JSONStorableBool choice = new JSONStorableBool(clip.displayName, false, new JSONStorableBool.SetBoolCallback((v) => {
						ToggleSelected(clip, v, true);
					}));
					CreateToggle(choice, true);

					soundsCustom.Add(choice);
				}
			}
		}

		/**
		* Create list of embedded sounds.
		*/
		private void BuildEmbeddedSoundList() {
			// Remove existing sounds
			soundsEmbedded.ForEach((item) => {
				RemoveToggle(item);
			});

			// Remove labels
			categoryLabels.ForEach((item) => {
				RemoveTextField(item);
			});

			// Remove buttons
			categoryButtons.ForEach((item) => {
				RemoveButton(item);
			});

			// Clear lists
			soundsEmbedded.Clear();
			categoryLabels.Clear();

			// Get embedded audio categories
			List<string> embeddedAudioCategories = EmbeddedAudioClipManager.singleton.GetCategories();

			foreach (string category in embeddedAudioCategories) {
				List<NamedAudioClip> embeddedAudioClips = EmbeddedAudioClipManager.singleton.GetCategoryClips(category);
				
				UIDynamicTextField txt = CreateTextField(new JSONStorableString($"label-{category}", $"\n {category}"));
				txt.height = 20f;
				categoryLabels.Add(txt);

				UIDynamicButton btn = CreateButton("Select All");
				btn.button.onClick.AddListener(() => {
					SelectCategoryClips(category);
				});
				categoryButtons.Add(btn);

				foreach (NamedAudioClip clip in embeddedAudioClips) {
					JSONStorableBool choice = new JSONStorableBool(clip.displayName, false, new JSONStorableBool.SetBoolCallback((v) => {
						ToggleSelected(clip, v, false);
					}));
					CreateToggle(choice);

					soundsEmbedded.Add(choice);
				}
			}
		}

		/**
		* Checks if Popup choice is a non-blank value.
		*/
		private bool IsEmpty(string choice) {
			if (choice == null || choice == "None") {
				return true;
			}

			return false;
		}

		/**
		* Play a selected sound at random.
		*/
		private void PlayRandomSound() {
			if (active.Count < 1) {
				return;
			}

			int rand = UnityEngine.Random.Range(0, active.Count);

			AudioSourceControl source = SuperController.singleton.GetAtomByUid(uiSourceAtom.val)?.GetStorableByID(uiSourceReceiver.val) as AudioSourceControl;

			if (source != null) {
				if (uiPlayIfClear.val) {
					source.PlayIfClear(active[rand]);
				} else {
					source.PlayNow(active[rand]);
				}
			}
		}

		/**
		* Get list of all Atoms currently in scene.
		*/
		private void PopulateAtomChoices() {
			List<string> choices = new List<string>();
			choices.Add("None");

			foreach (Atom atom in SuperController.singleton.GetAtoms()) {
				if (atom == null) {
					continue;
				}

				if (atom.category == "People" || atom.category == "Sound") {
					choices.Add(atom.name);
				}
			}

			uiSourceAtom.choices = choices;
		}

		/**
		* Get list of all receiver choices of selected Atom.
		*/
		private void PopulateReceiverChoices() {
			List<string> choices = new List<string>();
			choices.Add("None");

			if (IsEmpty(uiSourceAtom.val)) {
				return;
			}

			// Get Atom receivers
			List<string> receivers = SuperController.singleton.GetAtomByUid(uiSourceAtom.val)?.GetStorableIDs();

			if (receivers == null) {
				return;
			}

			foreach (string choice in receivers) {
				choices.Add(choice);
			}

			uiSourceReceiver.choices = choices;
		}

		/**
		* Toggle all embedded clips within a category.
		*/
		private void SelectCategoryClips(string category) {
			List<NamedAudioClip> embeddedAudioClips = EmbeddedAudioClipManager.singleton.GetCategoryClips(category);

			foreach (NamedAudioClip clip in embeddedAudioClips) {
				ToggleSelected(clip, true, false);
			}
		}

		/**
		* Load audio source Atom.
		*/
		private void SyncAudioSourceAtom(string uid) {
			if (!IsEmpty(uid)) {
				sourceAtom = SuperController.singleton.GetAtomByUid(uid);

				if (sourceAtom != null) {
					uiSourceAtom.valNoCallback = sourceAtom.uid;
				}
	
				PopulateReceiverChoices();
			}
		}

		/**
		* Load audio source receiver.
		*/
		private void SyncAudioSourceReceiver(string uid) {
			if (!IsEmpty(uid)) {
				sourceReceiver = SuperController.singleton.GetAtomByUid(uiSourceAtom.val)?.GetStorableByID(uid);

				if (sourceReceiver != null) {
					uiSourceReceiver.valNoCallback = sourceReceiver.name;
				}
			}
		}

		/**
		* Sync sound list to only check selected sounds.
		*/
		private void SyncSelected() {
			// Reset active list
			active.Clear();

			// Sync embedded sounds
			soundsEmbedded.ForEach((c) => {
				if (selected.val.Contains(c.name)) {
					c.valNoCallback = true;

					active.Add(EmbeddedAudioClipManager.singleton.GetClip(c.name));					
				} else {
					c.valNoCallback = false;
				}

			});

			// Sync custom sounds
			soundsCustom.ForEach((c) => {
				if (selected.val.Contains(c.name)) {
					c.valNoCallback = true;

					active.Add(URLAudioClipManager.singleton.GetClip(c.name));
				} else {
					c.valNoCallback = false;
				}
			});

			UpdateSelected();
		}

		/**
		* Add/remove a sound from selected clips.
		*/
		private void ToggleSelected(NamedAudioClip clip, bool enabled, bool custom) {
			if (enabled) {
				active.Add(clip);
			} else {
				active.Remove(clip);
			}

			if (custom) {
				JSONStorableBool sound = soundsCustom.Find((s) => s.name == clip.displayName);

				if (sound != null) {
					sound.valNoCallback = enabled;
				}
			} else {
				JSONStorableBool sound = soundsEmbedded.Find((s) => s.name == clip.displayName);
				
				if (sound != null) {
					sound.valNoCallback = enabled;
				}
			}
			
			UpdateSelected();
		}

		/**
		* Update selected sounds string.
		*/
		private void UpdateSelected() {
			selected.valNoCallback = string.Join("|", active.Select((item) => item.displayName).ToArray()).Trim();
		}


	
		public IEnumerator DeferredInit() {
			yield return new WaitForEndOfFrame();

			containingAtom.RestoreFromLast(this);
		}

		public override void Init() {
			try {
				// Register actions
				actionPlayRandom = new JSONStorableAction("PlayRandomSound", () => { PlayRandomSound(); });
				RegisterAction(actionPlayRandom);

				string initialAtom = "None";
				string initialReceiver = "None";

				// Pre-select person atom/receiver if applicable
				switch (containingAtom.category) {
					case "People":
						initialAtom = containingAtom.uid;
						initialReceiver = "HeadAudioSource";
						break;
					case "Sound":
						initialAtom = containingAtom.uid;

						if (containingAtom.name == "AudioSource") {
							initialReceiver = "AudioSource";
						} else if (containingAtom.name == "RhythmAudioSource") {
							initialReceiver = "RhythmSource";
						} else if (containingAtom.name == "AptSpeaker") {
							initialReceiver = "AptSpeaker_Import";
						}

						break;
				}

				/*
				* Build UI
				*/
				UIDynamicPopup dp;

				// Audio source Atom
				uiSourceAtom = new JSONStorableStringChooser("atom", null, initialAtom, "Audio Source Atom", SyncAudioSourceAtom);
				uiSourceAtom.storeType = JSONStorableParam.StoreType.Physical;
				RegisterStringChooser(uiSourceAtom);
				dp = CreateScrollablePopup(uiSourceAtom);
				dp.popupPanelHeight = 800f;
				dp.popup.onOpenPopupHandlers += PopulateAtomChoices;

				// Audio source receiver
				uiSourceReceiver = new JSONStorableStringChooser("receiver", null, initialReceiver, "Audio Source Receiver", SyncAudioSourceReceiver);
				uiSourceReceiver.storeType = JSONStorableParam.StoreType.Physical;
				RegisterStringChooser(uiSourceReceiver);
				dp = CreateScrollablePopup(uiSourceReceiver, true);
				dp.popup.onOpenPopupHandlers += PopulateReceiverChoices;

				// Play if clear checkbox
				uiPlayIfClear = new JSONStorableBool("Only play when clear", false);
				uiPlayIfClear.storeType = JSONStorableParam.StoreType.Physical;
				RegisterBool(uiPlayIfClear);
				CreateToggle(uiPlayIfClear);

				// Test sound button
				UIDynamicButton btnTest = CreateButton("Test Sound", true);
				btnTest.button.onClick.AddListener(() => {
					PlayRandomSound();
				});

				CreateSpacer().height = 60f;
				CreateSpacer(true).height = 60f;

				// Storable to save selected sounds
				selected = new JSONStorableString("selected", ":)");
				selected.storeType = JSONStorableParam.StoreType.Physical;
				RegisterString(selected);

				UIDynamicTextField txtEmbedded = CreateTextField(new JSONStorableString("embedded", "\n Default VaM Sounds"));
				txtEmbedded.backgroundColor = new Color32(60, 58, 210, 255);
				txtEmbedded.height = 20f;
				txtEmbedded.textColor = Color.white;

				CreateSpacer().height = 50f;

				// Clear all embedded choices button
				UIDynamicButton btnClearEmbedded = CreateButton("Unselect All");
				btnClearEmbedded.button.onClick.AddListener(() => {
					foreach (JSONStorableBool clip in soundsEmbedded) {
						clip.val = false;
					}

					SyncSelected();
				});

				CreateSpacer().height = 50f;

				UIDynamicTextField txtCustom = CreateTextField(new JSONStorableString("custom", "\n Custom Sounds"), true);
				txtCustom.backgroundColor = new Color32(128, 245, 128, 255);
				txtCustom.height = 20f;

				// Select all custom choices button
				UIDynamicButton btnSelectAllEmbedded = CreateButton("Select All", true);
				btnSelectAllEmbedded.button.onClick.AddListener(() => {
					foreach (JSONStorableBool clip in soundsCustom) {
						clip.val = true;
					}

					SyncSelected();
				});

				// Clear all custom choices button
				UIDynamicButton btnClearCustom = CreateButton("Unselect All", true);
				btnClearCustom.button.onClick.AddListener(() => {
					foreach (JSONStorableBool clip in soundsCustom) {
						clip.val = false;
					}

					SyncSelected();
				});

				CreateSpacer(true).height = 35f;

				// Build UI
				BuildEmbeddedSoundList();
				BuildCustomSoundList();

				// Load stored selected sounds (if existing)
				selected.setCallbackFunction = new JSONStorableString.SetStringCallback((val) => SyncSelected());

				StartCoroutine(DeferredInit());
			} catch (Exception e) {
				SuperController.LogError("SoundRandomizer.cs: " + e);
			}
		}
	}
}
