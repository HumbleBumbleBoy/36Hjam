using System.Threading.Tasks;
using Godot;
using Hjam.assets.entities.player;
using Hjam.assets.scripts.generic;
using Hjam.assets.scripts.lib.concurrency;
using Hjam.assets.scripts.lib.state;
using Hjam.assets.ui.components.overlay_text;

namespace Hjam.assets.levels.main_level.state;

public class MainLevelWaitingState : State<Node>
{
    public override async Task OnEnter(Node context, State<Node>? previousState)
    {
        var playerScene = GD.Load<PackedScene>("res://assets/entities/player/player_scene.tscn");
        var player = playerScene.Instantiate<Player>();
        context.AddChild(player);
        
        if (context.GetTree().GetFirstNodeInGroup("spawn_points") is SpawnPoints spawnPoints)
        {
            spawnPoints.IfEmptyGenerateSpawnPoints(
                (x: 0, y: 0, width: 1920, height: 1080),
                numberOfPoints: 36,
                minDistanceBetweenPoints: 100f
            );
            
            var spawnPoint = spawnPoints.GetRandomSpawnPosition();
            player.GlobalPosition = spawnPoint;
        }

        OverlayText.CreateInstance(context, "Get Ready!", reusable: true);
        await context.Delay(seconds: 2);
        
        await context.RunAtMostEvery(seconds: 1, times: 5, current =>
        {
            OverlayText.CreateInstance(context, (5 - current).ToString(), reusable: true);
            return Task.FromResult(false);
        });
        
        OverlayText.CreateInstance(context, "Fight!", reusable: true);
        await context.Delay(seconds: 1);
        
        OverlayText.DeleteInstance(context);
        
        ChangeState(new MainLevelPlayingState());
    }
}