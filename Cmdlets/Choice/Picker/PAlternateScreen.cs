using System.Text;
using PwshPrompt.Consts;
using PwshPrompt.Configs;

namespace PwshPrompt.IO.Choice;

internal sealed partial class Picker
{
	private void RenderAlternateScreen() {
		StringBuilder buffer = new();

		int available_line_count = CachedHeight
								- 2 /* top/bottom border */
								- 1 /* legend line */
								- 1 /* status bar */
								- 1 /* tag line */
								;

		RenderAlternateScreenBorder(buffer);
		buffer.Append('\n');

		// top border + newline = first 2 rows of buffer
		ItemListStartLine = 2;

		if (Title.HasValue) {
			RenderTitle(buffer);
			available_line_count -= 2;
			ItemListStartLine += 2;
		}

		int message_lines_len = RenderMessage(buffer);
		available_line_count -= message_lines_len;
		ItemListStartLine += message_lines_len;
		RenderTagLine(buffer);

		bool is_paginated = Items.Length > available_line_count;
		if (is_paginated)
			available_line_count -= 2; // margin above pagination + pagination indicator

		RenderItemList(buffer, pad_to_page_size: true, available_items_line_count: available_line_count);

		if (is_paginated) {
			AppendBorderedEmptyLine(buffer);
			RenderPaginationIndicator(buffer, content_height: available_line_count);
		}

		RenderStatusBar(buffer);
		RenderLegend(buffer);
		RenderAlternateScreenBorder(buffer, false);

		_buffer.Write(buffer.ToString());
	}

	private void RenderAlternateScreenBorder(StringBuilder buffer, bool top = true) {
		BorderConfig border = BufferConfig!.Value.Border;

		buffer.Append(ANSI.COLOR.Foreground(border.Color));
		buffer.Append(top ? border.TopLeft : border.BottomLeft);
		buffer.Append(ANSI.SEQUENCE.RepeatCharacter(border.HorizontalSide, CachedWidth - 2));
		buffer.Append(top ? border.TopRight : border.BottomRight);

		buffer.Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR);
	}

	private void RestoreAlternateScreenBackground() {
		if (ANSI.Supports24bit())
			_buffer.Write(ANSI.COLOR.Background(BufferConfig!.Value.BackgroundColor));
		_buffer.Write(ANSI.SEQUENCE.ERASE_ENTIRE_SCREEN + ANSI.SEQUENCE.MOVE_CURSOR_HOME);
	}
}
