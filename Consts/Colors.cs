namespace PwshPrompt.Consts;

/// <summary>
/// Predefined color tuples for all <see cref="System.ConsoleColor"/> members.
/// Each property returns a 2-element string array compatible with any color
/// parameter in pwsh-prompt (ForegroundColor, BackgroundColor, etc.).
/// Only ConsoleColor values are supported; for custom 256-color indices or
/// arbitrary RGB values, use the manual tuple format: @("index", "r;g;b").
/// Registered as a <c>[Colors]</c> type accelerator on module import.
/// </summary>
public sealed class Colors
{
	/// <summary>Color tuple for <see cref="ConsoleColor.Black"/> (index 0).</summary>
	public static string[] BLACK       => new[] { "0",  "0;0;0" };
	/// <summary>Color tuple for <see cref="ConsoleColor.DarkRed"/> (index 1).</summary>
	public static string[] DARKRED     => new[] { "1",  "128;0;0" };
	/// <summary>Color tuple for <see cref="ConsoleColor.DarkGreen"/> (index 2).</summary>
	public static string[] DARKGREEN   => new[] { "2",  "0;128;0" };
	/// <summary>Color tuple for <see cref="ConsoleColor.DarkYellow"/> (index 3).</summary>
	public static string[] DARKYELLOW  => new[] { "3",  "128;128;0" };
	/// <summary>Color tuple for <see cref="ConsoleColor.DarkBlue"/> (index 4).</summary>
	public static string[] DARKBLUE    => new[] { "4",  "0;0;128" };
	/// <summary>Color tuple for <see cref="ConsoleColor.DarkMagenta"/> (index 5).</summary>
	public static string[] DARKMAGENTA => new[] { "5",  "128;0;128" };
	/// <summary>Color tuple for <see cref="ConsoleColor.DarkCyan"/> (index 6).</summary>
	public static string[] DARKCYAN    => new[] { "6",  "0;128;128" };
	/// <summary>Color tuple for <see cref="ConsoleColor.Gray"/> (index 7).</summary>
	public static string[] GRAY        => new[] { "7",  "192;192;192" };
	/// <summary>Color tuple for <see cref="ConsoleColor.DarkGray"/> (index 8).</summary>
	public static string[] DARKGRAY    => new[] { "8",  "128;128;128" };
	/// <summary>Color tuple for <see cref="ConsoleColor.Red"/> (index 9).</summary>
	public static string[] RED         => new[] { "9",  "255;0;0" };
	/// <summary>Color tuple for <see cref="ConsoleColor.Green"/> (index 10).</summary>
	public static string[] GREEN       => new[] { "10", "0;255;0" };
	/// <summary>Color tuple for <see cref="ConsoleColor.Yellow"/> (index 11).</summary>
	public static string[] YELLOW      => new[] { "11", "255;255;0" };
	/// <summary>Color tuple for <see cref="ConsoleColor.Blue"/> (index 12).</summary>
	public static string[] BLUE        => new[] { "12", "0;0;255" };
	/// <summary>Color tuple for <see cref="ConsoleColor.Magenta"/> (index 13).</summary>
	public static string[] MAGENTA     => new[] { "13", "255;0;255" };
	/// <summary>Color tuple for <see cref="ConsoleColor.Cyan"/> (index 14).</summary>
	public static string[] CYAN        => new[] { "14", "0;255;255" };
	/// <summary>Color tuple for <see cref="ConsoleColor.White"/> (index 15).</summary>
	public static string[] WHITE       => new[] { "15", "255;255;255" };
}
