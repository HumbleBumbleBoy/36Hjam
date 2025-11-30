using Godot;

namespace Hjam.assets.scripts.generic.spawn;

public partial class SpawnPoint : Node2D
{
    public override void _Ready()
    {
        var spaceState = GetWorld2D().DirectSpaceState;
        var result = spaceState.IntersectPoint(new PhysicsPointQueryParameters2D
        {
            Position = GlobalPosition,
            CollisionMask = uint.MaxValue,
            CollideWithBodies = true
        });
        
        if (result.Count > 0)
        {
            Hide();
        }
    }
}