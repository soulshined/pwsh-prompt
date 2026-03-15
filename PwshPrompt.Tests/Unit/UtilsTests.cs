using System.Management.Automation;
using PwshPrompt.Consts;
using PwshPrompt.Utils;
using Xunit;

namespace PwshPrompt.Tests.Unit;

public class StringExtensionsTests
{
	public class StripNewlines
	{
		[Fact]
		public void Removes_newline() => Assert.Equal("ab", "a\nb".StripNewlines());

		[Fact]
		public void Removes_crlf() => Assert.Equal("ab", "a\r\nb".StripNewlines());

		[Fact]
		public void Empty_string_returns_empty() => Assert.Equal("", "".StripNewlines());

		[Fact]
		public void No_newlines_unchanged() => Assert.Equal("hello", "hello".StripNewlines());
	}

	public class VisualWidth
	{
		[Fact]
		public void Ascii_equals_length() => Assert.Equal(5, "hello".VisualWidth());

		[Fact]
		public void Empty_string_is_zero() => Assert.Equal(0, "".VisualWidth());

		[Theory]
		[InlineData("⌛", 2)]
		[InlineData("📋", 2)]
		public void Wide_emoji_is_two(string emoji, int expected) => Assert.Equal(expected, emoji.VisualWidth());

		[Fact]
		public void FE0F_forces_emoji_presentation_wide() => Assert.Equal(2, "☑️".VisualWidth());

		[Fact]
		public void Supplementary_pictograph_is_wide() => Assert.Equal(2, "\U0001F5D9".VisualWidth());

		[Fact]
		public void Mixed_emoji_and_ascii() => Assert.Equal(15, "⌛ Review Later".VisualWidth());

		[Fact]
		public void CJK_characters_are_wide() => Assert.Equal(6, "日本語".VisualWidth());
	}

	public class TruncateIf
	{
		[Fact]
		public void Short_text_unchanged() => Assert.Equal("short", "short".TruncateIf(100));

		[Fact]
		public void Long_text_ends_with_ellipsis()
		{
			string result = "long text here".TruncateIf(10);
			Assert.EndsWith("\u2026", result);
		}

		[Fact]
		public void Empty_string_unchanged() => Assert.Equal("", "".TruncateIf(5));
	}

	public class WordWrap
	{
		[Fact]
		public void Short_line_unchanged()
		{
			List<string> result = "hello world".WordWrap(80);
			Assert.Single(result);
			Assert.Equal("hello world", result[0]);
		}

		[Fact]
		public void Wraps_at_max_width()
		{
			List<string> result = "one two three four".WordWrap(10);
			Assert.True(result.Count >= 2);
			Assert.All(result, line => Assert.True(line.VisualWidth() <= 10));
		}

		[Fact]
		public void Preserves_explicit_newlines()
		{
			List<string> result = "line one\nline two\nline three".WordWrap(80);
			Assert.Equal(3, result.Count);
			Assert.Equal("line one", result[0]);
			Assert.Equal("line two", result[1]);
			Assert.Equal("line three", result[2]);
		}

		[Fact]
		public void Empty_lines_preserved()
		{
			List<string> result = "before\n\nafter".WordWrap(80);
			Assert.Equal(3, result.Count);
			Assert.Equal("before", result[0]);
			Assert.Equal("", result[1]);
			Assert.Equal("after", result[2]);
		}

		[Fact]
		public void Empty_string_returns_single_empty()
		{
			List<string> result = "".WordWrap(80);
			Assert.Single(result);
			Assert.Equal("", result[0]);
		}

		[Fact]
		public void Long_word_stays_on_own_line()
		{
			List<string> result = "short verylongwordthatexceedswidth end".WordWrap(10);
			Assert.Contains(result, l => l.Contains("verylongwordthatexceedswidth"));
		}

		[Fact]
		public void Multiple_paragraphs_wrapped_independently()
		{
			List<string> result = "aa bb cc dd\nee ff gg hh".WordWrap(8);
			Assert.True(result.Count >= 4);
		}
	}
}

public class AnsiColorTests
{
	public class ParseConsoleColor
	{
		[Fact]
		public void Gray_string_returns_Gray() =>
			Assert.Equal(ConsoleColor.Gray, AnsiColor.ParseConsoleColor("Gray"));

		[Fact]
		public void Case_insensitive() =>
			Assert.Equal(ConsoleColor.Gray, AnsiColor.ParseConsoleColor("gray"));

		[Fact]
		public void ConsoleColor_value_passthrough() =>
			Assert.Equal(ConsoleColor.Red, AnsiColor.ParseConsoleColor(ConsoleColor.Red));

		[Fact]
		public void Unknown_string_returns_null() =>
			Assert.Null(AnsiColor.ParseConsoleColor("NotAColor"));

		[Fact]
		public void Null_returns_null() =>
			Assert.Null(AnsiColor.ParseConsoleColor(null));

		[Fact]
		public void Non_string_non_consolecolor_returns_null() =>
			Assert.Null(AnsiColor.ParseConsoleColor(42));
	}

	public class Parse
	{
		[Fact]
		public void ConsoleColor_names_resolved_to_ansi()
		{
			AnsiColor result = AnsiColor.Parse(new object[] { "Gray", "Gray" }, "key");
			Assert.Equal("7", result.Index);
			Assert.Equal("192;192;192", result.RGB);
		}

		[Fact]
		public void ConsoleColor_name_with_explicit_rgb()
		{
			AnsiColor result = AnsiColor.Parse(new object[] { "Gray", "189;179;149" }, "key");
			Assert.Equal("7", result.Index);
			Assert.Equal("189;179;149", result.RGB);
		}

		[Fact]
		public void Not_array_throws() =>
			Assert.Throws<PSArgumentException>(() => AnsiColor.Parse("not-array", "key"));

		[Fact]
		public void Single_element_array_throws() =>
			Assert.Throws<PSArgumentException>(() =>
				AnsiColor.Parse(new object[] { "Gray" }, "key"));

		[Fact]
		public void RGB_component_out_of_range_throws() =>
			Assert.Throws<PSArgumentException>(() =>
				AnsiColor.Parse(new object[] { "Gray", "300;0;0" }, "key"));
	}

	public class From
	{
		[Fact]
		public void Gray_has_index_7_and_correct_rgb()
		{
			AnsiColor color = AnsiColor.From(ConsoleColor.Gray);
			Assert.Equal("7", color.Index);
			Assert.Equal("192;192;192", color.RGB);
		}

		[Fact]
		public void Black_has_index_0() =>
			Assert.Equal("0", AnsiColor.From(ConsoleColor.Black).Index);

		[Fact]
		public void White_has_index_15() =>
			Assert.Equal("15", AnsiColor.From(ConsoleColor.White).Index);
	}
}
