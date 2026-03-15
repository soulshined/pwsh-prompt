using System.Diagnostics;
using System.Management.Automation.Host;
using System.Text;
using PwshPrompt.Configs;
using PwshPrompt.Enums;
using PwshPrompt.Types;
using PwshPrompt.Consts;

using PwshPrompt.IO;
using PwshPrompt.Utils;

namespace PwshPrompt.IO.Choice;

internal sealed partial class Picker
{
	// region CHOICES
	private readonly Item[] Items;
	private readonly Label? Title;
	private readonly Label Message;
	private readonly string[] MessageParts;
	private readonly bool SupportsMultiple;
	private bool HelpMessageFitsInline;
	// endregion CHOICES

	// region APPEARANCE
	private readonly CycleMode Boundary;
	private readonly BufferConfig? BufferConfig;
	private readonly string ForegroundColor;
	private readonly string BackgroundColor;
	private readonly PSHostRawUserInterface PSUI;
	private readonly IBuffer _buffer;
	private bool ShouldShowFullLegend;
	// endregion APPEARANCE

	// region INTERNAL USAGE
	private readonly HashSet<int> SelectedIndices = new();
	private int TotalPages = 1;
	private int CurrentPage = 0;
	private int PageSize = 0;
	private int CurrentIndex;
	private int RenderedLineCount;
	private int CachedWidth;
	private int CachedHeight;
	private int DetailsViewScrollOffset;
	private int ItemListStartLine; // AlternateScreen: 1-indexed terminal row of the tagline (items at +1). Inline: 0-indexed line offset from top of render block.
	// endregion INTERNAL USAGE

	private const int MAX_NUM_PAGINATION_DOTS = 5;

	internal Picker(
		Item[] items,
		Label message,
		Label? title,
		CycleMode boundary,
		BufferConfig? bufferConfig,
		int? defaultIndex,
		bool supportsMultiple,
		PSHostRawUserInterface rawUI,
		IBuffer buffer)
	{
		Items = items;
		SupportsMultiple = supportsMultiple;
		Message = message;
		Title = title;
		Boundary = boundary;
		BufferConfig = bufferConfig;
		CurrentIndex = defaultIndex ?? 0;
		PSUI = rawUI;
		_buffer = buffer;
		CachedWidth = rawUI.WindowSize.Width;
		CachedHeight = rawUI.WindowSize.Height;
		ForegroundColor = bufferConfig.HasValue ? bufferConfig.Value.ForegroundColor.Fg : ANSI.SEQUENCE.RESET_FOREGROUND_COLOR;
		BackgroundColor = bufferConfig.HasValue ? bufferConfig.Value.BackgroundColor.Bg : ANSI.SEQUENCE.RESET_BACKGROUND_COLOR;

		MessageParts = Message.Text.StripNewlines().Split(' ', StringSplitOptions.TrimEntries);
	}

	internal int[]? Run()
	{
		bool previous_ctrl_c = _buffer.TreatControlCAsInput;
		_buffer.TreatControlCAsInput = true;

		try
		{
			if (BufferConfig.HasValue)
				_buffer.Write(ANSI.SEQUENCE.ENABLE_ALTERNATE_SCREEN_WITH_CURSOR);

			_buffer.Write(ANSI.SEQUENCE.DISABLE_CURSOR);
			Render();

			while (true)
			{
				if (!_buffer.KeyAvailable)
				{
					Size size = PSUI.WindowSize;
					if (size.Width != CachedWidth || size.Height != CachedHeight)
					{
						CachedWidth = size.Width;
						CachedHeight = size.Height;
						Render();
					}
					Thread.Sleep(50);
					continue;
				}

				ConsoleKeyInfo key = _buffer.ReadKey(intercept: true);

				if (KeyMap.TryGetValue(new(key.Key, key.Modifiers), out Func<Picker, LoopSignal<int[]>>? handler))
				{
					LoopSignal<int[]> signal = handler(this);
					if (signal.ShouldReturn)
						return signal.Value;
					continue;
				}

				if (key.KeyChar == '?')
				{
					if (BufferConfig.HasValue)
						RenderHelpOverlay();
					else
					{
						ShouldShowFullLegend = !ShouldShowFullLegend;
						Render();
					}
					continue;
				}

				if (key.KeyChar != '\0')
				{
					int hotkey_index = FindHotkey(key.KeyChar);
					if (hotkey_index >= 0)
					{
						if (SupportsMultiple)
						{
							if (!SelectedIndices.Remove(hotkey_index))
								SelectedIndices.Add(hotkey_index);
							CurrentIndex = hotkey_index;
							CurrentPage = PageSize > 0 ? CurrentIndex / PageSize : 0;
							Render();
						}
						else
							return new[] { hotkey_index };
					}

					if (key.KeyChar >= '1' && key.KeyChar <= '9')
					{
						int? digit_result = HandleDigit(key.KeyChar);
						if (digit_result.HasValue)
						{
							if (SupportsMultiple)
							{
								if (!SelectedIndices.Remove(digit_result.Value))
									SelectedIndices.Add(digit_result.Value);
								CurrentIndex = digit_result.Value;
								CurrentPage = PageSize > 0 ? CurrentIndex / PageSize : 0;
								Render();
							}
							else
								return new[] { digit_result.Value };
						}
					}
				}
			}
		}
		finally
		{
			_buffer.Write(ANSI.SEQUENCE.ENABLE_CURSOR);
			_buffer.TreatControlCAsInput = previous_ctrl_c;

			if (BufferConfig.HasValue)
				_buffer.Write(ANSI.SEQUENCE.RESET + ANSI.SEQUENCE.DISABLE_ALTERNATE_SCREEN_WITH_CURSOR);
			else {
				ClearRenderedBlock();
				_buffer.Write(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR);
			}
		}
	}

	#region Rendering - Layout

	private void Render() {
		if (BufferConfig.HasValue) {
			RestoreAlternateScreenBackground();
			RenderAlternateScreen();
		}
		else
			RenderInline();
	}

	private void RenderTitle(StringBuilder buffer) {
		if (!Title.HasValue) return;

		if (BufferConfig.HasValue) {
			BorderConfig border = BufferConfig.Value.Border;
			int title_len = Title.Value.Text.VisualWidth();
			int right_padding = Math.Max(0, CachedWidth - 3 - title_len);
			buffer.Append(ANSI.COLOR.Foreground(border.Color))
			      .Append(border.VerticalSide)
			      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
			      .Append(BackgroundColor)
			      .Append(' ')
			      .Append(Title.Value.ToString(CachedWidth - 4))
			      .Append(BackgroundColor)
			      .Append(' ', right_padding)
			      .Append(ANSI.COLOR.Foreground(border.Color))
			      .Append(border.VerticalSide)
			      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR);
		} else {
			buffer.Append("  ")
			      .Append(Title.Value.ToString(CachedWidth - 4));
		}

		buffer.Append("\n");
		AppendBorderedEmptyLine(buffer);
	}

	private int RenderMessage(StringBuilder buffer) {
		string msg_bg = Message.BackgroundColor.HasValue ? ANSI.COLOR.Background(Message.BackgroundColor.Value) : ANSI.SEQUENCE.RESET_BACKGROUND_COLOR;
		string msg_fg = Message.ForegroundColor.HasValue ? ANSI.COLOR.Foreground(Message.ForegroundColor.Value) : ANSI.SEQUENCE.RESET_FOREGROUND_COLOR;
		string msg_style = Message.Style.ToAnsi();

		int line_len = 0;
		int lines_added = 0;

		if (BufferConfig.HasValue) {
			BorderConfig border = BufferConfig.Value.Border;
			buffer.Append(ANSI.COLOR.Foreground(border.Color))
			      .Append(border.VerticalSide)
			      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR);
		}

		buffer.Append(msg_bg).Append(msg_fg).Append(msg_style);

		foreach (string part in MessageParts) {
			if (line_len + part.VisualWidth() > CachedWidth - 3) {
				if (BufferConfig.HasValue) {
					BorderConfig border = BufferConfig.Value.Border;
					int padding = Math.Max(0, CachedWidth - 3 - line_len);
					buffer.Append(' ', padding)
					      .Append(ANSI.SEQUENCE.RESET)
					      .Append(BackgroundColor)
					      .Append(ForegroundColor)
					      .Append(' ')
					      .Append(ANSI.COLOR.Foreground(border.Color))
					      .Append(border.VerticalSide)
					      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
					      .Append('\n');
					buffer.Append(ANSI.COLOR.Foreground(border.Color))
					      .Append(border.VerticalSide)
					      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
					      .Append(msg_bg).Append(msg_fg).Append(msg_style);
					line_len = 0;
				} else {
					buffer.Append("\n ");
					line_len = 1;
				}
				lines_added++;
			}

			if (line_len == 0) {
				buffer.Append(' ');
				line_len++;
			}

			buffer.Append(' ').Append(part);
			line_len += 1 + part.VisualWidth();
		}

		if (BufferConfig.HasValue) {
			BorderConfig border = BufferConfig.Value.Border;
			int padding = Math.Max(0, CachedWidth - 3 - line_len);
			buffer.Append(' ', padding)
			      .Append(ANSI.SEQUENCE.RESET)
			      .Append(BackgroundColor)
			      .Append(ForegroundColor)
			      .Append(' ')
			      .Append(ANSI.COLOR.Foreground(border.Color))
			      .Append(border.VerticalSide)
			      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
			      .Append('\n');
		} else {
			buffer.Append(ANSI.SEQUENCE.RESET).Append('\n');
		}

		return lines_added + 1;
	}

	private void RenderTagLine(StringBuilder buffer) {
		int total_width = CachedWidth / 3;
		bool buffer_config_exists = BufferConfig.HasValue;
		int inner_width = buffer_config_exists ? CachedWidth - 2 : CachedWidth;

		if (SupportsMultiple) {
			string label = SelectedIndices.Count > 0
				? $"  Select Multiple ❬{SelectedIndices.Count}❭ "
				: "  Select Multiple ";

			int label_visual_len = label.VisualWidth();
			int deco_total = Math.Max(2, total_width - label_visual_len);
			int deco_left = deco_total / 2;
			int deco_right = deco_total - deco_left;
			int tagline_len = deco_left + label_visual_len + deco_right;
			int left_padding = (inner_width - tagline_len) / 2;

			if (buffer_config_exists) {
				BorderConfig border = BufferConfig!.Value.Border;
				buffer.Append(ANSI.COLOR.Foreground(border.Color))
				      .Append(border.VerticalSide)
				      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR);
			}

			buffer.Append(ANSI.SEQUENCE.RepeatCharacter(" ", left_padding));

			if (buffer_config_exists)
				buffer.Append(ANSI.COLOR.Foreground(BufferConfig!.Value.Border.Color));
			else
				buffer.Append(ANSI.SEQUENCE.DIM);

			buffer.Append('―', deco_left);

			if (buffer_config_exists)
				buffer.Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR);
			else
				buffer.Append(ANSI.SEQUENCE.RESET_BOLD_DIM);

			buffer.Append(ANSI.SEQUENCE.DIM)
				.Append(label)
				.Append(ANSI.SEQUENCE.RESET_BOLD_DIM);

			if (buffer_config_exists)
				buffer.Append(ANSI.COLOR.Foreground(BufferConfig!.Value.Border.Color));
			else
				buffer.Append(ANSI.SEQUENCE.DIM);

			buffer.Append('―', deco_right);

			if (buffer_config_exists) {
				BorderConfig border = BufferConfig!.Value.Border;
				int right_padding = inner_width - left_padding - tagline_len;
				buffer.Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
				      .Append(' ', right_padding)
				      .Append(ANSI.COLOR.Foreground(border.Color))
				      .Append(border.VerticalSide)
				      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR);
			} else {
				buffer.Append(ANSI.SEQUENCE.RESET_BOLD_DIM);
			}

			buffer.Append('\n');
		} else {
			string decoration = new string('―', total_width);
			int decoration_visual_len = decoration.VisualWidth();
			int left_padding = (inner_width - decoration_visual_len) / 2;

			if (buffer_config_exists) {
				BorderConfig border = BufferConfig!.Value.Border;
				int right_padding = inner_width - left_padding - decoration_visual_len;
				buffer.Append(ANSI.COLOR.Foreground(border.Color))
				      .Append(border.VerticalSide)
				      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR);
				buffer.Append(ANSI.SEQUENCE.RepeatCharacter(" ", left_padding))
				      .Append(ANSI.COLOR.Foreground(border.Color))
				      .Append(decoration)
				      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
				      .Append(' ', right_padding);
				buffer.Append(ANSI.COLOR.Foreground(border.Color))
				      .Append(border.VerticalSide)
				      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR);
			} else {
				buffer.Append(ANSI.SEQUENCE.RepeatCharacter(" ", left_padding))
				      .Append(ANSI.COLOR.Foreground(ANSI.COLOR.GRAY_LIGHT))
				      .Append(decoration)
				      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR);
			}

			buffer.Append('\n');
		}
	}

	private void RenderPaginationIndicator(StringBuilder buffer, int content_height) {
		PaginationConfig config = BufferConfig?.Pagination ?? default;
		bool buffer_config_exists = BufferConfig.HasValue;
		int inner_width = buffer_config_exists ? CachedWidth - 2 : CachedWidth;

		int page_start = CurrentPage / MAX_NUM_PAGINATION_DOTS * MAX_NUM_PAGINATION_DOTS;
		int dot_count = Math.Min(MAX_NUM_PAGINATION_DOTS, TotalPages - page_start);

		string prev_text = (page_start + 1).ToString();
		string next_text = (page_start + dot_count).ToString();
		string total_text = ToSubscript(TotalPages);

		int dots_width = 0;
		for (int d = 0; d < dot_count; d++) {
			if (d > 0) dots_width++;
			dots_width += d == CurrentPage - page_start ? config.SelectedItem.Text.VisualWidth() : config.Item.Text.VisualWidth();
		}

		bool show_total = TotalPages != page_start + dot_count;
		int visible_len = prev_text.VisualWidth() + 1 + dots_width + 1 + next_text.VisualWidth()
						+ (show_total ? total_text.VisualWidth() : 0);
		int padding = Math.Max(0, (inner_width - visible_len) / 2);

		if (buffer_config_exists) {
			BorderConfig border = BufferConfig!.Value.Border;
			buffer.Append(ANSI.COLOR.Foreground(border.Color))
			      .Append(border.VerticalSide)
			      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR);
		}

		buffer.Append(' ', padding);
		buffer.Append(BackgroundColor).Append(ForegroundColor);
		buffer.Append((config.PrevPage with { Text = prev_text }).ToString());
		buffer.Append(BackgroundColor).Append(ForegroundColor).Append(' ');

		for (int d = 0; d < dot_count; d++) {
			if (d > 0) buffer.Append(BackgroundColor).Append(ForegroundColor).Append(' ');
			buffer.Append((d == CurrentPage - page_start ? config.SelectedItem : config.Item).ToString());
		}

		buffer.Append(BackgroundColor).Append(ForegroundColor).Append(' ');
		buffer.Append((config.NextPage with { Text = next_text }).ToString());
		if (show_total) {
			buffer.Append(BackgroundColor).Append(ForegroundColor);
			buffer.Append((config.TotalPage with { Text = total_text }).ToString());
		}
		buffer.Append(BackgroundColor).Append(ForegroundColor);

		if (buffer_config_exists) {
			BorderConfig border = BufferConfig!.Value.Border;
			int right_padding = Math.Max(0, inner_width - padding - visible_len);
			buffer.Append(' ', right_padding)
			      .Append(ANSI.COLOR.Foreground(border.Color))
			      .Append(border.VerticalSide)
			      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
			      .Append('\n');
		} else {
			buffer.Append("\n");
		}
	}

	private int RenderItemList(StringBuilder buffer, bool pad_to_page_size, int available_items_line_count) {
		// region Pagination Calc
		PageSize = available_items_line_count;
		TotalPages = (int)Math.Ceiling((double)Items.Length / (double)PageSize);
		CurrentPage = PageSize > 0 ? CurrentIndex / PageSize : 0;
		// endregion Pagination Calc

		int page_start = CurrentPage * PageSize;
		int page_item_count = Math.Min(PageSize, Items.Length - page_start);
		int line_count = pad_to_page_size ? PageSize : page_item_count;

		for (int i = 0; i < line_count; i++) {
			if (i < page_item_count)
			{
				int abs_index = page_start + i;
				buffer.Append(BuildItemLine(abs_index));
				buffer.Append('\n');
			}
			else
			{
				if (BufferConfig.HasValue)
					AppendBorderedEmptyLine(buffer, newline: false);
				buffer.Append('\n');
			}
		}

		return line_count;
	}

	private void RenderStatusBar(StringBuilder buffer)
	{
		bool has_border = BufferConfig.HasValue;
		int inner_width = has_border ? CachedWidth - 2 : CachedWidth;

		Item current = Items[CurrentIndex];
		bool show_help = current.HelpMessage is not null && !HelpMessageFitsInline;

		if (has_border)
		{
			BorderConfig border = BufferConfig!.Value.Border;
			buffer.Append(ANSI.COLOR.Foreground(border.Color))
			      .Append(border.VerticalSide)
			      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR);

			if (show_help)
			{
				string help_text = ("  " + current.HelpMessage!).TruncateIf(inner_width);
				int help_width = help_text.VisualWidth();
				int right_padding = Math.Max(0, inner_width - help_width);
				buffer.Append(BackgroundColor).Append(ForegroundColor)
				      .Append(ANSI.SEQUENCE.ITALIC).Append(help_text).Append(ANSI.SEQUENCE.RESET_ITALIC)
				      .Append(BackgroundColor)
				      .Append(' ', right_padding);
			}
			else
				buffer.Append(BackgroundColor).Append(' ', inner_width);

			buffer.Append(ANSI.COLOR.Foreground(border.Color))
			      .Append(border.VerticalSide)
			      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR);
		}
		else
		{
			if (show_help)
			{
				string help_text = ("  " + current.HelpMessage!).TruncateIf(CachedWidth);
				buffer.Append(ForegroundColor).Append(ANSI.SEQUENCE.ITALIC).Append(help_text).Append(ANSI.SEQUENCE.RESET_ITALIC);
			}
		}

		buffer.Append('\n');
	}

	private void AppendBorderedEmptyLine(StringBuilder buffer, bool newline = true) {
		if (!BufferConfig.HasValue) return;
		BorderConfig border = BufferConfig.Value.Border;
		buffer.Append(BackgroundColor)
		      .Append(ANSI.COLOR.Foreground(border.Color))
		      .Append(border.VerticalSide)
		      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
		      .Append(' ', CachedWidth - 2)
		      .Append(ANSI.COLOR.Foreground(border.Color))
		      .Append(border.VerticalSide)
		      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR);
		if (newline)
			buffer.Append('\n');
	}

	#endregion Rendering - Layout

	#region Rendering - Item

	private string BuildItemLine(int absolute_index)
	{
		Item item = Items[absolute_index];
		bool has_border = BufferConfig.HasValue;
		int border_chars = has_border ? 2 : 0;

		string hotkey_label = "";
		int hotkey_visual_len = 0;
		if (item.HotKey.HasValue)
		{
			hotkey_label = $"[{ANSI.SEQUENCE.UNDERLINE}{item.HotKey.Value}{ANSI.SEQUENCE.RESET_UNDERLINE}] ";
			hotkey_visual_len = 4;
		}

		string text = item.DisplayText.TruncateIf(CachedWidth - 4 - border_chars - hotkey_visual_len);

		ItemConfig? config = BufferConfig?.Item;
		bool is_selected = SelectedIndices.Contains(absolute_index);
		bool is_current = absolute_index == CurrentIndex;

		string prefix_ansi = "";
		int prefix_visual_len = 0;
		int prefix_render_width = 0;
		if (SupportsMultiple)
		{
			if (config.HasValue)
			{
				ToggleItem indicator = is_current
					? config.Value.selectedMultipleIndicator
					: config.Value.multipleIndicator;
				Label label = is_selected ? indicator.Enabled : indicator.Disabled;
				prefix_ansi = label.ToString();
				prefix_render_width = label.Text.VisualWidth();
				prefix_visual_len = prefix_render_width + 1;
			}
			else
			{
				prefix_ansi = is_selected ? "◉ " : "○ ";
				prefix_render_width = prefix_ansi.VisualWidth();
				prefix_visual_len = prefix_render_width;
			}
		}

		string item_fg = item.ForegroundColor.HasValue
			? ANSI.COLOR.Foreground(item.ForegroundColor.Value)
			: config.HasValue && config.Value.Item.ForegroundColor.HasValue
				? ANSI.COLOR.Foreground(config.Value.Item.ForegroundColor.Value)
				: ANSI.SEQUENCE.RESET_FOREGROUND_COLOR;
		string text_bg = item.BackgroundColor.HasValue
			? ANSI.COLOR.Background(item.BackgroundColor!.Value)
			: "";
		string item_style = item.Style.HasValue
			? item.Style.Value.ToAnsi()
			: "";

		string left_border = "";
		string right_border = "";
		if (has_border) {
			BorderConfig border = BufferConfig!.Value.Border;
			left_border = BackgroundColor
						+ ANSI.COLOR.Foreground(border.Color)
						+ border.VerticalSide
						+ ANSI.SEQUENCE.RESET_FOREGROUND_COLOR;
			right_border = BackgroundColor
						 + ANSI.COLOR.Foreground(border.Color)
						 + border.VerticalSide
						 + ANSI.SEQUENCE.RESET_FOREGROUND_COLOR;
		}

		// For border: use prefix_render_width (actual visible chars) so right border aligns exactly.
		// For no-border: use prefix_visual_len (includes phantom +1 for config path; the 1-short
		// line is invisible because the alternate screen background fills the gap).
		int inner_width = CachedWidth - border_chars;
		int prefix_width = has_border ? prefix_render_width : prefix_visual_len;

		StringBuilder sb = new(text.Length + 128);
		if (is_current)
		{
			// border: -5 accounts for leading_margin(1) + rev_space(1) + space_before_text(1)
			//         + trailing_space(1) + trailing_margin(1)
			// no-border: -4 (no trailing margin needed, bg fills the gap)
			int overhead = has_border ? 5 : 4;
			int content_width = prefix_width + hotkey_visual_len + text.VisualWidth();
			int remaining = inner_width - overhead - content_width;

			string help_inline = "";
			int help_inline_width = 0;
			if (is_current && item.HelpMessage is not null)
			{
				int help_visual = item.HelpMessage.VisualWidth();
				int needed = 2 + help_visual; // 2 spaces gap + help text
				if (needed <= remaining)
				{
					help_inline = item.HelpMessage;
					help_inline_width = needed;
					HelpMessageFitsInline = true;
				}
				else
					HelpMessageFitsInline = false;
			}
			else if (is_current)
				HelpMessageFitsInline = true; // no help message = treat as "fits" (no status bar needed)

			int right_padding = Math.Max(0, remaining - help_inline_width);
			sb.Append(left_border)
			  .Append(BackgroundColor).Append(' ')
			  .Append(item_fg)
			  .Append(ANSI.SEQUENCE.REVERSE_VIDEO)
			  .Append(' ').Append(prefix_ansi)
			  .Append(item_fg).Append(BackgroundColor)
			  .Append(ANSI.SEQUENCE.REVERSE_VIDEO)
			  .Append(' ').Append(hotkey_label).Append(text_bg).Append(item_style).Append(text)
			  .Append(ANSI.SEQUENCE.RESET);

			if (help_inline.Length > 0)
			{
				sb.Append(item_fg).Append(BackgroundColor).Append(ANSI.SEQUENCE.REVERSE_VIDEO)
				  .Append("  ")
				  .Append(ANSI.SEQUENCE.DIM).Append(help_inline).Append(ANSI.SEQUENCE.RESET_BOLD_DIM)
				  .Append(ANSI.SEQUENCE.RESET);
			}

			sb.Append(item_fg).Append(BackgroundColor).Append(ANSI.SEQUENCE.REVERSE_VIDEO)
			  .Append(' ')
			  .Append(ANSI.SEQUENCE.RepeatCharacter(" ", right_padding))
			  .Append(ANSI.SEQUENCE.RESET);
			if (has_border)
				sb.Append(BackgroundColor).Append(' ').Append(right_border);
			else
				sb.Append(right_border);
		}
		else
		{
			int text_visual = text.VisualWidth();
			string help_inline = "";
			int help_inline_width = 0;
			if (item.HelpMessage is not null)
			{
				int help_visual = item.HelpMessage.VisualWidth();
				int needed = 2 + help_visual;
				int available = inner_width - 3 - prefix_width - hotkey_visual_len - text_visual;
				if (needed <= available)
				{
					help_inline = item.HelpMessage;
					help_inline_width = needed;
				}
			}

			int right_padding = has_border
				? Math.Max(0, inner_width - 3 - prefix_width - hotkey_visual_len - text_visual - help_inline_width)
				: 0;
			sb.Append(left_border)
			  .Append(BackgroundColor).Append("  ")
			  .Append(prefix_ansi)
			  .Append(item_fg).Append(BackgroundColor)
			  .Append(' ').Append(hotkey_label).Append(text_bg).Append(item_style).Append(text)
			  .Append(ANSI.SEQUENCE.RESET);
			if (help_inline.Length > 0)
			{
				sb.Append(BackgroundColor)
				  .Append("  ")
				  .Append(ANSI.SEQUENCE.DIM).Append(help_inline).Append(ANSI.SEQUENCE.RESET_BOLD_DIM);
			}
			if (has_border)
				sb.Append(BackgroundColor)
				  .Append(' ', right_padding)
				  .Append(right_border);
		}
		return sb.ToString();
	}

	#endregion Rendering - Item

	#region Rendering - Partial Redraws

	// Appends ANSI to redraw one item line in AlternateScreen without a full re-render.
	private void AppendAlternateItemRedraw(StringBuilder sb, int absolute_index)
	{
		int row = ItemListStartLine + (absolute_index - CurrentPage * PageSize) + 1;
		if (ANSI.Supports24bit())
			sb.Append(ANSI.COLOR.Background(BufferConfig!.Value.BackgroundColor));
		sb.Append(ANSI.SEQUENCE.MoveCursorTo(row, 1));
		sb.Append(ANSI.SEQUENCE.ERASE_ENTIRE_LINE);
		sb.Append(BuildItemLine(absolute_index));
	}

	private void AppendStatusBarRedraw(StringBuilder sb)
	{
		int status_row = ItemListStartLine + PageSize + 1;
		if (Items.Length > PageSize)
			status_row += 2; // margin above pagination + pagination indicator
		sb.Append(ANSI.SEQUENCE.MoveCursorTo(status_row, 1));
		sb.Append(ANSI.SEQUENCE.ERASE_ENTIRE_LINE);
		RenderStatusBar(sb);
	}

	// Redraws prev item (now unselected) and current item (now selected) in one write.
	private void RedrawNavigation(int prev_index)
	{
		StringBuilder sb = new();
		AppendAlternateItemRedraw(sb, prev_index);
		AppendAlternateItemRedraw(sb, CurrentIndex);
		AppendStatusBarRedraw(sb);
		_buffer.Write(sb.ToString());
	}

	// Redraws the tagline (count changed) and current item (toggle indicator) in one write.
	private void RedrawToggle()
	{
		StringBuilder sb = new();
		if (ANSI.Supports24bit())
			sb.Append(ANSI.COLOR.Background(BufferConfig!.Value.BackgroundColor));
		sb.Append(ANSI.SEQUENCE.MoveCursorTo(ItemListStartLine, 1));
		sb.Append(ANSI.SEQUENCE.ERASE_ENTIRE_LINE);
		RenderTagLine(sb);
		AppendAlternateItemRedraw(sb, CurrentIndex);
		AppendStatusBarRedraw(sb);
		_buffer.Write(sb.ToString());
	}

	// Redraws the tagline and every visible item on the current page in one write (for Ctrl-A).
	private void RedrawPageItemsAndTag()
	{
		StringBuilder sb = new();
		if (ANSI.Supports24bit())
			sb.Append(ANSI.COLOR.Background(BufferConfig!.Value.BackgroundColor));
		sb.Append(ANSI.SEQUENCE.MoveCursorTo(ItemListStartLine, 1));
		sb.Append(ANSI.SEQUENCE.ERASE_ENTIRE_LINE);
		RenderTagLine(sb);
		int page_start = CurrentPage * PageSize;
		int page_end = Math.Min(page_start + PageSize, Items.Length);
		for (int i = page_start; i < page_end; i++)
			AppendAlternateItemRedraw(sb, i);
		AppendStatusBarRedraw(sb);
		_buffer.Write(sb.ToString());
	}

	#endregion Rendering - Partial Redraws

	#region Navigation & Input

	private void MoveCursor(int direction)
	{
		int page_start = CurrentPage * PageSize;
		int page_end = Math.Min((CurrentPage + 1) * PageSize - 1, Items.Length - 1);
		int new_index = CurrentIndex + direction;

		if (new_index > page_end)
		{
			switch (Boundary)
			{
				case CycleMode.Next:
					if (CurrentPage < TotalPages - 1)
					{
						CurrentPage++;
						CurrentIndex = CurrentPage * PageSize;
					}
					break;
				case CycleMode.Cycle:
					CurrentIndex = page_start;
					break;
				case CycleMode.Stop:
					break;
			}
		}
		else if (new_index < page_start)
		{
			switch (Boundary)
			{
				case CycleMode.Next:
					if (CurrentPage > 0)
					{
						CurrentPage--;
						CurrentIndex = Math.Min((CurrentPage + 1) * PageSize - 1, Items.Length - 1);
					}
					break;
				case CycleMode.Cycle:
					CurrentIndex = page_end;
					break;
				case CycleMode.Stop:
					break;
			}
		}
		else
		{
			CurrentIndex = new_index;
		}
	}

	private int FindHotkey(char key)
	{
		int page_start = PageSize > 0 ? CurrentPage * PageSize : 0;
		int page_end = PageSize > 0 ? Math.Min(page_start + PageSize, Items.Length) : Items.Length;
		for (int i = page_start; i < page_end; i++)
		{
			if (Items[i].HotKey == key)
				return i;
		}
		return -1;
	}

	private int? HandleDigit(char firstChar)
	{
		int page_start = CurrentPage * PageSize;
		int page_end = Math.Min((CurrentPage + 1) * PageSize - 1, Items.Length - 1);
		int items_on_page = page_end - page_start + 1;
		int first = firstChar - '0';

		if (items_on_page <= 9)
		{
			return first >= 1 && first <= items_on_page
				? page_start + first - 1
				: null;
		}

		Stopwatch sw = Stopwatch.StartNew();
		while (sw.ElapsedMilliseconds < 500)
		{
			if (_buffer.KeyAvailable)
			{
				ConsoleKeyInfo next_key = _buffer.ReadKey(intercept: true);
				if (next_key.KeyChar >= '0' && next_key.KeyChar <= '9')
				{
					int two_digit = first * 10 + (next_key.KeyChar - '0');
					if (two_digit >= 1 && two_digit <= items_on_page)
						return page_start + two_digit - 1;
				}
				break;
			}
			Thread.Sleep(10);
		}

		return first >= 1 && first <= items_on_page
			? page_start + first - 1
			: null;
	}

	#endregion Navigation & Input

	#region Utilities

	private void ClearRenderedBlock()
	{
		if (RenderedLineCount <= 0) return;

		StringBuilder sb = new();

		if (RenderedLineCount > 1)
			sb.Append(ANSI.SEQUENCE.MoveCursorUp(RenderedLineCount - 1)).Append('\r');
		else
			sb.Append('\r');

		for (int i = 0; i < RenderedLineCount; i++)
		{
			sb.Append(ANSI.SEQUENCE.ERASE_ENTIRE_LINE);
			if (i < RenderedLineCount - 1)
				sb.Append(ANSI.SEQUENCE.MoveCursorDown(1));
		}

		if (RenderedLineCount > 1)
			sb.Append('\r').Append(ANSI.SEQUENCE.MoveCursorUp(RenderedLineCount - 1));
		else
			sb.Append('\r');

		_buffer.Write(sb.ToString());
	}

	private static string ToSubscript(int number)
	{
		Span<char> digits = stackalloc char[10];
		int len = 0;
		if (number == 0) { digits[0] = '₀'; len = 1; }
		else { for (int n = number; n > 0; n /= 10) { digits[len++] = (char)('₀' + n % 10); } }
		digits[..len].Reverse();
		return new string(digits[..len]);
	}

	#endregion Utilities

}
