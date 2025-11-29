using System.Threading.Tasks;
using Godot;
using Hjam.assets.scripts.lib.concurrency;
using Hjam.assets.scripts.lib.state;

namespace Hjam.assets.levels.main_level.state;

public class MainLevelWaitingState : State<Node>
{
    public override async Task OnEnter(Node context, State<Node>? previousState)
    {
        await context.RunAtMost(seconds: 1, times: 5, current =>
        {
            GD.Print($"Waiting... {current}");
            return Task.FromResult(false);
        });
    }
}