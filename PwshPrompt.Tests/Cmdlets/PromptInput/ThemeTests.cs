using System.Collections;
using PwshPrompt.Tests.Helpers;
using Xunit;

namespace PwshPrompt.Tests.Cmdlets.PromptInput;

public class ThemeTests
{
	public class Pass
	{
		[Fact]
		public void Message_string_written_line_contains_foreground_escape()
		{
			using CmdletHarness harness = new("val");
			harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "Enter:"
			});
			string written = string.Join("", harness.Host.TestUI.WrittenLines);
			// Both 256-color and 24-bit modes include "\x1b[38;"
			Assert.Contains("\x1b[38;", written);
		}

		[Fact]
		public void Message_string_written_line_contains_reset()
		{
			using CmdletHarness harness = new("val");
			harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "Enter:"
			});
			string written = string.Join("", harness.Host.TestUI.WrittenLines);
			Assert.Contains("\x1b[0m", written);
		}

		[Fact]
		public void Message_with_explicit_foreground_color_contains_that_color()
		{
			using CmdletHarness harness = new("val");
			harness.Invoke("Prompt-Input", new()
			{
				["Message"] = new Hashtable
				{
					["Text"] = "Enter:",
					["ForegroundColor"] = new object[] { "196", "255;0;0" }
				}
			});
			string written = string.Join("", harness.Host.TestUI.WrittenLines);
			// 256-color: \x1b[38;5;196m  OR  24-bit: \x1b[38;2;255;0;0m
			Assert.True(
				written.Contains("\x1b[38;5;196m") || written.Contains("\x1b[38;2;255;0;0m"),
				$"Expected 256-color or 24-bit foreground escape in: {written}");
		}

		[Fact]
		public void Message_with_background_color_contains_bg_escape()
		{
			using CmdletHarness harness = new("val");
			harness.Invoke("Prompt-Input", new()
			{
				["Message"] = new Hashtable
				{
					["Text"] = "Enter:",
					["BackgroundColor"] = new object[] { "196", "255;0;0" }
				}
			});
			string written = string.Join("", harness.Host.TestUI.WrittenLines);
			Assert.Contains("\x1b[48;", written);
		}

		[Fact]
		public void Title_string_has_bold_style_by_default()
		{
			using CmdletHarness harness = new("val");
			harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "msg",
				["Title"] = "Section"
			});
			string written = string.Join("", harness.Host.TestUI.WrittenLines);
			Assert.Contains("\x1b[1m", written); // Bold = code 1
		}

		[Fact]
		public void Title_hashtable_without_style_uses_bold_default()
		{
			using CmdletHarness harness = new("val");
			harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "msg",
				["Title"] = new Hashtable { ["Text"] = "Section" }
			});
			string written = string.Join("", harness.Host.TestUI.WrittenLines);
			Assert.Contains("\x1b[1m", written);
		}

		[Fact]
		public void Title_with_bold_italic_style_contains_both_codes()
		{
			using CmdletHarness harness = new("val");
			harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "msg",
				["Title"] = new Hashtable { ["Text"] = "T", ["Style"] = "Bold,Italic" }
			});
			string written = string.Join("", harness.Host.TestUI.WrittenLines);
			Assert.Contains("1", written);
			Assert.Contains("3", written);
		}

		[Fact]
		public void Default_parameter_writes_default_prefix_in_text()
		{
			using CmdletHarness harness = new("");
			harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test",
				["Default"] = "myval"
			});
			string written = string.Join("", harness.Host.TestUI.WrittenText);
			Assert.Contains("(default: myval)", written);
		}

		[Fact]
		public void No_default_does_not_write_default_prefix()
		{
			using CmdletHarness harness = new("input");
			harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test"
			});
			string written = string.Join("", harness.Host.TestUI.WrittenText);
			Assert.DoesNotContain("(default:", written);
		}

		[Fact]
		public void Prompt_character_written_to_text()
		{
			using CmdletHarness harness = new("input");
			harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test"
			});
			string written = string.Join("", harness.Host.TestUI.WrittenText);
			Assert.Contains("> ", written);
		}

		[Fact]
		public void Negative_rawUI_bg_omits_background_escape_in_message()
		{
			using CmdletHarness harness = new("val");
			// Set rawUI background to negative so Label ctor sets BackgroundColor=null
			harness.Host.TestUI.RawUIImpl.BackgroundColor = (ConsoleColor)(-1);
			harness.Invoke("Prompt-Input", new()
			{
				["Message"] = "test"
			});
			string written = string.Join("", harness.Host.TestUI.WrittenLines);
			Assert.DoesNotContain("\x1b[48;", written);
		}
	}
}
