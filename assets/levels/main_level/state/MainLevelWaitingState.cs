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
    
    private readonly (int x, int y, int width, int height) _worldBounds = (x: 0, y: 0, width: 1920, height: 1080);
    private readonly (int x, int y) _worldPadding = (x: 10, y: 10);
    
    private (int x, int y, int width, int height) WorldBounds =>
        (x: _worldBounds.x + _worldPadding.x, 
            y: _worldBounds.y + _worldPadding.y,
            width: _worldBounds.width - _worldPadding.x * 2,
            height: _worldBounds.height - _worldPadding.y * 2);
    private Rect2 WorldRect => new(WorldBounds.x, WorldBounds.y, WorldBounds.width, WorldBounds.height);
    
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
        
        var navigationPolygon = GenerateNavigationMesh(context);
        var navigationRegion = context.GetTree().GetFirstNodeInGroup("navigation_region");
        if (navigationRegion is NavigationRegion2D navRegion)
        {
            navRegion.NavigationPolygon = navigationPolygon;
        }
        
        GenerateWorldWalls(context);

        _ = ShowLevelPlacement(context, 4);
        
        spawnPoints.GenerateSpawnPoints(
            WorldBounds,
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

    private NavigationPolygon GenerateNavigationMesh(Node2D context)
    {
        var navigationPolygon = new NavigationPolygon();
        
        var allVertices = new Vector2[]
        {
            new(WorldBounds.x, WorldBounds.y),
            new(WorldBounds.x + WorldBounds.width, WorldBounds.y),
            new(WorldBounds.x + WorldBounds.width, WorldBounds.y + WorldBounds.height),
            new(WorldBounds.x, WorldBounds.y + WorldBounds.height)
        };

        navigationPolygon.AddOutline(allVertices);
        
        var geometryData = new NavigationMeshSourceGeometryData2D();
        
        NavigationServer2D.ParseSourceGeometryData(navigationPolygon, geometryData, context);
        NavigationServer2D.BakeFromSourceGeometryData(navigationPolygon, geometryData);
        
        return navigationPolygon;
    }

    private void GenerateLevel(Node2D context, SpawnPoints spawnPointContainer)
    {
        var random = new Random();
        
        var blockCount = 6;

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
                (float)(random.NextDouble() * WorldBounds.width) + WorldBounds.x,
                (float)(random.NextDouble() * WorldBounds.height) + WorldBounds.y
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
            block.AddToGroup("level_obstacles");
            
            // check the collision shape is fully within the world bounds
            var blockRect = new Rect2(
                randomPosition - randomSize / 2,
                randomSize
            );
            if (!WorldRect.Encloses(blockRect))
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
    
    private void GenerateWorldWalls(Node2D context)
    {
        const float wallThickness = 50f;

        var walls = new List<StaticBody2D>();

        // Top wall
        walls.Add(CreateWall(
            position: new Vector2(WorldBounds.x + WorldBounds.width / 2f, WorldBounds.y - wallThickness / 2),
            size: new Vector2(WorldBounds.width, wallThickness)
        ));
        
        // Bottom wall
        walls.Add(CreateWall(
            position: new Vector2(WorldBounds.x + WorldBounds.width / 2f, WorldBounds.y + WorldBounds.height + wallThickness / 2),
            size: new Vector2(WorldBounds.width, wallThickness)
        ));
        
        // Left wall
        walls.Add(CreateWall(
            position: new Vector2(WorldBounds.x - wallThickness / 2, WorldBounds.y + WorldBounds.height / 2f),
            size: new Vector2(wallThickness, WorldBounds.height)
        ));
        
        // Right wall
        walls.Add(CreateWall(
            position: new Vector2(WorldBounds.x + WorldBounds.width + wallThickness / 2, WorldBounds.y + WorldBounds.height / 2f),
            size: new Vector2(wallThickness, WorldBounds.height)
        ));

        foreach (var wall in walls)
        {
            context.AddChild(wall);
        }
    }
    
    private static StaticBody2D CreateWall(Vector2 position, Vector2 size)
    {
        var wall = new StaticBody2D
        {
            Position = position
        };

        var collisionShape = new CollisionShape2D
        {
            Shape = new RectangleShape2D
            {
                Size = size
            }
        };
        
        wall.AddChild(collisionShape);
        
        return wall;
    }
}