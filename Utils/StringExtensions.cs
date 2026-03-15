using System.Globalization;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using Wcwidth;

namespace PwshPrompt.Utils;

internal static class StringExtensions {

	internal static string StripNewlines(this string text) {
		return text.Replace("\n", "").Replace("\r", "");
	}

	internal static string TruncateIf(this string text, int max_visual_width) {
		if (text.VisualWidth() < max_visual_width)
			return text;

		StringBuilder sb = new();
		int width = 0;
		var enumerator = StringInfo.GetTextElementEnumerator(text);
		while (enumerator.MoveNext()) {
			string element = enumerator.GetTextElement();
			int cw = ElementWidth(element);
			if (cw <= 0) {
				sb.Append(element);
				continue;
			}
			if (width + cw > max_visual_width - 1)
				break;
			width += cw;
			sb.Append(element);
		}
		sb.Append('\u2026');
		return sb.ToString();
	}

	internal static bool EqualsAny(this string needle, string[] haystack, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
	{
		foreach (string item in haystack)
			if (string.Equals(needle, item, comparison))
				return true;
		return false;
	}

	/// <summary>
	/// Compiles the string as a <see cref="Regex"/>.
	/// </summary>
	/// <returns>A compiled <see cref="Regex"/>.</returns>
	/// <exception cref="PSInvalidCastException">Thrown when the pattern is syntactically invalid.</exception>
	internal static Regex ParseRegex(this string input) {
		try {
			return new Regex(input);
		} catch {
			throw new PSInvalidCastException();
		}
	}

	internal static int VisualWidth(this string text) {
		int width = 0;
		var enumerator = StringInfo.GetTextElementEnumerator(text);
		while (enumerator.MoveNext()) {
			string element = enumerator.GetTextElement();
			int cw = ElementWidth(element);
			if (cw > 0)
				width += cw;
		}
		return width;
	}

	/// <summary>
	/// Returns the terminal cell width of a single grapheme cluster.
	/// Wcwidth uses legacy wcswidth tables that underreport width for two cases
	/// that modern terminals (Ghostty, Kitty, etc.) handle per the Unicode standard:
	///   1. U+FE0F (Variation Selector 16) forces emoji presentation, making the
	///      base character 2 cells wide (e.g. ☑️ = U+2611 + FE0F).
	///   2. Supplementary-plane pictographs (>= U+10000) like 🗙 (U+1F5D9) that
	///      have East_Asian_Width=Neutral but render as 2 cells.
	///      Detected via .NET's UnicodeCategory (updates with the runtime).
	/// </summary>
	private static int ElementWidth(string element) {
		int cp = char.ConvertToUtf32(element, 0);
		int cw = UnicodeCalculator.GetWidth(cp);
		if (cw >= 2)
			return cw;
		if (element.Length > 1 && element.Contains('\uFE0F'))
			return 2;
		if (cp >= 0x10000 && CharUnicodeInfo.GetUnicodeCategory(element, 0) == UnicodeCategory.OtherSymbol)
			return 2;
		return cw;
	}

	internal static List<string> WordWrap(this string text, int max_width)
	{
		List<string> result = new();
		string[] paragraphs = text.Split('\n');

		foreach (string paragraph in paragraphs)
		{
			if (string.IsNullOrEmpty(paragraph))
			{
				result.Add("");
				continue;
			}

			string[] words = paragraph.Split(' ');
			StringBuilder line = new();
			int line_width = 0;

			foreach (string word in words)
			{
				int word_width = word.VisualWidth();
				if (line_width > 0 && line_width + 1 + word_width > max_width)
				{
					result.Add(line.ToString());
					line.Clear();
					line_width = 0;
				}

				if (line_width > 0)
				{
					line.Append(' ');
					line_width++;
				}

				line.Append(word);
				line_width += word_width;
			}

			result.Add(line.ToString());
		}

		return result;
	}

}
