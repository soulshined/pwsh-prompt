namespace PwshPrompt.IO;

internal sealed class Buffer : IBuffer
{
	public bool TreatControlCAsInput
	{
		get => Console.TreatControlCAsInput;
		set => Console.TreatControlCAsInput = value;
	}
	public bool KeyAvailable => Console.KeyAvailable;
	public ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);
	public void Write(string text) => Console.Write(text);
	public void WriteLine() => Console.WriteLine();
}
