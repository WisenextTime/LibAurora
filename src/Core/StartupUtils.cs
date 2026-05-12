using LibAurora.Debug;
using LibAurora.Graphics.Fonts;
using LibAurora.Graphics.Myra;
using LibAurora.Resources;
namespace LibAurora.Core;

public static class StartupUtils
{
	public static void InitAllServices(ApplicationContext context, bool logEnabled = false)
	{
		AssetServer.Init(context);
		if (logEnabled) LogServer.Log("Assets server initialized.");
		if (context.Graphics is { } graphics)
		{
			GuiServer.Init(context.Input, graphics);
			if (logEnabled) LogServer.Log("Gui server initialized.");
			FontServer.Init(graphics);
			if (logEnabled) LogServer.Log("Font server initialized.");
		}
		else if (logEnabled)
		{
			LogServer.Log("No graphics device enabled, skipping init gui and font servers.");
		}
	}
}