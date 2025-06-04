using Godot;

namespace EIODE.Scripts.Core.Level;
public partial class LocationObjective : Objective
{
    [Export] public float Radius { get; private set; }
    private Area3D _area;
    public override void Activate()
    {
        _area = new Area3D();

    }

    public override void Deactivate()
    {
        throw new System.NotImplementedException();
    }
}
