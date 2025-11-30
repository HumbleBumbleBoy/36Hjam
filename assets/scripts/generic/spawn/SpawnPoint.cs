using Godot;

namespace Hjam.assets.scripts.generic.spawn;

public partial class SpawnPoint : Node2D
{
    
    private const bool DebugMode = true;

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

        if (!DebugMode) return;
        
        // Add a 20x20 color rect for debug
        var debugRect = new ColorRect
        {
            Color = new Color(1, 0, 0, 0.5f),
            Size = new Vector2(20, 20),
            Position = new Vector2(-10, -10)
        };
        AddChild(debugRect);
    }
}