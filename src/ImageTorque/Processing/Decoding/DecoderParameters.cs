namespace ImageTorque.Processing;

public record DecoderParameters : ProcessorParameters
{
    public Stream? Input { get; set; }

    public Type OutputType { get; init; } = null!;
}
