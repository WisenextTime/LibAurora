using LibAurora.Graphics;
using LibAurora.Input;
using Veldrid;
namespace LibAurora.UI;

public class UIScene(IGraphics graphics, IInput input)
{
	public UIElement? Root { get; set; }
	public void Update()
	{

	}
	public void Render()
	{
		Root?.Render(graphics.CommandList);
	}
}
public class UIElement
{
	public virtual void Render(CommandList cl)
	{

	}
}