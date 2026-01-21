namespace SharedKernel.StateMachine;

/// <summary>
/// Helper class for defining and validating state transitions
/// Provides a fluent API for configuring allowed transitions
/// Used by Order, Shipment, Invoice bounded contexts
/// </summary>
/// <typeparam name="TState">The state type (usually an enum)</typeparam>
public class StateTransitionMap<TState> where TState : notnull
{
    private readonly Dictionary<TState, HashSet<TState>> _allowedTransitions = new();

    /// <summary>
    /// Allow a transition from one state to another
    /// </summary>
    public StateTransitionMap<TState> Allow(TState from, TState to)
    {
        if (!_allowedTransitions.ContainsKey(from))
            _allowedTransitions[from] = new HashSet<TState>();

        _allowedTransitions[from].Add(to);
        return this;
    }

    /// <summary>
    /// Allow transitions from one state to multiple target states
    /// </summary>
    public StateTransitionMap<TState> Allow(TState from, params TState[] toStates)
    {
        foreach (var to in toStates)
            Allow(from, to);
        return this;
    }

    /// <summary>
    /// Check if a transition is allowed
    /// </summary>
    public bool IsAllowed(TState from, TState to)
    {
        return _allowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }
}

