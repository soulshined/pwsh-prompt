using PwshPrompt.IO;

namespace PwshPrompt.Tests.Helpers;

internal sealed class TestBuffer : IBuffer
{
	private readonly Queue<ConsoleKeyInfo> _keys;
	private readonly List<string> _written = new();

	public bool TreatControlCAsInput { get; set; }
	public IReadOnlyList<string> Written => _written;
	public string AllWritten => string.Concat(_written);

	public TestBuffer(params ConsoleKeyInfo[] keys)
	{
		_keys = new Queue<ConsoleKeyInfo>(keys);
	}

	public bool KeyAvailable => _keys.Count > 0;
	public ConsoleKeyInfo ReadKey(bool intercept) => _keys.Dequeue();
	public void Write(string text) => _written.Add(text);
	public void WriteLine() => _written.Add("\n");

	internal static ConsoleKeyInfo Key(ConsoleKey key, char ch = '\0', ConsoleModifiers mod = 0)
		=> new(ch, key, mod.HasFlag(ConsoleModifiers.Shift),
			mod.HasFlag(ConsoleModifiers.Alt), mod.HasFlag(ConsoleModifiers.Control));
}
