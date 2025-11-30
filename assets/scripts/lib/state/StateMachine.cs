using Godot;

namespace Hjam.assets.scripts.lib.state;

public partial class StateMachine<TValue>(TValue context) : Node
{
    public State<TValue>? CurrentState { get; private set; }
    public TValue GetContext() => context;

    public bool Enabled = true;
    
    private State<TValue>? _pendingState;

    public void ChangeState(State<TValue>? newState)
    {
        while (true)
        {
            if (CurrentState is not null)
            {
                CurrentState.OnExit(GetContext(), newState);
                CurrentState.RequestStateChange -= _HandleStateChangeRequest;

                if (_pendingState is not null)
                {
                    var pendingState = _pendingState;
                    _pendingState = null;

                    newState = pendingState;
                    continue;
                }
            }

            var previousState = CurrentState;
            CurrentState = newState;

            if (CurrentState is null) return;

            CurrentState.RequestStateChange += _HandleStateChangeRequest;
            CurrentState.OnEnter(GetContext(), previousState);
            break;
        }
    }

    public void Update(double delta)
    {
        if (!Enabled)
        {
            return;
        }
        
        CurrentState?.OnUpdate(GetContext(), delta);
    }
    
    public void FixedUpdate(double delta)
    {
        if (!Enabled)
        {
            return;
        }
        
        CurrentState?.OnFixedUpdate(GetContext(), delta);
    }
    
    public void EmitSignal(string signalName, params object?[] args)
    {
        if (!Enabled)
        {
            return;
        }
        
        CurrentState?.OnSignal(GetContext(), signalName, args);
    }

    private void _HandleStateChangeRequest(State<TValue>? newState)
    {
        if (_pendingState is null && CurrentState is not null)
        {
            _pendingState = newState;
            return;
        }
        
        ChangeState(newState);
    }
    
}