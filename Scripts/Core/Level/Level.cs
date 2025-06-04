using Godot;
using System.Collections.Generic;
using System.Linq;

namespace EIODE.Scripts.Core.Level;
[GlobalClass]
public partial class Level : Resource
{
    [Export] public string LevelName { get; set; }
    [Export] public PackedScene LevelScene { get; set; }
    [Export] public Vector3 LevelStartingPosition { get; set; }
    [Export] public Objective[] Objectives { get; set; }
    public Objective CurrentObjective { get; set; }

    public bool AllObjectivesCompleted
    {
        get
        {
            return Objectives.All(ob => (ob.IsCompleted && !ob.IsOptional) || (ob.IsCompleted && ob.IsOptional));
        }
    }

    public Level() { }
}
