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
            if (!_isMouseShowed)
                Input.MouseMode = Input.MouseModeEnum.Captured;
            else
                Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }
}
