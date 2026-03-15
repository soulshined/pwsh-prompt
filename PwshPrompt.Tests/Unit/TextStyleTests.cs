using PwshPrompt.Enums;
using Xunit;

namespace PwshPrompt.Tests.Unit;

public class TextStyleTests
{
	public class ToAnsi
	{
		[Fact]
		public void None_returns_empty_string() =>
			Assert.Equal(string.Empty, TextStyle.None.ToAnsi());

		[Fact]
		public void Bold_returns_correct_code() =>
			Assert.Equal("\x1b[1m", TextStyle.Bold.ToAnsi());

		[Fact]
		public void Dim_returns_correct_code() =>
			Assert.Equal("\x1b[2m", TextStyle.Dim.ToAnsi());

		[Fact]
		public void Italic_returns_correct_code() =>
			Assert.Equal("\x1b[3m", TextStyle.Italic.ToAnsi());

		[Fact]
		public void Underline_returns_correct_code() =>
			Assert.Equal("\x1b[4m", TextStyle.Underline.ToAnsi());

		[Fact]
		public void SlowBlink_returns_correct_code() =>
			Assert.Equal("\x1b[5m", TextStyle.SlowBlink.ToAnsi());

		[Fact]
		public void RapidBlink_returns_correct_code() =>
			Assert.Equal("\x1b[6m", TextStyle.RapidBlink.ToAnsi());

		[Fact]
		public void Reverse_returns_correct_code() =>
			Assert.Equal("\x1b[7m", TextStyle.Reverse.ToAnsi());

		[Fact]
		public void Hidden_returns_correct_code() =>
			Assert.Equal("\x1b[8m", TextStyle.Hidden.ToAnsi());

		[Fact]
		public void Strikethrough_returns_correct_code() =>
			Assert.Equal("\x1b[9m", TextStyle.Strikethrough.ToAnsi());

		[Fact]
		public void DoubleUnderline_returns_correct_code() =>
			Assert.Equal("\x1b[21m", TextStyle.DoubleUnderline.ToAnsi());

		[Fact]
		public void Overline_returns_correct_code() =>
			Assert.Equal("\x1b[53m", TextStyle.Overline.ToAnsi());

		[Fact]
		public void Bold_Italic_Underline_combined_sorted_ascending() =>
			Assert.Equal("\x1b[1;3;4m", (TextStyle.Bold | TextStyle.Italic | TextStyle.Underline).ToAnsi());

		[Fact]
		public void None_value_is_zero() =>
			Assert.Equal(0, (int)TextStyle.None);
	}

	public class Parse
	{
		[Fact]
		public void TextStyle_passthrough() =>
			Assert.Equal(TextStyle.Bold, TextStyleExtensions.Parse(TextStyle.Bold));

		[Theory]
		[InlineData("bold")]
		[InlineData("Bold")]
		[InlineData("BOLD")]
		public void String_case_insensitive(string input) =>
			Assert.Equal(TextStyle.Bold, TextStyleExtensions.Parse(input));

		[Fact]
		public void Comma_separated_string_combines_flags() =>
			Assert.Equal(TextStyle.Bold | TextStyle.Italic | TextStyle.Underline,
				TextStyleExtensions.Parse("Bold,Italic,Underline"));

		[Fact]
		public void Null_returns_None() =>
			Assert.Equal(TextStyle.None, TextStyleExtensions.Parse(null));

		[Fact]
		public void Unknown_parts_silently_ignored() =>
			Assert.Equal(TextStyle.Bold, TextStyleExtensions.Parse("Bold,Bogus"));

		[Fact]
		public void All_unknown_parts_return_None() =>
			Assert.Equal(TextStyle.None, TextStyleExtensions.Parse("Bogus,AlsoFake"));

		[Fact]
		public void Empty_string_returns_None() =>
			Assert.Equal(TextStyle.None, TextStyleExtensions.Parse(""));
	}
}
