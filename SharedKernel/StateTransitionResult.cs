namespace SharedKernel;

/// <summary>
/// Result of a state transition - either success with new state or failure with reasons
/// </summary>
/// <typeparam name="TEntity">The entity type being transitioned</typeparam>
public abstract record StateTransitionResult<TEntity>
{
    private StateTransitionResult() { }

    public sealed record Success(TEntity Entity) : StateTransitionResult<TEntity>;
    
    public sealed record Failure(IEnumerable<string> Reasons) : StateTransitionResult<TEntity>
    {
        public Failure(string reason) : this(new[] { reason }) { }
    }

    public bool IsSuccess => this is Success;
    public bool IsFailure => this is Failure;

    public TEntity? GetEntityOrDefault() => this is Success s ? s.Entity : default;
    public IEnumerable<string> GetReasonsOrEmpty() => this is Failure f ? f.Reasons : Enumerable.Empty<string>();

    /// <summary>
    /// Match pattern for handling success/failure cases
    /// </summary>
    public TResult Match<TResult>(
        Func<TEntity, TResult> onSuccess,
        Func<IEnumerable<string>, TResult> onFailure)
    {
        return this switch
        {
            Success s => onSuccess(s.Entity),
            Failure f => onFailure(f.Reasons),
            _ => throw new InvalidOperationException("Unknown state transition result")
        };
    }
}

