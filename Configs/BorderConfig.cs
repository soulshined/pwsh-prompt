using System.Collections;
using System.Management.Automation;
using PwshPrompt.Consts;
using PwshPrompt.Utils;

namespace PwshPrompt.Configs;

internal readonly struct BorderConfig
{
	private static readonly string TL_DEFAULT = "┌";
	private static readonly string TR_DEFAULT = "┐";
	private static readonly string BL_DEFAULT = "└";
	private static readonly string BR_DEFAULT = "┘";
	private static readonly string HS_DEFAULT = "─";
	private static readonly string VS_DEFAULT = " ";

	internal string HorizontalSide { get; }
	internal string VerticalSide { get; }
	internal string TopLeft { get; }
	internal string TopRight { get; }
	internal string BottomLeft { get; }
	internal string BottomRight { get; }
	internal AnsiColor Color { get; }

	private BorderConfig(
		string horizontal_side, string vertical_side,
		string top_left, string top_right,
		string bottom_left, string bottom_right,
		AnsiColor color)
	{
		HorizontalSide = horizontal_side;
		VerticalSide = vertical_side;
		TopLeft = top_left;
		TopRight = top_right;
		BottomLeft = bottom_left;
		BottomRight = bottom_right;
		Color = color;
	}

	internal static BorderConfig FromParameter(Hashtable? ht, string parameter_name)
	{
		string horizontal_side = HS_DEFAULT;
		string vertical_side = VS_DEFAULT;
		string top_left = TL_DEFAULT;
		string top_right = TR_DEFAULT;
		string bottom_left = BL_DEFAULT;
		string bottom_right = BR_DEFAULT;
		AnsiColor color = ANSI.COLOR.BEIGE;

		if (ht is null)
			return new BorderConfig(horizontal_side, vertical_side, top_left, top_right, bottom_left, bottom_right, color);

		foreach (DictionaryEntry entry in ht)
		{
			string? key = entry.Key.ToString();
			if (key is null) continue;

			string val = entry.Value?.ToString()?.Trim() ?? "";
			if (val.Length == 0) continue;

			if (string.Equals(key, "HorizontalSide", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(key, "hs", StringComparison.OrdinalIgnoreCase))
				horizontal_side = val;
			else if (string.Equals(key, "VerticalSide", StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(key, "vs", StringComparison.OrdinalIgnoreCase))
				vertical_side = val;
			else if (string.Equals(key, "TopLeft", StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(key, "tl", StringComparison.OrdinalIgnoreCase))
				top_left = val;
			else if (string.Equals(key, "TopRight", StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(key, "tr", StringComparison.OrdinalIgnoreCase))
				top_right = val;
			else if (string.Equals(key, "BottomLeft", StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(key, "bl", StringComparison.OrdinalIgnoreCase))
				bottom_left = val;
			else if (string.Equals(key, "BottomRight", StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(key, "br", StringComparison.OrdinalIgnoreCase))
				bottom_right = val;
			else if (string.Equals(key, "Color", StringComparison.OrdinalIgnoreCase)) {
				color = AnsiColor.Parse(entry.Value, "Color");
			} else
				throw new PSArgumentException(
					$"Unknown key '{key}' in Border hashtable. Valid keys: HorizontalSide (hs), VerticalSide (vs), TopLeft (tl), TopRight (tr), BottomLeft (bl), BottomRight (br), Color",
					parameter_name);
		}

		return new(horizontal_side, vertical_side, top_left, top_right, bottom_left, bottom_right, color);
	}

}
