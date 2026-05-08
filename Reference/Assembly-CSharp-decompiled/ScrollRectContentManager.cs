using System.Collections.Generic;
using UnityEngine;

public class ScrollRectContentManager : MonoBehaviour
{
	public float buffer = 10f;

	public float bottomExtraSize = 200f;

	public RectTransform contentPanel;

	protected List<RectTransform> items;

	protected void RelayoutPanel()
	{
		if (!(contentPanel != null))
		{
			return;
		}
		float num = buffer;
		Vector2 anchoredPosition = default(Vector2);
		anchoredPosition.x = 0f;
		anchoredPosition.y = 0f - num;
		foreach (RectTransform item in items)
		{
			item.anchoredPosition = anchoredPosition;
			float num2 = item.rect.height + 10f;
			num += num2;
			anchoredPosition.y -= num2;
		}
		num += bottomExtraSize;
		Vector2 sizeDelta = contentPanel.sizeDelta;
		sizeDelta.y = num;
		contentPanel.sizeDelta = sizeDelta;
	}

	public void AddItem(RectTransform item, int index = -1)
	{
		if (items == null)
		{
			items = new List<RectTransform>();
		}
		item.SetParent(base.transform, worldPositionStays: false);
		if (index == -1)
		{
			items.Add(item);
		}
		else if (index < items.Count)
		{
			items.Insert(index, item);
		}
		else
		{
			items.Add(item);
		}
		RelayoutPanel();
	}

	public void RemoveItem(RectTransform item)
	{
		if (items == null)
		{
			items = new List<RectTransform>();
		}
		items.Remove(item);
		RelayoutPanel();
	}

	public void RemoveAllItems()
	{
		if (items != null)
		{
			items.Clear();
			RelayoutPanel();
		}
	}
}
