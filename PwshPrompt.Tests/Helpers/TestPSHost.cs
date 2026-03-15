using System.Globalization;
using System.Management.Automation.Host;

namespace PwshPrompt.Tests.Helpers;

internal sealed class TestPSHost : PSHost
{
	private readonly TestPSHostUI _ui;

	public TestPSHost(IEnumerable<string?> inputs)
	{
		_ui = new TestPSHostUI(inputs);
	}

	public TestPSHostUI TestUI => _ui;

	public override PSHostUserInterface UI => _ui;
	public override CultureInfo CurrentCulture { get; } = CultureInfo.InvariantCulture;
	public override CultureInfo CurrentUICulture { get; } = CultureInfo.InvariantCulture;
	public override Guid InstanceId { get; } = Guid.NewGuid();
	public override string Name => "TestHost";
	public override Version Version { get; } = new Version(1, 0);

	public override void EnterNestedPrompt() => throw new NotSupportedException();
	public override void ExitNestedPrompt() => throw new NotSupportedException();
	public override void NotifyBeginApplication() { }
	public override void NotifyEndApplication() { }
	public override void SetShouldExit(int exitCode) { }
}
