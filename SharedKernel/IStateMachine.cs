namespace SharedKernel;

/// <summary>
/// Base interface for entities that follow a state machine pattern
/// </summary>
/// <typeparam name="TState">The state type (usually an enum)</typeparam>
public interface IStateMachine<TState> where TState : notnull
{
    TState CurrentState { get; }
    bool CanTransitionTo(TState targetState);
}

