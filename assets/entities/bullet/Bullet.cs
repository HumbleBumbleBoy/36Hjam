using Godot;
using Hjam.assets.entities.enemy;

namespace Hjam.assets.entities.bullet;

public partial class Bullet : Node2D
{
    
    [Export] public float Speed = 400f;
    [Export] public float Damage = 25f;
    
    public override void _Process(double delta)
    {
        var direction = Transform.X * Speed * (float)delta;
        if (IsAboutToCollide(direction))
        {
            QueueFree();
            return;
        }
        
        GlobalPosition += direction;
    }

    private bool IsAboutToCollide(Vector2 direction)
    {
        var space = GetWorld2D().DirectSpaceState;
        var result = space.IntersectRay(new PhysicsRayQueryParameters2D
        {
            From = GlobalPosition,
            To = GlobalPosition + direction.Normalized() * 32f,
            CollideWithAreas = false,
            CollideWithBodies = true
        });
        
        var collidedWith = result.TryGetValue("collider", out var colliderObj) ? (Node) colliderObj : null;
        if (collidedWith == null) return false;

        if (collidedWith is Enemy enemy)
        {
            enemy.TakeDamage(Damage);
        }
        
        return !collidedWith.IsInGroup("bullet");
    }
    
    
    
}