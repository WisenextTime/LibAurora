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
				UV = new Vector2(vert.uv.X, vert.uv.Y),
				Color = new Color(
					(byte)(col & 0xFF),
					(byte)((col >> 8) & 0xFF),
					(byte)((col >> 16) & 0xFF),
					(byte)(col >> 24)
				),
			};
		}
	}
        
	private struct RenderCommand
	{
		public Texture2D Texture;
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
		ExecuteRenderCommands();
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
				Texture = cmd.GetTexID() == (IntPtr)_fontTexture.Id ? _fontTexture : default,
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
	private void ExecuteRenderCommands()
	{
		Rlgl.DisableBackfaceCulling();
		Rlgl.SetBlendMode(BlendMode.Alpha);
		Rlgl.EnableColorBlend();
		Rlgl.EnableScissorTest();
		var prevMatrix = Rlgl.GetMatrixModelview();
		try
		{
			SetupOrthographicProjection();
			foreach (var cmd in _commands)
			{
				Raylib.BeginScissorMode(
					(int)cmd.ClipRect.X,
					(int)cmd.ClipRect.Y,
					(int)cmd.ClipRect.Width,
					(int)cmd.ClipRect.Height
				);
				Rlgl.SetTexture(cmd.Texture.Id != 0 ? cmd.Texture.Id : 1);
				DrawTriangles(cmd);
				Rlgl.SetTexture(0);
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
	private void SetupOrthographicProjection()
	{
		Rlgl.MatrixMode(MatrixMode.Projection);
		Rlgl.PushMatrix();
		Rlgl.LoadIdentity();
		Rlgl.Ortho(0, _io.DisplaySize.X, _io.DisplaySize.Y, 0, -1, 1);
		Rlgl.MatrixMode(MatrixMode.ModelView);
		Rlgl.PushMatrix();
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
    
		for (var i = 0; i < cmd.IndexCount; i++)
		{
			int idx = _indices[cmd.IndexOffset + i];
			var vertex = _vertices[idx];
			Rlgl.Color4ub(vertex.Color.R, vertex.Color.G, vertex.Color.B, vertex.Color.A);
			Rlgl.TexCoord2f(vertex.UV.X, vertex.UV.Y);
			Rlgl.Vertex2f(vertex.Position.X, vertex.Position.Y);
		}
		Rlgl.End();
	}
}