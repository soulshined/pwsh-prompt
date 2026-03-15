using PwshPrompt.Tests.Helpers;
using Xunit;

namespace PwshPrompt.Tests.Cmdlets.PromptInput;

public class TypeCoercionTests
{
	private static CmdletResult RunWithInput(string expectedType, string input, string? culture = null)
	{
		using CmdletHarness harness = new(input);
		Dictionary<string, object?> p = new()
		{
			["Message"] = "test",
			["ExpectedType"] = expectedType
		};
		if (culture != null) p["Culture"] = culture;
		return harness.Invoke("Prompt-Input", p);
	}

	private static CmdletResult RunFail(string expectedType, string input1, string input2 = "also-bad")
	{
		using CmdletHarness harness = new(input1, input2);
		return harness.Invoke("Prompt-Input", new()
		{
			["Message"] = "test",
			["ExpectedType"] = expectedType,
			["AttemptsAllotment"] = 2
		});
	}

	public class Pass
	{
		[Theory]
		[InlineData("1", true)]
		[InlineData("y", true)]
		[InlineData("yes", true)]
		[InlineData("on", true)]
		[InlineData("YES", true)]
		[InlineData("ON", true)]
		[InlineData("0", false)]
		[InlineData("n", false)]
		[InlineData("no", false)]
		[InlineData("off", false)]
		[InlineData("NO", false)]
		[InlineData("OFF", false)]
		public void Bool_valid_input(string input, bool expected)
		{
			CmdletResult result = RunWithInput("bool", input);
			Assert.False(result.HadTerminatingError);
			Assert.Equal(expected, result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("0", (byte)0)]
		[InlineData("128", (byte)128)]
		[InlineData("255", (byte)255)]
		public void Byte_valid_input(string input, byte expected)
		{
			CmdletResult result = RunWithInput("byte", input);
			Assert.Equal(expected, result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("-32768", (short)-32768)]
		[InlineData("0", (short)0)]
		[InlineData("32767", (short)32767)]
		public void Short_valid_input(string input, short expected)
		{
			CmdletResult result = RunWithInput("short", input);
			Assert.Equal(expected, result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("0", (ushort)0)]
		[InlineData("65535", (ushort)65535)]
		public void Ushort_valid_input(string input, ushort expected)
		{
			CmdletResult result = RunWithInput("ushort", input);
			Assert.Equal(expected, result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("-2147483648", int.MinValue)]
		[InlineData("0", 0)]
		[InlineData("2147483647", int.MaxValue)]
		public void Int_valid_input(string input, int expected)
		{
			CmdletResult result = RunWithInput("int", input);
			Assert.Equal(expected, result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("-2147483648", int.MinValue)]
		[InlineData("0", 0)]
		[InlineData("2147483647", int.MaxValue)]
		public void Integer_alias_valid_input(string input, int expected)
		{
			CmdletResult result = RunWithInput("integer", input);
			Assert.Equal(expected, result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("0", 0u)]
		[InlineData("4294967295", 4294967295u)]
		public void Uint_valid_input(string input, uint expected)
		{
			CmdletResult result = RunWithInput("uint", input);
			Assert.Equal(expected, result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("-9223372036854775808")]
		[InlineData("0")]
		[InlineData("9223372036854775807")]
		public void Long_valid_input(string input)
		{
			CmdletResult result = RunWithInput("long", input);
			Assert.False(result.HadTerminatingError);
			Assert.IsType<long>(result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("0")]
		[InlineData("18446744073709551615")]
		public void Ulong_valid_input(string input)
		{
			CmdletResult result = RunWithInput("ulong", input);
			Assert.False(result.HadTerminatingError);
			Assert.IsType<ulong>(result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("1.5")]
		[InlineData("0")]
		[InlineData("-1.5")]
		public void Float_valid_input(string input)
		{
			CmdletResult result = RunWithInput("float", input);
			Assert.False(result.HadTerminatingError);
			Assert.IsType<float>(result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("1.5")]
		[InlineData("0")]
		[InlineData("-1.5")]
		public void Double_valid_input(string input)
		{
			CmdletResult result = RunWithInput("double", input);
			Assert.False(result.HadTerminatingError);
			Assert.IsType<double>(result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("1.5")]
		[InlineData("0")]
		[InlineData("-1.5")]
		public void Decimal_valid_input(string input)
		{
			CmdletResult result = RunWithInput("decimal", input);
			Assert.False(result.HadTerminatingError);
			Assert.IsType<decimal>(result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("hello")]
		[InlineData("any string")]
		[InlineData("123")]
		public void String_returns_input_unchanged(string input)
		{
			CmdletResult result = RunWithInput("string", input);
			Assert.Equal(input, result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("a", 'a')]
		[InlineData("Z", 'Z')]
		[InlineData("9", '9')]
		[InlineData("!", '!')]
		public void Char_single_character(string input, char expected)
		{
			CmdletResult result = RunWithInput("char", input);
			Assert.Equal(expected, result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("^[a-z]+$")]
		[InlineData(".*")]
		[InlineData("[0-9]{3}")]
		public void Regex_valid_pattern(string input)
		{
			CmdletResult result = RunWithInput("regex", input);
			Assert.False(result.HadTerminatingError);
			Assert.IsType<System.Text.RegularExpressions.Regex>(result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("12345678-1234-1234-1234-123456789012")]
		public void Guid_valid_format(string input)
		{
			CmdletResult result = RunWithInput("guid", input);
			Assert.False(result.HadTerminatingError);
			Assert.IsType<Guid>(result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("1.0")]
		[InlineData("1.0.0")]
		[InlineData("1.2.3.4")]
		public void Version_valid_format(string input)
		{
			CmdletResult result = RunWithInput("version", input);
			Assert.False(result.HadTerminatingError);
			Assert.IsType<Version>(result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("https://example.com")]
		[InlineData("http://localhost")]
		[InlineData("ftp://host/path")]
		public void Uri_absolute_uri(string input)
		{
			CmdletResult result = RunWithInput("uri", input);
			Assert.False(result.HadTerminatingError);
			Assert.IsType<Uri>(result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("2024-01-15")]
		[InlineData("2000-12-31")]
		public void Date_valid_format(string input)
		{
			CmdletResult result = RunWithInput("date", input, "en-US");
			Assert.False(result.HadTerminatingError);
			Assert.IsType<DateOnly>(result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("2024-01-15 14:30:00")]
		public void Datetime_valid_format(string input)
		{
			CmdletResult result = RunWithInput("datetime", input, "en-US");
			Assert.False(result.HadTerminatingError);
			Assert.IsType<DateTime>(result.Output[0].BaseObject);
		}

		[Theory]
		[InlineData("14:30:00")]
		[InlineData("00:00:00")]
		[InlineData("23:59:59")]
		public void Time_valid_format(string input)
		{
			CmdletResult result = RunWithInput("time", input, "en-US");
			Assert.False(result.HadTerminatingError);
			Assert.IsType<TimeOnly>(result.Output[0].BaseObject);
		}

		[Fact]
		public void Directory_existing_path_returns_DirectoryInfo()
		{
			string tempDir = Path.GetTempPath();
			CmdletResult result = RunWithInput("directory", tempDir);
			Assert.False(result.HadTerminatingError);
			Assert.IsType<System.IO.DirectoryInfo>(result.Output[0].BaseObject);
		}

		[Fact]
		public void File_existing_path_returns_FileInfo()
		{
			string tempFile = Path.GetTempFileName();
			try
			{
				CmdletResult result = RunWithInput("file", tempFile);
				Assert.False(result.HadTerminatingError);
				Assert.IsType<System.IO.FileInfo>(result.Output[0].BaseObject);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}
	}

	public class Fail
	{
		[Theory]
		[InlineData("true")]
		[InlineData("false")]
		[InlineData("2")]
		[InlineData("yes!")]
		public void Bool_invalid_input_emits_InvalidInputType(string input)
		{
			CmdletResult result = RunFail("bool", input);
			Assert.Equal("InvalidInputType", result.FirstErrorId);
		}

		[Theory]
		[InlineData("256")]
		[InlineData("-1")]
		[InlineData("abc")]
		public void Byte_invalid_input_emits_InvalidInputType(string input)
		{
			CmdletResult result = RunFail("byte", input);
			Assert.Equal("InvalidInputType", result.FirstErrorId);
		}

		[Theory]
		[InlineData("ab")]
		[InlineData("abc")]
		public void Char_multi_char_emits_InvalidInputType(string input)
		{
			CmdletResult result = RunFail("char", input);
			Assert.Equal("InvalidInputType", result.FirstErrorId);
		}

		[Theory]
		[InlineData("abc")]
		[InlineData("2147483648")]
		[InlineData("-2147483649")]
		public void Int_invalid_input_emits_InvalidInputType(string input)
		{
			CmdletResult result = RunFail("int", input);
			Assert.Equal("InvalidInputType", result.FirstErrorId);
		}

		[Theory]
		[InlineData("abc")]
		[InlineData("not-a-number")]
		public void Float_invalid_input_emits_InvalidInputType(string input)
		{
			CmdletResult result = RunFail("float", input);
			Assert.Equal("InvalidInputType", result.FirstErrorId);
		}

		[Theory]
		[InlineData("abc")]
		[InlineData("not-a-number")]
		public void Double_invalid_input_emits_InvalidInputType(string input)
		{
			CmdletResult result = RunFail("double", input);
			Assert.Equal("InvalidInputType", result.FirstErrorId);
		}

		[Theory]
		[InlineData("abc")]
		[InlineData("not-a-number")]
		public void Decimal_invalid_input_emits_InvalidInputType(string input)
		{
			CmdletResult result = RunFail("decimal", input);
			Assert.Equal("InvalidInputType", result.FirstErrorId);
		}

		[Theory]
		[InlineData("[unclosed")]
		[InlineData("(?invalid")]
		public void Regex_invalid_pattern_emits_InvalidInputType(string input)
		{
			CmdletResult result = RunFail("regex", input);
			Assert.Equal("InvalidInputType", result.FirstErrorId);
		}

		[Theory]
		[InlineData("not-a-guid")]
		[InlineData("12345")]
		public void Guid_invalid_format_emits_InvalidInputType(string input)
		{
			CmdletResult result = RunFail("guid", input);
			Assert.Equal("InvalidInputType", result.FirstErrorId);
		}

		[Theory]
		[InlineData("abc")]
		[InlineData("1.2.3.4.5.6")]
		public void Version_invalid_format_emits_InvalidInputType(string input)
		{
			CmdletResult result = RunFail("version", input);
			Assert.Equal("InvalidInputType", result.FirstErrorId);
		}

		[Theory]
		[InlineData("relative/path")]
		public void Uri_non_absolute_emits_InvalidInputType(string input)
		{
			CmdletResult result = RunFail("uri", input);
			Assert.Equal("InvalidInputType", result.FirstErrorId);
		}

		[Theory]
		[InlineData("not-a-date")]
		[InlineData("32/13/2024")]
		public void Date_invalid_format_emits_InvalidInputType(string input)
		{
			CmdletResult result = RunFail("date", input);
			Assert.Equal("InvalidInputType", result.FirstErrorId);
		}

		[Fact]
		public void Directory_nonexistent_path_emits_InvalidInputType()
		{
			CmdletResult result = RunFail("directory", "/does/not/exist/at/all");
			Assert.Equal("InvalidInputType", result.FirstErrorId);
		}

		[Fact]
		public void File_nonexistent_path_emits_InvalidInputType()
		{
			CmdletResult result = RunFail("file", "/does/not/exist/at/all.txt");
			Assert.Equal("InvalidInputType", result.FirstErrorId);
		}
	}
}
