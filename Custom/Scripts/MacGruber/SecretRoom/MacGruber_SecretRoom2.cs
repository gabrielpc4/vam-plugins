/* /////////////////////////////////////////////////////////////////////////////////////////////////
SecretRoom2 v1.1 by MacGruber.
Environment customizer script for SecretRoom2 scene

Version 1.1 2019-11-10
	Fixed issue with settings not being applied on scene load.

Version 1.0 2019-11-10
	Initial release.
	
///////////////////////////////////////////////////////////////////////////////////////////////// */

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace MacGruber
{
    public class SecretRoom2 : MVRScript
    {
		private JSONStorableFloat hookLow;
		private JSONStorableFloat hookMid;
		private JSONStorableFloat hookHigh;
		private JSONStorableFloat bars;
		
		JSONStorableBool rotateVertical;
		JSONStorableBool rotateCeiling;
		JSONStorableBool rotateBed;
		
		private bool syncNeeded = true;
		private bool syncHookLow = true;
		private bool syncHookMid = true;
		private bool syncHookHigh = true;
		private bool syncHookCeil = true;
		private bool syncHookBed = true;
		private bool syncBars = true;
		
		private Dictionary<Transform,Quaternion> rotationTable = new Dictionary<Transform,Quaternion>();
		
        public override void Init()
		{
			
			
			hookLow  = SetupSliderFloat("Hooks Low",  0.12f, 0.05f, 2.55f, false);
			hookMid  = SetupSliderFloat("Hooks Mid",  1.10f, 0.05f, 2.55f, false);
			hookHigh = SetupSliderFloat("Hooks High", 1.90f, 0.05f, 2.55f, false);
			hookLow.setCallbackFunction  += (float v) => { syncNeeded = syncHookLow  = true; };
			hookMid.setCallbackFunction  += (float v) => { syncNeeded = syncHookMid  = true; };			
			hookHigh.setCallbackFunction += (float v) => { syncNeeded = syncHookHigh = true; };
			
			rotateVertical = SetupToggle("Hooks Low/Mid/High Rotate", false, true);
			rotateVertical.setCallbackFunction += (bool v) => { syncNeeded = syncHookLow = syncHookMid = syncHookHigh = true; };
			
			rotateCeiling = SetupToggle("Hooks Ceiling Rotate", false, true);
			rotateCeiling.setCallbackFunction += (bool v) => { syncNeeded = syncHookCeil = true; };
			
			rotateBed = SetupToggle("Hooks Bed Rotate", false, true);
			rotateBed.setCallbackFunction += (bool v) => { syncNeeded = syncHookBed = true; };
			
			bars = SetupSliderFloat("Bars", 0.2f, 0.05f, 0.85f, false);
			bars.setCallbackFunction  += (float v) => { syncNeeded = syncBars = true; };
		}
		
		private JSONStorableFloat SetupSliderFloat(string name, float defaultValue, float minValue, float maxValue, bool rightSide)
		{
			JSONStorableFloat storable = new JSONStorableFloat(name, defaultValue, minValue, maxValue, true, true);
			storable.storeType = JSONStorableParam.StoreType.Full;
			CreateSlider(storable, rightSide);
			RegisterFloat(storable);
			return storable;
		}
		
		public JSONStorableBool SetupToggle(string name, bool defaultValue, bool rightSide)
		{
			JSONStorableBool storable = new JSONStorableBool(name, defaultValue);
			storable.storeType = JSONStorableParam.StoreType.Full;
			CreateToggle(storable, rightSide);
			RegisterBool(storable);
			return storable;
		}
				
		private void Update()
		{
			if (SuperController.singleton.isLoading)
			{
				syncNeeded = true;
				syncHookLow = syncHookMid = syncHookHigh = true;
				syncHookCeil = syncHookBed = true;
				syncBars = true;
			}
			
			if (!syncNeeded)
				return;
			
			try 
			{
				SyncValues();				
			}
			catch (System.Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}
		
		private void SyncValues()
		{
			Transform root = containingAtom.transform.parent;
			if (root == null)
				return;
			if (syncHookLow)
				ApplyToNamedTransforms(root, "Hook_Low",  (Transform t) => { SetLocalY(t, hookLow.val);  SetRotationY(t, rotateVertical.val); });
			if (syncHookMid)
				ApplyToNamedTransforms(root, "Hook_Mid",  (Transform t) => { SetLocalY(t, hookMid.val);  SetRotationY(t, rotateVertical.val); });
			if (syncHookHigh)
				ApplyToNamedTransforms(root, "Hook_High", (Transform t) => { SetLocalY(t, hookHigh.val); SetRotationY(t, rotateVertical.val); });
			if (syncHookCeil)
				ApplyToNamedTransforms(root, "Hook_Ceil", (Transform t) => { SetRotationY(t, rotateCeiling.val); });
			if (syncHookBed)
				ApplyToNamedTransforms(root, "Hook_Bed", (Transform t) => { SetRotationY(t, rotateBed.val); });
			if (syncBars)
				ApplyToNamedTransforms(root, "Bars_Vertical", (Transform t) => { SetLocalY(t, bars.val-0.2f); });
			
			syncNeeded = false;
			syncHookLow = syncHookMid = syncHookHigh = false;
			syncHookCeil = syncHookBed = false;
			syncBars = false;
		}
		
		private void SetLocalX(Transform t, float v)
		{
			Vector3 p = t.localPosition;
			p.x = v;
			t.localPosition = p;
		}
		
		private void SetLocalY(Transform t, float v)
		{
			Vector3 p = t.localPosition;
			p.y = v;
			t.localPosition = p;
		}
		
		private void SetLocalZ(Transform t, float v)
		{
			Vector3 p = t.localPosition;
			p.z = v;
			t.localPosition = p;
		}
		
		private void SetRotationY(Transform t, bool enable)
		{
			Quaternion originalRotation;
			bool have = rotationTable.TryGetValue(t, out originalRotation);
			if (enable && !have)
			{
				rotationTable.Add(t, t.localRotation);
				t.localRotation *= Quaternion.AngleAxis(90, Vector3.up);
			}
			else if (!enable && have)
			{
				rotationTable.Remove(t);
				t.localRotation = originalRotation;
			}
		}
		
		private delegate void TransformCallback(Transform t);		
		private int uiLayer = LayerMask.NameToLayer("UI");		
		
		private void ApplyToNamedTransforms(Transform root, string name, TransformCallback callback)
		{
			foreach (Transform child in root)
			{
				if (child.gameObject.layer == uiLayer)
					continue;
				if (child.name == name)
					callback(child);
				ApplyToNamedTransforms(child, name, callback);
			}
		}
    }
}
