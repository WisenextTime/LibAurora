using System;
using System.Collections.Generic;
using System.IO;
using LibAurora.Graphics;
using Veldrid;
using Veldrid.SPIRV;
namespace LibAurora.Resources;

public partial class ResourceManager
{
	internal void InitShaderProcessor(IGraphics graphics) =>
		RegisterProcesser(new ResourceProcesser<Shader[]>(this,
			load: stream =>
			{
				using var reader = new StreamReader(stream);
				var source = reader.ReadToEnd();
				var shaders = new List<Shader>();
				var compileOpts = GlslCompileOptions.Default;

				if (ExtractSection(source, "VERTEX") is { } vertSrc)
				{
					var spriv = SpirvCompilation.CompileGlslToSpirv(vertSrc, "shader.vert", ShaderStages.Vertex, compileOpts);
					var desc = new ShaderDescription(ShaderStages.Vertex, spriv.SpirvBytes, "main");
					shaders.Add(graphics.Factory.CreateShader(ref desc));
				}

				if (ExtractSection(source, "GEOMETRY") is { } geomSrc)
				{
					var spriv = SpirvCompilation.CompileGlslToSpirv(geomSrc, "shader.geom", ShaderStages.Geometry, compileOpts);
					var desc = new ShaderDescription(ShaderStages.Geometry, spriv.SpirvBytes, "main");
					shaders.Add(graphics.Factory.CreateShader(ref desc));
				}

				if (ExtractSection(source, "FRAGMENT") is { } fragSrc)
				{
					var spriv = SpirvCompilation.CompileGlslToSpirv(fragSrc, "shader.frag", ShaderStages.Fragment, compileOpts);
					var desc = new ShaderDescription(ShaderStages.Fragment, spriv.SpirvBytes, "main");
					shaders.Add(graphics.Factory.CreateShader(ref desc));
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