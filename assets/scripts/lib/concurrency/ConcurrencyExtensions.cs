using System;
using System.Threading.Tasks;
using Godot;

namespace Hjam.assets.scripts.lib.concurrency;

public static class ConcurrencyExtensions
{
    
    public static async Task Await(this Node node, float seconds)
    {
        var timer = node.GetTree().CreateTimer(seconds);
        await node.ToSignal(timer, Timer.SignalName.Timeout);
        
        timer.Dispose();
    }
    
    public static async Task Every(this Node node, float seconds, Func<Task<bool>> action)
    {
        while (true)
        {
            await node.Await(seconds);
            
            var shouldStop = await action();
            if (shouldStop) break;
        }
    }

    public static async Task RunAtMost(this Node node, float seconds, int times, Func<int, Task<bool>> action)
    {
        var count = 0;
        await node.Every(seconds, async () =>
        {
            var shouldStop = await action(count);
            
            count++;
            return shouldStop || count >= times;
        });
    }
    
}