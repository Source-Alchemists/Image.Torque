namespace ImageTorque.Processing;

/// <summary>
/// Represents the processor parameters.
/// </summary>
public abstract record ProcessorParameters
{
    /// <summary>
    /// Gets or sets the parallel options.
    /// </summary>
    public virtual ParallelOptions ParallelOptions { get; init; } = new ParallelOptions { MaxDegreeOfParallelism = 1 };
}
