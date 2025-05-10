using EIODE.Utils;
using Godot;

namespace EIODE.Scripts.Core;
public partial class GlobalGameManager : Node
{
    private bool _isMouseShowed = false;
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }
    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed(InputHash.K_ESC))
        {
            _isMouseShowed = !_isMouseShowed;
            Input.MouseMode = !_isMouseShowed ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
        }
        if (Input.IsActionJustPressed(InputHash.K_R)) { GetTree().ReloadCurrentScene(); }
    }
}
