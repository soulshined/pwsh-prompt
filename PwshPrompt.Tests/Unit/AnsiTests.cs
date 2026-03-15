using PwshPrompt.Consts;
using Xunit;

namespace PwshPrompt.Tests.Unit;

public class AnsiTests
{
	[Fact]
	public void Reset_is_correct_escape() =>
		Assert.Equal("\x1b[0m", ANSI.SEQUENCE.RESET);

	[Fact]
	public void MoveCursorUp_produces_correct_sequence() =>
		Assert.Equal("\x1b[3A", ANSI.SEQUENCE.MoveCursorUp(3));

	[Fact]
	public void MoveCursorDown_produces_correct_sequence() =>
		Assert.Equal("\x1b[2B", ANSI.SEQUENCE.MoveCursorDown(2));

	[Fact]
	public void MoveCursorTo_produces_correct_sequence() =>
		Assert.Equal("\x1b[5;10H", ANSI.SEQUENCE.MoveCursorTo(5, 10));

	[Fact]
	public void RepeatCharacter_produces_correct_sequence()
	{
		string result = ANSI.SEQUENCE.RepeatCharacter("─", 3);
		Assert.Equal("─\x1b[2b", result);
	}

	[Fact]
	public void Foreground_contains_38_marker() =>
		Assert.Contains("38", ANSI.COLOR.Foreground(ANSI.COLOR.BEIGE));

	[Fact]
	public void Background_contains_48_marker() =>
		Assert.Contains("48", ANSI.COLOR.Background(ANSI.COLOR.GREEN));

	[Fact]
	public void Beige_has_correct_index_and_rgb()
	{
		Assert.Equal("230", ANSI.COLOR.BEIGE.Index);
		Assert.Equal("189;179;149", ANSI.COLOR.BEIGE.RGB);
	}

	[Fact]
	public void Green_has_correct_index_and_rgb()
	{
		Assert.Equal("23", ANSI.COLOR.GREEN.Index);
		Assert.Equal("4;35;39", ANSI.COLOR.GREEN.RGB);
	}
}
