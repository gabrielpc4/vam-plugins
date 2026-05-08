using System.Collections.Generic;
using UnityEngine;

public class MVRPlugin
{
	public string uid;

	public JSONStorableUrl pluginURLJSON;

	public List<MVRScriptController> scriptControllers;

	public Transform configUI;

	public RectTransform scriptControllerContent;

	public MVRPlugin()
	{
		scriptControllers = new List<MVRScriptController>();
	}
}
