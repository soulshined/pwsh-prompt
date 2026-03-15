using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;
using PwshPrompt.Consts;
using PwshPrompt.Configs;
using PwshPrompt.Types;
using PwshPrompt.Utils;

namespace PwshPrompt.IO.Choice;

internal sealed partial class Picker
{
	private void RenderDetailsView()
	{
		Item item = Items[CurrentIndex];
		if (item.Description is null) return;

		bool has_border = BufferConfig.HasValue;
		int content_width = CachedWidth - 4;

		List<string> wrapped = item.Description.WordWrap(content_width);

		while (true)
		{
			RenderDetailsContent(item, wrapped, has_border, content_width);

			while (true)
			{
				if (!_buffer.KeyAvailable)
				{
					Size size = PSUI.WindowSize;
					if (size.Width != CachedWidth || size.Height != CachedHeight)
					{
						CachedWidth = size.Width;
						CachedHeight = size.Height;
						content_width = CachedWidth - 4;
						wrapped = item.Description.WordWrap(content_width);
						break;
					}
					Thread.Sleep(50);
					continue;
				}

				ConsoleKeyInfo key = _buffer.ReadKey(intercept: true);

				if (key.Key == ConsoleKey.Escape)
				{
					DetailsViewScrollOffset = 0;
					Render();
					return;
				}

				if (key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control))
				{
					_buffer.WriteLine();
					throw new PipelineStoppedException();
				}

				int visible_lines = has_border
					? CachedHeight
						- 2 /* top/bottom border */
						- 1 /* title */
						- 1 /* tagline */
						- 1 /* margin above legend */
						- 1 /* legend */
					: CachedHeight
						- 1 /* title */
						- 1 /* tagline */
						- 1 /* margin above legend */
						- 1 /* legend */
						- 1 /* trailing cursor row for ClearRenderedBlock */
					;
				int max_scroll = Math.Max(0, wrapped.Count - visible_lines);

				if (key.Key == ConsoleKey.UpArrow)
				{
					if (DetailsViewScrollOffset > 0)
					{
						DetailsViewScrollOffset--;
						break;
					}
				}
				else if (key.Key == ConsoleKey.DownArrow)
				{
					if (DetailsViewScrollOffset < max_scroll)
					{
						DetailsViewScrollOffset++;
						break;
					}
				}
			}
		}
	}

	private void RenderDetailsTagLine(StringBuilder sb, string label, bool has_border)
	{
		int inner_width = has_border ? CachedWidth - 2 : CachedWidth;
		string padded_label = "  " + label + "  ";
		int label_visual = padded_label.VisualWidth();
		int total_width = Math.Min(inner_width - 4, CachedWidth * 2 / 5);
		int deco_total = Math.Max(2, total_width - label_visual);
		int deco_left = deco_total / 2;
		int deco_right = deco_total - deco_left;
		int tagline_len = deco_left + label_visual + deco_right;
		int left_padding = (inner_width - tagline_len) / 2;

		if (has_border)
		{
			BorderConfig border = BufferConfig!.Value.Border;
			int right_padding = inner_width - left_padding - tagline_len;
			sb.Append(ANSI.COLOR.Foreground(border.Color))
			  .Append(border.VerticalSide)
			  .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
			  .Append(BackgroundColor)
			  .Append(' ', left_padding)
			  .Append(ANSI.COLOR.Foreground(border.Color)).Append('─', deco_left).Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
			  .Append(ANSI.SEQUENCE.DIM).Append(padded_label).Append(ANSI.SEQUENCE.RESET_BOLD_DIM)
			  .Append(ANSI.COLOR.Foreground(border.Color)).Append('─', deco_right).Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
			  .Append(BackgroundColor)
			  .Append(' ', right_padding)
			  .Append(ANSI.COLOR.Foreground(border.Color))
			  .Append(border.VerticalSide)
			  .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
			  .Append('\n');
		}
		else
		{
			sb.Append(' ', left_padding)
			  .Append(ANSI.SEQUENCE.DIM)
			  .Append('─', deco_left)
			  .Append(padded_label)
			  .Append('─', deco_right)
			  .Append(ANSI.SEQUENCE.RESET_BOLD_DIM)
			  .Append('\n');
		}
	}

	private void RenderDetailsContent(Item item, List<string> wrapped, bool has_border, int content_width)
	{
		StringBuilder sb = new();

		if (has_border)
		{
			RestoreAlternateScreenBackground();

			BorderConfig border = BufferConfig!.Value.Border;
			int visible_lines = CachedHeight
									- 2 /* top/bottom border */
									- 1 /* title */
									- 1 /* tagline */
									- 1 /* margin above legend */
									- 1 /* legend */
									;
			int max_scroll = Math.Max(0, wrapped.Count - visible_lines);
			DetailsViewScrollOffset = Math.Min(DetailsViewScrollOffset, max_scroll);

			// top border
			RenderAlternateScreenBorder(sb);
			sb.Append('\n');

			// title
			string title = item.DisplayText.TruncateIf(CachedWidth - 4);
			int title_padding = Math.Max(0, CachedWidth - 2 - 2 - title.VisualWidth());
			sb.Append(ANSI.COLOR.Foreground(border.Color)).Append(border.VerticalSide).Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
			  .Append(BackgroundColor).Append(' ').Append(ForegroundColor).Append(title)
			  .Append(' ', title_padding).Append(' ')
			  .Append(ANSI.COLOR.Foreground(border.Color)).Append(border.VerticalSide).Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
			  .Append('\n');

			// tagline
			RenderDetailsTagLine(sb, "Description", has_border);

			// content lines
			int rendered_lines = 0;
			for (int i = DetailsViewScrollOffset; i < wrapped.Count && rendered_lines < visible_lines; i++)
			{
				string line_text = wrapped[i].TruncateIf(content_width);
				int line_padding = Math.Max(0, CachedWidth - 2 - 2 - line_text.VisualWidth());
				sb.Append(ANSI.COLOR.Foreground(border.Color)).Append(border.VerticalSide).Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
				  .Append(BackgroundColor).Append(' ').Append(ForegroundColor).Append(line_text)
				  .Append(' ', line_padding).Append(' ')
				  .Append(ANSI.COLOR.Foreground(border.Color)).Append(border.VerticalSide).Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
				  .Append('\n');
				rendered_lines++;
			}

			// fill remaining lines
			for (int i = rendered_lines; i < visible_lines; i++)
				AppendBorderedEmptyLine(sb);

			// margin above legend
			AppendBorderedEmptyLine(sb);

			// legend
			string legend = "";
			if (DetailsViewScrollOffset > 0)
				legend += "↑ more  ";
			if (DetailsViewScrollOffset < max_scroll)
				legend += "↓ more  ";
			legend += "Esc Go back";
			legend = legend.TruncateIf(CachedWidth - 4);

			int legend_padding = Math.Max(0, CachedWidth - 2 - 2 - legend.VisualWidth());
			sb.Append(ANSI.COLOR.Foreground(border.Color)).Append(border.VerticalSide).Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
			  .Append(BackgroundColor).Append(' ')
			  .Append(ANSI.COLOR.Foreground(ANSI.COLOR.GRAY_LIGHT))
			  .Append(legend)
			  .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
			  .Append(BackgroundColor).Append(' ', legend_padding).Append(' ')
			  .Append(ANSI.COLOR.Foreground(border.Color)).Append(border.VerticalSide).Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
			  .Append('\n');

			// bottom border
			RenderAlternateScreenBorder(sb, false);
		}
		else
		{
			ClearRenderedBlock();

			int visible_lines = CachedHeight
									- 1 /* title */
									- 1 /* tagline */
									- 1 /* margin above legend */
									- 1 /* legend */
									- 1 /* trailing cursor row for ClearRenderedBlock */
									;
			int max_scroll = Math.Max(0, wrapped.Count - visible_lines);
			DetailsViewScrollOffset = Math.Min(DetailsViewScrollOffset, max_scroll);

			int total_lines = 0;

			// title
			string title = item.DisplayText.TruncateIf(CachedWidth - 4);
			sb.Append("  ").Append(title).Append('\n');
			total_lines++;

			// tagline
			RenderDetailsTagLine(sb, "Description", has_border);
			total_lines++;

			// content
			int rendered_lines = 0;
			for (int i = DetailsViewScrollOffset; i < wrapped.Count && rendered_lines < visible_lines; i++)
			{
				string line_text = wrapped[i].TruncateIf(CachedWidth - 4);
				sb.Append("  ").Append(line_text).Append('\n');
				rendered_lines++;
				total_lines++;
			}

			// margin above legend
			sb.Append('\n');
			total_lines++;

			// legend
			string legend = "";
			if (DetailsViewScrollOffset > 0)
				legend += "↑ more  ";
			if (DetailsViewScrollOffset < max_scroll)
				legend += "↓ more  ";
			legend += "Esc Go back";
			sb.Append(ANSI.COLOR.Foreground(ANSI.COLOR.GRAY_LIGHT))
			  .Append("  ").Append(legend)
			  .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR).Append('\n');
			total_lines++;

			RenderedLineCount = total_lines + 1;
		}

		_buffer.Write(sb.ToString());
	}
}
