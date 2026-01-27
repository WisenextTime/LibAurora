using System;
using System.Collections.Generic;
namespace LibAurora.Graphics.Shader;

public sealed class ShaderService
{
	public static ShaderService Instance => _instance ?? throw new InvalidOperationException("Input service is not initialized.");
	
	
	private Stack<int> _idStack = [];
	internal  ShaderService()
	{
		if(_instance != null) throw new InvalidOperationException("Shader service already been created");
		_instance = this;
	}
	private static ShaderService? _instance;
}