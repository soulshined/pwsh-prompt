namespace PwshPrompt.Types;

internal readonly record struct Keychord(ConsoleKey Key, ConsoleModifiers Modifiers = 0);
