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

	public class IntegrationWithExpectedType
	{
		[Fact]
		public void Int_validation_passes_when_in_range()
		{
			using CmdletHarness harness = new("1025");
			CmdletResult result = harness.Run(
				"Prompt-Input 'Port number' -ExpectedType int -Validation { @(($_ -ge 1024 -and $_ -le 65535), 'Must be between 1024 and 65535') }");
			Assert.False(result.HadTerminatingError);
			Assert.Empty(result.Errors);
			Assert.Equal(1025, result.Output[0].BaseObject);
			Assert.IsType<int>(result.Output[0].BaseObject);
		}

		[Fact]
		public void Int_validation_fails_with_custom_message_when_out_of_range()
		{
			using CmdletHarness harness = new("9", "1024");
			CmdletResult result = harness.Run(
				"Prompt-Input 'Port number' -ExpectedType int -Validation { @(($_ -ge 1024 -and $_ -le 65535), 'Must be between 1024 and 65535') }");
			Assert.False(result.HadTerminatingError);
			Assert.True(result.Errors.Count > 0);
			Assert.Equal("InvalidInputType", result.FirstErrorId);
			Assert.Contains("Must be between 1024 and 65535", result.FirstError!.Exception.Message);
			Assert.Equal(1024, result.Output[0].BaseObject);
		}

		[Fact]
		public void Int_validation_rejects_non_int_before_validation_runs()
		{
			using CmdletHarness harness = new("abc", "2000");
			CmdletResult result = harness.Run(
				"Prompt-Input 'Port number' -ExpectedType int -Validation { @(($_ -ge 1024 -and $_ -le 65535), 'Must be between 1024 and 65535') }");
			Assert.False(result.HadTerminatingError);
			Assert.True(result.Errors.Count > 0);
			Assert.Contains("cannot be converted to int", result.FirstError!.Exception.Message);
			Assert.Equal(2000, result.Output[0].BaseObject);
		}

		[Fact]
		public void Validation_false_without_message_includes_failed_validation()
		{
			using CmdletHarness harness = new("5", "hello");
			CmdletResult result = harness.Run(
				"Prompt-Input 'test' -Validation { @($_ -eq 'hello') }");
			Assert.False(result.HadTerminatingError);
			Assert.True(result.Errors.Count > 0);
			Assert.Contains("failed validation", result.FirstError!.Exception.Message);
			Assert.Equal("hello", result.Output[0].BaseObject);
		}

		[Fact]
		public void Dollar_underscore_receives_typed_int_value()
		{
			using CmdletHarness harness = new("42");
			CmdletResult result = harness.Run(
				"Prompt-Input 'test' -ExpectedType int -Validation { @($_ -is [int]) }");
			Assert.False(result.HadTerminatingError);
			Assert.Empty(result.Errors);
			Assert.Equal(42, result.Output[0].BaseObject);
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
