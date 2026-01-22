namespace SharedKernel;

/// <summary>
/// Base class for domain operations following the lab pattern
/// Pure transformations - SYNC
/// </summary>
/// <typeparam name="TEntity">The entity type being operated on</typeparam>
/// <typeparam name="TState">The state/dependency type for the operation</typeparam>
/// <typeparam name="TResult">The result type of the operation</typeparam>
public abstract class DomainOperation<TEntity, TState, TResult>
    where TEntity : notnull
    where TState : class
{
    /// <summary>
    /// Transforms the entity using the provided state (SYNC - pure transformation)
    /// </summary>
    public abstract TResult Transform(TEntity entity, TState? state);
}
