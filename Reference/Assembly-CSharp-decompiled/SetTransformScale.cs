using System.Collections.Generic;
using UnityEngine;

public class SetTransformScale : JSONStorable
{
	public Transform additionalScaleTransform;

	public Transform additionalScaleTransform2;

	public bool alignAdditionalScaleTransform = true;

	protected JSONStorableFloat scaleJSON;

	[SerializeField]
	protected float _scale = 1f;

	protected void SyncScale(float f)
	{
		_scale = f;
		Vector3 localScale = default(Vector3);
		localScale.x = f;
		localScale.y = f;
		localScale.z = f;
		if (additionalScaleTransform != null)
		{
			if (alignAdditionalScaleTransform)
			{
				List<Transform> list = new List<Transform>();
				foreach (Transform item3 in additionalScaleTransform)
				{
					list.Add(item3);
				}
				foreach (Transform item4 in list)
				{
					item4.SetParent(null, worldPositionStays: true);
				}
				additionalScaleTransform.position = base.transform.position;
				additionalScaleTransform.rotation = base.transform.rotation;
				foreach (Transform item5 in list)
				{
					item5.SetParent(additionalScaleTransform, worldPositionStays: true);
					item5.localScale = Vector3.one;
				}
			}
			additionalScaleTransform.localScale = localScale;
		}
		if (additionalScaleTransform2 != null)
		{
			if (alignAdditionalScaleTransform)
			{
				List<Transform> list2 = new List<Transform>();
				foreach (Transform item6 in additionalScaleTransform2)
				{
					list2.Add(item6);
				}
				foreach (Transform item7 in list2)
				{
					item7.SetParent(null, worldPositionStays: true);
				}
				additionalScaleTransform2.position = base.transform.position;
				additionalScaleTransform2.rotation = base.transform.rotation;
				foreach (Transform item8 in list2)
				{
					item8.SetParent(additionalScaleTransform2, worldPositionStays: true);
					item8.localScale = Vector3.one;
				}
			}
			additionalScaleTransform2.localScale = localScale;
		}
		base.transform.localScale = localScale;
		if (containingAtom != null)
		{
			containingAtom.ScaleChanged(f);
		}
	}

	protected void Init()
	{
		scaleJSON = new JSONStorableFloat("scale", _scale, SyncScale, 0.01f, 10f);
		RegisterFloat(scaleJSON);
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			SetTransformScaleUI componentInChildren = UITransform.GetComponentInChildren<SetTransformScaleUI>();
			if (componentInChildren != null)
			{
				scaleJSON.slider = componentInChildren.scaleSlider;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (UITransformAlt != null)
		{
			SetTransformScaleUI componentInChildren = UITransformAlt.GetComponentInChildren<SetTransformScaleUI>();
			if (componentInChildren != null)
			{
				scaleJSON.sliderAlt = componentInChildren.scaleSlider;
			}
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			InitUI();
			InitUIAlt();
		}
	}
}
