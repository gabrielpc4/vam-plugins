using UnityEngine;
using UnityEngine.UI;

namespace uFileBrowser;

public class DirectoryButton : MonoBehaviour
{
	public Text packageLabel;

	public Text label;

	[HideInInspector]
	public string package;

	[HideInInspector]
	public string packageFilter;

	[HideInInspector]
	public string text;

	[HideInInspector]
	public string fullPath;

	[HideInInspector]
	public int id;

	private FileBrowser browser;

	public void OnClick()
	{
		if ((bool)browser)
		{
			browser.OnDirectoryClick(id);
		}
	}

	public void Set(FileBrowser b, string pkg, string pkgFilter, string txt, string path, int i)
	{
		browser = b;
		package = pkg;
		packageFilter = pkgFilter;
		text = txt;
		fullPath = path;
		id = i;
		if (packageLabel != null)
		{
			if (package == string.Empty)
			{
				packageLabel.gameObject.SetActive(value: false);
			}
			else
			{
				packageLabel.gameObject.SetActive(value: true);
			}
			packageLabel.text = package;
		}
		if (label != null)
		{
			label.text = text;
		}
	}
}
