using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Host;

using PwshPrompt.Utils;

namespace PwshPrompt.Types;

internal record struct ToggleItem {
	private static readonly Dictionary<string, string[]> CONFIG_PROPS = new () {
		{ "Enabled", new string[2] {"on", "checked"}},
		{ "Disabled", new string[2] {"off", "unchecked"}}
	};

	internal Label Enabled { get; init; }
	internal Label Disabled { get; init; }

	internal static ToggleItem FromParameter(Hashtable? hashtable, string parameter_name, PSHostRawUserInterface rawUI) {
		Label enabled = new Label(rawUI, null, "◉");
		Label disabled = new Label(rawUI, null, "○");

		if (hashtable is null)
			return new ToggleItem { Enabled = enabled, Disabled = disabled };

		foreach (DictionaryEntry entry in hashtable) {
			string? key = entry.Key.ToString();
			if (key is null) continue;

			if (string.Equals(key, "Enabled", StringComparison.OrdinalIgnoreCase) || key.EqualsAny(CONFIG_PROPS["Enabled"]))
				enabled = Label.FromParameter(rawUI, null, entry.Value, parameter_name);
			else if (string.Equals(key, "Disabled", StringComparison.OrdinalIgnoreCase) || key.EqualsAny(CONFIG_PROPS["Disabled"]))
				disabled = Label.FromParameter(rawUI, null, entry.Value, parameter_name);
			else
				throw new PSArgumentException(
					$"Unknown key '{key}' in {parameter_name} hashtable. Valid keys: ${string.Join(", ", CONFIG_PROPS.Keys)}",
					parameter_name);
		}

		return new ToggleItem { Enabled = enabled, Disabled = disabled };
	}
}
