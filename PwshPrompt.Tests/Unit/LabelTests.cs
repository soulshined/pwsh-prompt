using System.Collections;
using System.Management.Automation;
using PwshPrompt.Consts;
using PwshPrompt.Configs;
using PwshPrompt.Enums;
using PwshPrompt.Tests.Helpers;
using PwshPrompt.Types;
using Xunit;

namespace PwshPrompt.Tests.Unit;

public class LabelTests
{
	private static TestPSHostRawUI DefaultRawUI() => new();

	// ConsoleColor names round-trip through ParseColorArray correctly.
	// Numeric 256-color indices (e.g. "230") get caught by Enum.TryParse<ConsoleColor>
	// and mapped to the default branch ("7"), so only named ConsoleColors work here.
	private static BufferConfig MakeAppearance()
	{
		TestPSHostRawUI rawUI = DefaultRawUI();
		System.Collections.Hashtable ht = new System.Collections.Hashtable
		{
			["ForegroundColor"] = new object[] { "Red", "Red" },
			["BackgroundColor"] = new object[] { "Blue", "Blue" }
		};
		return BufferConfig.FromParameter(ht, "test", rawUI);
	}

	private static AnsiColor AppearanceFg() => AnsiColor.From(ConsoleColor.Red);
	private static AnsiColor AppearanceBg() => AnsiColor.From(ConsoleColor.Blue);

	public class StringCtor
	{
		[Fact]
		public void Text_is_set()
		{
			Label label = new Label(DefaultRawUI(), null, "hello");
			Assert.Equal("hello", label.Text);
		}

		[Fact]
		public void ForegroundColor_from_rawUI_when_no_buffer_config()
		{
			TestPSHostRawUI rawUI = DefaultRawUI(); // ForegroundColor = Gray
			Label label = new Label(rawUI, null, "hello");
			Assert.Equal(AnsiColor.From(ConsoleColor.Gray), label.ForegroundColor);
		}

		[Fact]
		public void BackgroundColor_from_rawUI_when_no_buffer_config()
		{
			TestPSHostRawUI rawUI = DefaultRawUI(); // BackgroundColor = Black
			Label label = new Label(rawUI, null, "hello");
			Assert.Equal(AnsiColor.From(ConsoleColor.Black), label.BackgroundColor);
		}

		[Fact]
		public void ForegroundColor_is_null_when_rawUI_fg_is_negative()
		{
			TestPSHostRawUI rawUI = DefaultRawUI();
			rawUI.ForegroundColor = (ConsoleColor)(-1);
			Label label = new Label(rawUI, null, "hello");
			Assert.Null(label.ForegroundColor);
		}

		[Fact]
		public void BackgroundColor_is_null_when_rawUI_bg_is_negative()
		{
			TestPSHostRawUI rawUI = DefaultRawUI();
			rawUI.BackgroundColor = (ConsoleColor)(-1);
			Label label = new Label(rawUI, null, "hello");
			Assert.Null(label.BackgroundColor);
		}

		[Fact]
		public void Default_style_is_None()
		{
			Label label = new Label(DefaultRawUI(), null, "hello");
			Assert.Equal(TextStyle.None, label.Style);
		}

		[Fact]
		public void ForegroundColor_from_buffer_config_overrides_rawUI()
		{
			TestPSHostRawUI rawUI = DefaultRawUI();
			BufferConfig appearance = MakeAppearance();
			Label label = new Label(rawUI, appearance, "hello");
			Assert.Equal(AppearanceFg(), label.ForegroundColor);
		}

		[Fact]
		public void BackgroundColor_from_buffer_config_overrides_rawUI()
		{
			TestPSHostRawUI rawUI = DefaultRawUI();
			BufferConfig appearance = MakeAppearance();
			Label label = new Label(rawUI, appearance, "hello");
			Assert.Equal(AppearanceBg(), label.BackgroundColor);
		}
	}

	public class HashtableCtor
	{
		[Fact]
		public void Text_is_set_from_hashtable()
		{
			Hashtable ht = new Hashtable { ["Text"] = "world" };
			Label label = new Label(DefaultRawUI(), null, ht);
			Assert.Equal("world", label.Text);
		}

		[Fact]
		public void ForegroundColor_defaults_to_rawUI_when_absent()
		{
			TestPSHostRawUI rawUI = DefaultRawUI();
			Hashtable ht = new Hashtable { ["Text"] = "x" };
			Label label = new Label(rawUI, null, ht);
			Assert.Equal(AnsiColor.From(ConsoleColor.Gray), label.ForegroundColor);
		}

		[Fact]
		public void Style_defaults_to_defaultStyle_arg_when_absent()
		{
			Hashtable ht = new Hashtable { ["Text"] = "x" };
			Label label = new Label(DefaultRawUI(), null, ht, TextStyle.Bold);
			Assert.Equal(TextStyle.Bold, label.Style);
		}

		[Fact]
		public void Style_key_overrides_defaultStyle()
		{
			Hashtable ht = new Hashtable { ["Text"] = "x", ["Style"] = "Italic" };
			Label label = new Label(DefaultRawUI(), null, ht, TextStyle.Bold);
			Assert.Equal(TextStyle.Italic, label.Style);
		}

		[Fact]
		public void ForegroundColor_key_overrides_rawUI()
		{
			Hashtable ht = new Hashtable
			{
				["Text"] = "x",
				["ForegroundColor"] = new object[] { "Red", "Red" }
			};
			Label label = new Label(DefaultRawUI(), null, ht);
			Assert.Equal(AnsiColor.From(ConsoleColor.Red), label.ForegroundColor);
		}

		[Fact]
		public void Fg_short_key_accepted()
		{
			Hashtable ht = new Hashtable
			{
				["Text"] = "x",
				["fg"] = new object[] { "Red", "Red" }
			};
			Label label = new Label(DefaultRawUI(), null, ht);
			Assert.Equal(AnsiColor.From(ConsoleColor.Red), label.ForegroundColor);
		}

		[Fact]
		public void Missing_Text_throws_PSArgumentException()
		{
			Hashtable ht = new Hashtable { ["Style"] = "Bold" };
			Assert.Throws<PSArgumentException>(() => new Label(DefaultRawUI(), null, ht));
		}
	}

	public class FromParameter
	{
		[Fact]
		public void String_input_returns_label()
		{
			Label label = Label.FromParameter(DefaultRawUI(), null, "hello", "param");
			Assert.Equal("hello", label.Text);
		}

		[Fact]
		public void Hashtable_input_returns_label()
		{
			Hashtable ht = new Hashtable { ["Text"] = "hi" };
			Label label = Label.FromParameter(DefaultRawUI(), null, ht, "param");
			Assert.Equal("hi", label.Text);
		}

		[Fact]
		public void Integer_throws_PSArgumentException() =>
			Assert.Throws<PSArgumentException>(() => Label.FromParameter(DefaultRawUI(), null, 42, "param"));

		[Fact]
		public void Null_throws_PSArgumentException() =>
			Assert.Throws<PSArgumentException>(() => Label.FromParameter(DefaultRawUI(), null, null, "param"));
	}

	public class ToStringMethod
	{
		[Fact]
		public void Contains_reset_suffix()
		{
			Label label = new Label(DefaultRawUI(), null, "test");
			Assert.Contains(ANSI.SEQUENCE.RESET, label.ToString());
		}

		[Fact]
		public void Long_text_truncated_with_ellipsis()
		{
			Label label = new Label(DefaultRawUI(), null, "this is a very long text string");
			string result = label.ToString(10);
			Assert.EndsWith("\u2026" + ANSI.SEQUENCE.RESET, result);
		}

		[Fact]
		public void Short_text_not_truncated()
		{
			Label label = new Label(DefaultRawUI(), null, "hi");
			Assert.Contains("hi", label.ToString(100));
		}

		[Fact]
		public void No_foreground_color_omits_fg_escape()
		{
			TestPSHostRawUI rawUI = DefaultRawUI();
			rawUI.ForegroundColor = (ConsoleColor)(-1);
			Label label = new Label(rawUI, null, "test");
			Assert.DoesNotContain("\x1b[38;", label.ToString());
		}

		[Fact]
		public void No_background_color_omits_bg_escape()
		{
			TestPSHostRawUI rawUI = DefaultRawUI();
			rawUI.BackgroundColor = (ConsoleColor)(-1);
			Label label = new Label(rawUI, null, "test");
			Assert.DoesNotContain("\x1b[48;", label.ToString());
		}

		[Fact]
		public void MaxValue_does_not_truncate()
		{
			Label label = new Label(DefaultRawUI(), null, "a very long piece of text indeed");
			Assert.Contains("a very long piece of text indeed", label.ToString(int.MaxValue));
		}
	}
}
