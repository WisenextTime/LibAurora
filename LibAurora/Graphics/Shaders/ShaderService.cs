using System;
using System.Collections.Generic;
using Raylib_cs;
namespace LibAurora.Graphics.Shaders;

public sealed class ShaderService
{
	public static ShaderService Instance => _instance ?? throw new InvalidOperationException("Input service is not initialized.");

	private Dictionary<uint, Shader> _shaders = new();
	internal  ShaderService()
	{
		if(_instance != null) throw new InvalidOperationException("Shader service already been created");
		_instance = this;
	}
	private static ShaderService? _instance;

	public void RegisterShader(Shader shader)
	{
		_shaders.TryAdd(shader.Id, shader);
	}

	public void UnregisterShader(Shader shader)
	{
		if (!_shaders.Remove(shader.Id))
		{
			Raylib.UnloadShader(shader);
		}
	}
	public Shader GetShader(uint id) => _shaders[id];
}