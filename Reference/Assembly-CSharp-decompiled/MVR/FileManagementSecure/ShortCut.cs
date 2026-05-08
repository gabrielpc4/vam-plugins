using System;

namespace MVR.FileManagementSecure;

[Serializable]
public class ShortCut
{
	public string path;

	public string package = string.Empty;

	public string packageFilter;

	public string displayName;

	public bool isLatest = true;

	public bool flatten;

	public bool includeRegularDirsInFlatten;
}
