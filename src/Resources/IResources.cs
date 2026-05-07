using System.IO;
using System.Reflection;
namespace LibAurora.Resources;

/// <summary>
/// Abstraction over the file io.
/// Provides cross-platform file provider.
/// </summary>
public interface IResources
{
	/// <summary>Get the resources in excusing program directory. Usually read-only.</summary>
	public Stream GetApplicationResource(string path);
	/// <summary>Get the resources in the user data directory.</summary>
	public Stream GetUserResource(string path);
	/// <summary>Get the resources in the DLL file. Usually read-only.</summary>
	public Stream GetAssemblyResource(string path);
	/// <summary>Register the DLL file to read. </summary>
	public void RegisterAssembly(Assembly assembly);
	/// <summary>Get resources with different prefix. </summary>
	public Stream GetResource(string path)
	{
		var paths = path.Split("://");
		if (paths.Length != 2) throw new IOException("Invalid path");
		return paths[0] switch
		{
			"res" => GetApplicationResource(paths[1]),
			"usr" => GetUserResource(paths[1]),
			"ase" => GetAssemblyResource(paths[1]),
			_ => throw new IOException("Invalid path")
		};
	}
}