using System;
using UnityEngine;

namespace MVR.FileManagement;

public class VarFileEntry : FileEntry
{
	public VarPackage Package { get; protected set; }

	public string InternalPath { get; protected set; }

	public string InternalSlashPath { get; protected set; }

	public bool Simulated { get; protected set; }

	public VarFileEntry(VarPackage vp, string entryName, DateTime lastWriteTime, bool simulated = false)
		: base(entryName)
	{
		Package = vp;
		InternalSlashPath = entryName;
		Uid = vp.Uid + ":/" + InternalSlashPath;
		InternalPath = InternalSlashPath.Replace('/', '\\');
		Path = vp.Path + ":\\" + InternalPath;
		SlashPath = Path.Replace('\\', '/');
		FullPath = vp.FullPath + ":\\" + InternalPath;
		FullSlashPath = FullPath.Replace('\\', '/');
		LastWriteTime = lastWriteTime;
		Simulated = simulated;
		if (FileManager.debug)
		{
			Debug.Log("New var file entry\n Uid: " + Uid + "\n Path: " + Path + "\n FullPath: " + FullPath + "\n SlashPath: " + SlashPath + "\n Name: " + Name + "\n InternalSlashPath: " + InternalSlashPath);
		}
	}

	public override FileEntryStream OpenStream()
	{
		return new VarFileEntryStream(this);
	}

	public override FileEntryStreamReader OpenStreamReader()
	{
		return new VarFileEntryStreamReader(this);
	}
}
