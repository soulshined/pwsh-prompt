namespace PwshPrompt.IO;

internal interface IBuffer
{
	bool TreatControlCAsInput { get; set; }
	bool KeyAvailable { get; }
	ConsoleKeyInfo ReadKey(bool intercept);
	void Write(string text);
	void WriteLine();
}
