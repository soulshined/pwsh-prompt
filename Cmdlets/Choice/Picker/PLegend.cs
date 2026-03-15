using System.Text;
using PwshPrompt.Consts;
using PwshPrompt.Configs;

namespace PwshPrompt.IO.Choice;

internal sealed partial class Picker
{
	internal static class Legend
	{
		public const string PAGED       = "Esc Cancel • ↑/↓ Navigate • Enter Confirm • PgUp/PgDn Page";
		public const string SINGLE      = "Esc Cancel • ↑/↓ Navigate • Enter Confirm";
		public const string MULTI_PAGED = "Esc Cancel • ↑/↓ Navigate • Space Toggle • Enter Confirm • PgUp/PgDn Page";
		public const string MULTI       = "Esc Cancel • ↑/↓ Navigate • Space Toggle • Enter Confirm";
		public const string COMPACT     = "? Help";
	}

	private void RenderLegend(StringBuilder buffer) {
		string legend;
		if (SupportsMultiple)
			legend = TotalPages == 1 ? Legend.MULTI : Legend.MULTI_PAGED;
		else
			legend = TotalPages == 1 ? Legend.SINGLE : Legend.PAGED;

		if (Items[CurrentIndex].Description is not null)
			legend += " • F1 Details";

		string legend_text = ShouldShowFullLegend || legend.Length < CachedWidth - 4 ? legend : Legend.COMPACT;

		if (BufferConfig.HasValue) {
			BorderConfig border = BufferConfig.Value.Border;
			int right_padding = Math.Max(0, CachedWidth - 4 - legend_text.Length);
			buffer.Append(ANSI.COLOR.Foreground(border.Color))
			      .Append(border.VerticalSide)
			      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
			      .Append(ANSI.COLOR.Foreground(ANSI.COLOR.GRAY_LIGHT))
			      .Append("  ")
			      .Append(legend_text)
			      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
			      .Append(' ', right_padding)
			      .Append(ANSI.COLOR.Foreground(border.Color))
			      .Append(border.VerticalSide)
			      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR)
			      .Append('\n');
		} else {
			buffer.Append(ANSI.COLOR.Foreground(ANSI.COLOR.GRAY_LIGHT)).Append("  ")
			      .Append(legend_text)
			      .Append(ANSI.SEQUENCE.RESET_FOREGROUND_COLOR).Append('\n');
		}
	}
}
