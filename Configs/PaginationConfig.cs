using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Host;
using PwshPrompt.Consts;
using PwshPrompt.Enums;
using PwshPrompt.Types;

namespace PwshPrompt.Configs;

internal readonly struct PaginationConfig {
	private readonly Label? _item;
	private readonly Label? _selectedItem;
	private readonly Label? _prevPage;
	private readonly Label? _nextPage;
	private readonly Label? _totalPage;

	private static readonly Label _defaultItem       = new Label { Text = "○", Style = TextStyle.None };
	private static readonly Label _defaultSelectedItem = new Label { Text = "●", Style = TextStyle.None };
	private static readonly Label _defaultPrevPage   = new Label { Text = "", Style = TextStyle.None };
	private static readonly Label _defaultNextPage   = new Label { Text = "", Style = TextStyle.None };
	private static readonly Label _defaultTotalPage  = new Label { Text = "", Style = TextStyle.Dim };

	private PaginationConfig(Label? item, Label? selected_item, Label? prev_page, Label? next_page, Label? total_page) {
		_item = item;
		_selectedItem = selected_item;
		_prevPage = prev_page;
		_nextPage = next_page;
		_totalPage = total_page;
	}

	internal Label Item => _item ?? _defaultItem;
	internal Label SelectedItem => _selectedItem ?? _defaultSelectedItem;
	internal Label PrevPage => _prevPage ?? _defaultPrevPage;
	internal Label NextPage => _nextPage ?? _defaultNextPage;
	internal Label TotalPage => _totalPage ?? _defaultTotalPage;

	internal static PaginationConfig FromParameter(Hashtable? ht, string parameter_name, PSHostRawUserInterface rawUI) {
		if (ht is null)
			return new PaginationConfig(null, null, null, null, null);

		Label? item = null;
		Label? selected_item = null;
		Label? prev_page = null;
		Label? next_page = null;
		Label? total_page = null;

		foreach (DictionaryEntry entry in ht) {
			string? key = entry.Key.ToString();
			if (key is null) continue;

			if (string.Equals(key, "Item", StringComparison.OrdinalIgnoreCase))
				item = Label.FromParameter(rawUI, null, entry.Value, parameter_name);
			else if (string.Equals(key, "SelectedItem", StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(key, "selected", StringComparison.OrdinalIgnoreCase))
				selected_item = Label.FromParameter(rawUI, null, entry.Value, parameter_name);
			else if (string.Equals(key, "PrevPage", StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(key, "prev", StringComparison.OrdinalIgnoreCase))
				prev_page = Label.FromParameter(rawUI, null, entry.Value, parameter_name);
			else if (string.Equals(key, "NextPage", StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(key, "next", StringComparison.OrdinalIgnoreCase))
				next_page = Label.FromParameter(rawUI, null, entry.Value, parameter_name);
			else if (string.Equals(key, "TotalPage", StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(key, "total", StringComparison.OrdinalIgnoreCase))
				total_page = Label.FromParameter(rawUI, null, entry.Value, parameter_name);
			else
				throw new PSArgumentException(
					$"Unknown key '{key}' in {parameter_name} hashtable. Valid keys: Item, SelectedItem, PrevPage , NextPage, TotalPage",
					parameter_name);
		}

		return new PaginationConfig(item, selected_item, prev_page, next_page, total_page);
	}
}
