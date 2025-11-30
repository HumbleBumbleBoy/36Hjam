using System.Threading.Tasks;
using Godot;
using Hjam.assets.scripts.lib.state;

namespace Hjam.assets.entities.player.state;

public class PlayerRunState : State<Player>
{
    public override Task OnEnter(Player context, State<Player>? previousState)
    {
        var animatedSprite = context.GetNode<AnimatedSprite2D>("PlayerSprite");
        animatedSprite.Play("run");
        
        return Task.CompletedTask;
    }

    public override Task OnSignal(Player context, string signalName, params object?[] args)
    {
        switch (signalName)
        {
            case "stop_moving":
                ChangeState(new PlayerIdleState());
                break;
            case "move":
            {
                var movementVector = (Vector2) args[0]!;
                var speed = (int) args[1]!;
                context.Velocity = movementVector * speed;
                context.MoveAndSlide();
                break;
            }
        }

        return Task.CompletedTask;
    }
}