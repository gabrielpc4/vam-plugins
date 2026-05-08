using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

/*
 * UnityAssetVamifier is a simple plugin to convert Materials used in
 * Unity Asset Bundles (.assetbundle) from Unity default shaders
 * (Roughness setup preferred) to the builtin VAM shader and expose
 * the shader properties in the UI.
 *
 * Authors: NoStage3
 * License: Creative Commons with Attribution (CC BY 3.0)
 * Current version: 1.8
 * 
 * Changes for 1.8:
 * - added sliders for diffuse and specular bumpiness
 *  
 * Changes for 1.7:
 * - fix: shaders with alphatest/alphablend keywords no longer get converted
 *
 * Changes for 1.6:
 * - material colors from unity are now converted better by default
 * - manual color inputs now only work when "Colorize" is toggled on
 *
 * Changes for 1.5:
 * - fixed bug where only first material of each renderer was converted
 *
 * Changes for 1.4:
 * - fixed loading bug for very large assets (that take long to load)
 * 
 * Changes for 1.3:
 * - tiling and offset values are now all taken from main texture
 * 
 * Changes for 1.2:
 * - Support for Texture tiling and offset
 * 
 * Changes for 1.1:
 * - Improved waiting for Asset Bundle solution
 * - Reordered UI to match VAM UI (sliders left, colors right)
 * 
 */

namespace MVRPlugin {
	public class UnityAssetVamifier : MVRScript {
		private readonly string VERSION_NUMBER = "1.8";

		private readonly List<string> UNITY_SHADER_NAMES = new List<string>(new string[] {
			"Standard",
			"Standard (Specular setup)",
			"Standard (Roughness setup)"
		});
		private readonly List<string> INVALID_SHADER_KEYWORDS = new List<string>(new string[] {
			"_ALPHAPREMULTIPLY_ON",
			"_ALPHATEST_ON",
			"_ALPHABLEND_ON"
		});
		private readonly string VAM_SHADER_NAME = "Custom/Subsurface/GlossNMCull";

		protected UIDynamicSlider specIntensitySlider;
		protected UIDynamicSlider specFresnelSlider;
		protected UIDynamicSlider specSharpnessSlider;
		protected UIDynamicSlider diffOffsetSlider;
		protected UIDynamicSlider specOffsetSlider;
		protected UIDynamicSlider glossOffsetSlider;
		protected UIDynamicSlider iBLFilterSlider;
		protected UIDynamicSlider diffBumpinessSlider;
		protected UIDynamicSlider specBumpinessSlider;

		protected UIDynamicToggle allowColorizeButton;
		protected UIDynamicColorPicker diffColorPicker;
		protected UIDynamicColorPicker specColorPicker;
		protected UIDynamicColorPicker subdermisColorPicker;
		
		protected JSONStorableFloat jSpecIntensityFloat;
		protected JSONStorableFloat jSpecSharpnessFloat;
		protected JSONStorableFloat jSpecFresnelFloat;
		protected JSONStorableFloat jDiffOffsetFloat;
		protected JSONStorableFloat jSpecOffsetFloat;
		protected JSONStorableFloat jGlossOffsetFloat;
		protected JSONStorableFloat jIBLFilterFloat;
		protected JSONStorableFloat jDiffBumpinessFloat;
		protected JSONStorableFloat jSpecBumpinessFloat;
		protected JSONStorableColor jDiffColor;
		protected JSONStorableColor jSpecColor;
		protected JSONStorableColor jSubdermisColor;
		protected JSONStorableBool jAllowColorizeBool;

		protected List<MatPackage> origMaterials = new List<MatPackage>();
		protected List<MatPackage> newMaterials = new List<MatPackage>();
		protected Shader vamPropShader;
		protected List<Renderer> allRenderers = new List<Renderer>();


		public struct MatPackage {
			public int rendererIndex;
			public int materialIndex;
			public Material material;
			public bool isConverted;

			public MatPackage(int rIndex, int mIndex, Material mat, bool converted) {
				this.rendererIndex = rIndex;
				this.materialIndex = mIndex;
				this.material = mat;
				this.isConverted = converted;
			}
		}

		public override void Init() {
			try {

				// create new VAM material from built-in shader
				vamPropShader = Shader.Find(VAM_SHADER_NAME);

				// build/load/populate material controls for VAM UI
				BuildUIControls();

				AddVersionNumberToUI();

				// since there is no "asset bundle loaded" callback yet, we have to wait
				StartCoroutine(WaitForAssetBundle());

			} catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}


		protected void BuildUIControls() {

			// Specular Intensity
			jSpecIntensityFloat = new JSONStorableFloat("SpecularIntensity", 0.5f, SetSpecIntensity, 0f, 1f, true);
			RegisterFloat(jSpecIntensityFloat);
			specIntensitySlider = CreateSlider(jSpecIntensityFloat);

			// Specular Sharpness
			jSpecSharpnessFloat = new JSONStorableFloat("SpecularSharpness", 6f, SetSpecSharpness, 0f, 10f, true);
			RegisterFloat(jSpecSharpnessFloat);
			specSharpnessSlider = CreateSlider(jSpecSharpnessFloat);

			// Specular Fresnel
			jSpecFresnelFloat = new JSONStorableFloat("SpecularFresnel", 0f, SetSpecFresnel, 0f, 1f, true);
			RegisterFloat(jSpecFresnelFloat);
			specFresnelSlider = CreateSlider(jSpecFresnelFloat);

			// Diffuse Offset
			jDiffOffsetFloat = new JSONStorableFloat("DiffuseOffset", 0f, SetDiffOffset, -1f, 1f, true);
			RegisterFloat(jDiffOffsetFloat);
			diffOffsetSlider = CreateSlider(jDiffOffsetFloat);

			// Spec Offset
			jSpecOffsetFloat = new JSONStorableFloat("SpecularOffset", 0f, SetSpecOffset, -1f, 1f, true);
			RegisterFloat(jSpecOffsetFloat);
			specOffsetSlider = CreateSlider(jSpecOffsetFloat);

			// Gloss Offset
			jGlossOffsetFloat = new JSONStorableFloat("GlossOffset", 0.8f, SetGlossOffset, 0, 1f, true);
			RegisterFloat(jGlossOffsetFloat);
			glossOffsetSlider = CreateSlider(jGlossOffsetFloat);

			// IBL Filter (affects Global Illum Skybox ?!)
			jIBLFilterFloat = new JSONStorableFloat("IBLFilter", 0f, SetIBLFilter, 0, 1f, true);
			RegisterFloat(jIBLFilterFloat);
			iBLFilterSlider = CreateSlider(jIBLFilterFloat);

			// Diffuse Bumpiness
			jDiffBumpinessFloat = new JSONStorableFloat("DiffuseBumpiness", 1f, SetDiffBumpiness, 0, 3f, true);
			RegisterFloat(jDiffBumpinessFloat);
			diffBumpinessSlider = CreateSlider(jDiffBumpinessFloat);

			// Spec Bumpiness
			jSpecBumpinessFloat = new JSONStorableFloat("SpecularBumpiness", 1f, SetSpecBumpiness, 0, 3f, true);
			RegisterFloat(jSpecBumpinessFloat);
			specBumpinessSlider = CreateSlider(jSpecBumpinessFloat);


			//allowColorizeButton
			jAllowColorizeBool = new JSONStorableBool("Colorize", false);
			RegisterBool(jAllowColorizeBool);
			allowColorizeButton = CreateToggle(jAllowColorizeBool, true);
			allowColorizeButton.toggle.onValueChanged.AddListener(delegate (bool state) {
				if (state) {
					BuildColorUI();
					ApplyJStoredColors();
				} else {
					RemoveColorUI();
					RestoreOriginalColors();
				}
			});
			
		}

		protected void BuildColorUI() {
			//Diff Color
			HSVColor diffColorHSVC = HSVColorPicker.RGBToHSV(1f, 1f, 1f);
			jDiffColor = new JSONStorableColor("DiffuseColor", diffColorHSVC, SetDiffColor);
			RegisterColor(jDiffColor);
			diffColorPicker = CreateColorPicker(jDiffColor, true);

			//Specular Color
			HSVColor specColorHSVC = HSVColorPicker.RGBToHSV(1f, 1f, 1f);
			jSpecColor = new JSONStorableColor("SpecularColor", specColorHSVC, SetSpecColor);
			RegisterColor(jSpecColor);
			specColorPicker = CreateColorPicker(jSpecColor, true);

			//Subdermis Color
			HSVColor subdermisColorHSVC = HSVColorPicker.RGBToHSV(1f, 1f, 1f);
			jSubdermisColor = new JSONStorableColor("SubdermisColor", subdermisColorHSVC, SetSubdermisColor);
			RegisterColor(jSubdermisColor);
			subdermisColorPicker = CreateColorPicker(jSubdermisColor, true);
		}

		protected void RemoveColorUI() {
			RemoveColorPicker(diffColorPicker);
			RemoveColorPicker(specColorPicker);
			RemoveColorPicker(subdermisColorPicker);
		}


		protected void AddVersionNumberToUI() {
			JSONStorableString versionString = new JSONStorableString("pluginVersion", "\nUnityAssetVamifier "+ VERSION_NUMBER + "\nby NoStage3");
			UIDynamicTextField textFieldVersion = CreateTextField(versionString, true);
			textFieldVersion.height = 130;
		}


		// determine if the material should be converted or not
		protected bool IsValidMaterial(Material mat) {
			if (!UNITY_SHADER_NAMES.Contains(mat.shader.name))
				return false;

			foreach (string keyword in INVALID_SHADER_KEYWORDS) {
				if (mat.IsKeywordEnabled(keyword))
					return false;
			}
			return true;
		}


		// wait for the asset bundle to be loaded by checking for renderers every 0.5s (a bit hacky...)
		protected IEnumerator WaitForAssetBundle() {

			while (allRenderers.Count < 1) {
				containingAtom.GetComponentsInChildren<Renderer>(false, allRenderers);
				yield return new WaitForSeconds(0.5f);
			}

			ConvertMaterial();
		}


		protected void ConvertMaterial() {
			List<Material> invalidMaterials = new List<Material>();

			if (allRenderers.Count == 0) {
				SuperController.LogMessage("Make sure you have an .assetbundle loaded and a prop selected from the Asset dropdown. Then reload this plugin.");
				return;
			}

			// fill newMaterials with either the original material or an new material with the vam shader
			for (int i = 0; i < allRenderers.Count; i++) {
				for (int j = 0; j < allRenderers[i].materials.Length; j++) {
					origMaterials.Add(new MatPackage(i, j, allRenderers[i].materials[j], false));

					if (IsValidMaterial(allRenderers[i].materials[j])) {
						newMaterials.Add(new MatPackage(i, j, new Material(vamPropShader), true));

					} else {
						newMaterials.Add(new MatPackage(i, j, allRenderers[i].materials[j], false));
						invalidMaterials.Add(allRenderers[i].materials[j]);
					}
				}
			}


			// output invalid/skipped materials
			foreach (Material invalidMaterial in invalidMaterials) {
				SuperController.LogMessage("Material skipped: " + invalidMaterial.name + " -- Shader: [" + invalidMaterial.shader.name + "]");
			}


			if (newMaterials.Count == 0) {
				SuperController.LogError("No compatible material could be found on this asset. Check message log for skipped materials/shaders.");
				return;
			}


			//collect materials (matsToAssign) to assign to each renderer (needs to be assigned as array...)
			for (int i = 0; i < allRenderers.Count; i++) {
				Material[] matsToAssign = allRenderers[i].materials;
				for (int j = 0; j < allRenderers[i].materials.Length; j++) {
					if (IsValidMaterial(allRenderers[i].materials[j])) {

						Material origMat = GetOrigMaterialByIndex(i, j);
						Material vamMat = GetVamMaterialByIndex(i, j);
						Vector2 mainTexScale = origMat.GetTextureScale("_MainTex");
						Vector2 mainTexOffset = origMat.GetTextureOffset("_MainTex");

						// reassign colors (can get overwritten by colorize toggle)
						if (origMat.HasProperty("_Color")) {
							vamMat.SetColor("_Color", origMat.GetColor("_Color"));
						}
						if (origMat.HasProperty("_SpecColor")) {
							vamMat.SetColor("_SpecColor", origMat.GetColor("_SpecColor"));
						}
						if (origMat.HasProperty("_SubdermisColor")) {
							vamMat.SetColor("_SubdermisColor", origMat.GetColor("_SubdermisColor"));
						}
						
						// reassign textures and tiling from unity to vam material
						// use tiling from main texture since supported unity shaders don't allow tiling per texture type

						if (origMat.HasProperty("_MainTex")) {
							vamMat.SetTexture("_MainTex", origMat.GetTexture("_MainTex"));
							vamMat.SetTextureScale("_MainTex", mainTexScale);
							vamMat.SetTextureOffset("_MainTex", mainTexOffset);
						}

						if (origMat.HasProperty("_BumpMap")) {
							vamMat.SetTexture("_BumpMap", origMat.GetTexture("_BumpMap"));
							vamMat.SetTextureScale("_BumpMap", mainTexScale);
							vamMat.SetTextureOffset("_BumpMap", mainTexOffset);
						}

						if (origMat.HasProperty("_SpecGlossMap")) {
							vamMat.SetTexture("_SpecTex", origMat.GetTexture("_SpecGlossMap"));
							vamMat.SetTextureScale("_SpecTex", mainTexScale);
							vamMat.SetTextureOffset("_SpecTex", mainTexOffset);
						}

						if (origMat.HasProperty("_MetallicGlossMap")) {
							vamMat.SetTexture("_GlossTex", origMat.GetTexture("_MetallicGlossMap"));
							vamMat.SetTextureScale("_GlossTex", mainTexScale);
							vamMat.SetTextureOffset("_GlossTex", mainTexOffset);
						}

						matsToAssign[j] = vamMat;
					}
				}
				// assign VAM materials to prop renderer as array
				allRenderers[i].materials = matsToAssign;
			}

			// apply user definable values initially
			
			SetSpecIntensity(jSpecIntensityFloat);
			SetSpecSharpness(jSpecSharpnessFloat);
			SetSpecFresnel(jSpecFresnelFloat);
			SetDiffOffset(jDiffOffsetFloat);
			SetSpecOffset(jSpecOffsetFloat);
			SetDiffBumpiness(jDiffBumpinessFloat);
			SetSpecBumpiness(jSpecBumpinessFloat);
			SetGlossOffset(jGlossOffsetFloat);
			ApplyJStoredColors();
		}

		protected Material GetOrigMaterialByIndex(int rIndex, int mIndex) {
			foreach (MatPackage matPackage in origMaterials) {
				if (matPackage.rendererIndex == rIndex && matPackage.materialIndex == mIndex) {
					return matPackage.material;
				}
			}
			return null;
		}

		protected Material GetVamMaterialByIndex(int rIndex, int mIndex) {
			foreach (MatPackage matPackage in newMaterials) {
				if (matPackage.rendererIndex == rIndex && matPackage.materialIndex == mIndex) {
					return matPackage.material;
				}
			}
			return null;
		}


		void OnDestroy() {
			RestoreOriginalMaterials();
		}

		protected void RestoreOriginalMaterials() {
			for (int i = 0; i < allRenderers.Count; i++) {
				List<Material> matsToAssign = new List<Material>();
				for (int j = 0; j < allRenderers[i].materials.Length; j++) {
					matsToAssign.Add(GetOrigMaterialByIndex(i, j));
				}
				allRenderers[i].materials = matsToAssign.ToArray();
			}
		}

		protected void RestoreOriginalColors() {
			foreach (MatPackage matPackage in newMaterials) {
				if (GetOrigMaterialByIndex(matPackage.rendererIndex, matPackage.materialIndex).HasProperty("_Color")) {
					matPackage.material.SetColor("_Color", GetOrigMaterialByIndex(matPackage.rendererIndex, matPackage.materialIndex).GetColor("_Color"));
				} else {
					matPackage.material.SetColor("_Color", Color.white);
				}
				if (GetOrigMaterialByIndex(matPackage.rendererIndex, matPackage.materialIndex).HasProperty("_SpecColor")) {
					matPackage.material.SetColor("_SpecColor", GetOrigMaterialByIndex(matPackage.rendererIndex, matPackage.materialIndex).GetColor("_SpecColor"));
				} else {
					matPackage.material.SetColor("_SpecColor", Color.white);
				}
				if (GetOrigMaterialByIndex(matPackage.rendererIndex, matPackage.materialIndex).HasProperty("_SubdermisColor")) {
					matPackage.material.SetColor("_SubdermisColor", GetOrigMaterialByIndex(matPackage.rendererIndex, matPackage.materialIndex).GetColor("_SubdermisColor"));
				} else {
					matPackage.material.SetColor("_SubdermisColor", Color.white);
				}
			}
		}

		protected void ApplyJStoredColors() {
			SetDiffColor(jDiffColor);
			SetSpecColor(jSpecColor);
			SetSubdermisColor(jSubdermisColor);
		}


		protected void SetSpecIntensity(JSONStorableFloat jf) {
			foreach (MatPackage matPackage in newMaterials) {
				if (!matPackage.isConverted) continue;

				matPackage.material.SetFloat("_SpecInt", jf.val);
			}
		}

		protected void SetSpecFresnel(JSONStorableFloat jf) {
			foreach (MatPackage matPackage in newMaterials) {
				if (!matPackage.isConverted) continue;

				matPackage.material.SetFloat("_Fresnel", jf.val);
			}
		}

		protected void SetSpecSharpness(JSONStorableFloat jf) {
			foreach (MatPackage matPackage in newMaterials) {
				if (!matPackage.isConverted) continue;

				matPackage.material.SetFloat("_Shininess", jf.val);
			}
		}

		protected void SetDiffOffset(JSONStorableFloat jf) {
			foreach (MatPackage matPackage in newMaterials) {
				if (!matPackage.isConverted) continue;

				matPackage.material.SetFloat("_DiffOffset", jf.val);
			}
		}

		protected void SetSpecBumpiness(JSONStorableFloat jf) {
			foreach (MatPackage matPackage in newMaterials) {
				if (!matPackage.isConverted) continue;

				matPackage.material.SetFloat("_SpecularBumpiness", jf.val);
			}
		}

		protected void SetDiffBumpiness(JSONStorableFloat jf) {
			foreach (MatPackage matPackage in newMaterials) {
				if (!matPackage.isConverted)
					continue;

				matPackage.material.SetFloat("_DiffuseBumpiness", jf.val);
			}
		}

		protected void SetSpecOffset(JSONStorableFloat jf) {
			foreach (MatPackage matPackage in newMaterials) {
				if (!matPackage.isConverted)
					continue;

				matPackage.material.SetFloat("_SpecOffset", jf.val);
			}
		}

		protected void SetGlossOffset(JSONStorableFloat jf) {
			foreach (MatPackage matPackage in newMaterials) {
				if (!matPackage.isConverted) continue;

				matPackage.material.SetFloat("_GlossOffset", jf.val);
			}
		}

		protected void SetIBLFilter(JSONStorableFloat jf) {
			foreach (MatPackage matPackage in newMaterials) {
				if (!matPackage.isConverted) continue;

				matPackage.material.SetFloat("_IBLFilter", jf.val);
			}
		}

		protected void SetDiffColor(JSONStorableColor jcolor) {
			if (!jAllowColorizeBool.val) return;

			foreach (MatPackage matPackage in newMaterials) {
				if (!matPackage.isConverted) continue;

				matPackage.material.SetColor("_Color", jcolor.colorPicker.currentColor);
			}
		}

		protected void SetSpecColor(JSONStorableColor jcolor) {
			if (!jAllowColorizeBool.val) return;

			foreach (MatPackage matPackage in newMaterials) {
				if (!matPackage.isConverted) continue;

				matPackage.material.SetColor("_SpecColor", jcolor.colorPicker.currentColor);
			}
		}

		protected void SetSubdermisColor(JSONStorableColor jcolor) {
			if (!jAllowColorizeBool.val)return;

			foreach (MatPackage matPackage in newMaterials) {
				if (!matPackage.isConverted) continue;

				matPackage.material.SetColor("_SubdermisColor", jcolor.colorPicker.currentColor);
			}
		}

	}
}