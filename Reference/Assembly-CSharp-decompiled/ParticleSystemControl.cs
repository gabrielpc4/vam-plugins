using UnityEngine;

public class ParticleSystemControl : JSONStorable
{
	public ParticleSystem[] systems1;

	public ParticleSystem[] systems2;

	public Material mat1;

	public Material mat2;

	protected JSONStorableFloat system1MaterialAlphaJSON;

	protected JSONStorableFloat system2MaterialAlphaJSON;

	protected void SyncSystem1MaterialAlpha(float f)
	{
		ParticleSystem[] array = systems1;
		foreach (ParticleSystem particleSystem in array)
		{
			ParticleSystemRenderer component = particleSystem.GetComponent<ParticleSystemRenderer>();
			if (component != null)
			{
				Material material = component.material;
				Color color = material.GetColor("_TintColor");
				color.a = f;
				material.SetColor("_TintColor", color);
			}
		}
	}

	protected void SyncSystem2MaterialAlpha(float f)
	{
		ParticleSystem[] array = systems2;
		foreach (ParticleSystem particleSystem in array)
		{
			ParticleSystemRenderer component = particleSystem.GetComponent<ParticleSystemRenderer>();
			if (component != null)
			{
				Material material = component.material;
				Color color = material.GetColor("_TintColor");
				color.a = f;
				material.SetColor("_TintColor", color);
			}
		}
	}

	protected void Init()
	{
		if (mat1 != null)
		{
			system1MaterialAlphaJSON = new JSONStorableFloat("system1MaterialAlpha", mat1.GetColor("_TintColor").a, SyncSystem1MaterialAlpha, 0f, 1f);
			RegisterFloat(system1MaterialAlphaJSON);
		}
		if (mat2 != null)
		{
			system2MaterialAlphaJSON = new JSONStorableFloat("system2MaterialAlpha", mat2.GetColor("_TintColor").a, SyncSystem2MaterialAlpha, 0f, 1f);
			RegisterFloat(system2MaterialAlphaJSON);
		}
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		ParticleSystemControlUI componentInChildren = UITransform.GetComponentInChildren<ParticleSystemControlUI>();
		if (componentInChildren != null)
		{
			if (system1MaterialAlphaJSON != null)
			{
				system1MaterialAlphaJSON.slider = componentInChildren.system1MaterialAlphaSlider;
			}
			if (system2MaterialAlphaJSON != null)
			{
				system2MaterialAlphaJSON.slider = componentInChildren.system2MaterialAlphaSlider;
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
		}
	}
}
