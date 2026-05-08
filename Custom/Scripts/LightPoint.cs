using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace MVRPlugin {
	public class Template : MVRScript {
		
		
		private static Atom person;
		private static Atom light;
		private static FreeControllerV3 chestController;
		private static FreeControllerV3 lightController;
		
		// IMPORTANT - DO NOT make custom enums. The dynamic C# complier crashes Unity when it encounters these for
		// some reason

		// IMPORTANT - DO NOT OVERRIDE Awake() as it is used internally by MVRScript - instead use Init() function which
		// is called right after creation
		public override void Init() {
			try {
				// put init code in here
				SuperController.LogMessage("Template Loaded");

				// create custom JSON storable params here if you want them to be stored with scene JSON
				// types are JSONStorableFloat, JSONStorableBool, JSONStorableString, JSONStorableStringChooser
				// JSONStorableColor

			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// Start is called once before Update or FixedUpdate is called and after Init()
		void Start() {
			try {
				// put code in here
				person = SuperController.singleton.GetAtomByUid("Person");
				chestController = person.GetStorableByID("chestControl") as FreeControllerV3;
				light = containingAtom;
				lightController = light.GetStorableByID("control") as FreeControllerV3;
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// Update is called with each rendered frame by Unity
		void Update() {
			try {
				// put code in here
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// FixedUpdate is called with each physics simulation frame by Unity
		void FixedUpdate() {
			try {
				// put code in here
				lightController.transform.LookAt(chestController.transform.position);
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// OnDestroy is where you should put any cleanup
		// if you registered objects to supercontroller or atom, you should unregister them here
		void OnDestroy() {
		}

	}
}