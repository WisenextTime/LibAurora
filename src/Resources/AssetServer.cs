using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LibAurora.Core;
using LibAurora.Debug;
namespace LibAurora.Resources;

/// <summary>The engine-level asset loading service. </summary>
public static partial class AssetServer
{
	private readonly static Dictionary<Type, IResourceProcesser> _processers = new();
	/// <summary>The resource service. </summary>
	private static ResourcesBase? _services;
	public static ResourcesBase Services => _services ?? throw new InvalidOperationException("AssetServer is not initialized");

	/// <summary>Registers a resource processor for the specified type <typeparamref name="T"/>.</summary>
	public static void RegisterProcesser<T>(ResourceProcesser<T> processer) => _processers[typeof(T)] = processer;
	/// <summary>Loads a resource of type <typeparamref name="T"/> from the specified path.</summary>
	public static T LoadResource<T>(string resourcePath)
		=> _processers[typeof(T)] is not ResourceProcesser<T> processer
			? throw new ArgumentException($"Resource process type {typeof(T).FullName} not found")
			: processer.Load(resourcePath);
	/// <summary>Saves a resource of type <typeparamref name="T"/> to the specified path.</summary>
	public static void SaveResource<T>(string resourcePath, T resource)
	{
		if (_processers[typeof(T)] is not ResourceProcesser<T> processer)
			throw new ArgumentException($"Resource process type {typeof(T).FullName} not found");
		processer.Save(resource, resourcePath);
	}
	/// <summary> Initializes GPU resource processors. When <paramref name="graphics"/> is provided. </summary>
	public static void Init(ApplicationContext context)
	{
		_services = context.ResourcesBase;
		_services.RegisterAssemblyResources(typeof(AssetServer).Assembly);
		_services.RegisterAssemblyResources(Assembly.GetEntryAssembly());
		if (context.Graphics is { } graphics)
		{
			InitTextureProcessor(graphics);
			InitShaderProcessor(graphics);
		}
		else LogServer.Log("No graphics device enabled, skipping texture and shader processors.");
		InitWaveSourceProcessor();
	}
}
/// <summary>Internal interface for resource processor type-erasure.</summary>
internal interface IResourceProcesser;
/// <summary>
/// Typed resource processor that loads and saves resources of type <typeparamref name="T"/>
/// using the provided load and save delegates.
/// </summary>
public class ResourceProcesser<T>(Func<Stream, T> load, Action<T, Stream> save) : IResourceProcesser
{
	/// <summary>Loads a resource from the specified path.</summary>
	public T Load(string path)
	{
		using var fs = AssetServer.Services.GetResource(path);
		return load(fs);
	}

	/// <summary>Saves a resource to the specified path.</summary>
	public void Save(T resource, string path)
	{
		using var fs = AssetServer.Services.GetResource(path);
		if (!fs.CanWrite) throw new IOException($"Resource {path} can't be written");
		save(resource, fs);
	}
}