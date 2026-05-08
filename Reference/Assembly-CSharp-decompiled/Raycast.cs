using UnityEngine;

public class Raycast : MonoBehaviour
{
	public float range;

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo, range))
		{
			if (hitInfo.transform.tag == "Lever")
			{
				hitInfo.transform.gameObject.GetComponent<LeverControll>().turn();
			}
			else if (hitInfo.transform.tag == "Button")
			{
				hitInfo.transform.gameObject.GetComponent<ButtonControl>().turn();
			}
			else if (!(hitInfo.transform.tag == "Code"))
			{
			}
		}
	}
}
