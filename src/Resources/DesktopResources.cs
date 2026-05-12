using System;
using System.IO;
namespace LibAurora.Resources;

/// <summary>
/// Desktop implementation of <see cref="ResourcesBase"/>.
/// Provides file access from the application directory, user data directory, and embedded assembly resources.
/// </summary>
public class DesktopResources(string userFileDirectoryName) : ResourcesBase
{
	/// <summary>Gets the application working directory provider.</summary>
	protected override ResourceProvider GetLocalProvider()
		=> new PhysicalProvider(Environment.CurrentDirectory);
	/// <summary>Gets the application data directory provider.</summary>
	protected override ResourceProvider GetUserProvider()
		=> new PhysicalProvider(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			userFileDirectoryName));
}