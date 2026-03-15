namespace PwshPrompt.Types;

internal readonly record struct LoopSignal<T>(T? Value, bool ShouldReturn = false)
{
	internal static readonly LoopSignal<T> Continue = new();
	internal static LoopSignal<T> Return(T? value) => new(value, true);
}
