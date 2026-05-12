using System;
using System.Collections.Generic;
using System.IO;
namespace LibAurora.Resources;

/// <summary>Provides access to resources stored in a physical directory.</summary>
public class PhysicalProvider(string basePath, bool readOnly = true) : ResourceProvider
{
	private string ToStandardPath(string path)
	{
		var paths = path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
		return Path.Combine([basePath, ..paths]);
	}
	/// <summary>Opens a file stream from the specified provider-local path.</summary>
	public override Stream GetResource(string path)
	{
		var standardPath = ToStandardPath(path);
		return File.Open(standardPath,
			readOnly ? FileMode.Open : FileMode.OpenOrCreate,
			readOnly ? FileAccess.Read : FileAccess.ReadWrite,
			readOnly ? FileShare.Read : FileShare.ReadWrite);
	}
	/// <summary>Gets file entries under the specified provider-local directory.</summary>
	public override IEnumerable<ResourceEntry> GetResourceEntries(string path)
	{
		var standardPath = ToStandardPath(path);
		foreach (var entry in Directory.EnumerateFiles(standardPath, "*", SearchOption.AllDirectories))
		{
			var relativePath = Path.GetRelativePath(basePath, entry).Replace('\\', '/');
			yield return new ResourceEntry(relativePath,
				() => File.Open(entry, readOnly ? FileMode.Open : FileMode.OpenOrCreate,
					readOnly ? FileAccess.Read : FileAccess.ReadWrite,
					readOnly ? FileShare.Read : FileShare.ReadWrite),
				entry);
		}
	}
	/// <summary>Gets whether the specified provider-local file exists.</summary>
	public override bool FileExists(string path)
	{
		var standardPath = ToStandardPath(path);
		return File.Exists(standardPath);
	}
	/// <summary>Gets whether the specified provider-local directory exists.</summary>
	public override bool DirectoryExists(string path)
	{
		var standardPath = ToStandardPath(path);
		return Directory.Exists(standardPath);
	}
	/// <summary>Creates a directory at the specified provider-local path.</summary>
	public override void CreateDirectory(string path)
	{
		var standardPath = ToStandardPath(path);
		Directory.CreateDirectory(standardPath);
	}
}