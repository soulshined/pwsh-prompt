using System.Management.Automation;
using System.Management.Automation.Runspaces;
using PwshPrompt.Cmdlets;

namespace PwshPrompt.Tests.Helpers;

internal sealed class CmdletHarness : IDisposable
{
	private readonly Runspace _runspace;
	private bool _disposed;

	public TestPSHost Host { get; }

	public CmdletHarness(params string?[] inputs)
	{
		Host = new TestPSHost(inputs);
		InitialSessionState iss = InitialSessionState.CreateDefault2();
		iss.Commands.Add(new SessionStateCmdletEntry("Prompt-Input", typeof(PromptInputCmdlet), null));
		iss.Commands.Add(new SessionStateCmdletEntry("Prompt-Choice", typeof(PromptChoiceCmdlet), null));
		_runspace = RunspaceFactory.CreateRunspace(Host, iss);
		_runspace.Open();
	}

	public CmdletHarness WithInputs(params string?[] inputs)
	{
		Host.TestUI.ResetInputs(inputs);
		return this;
	}

	public CmdletResult Run(string script)
	{
		using PowerShell ps = PowerShell.Create();
		ps.Runspace = _runspace;
		ps.AddScript(script);

		List<PSObject> output = new();
		Exception? terminatingEx = null;

		try
		{
			output.AddRange(ps.Invoke());
		}
		catch (Exception ex)
		{
			terminatingEx = ex;
		}

		return new CmdletResult(output, ps.Streams.Error.ToList(), terminatingEx);
	}

	public CmdletResult Invoke(string cmdletName, Dictionary<string, object?> parameters)
	{
		using PowerShell ps = PowerShell.Create();
		ps.Runspace = _runspace;
		ps.AddCommand(cmdletName);

		foreach ((string key, object? value) in parameters)
			ps.AddParameter(key, value);

		List<PSObject> output = new();
		Exception? terminatingEx = null;

		try
		{
			output.AddRange(ps.Invoke());
		}
		catch (Exception ex)
		{
			terminatingEx = ex;
		}

		return new CmdletResult(output, ps.Streams.Error.ToList(), terminatingEx);
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_runspace.Close();
			_runspace.Dispose();
			_disposed = true;
		}
	}
}

internal record CmdletResult(
	IReadOnlyList<PSObject> Output,
	IReadOnlyList<ErrorRecord> Errors,
	Exception? TerminatingException)
{
	public bool HadTerminatingError => TerminatingException != null;

	public string? TerminatingErrorId => TerminatingException switch
	{
		RuntimeException re => BaseId(re.ErrorRecord?.FullyQualifiedErrorId),
		_ => null
	};

	public ErrorRecord? FirstError => Errors.Count > 0 ? Errors[0] : null;
	public string? FirstErrorId => BaseId(FirstError?.FullyQualifiedErrorId);

	private static string? BaseId(string? id) => id?.Split(',')[0];
}
