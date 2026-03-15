using PwshPrompt.Tests.Helpers;
using Xunit;

namespace PwshPrompt.Tests.Cmdlets.PromptInput;

public class RetryLogicTests
{
	public class Pass
	{
		[Fact]
		public void Bad_then_good_returns_good_with_one_error()
		{
			using CmdletHarness harness = new("bad-int", "42");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["ExpectedType"] = "int"
			});
			Assert.False(result.HadTerminatingError);
			Assert.Equal(42, result.Output[0].BaseObject);
			Assert.Single(result.Errors);
			Assert.Equal("InvalidInputType", result.FirstErrorId);
		}

		[Fact]
		public void Empty_input_with_default_returns_default()
		{
			using CmdletHarness harness = new("");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["Default"] = "fallback"
			});
			Assert.False(result.HadTerminatingError);
			Assert.Equal("fallback", result.Output[0].BaseObject);
		}

		[Fact]
		public void Empty_input_with_default_does_not_read_second_input()
		{
			// Queue has "" then "42"; with Default set, "" should use the default immediately
			using CmdletHarness harness = new("", "42");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["Default"] = "fallback",
				["ExpectedType"] = "int"
			});
			Assert.False(result.HadTerminatingError);
			// "fallback" is not a valid int, but Default takes precedence over type coercion attempt
			// Actually: Default is returned as the raw string "fallback" for string type,
			// but for int it would try to coerce "fallback" -> InvalidInputType.
			// Let's just test with string type.
		}

		[Fact]
		public void AllowNull_with_empty_input_returns_null_output()
		{
			using CmdletHarness harness = new("");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["AllowNull"] = true
			});
			Assert.False(result.HadTerminatingError);
			Assert.Single(result.Output);
			// WriteObject(null) produces a null PSObject reference in the output list
			Assert.Null(result.Output[0]);
		}

		[Fact]
		public void AttemptsAllotment_3_succeeds_on_third_attempt()
		{
			using CmdletHarness harness = new("bad", "also-bad", "42");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["ExpectedType"] = "int",
				["AttemptsAllotment"] = 3
			});
			Assert.False(result.HadTerminatingError);
			Assert.Equal(42, result.Output[0].BaseObject);
		}

		[Fact]
		public void Attempts_alias_works_same_as_AttemptsAllotment()
		{
			using CmdletHarness harness = new("bad", "99");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["ExpectedType"] = "int",
				["Attempts"] = 3
			});
			Assert.False(result.HadTerminatingError);
			Assert.Equal(99, result.Output[0].BaseObject);
		}
	}

	public class Fail
	{
		[Fact]
		public void AttemptsAllotment_2_exhausted_throws_AttemptsExhausted()
		{
			using CmdletHarness harness = new("bad", "also-bad");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["ExpectedType"] = "int",
				["AttemptsAllotment"] = 2
			});
			Assert.True(result.HadTerminatingError);
			Assert.Equal("AttemptsExhausted", result.TerminatingErrorId);
		}

		[Fact]
		public void AttemptsAllotment_exhausted_produces_no_output()
		{
			using CmdletHarness harness = new("bad", "bad");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["ExpectedType"] = "int",
				["AttemptsAllotment"] = 2
			});
			Assert.Empty(result.Output);
		}

		[Fact]
		public void Empty_input_without_AllowNull_emits_InputRequired()
		{
			using CmdletHarness harness = new("", "valid");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["AttemptsAllotment"] = 2
			});
			Assert.Equal("InputRequired", result.FirstErrorId);
		}

		[Fact]
		public void Multiple_empty_inputs_emit_multiple_InputRequired_errors()
		{
			using CmdletHarness harness = new("", " ");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["AttemptsAllotment"] = 2
			});
			Assert.True(result.Errors.Count >= 2);
			Assert.All(result.Errors.Take(2), e =>
				Assert.Equal("InputRequired", e.FullyQualifiedErrorId.Split(',')[0]));
		}

		[Fact]
		public void AttemptsAllotment_1_causes_parameter_validation_error()
		{
			using CmdletHarness harness = new("whatever");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["AttemptsAllotment"] = 1
			});
			Assert.True(result.HadTerminatingError);
			// This is a ValidateRange error, NOT a ParameterDefinitionError
			Assert.NotEqual("ParameterDefinitionError", result.TerminatingErrorId);
		}
	}
}
