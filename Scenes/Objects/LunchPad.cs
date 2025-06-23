using Godot;

namespace EIODE.Scenes.Objects;

public partial class LunchPad : Area3D
{
    /// <summary>
    /// Lunch power should be equal or less than PlayerMovementSettings.MaxLunchPadForce
    /// </summary>
    [Export] public float LunchPower { get; set; } = 15f;

    public override void _Ready()
    {
        BodyEntered += LunchPad_BodyEntered;
    }

    public override void _ExitTree()
    {
        BodyEntered -= LunchPad_BodyEntered;
    }

    private void LunchPad_BodyEntered(Node3D body)
    {
        if (body != null && body is Player.Player player)
        {
            player.AddLunchForce(LunchPower);
        }
    }
}