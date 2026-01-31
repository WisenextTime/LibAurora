using ImGuiNET;
using LibAurora.Framework;
using Raylib_cs;
namespace LibAurora.Ui;

public partial class GuiService : IUpdatable, IInitializable, IDisposable
{
	public static GuiService Instance => _instance ?? throw new InvalidOperationException("Gui service is not initialized.");
	public GuiService()
	{
		if(_instance != null) throw new InvalidOperationException("Gui service already been created");
		_instance = this;
	}
	private static GuiService? _instance;
	private nint _imguiContext;
	private Texture2D _fontTexture;
	private ImGuiIOPtr _io;
	public void Initialize()
	{
		_imguiContext = ImGui.CreateContext();
		ImGui.SetCurrentContext(_imguiContext);
		_io = ImGui.GetIO();
		_io.ConfigFlags |= ImGuiConfigFlags.DockingEnable |
		                   ImGuiConfigFlags.NavEnableKeyboard |
		                   ImGuiConfigFlags.NavEnableGamepad;
		CreateFontTexture();
	}

	private unsafe void CreateFontTexture()
	{
		
		_io.Fonts.AddFontDefault();
		_io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out var width, out var height, out _);
		var image = new Image
		{
			Data = pixels,
			Width = width,
			Height = height,
			Format = PixelFormat.UncompressedR8G8B8A8,
			Mipmaps = 1,
		};
		_fontTexture = Raylib.LoadTextureFromImage(image);
		_io.Fonts.SetTexID((IntPtr)_fontTexture.Id);
		_io.Fonts.ClearTexData();
	}
	
	public void Update(double delta)
	{
		
	}
	public void Dispose()
	{
		_instance = null;
	}
}