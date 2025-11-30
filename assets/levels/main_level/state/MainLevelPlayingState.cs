using System.Threading.Tasks;
using Godot;
using Hjam.assets.scripts.lib.state;

namespace Hjam.assets.levels.main_level.state;

public class MainLevelPlayingState(int stage) : State<Node2D>
{
    public override async Task OnEnter(Node2D context, State<Node2D>? previousState)
    {
        GD.Print("Entered Playing State");
        await Task.CompletedTask;
    }
    
    
}