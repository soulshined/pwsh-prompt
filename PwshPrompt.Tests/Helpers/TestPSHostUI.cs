using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;

namespace PwshPrompt.Tests.Helpers;

internal sealed class TestPSHostUI : PSHostUserInterface
{
	private Queue<string> _inputs;

	public List<string> WrittenLines { get; } = new();
	public List<string> WrittenText { get; } = new();
	public TestPSHostRawUI RawUIImpl { get; } = new();

	public override PSHostRawUserInterface RawUI => RawUIImpl;

	public TestPSHostUI(IEnumerable<string?> inputs)
	{
		_inputs = new Queue<string>(inputs.Select(s => s ?? ""));
	}

	public void ResetInputs(IEnumerable<string?> inputs)
	{
		_inputs = new Queue<string>(inputs.Select(s => s ?? ""));
		WrittenLines.Clear();
		WrittenText.Clear();
	}

	public override string ReadLine() => _inputs.Count > 0 ? _inputs.Dequeue() : "";

	public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value) => WrittenText.Add(value);
	public override void Write(string value) => WrittenText.Add(value);
	public override void WriteLine(string value) => WrittenLines.Add(value);
	public override void WriteLine() => WrittenLines.Add(string.Empty);
	public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value) => WrittenLines.Add(value);

	public override void WriteErrorLine(string value) { }
	public override void WriteDebugLine(string message) { }
	public override void WriteProgress(long sourceId, ProgressRecord record) { }
	public override void WriteVerboseLine(string message) { }
	public override void WriteWarningLine(string message) { }

	public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
		=> throw new NotSupportedException();
	public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
		=> throw new NotSupportedException();
	public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
		=> throw new NotSupportedException();
	public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
		=> throw new NotSupportedException();
	public override SecureString ReadLineAsSecureString()
		=> throw new NotSupportedException();
}
