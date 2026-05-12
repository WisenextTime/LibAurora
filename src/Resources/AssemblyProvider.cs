using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
namespace LibAurora.Resources;

/// <summary>Provides access to resources embedded in registered assemblies.</summary>
public class AssemblyProvider : ResourceProvider
{

	private readonly Dictionary<string, (Assembly Assembly, string Resource)> _assemblyResourceMap = [];
	private readonly HashSet<string> _logicalResources = [];
	/// <summary>Registers all embedded resources from the specified assembly.</summary>
	public void RegisterAssembly(Assembly? assembly)
	{
		if (assembly is null) return;
		foreach (var resource in assembly.GetManifestResourceNames())
		{
			_assemblyResourceMap[resource] = (assembly, resource);
			var logicalPath = ToLogicalPath(assembly, resource);
			_assemblyResourceMap[logicalPath] = (assembly, resource);
			_logicalResources.Add(logicalPath);
		}
	}

	private static string ToLogicalPath(Assembly assembly, string resource)
	{
		var extensionIndex = resource.LastIndexOf('.');
		if (extensionIndex < 0) return resource;

		var path = resource[..extensionIndex].Replace('.', '/');
		return $"{path}{resource[extensionIndex..]}";
	}
	/// <summary>Opens an embedded resource stream from the specified logical path.</summary>
	public override Stream GetResource(string path)
	{
		var (assembly, resource) = _assemblyResourceMap[path];
		return assembly.GetManifestResourceStream(resource) ?? throw new IOException($"Resource {path} not found");
	}
	/// <summary>Gets embedded resource entries under the specified logical path.</summary>
	public override IEnumerable<ResourceEntry> GetResourceEntries(string path)
		=> _logicalResources.Where(resource => resource.StartsWith(path))
			.Select(resource => new ResourceEntry(resource, () => GetResource(resource)));

	/// <summary>Gets whether the specified embedded resource exists.</summary>
	public override bool FileExists(string path) => _assemblyResourceMap.ContainsKey(path);

	/// <summary>Gets whether any embedded resource exists under the specified logical path.</summary>
	public override bool DirectoryExists(string path) => _logicalResources.Any(resource => resource.StartsWith(path));
	/// <summary>Throws because embedded assembly resources are read-only.</summary>
	public override void CreateDirectory(string path) => throw new IOException("Cannot create directory in assembly provider");
}