using Godot;

namespace EIODE.Resources.Src;

[GlobalClass]
public partial class DevConsoleSettings : Resource
{
    [Export] public int FontSize = 14;
    [Export] public int OutlineSize = 8;
    [Export] public Color OutlineColor = Color.Color8(0, 0, 0);

    public DevConsoleSettings() { }
}
