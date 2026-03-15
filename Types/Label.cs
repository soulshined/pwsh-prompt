using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;
using PwshPrompt.Configs;
using PwshPrompt.Consts;
using PwshPrompt.Enums;
using PwshPrompt.Utils;

namespace PwshPrompt.Types;

/// <summary>
/// <para>A styled-text unit combining text content, foreground/background colors, and text decoration.</para>
///
/// <para>Accepts a plain <c>[string]</c> or a <c>[hashtable]</c> with the following keys (case-insensitive):</para>
/// <list type="bullet">
/// <item><description><c>Text</c> (<c>[string]</c>, required) — the display text.</description></item>
/// <item><description><c>ForegroundColor</c> (alias <c>fg</c>, <c>[string, string]</c>) — foreground color tuple.</description></item>
/// <item><description><c>BackgroundColor</c> (alias <c>bg</c>, <c>[string, string]</c>) — background color tuple.</description></item>
/// <item><description><c>Style</c> (<c>[string]</c>) — comma-separated <see cref="TextStyle"/> flags.</description></item>
/// </list>
/// </summary>
internal record struct Label
{
	internal string Text { get; init; }
	internal AnsiColor? ForegroundColor { get; set; }
	internal AnsiColor? BackgroundColor { get; set; }
	internal TextStyle Style { get; set; }

	internal Label(PSHostRawUserInterface rawUI, BufferConfig? buffer_config, string text, TextStyle style = TextStyle.None)
	{
		Text = text;
		Style = style;

		if (buffer_config.HasValue)  {
			ForegroundColor = buffer_config.Value.ForegroundColor;
			BackgroundColor = buffer_config.Value.BackgroundColor;
		}
		else {
			ForegroundColor = (int)rawUI.ForegroundColor >= 0 ? AnsiColor.From(rawUI.ForegroundColor) : null;
			BackgroundColor = (int)rawUI.BackgroundColor >= 0 ? AnsiColor.From(rawUI.BackgroundColor) : null;
		}
	}

	internal Label(PSHostRawUserInterface rawUI, BufferConfig? buffer_config, Hashtable properties, TextStyle defaultStyle = TextStyle.None)
	{
		string? text = null;
		TextStyle? style = null;

		AnsiColor? foreground_color = buffer_config.HasValue ? buffer_config.Value.ForegroundColor : ((int)rawUI.ForegroundColor >= 0 ? AnsiColor.From(rawUI.ForegroundColor) : null);
		AnsiColor? background_color = buffer_config.HasValue ? buffer_config.Value.BackgroundColor : ((int)rawUI.BackgroundColor >= 0 ? AnsiColor.From(rawUI.BackgroundColor) : null);

		foreach (DictionaryEntry entry in properties)
		{
			string? key = entry.Key.ToString();
			if (string.Equals(key, "Text", StringComparison.OrdinalIgnoreCase))
				text = entry.Value as string;
			else if (string.Equals(key, "ForegroundColor", StringComparison.OrdinalIgnoreCase) || string.Equals(key, "fg", StringComparison.OrdinalIgnoreCase))
				foreground_color = AnsiColor.Parse(entry.Value, "ForegroundColor");
			else if (string.Equals(key, "BackgroundColor", StringComparison.OrdinalIgnoreCase) || string.Equals(key, "bg", StringComparison.OrdinalIgnoreCase))
				background_color = AnsiColor.Parse(entry.Value, "BackgroundColor");
			else if (string.Equals(key, "Style", StringComparison.OrdinalIgnoreCase))
				style = TextStyleExtensions.Parse(entry.Value);
		}

		Text = text ?? throw new PSArgumentException("Text property is required.", nameof(properties));
		ForegroundColor = foreground_color;
		BackgroundColor = background_color;
		Style = style ?? defaultStyle;
	}

	public string ToString(int max_length = int.MaxValue)
	{
		StringBuilder sb = new();
		if (BackgroundColor.HasValue)
			sb.Append(ANSI.COLOR.Background(BackgroundColor.Value));
		if (ForegroundColor.HasValue)
			sb.Append(ANSI.COLOR.Foreground(ForegroundColor.Value));
		sb.Append(Style.ToAnsi())
		  .Append(Text.TruncateIf(max_length))
		  .Append(ANSI.SEQUENCE.RESET);

		return sb.ToString();
	}

	internal static Label FromParameter(PSHostRawUserInterface rawUI, BufferConfig? buffer_config, object? value, string parameterName, TextStyle defaultStyle = TextStyle.None)
	{
		return value switch
		{
			string text => new Label(rawUI, buffer_config.HasValue ? buffer_config : null, text, defaultStyle),
			Hashtable hashtable => new Label(rawUI, buffer_config, hashtable, defaultStyle),
			_ => throw new PSArgumentException(
				$"{parameterName} must be a string or hashtable of @{{ Text; ForegroundColor; BackgroundColor; Style }}",
				parameterName)
		};
	}
}
