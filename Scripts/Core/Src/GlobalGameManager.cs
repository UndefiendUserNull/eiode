using EIODE.Scenes.Player;
using EIODE.Utils;
using Godot;

namespace EIODE.Scripts.Core;
public partial class GlobalGameManager : Node
{
    public PlayerMovement Player { get; private set; }
    public static GlobalGameManager Instance { get; private set; }
    public void Initialize()
    {
        Setup();
    }
    private void Setup()
    {
        if (Instance == null) Instance = this;
        else
        {
            Instance.QueueFree();
            Instance = this;
        }
    }
    public void SetPlayer(PlayerMovement newPlayer)
    {
        Player = newPlayer;
    }


    public GlobalGameManager() { }
}
