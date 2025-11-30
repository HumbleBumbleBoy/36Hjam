using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Hjam.assets.scripts.generic.spawn;

namespace Hjam.assets.scripts.generic;

public partial class SpawnPoints : Node
{
    
    private readonly Random _random = new();
    
    public void GenerateSpawnPoints((int x, int y, int width, int height) area, int numberOfPoints, float minDistanceBetweenPoints = 0f)
    {
        var points = new List<Vector2>();
        var existingPoints = GetChildren().Where(child => child is Node2D).Cast<Node2D>().Select(node => node.Position).ToList();
        points.AddRange(existingPoints);
        
        var attempts = 0;
        var maxAttempts = numberOfPoints * 10;
        while (points.Count < numberOfPoints && attempts < maxAttempts)
        {
            var randomPoint = new Vector2(
                (float)(_random.NextDouble() * area.width) + area.x,
                (float)(_random.NextDouble() * area.height) + area.y
            );
            
            if (points.All(existingPoint => existingPoint.DistanceTo(randomPoint) >= minDistanceBetweenPoints))
            {
                points.Add(randomPoint);
                
                CreateSpawnPoint(randomPoint);
            }
            
            attempts++;
        }
    }
    
    public SpawnPoint CreateSpawnPoint(Vector2 position)
    {
        var spawnPoint = new SpawnPoint
        {
            Position = position
        };
        AddChild(spawnPoint);
        
        return spawnPoint;
    }
    
    public Node2D? GetRandomSpawnPoint()
    {
        var spawnPoints = GetChildren().Where(child => child is Node2D { Visible: true }).ToList();
        if (spawnPoints.Count == 0)
            return null;
        
        var randomIndex = _random.Next(0, spawnPoints.Count);
        return spawnPoints[randomIndex] as Node2D ?? null!;
    }
    
    public Node2D? GetFarRandomSpawnPoint(List<Vector2> centers, float minDistance)
    {
        var spawnPoints = GetChildren().Where(child => child is Node2D { Visible: true }).Cast<Node2D>().ToList();
        var validSpawnPoints = spawnPoints.Where(sp =>
            centers.All(center => sp.GlobalPosition.DistanceTo(center) >= minDistance)
        ).ToList();
        
        if (validSpawnPoints.Count == 0)
            return null;
        
        var randomIndex = _random.Next(0, validSpawnPoints.Count);
        return validSpawnPoints[randomIndex];
    }
    
    public Vector2 GetRandomSpawnPosition()
    {
        var spawnPoint = GetRandomSpawnPoint();
        return spawnPoint?.GlobalPosition ?? Vector2.Zero;
    }
    
}