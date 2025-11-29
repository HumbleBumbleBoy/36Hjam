using System.Threading.Tasks;
using Godot;
using Hjam.assets.scripts.lib.state;

namespace Hjam.assets.levels.main_level.state;

public class MainLevelPlayingState : State<Node>
{
    public override async Task OnEnter(Node context, State<Node>? previousState)
    {
        GD.Print("Entered Playing State");
        await Task.CompletedTask;
    }
}