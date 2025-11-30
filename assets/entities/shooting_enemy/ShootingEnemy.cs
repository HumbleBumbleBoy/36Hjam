using System;
using Godot;
using Hjam.assets.entities.bullet;
using Hjam.assets.entities.enemy;
using Hjam.assets.entities.player;

namespace Hjam.assets.entities.shooting_enemy;

public partial class ShootingEnemy : Enemy
{
    [Export] public float StrafeSpeed = 80f;
    [Export] public float RetreatSpeed = 120f;
    [Export] public float ShootingInterval = 1.5f;

    private bool _strafeDirectionRight = true;
    private float _timeSinceLastShot = 0f;

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        var pivot = GetNode<Node2D>("Pivot");
        // rotate around center of enemy as pivot to face player
        var player = (GetTree().GetFirstNodeInGroup("player") as Player)!;
        var toPlayer = player.GlobalPosition - GlobalPosition;
        pivot.Rotation = toPlayer.Angle();
        
        _timeSinceLastShot += (float) delta;
        if (_timeSinceLastShot >= ShootingInterval && HasLineOfSight(player.GlobalPosition))
        {
            ShootAtPlayer(toPlayer);
            _timeSinceLastShot = 0f;
        }
    }

    public override void HandleMinimumDistanceReached(Player player)
    {
        var toPlayer = player.GlobalPosition - GlobalPosition;
        var dist = toPlayer.Length();

        Vector2 moveDir;
        
        var strafeDir = toPlayer.Rotated(_strafeDirectionRight ? MathF.PI / 2 : -MathF.PI / 2).Normalized();
        if (dist < MinimumDistanceToPlayer * .6)
        {
            moveDir = -toPlayer.Normalized() * RetreatSpeed;
        }
        else 
        {
            moveDir = strafeDir * StrafeSpeed;

            if (IsAboutToCollide(moveDir))
            {
                _strafeDirectionRight = !_strafeDirectionRight;
            }
        }

        Velocity = moveDir;
        MoveAndSlide();
    }

    public override Vector2 ComputeMovementDirection(Player player, Vector2 desiredDirection)
    {
        var moveDir = desiredDirection + ComputeSeparation(GlobalPosition, MinimumDistanceToOthers);

        if (!IsBlockedByTeammate(player.GlobalPosition)) return moveDir.Normalized() * Speed;
        
        var toPlayer = player.GlobalPosition - GlobalPosition;
        var strafeDir = toPlayer.Rotated(!_strafeDirectionRight ? MathF.PI / 2 : -MathF.PI / 2).Normalized();
        moveDir += strafeDir * StrafeSpeed;

        return moveDir.Normalized() * Speed;
    }
    
    private void ShootAtPlayer(Vector2 toPlayer)
    {
        var bulletScene = GD.Load<PackedScene>("res://assets/entities/bullet/Bullet.tscn");
        var bullet = bulletScene.Instantiate<Bullet>();
        var pivot = GetNode<Node2D>("Pivot");
        var muzzle = pivot.GetNode<Node2D>("Muzzle");
        bullet.GlobalPosition = muzzle.GlobalPosition;
        bullet.Rotation = toPlayer.Angle();
        GetParent().AddChild(bullet);
    }
    
    private bool IsBlockedByTeammate(Vector2 targetPos)
    {
        var rayParams = new PhysicsRayQueryParameters2D
        {
            From = GlobalPosition,
            To = targetPos,
            CollideWithAreas = false,
            CollideWithBodies = true
        };

        var space = GetWorld2D().DirectSpaceState;
        var result = space.IntersectRay(rayParams);

        if (result.Count == 0)
        {
            return false;
        }

        return result.TryGetValue("collider", out var colliderObj) && ((Node) colliderObj).IsInGroup("enemy");
    }
    
    private bool HasLineOfSight(Vector2 targetPos)
    {
        var rayParams = new PhysicsRayQueryParameters2D
        {
            From = GlobalPosition,
            To = targetPos,
            CollideWithAreas = false,
            CollideWithBodies = true
        };

        var space = GetWorld2D().DirectSpaceState;
        var result = space.IntersectRay(rayParams);

        return result.TryGetValue("collider", out var colliderObj) &&
               ((Node) colliderObj).IsInGroup("player");
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
        return result.Count > 0;
    }
}