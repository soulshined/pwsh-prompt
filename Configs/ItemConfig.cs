using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Host;
using PwshPrompt.Consts;
using PwshPrompt.Enums;
using PwshPrompt.Types;

namespace PwshPrompt.Configs;

internal readonly struct ItemConfig {
	private readonly Label? _item;
	private readonly Label? _selectedItem;
	private readonly ToggleItem? _multipleIndicator;
	private readonly ToggleItem? _selectedMultipleIndicator;

	private static readonly Label _defaultItem           = new Label { Text = string.Empty, ForegroundColor = ANSI.COLOR.BEIGE, BackgroundColor = ANSI.COLOR.GREEN, Style = TextStyle.None };
	private static readonly Label _defaultSelectedItem   = new Label { Text = string.Empty, ForegroundColor = ANSI.COLOR.BEIGE, BackgroundColor = ANSI.COLOR.GREEN, Style = TextStyle.Reverse };
	private static readonly ToggleItem _defaultMultipleIndicator = new ToggleItem {
		Enabled = new Label { Text = "◉", ForegroundColor = ANSI.COLOR.BEIGE, BackgroundColor = ANSI.COLOR.GREEN, Style = TextStyle.None },
		Disabled = new Label { Text = "○", ForegroundColor = ANSI.COLOR.BEIGE, BackgroundColor = ANSI.COLOR.GREEN, Style = TextStyle.None }
	};
	private static readonly ToggleItem _defaultSelectedMultipleIndicator = new ToggleItem {
		Enabled = new Label { Text = "◉", ForegroundColor = ANSI.COLOR.BEIGE, BackgroundColor = ANSI.COLOR.GREEN, Style = TextStyle.None },
		Disabled = new Label { Text = "○", ForegroundColor = ANSI.COLOR.BEIGE, BackgroundColor = ANSI.COLOR.GREEN, Style = TextStyle.None }
	};

	private ItemConfig(Label? item, Label? selected_item, ToggleItem? multiple_indicator, ToggleItem? selected_multiple_indicator) {
		_item = item;
		_selectedItem = selected_item;
		_multipleIndicator = multiple_indicator;
		_selectedMultipleIndicator = selected_multiple_indicator;
	}

	internal Label Item => _item ?? _defaultItem;
	internal Label SelectedItem => _selectedItem ?? _defaultSelectedItem;
	internal ToggleItem multipleIndicator => _multipleIndicator ?? _defaultMultipleIndicator;
	internal ToggleItem selectedMultipleIndicator => _selectedMultipleIndicator ?? _defaultSelectedMultipleIndicator;

	internal static ItemConfig FromParameter(Hashtable? ht, string parameter_name, PSHostRawUserInterface rawUI) {
		if (ht is null)
			return new ItemConfig(null, null, null, null);

		Label? item = null;
		Label? selected_item = null;
		ToggleItem? multiple_indicator = null;
		ToggleItem? selected_multiple_indicator = null;

		foreach (DictionaryEntry entry in ht) {
			string? key = entry.Key.ToString();
			if (key is null) continue;

			if (string.Equals(key, "Item", StringComparison.OrdinalIgnoreCase))
				item = Label.FromParameter(rawUI, null, entry.Value, parameter_name);
			else if (string.Equals(key, "selected", StringComparison.OrdinalIgnoreCase))
				selected_item = Label.FromParameter(rawUI, null, entry.Value, parameter_name);
			else if (string.Equals(key, "multipleIndicator", StringComparison.OrdinalIgnoreCase))
				multiple_indicator = ToggleItem.FromParameter(entry.Value as Hashtable, parameter_name, rawUI);
			else if (string.Equals(key, "selectedMultipleIndicator", StringComparison.OrdinalIgnoreCase))
				selected_multiple_indicator = ToggleItem.FromParameter(entry.Value as Hashtable, parameter_name, rawUI);
			else
				throw new PSArgumentException(
					$"Unknown key '{key}' in {parameter_name} hashtable. Valid keys: Item, SelectedItem (selected), multipleIndicator, selectedMultipleIndicator",
					parameter_name);
		}

		return new ItemConfig(item, selected_item, multiple_indicator, selected_multiple_indicator);
	}
}
