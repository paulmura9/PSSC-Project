namespace SharedKernel.StateMachine;

/// <summary>
/// Base interface for entities that follow a state machine pattern
/// Used by Order, Shipment, Invoice bounded contexts
/// </summary>
/// <typeparam name="TState">The state type (usually an enum)</typeparam>
public interface IStateMachine<TState> where TState : notnull
{
    /// <summary>
    /// Gets the current state of the entity
    /// </summary>
    TState CurrentState { get; }

    /// <summary>
    /// Checks if a transition to the target state is allowed
    /// </summary>
    bool CanTransitionTo(TState targetState);
}

