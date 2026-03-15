using System.Collections;
using PwshPrompt.Tests.Helpers;
using Xunit;

namespace PwshPrompt.Tests.Cmdlets.PromptInput;

public class ParameterTests
{
	public class Pass
	{
		[Fact]
		public void Message_as_string_produces_output()
		{
			using CmdletHarness harness = new("hello");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "Enter name"
			});
			Assert.False(result.HadTerminatingError);
			Assert.Equal("hello", result.Output[0].BaseObject);
		}

		[Fact]
		public void Message_as_hashtable_produces_output()
		{
			using CmdletHarness harness = new("hello");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = new Hashtable { ["Text"] = "Enter name" }
			});
			Assert.False(result.HadTerminatingError);
			Assert.Equal("hello", result.Output[0].BaseObject);
		}

		[Fact]
		public void Title_as_string_is_written_before_prompt()
		{
			using CmdletHarness harness = new("input");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "msg",
				["Title"] = "My Title"
			});
			Assert.False(result.HadTerminatingError);
			string written = string.Join("", harness.Host.TestUI.WrittenLines);
			Assert.Contains("My Title", written);
		}

		[Fact]
		public void Title_as_hashtable_is_written()
		{
			using CmdletHarness harness = new("input");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "msg",
				["Title"] = new Hashtable { ["Text"] = "My Title", ["Style"] = "Bold" }
			});
			Assert.False(result.HadTerminatingError);
			string written = string.Join("", harness.Host.TestUI.WrittenLines);
			Assert.Contains("My Title", written);
		}

		[Fact]
		public void Culture_en_US_parses_decimal_with_period()
		{
			using CmdletHarness harness = new("1.5");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["ExpectedType"] = "float",
				["Culture"] = "en-US"
			});
			Assert.False(result.HadTerminatingError);
			Assert.Equal(1.5f, result.Output[0].BaseObject);
		}

		[Fact]
		public void Culture_de_DE_parses_decimal_with_comma()
		{
			using CmdletHarness harness = new("1,5");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["ExpectedType"] = "float",
				["Culture"] = "de-DE"
			});
			Assert.False(result.HadTerminatingError);
			Assert.Equal(1.5f, (float)result.Output[0].BaseObject, precision: 3);
		}

		[Fact]
		public void Default_parameter_returns_on_empty_input()
		{
			using CmdletHarness harness = new("");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["Default"] = "fallback"
			});
			Assert.Equal("fallback", result.Output[0].BaseObject);
		}

		[Fact]
		public void Default_parameter_appears_in_written_text()
		{
			using CmdletHarness harness = new("");
			harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["Default"] = "mydefault"
			});
			string written = string.Join("", harness.Host.TestUI.WrittenText);
			Assert.Contains("(default: mydefault)", written);
		}

		[Fact]
		public void No_default_parameter_does_not_write_default_text()
		{
			using CmdletHarness harness = new("val");
			harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test"
			});
			string written = string.Join("", harness.Host.TestUI.WrittenText);
			Assert.DoesNotContain("(default:", written);
		}

		[Fact]
		public void Attempts_alias_accepted()
		{
			using CmdletHarness harness = new("42");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["ExpectedType"] = "int",
				["Attempts"] = 3
			});
			Assert.False(result.HadTerminatingError);
		}

	}

	public class Fail
	{
		[Fact]
		public void Integer_Message_emits_ParameterDefinitionError()
		{
			using CmdletHarness harness = new("x");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = 42
			});
			Assert.True(result.HadTerminatingError);
			Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
		}

		[Fact]
		public void Integer_Title_emits_ParameterDefinitionError()
		{
			using CmdletHarness harness = new("x");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["Title"] = 42
			});
			Assert.True(result.HadTerminatingError);
			Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
		}

		[Fact]
		public void Message_hashtable_missing_Text_emits_ParameterDefinitionError()
		{
			using CmdletHarness harness = new("x");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = new Hashtable { ["Style"] = "Bold" }
			});
			Assert.True(result.HadTerminatingError);
			Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
		}

		[Fact]
		public void Title_hashtable_missing_Text_emits_ParameterDefinitionError()
		{
			using CmdletHarness harness = new("x");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["Title"] = new Hashtable { ["ForegroundColor"] = new object[] { "0", "0;0;0" } }
			});
			Assert.True(result.HadTerminatingError);
			Assert.Equal("ParameterDefinitionError", result.TerminatingErrorId);
		}

		[Fact]
		public void AttemptsAllotment_1_causes_parameter_binding_error()
		{
			using CmdletHarness harness = new("x");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["AttemptsAllotment"] = 1
			});
			Assert.True(result.HadTerminatingError);
			Assert.NotEqual("ParameterDefinitionError", result.TerminatingErrorId);
		}

		[Fact]
		public void Culture_en_US_with_non_numeric_emits_InvalidInputType()
		{
			using CmdletHarness harness = new("not-a-float", "not-a-float");
			CmdletResult result = harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["ExpectedType"] = "float",
				["Culture"] = "en-US",
				["AttemptsAllotment"] = 2
			});
			Assert.Equal("InvalidInputType", result.FirstErrorId);
		}
	}
}
