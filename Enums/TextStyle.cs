using System.Text;

namespace PwshPrompt.Enums;

/// <summary>ANSI text-decoration flags (combinable via bitwise OR).</summary>
[Flags]
public enum TextStyle
{
	/// <summary>No styling.</summary>
	None            = 0,
	/// <summary>SGR 1 — bold / increased intensity.</summary>
	Bold            = 1 << 0,
	/// <summary>SGR 2 — dim / faint.</summary>
	Dim             = 1 << 1,
	/// <summary>SGR 3 — italic.</summary>
	Italic          = 1 << 2,
	/// <summary>SGR 4 — underline.</summary>
	Underline       = 1 << 3,
	/// <summary>SGR 5 — slow blink.</summary>
	SlowBlink       = 1 << 4,
	/// <summary>SGR 6 — rapid blink.</summary>
	RapidBlink      = 1 << 5,
	/// <summary>SGR 7 — reverse video.</summary>
	Reverse         = 1 << 6,
	/// <summary>SGR 8 — hidden / conceal.</summary>
	Hidden          = 1 << 7,
	/// <summary>SGR 9 — strikethrough.</summary>
	Strikethrough   = 1 << 8,
	/// <summary>SGR 21 — double underline.</summary>
	DoubleUnderline = 1 << 9,
	/// <summary>SGR 53 — overline.</summary>
	Overline        = 1 << 10
}

internal static class TextStyleExtensions
{
	private static readonly Dictionary<TextStyle, string> _cache = new();

	private static readonly (TextStyle Flag, int Code)[] AnsiCodes =
	[
		(TextStyle.Bold, 1),
		(TextStyle.Dim, 2),
		(TextStyle.Italic, 3),
		(TextStyle.Underline, 4),
		(TextStyle.SlowBlink, 5),
		(TextStyle.RapidBlink, 6),
		(TextStyle.Reverse, 7),
		(TextStyle.Hidden, 8),
		(TextStyle.Strikethrough, 9),
		(TextStyle.DoubleUnderline, 21),
		(TextStyle.Overline, 53)
	];

	internal static string ToAnsi(this TextStyle style)
	{
		if (style == TextStyle.None) return string.Empty;

		if (_cache.TryGetValue(style, out string? cached))
			return cached;

		Span<int> codes = stackalloc int[11];
		int count = 0;
		foreach ((TextStyle flag, int code) in AnsiCodes)
		{
			if (style.HasFlag(flag))
				codes[count++] = code;
		}

		StringBuilder sb = new(16);
		sb.Append("\x1b[");
		for (int i = 0; i < count; i++)
		{
			if (i > 0) sb.Append(';');
			sb.Append(codes[i]);
		}
		sb.Append('m');

		string result = sb.ToString();
		_cache[style] = result;
		return result;
	}

	internal static TextStyle Parse(object? value)
	{
		return value switch
		{
			TextStyle s => s,
			string str => ParseString(str),
			_ => TextStyle.None
		};
	}

	private static TextStyle ParseString(string input)
	{
		TextStyle result = TextStyle.None;
		foreach (string part in input.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
		{
			if (Enum.TryParse<TextStyle>(part, ignoreCase: true, out TextStyle parsed))
				result |= parsed;
		}
		return result;
	}
}
