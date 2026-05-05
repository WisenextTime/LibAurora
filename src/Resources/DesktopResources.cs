using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Veldrid;
namespace LibAurora.Resources;

/// <summary>
/// Desktop implementation of <see cref="IResources"/>.
/// Provides file access from the application directory, user data directory, and embedded assembly resources.
/// </summary>
public class DesktopResources : IResources
{
	private readonly List<Assembly> _assemblies = [];
	private readonly Dictionary<string, int> _assemblyResources = new();
	private readonly string _executingPath;
	private readonly string _userPath;

	/// <summary>
	/// Creates a new desktop resources provider.
	/// Registers embedded resources from the executing assembly and sets up
	/// application and user data directories.
	/// </summary>
	/// <param name="applicationName">The name of the application used for the user data folder.</param>
	public DesktopResources(string applicationName)
	{
		if (applicationName.ContainsAny(Path.GetInvalidFileNameChars())) throw new InvalidDataException("Invalid app name");
		RegisterAssembly(Assembly.GetEntryAssembly());
		_executingPath = Environment.CurrentDirectory;
		_userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), applicationName);
	}

	/// <inheritdoc/>
	public void RegisterAssembly(Assembly? assembly)
	{
		if (assembly is null) return;
		var assemblyIndex = _assemblies.Count;
		_assemblies.Add(assembly);
		foreach (var resourceName in assembly.GetManifestResourceNames())
		{
			var res = assembly.GetManifestResourceStream(resourceName);
			if (res == null) continue;
			_assemblyResources[resourceName] = assemblyIndex;
		}
	}

	/// <inheritdoc/>
	public Stream GetApplicationResource(string path)
	{
		var standardPath = Path.Combine([_executingPath, ..GetPath(path)]);
		return new FileStream(standardPath, FileMode.Open, FileAccess.Read, FileShare.Read);
	}

	/// <inheritdoc/>
	public Stream GetUserResource(string path)
	{
		var standardPath = Path.Combine([_userPath, ..GetPath(path)]);
		Directory.CreateDirectory(Path.GetDirectoryName(standardPath)!);
		return new FileStream(standardPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
	}

	/// <inheritdoc/>
	public Stream GetAssemblyResource(string path) =>
		(_assemblyResources.TryGetValue(string.Join('.', GetPath(path)), out var stream)
			? _assemblies[stream].GetManifestResourceStream(path)
			: throw new FileNotFoundException($"Assembly resource not found: {path}")) ??
		throw new InvalidOperationException($"Invalid resource {path}");

	private static string[] GetPath(string path) => path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
}