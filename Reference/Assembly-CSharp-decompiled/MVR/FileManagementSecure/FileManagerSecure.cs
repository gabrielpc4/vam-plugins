using System.Collections.Generic;
using System.IO;
using MVR.FileManagement;

namespace MVR.FileManagementSecure;

public class FileManagerSecure
{
	public static string GetFullPath(string path)
	{
		return FileManager.GetFullPath(path);
	}

	public static string NormalizePath(string path)
	{
		return FileManager.NormalizePath(path);
	}

	public static string GetDirectoryName(string path, bool returnSlashPath = false)
	{
		return FileManager.GetDirectoryName(path, returnSlashPath);
	}

	public static string GetFileName(string path)
	{
		return Path.GetFileName(path);
	}

	public static bool FileExists(string path, bool onlySystemFiles = false)
	{
		return FileManager.FileExists(path, onlySystemFiles, restrictPath: true);
	}

	public static bool IsFileInPackage(string path)
	{
		return FileManager.IsFileInPackage(path);
	}

	public static List<ShortCut> GetShortCutsForDirectory(string dir, bool allowNavigationAboveRegularDirectories = false, bool useFullPaths = false, bool generateAllFlattenedShortcut = false, bool includeRegularDirsInFlattenedShortcut = false)
	{
		return FileManager.GetShortCutsForDirectory(dir, allowNavigationAboveRegularDirectories, useFullPaths, generateAllFlattenedShortcut, includeRegularDirsInFlattenedShortcut);
	}

	public static bool DirectoryExists(string path, bool onlySystemDirectories = false)
	{
		return FileManager.DirectoryExists(path, onlySystemDirectories, restrictPath: true);
	}

	public static bool IsDirectoryInPackage(string path)
	{
		return FileManager.IsDirectoryInPackage(path);
	}

	public static string[] GetDirectories(string dir, string pattern = null)
	{
		return FileManager.GetDirectories(dir, pattern, restrictPath: true);
	}

	public static string[] GetFiles(string dir, string pattern = null)
	{
		return FileManager.GetFiles(dir, pattern, restrictPath: true);
	}

	public static void CreateDirectory(string path)
	{
		FileManager.CreateDirectory(path);
	}

	public static byte[] ReadAllBytes(string path)
	{
		return FileManager.ReadAllBytes(path, restrictPath: true);
	}

	public static string ReadAllText(string path)
	{
		return FileManager.ReadAllText(path, restrictPath: true);
	}

	public static void WriteAllText(string path, string text)
	{
		FileManager.WriteAllText(path, text);
	}

	public static void WriteAllBytes(string path, byte[] bytes)
	{
		FileManager.WriteAllBytes(path, bytes);
	}

	public static void DeleteFile(string path)
	{
		FileManager.DeleteFile(path);
	}

	public static void CopyFile(string oldPath, string newPath)
	{
		FileManager.CopyFile(oldPath, newPath, restrictPath: true);
	}

	public static void MoveFile(string oldPath, string newPath)
	{
		FileManager.MoveFile(oldPath, newPath);
	}
}
