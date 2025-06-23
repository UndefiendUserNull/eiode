using Godot;
using System.Linq;

namespace EIODE.Scenes.Objects;

public partial class JumpPad : Area3D
{
    /// <summary>
    /// Lunch power should be equal or less than PlayerMovementSettings.MaxLunchPadForce
    /// </summary>
    [Export] public float JumpPower { get; set; } = 15f;

    public override void _Ready()
    {
        BodyEntered += JumpPad_BodyEntered;
    }

    public override void _ExitTree()
    {
        BodyEntered -= JumpPad_BodyEntered;
    }

    private void JumpPad_BodyEntered(Node3D body)
    {
        if (body != null && body is Player.Player player)
        {
            // If this lunch pad is not in the list PrevLunchPads, act like a normal lunch pad
            if (!player.PrevJumpPads.Any((x) => x == this))
            {
                player.PrevJumpPads.Add(this);
                player.AddJumpForce(JumpPower);
            }
            else
            {
                // Reset everything if this lunch pad was found
                player.JumpPadForce = 0f;
                player.ForceSetVelocity(Vector3.Up * JumpPower);
            }
        }
    }
}