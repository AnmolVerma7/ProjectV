/// <summary>
/// Implement this interface on any MonoBehaviour that needs to receive input.
/// <para>
/// The input handler will automatically find these and call <see cref="RegisterInputs"/>.
/// </para>
/// </summary>
public interface IInputUser
{
    /// <summary>
    /// Register your input bindings here using the provided builder.
    /// </summary>
    /// <param name="builder">The fluent builder for binding actions to methods.</param>
    void RegisterInputs(InputBuilder builder);
}
