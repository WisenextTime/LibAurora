using System;
using System.Collections.Generic;
using Raylib_cs;
namespace LibAurora.Graphics.Shaders;

public sealed class ShaderService
{
	/// <summary>
	/// Get the ShaderService singleton
	/// </summary>
	/// <exception cref="InvalidOperationException">ShaderService is not initialized.</exception>
	public static ShaderService Instance => _instance ?? throw new InvalidOperationException("Input service is not initialized.");

	private Dictionary<uint, Shader> _shaders = new();
	internal  ShaderService()
	{
		if(_instance != null) throw new InvalidOperationException("Shader service already been created");
		_instance = this;
	}
	private static ShaderService? _instance;

	/// <summary>
	/// Register a shader into ShaderService
	/// </summary>
	/// <param name="shader"></param>
	public void RegisterShader(Shader shader)
	{
		_shaders.TryAdd(shader.Id, shader);
	}
	/// <summary>
	/// Unregister a shader from ShaderService
	/// </summary>
	/// <param name="shader"></param>
	public void UnregisterShader(Shader shader)
	{
		if (!_shaders.Remove(shader.Id))
		{
			Raylib.UnloadShader(shader);
		}
	}
	/// <summary>
	/// Get a registered shader with id
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public Shader GetShader(uint id) => _shaders[id];
}