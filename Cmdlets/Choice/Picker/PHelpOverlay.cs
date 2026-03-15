using System.Management.Automation.Host;
using System.Text;
using PwshPrompt.Consts;
namespace PwshPrompt.IO.Choice;

internal sealed partial class Picker
{
	private static readonly string[][] HELP_OVERLAY_ENTRIES = {
		new[] { "Enter",			"Confirm" },
		new[] { "↑ / ↓",			"Navigate items" },
		new[] { "Space",			"Toggle selection" },
		new[] { "Ctrl-a",			"Toggle select all on page" },
		new[] { "1-9",				"Quick select by number" },
		new[] { "Home / End",		"First • Last on page" },
		new[] { "PgUp / PgDn",		"Next • Previous page" },
		new[] { "C-Home / C-End",	"First • Last page" },
		new[] { "F1",				"View full description" },
		new[] { "Esc",				"Cancel" },
		new[] { "?",				"Toggle this overlay" },
	};

	private static readonly string[][] HELP_OVERLAY_ENTRIES_SINGLE_MODE =
		Array.FindAll(HELP_OVERLAY_ENTRIES, static e =>
				e[0] != "Space"
				&& e[0] != "Ctrl-a"
		);

	private void RenderHelpOverlay() {
		StringBuilder sb = new();

		string[][] entries = SupportsMultiple
			? HELP_OVERLAY_ENTRIES
			: HELP_OVERLAY_ENTRIES_SINGLE_MODE;

		int width = CachedWidth - 10;
		int height = entries.Length + 4;
		int margin_left = 5;
		int margin_top = Math.Max(1, (CachedHeight - height) / 2);
		AnsiColor border_color = ANSI.COLOR.BEIGE;
		int shadow_offset_x = -1;
		int shadow_offset_y = 1;
		string overlay_bg = ANSI.COLOR.Background(ANSI.COLOR.GREEN);

		// drop shadow
		for (int row = 0; row < height; row++) {
			int abs_row = margin_top + shadow_offset_y + row;
			if (abs_row > CachedHeight) break;

			sb.Append(ANSI.SEQUENCE.MoveCursorTo(abs_row, margin_left + shadow_offset_x + 1));
			sb.Append(ANSI.COLOR.Background(ANSI.COLOR.DROP_SHADOW));
			sb.Append(ANSI.SEQUENCE.RepeatCharacter(" ", width + 2));
			sb.Append(ANSI.SEQUENCE.RESET);
		}

		// top border with title
		string title = " Shortcuts ";
		int padding_left = (width - 2 - title.Length) / 2;
		int padding_right = width - 2 - title.Length - padding_left;

		sb.Append(ANSI.SEQUENCE.MoveCursorTo(margin_top, margin_left + 1));
		sb.Append(overlay_bg);
		sb.Append(ANSI.COLOR.Foreground(border_color));
		sb.Append('┌');
		sb.Append(ANSI.SEQUENCE.RepeatCharacter("─", padding_left));
		sb.Append(title);
		sb.Append(ANSI.SEQUENCE.RepeatCharacter("─", padding_right));
		sb.Append('┐');

		// blank line after title
		sb.Append(ANSI.SEQUENCE.MoveCursorTo(margin_top + 1, margin_left + 1));
		sb.Append(overlay_bg);
		sb.Append('│');
		sb.Append(ANSI.SEQUENCE.RepeatCharacter(" ", width - 2));
		sb.Append('│');

		for (int i = 0; i < entries.Length; i++) {
			string key = entries[i][0];
			string description = entries[i][1];
			string line_text = key.PadRight(16) + description;

			int abs_row = margin_top + 2 + i;
			sb.Append(ANSI.SEQUENCE.MoveCursorTo(abs_row, margin_left + 1));
			sb.Append(overlay_bg);
			sb.Append(ANSI.COLOR.Foreground(border_color)).Append("│ ");

			if (line_text.Length > width - 4)
				line_text = line_text.Substring(0, width - 5) + "…";

			padding_right = width - 4 - line_text.Length;
			sb.Append(line_text);
			if (padding_right > 0)
				sb.Append(' ', padding_right);
			sb.Append(' ').Append(ANSI.COLOR.Foreground(border_color)).Append('│');
		}

		// blank line before bottom border
		int bottom_blank_row = margin_top + 2 + entries.Length;
		sb.Append(ANSI.SEQUENCE.MoveCursorTo(bottom_blank_row, margin_left + 1));
		sb.Append(overlay_bg);
		sb.Append(ANSI.COLOR.Foreground(border_color)).Append('│');
		sb.Append(ANSI.SEQUENCE.RepeatCharacter(" ", width - 2));
		sb.Append('│');

		// bottom border with version label
		string version_label = " pwsh-prompt v" + GetType().Assembly.GetName().Version!.ToString(3) + " ";
		int label_left = width - 2 - version_label.Length;

		sb.Append(ANSI.SEQUENCE.MoveCursorTo(bottom_blank_row + 1, margin_left + 1));
		sb.Append(overlay_bg);
		sb.Append(ANSI.COLOR.Foreground(border_color));
		sb.Append('└');
		sb.Append(ANSI.SEQUENCE.RepeatCharacter("─", label_left));
		sb.Append(version_label);
		sb.Append('┘');
		sb.Append(ANSI.SEQUENCE.RESET);

		_buffer.Write(sb.ToString());

		while (true) {
			if (!_buffer.KeyAvailable) {
				Size size = PSUI.WindowSize;
				if (size.Width != CachedWidth || size.Height != CachedHeight) {
					CachedWidth = size.Width;
					CachedHeight = size.Height;
					Render();
					RenderHelpOverlay();
					return;
				}
				Thread.Sleep(50);
				continue;
			}

			ConsoleKeyInfo dismiss_key = _buffer.ReadKey(intercept: true);
			if (dismiss_key.Key == ConsoleKey.Escape || dismiss_key.KeyChar == '?')
				break;
		}

		Render();
	}
}
