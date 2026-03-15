using System.Management.Automation;

namespace PwshPrompt.IO;

/// <summary>
/// A readline implementation that supports tab-completion of filesystem paths
/// using PowerShell's built-in <see cref="CommandCompletion"/> engine.
/// <para>
/// This is a reusable utility — any cmdlet can instantiate it with a
/// <see cref="System.Management.Automation.Host.PSHostUserInterface"/> to get interactive path input with completion.
/// </para>
/// </summary>
/// <example>
/// <code>
/// var input = new PathReadLine("directory").ReadLine();
/// </code>
/// </example>
internal sealed class PathReadLine
{
	private readonly string _expectedType;
	private int _previousRenderLength;
	private readonly string? _default;

	/// <param name="expected_type">
	/// Either <c>"file"</c> or <c>"directory"</c>.
	/// Controls completion filtering: directories use <c>Set-Location</c>,
	/// files use <c>Get-Item</c> as the fake command for the completion engine.
	/// </param>
	/// <param name="default_value">Optional default path returned when the user submits empty input.</param>
	public PathReadLine(string expected_type, string? default_value = null)
	{
		_expectedType = expected_type;
		_default = default_value;
	}

	/// <summary>
	/// Reads a line of input with tab completion support.
	/// Blocks until the user presses Enter.
	/// </summary>
	/// <returns>The input string with any completion-engine quotes stripped.</returns>
	/// <exception cref="PipelineStoppedException">Thrown when the user presses Ctrl+C.</exception>
	public string ReadLine()
	{
		List<char> buffer = new();
		int cursor_pos = 0;

		CommandCompletion? completion = null;
		string? original_input = null;

		bool previous_ctrl_c = Console.TreatControlCAsInput;
		Console.TreatControlCAsInput = true;

		try
		{
			while (true)
			{
				ConsoleKeyInfo key = Console.ReadKey(intercept: true);

				if (key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control))
				{
					Console.WriteLine();
					throw new PipelineStoppedException();
				}

				switch (key.Key)
				{
					case ConsoleKey.Enter:
						Console.WriteLine();
						return StripQuotes(new string(buffer.ToArray()));

					case ConsoleKey.Backspace:
						if (cursor_pos > 0)
						{
							buffer.RemoveAt(cursor_pos - 1);
							cursor_pos--;
							completion = null;
							original_input = null;
							RedrawLine(buffer, cursor_pos);
						}
						break;

					case ConsoleKey.Tab:
						HandleTab(buffer, ref cursor_pos, ref completion, ref original_input,
							forward: !key.Modifiers.HasFlag(ConsoleModifiers.Shift));
						break;

					case ConsoleKey.Escape:
						if (completion != null && original_input != null)
						{
							buffer.Clear();
							buffer.AddRange(original_input);
							cursor_pos = buffer.Count;
							completion = null;
							original_input = null;
						}
						else
						{
							buffer.Clear();
							cursor_pos = 0;
						}
						RedrawLine(buffer, cursor_pos);
						break;

					default:
						if (key.KeyChar != '\0' && !char.IsControl(key.KeyChar))
						{
							buffer.Insert(cursor_pos, key.KeyChar);
							cursor_pos++;
							completion = null;
							original_input = null;
							RedrawLine(buffer, cursor_pos);
						}
						break;
				}
			}
		}
		finally
		{
			Console.TreatControlCAsInput = previous_ctrl_c;
		}
	}

	private void HandleTab(List<char> buffer, ref int cursor_pos,
		ref CommandCompletion? completion, ref string? original_input, bool forward)
	{
		if (completion == null)
		{
			original_input = new string(buffer.ToArray());
			completion = GetCompletions(original_input);

			if (completion.CompletionMatches.Count == 0)
			{
				completion = null;
				original_input = null;
				return;
			}
		}

		CompletionResult result = completion.GetNextResult(forward);
		if (result == null) return;

		string prefix = GetFakeCommandPrefix();
		int replace_start = Math.Max(0, completion.ReplacementIndex - prefix.Length);

		// Clear everything from the replacement start
		if (replace_start < buffer.Count)
			buffer.RemoveRange(replace_start, buffer.Count - replace_start);

		// Insert the completion text
		buffer.InsertRange(replace_start, result.CompletionText);
		cursor_pos = replace_start + result.CompletionText.Length;

		RedrawLine(buffer, cursor_pos);
	}

	private CommandCompletion GetCompletions(string user_input)
	{
		string prefix = GetFakeCommandPrefix();
		string fake_command = prefix + user_input;
		return CommandCompletion.CompleteInput(fake_command, fake_command.Length, options: null);
	}

	private string GetFakeCommandPrefix() =>
		_expectedType == "directory" ? "Set-Location " : "Get-Item ";

	private void RedrawLine(List<char> buffer, int cursor_pos)
	{
		string text = new string(buffer.ToArray());
		string prefix = _default != null ? $"{_default} " : "";
		Console.Write($"\r{prefix}> ");
		Console.Write(text);

		int clear_count = Math.Max(0, _previousRenderLength - text.Length);
		if (clear_count > 0)
		{
			Console.Write(new string(' ', clear_count));
			Console.Write(new string('\b', clear_count));
		}

		int backtrack = text.Length - cursor_pos;
		if (backtrack > 0)
			Console.Write(new string('\b', backtrack));

		_previousRenderLength = text.Length;
	}

	private static string StripQuotes(string input)
	{
		if (input.Length >= 2 &&
			((input[0] == '\'' && input[^1] == '\'') ||
			 (input[0] == '"' && input[^1] == '"')))
		{
			return input[1..^1];
		}
		return input;
	}
}
