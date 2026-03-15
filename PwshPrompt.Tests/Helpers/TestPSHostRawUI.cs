using System.Management.Automation.Host;

namespace PwshPrompt.Tests.Helpers;

internal sealed class TestPSHostRawUI : PSHostRawUserInterface
{
	public override ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Gray;
	public override ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
	public override Coordinates WindowPosition { get; set; } = new Coordinates(0, 0);
	public override Size WindowSize { get; set; } = new Size(80, 24);
	public override Size MaxWindowSize => new Size(80, 24);
	public override Size MaxPhysicalWindowSize => new Size(80, 24);
	public override string WindowTitle { get; set; } = "Test";
	public override Coordinates CursorPosition { get; set; } = new Coordinates(0, 0);
	public override int CursorSize { get; set; } = 25;
	public override Size BufferSize { get; set; } = new Size(80, 24);
	public override bool KeyAvailable => false;

	public override void FlushInputBuffer() { }
	public override BufferCell[,] GetBufferContents(Rectangle rectangle) => throw new NotSupportedException();
	public override KeyInfo ReadKey(ReadKeyOptions options) => throw new NotSupportedException();
	public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill) => throw new NotSupportedException();
	public override void SetBufferContents(Coordinates origin, BufferCell[,] contents) => throw new NotSupportedException();
	public override void SetBufferContents(Rectangle rectangle, BufferCell fill) => throw new NotSupportedException();
}
