using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
namespace LibAurora.Resources;

/// <summary>
///     Abstraction over the file io.
///     Provides cross-platform file provider.
/// </summary>
public abstract class ResourcesBase
{
	private readonly Dictionary<string, ResourceProvider> _providers;
	/// <summary>The provider used to read embedded assembly resources.</summary>
	protected readonly AssemblyProvider AssemblyProvider;
	protected ResourcesBase()
	{
		_providers = new Dictionary<string, ResourceProvider>();
		AddProvider("res", GetLocalProvider());
		AddProvider("usr", GetUserProvider());
		AssemblyProvider = new AssemblyProvider();
		AddProvider("ase", AssemblyProvider);
	}
	/// <summary>Adds a resource provider for the specified path prefix.</summary>
	public void AddProvider(string prefix, ResourceProvider provider) => _providers.Add(prefix, provider);

	/// <summary>Gets the default local resource provider.</summary>
	protected abstract ResourceProvider GetLocalProvider();
	/// <summary>Gets the user-writable resource provider.</summary>
	protected abstract ResourceProvider GetUserProvider();

	/// <summary>Registers embedded resources from the specified assembly.</summary>
	public void RegisterAssemblyResources(Assembly? assembly) => AssemblyProvider.RegisterAssembly(assembly);

	private (string provider, string path) GetStandardPath(string path)
	{
		var paths = path.Split("://");
		return paths.Length switch
		{
			1 => ("res", paths[0]),
			2 => _providers.ContainsKey(paths[0]) ? (paths[0], paths[1]) : ("res", path),
			_ => throw new IOException("Invalid path"),
		};
	}
	/// <summary>Get resources with different prefix. </summary>
	public Stream GetResource(string path)
	{
		var paths = GetStandardPath(path);
		return _providers[paths.provider].GetResource(paths.path);
	}
	/// <summary>Get resources in directory.</summary>
	public IEnumerable<ResourceEntry> GetResourceEntries(string path)
	{
		var paths = GetStandardPath(path);
		return _providers[paths.provider].GetResourceEntries(paths.path)
			.Select(entry => entry with { Path = $"{paths.provider}://{entry.Path}" });
	}
	/// <summary>Get whether a resource exists. </summary>
	public bool FileExists(string path)
	{
		var paths = GetStandardPath(path);
		return _providers[paths.provider].FileExists(paths.path);
	}

	/// <summary>Get whether a directory exists. </summary>
	public bool DirectoryExists(string path)
	{
		var paths = GetStandardPath(path);
		return _providers[paths.provider].DirectoryExists(paths.path);
	}
	/// <summary>Create a directory. </summary>
	public void CreateDirectory(string path)
	{
		var paths = GetStandardPath(path);
		_providers[paths.provider].CreateDirectory(paths.path);
	}
}
public abstract class ResourceProvider
{
	/// <summary>Opens a resource stream from the specified provider-local path.</summary>
	public abstract Stream GetResource(string path);
	/// <summary>Gets resource entries under the specified provider-local path.</summary>
	public abstract IEnumerable<ResourceEntry> GetResourceEntries(string path);
	/// <summary>Gets whether the specified provider-local file exists.</summary>
	public abstract bool FileExists(string path);
	/// <summary>Gets whether the specified provider-local directory exists.</summary>
	public abstract bool DirectoryExists(string path);
	/// <summary>Creates a directory at the specified provider-local path.</summary>
	public abstract void CreateDirectory(string path);
}
/// <summary>Represents a resource found by a provider, including a logical path and stream opener.</summary>
public readonly struct ResourceEntry(string path, Func<Stream> open, string? physicalPath = null)
{
	/// <summary>The file name portion of <see cref="Path" />.</summary>
	public string Name => System.IO.Path.GetFileName(Path);
	/// <summary>The logical resource path.</summary>
	public string Path { get; init; } = path;
	/// <summary>Opens a new readable stream for this resource.</summary>
	public Func<Stream> Open { get; init; } = open;
	/// <summary>The physical file path when the provider can expose one.</summary>
	public string? PhysicalPath { get; init; } = physicalPath;
}