using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverPopup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public GameObject popup;

	protected void ShowPopup()
	{
		if (popup != null)
		{
			popup.SetActive(value: true);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		ShowPopup();
	}

	protected void HidePopup()
	{
		if (popup != null)
		{
			popup.SetActive(value: false);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		HidePopup();
	}

	private void OnEnable()
	{
		HidePopup();
	}

	private void OnDisable()
	{
		HidePopup();
	}
}
