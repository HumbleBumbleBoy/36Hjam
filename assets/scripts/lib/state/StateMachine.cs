using Godot;

namespace Hjam.assets.scripts.lib.state;

public partial class StateMachine<TValue>(TValue context) : Node
{
    public State<TValue>? CurrentState { get; private set; }
    public TValue GetContext() => context;

    public bool Enabled = true;
    
    private State<TValue>? _pendingState;
    private bool _transitioning;

    public void ChangeState(State<TValue>? newState)
    {
        _transitioning = true;
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
        
        _transitioning = false;
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
        if (_transitioning)
        {
            _pendingState = newState;
            return;
        }
        
        ChangeState(newState);
    }
    
}