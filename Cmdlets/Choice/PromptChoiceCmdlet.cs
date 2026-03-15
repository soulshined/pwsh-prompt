using System.Collections;
using System.Management.Automation;
using PwshPrompt.IO.Choice;
using PwshPrompt.Configs;
using PwshPrompt.Enums;
using PwshPrompt.Types;
using PwshPrompt.Consts;
using PwshPrompt.Utils;

namespace PwshPrompt.Cmdlets;

/// <summary>
/// <para>Prompts the user to select one or more items from an interactive picker.</para>
///
/// <para>Choices are rendered as a navigable list in the terminal. The user selects via arrow keys,
/// digit keys (1-based page-relative), or hotkeys defined on individual items. When
/// <c>-Multiple</c> is set, the user may toggle multiple selections before confirming with Enter.</para>
///
/// <para><b>Output:</b> An <c>int[]</c> of 0-based indices into the original <c>-Choices</c> array,
/// or <c>$null</c> when the user cancels with Escape or Ctrl-C.</para>
///
/// <para><b>Terminating errors:</b></para>
/// <list type="bullet">
/// <item><description><c>ParameterDefinitionError</c> — a parameter was defined incorrectly by the caller
/// (mixed non-hashtable element types in <c>-Choices</c>, missing <c>Value</c> key in a hashtable
/// choice, unknown key in a hashtable choice, <c>-Message</c> or <c>-Title</c> is not a string or
/// hashtable, <c>-Message</c> text exceeds 228 characters, <c>-Default</c> index is out of range,
/// unknown key in <c>-AlternateBuffer</c>). These represent developer mistakes and are thrown
/// during parameter validation in <c>BeginProcessing</c>.</description></item>
/// <item><description><c>NonInteractiveTerminal</c> — the cmdlet requires an interactive terminal
/// (a real console with <c>Console.KeyAvailable</c> support) but none is attached. Thrown
/// during <c>ProcessRecord</c>.</description></item>
/// </list>
///
/// <para><b>Non-terminating errors:</b> None. All errors are terminating.</para>
///
/// <para><b>Color format:</b> See <see cref="AnsiColor"/> for the color tuple format used by all
/// <c>ForegroundColor</c>/<c>fg</c> and <c>BackgroundColor</c>/<c>bg</c> keys.</para>
///
/// <para><b>Style format:</b> See <see cref="TextStyle"/> for the valid text decoration flags.</para>
///
/// <para><b>Label format:</b> See <see cref="Label"/> for the hashtable format used by
/// <c>-Message</c>, <c>-Title</c>, and nested item/pagination configs.</para>
///
/// <para><b>Buffer configuration:</b> See <see cref="BufferConfig"/>, <see cref="BorderConfig"/>,
/// <see cref="ItemConfig"/>, and <see cref="PaginationConfig"/> for the <c>-AlternateBuffer</c>
/// hashtable schema.</para>
/// </summary>
/// <example>
/// <code>Prompt-Choice @("Red", "Green", "Blue") "Pick a color"</code>
/// <para>Presents a single-select picker. Returns the selected index (e.g. <c>@(1)</c> for "Green").</para>
/// </example>
/// <example>
/// <code>$items = @(
///   @{ Value = "dev"; Description = "Development"; HotKey = "d" },
///   @{ Value = "staging"; HotKey = "s" },
///   @{ Value = "prod"; Description = "Production"; HotKey = "p" }
/// )
/// Prompt-Choice $items "Deploy to:" -Multiple</code>
/// <para>Presents a multi-select picker with hotkeys and descriptions.</para>
/// </example>
/// <example>
/// <code>$dbs = @(
///   @{ Value = "Redis"; HelpMessage = "Fast in-memory cache"; Description = "In-memory key-value store.`nSupports pub/sub, streams, and sorted sets." },
///   @{ Value = "PostgreSQL"; HelpMessage = "Relational DB"; Description = "Advanced open-source relational database.`nFull SQL compliance with JSONB and range types." },
///   @{ Value = "MongoDB"; HelpMessage = "Document store" }
/// )
/// Prompt-Choice $dbs "Pick a database"</code>
/// <para>Items with HelpMessage show it dimmed inline (or in the status bar if too long). Items with Description show a first-line preview; press F1 to view the full description.</para>
/// </example>
/// <example>
/// <code>Prompt-Choice @("A","B","C") "Choose" -Default 2 -CycleMode Cycle</code>
/// <para>Pre-selects index 2 ("C") and wraps navigation within the current page.</para>
/// </example>
/// <example>
/// <code>Prompt-Choice @("One","Two","Three") "Select" -AlternateBuffer @{}</code>
/// <para>Renders the picker in the alternate screen buffer with default styling.</para>
/// </example>
/// <example>
/// <code>Prompt-Choice @("Alpha","Beta","Gamma") "Pick one" -AlternateBuffer @{
///   fg = @("White", "255;255;255")
///   bg = @("DarkBlue", "0;0;139")
///   Border = @{
///     hs = "═"; vs = "║"
///     tl = "╔"; tr = "╗"
///     bl = "╚"; br = "╝"
///     Color = @("Cyan", "0;255;255")
///   }
/// }</code>
/// <para>Renders in the alternate screen buffer with custom colors and double-line box-drawing border.</para>
/// </example>
[Cmdlet("Prompt", "Choice", HelpUri = "https://github.com/soulshined/pwsh-prompt/wiki/cmdlets/Prompt-Choice")]
[OutputType(typeof(int[]))]
public class PromptChoiceCmdlet : PSCmdlet
{

	/// <summary>
	/// <para>The list of choices to present to the user. Must contain at least one element.</para>
	///
	/// <para>Accepts an array of any object type. All non-hashtable elements must share the same type;
	/// mixing different non-hashtable types (e.g. strings and integers) throws a terminating
	/// <c>ParameterDefinitionError</c>. Hashtable elements may be freely intermixed with any
	/// non-hashtable type.</para>
	///
	/// <para>Hashtable elements are parsed into <c>Item</c> structs. Valid keys (case-insensitive):</para>
	/// <list type="bullet">
	/// <item><description><c>Value</c> (required) — the display text for the choice.</description></item>
	/// <item><description><c>Description</c> (alias <c>desc</c>) — full description text. The first line is shown as a dimmed preview under the focused item. Press F1 to open a scrollable details view of the complete description. Supports multi-line text (<c>`n</c> in PowerShell).</description></item>
	/// <item><description><c>HotKey</c> (alias <c>hk</c>) — a single character that selects this choice when pressed.</description></item>
	/// <item><description><c>HelpMessage</c> (alias <c>help</c>) — short help text shown dimmed inline next to the item when space permits; otherwise displayed in italic in the status bar above the legend.</description></item>
	/// <item><description><c>Style</c> — text style (see <see cref="TextStyle"/>).</description></item>
	/// <item><description><c>ForegroundColor</c> (alias <c>fg</c>) — foreground color (see <see cref="AnsiColor"/>).</description></item>
	/// <item><description><c>BackgroundColor</c> (alias <c>bg</c>) — background color (see <see cref="AnsiColor"/>).</description></item>
	/// </list>
	///
	/// <para>A hashtable missing the <c>Value</c> key, or containing an unknown key, throws a
	/// terminating <c>ParameterDefinitionError</c>.</para>
	/// </summary>
	[Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true,
		HelpMessage = "Array of choices to present. Must not be empty")]
	[ValidateNotNullOrEmpty]
	public object[] Choices { get; set; } = null!;

	/// <summary>
	/// <para>The prompt message displayed above the choice list. Accepts a <see cref="Label"/>
	/// configuration: a plain string or a hashtable.</para>
	///
	/// <para>The resolved text must not exceed 228 characters (3 lines × 76 columns). Exceeding this
	/// limit throws a terminating <c>ParameterDefinitionError</c>.</para>
	///
	/// <para>An invalid type (neither string nor hashtable) throws a terminating
	/// <c>ParameterDefinitionError</c>.</para>
	/// </summary>
	[Parameter(Position = 1, Mandatory = true,
		HelpMessage = "The prompt message. Accepts a Label configuration: a string or hashtable. See `about_Label`.")]
	[ValidateNotNullOrWhiteSpace]
	public object Message { get; set; } = null!;

	/// <summary>
	/// <para>Optional title displayed above the prompt message. When using the inline renderer,
	/// the title defaults to a snow-white foreground with bold styling.</para>
	///
	/// <para>Accepts a <see cref="Label"/> configuration: a plain string or a hashtable.</para>
	///
	/// <para>An invalid type (neither string nor hashtable) throws a terminating
	/// <c>ParameterDefinitionError</c>.</para>
	/// </summary>
	[Parameter(Mandatory = false,
		HelpMessage = "Title displayed above the prompt. Accepts a Label configuration: a string or hashtable. See `about_Label`.")]
	[ValidateNotNullOrWhiteSpace]
	public object? Title { get; set; }

	/// <summary>
	/// <para>The 0-based index into <c>-Choices</c> to pre-select when the picker appears.
	/// Defaults to <c>0</c> (the first item).</para>
	///
	/// <para>Must be non-negative (enforced by <c>[ValidateRange]</c>) and less than the number of
	/// choices. An out-of-range value throws a terminating <c>ParameterDefinitionError</c>.</para>
	/// </summary>
	[Parameter(Mandatory = false, HelpMessage = "0-based index of the initially selected choice. Must be less than the number of choices.")]
	[ValidateRange(0, int.MaxValue)]
	public int Default { get; set; } = 0;

	/// <summary>
	/// <para>Arrow-key boundary behavior when the cursor reaches the top or bottom of a page.
	/// Valid <see cref="CycleMode"/> values:</para>
	/// <list type="bullet">
	/// <item><description><c>Next</c> (default) — advances to the next or previous page.</description></item>
	/// <item><description><c>Cycle</c> — wraps around within the current page.</description></item>
	/// <item><description><c>Stop</c> — halts at the boundary; no movement.</description></item>
	/// </list>
	/// </summary>
	[Parameter(Mandatory = false, HelpMessage = "Arrow-key boundary behavior: Next, Cycle, or Stop")]
	public CycleMode CycleMode { get; set; } = CycleMode.Next;

	/// <summary>
	/// When set, the user may toggle multiple selections (with Space or hotkeys) before confirming
	/// with Enter. The output array contains all selected indices.
	/// </summary>
	[Parameter(Mandatory = false, HelpMessage = "Allow multi-select. Users toggle choices with Space and confirm with Enter.")]
	public SwitchParameter Multiple { get; set; }

	/// <summary>
	/// <para>Renders the picker in the terminal's alternate screen buffer instead of inline.
	/// Pass an empty hashtable <c>@{}</c> for default appearance, or customize.
	/// See <see cref="BufferConfig"/> for the full hashtable schema.</para>
	///
	/// <para>Hashtable keys (case-insensitive):</para>
	/// <list type="bullet">
	/// <item><description><c>ForegroundColor</c> / <c>fg</c> — base foreground color (see <see cref="AnsiColor"/>).</description></item>
	/// <item><description><c>BackgroundColor</c> / <c>bg</c> — base background color (see <see cref="AnsiColor"/>).</description></item>
	/// <item><description><c>Border</c> — border characters and color (see <see cref="BorderConfig"/>).</description></item>
	/// <item><description><c>Item</c> — item appearance and multi-select indicators (see <see cref="ItemConfig"/>).</description></item>
	/// <item><description><c>Pagination</c> — pagination indicator labels (see <see cref="PaginationConfig"/>).</description></item>
	/// </list>
	///
	/// <para>An unknown key throws a terminating <c>ParameterDefinitionError</c>.</para>
	/// </summary>
	[Parameter(Mandatory = false,
		HelpMessage = "Alternate buffer appearance. @{} for defaults. See `about_BufferConfig`.")]
	public Hashtable? AlternateBuffer { get; set; }

	private Types.Items _choices;
	private Label _messageLabel;
	private Label? _titleLabel;
	private BufferConfig? _bufferConfig;

	/// <inheritdoc />
	protected override void BeginProcessing()
	{
		try
		{
			_choices = Types.Items.FromParameter(Choices, nameof(Choices));
		}
		catch (PSArgumentException ex)
		{
			ThrowTerminatingError(new ErrorRecord(
				ex,
				"ParameterDefinitionError",
				ErrorCategory.InvalidArgument,
				Choices));
		}

		try
		{
			if (AlternateBuffer is not null)
				_bufferConfig = BufferConfig.FromParameter(AlternateBuffer, nameof(AlternateBuffer), Host.UI.RawUI);

			_messageLabel = Label.FromParameter(Host.UI.RawUI, _bufferConfig, Message, nameof(Message));

			if (Title is not null) {
				_titleLabel = Label.FromParameter(Host.UI.RawUI, _bufferConfig, Title, nameof(Title), TextStyle.Bold);

				if (_bufferConfig is null || Title is string) {
					Label label = _titleLabel.Value;
					label.ForegroundColor = ANSI.COLOR.SNOW;
					_titleLabel = label;
				}
			}

			if (_bufferConfig is null || Message is string) {
				Label label = _messageLabel;
				label.ForegroundColor = ANSI.COLOR.SNOW;
				_messageLabel = label;
			}
		}
		catch (PSArgumentException ex)
		{
			ThrowTerminatingError(new ErrorRecord(
				ex,
				"ParameterDefinitionError",
				ErrorCategory.InvalidArgument,
				ex.ParamName == nameof(Message) ? Message : ex.ParamName == nameof(Title) ? Title : AlternateBuffer));
		}

		if (_messageLabel.Text.VisualWidth() > 76 * 3)
		{
			ThrowTerminatingError(new ErrorRecord(
				new PSArgumentException(
					"Message text exceeds the maximum allowed length of 228 characters.",
					nameof(Message)),
				"ParameterDefinitionError",
				ErrorCategory.InvalidArgument,
				Message));
		}

		if (Default >= _choices.Count)
		{
			ThrowTerminatingError(new ErrorRecord(
				new PSArgumentException(
					$"Default index {Default} is out of range. Must be less than {_choices.Count}",
					nameof(Default)),
				"ParameterDefinitionError",
				ErrorCategory.InvalidArgument,
				Default));
		}
	}

	/// <inheritdoc />
	protected override void ProcessRecord()
	{
		try
		{
			Picker selector = new(
				_choices.ToArray(),
				_messageLabel,
				_titleLabel,
				CycleMode,
				_bufferConfig,
				Default,
				Multiple.IsPresent,
				Host.UI.RawUI,
				new PwshPrompt.IO.Buffer());

			int[]? result = selector.Run();
			if (result != null)
				WriteObject(result);
		}
		catch (InvalidOperationException)
		{
			ThrowTerminatingError(new ErrorRecord(
				new InvalidOperationException("Prompt-Choice requires an interactive terminal"),
				"NonInteractiveTerminal",
				ErrorCategory.ResourceUnavailable,
				null));
		}
	}
}
