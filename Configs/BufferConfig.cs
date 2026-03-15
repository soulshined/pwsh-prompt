using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Host;
using PwshPrompt.Consts;
using PwshPrompt.Utils;

namespace PwshPrompt.Configs;

internal readonly struct BufferConfig
{
	internal AnsiColor ForegroundColor { get; }
	internal AnsiColor BackgroundColor { get; }
	internal BorderConfig Border { get; }
	internal ItemConfig Item { get; }
	internal PaginationConfig Pagination { get; }

	private BufferConfig(AnsiColor? fg, AnsiColor? bg, BorderConfig border, ItemConfig item, PaginationConfig pagination)
	{
		ForegroundColor = fg ?? ANSI.COLOR.BEIGE;
		BackgroundColor = bg ?? ANSI.COLOR.GREEN;
		Border = border;
		Item = item;
		Pagination = pagination;
	}

	internal static BufferConfig FromParameter(Hashtable? ht, string parameter_name, PSHostRawUserInterface rawUI)
	{
		BorderConfig border = BorderConfig.FromParameter(null, parameter_name);
		ItemConfig item = ItemConfig.FromParameter(null, parameter_name, rawUI);
		PaginationConfig pagination = PaginationConfig.FromParameter(null, parameter_name, rawUI);

		if (ht is null)
			return new BufferConfig(null, null, border, item, pagination);

		AnsiColor? fg = null;
		AnsiColor? bg = null;

		foreach (DictionaryEntry entry in ht)
		{
			string? key = entry.Key.ToString();
			if (key is null) continue;

			if (string.Equals(key, "ForegroundColor", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(key, "fg", StringComparison.OrdinalIgnoreCase))
				fg = AnsiColor.Parse(entry.Value, "ForegroundColor");
			else if (string.Equals(key, "BackgroundColor", StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(key, "bg", StringComparison.OrdinalIgnoreCase))
				bg = AnsiColor.Parse(entry.Value, "BackgroundColor");
			else if (string.Equals(key, "Border", StringComparison.OrdinalIgnoreCase))
				border = BorderConfig.FromParameter(entry.Value as Hashtable, parameter_name);
			else if (string.Equals(key, "item", StringComparison.OrdinalIgnoreCase))
				item = ItemConfig.FromParameter(entry.Value as Hashtable, parameter_name, rawUI);
			else if (string.Equals(key, "pagination", StringComparison.OrdinalIgnoreCase))
				pagination = PaginationConfig.FromParameter(entry.Value as Hashtable, parameter_name, rawUI);
			else throw new PSArgumentException(
					$"Unknown key '{key}' in {parameter_name} hashtable. Valid keys: ForegroundColor, BackgroundColor, Border, Item, Pagination",
					parameter_name);
		}

		return new BufferConfig(fg, bg, border, item, pagination);
	}

}
