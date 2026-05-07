using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LibAurora.Graphics;
using Veldrid;
using Veldrid.SPIRV;
namespace LibAurora.Resources;

public static partial class ResourceManager
{
	internal static void InitShaderProcessor(IGraphics graphics) =>
		RegisterProcesser(new ResourceProcesser<Shader[]>(
			load: stream =>
			{
				using var reader = new StreamReader(stream);
				var source = reader.ReadToEnd();
				var shaders = new List<Shader>();
				var factory = graphics.Factory;
				var compileOpts = GlslCompileOptions.Default;
				var isD3D11 = graphics.Device.BackendType == GraphicsBackend.Direct3D11;

				var vertSrc = ExtractSection(source, "VERTEX");
				var fragSrc = ExtractSection(source, "FRAGMENT");

				if (isD3D11 && vertSrc is not null && fragSrc is not null)
				{
					var result = SpirvCompilation.CompileVertexFragment(
						Encoding.UTF8.GetBytes(vertSrc),
						Encoding.UTF8.GetBytes(fragSrc),
						CrossCompileTarget.HLSL);

					var vertDesc = new ShaderDescription(ShaderStages.Vertex,
						Encoding.UTF8.GetBytes(result.VertexShader), "main");
					shaders.Add(factory.CreateShader(ref vertDesc));

					var fragDesc = new ShaderDescription(ShaderStages.Fragment,
						Encoding.UTF8.GetBytes(result.FragmentShader), "main");
					shaders.Add(factory.CreateShader(ref fragDesc));
				}
				else
				{
					if (vertSrc is not null)
					{
						var spriv = SpirvCompilation.CompileGlslToSpirv(vertSrc,
							"shader.vert", ShaderStages.Vertex, compileOpts);
						var desc = new ShaderDescription(ShaderStages.Vertex, spriv.SpirvBytes, "main");
						shaders.Add(factory.CreateShader(ref desc));
					}

					if (ExtractSection(source, "GEOMETRY") is { } geomSrc)
					{
						var spriv = SpirvCompilation.CompileGlslToSpirv(geomSrc,
							"shader.geom", ShaderStages.Geometry, compileOpts);
						var desc = new ShaderDescription(ShaderStages.Geometry, spriv.SpirvBytes, "main");
						shaders.Add(factory.CreateShader(ref desc));
					}

					if (fragSrc is not null)
					{
						var spriv = SpirvCompilation.CompileGlslToSpirv(fragSrc,
							"shader.frag", ShaderStages.Fragment, compileOpts);
						var desc = new ShaderDescription(ShaderStages.Fragment, spriv.SpirvBytes, "main");
						shaders.Add(factory.CreateShader(ref desc));
					}
				}

				return shaders.Count == 0
					? throw new InvalidDataException("No shader stage found in the source file")
					: shaders.ToArray();
			},
			save: (_, _) => throw new NotSupportedException()
		));

	private static string? ExtractSection(string source, string stage)
	{
		var ifdef = $"#ifdef {stage}";
		var start = source.IndexOf(ifdef, StringComparison.Ordinal);
		if (start < 0) return null;
		start += ifdef.Length;
		var newline = source.IndexOf('\n', start);
		if (newline >= 0) start = newline + 1;
		var end = source.IndexOf("#endif", start, StringComparison.Ordinal);
		if (end < 0) end = source.Length;
		return source[start..end].Trim();
	}
}