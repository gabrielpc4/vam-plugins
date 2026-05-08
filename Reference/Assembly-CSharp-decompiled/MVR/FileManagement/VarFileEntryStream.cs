using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace MVR.FileManagement;

public class VarFileEntryStream : FileEntryStream
{
	protected ZipFile zipFile;

	public VarFileEntryStream(VarFileEntry entry)
		: base(entry)
	{
		if (entry.Simulated)
		{
			string path = entry.Package.Path + "\\" + entry.InternalPath;
			base.Stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			return;
		}
		FileStream file = File.OpenRead(entry.Package.Path);
		zipFile = new ZipFile(file);
		zipFile.IsStreamOwner = true;
		ZipEntry entry2 = zipFile.GetEntry(entry.InternalSlashPath);
		if (entry2 == null)
		{
			Dispose();
			throw new Exception("Could not find entry " + entry.InternalSlashPath + " in zip file " + entry.Package.Path);
		}
		base.Stream = zipFile.GetInputStream(entry2);
	}

	public override void Dispose()
	{
		base.Dispose();
		if (zipFile != null)
		{
			zipFile.Close();
		}
	}
}
