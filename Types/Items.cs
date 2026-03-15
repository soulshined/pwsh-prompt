using System.Collections;
using System.Management.Automation;
using PwshPrompt.Consts;
using PwshPrompt.Enums;
using PwshPrompt.Utils;

namespace PwshPrompt.Types;

internal record struct Item
{
	internal static readonly Dictionary<string, string[]> CONFIG_PROPS = new() {
		{ "Value", new string[0] },
		{ "Description", new string[1] { "desc" }},
		{ "HotKey", new string[1] { "hk" }},
		{ "HelpMessage", new string[1] { "help" }},
		{ "Style", new string[0] },
		{ "ForegroundColor", new string[1] { "fg" }},
		{ "BackgroundColor", new string[1] { "bg" }}
	};

	internal object Value { get; init; }
	internal string DisplayText { get; init; }
	internal string? Description { get; init; }
	internal char? HotKey { get; init; }
	internal string? HelpMessage { get; init; }
	internal TextStyle? Style { get; init; }
	internal AnsiColor? ForegroundColor { get; init; }
	internal AnsiColor? BackgroundColor { get; init; }

	private static string ResolveDisplayText(object? value)
	{
		return value?.ToString() ?? "";
	}

	internal static Item FromHashtable(Hashtable ht, int index, string parameterName)
	{
		object? val = null;
		string? description = null;
		char? hotkey = null;
		string? help = null;
		TextStyle? style = null;
		AnsiColor? fg = null;
		AnsiColor? bg = null;

		foreach (DictionaryEntry entry in ht)
		{
			string? key = entry.Key.ToString();
			if (key is null) continue;

			if (string.Equals(key, "Value", StringComparison.OrdinalIgnoreCase) ||
				key.EqualsAny(CONFIG_PROPS["Value"]))
				val = entry.Value;
			else if (string.Equals(key, "Description", StringComparison.OrdinalIgnoreCase) ||
					key.EqualsAny(CONFIG_PROPS["Description"]))
				description = entry.Value as string;
			else if (string.Equals(key, "HotKey", StringComparison.OrdinalIgnoreCase) ||
					key.EqualsAny(CONFIG_PROPS["HotKey"]))
				hotkey = entry.Value is string s && s.Length == 1 ? s[0] : entry.Value as char?;
			else if (string.Equals(key, "HelpMessage", StringComparison.OrdinalIgnoreCase) ||
					key.EqualsAny(CONFIG_PROPS["HelpMessage"]))
				help = entry.Value as string;
			else if (string.Equals(key, "Style", StringComparison.OrdinalIgnoreCase) ||
					key.EqualsAny(CONFIG_PROPS["Style"]))
				style = TextStyleExtensions.Parse(entry.Value);
			else if (string.Equals(key, "ForegroundColor", StringComparison.OrdinalIgnoreCase) ||
					key.EqualsAny(CONFIG_PROPS["ForegroundColor"]))
				fg = AnsiColor.Parse(entry.Value, "ForegroundColor");
			else if (string.Equals(key, "BackgroundColor", StringComparison.OrdinalIgnoreCase) ||
					key.EqualsAny(CONFIG_PROPS["BackgroundColor"]))
				bg = AnsiColor.Parse(entry.Value, "BackgroundColor");
			else
				throw new PSArgumentException(
					$"Unknown key '{key}' in choices hashtable at index {index}. Valid keys: {string.Join(", ", CONFIG_PROPS.Keys)}", parameterName);
		}

		if (val is null || (val is string str && string.IsNullOrWhiteSpace(str)))
			throw new PSArgumentException(
				$"Choices hashtable at index {index} is missing the required 'Value' key", parameterName);

		return new Item {
			Value = val,
			DisplayText = ResolveDisplayText(val),
			Description = description,
			HotKey = hotkey,
			HelpMessage = help,
			Style = style,
			ForegroundColor = fg,
			BackgroundColor = bg
		};
	}
}

internal readonly struct Items : IReadOnlyList<Item>
{
	private readonly Item[] _items;

	public int Count => _items.Length;
	public Item this[int index] => _items[index];

	private Items(Item[] items)
	{
		_items = items;
	}

	/// <summary>
	/// Validates and converts a raw parameter array into an <see cref="Items"/> instance.
	/// All elements must be the same type, or a hashtable.  Hashtable elements are validated against the
	/// <see cref="Item"/> key structure and converted. Non-hashtable elements are
	/// wrapped in an Item with only Value set.
	/// </summary>
	/// <param name="choices">The raw parameter array.</param>
	/// <param name="parameterName">The parameter name for error messages.</param>
	/// <returns>An <see cref="Items"/> instance containing validated <see cref="Item"/> entries.</returns>
	/// <exception cref="PSArgumentException">
	/// Thrown when elements are mixed types, a hashtable has unknown keys,
	/// or a hashtable is missing the required 'Value' key.
	/// </exception>
	internal static Items FromParameter(object[] choices, string parameterName)
	{
		Type? element_type = null;
		Item[] items = new Item[choices.Length];

		for (int i = 0; i < choices.Length; i++)
		{
			object item = choices[i] is PSObject pso ? pso.BaseObject : choices[i];

			if (item is Hashtable ht)
			{
				items[i] = Item.FromHashtable(ht, i, parameterName);
			}
			else
			{
				Type item_type = item.GetType();
				if (element_type is not null && element_type != item_type)
					throw new PSArgumentException(
						$"All choices must be the same type. Expected '{element_type.Name}' but found '{item_type.Name}' at index {i}", parameterName);

				element_type = item_type;
				items[i] = new Item { Value = item, DisplayText = item.ToString() ?? "" };
			}
		}

		return new Items(items);
	}

	public IEnumerator<Item> GetEnumerator() => ((IEnumerable<Item>)_items).GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
