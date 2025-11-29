using System.Threading.Tasks;

namespace Hjam.assets.scripts.lib.state;

public abstract class State<TValue>
{
    public delegate void RequestStateChangeDelegate(State<TValue>? newState);
    
    public event RequestStateChangeDelegate? RequestStateChange;
    
    protected void ChangeState(State<TValue>? newState)
    {
        RequestStateChange?.Invoke(newState);
    }

    public virtual Task OnEnter(TValue context, State<TValue>? previousState) => Task.CompletedTask;
    public virtual Task OnExit(TValue context, State<TValue>? nextState) => Task.CompletedTask;
    public virtual Task OnUpdate(TValue context, double deltaTime) => Task.CompletedTask;
    public virtual Task OnFixedUpdate(TValue context, double deltaTime) => Task.CompletedTask;
    
}