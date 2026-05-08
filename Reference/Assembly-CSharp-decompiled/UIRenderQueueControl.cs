using UnityEngine;
using UnityEngine.UI;

public class UIRenderQueueControl : JSONStorable
{
	protected JSONStorableFloat renderQueueJSON;

	public Transform rendererContainer;

	public Material sharedMaterial;

	protected Material runtimeMaterial;

	public int renderQueue
	{
		get
		{
			if (renderQueueJSON != null)
			{
				return Mathf.FloorToInt(renderQueueJSON.val);
			}
			return 0;
		}
		set
		{
			if (renderQueueJSON != null)
			{
				renderQueueJSON.val = value;
			}
		}
	}

	protected void SyncRenderQueue(float f)
	{
		int num = Mathf.FloorToInt(f);
		if (runtimeMaterial != null)
		{
			runtimeMaterial.renderQueue = num;
		}
	}

	protected void Init()
	{
		if (!(sharedMaterial != null))
		{
			return;
		}
		runtimeMaterial = Object.Instantiate(sharedMaterial);
		renderQueueJSON = new JSONStorableFloat("renderQueue", runtimeMaterial.renderQueue, SyncRenderQueue, -1f, 5000f);
		RegisterFloat(renderQueueJSON);
		if (rendererContainer != null)
		{
			Graphic[] componentsInChildren = rendererContainer.GetComponentsInChildren<Graphic>(includeInactive: true);
			Graphic[] array = componentsInChildren;
			foreach (Graphic graphic in array)
			{
				graphic.material = runtimeMaterial;
			}
		}
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (t != null)
		{
			UIRenderQueueControlUI componentInChildren = t.GetComponentInChildren<UIRenderQueueControlUI>();
			if (componentInChildren != null && renderQueueJSON != null)
			{
				renderQueueJSON.RegisterSlider(componentInChildren.renderQueueSlider, isAlt);
			}
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			if (Application.isPlaying)
			{
				Init();
				InitUI();
				InitUIAlt();
			}
		}
	}
}
