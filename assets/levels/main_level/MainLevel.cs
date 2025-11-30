using Godot;
using Hjam.assets.levels.main_level.state;
using Hjam.assets.scripts.lib.state;

namespace Hjam.assets.levels.main_level;

public partial class MainLevel : Node2D
{
    
    public StateMachine<Node2D> StateMachine { get; private set; } = null!;

    public override void _Ready()
    {
        StateMachine = new StateMachine<Node2D>(this);
        AddChild(StateMachine);
        
        StateMachine.ChangeState(new MainLevelWaitingState(0));
    }

    public override void _Process(double delta)
    {
        StateMachine.Update(delta);
    }
    
    public override void _PhysicsProcess(double delta)
    {
        StateMachine.FixedUpdate(delta);
    }
}