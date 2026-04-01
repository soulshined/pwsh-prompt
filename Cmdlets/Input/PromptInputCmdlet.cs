using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;
using PwshPrompt.IO;
using PwshPrompt.Enums;
using PwshPrompt.Types;
using PwshPrompt.Utils;

namespace PwshPrompt.Cmdlets;

/// <summary>
/// <para>Prompts the user for typed input with optional validation, type coercion, and retry logic.</para>
///
/// <para><b>Retry behavior:</b> By default the cmdlet re-prompts indefinitely until valid input is
/// received. To limit retries, use <c>-AttemptsAllotment</c> (alias <c>-Attempts</c>) to cap the
/// number of attempts, or use <c>-ErrorAction Stop</c> to terminate on the first invalid input.
/// When the attempt limit is reached a terminating <c>AttemptsExhausted</c> error is thrown.</para>
///
/// <para><b>ErrorAction scope:</b> The <c>-ErrorAction</c> preference only affects non-terminating
/// errors emitted during the retry loop (ErrorIds: <c>InputRequired</c>, <c>InvalidInputType</c>).
/// All other errors are terminating and cannot be suppressed with <c>-ErrorAction</c>.</para>
///
/// <para><b>Terminating errors:</b></para>
/// <list type="bullet">
/// <item><description><c>ParameterDefinitionError</c> — a parameter was defined incorrectly by the caller
/// (invalid Culture name, Message/Title is not a string or hashtable, Validation scriptblock
/// returned an unexpected shape or threw an unexpected exception). These represent developer mistakes.</description></item>
/// <item><description><c>AttemptsExhausted</c> — the user exceeded the allowed number of input attempts.</description></item>
/// </list>
///
/// <para><b>Non-terminating errors (retry loop):</b></para>
/// <list type="bullet">
/// <item><description><c>InputRequired</c> — empty input when <c>-AllowNull</c> is not set and no <c>-Default</c> is provided.</description></item>
/// <item><description><c>InvalidInputType</c> — input could not be converted to <c>-ExpectedType</c> or failed <c>-Validation</c>.</description></item>
/// </list>
/// </summary>
/// <example>
/// <code>Prompt-Input "Enter your name"</code>
/// <para>Prompts indefinitely until a non-empty string is entered.</para>
/// </example>
/// <example>
/// <code>Prompt-Input "Pick a number" -ExpectedType int -AttemptsAllotment 3</code>
/// <para>Allows at most 3 attempts to enter a valid integer before throwing a terminating error.</para>
/// </example>
/// <example>
/// <code>Prompt-Input "Config dir" -ExpectedType directory -Default "~/.config"</code>
/// <para>Prompts for an existing directory with tab completion; defaults to ~/.config on empty input.</para>
/// </example>
[Cmdlet("Prompt", "Input", HelpUri = "https://github.com/soulshined/pwsh-prompt/wiki/cmdlets/Prompt-Input")]
[OutputType(typeof(string))]
[OutputType(typeof(bool))]
[OutputType(typeof(byte))]
[OutputType(typeof(sbyte))]
[OutputType(typeof(char))]
[OutputType(typeof(short))]
[OutputType(typeof(ushort))]
[OutputType(typeof(int))]
[OutputType(typeof(uint))]
[OutputType(typeof(long))]
[OutputType(typeof(ulong))]
[OutputType(typeof(float))]
[OutputType(typeof(double))]
[OutputType(typeof(decimal))]
[OutputType(typeof(DirectoryInfo))]
[OutputType(typeof(FileInfo))]
[OutputType(typeof(Regex))]
[OutputType(typeof(Guid))]
[OutputType(typeof(Version))]
[OutputType(typeof(Uri))]
[OutputType(typeof(DateOnly))]
[OutputType(typeof(DateTime))]
[OutputType(typeof(TimeOnly))]
[OutputType(typeof(TimeZoneInfo))]
public class PromptInputCmdlet : PSCmdlet
{
	/// <summary>
	/// <para>The prompt message to display to the user. Accepts a <see cref="Label"/> configuration:
	/// a plain string or a hashtable.</para>
	///
	/// <para>An invalid type (neither string nor hashtable) throws a terminating
	/// <c>ParameterDefinitionError</c>.</para>
	/// </summary>
	[Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true,
		HelpMessage = "The prompt message. Accepts a Label configuration: a string or hashtable. See `about_Label`.")]
	[ValidateNotNullOrWhiteSpace]
	public object Message { get; set; } = null!;

	/// <summary>
	/// <para>Optional title displayed above the prompt. Defaults to bold styling.
	/// Accepts a <see cref="Label"/> configuration: a plain string or a hashtable.</para>
	///
	/// <para>An invalid type (neither string nor hashtable) throws a terminating
	/// <c>ParameterDefinitionError</c>.</para>
	/// </summary>
	[Parameter(Mandatory = false,
		HelpMessage = "Title displayed above the prompt. Accepts a Label configuration: a string or hashtable. See `about_Label`.")]
	[ValidateNotNullOrWhiteSpace]
	public object? Title { get; set; }

	/// <summary>
	/// The default value returned when the user submits empty input. Displayed as <c>(default: value)</c> in the prompt.
	/// </summary>
	[Parameter(Mandatory = false, HelpMessage = "Fallback value used when the user presses Enter without typing anything.")]
	[ValidateNotNullOrWhiteSpace]
	public string? Default { get; set; }

	/// <summary>
	/// The expected type that user input is coerced to. Defaults to <c>"string"</c>. The output object type
	/// varies accordingly (see the cmdlet's OutputType attributes). Case-sensitive.
	/// For <c>"directory"</c> and <c>"file"</c>, tab completion of filesystem paths is enabled.
	/// </summary>
	[Parameter(Mandatory = false, HelpMessage = "Target type for input coercion. Case-sensitive. Defaults to 'string'.")]
	[ValidateSet("bool", "byte", "sbyte", "char", "string",
		"short", "ushort", "int", "integer", "uint", "long", "ulong",
		"float", "double", "decimal",
		"hex", "directory", "file", "regex", "guid", "version",
		"uri", "date", "datetime", "time", "timezone", IgnoreCase = false)]
	public string ExpectedType { get; set; } = "string";

	/// <summary>
	/// Optional scriptblock invoked after type coercion succeeds. The coerced value is available
	/// as <c>$_</c> (pipeline variable) and <c>$args[0]</c>. Must return a single tuple
	/// <c>@($ok, $message)</c> where <c>$ok</c> is <c>[bool]</c> and <c>$message</c> is
	/// <c>[string]</c> or <c>$null</c>.
	/// When <c>$ok</c> is <c>$false</c>, the <c>$message</c> is shown and the user is re-prompted.
	/// Only invoked on non-null values. A scriptblock that returns an unexpected shape throws a
	/// terminating <c>ParameterDefinitionError</c>.
	/// </summary>
	[Parameter(Mandatory = false,
		HelpMessage = "ScriptBlock invoked with the coerced value as $_ and $args[0]. Must return a single tuple @($ok, $message) where $ok is [bool] and $message is [string] or $null.")]
	public ScriptBlock? Validation { get; set; }

	/// <summary>
	/// Maximum number of input attempts before a terminating <c>AttemptsExhausted</c> error is thrown.
	/// When omitted, the user may retry indefinitely unless <c>-ErrorAction Stop</c> is used.
	/// </summary>
	[Parameter(Mandatory = false, HelpMessage = "Maximum number of attempts. Omit for unlimited retries.")]
	[Alias("Attempts")]
	[ValidateRange(2, int.MaxValue)]
	public int? AttemptsAllotment { get; set; }

	/// <summary>
	/// Overrides the current culture used for parsing culture-sensitive types
	/// (byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal, date, datetime, time).
	/// Accepts a culture name such as <c>"en-US"</c> or <c>"de-DE"</c>.
	/// An invalid culture name throws a terminating <c>ParameterDefinitionError</c>.
	/// </summary>
	[Parameter(Mandatory = false, HelpMessage = "Culture name (e.g. 'en-US') for parsing numeric and date/time types. Defaults to current culture.")]
	public string? Culture { get; set; }

	/// <summary>
	/// When set, allows the user to press Enter without typing anything. The cmdlet outputs <c>$null</c>
	/// instead of re-prompting. Without this switch, empty input produces a non-terminating
	/// <c>InputRequired</c> error and re-prompts.
	/// </summary>
	[Parameter(Mandatory = false, HelpMessage = "Permit empty input. The cmdlet outputs $null when the user presses Enter.")]
	public SwitchParameter AllowNull { get; set; }

	private CultureInfo _culture = null!;
	private Label _messageLabel;
	private string? _default;

	/// <inheritdoc />
	protected override void BeginProcessing()
	{
		#pragma warning disable CS8604
		try { _culture = CultureInfo.GetCultureInfo(Culture); }
		#pragma warning restore CS8604

		catch (ArgumentNullException) {
			_culture = CultureInfo.CurrentCulture;
		}
		catch (CultureNotFoundException)
		{
			ThrowTerminatingError(new ErrorRecord(
				new PSArgumentException($"'{Culture}' is not a valid culture name", nameof(Culture)),
				"ParameterDefinitionError",
				ErrorCategory.InvalidArgument,
				Culture));
		}

		try
		{
			if (Title is not null) {
				Label title = Label.FromParameter(Host.UI.RawUI, null, Title, nameof(Title), TextStyle.Bold);
				Host.UI.WriteLine(title.ToString());
			}

			_messageLabel = Label.FromParameter(Host.UI.RawUI, null, Message, nameof(Message));
		}
		catch (PSArgumentException ex)
		{
			ThrowTerminatingError(new ErrorRecord(
				ex,
				"ParameterDefinitionError",
				ErrorCategory.InvalidArgument,
				null));
		}

		if (!string.IsNullOrWhiteSpace(Default)) {
			_default = Default.Trim();
		}
	}

	/// <inheritdoc />
	protected override void ProcessRecord()
	{
		int attempts = 0;
		while (true)
		{
			if (AttemptsAllotment  is not null && attempts++ == AttemptsAllotment) {
				ThrowTerminatingError(new ErrorRecord(
					new PSInvalidOperationException("Maximum attempts allowed exhausted"),
					"AttemptsExhausted",
					ErrorCategory.LimitsExceeded,
					null));
			}

			Host.UI.WriteLine(_messageLabel.ToString());

			if (_default is not null) {
				Host.UI.Write($"(default: {_default}) ");
			}
			Host.UI.Write("> ");

			string input;
			if (ExpectedType is "directory" or "file")
			{
				try
				{
					input = new PathReadLine(ExpectedType, _default).ReadLine();
				}
				catch (InvalidOperationException)
				{
					input = Host.UI.ReadLine();
				}
			}
			else
			{
				input = Host.UI.ReadLine();
			}

			if (string.IsNullOrWhiteSpace(input))
			{
				if (_default != null)
				{
					input = _default;
				}
				else if (!AllowNull)
				{
					WriteError(new ErrorRecord(
						new PSArgumentException("Input cannot be empty"),
						"InputRequired",
						ErrorCategory.InvalidArgument,
						null));
					continue;
				}
				else
				{
					WriteObject(null);
					return;
				}
			}

			try
			{
				object resolved = Cast(input);
				(bool ok, string? _) = InvokeValidation(resolved);
				if (!ok) { continue; }

				WriteObject(resolved);
				return;
			}
			catch (PSInvalidCastException)
			{
				WriteError(new ErrorRecord(
					new PSArgumentException($"Input '{input}' cannot be converted to {ExpectedType}"),
					"InvalidInputType",
					ErrorCategory.InvalidArgument,
					input));
			}
		}
	}

	/// <summary>
	/// Casts the user input string to the type specified by <see cref="ExpectedType"/>.
	/// </summary>
	/// <param name="input">The raw string input from the user.</param>
	/// <returns>
	/// The converted value based on <see cref="ExpectedType"/>:
	/// <list type="bullet">
	/// <item><description><c>"bool"</c> — <see cref="bool"/></description></item>
	/// <item><description><c>"byte"</c> — <see cref="byte"/></description></item>
	/// <item><description><c>"sbyte"</c> — <see cref="sbyte"/></description></item>
	/// <item><description><c>"char"</c> — <see cref="char"/></description></item>
	/// <item><description><c>"string"</c> — <see cref="string"/></description></item>
	/// <item><description><c>"short"</c> — <see cref="short"/></description></item>
	/// <item><description><c>"ushort"</c> — <see cref="ushort"/></description></item>
	/// <item><description><c>"int"</c> or <c>"integer"</c> — <see cref="int"/></description></item>
	/// <item><description><c>"uint"</c> — <see cref="uint"/></description></item>
	/// <item><description><c>"long"</c> — <see cref="long"/></description></item>
	/// <item><description><c>"ulong"</c> — <see cref="ulong"/></description></item>
	/// <item><description><c>"float"</c> — <see cref="float"/></description></item>
	/// <item><description><c>"double"</c> — <see cref="double"/></description></item>
	/// <item><description><c>"decimal"</c> — <see cref="decimal"/></description></item>
	/// <item><description><c>"hex"</c> — <see cref="string"/> (hexadecimal input with optional 0x prefix)</description></item>
	/// <item><description><c>"directory"</c> — <see cref="DirectoryInfo"/> (must exist on disk)</description></item>
	/// <item><description><c>"file"</c> — <see cref="FileInfo"/> (must exist on disk)</description></item>
	/// <item><description><c>"regex"</c> — <see cref="Regex"/></description></item>
	/// <item><description><c>"guid"</c> — <see cref="Guid"/></description></item>
	/// <item><description><c>"version"</c> — <see cref="Version"/></description></item>
	/// <item><description><c>"uri"</c> — <see cref="Uri"/> (must be absolute)</description></item>
	/// <item><description><c>"date"</c> — <see cref="DateOnly"/></description></item>
	/// <item><description><c>"datetime"</c> — <see cref="DateTime"/></description></item>
	/// <item><description><c>"time"</c> — <see cref="TimeOnly"/></description></item>
	/// <item><description><c>"timezone"</c> — <see cref="TimeZoneInfo"/></description></item>
	/// </list>
	/// </returns>
	/// <exception cref="PSInvalidCastException">Thrown when the input cannot be converted to the expected type.</exception>
	private object Cast(string input) {
		object resolved = ExpectedType switch
		{
			"string" => input,
			"bool" => input.ToLower() switch
			{
				"1" or "y" or "yes" or "on" => true,
				"0" or "n" or "no" or "off" => false,
				_ => throw new PSInvalidCastException()
			},
			"byte" => byte.TryParse(input, NumberStyles.Integer, _culture, out byte b) ? b : throw new PSInvalidCastException(),
			"sbyte" => sbyte.TryParse(input, NumberStyles.Integer, _culture, out sbyte sb) ? sb : throw new PSInvalidCastException(),
			"short" => short.TryParse(input, NumberStyles.Integer, _culture, out short s) ? s : throw new PSInvalidCastException(),
			"ushort" => ushort.TryParse(input, NumberStyles.Integer, _culture, out ushort us) ? us : throw new PSInvalidCastException(),
			"int" or "integer" => int.TryParse(input, NumberStyles.Integer, _culture, out int i) ? i : throw new PSInvalidCastException(),
			"uint" => uint.TryParse(input, NumberStyles.Integer, _culture, out uint ui) ? ui : throw new PSInvalidCastException(),
			"long" => long.TryParse(input, NumberStyles.Integer, _culture, out long l) ? l : throw new PSInvalidCastException(),
			"ulong" => ulong.TryParse(input, NumberStyles.Integer, _culture, out ulong ul) ? ul : throw new PSInvalidCastException(),
			"float" => float.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, _culture, out float f) ? f : throw new PSInvalidCastException(),
			"double" => double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, _culture, out double d) ? d : throw new PSInvalidCastException(),
			"decimal" => decimal.TryParse(input, NumberStyles.Number, _culture, out decimal m) ? m : throw new PSInvalidCastException(),
			"directory" => ResolveDirectory(input),
			"file" => ResolveFile(input),
			"regex" => input.ParseRegex(),
			"guid" => Guid.TryParse(input, out Guid g) ? g : throw new PSInvalidCastException(),
			"char" => input.Length == 1 ? input[0] : throw new PSInvalidCastException(),
			"version" => Version.TryParse(input, out Version? v) ? v : throw new PSInvalidCastException(),
			"uri" => Uri.TryCreate(input, UriKind.Absolute, out Uri? u) ? u : throw new PSInvalidCastException(),
			"date" => DateOnly.TryParse(input, _culture, out DateOnly dt) ? dt : throw new PSInvalidCastException(),
			"datetime" => DateTime.TryParse(input, _culture, out DateTime dtm) ? dtm : throw new PSInvalidCastException(),
			"time" => TimeOnly.TryParse(input, _culture, out TimeOnly t) ? t : throw new PSInvalidCastException(),
			"timezone" => ResolveTimeZone(input),
			"hex" => ParseHex(input),
			_ => throw new NotImplementedException(ExpectedType)
		};

		return resolved;
	}

	/// <summary>
	/// Resolves a PowerShell path string to a <see cref="DirectoryInfo"/> for an existing directory.
	/// Handles PS path provider resolution (relative paths, drive-qualified paths).
	/// </summary>
	/// <param name="input">The raw path string entered by the user.</param>
	/// <returns>A <see cref="DirectoryInfo"/> for an existing directory.</returns>
	/// <exception cref="PSInvalidCastException">Thrown when the path cannot be resolved or the directory does not exist.</exception>
	private DirectoryInfo ResolveDirectory(string input) {
		try {
			string resolved = SessionState.Path.GetUnresolvedProviderPathFromPSPath(input);
			DirectoryInfo info = new(resolved);
			return info.Exists ? info : throw new PSInvalidCastException();
		} catch (PSInvalidCastException) {
			throw;
		} catch {
			throw new PSInvalidCastException();
		}
	}

	/// <summary>
	/// Resolves a PowerShell path string to a <see cref="FileInfo"/> for an existing file.
	/// Handles PS path provider resolution (relative paths, drive-qualified paths).
	/// </summary>
	/// <param name="input">The raw path string entered by the user.</param>
	/// <returns>A <see cref="FileInfo"/> for an existing file.</returns>
	/// <exception cref="PSInvalidCastException">Thrown when the path cannot be resolved or the file does not exist.</exception>
	private FileInfo ResolveFile(string input) {
		try {
			string resolved = SessionState.Path.GetUnresolvedProviderPathFromPSPath(input);
			FileInfo info = new(resolved);
			return info.Exists ? info : throw new PSInvalidCastException();
		} catch (PSInvalidCastException) {
			throw;
		} catch {
			throw new PSInvalidCastException();
		}
	}

	/// <summary>
	/// Parses a hexadecimal string to a <see cref="string"/>. Accepts optional <c>0x</c> or <c>0X</c> prefix.
	/// </summary>
	/// <param name="input">The hex string entered by the user.</param>
	/// <returns>A <see cref="string"/> value without the prefix</returns>
	/// <exception cref="PSInvalidCastException">Thrown when the input is not valid hexadecimal.</exception>
	private static string ParseHex(string input)
	{
		ReadOnlySpan<char> hex = input.AsSpan();
		if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			hex = hex[2..];

		return long.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _)
			? hex.ToString()
			: throw new PSInvalidCastException();
	}

	/// <summary>
	/// Resolves a time zone identifier to a <see cref="TimeZoneInfo"/>.
	/// Accepts system time zone IDs (e.g. <c>"America/New_York"</c>, <c>"UTC"</c>).
	/// </summary>
	/// <param name="input">The time zone identifier entered by the user.</param>
	/// <returns>A <see cref="TimeZoneInfo"/> instance.</returns>
	/// <exception cref="PSInvalidCastException">Thrown when the identifier does not match a known time zone.</exception>
	private static TimeZoneInfo ResolveTimeZone(string input)
	{
		try
		{
			return TimeZoneInfo.FindSystemTimeZoneById(input);
		}
		catch
		{
			throw new PSInvalidCastException();
		}
	}

	/// <summary>
	/// Invokes the <see cref="Validation"/> scriptblock against <paramref name="input"/>.
	/// Returns <c>(true, null)</c> immediately when <see cref="Validation"/> is <c>null</c>.
	/// On a failing result emits a non-terminating <c>InvalidInputType</c> error via <see cref="Cmdlet.WriteError"/>;
	/// the caller should <c>continue</c> the retry loop when <c>ok</c> is <c>false</c>.
	/// Throws a terminating <c>ParameterDefinitionError</c> if the scriptblock returns an
	/// unexpected shape or throws an unexpected exception.
	/// </summary>
	/// <param name="input">The coerced value to validate (or the raw string for the <c>"string"</c> type).</param>
	/// <returns>A tuple of <c>(ok, message)</c>.</returns>
	private (bool, string?) InvokeValidation(object input) {
		if (Validation is null) return (true, null);

		Collection<PSObject> resp;
		try
		{
			resp = Validation.InvokeWithContext(
				null,
				new List<PSVariable> { new("_", input) },
				input);
		}
		catch (Exception ex) when (ex is not PipelineStoppedException)
		{
			ThrowTerminatingError(new ErrorRecord(
				new PSInvalidOperationException($"Validation scriptblock threw an unexpected exception: {ex.Message}", ex),
				"ParameterDefinitionError",
				ErrorCategory.InvalidResult,
				null));
			return (false, null); // unreachable; ThrowTerminatingError always throws
		}

		object? first = resp.Count > 0 ? resp[0]?.BaseObject : null;
		object? second = resp.Count > 1 ? resp[1]?.BaseObject : null;

		if (resp.Count > 2 || first is not bool || (second is not null and not string)) {
			ThrowTerminatingError(new ErrorRecord(
				new PSNotSupportedException("A validation scriptblock result expects a tuple of @(bool ok, string? message)"),
				"ParameterDefinitionError",
				ErrorCategory.InvalidResult,
				null));
		}

		bool ok = (bool)first!;
		string? message = second as string;

		if (!ok) {
			string msg = $"cannot be converted to {ExpectedType}";
			if (!string.IsNullOrWhiteSpace(message)) {
				msg = message;
			}

			WriteError(new ErrorRecord(
				new PSArgumentException($"Input '{input}' {msg}"),
				"InvalidInputType",
				ErrorCategory.InvalidArgument,
				input));
		}

		return (ok, message);

	}
}
