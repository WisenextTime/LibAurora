using ImGuiNET;
using LibAurora.Framework;
using Raylib_cs;
namespace LibAurora.Ui;

public partial class GuiService : IInitializable, IDisposable
{
	public static GuiService Instance => _instance ?? throw new InvalidOperationException("Gui service is not initialized.");
	public GuiService()
	{
		if(_instance != null) throw new InvalidOperationException("Gui service already been created");
		_instance = this;
	}
	private static GuiService? _instance;
	private nint _imguiContext;
	private ImGuiIOPtr _io;
	public void Initialize()
	{
		_imguiContext = ImGui.CreateContext();
		ImGui.SetCurrentContext(_imguiContext);
		_io = ImGui.GetIO();
		InitRender();
		InitInput();
	}
	
	public void Dispose()
	{
		Rlgl.UnloadTexture((uint)_io.Fonts.TexID);
		_instance = null;
	}
}