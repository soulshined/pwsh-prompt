using System.Text;
using PwshPrompt.Consts;

namespace PwshPrompt.IO.Choice;

internal sealed partial class Picker
{
	private const int INLINE_MAX_PAGE_SIZE = 20;

	private void RenderInline() {
		StringBuilder buffer = new();

		int line_count = 1 /* tagline */
						+ 1 /* status bar */
						+ 2 /* legend + 1 trailing cursor row for ClearRenderedBlock */
						;

		if (Title.HasValue) {
			RenderTitle(buffer);
			line_count += 1;
		}

		line_count += RenderMessage(buffer);

		RenderTagLine(buffer);

		int max_items = CachedHeight - line_count;

		if (max_items < Items.Length)
			max_items -= 1 /* margin above pagination */
						+ 1 /* pagination line */
						;

		int available_item_lines = Math.Min(
				Math.Clamp(max_items, 1, INLINE_MAX_PAGE_SIZE),
				Items.Length
		);

		ItemListStartLine = line_count;
		line_count += RenderItemList(buffer, pad_to_page_size: false, available_items_line_count: available_item_lines);

		if (Items.Length > available_item_lines) {
			buffer.Append('\n'); // pagination margin
			RenderPaginationIndicator(buffer, available_item_lines);
			line_count += 2;
		}

		RenderStatusBar(buffer);
		RenderLegend(buffer);

		if (RenderedLineCount == 0) {
			_buffer.Write(new string('\n', line_count));
			_buffer.Write(ANSI.SEQUENCE.MoveCursorUp(line_count));
		} else {
			ClearRenderedBlock();
		}

		_buffer.Write(buffer.ToString());
		RenderedLineCount = line_count;
	}
}
