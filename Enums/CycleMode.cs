namespace PwshPrompt.Enums;

/// <summary>Controls what happens when the user moves past the last item.</summary>
public enum CycleMode
{
	/// <summary>Advance to the next group (if any).</summary>
	Next,
	/// <summary>Wrap around to the first item.</summary>
	Cycle,
	/// <summary>Stay on the current item.</summary>
	Stop
}
