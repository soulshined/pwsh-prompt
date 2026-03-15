using System.Collections;
using PwshPrompt.Enums;
using PwshPrompt.Tests.Helpers;
using Xunit;

namespace PwshPrompt.Tests.Cmdlets.PromptChoice;

public class ParameterTests : IDisposable
{
	private readonly CmdletHarness _harness = new();

	public void Dispose() => _harness.Dispose();

	private CmdletResult Invoke(Dictionary<string, object?> parameters)
	{
		if (!parameters.ContainsKey("Choices"))
			parameters["Choices"] = new object[] { "a", "b", "c" };
		if (!parameters.ContainsKey("Message"))
			parameters["Message"] = "Pick one";
		return _harness.Invoke("Prompt-Choice", parameters);
	}

	#region Pass tests
	// These pass BeginProcessing but fail at ProcessRecord with NonInteractiveTerminal
	// because there is no real console attached.

	[Fact]
	public void String_array_choices()
	{
		var result = Invoke(new() { ["Choices"] = new object[] { "a", "b", "c" } });
		Assert.True(result.HadTerminatingError);
		Assert.Equal("NonInteractiveTerminal", result.TerminatingErrorId);
	}

	[Fact]
	public void Int_array_choices()
	{
		var result = Invoke(new() { ["Choices"] = new object[] { 1, 2, 3 } });
		Assert.True(result.HadTerminatingError);
		Assert.Equal("NonInteractiveTerminal", result.TerminatingErrorId);
	}

	[Fact]
	public void Hashtable_choices_with_Value_key()
	{
		var result = Invoke(new() {
			["Choices"] = new object[] { new Hashtable { { "Value", "opt1" } } }
		});
		Assert.True(result.HadTerminatingError);
		Assert.Equal("NonInteractiveTerminal", result.TerminatingErrorId);
	}

	[Fact]
	public void Single_element_choices()
	{
		var result = Invoke(new() { ["Choices"] = new object[] { "only" } });
		Assert.True(result.HadTerminatingError);
		Assert.Equal("NonInteractiveTerminal", result.TerminatingErrorId);
	}

	[Fact]
	public void Mixed_string_and_hashtable_choices()
	{
		var result = Invoke(new() {
			["Choices"] = new object[] { "a", new Hashtable { { "Value", "b" } } }
		});
		Assert.True(result.HadTerminatingError);
		Assert.Equal("NonInteractiveTerminal", result.TerminatingErrorId);
	}

	[Fact]
	public void Mixed_int_and_hashtable_choices()
	{
		var result = Invoke(new() {
			["Choices"] = new object[] { 1, new Hashtable { { "Value", "b" } } }
		});
		Assert.True(result.HadTerminatingError);
		Assert.Equal("NonInteractiveTerminal", result.TerminatingErrorId);
	}

	[Fact]
	public void Message_as_string()
	{
		var result = Invoke(new() { ["Message"] = "Pick one" });
		Assert.True(result.HadTerminatingError);
		Assert.Equal("NonInteractiveTerminal", result.TerminatingErrorId);
	}

	[Fact]
	public void Message_as_hashtable_with_Text()
	{
		var result = Invoke(new() {
			["Message"] = new Hashtable { { "Text", "Pick" } }
		});
		Assert.True(result.HadTerminatingError);
		Assert.Equal("NonInteractiveTerminal", result.TerminatingErrorId);
	}

	[Fact]
	public void Title_as_string()
	{
		var result = Invoke(new() { ["Title"] = "My Title" });
		Assert.True(result.HadTerminatingError);
		Assert.Equal("NonInteractiveTerminal", result.TerminatingErrorId);
	}

	[Fact]
	public void Title_as_hashtable_with_Text_and_Style()
	{
		var result = Invoke(new() {
			["Title"] = new Hashtable { { "Text", "T" }, { "Style", "Bold" } }
		});
		Assert.True(result.HadTerminatingError);
		Assert.Equal("NonInteractiveTerminal", result.TerminatingErrorId);
	}

	[Fact]
	public void Default_zero_with_one_choice()
	{
		var result = Invoke(new() {
			["Choices"] = new object[] { "only" },
			["Default"] = 0
		});
		Assert.True(result.HadTerminatingError);
		Assert.Equal("NonInteractiveTerminal", result.TerminatingErrorId);
	}

	[Fact]
	public void CycleMode_Cycle()
	{
		var result = Invoke(new() { ["CycleMode"] = CycleMode.Cycle });
		Assert.True(result.HadTerminatingError);
		Assert.Equal("NonInteractiveTerminal", result.TerminatingErrorId);
	}

	[Fact]
	public void CycleMode_Stop()
	{
		var result = Invoke(new() { ["CycleMode"] = CycleMode.Stop });
		Assert.True(result.HadTerminatingError);
		Assert.Equal("NonInteractiveTerminal", result.TerminatingErrorId);
	}

	[Fact]
	public void Multiple_switch()
	{
		var result = Invoke(new() { ["Multiple"] = true });
		Assert.True(result.HadTerminatingError);
		Assert.Equal("NonInteractiveTerminal", result.TerminatingErrorId);
	}

	[Fact]
	public void AlternateBuffer_empty_hashtable()
	{
		var result = Invoke(new() { ["AlternateBuffer"] = new Hashtable() });
		Assert.True(result.HadTerminatingError);
		Assert.Equal("NonInteractiveTerminal", result.TerminatingErrorId);
	}

	[Fact]
	public void AlternateBuffer_with_fg_bg()
	{
		var result = Invoke(new() {
			["AlternateBuffer"] = new Hashtable {
				{ "fg", new object[] { "White", "255;255;255" } },
				{ "bg", new object[] { "DarkBlue", "0;0;139" } }
			}
		});
		Assert.True(result.HadTerminatingError);
		Assert.Equal("NonInteractiveTerminal", result.TerminatingErrorId);
	}

	#endregion

	#region Fail tests

	[Fact]
	public void Mixed_string_and_int_choices()
	{
		var result = Invoke(new() {
			["Choices"] = new object[] { "a", 1 }
		});
		Assert.True(result.HadTerminatingError);
		Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
	}

	[Fact]
	public void Hashtable_choices_missing_Value()
	{
		var result = Invoke(new() {
			["Choices"] = new object[] { new Hashtable { { "Description", "x" } } }
		});
		Assert.True(result.HadTerminatingError);
		Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
	}

	[Fact]
	public void Hashtable_choices_with_unknown_key()
	{
		var result = Invoke(new() {
			["Choices"] = new object[] { new Hashtable { { "Value", "x" }, { "Unknown", "y" } } }
		});
		Assert.True(result.HadTerminatingError);
		Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
	}

	[Fact]
	public void Integer_Message()
	{
		var result = Invoke(new() { ["Message"] = 42 });
		Assert.True(result.HadTerminatingError);
		Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
	}

	[Fact]
	public void Message_hashtable_missing_Text()
	{
		var result = Invoke(new() {
			["Message"] = new Hashtable { { "Style", "Bold" } }
		});
		Assert.True(result.HadTerminatingError);
		Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
	}

	[Fact]
	public void Integer_Title()
	{
		var result = Invoke(new() { ["Title"] = 42 });
		Assert.True(result.HadTerminatingError);
		Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
	}

	[Fact]
	public void Title_hashtable_missing_Text()
	{
		var result = Invoke(new() {
			["Title"] = new Hashtable { { "ForegroundColor", "Red" } }
		});
		Assert.True(result.HadTerminatingError);
		Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
	}

	[Fact]
	public void Default_exceeds_choices_count()
	{
		var result = Invoke(new() {
			["Choices"] = new object[] { "a", "b", "c" },
			["Default"] = 3
		});
		Assert.True(result.HadTerminatingError);
		Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
	}

	[Fact]
	public void Default_one_with_single_choice()
	{
		var result = Invoke(new() {
			["Choices"] = new object[] { "only" },
			["Default"] = 1
		});
		Assert.True(result.HadTerminatingError);
		Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
	}

	[Fact]
	public void AlternateBuffer_unknown_key()
	{
		var result = Invoke(new() {
			["AlternateBuffer"] = new Hashtable { { "UnknownKey", "x" } }
		});
		Assert.True(result.HadTerminatingError);
		Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
	}

	[Fact]
	public void Message_exceeding_228_chars()
	{
		var result = Invoke(new() {
			["Message"] = new string('x', 229)
		});
		Assert.True(result.HadTerminatingError);
		Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
	}

	#endregion
}
