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
}