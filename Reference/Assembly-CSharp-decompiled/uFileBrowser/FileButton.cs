using UnityEngine;
using UnityEngine.UI;

namespace uFileBrowser;

public class FileButton : MonoBehaviour
{
	public Button button;

	public Image buttonImage;

	public Image fileIcon;

	public RawImage altIcon;

	public Text label;

	public Sprite selectedSprite;

	public Button renameButton;

	public Button deleteButton;

	[HideInInspector]
	public string text;

	[HideInInspector]
	public string fullPath;

	[HideInInspector]
	public string removedPrefix;

	[HideInInspector]
	public bool isDir;

	[HideInInspector]
	public int id;

	private FileBrowser browser;

	public void Select()
	{
		button.transition = Selectable.Transition.None;
		buttonImage.overrideSprite = selectedSprite;
	}

	public void Unselect()
	{
		button.transition = Selectable.Transition.SpriteSwap;
		buttonImage.overrideSprite = null;
	}

	public void OnClick()
	{
		if ((bool)browser)
		{
			browser.OnFileClick(id);
		}
	}

	public void OnRenameClick()
	{
		if ((bool)browser)
		{
			browser.OnRenameClick(id);
		}
	}

	public void OnDeleteClick()
	{
		if ((bool)browser)
		{
			browser.OnDeleteClick(id);
		}
	}

	public void Set(FileBrowser b, string txt, string path, bool dir, int i, bool writeable)
	{
		browser = b;
		text = txt;
		fullPath = path;
		isDir = dir;
		id = i;
		label.text = text;
		if (isDir)
		{
			fileIcon.sprite = b.folderIcon;
		}
		else
		{
			fileIcon.sprite = b.GetFileIcon(txt);
		}
		if (deleteButton != null)
		{
			deleteButton.gameObject.SetActive(writeable);
		}
		if (renameButton != null)
		{
			renameButton.gameObject.SetActive(writeable);
		}
	}
}
