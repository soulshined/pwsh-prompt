using System.Management.Automation;
using PwshPrompt.Tests.Helpers;
using Xunit;

namespace PwshPrompt.Tests.Cmdlets.PromptInput;

public class ValidationTests
{
	public class Pass
	{
		[Fact]
		public void ScriptBlock_returning_true_null_allows_input()
		{
			using CmdletHarness harness = new("anything");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["Validation"] = ScriptBlock.Create("@($true, $null)")
			});
			Assert.False(result.HadTerminatingError);
			Assert.Equal("anything", result.Output[0].BaseObject);
		}

		[Fact]
		public void ScriptBlock_returning_single_true_is_accepted()
		{
			using CmdletHarness harness = new("hello");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["Validation"] = ScriptBlock.Create("@($true)")
			});
			Assert.False(result.HadTerminatingError);
			Assert.Equal("hello", result.Output[0].BaseObject);
		}

		[Fact]
		public void Dollar_underscore_receives_coerced_value()
		{
			using CmdletHarness harness = new("hello");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["Validation"] = ScriptBlock.Create("@($_ -eq 'hello')")
			});
			Assert.False(result.HadTerminatingError);
			Assert.Equal("hello", result.Output[0].BaseObject);
		}

		[Fact]
		public void Null_validation_passes_through()
		{
			using CmdletHarness harness = new("input");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["Validation"] = null
			});
			Assert.False(result.HadTerminatingError);
			Assert.Equal("input", result.Output[0].BaseObject);
		}

		[Fact]
		public void Validation_with_int_type_output_is_coerced_int()
		{
			using CmdletHarness harness = new("5");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["ExpectedType"] = "int",
				["Validation"] = ScriptBlock.Create("@($true, $null)")
			});
			Assert.False(result.HadTerminatingError);
			Assert.Equal(5, result.Output[0].BaseObject);
			Assert.IsType<int>(result.Output[0].BaseObject);
		}
	}

	public class Fail
	{
		[Fact]
		public void Throwing_scriptblock_emits_ParameterDefinitionError()
		{
			using CmdletHarness harness = new("anything");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["Validation"] = ScriptBlock.Create("throw [System.ArgumentNullException]::new('x')")
			});
			Assert.True(result.HadTerminatingError);
			Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
		}

		[Fact]
		public void Non_bool_first_element_emits_ParameterDefinitionError()
		{
			using CmdletHarness harness = new("anything");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["Validation"] = ScriptBlock.Create("@('not-bool', 'message')")
			});
			Assert.True(result.HadTerminatingError);
			Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
		}

		[Fact]
		public void Three_element_result_emits_ParameterDefinitionError()
		{
			using CmdletHarness harness = new("anything");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["Validation"] = ScriptBlock.Create("@($true, $false, $false)")
			});
			Assert.True(result.HadTerminatingError);
			Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
		}

		[Fact]
		public void Non_string_second_element_emits_ParameterDefinitionError()
		{
			using CmdletHarness harness = new("anything");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["Validation"] = ScriptBlock.Create("@($false, 42)")
			});
			Assert.True(result.HadTerminatingError);
			Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
		}

		[Fact]
		public void Empty_result_emits_ParameterDefinitionError()
		{
			using CmdletHarness harness = new("anything");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["Validation"] = ScriptBlock.Create("@()")
			});
			Assert.True(result.HadTerminatingError);
			Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
		}

		[Fact]
		public void False_result_emits_InvalidInputType_with_message()
		{
			using CmdletHarness harness = new("ab", "also-bad");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["Validation"] = ScriptBlock.Create("@($false, 'Too short')"),
				["AttemptsAllotment"] = 2
			});
			Assert.True(result.Errors.Count > 0);
			Assert.Equal("InvalidInputType", result.FirstErrorId);
			Assert.Contains("Too short", result.FirstError!.Exception.Message);
		}
	}
}
