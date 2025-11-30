using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Hjam.assets.entities.player;
using Hjam.assets.scripts.generic;
using Hjam.assets.scripts.lib.concurrency;
using Hjam.assets.scripts.lib.state;
using Hjam.assets.ui.components.overlay_text;

namespace Hjam.assets.levels.main_level.state;

public class MainLevelWaitingState(int stage) : State<Node2D>
{
    public override async Task OnEnter(Node2D context, State<Node2D>? previousState)
    {
        var playerScene = GD.Load<PackedScene>("res://assets/entities/player/player_scene.tscn");
        var player = playerScene.Instantiate<Player>();
        context.AddChild(player);
        
        player.StateMachine.Enabled = false;

        var spawnPointContainer = context.GetTree().GetFirstNodeInGroup("spawn_points");
        if (spawnPointContainer is not SpawnPoints spawnPoints)
        {
            return;
        }
        
        GenerateLevel(context, spawnPoints);

        _ = ShowLevelPlacement(context, 4);
        
        spawnPoints.GenerateSpawnPoints(
            (x: 0, y: 0, width: 1920, height: 1080),
            numberOfPoints: 120,
            minDistanceBetweenPoints: 100f
        );
        
        var spawnPoint = spawnPoints.GetRandomSpawnPosition();
        player.GlobalPosition = spawnPoint;
            
        OverlayText.CreateInstance(context, "Get Ready!", reusable: true);
        await context.Delay(seconds: 1);
        
        await context.RunAtMostEvery(seconds: 1, times: 3, current =>
        {
            OverlayText.CreateInstance(context, (3 - current).ToString(), reusable: true);
            return Task.FromResult(false);
        });
        
        OverlayText.CreateInstance(context, "Fight!", reusable: true);
        await context.Delay(seconds: 1);
        
        OverlayText.DeleteInstance(context);
        
        player.StateMachine.Enabled = true;
        ChangeState(new MainLevelPlayingState(stage));
    }
    
    private void GenerateLevel(Node2D context, SpawnPoints spawnPointContainer)
    {
        var random = new Random();
        
        var worldSize = (x: 0, y: 0, width: 1920, height: 1080);
        
        var blockCount = 10;

        const float scaleFactor    = 1.5f;
        const float maxBlockWidth  = 200f * scaleFactor;
        const float maxBlockHeight =  50f * scaleFactor;
        const float minBlockWidth  =  50f * scaleFactor;
        const float minBlockHeight =  50f * scaleFactor;
        
        const float separationDistance = 350f;

        var blockPositions = new List<Vector2>();
        var tries = 0;
        var maxTries = blockCount * 10;
        while (tries < maxTries)
        {
            tries++;
            
            var randomPosition = new Vector2(
                (float)(random.NextDouble() * worldSize.width) + worldSize.x,
                (float)(random.NextDouble() * worldSize.height) + worldSize.y
            );
            
            Vector2? closestPosition = blockPositions.Count == 0 ? null : blockPositions
                .Where(pos => pos != randomPosition)
                .MinBy(pos => pos.DistanceTo(randomPosition));
            if (closestPosition is not null && closestPosition.Value.DistanceTo(randomPosition) < separationDistance)
            {
                continue;
            }
            
            var randomSize = new Vector2(
                (float)(random.NextDouble() * maxBlockWidth) + minBlockWidth,
                (float)(random.NextDouble() * maxBlockHeight) + minBlockHeight
            );
            
            var randomRotation = (float)(random.NextDouble() * Math.PI * 2);

            var block = new StaticBody2D
            {
                Position = randomPosition,
                Rotation = randomRotation
            };

            var collisionShape = new CollisionShape2D
            {
                Shape = new RectangleShape2D
                {
                    Size = randomSize
                }
            };
            
            var colorRect = new ColorRect
            {
                Color = new Color(0.2f, 0.2f, 0.2f),
                Size = randomSize,
                Position = -randomSize / 2
            };
            
            block.AddChild(colorRect);
            block.AddChild(collisionShape);
            
            // check the collision shape is fully within the world bounds
            var blockRect = new Rect2(
                randomPosition - randomSize / 2,
                randomSize
            );
            if (!new Rect2(worldSize.x, worldSize.y, worldSize.width, worldSize.height).Encloses(blockRect))
            {
                continue;
            }
            
            // create spawn points on the corners
            var transform = block.Transform;
            var topLeft     = transform * new Vector2(-randomSize.X / 2, -randomSize.Y / 2);
            var topRight    = transform * new Vector2( randomSize.X / 2, -randomSize.Y / 2);
            var bottomLeft  = transform * new Vector2(-randomSize.X / 2,  randomSize.Y / 2);
            var bottomRight = transform * new Vector2( randomSize.X / 2,  randomSize.Y / 2);
            spawnPointContainer.CreateSpawnPoint(topLeft).Hide();
            spawnPointContainer.CreateSpawnPoint(topRight).Hide();
            spawnPointContainer.CreateSpawnPoint(bottomLeft).Hide();
            spawnPointContainer.CreateSpawnPoint(bottomRight).Hide();
            
            spawnPointContainer.CreateSpawnPoint(randomPosition).Hide();
            
            var spaceState = context.GetWorld2D().DirectSpaceState;
            var result = spaceState.IntersectShape(new PhysicsShapeQueryParameters2D
            {
                Shape = collisionShape.Shape,
                Transform = block.GetGlobalTransform(),
                CollisionMask = uint.MaxValue,
                CollideWithBodies = true
            });
            if (result.Count != 0)
            {
                continue;
            }
            
            blockPositions.Add(randomPosition);
            
            // Hide the block for later
            block.Hide();
            block.AddToGroup("level_obstacles");
            
            context.AddChild(block);
            blockCount--;
            if (blockCount <= 0)
            {
                break;
            }
        }
    }

    private async Task ShowLevelPlacement(Node context, float seconds)
    {
        var obstacles = context.GetTree()
            .GetNodesInGroup("level_obstacles")
            .Where(node => node is StaticBody2D)
            .Cast<StaticBody2D>()
            .ToList();
        
        if (obstacles.Count == 0) return;
        
        var n = obstacles.Count;
        var sumWeights = n * (n + 1) / 2f;
        for (var i = 0; i < n; i++)
        {
            var obstacle = obstacles[i];
            var weight = n - i;
            var delay = seconds * (weight / sumWeights);
            obstacle.Show();
            await context.Delay(delay);
        }
    }
}