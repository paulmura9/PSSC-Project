namespace SharedKernel;

/// <summary>
/// Helper class for defining and validating state transitions
/// Provides a fluent API for configuring allowed transitions
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

    /// <summary>
    /// Get all allowed transitions from a given state
    /// </summary>
    public IEnumerable<TState> GetAllowedTransitions(TState from)
    {
        return _allowedTransitions.TryGetValue(from, out var allowed)
            ? allowed
            : Enumerable.Empty<TState>();
    }

    /// <summary>
    /// Validate and attempt a transition
    /// </summary>
    public StateTransitionResult<TEntity> TryTransition<TEntity>(
        TEntity entity,
        TState currentState,
        TState targetState,
        Func<TEntity, TEntity> applyTransition)
    {
        if (!IsAllowed(currentState, targetState))
        {
            return new StateTransitionResult<TEntity>.Failure(
                $"Invalid transition: cannot go from '{currentState}' to '{targetState}'");
        }

        try
        {
            var newEntity = applyTransition(entity);
            return new StateTransitionResult<TEntity>.Success(newEntity);
        }
        catch (Exception ex)
        {
            return new StateTransitionResult<TEntity>.Failure(
                $"Transition failed: {ex.Message}");
        }
    }
}

