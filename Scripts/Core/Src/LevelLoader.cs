using Godot;

namespace EIODE.Scripts.Core
{
    public partial class LevelLoader : Node
    {
        public static LevelLoader Instance { get; private set; }
        public Node CurrentLevel { get; set; } = null;
        public override void _EnterTree()
        {
            Instance = this;
        }
        public static PackedScene LoadLevel(string path)
        {
            try
            {
                GD.Print($"Loading level {path}");
                return ResourceLoader.Load<PackedScene>(path);
            }
            catch (System.Exception e)
            {
                GD.PushError($"Error while loading level {e}");
                throw;
            }
        }

        public void ChangeLevel(PackedScene newLevel, bool freeCurrentLevel = true, bool movePlayer = true)
        {
            GD.Print($"Changing Level to {newLevel}");
            CallDeferred(MethodName.DeferredChangeLevel, newLevel, freeCurrentLevel, movePlayer);
        }
        private void DeferredChangeLevel(PackedScene newLevel, bool freeCurrentLevel = true, bool movePlayer = true)
        {
            // TODO: Levels in the future should have their own class
            if (freeCurrentLevel) CurrentLevel.QueueFree();
            CurrentLevel = newLevel.Instantiate();
            GetTree().Root.AddChild(CurrentLevel);
            GetTree().CurrentScene = CurrentLevel;

            if (movePlayer)
            {
                var player = GetNode<Game>(Game.Location).Player;
                player.Lock();
                player.Reparent(CurrentLevel);
                // TODO: Also move player global position to the level's starting position, it should be a public Vector3 in the level class
                player.Unlock();
            }
        }
    }
}
