using System;
using Godot;
using Hjam.assets.entities.enemy;
using Hjam.assets.entities.player;

namespace Hjam.assets.entities.shooting_enemy;

public partial class ShootingEnemy : Enemy
{
    [Export] public float StrafeSpeed = 80f;
    [Export] public float RetreatSpeed = 120f;

    private bool _strafeDirectionRight = true;

    public override void HandleMinimumDistanceReached(Player player)
    {
        var toPlayer = player.GlobalPosition - GlobalPosition;
        var dist = toPlayer.Length();

        Vector2 moveDir;
        
        var strafeDir = toPlayer.Rotated(_strafeDirectionRight ? MathF.PI / 2 : -MathF.PI / 2).Normalized();
        if (dist < MinimumDistanceToPlayer * 0.95f)
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
    
    private bool IsAboutToCollide(Vector2 direction)
    {
        var space = GetWorld2D().DirectSpaceState;
        var result = space.IntersectRay(new PhysicsRayQueryParameters2D
        {
            From = GlobalPosition,
            To = GlobalPosition + direction.Normalized() * 16f,
            CollideWithAreas = false,
            CollideWithBodies = true
        });
        return result.Count > 0;
    }
}