using Godot;
using Hjam.assets.entities.player;

namespace Hjam.assets.entities.enemy;

public partial class Enemy : CharacterBody2D
{
    [Export] public float Speed = 100f;
    [Export] public float MinimumDistanceToPlayer = 150f;
    [Export] public float MinimumDistanceToOthers = 120f;
    
    private NavigationAgent2D _agent = null!;
    private Player _player = null!;
    
    private bool _isMoving = false;

    public override void _Ready()
    {
        _agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        _player = (GetTree().GetFirstNodeInGroup("player") as Player)!;
    }

    public override void _Process(double delta)
    {
        if (Velocity.Length() <= 0.02 && _isMoving)
        {
            _isMoving = false;
            OnIdle();
        }
        else if (Velocity.Length() >= 0.02 && !_isMoving)
        {
            _isMoving = true;
            OnMove();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _agent.TargetPosition = _player.GlobalPosition;

        var distanceToTarget = GlobalPosition.DistanceTo(_agent.TargetPosition);
        
        if (distanceToTarget < MinimumDistanceToPlayer)
        {
            HandleMinimumDistanceReached(_player);
            return;
        }
        
        var next = _agent.GetNextPathPosition();

        var dir = (next - GlobalPosition).Normalized();
        Velocity = ComputeMovementDirection(_player, dir);

        MoveAndSlide();
    }
    
    public virtual void OnIdle()
    {
        var sprite = GetNode<AnimatedSprite2D>("Sprite");
        sprite?.Play("idle");
    }
    
    public virtual void OnMove()
    {
        var sprite = GetNode<AnimatedSprite2D>("Sprite");
        sprite?.Play("run");
    }

    public virtual Vector2 ComputeMovementDirection(Player player, Vector2 desiredDirection)
    {
        return (desiredDirection + ComputeSeparation(GlobalPosition, MinimumDistanceToOthers)).Normalized() * Speed;
    }
    
    public virtual void HandleMinimumDistanceReached(Player player)
    {
        Velocity = Vector2.Zero;
        MoveAndSlide();
    }
    
    protected Vector2 ComputeSeparation(Vector2 currentPos, float minSeparation)
    {
        var separation = Vector2.Zero;

        foreach (var node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node == this) continue;
            if (node is not Enemy other) continue;
            
            var toOther = currentPos - other.GlobalPosition;
            var dist = toOther.Length();
            if (dist < minSeparation && dist > 0)
            {
                separation += toOther.Normalized() * ((minSeparation - dist) / minSeparation);
            }
        }

        return separation;
    }
}
