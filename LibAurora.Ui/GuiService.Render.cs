using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using Raylib_cs;
namespace LibAurora.Ui;

public partial class GuiService
{
	private readonly List<Vertex> _vertices = [];
	private readonly List<ushort> _indices = [];
	private readonly List<RenderCommand> _commands = [];
        
	private struct Vertex
	{
		public Vector2 Position;
		public Vector2 UV;
		public Color Color;
            
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vertex FromImDrawVert(ImDrawVert vert)
		{
			var col = vert.col;
			return new Vertex
			{
				Position = new Vector2(vert.pos.X, vert.pos.Y),
				UV = new Vector2(vert.uv.X,vert.uv.Y),
				Color = new Color(
					(byte)(col & 0xFF),
					(byte)(col >> 8 & 0xFF),
					(byte)(col >> 16 & 0xFF),
					(byte)(col >> 24)
				),
			};
		}
	}
        
	private struct RenderCommand
	{
		public nint Texture;
		public Rectangle ClipRect;
		public int IndexOffset;
		public long IndexCount;
	}
	private bool _frameStarted;
	
	public void BeginFrame()
	{
		_io.DisplaySize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
		_io.DeltaTime = Raylib.GetFrameTime();
		ImGui.NewFrame();
		_frameStarted = true;
		InputCheck();
	}
    
	public void EndFrame()
	{
		if (!_frameStarted) return;
		Draw();
		_frameStarted = false;
	}
	private void Draw()
	{
		ImGui.Render();
		var drawData = ImGui.GetDrawData();
		if (!drawData.Valid || drawData.CmdListsCount == 0) return;
		_vertices.Clear();
		_indices.Clear();
		_commands.Clear();
		for (var n = 0; n < drawData.CmdListsCount; n++)
		{
			ProcessDrawList(drawData.CmdLists[n]);
		}
		ExecuteRenderCommands(drawData);
	}
	private unsafe void ProcessDrawList(ImDrawListPtr cmdList)
	{
		var vertexOffset = _vertices.Count;
		var vtxBuffer = (ImDrawVert*)cmdList.VtxBuffer.Data;
    
		for (var i = 0; i < cmdList.VtxBuffer.Size; i++)
		{
			_vertices.Add(Vertex.FromImDrawVert(vtxBuffer[i]));
		}
    
		var indexOffset = _indices.Count;
		var idxBuffer = (ushort*)cmdList.IdxBuffer.Data;
    
		for (var i = 0; i < cmdList.IdxBuffer.Size; i++)
		{
			_indices.Add((ushort)(idxBuffer[i] + vertexOffset));
		}
    
		var currentIndexOffset = indexOffset;
    
		for (var cmdIndex = 0; cmdIndex < cmdList.CmdBuffer.Size; cmdIndex++)
		{
			var cmd = cmdList.CmdBuffer[cmdIndex];
        
			if (cmd.ElemCount == 0) continue;

			_commands.Add(new RenderCommand
			{
				Texture = cmd.GetTexID(),
				ClipRect = new Rectangle(
					cmd.ClipRect.X,
					cmd.ClipRect.Y,
					cmd.ClipRect.Z - cmd.ClipRect.X,
					cmd.ClipRect.W - cmd.ClipRect.Y
				),
				IndexOffset = currentIndexOffset,
				IndexCount = cmd.ElemCount
			});
			currentIndexOffset += (int)cmd.ElemCount;
		}
	}
	private void ExecuteRenderCommands(ImDrawDataPtr drawData)
	{
		Rlgl.DisableDepthTest(); 
		Rlgl.DisableBackfaceCulling();
		Rlgl.SetBlendMode(BlendMode.Alpha);
		Rlgl.SetBlendFactors(
			Rlgl.SRC_ALPHA, 
			Rlgl.ONE_MINUS_SRC_ALPHA,
			Rlgl.BLEND_EQUATION_ALPHA
		);
		Rlgl.EnableColorBlend();
		Rlgl.EnableScissorTest();
		var prevMatrix = Rlgl.GetMatrixModelview();
		try
		{
			SetupOrthographicProjection(drawData);
			foreach (var cmd in _commands)
			{
				var clipRect = new Rectangle(
					cmd.ClipRect.X * drawData.FramebufferScale.X,
					cmd.ClipRect.Y * drawData.FramebufferScale.Y,
					cmd.ClipRect.Width * drawData.FramebufferScale.X,
					cmd.ClipRect.Height * drawData.FramebufferScale.Y
				);
				Raylib.BeginScissorMode(
					(int)clipRect.X,
					(int)clipRect.Y,
					(int)clipRect.Width,
					(int)clipRect.Height
				);
				Rlgl.ActiveTextureSlot(0);
				DrawTriangles(cmd);
				Raylib.EndScissorMode(); 
			}
		}
		finally
		{
			Rlgl.EnableBackfaceCulling();
			Rlgl.DisableScissorTest();
			Rlgl.DisableColorBlend();
			Rlgl.SetMatrixModelView(prevMatrix);
			RestoreProjection();
		}
	}
	private void SetupOrthographicProjection(ImDrawDataPtr drawData)
	{
		Rlgl.MatrixMode(MatrixMode.Projection);
		Rlgl.PushMatrix();
		Rlgl.MatrixMode(MatrixMode.ModelView);
		Rlgl.PushMatrix();
		
		Rlgl.MatrixMode(MatrixMode.Projection);
		Rlgl.LoadIdentity();
		var l = drawData.DisplayPos.X;
		var r = drawData.DisplayPos.X + drawData.DisplaySize.X;
		var t = drawData.DisplayPos.Y;
		var b = drawData.DisplayPos.Y + drawData.DisplaySize.Y;
		Rlgl.Ortho(l, r, b, t, -1.0f, 1.0f);
		Rlgl.MatrixMode(MatrixMode.ModelView);
		Rlgl.LoadIdentity();
	}
	private void RestoreProjection()
	{
		Rlgl.MatrixMode(MatrixMode.ModelView);
		Rlgl.PopMatrix();
		Rlgl.MatrixMode(MatrixMode.Projection);
		Rlgl.PopMatrix();
		Rlgl.MatrixMode(MatrixMode.ModelView);
	}
	private void DrawTriangles(RenderCommand cmd)
	{
		Rlgl.Begin(DrawMode.Triangles);
		Rlgl.SetTexture((uint)cmd.Texture);
		for (var i = 0; i < cmd.IndexCount; i++)
		{
			int idx = _indices[cmd.IndexOffset + i];
			var vertex = _vertices[idx];
			Rlgl.Color4ub(vertex.Color.R, vertex.Color.G, vertex.Color.B, vertex.Color.A);
			Rlgl.TexCoord2f(vertex.UV.X, vertex.UV.Y);
			Rlgl.Vertex2f(vertex.Position.X, vertex.Position.Y);
		}
		Rlgl.SetTexture(0);
		Rlgl.End();
	}
	
	private void InitRender()
	{
		_io.DisplaySize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
		_io.DisplayFramebufferScale = Vector2.One;
		CreateFontTexture();
	}
	
	private unsafe void CreateFontTexture()
	{
		_io.Fonts.AddFontDefault();
		_io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out var width, out var height, out _);
		var id = Rlgl.LoadTexture(pixels, width, height, PixelFormat.UncompressedR8G8B8A8, 1);
		Rlgl.TextureParameters(id,Rlgl.TEXTURE_MIN_FILTER, Rlgl.TEXTURE_FILTER_LINEAR);
		Rlgl.TextureParameters(id,Rlgl.TEXTURE_MAG_FILTER, Rlgl.TEXTURE_FILTER_LINEAR);
		
		Rlgl.TextureParameters(id, Rlgl.TEXTURE_WRAP_S, Rlgl.TEXTURE_WRAP_CLAMP);
		Rlgl.TextureParameters(id, Rlgl.TEXTURE_WRAP_T, Rlgl.TEXTURE_WRAP_CLAMP);
		_io.Fonts.SetTexID((nint)id);
		_io.Fonts.ClearTexData();
	}
}