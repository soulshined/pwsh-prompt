using System.Management.Automation;

namespace PwshPrompt.Consts;

internal readonly record struct AnsiColor(string Index, string RGB)
{
	internal string Fg { get; } = ANSI.Supports24bit()
		? $"\x1b[38;2;{RGB}m"
		: $"\x1b[38;5;{Index}m";
	internal string Bg { get; } = ANSI.Supports24bit()
		? $"\x1b[48;2;{RGB}m"
		: $"\x1b[48;5;{Index}m";

	internal static AnsiColor From(ConsoleColor color) =>
		new(ConsoleColorTo256Index(color), ConsoleColorToRgb(color));

	internal static ConsoleColor? ParseConsoleColor(object? value)
	{
		return value switch
		{
			ConsoleColor color => color,
			string str when Enum.TryParse<ConsoleColor>(str, ignoreCase: true, out ConsoleColor parsed) => parsed,
			_ => null
		};
	}

	internal static AnsiColor Parse(object? value, string key)
	{
		string? s0 = null, s1 = null;
		if (value is string[] { Length: 2 } sa)
			(s0, s1) = (sa[0], sa[1]);
		else if (value is object[] { Length: 2 } oa && oa[0] is string o0 && oa[1] is string o1)
			(s0, s1) = (o0, o1);

		if (s0 == null || s1 == null)
			throw new PSArgumentException(
				$"'{key}' must be a 2-element string array: @(\"256-index | ConsoleColor\", \"r;g;b | ConsoleColor\").",
				key);

		string resolved_index = ResolveIndex(s0, key);
		string resolved_rgb = ResolveRgb(s1, key);

		return new AnsiColor(resolved_index, resolved_rgb);
	}

	private static string ResolveIndex(string s, string key)
	{
		if (int.TryParse(s, out int n) && n >= 0 && n <= 255)
			return s;
		if (ParseConsoleColor(s) is ConsoleColor color)
			return ConsoleColorTo256Index(color);
		throw new PSArgumentException(
			$"'{key}[0]' must be a 256-color index (0–255) or a ConsoleColor name.", key);
	}

	private static string ResolveRgb(string s, string key)
	{
		if (ParseConsoleColor(s) is ConsoleColor color)
			return ConsoleColorToRgb(color);
		string[] parts = s.Split(';');
		if (parts.Length == 3 &&
			int.TryParse(parts[0], out int r) && r >= 0 && r <= 255 &&
			int.TryParse(parts[1], out int g) && g >= 0 && g <= 255 &&
			int.TryParse(parts[2], out int b) && b >= 0 && b <= 255)
			return s;
		throw new PSArgumentException(
			$"'{key}[1]' must be an RGB string (r;g;b) or a ConsoleColor name.", key);
	}

	private static string ConsoleColorTo256Index(ConsoleColor color) => color switch
	{
		ConsoleColor.Black       => "0",
		ConsoleColor.DarkRed     => "1",
		ConsoleColor.DarkGreen   => "2",
		ConsoleColor.DarkYellow  => "3",
		ConsoleColor.DarkBlue    => "4",
		ConsoleColor.DarkMagenta => "5",
		ConsoleColor.DarkCyan    => "6",
		ConsoleColor.Gray        => "7",
		ConsoleColor.DarkGray    => "8",
		ConsoleColor.Red         => "9",
		ConsoleColor.Green       => "10",
		ConsoleColor.Yellow      => "11",
		ConsoleColor.Blue        => "12",
		ConsoleColor.Magenta     => "13",
		ConsoleColor.Cyan        => "14",
		ConsoleColor.White       => "15",
		_                        => "7",
	};

	private static string ConsoleColorToRgb(ConsoleColor color) => color switch
	{
		ConsoleColor.Black       => "0;0;0",
		ConsoleColor.DarkRed     => "128;0;0",
		ConsoleColor.DarkGreen   => "0;128;0",
		ConsoleColor.DarkYellow  => "128;128;0",
		ConsoleColor.DarkBlue    => "0;0;128",
		ConsoleColor.DarkMagenta => "128;0;128",
		ConsoleColor.DarkCyan    => "0;128;128",
		ConsoleColor.Gray        => "192;192;192",
		ConsoleColor.DarkGray    => "128;128;128",
		ConsoleColor.Red         => "255;0;0",
		ConsoleColor.Green       => "0;255;0",
		ConsoleColor.Yellow      => "255;255;0",
		ConsoleColor.Blue        => "0;0;255",
		ConsoleColor.Magenta     => "255;0;255",
		ConsoleColor.Cyan        => "0;255;255",
		ConsoleColor.White       => "255;255;255",
		_                        => "192;192;192",
	};
}

internal sealed class ANSI {
	private static readonly bool _supports24bit = Environment.GetEnvironmentVariable("COLORTERM") == "truecolor" ||
												Environment.GetEnvironmentVariable("COLORTERM") == "24bit";

	internal static bool Supports24bit() {
		return _supports24bit;
	}

	internal sealed class SEQUENCE {
		internal static readonly string RESET = "\x1b[0m";
		internal static readonly string DIM = "\x1b[2m";
		internal static readonly string ITALIC = "\x1b[3m";
		internal static readonly string RESET_ITALIC = "\x1b[23m";
		internal static readonly string UNDERLINE = "\x1b[4m";
		internal static readonly string RESET_UNDERLINE = "\x1b[24m";
		internal static readonly string REVERSE_VIDEO = "\x1b[7m";
		internal static readonly string RESET_REVERSE_VIDEO = "\x1b[27m";
		internal static readonly string RESET_FOREGROUND_COLOR = "\x1b[39m";
		internal static readonly string RESET_BACKGROUND_COLOR = "\x1b[49m";
		internal static readonly string RESET_BOLD_DIM = "\x1b[22m";
		internal static readonly string ENABLE_ALTERNATE_SCREEN_WITH_CURSOR = "\x1b[?1049h";
		internal static readonly string DISABLE_ALTERNATE_SCREEN_WITH_CURSOR = "\x1b[?1049l";
		internal static readonly string ERASE_ENTIRE_SCREEN = "\x1b[2J";
		internal static readonly string ERASE_ENTIRE_LINE = "\x1b[2K";
		internal static readonly string ENABLE_CURSOR = "\x1b[?25h";
		internal static readonly string DISABLE_CURSOR = "\x1b[?25l";
		internal static readonly string MOVE_CURSOR_HOME = "\x1b[H";

		internal static string MoveCursorDown(int rows) {
			return $"\x1b[{rows}B";
		}

		internal static string MoveCursorUp(int rows) {
			return $"\x1b[{rows}A";
		}

		internal static string MoveCursorTo(int row, int col) {
			return $"\x1b[{row};{col}H";
		}

		internal static string RepeatCharacter(string ch, int times) {
			if (times <= 0) return string.Empty;
			if (times == 1) return ch;
			return $"{ch}\x1b[{times-1}b";
		}
	}

	internal sealed class COLOR {
		internal static readonly AnsiColor BEIGE       = new("230", "189;179;149");
		internal static readonly AnsiColor SNOW        = new("37",  "250;250;250");
		internal static readonly AnsiColor GREEN       = new("23",  "4;35;39");
		internal static readonly AnsiColor GRAY_LIGHT  = new("2",   "153;153;153");
		internal static readonly AnsiColor DROP_SHADOW = new("30",  "20;20;20");

		internal static string Foreground(AnsiColor color) => color.Fg;
		internal static string Background(AnsiColor color) => color.Bg;
	}
}
