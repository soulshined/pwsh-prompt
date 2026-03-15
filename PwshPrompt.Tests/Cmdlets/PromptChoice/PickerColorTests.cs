using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Host;
using PwshPrompt.Configs;
using PwshPrompt.Consts;
using PwshPrompt.Enums;
using PwshPrompt.IO.Choice;
using PwshPrompt.Tests.Helpers;
using PwshPrompt.Types;
using Xunit;
using static PwshPrompt.Tests.Helpers.TestBuffer;

namespace PwshPrompt.Tests.Cmdlets.PromptChoice;

public class PickerColorTests
{
	private static TestPSHostRawUI MakeRawUI(
		ConsoleColor fg = ConsoleColor.Gray, ConsoleColor bg = ConsoleColor.Black)
		=> new() { ForegroundColor = fg, BackgroundColor = bg };

	private static Item[] MakeItems(params string[] values)
		=> values.Select(v => new Item { Value = v, DisplayText = v }).ToArray();

	private static Item[] MakeItems(int count)
		=> Enumerable.Range(1, count).Select(i => new Item { Value = $"item{i}", DisplayText = $"item{i}" }).ToArray();

	private static BufferConfig MakeBufferConfig(PSHostRawUserInterface rawUI)
		=> BufferConfig.FromParameter(new Hashtable(), "AlternateBuffer", rawUI);

	private static Picker CreatePicker(
		Item[] items,
		TestBuffer buffer,
		PSHostRawUserInterface rawUI,
		bool multiple = false,
		CycleMode cycleMode = CycleMode.Next,
		int defaultIndex = 0,
		BufferConfig? bufferConfig = null,
		string message = "Pick",
		string? title = null)
	{
		Label? titleLabel = title != null ? new Label(rawUI, bufferConfig, title) : null;
		return new Picker(
			items,
			new Label(rawUI, bufferConfig, message),
			titleLabel,
			cycleMode,
			bufferConfig,
			defaultIndex,
			multiple,
			rawUI,
			buffer);
	}

	#region Message colors from RawUI (inline, no BufferConfig)

	[Fact]
	public void Message_uses_RawUI_foreground_color()
	{
		var rawUI = MakeRawUI(fg: ConsoleColor.Cyan);
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b"), buf, rawUI);
		picker.Run();
		Assert.Contains(AnsiColor.From(ConsoleColor.Cyan).Fg, buf.AllWritten);
	}

	[Fact]
	public void Message_uses_RawUI_background_color()
	{
		var rawUI = MakeRawUI(bg: ConsoleColor.DarkBlue);
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b"), buf, rawUI);
		picker.Run();
		Assert.Contains(AnsiColor.From(ConsoleColor.DarkBlue).Bg, buf.AllWritten);
	}

	[Fact]
	public void Different_RawUI_fg_changes_message_output()
	{
		var rawUI1 = MakeRawUI(fg: ConsoleColor.Red);
		var rawUI2 = MakeRawUI(fg: ConsoleColor.Blue);
		var buf1 = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var buf2 = new TestBuffer(Key(ConsoleKey.Enter, '\r'));

		CreatePicker(MakeItems("a", "b"), buf1, rawUI1).Run();
		CreatePicker(MakeItems("a", "b"), buf2, rawUI2).Run();

		Assert.Contains(AnsiColor.From(ConsoleColor.Red).Fg, buf1.AllWritten);
		Assert.DoesNotContain(AnsiColor.From(ConsoleColor.Blue).Fg, buf1.AllWritten);
		Assert.Contains(AnsiColor.From(ConsoleColor.Blue).Fg, buf2.AllWritten);
		Assert.DoesNotContain(AnsiColor.From(ConsoleColor.Red).Fg, buf2.AllWritten);
	}

	#endregion

	#region Title colors from RawUI

	[Fact]
	public void Title_uses_RawUI_foreground_color()
	{
		var rawUI = MakeRawUI(fg: ConsoleColor.Magenta);
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b"), buf, rawUI, title: "T");
		picker.Run();
		Assert.Contains(AnsiColor.From(ConsoleColor.Magenta).Fg, buf.AllWritten);
	}

	[Fact]
	public void Title_uses_RawUI_background_color()
	{
		var rawUI = MakeRawUI(bg: ConsoleColor.DarkGreen);
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b"), buf, rawUI, title: "T");
		picker.Run();
		Assert.Contains(AnsiColor.From(ConsoleColor.DarkGreen).Bg, buf.AllWritten);
	}

	#endregion

	#region BufferConfig overrides RawUI for Labels

	[Fact]
	public void BufferConfig_overrides_RawUI_for_message_fg()
	{
		var rawUI = MakeRawUI(fg: ConsoleColor.Red);
		var bc = MakeBufferConfig(rawUI);
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b"), buf, rawUI, bufferConfig: bc);
		picker.Run();
		Assert.Contains(ANSI.COLOR.BEIGE.Fg, buf.AllWritten);
		Assert.DoesNotContain(AnsiColor.From(ConsoleColor.Red).Fg, buf.AllWritten);
	}

	[Fact]
	public void BufferConfig_overrides_RawUI_for_message_bg()
	{
		var rawUI = MakeRawUI(bg: ConsoleColor.Blue);
		var bc = MakeBufferConfig(rawUI);
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b"), buf, rawUI, bufferConfig: bc);
		picker.Run();
		Assert.Contains(ANSI.COLOR.GREEN.Bg, buf.AllWritten);
		Assert.DoesNotContain(AnsiColor.From(ConsoleColor.Blue).Bg, buf.AllWritten);
	}

	[Fact]
	public void BufferConfig_overrides_RawUI_for_title()
	{
		var rawUI = MakeRawUI(fg: ConsoleColor.Red);
		var bc = MakeBufferConfig(rawUI);
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b"), buf, rawUI, bufferConfig: bc, title: "T");
		picker.Run();
		Assert.Contains(ANSI.COLOR.BEIGE.Fg, buf.AllWritten);
	}

	#endregion

	#region Items use correct color source

	[Fact]
	public void Inline_items_use_reset_foreground()
	{
		var rawUI = MakeRawUI(fg: ConsoleColor.Red);
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b"), buf, rawUI);
		picker.Run();
		Assert.Contains("\x1b[39m", buf.AllWritten);
	}

	[Fact]
	public void Alternate_items_use_BufferConfig_item_fg()
	{
		var rawUI = MakeRawUI();
		var bc = MakeBufferConfig(rawUI);
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b"), buf, rawUI, bufferConfig: bc);
		picker.Run();
		Assert.Contains(ANSI.COLOR.BEIGE.Fg, buf.AllWritten);
	}

	#endregion

	#region Picker-level fg/bg fields

	[Fact]
	public void Inline_picker_uses_reset_sequences()
	{
		var rawUI = MakeRawUI(fg: ConsoleColor.Yellow);
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems(30), buf, rawUI);
		picker.Run();
		Assert.Contains("\x1b[39m", buf.AllWritten);
		Assert.Contains("\x1b[49m", buf.AllWritten);
	}

	[Fact]
	public void Alternate_picker_uses_BufferConfig_colors()
	{
		var rawUI = MakeRawUI();
		var bc = MakeBufferConfig(rawUI);
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems(30), buf, rawUI, bufferConfig: bc);
		picker.Run();
		Assert.Contains(ANSI.COLOR.BEIGE.Fg, buf.AllWritten);
		Assert.Contains(ANSI.COLOR.GREEN.Bg, buf.AllWritten);
	}

	#endregion

	#region Legend always GRAY_LIGHT

	[Fact]
	public void Legend_uses_gray_light_inline()
	{
		var rawUI = MakeRawUI(fg: ConsoleColor.Red);
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b"), buf, rawUI);
		picker.Run();
		Assert.Contains(ANSI.COLOR.GRAY_LIGHT.Fg, buf.AllWritten);
	}

	[Fact]
	public void Legend_uses_gray_light_alternate()
	{
		var rawUI = MakeRawUI();
		var bc = MakeBufferConfig(rawUI);
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b"), buf, rawUI, bufferConfig: bc);
		picker.Run();
		Assert.Contains(ANSI.COLOR.GRAY_LIGHT.Fg, buf.AllWritten);
	}

	#endregion

	#region Tagline decoration

	[Fact]
	public void Inline_tagline_uses_DIM()
	{
		var rawUI = MakeRawUI();
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b"), buf, rawUI, multiple: true);
		picker.Run();
		Assert.Contains("\x1b[2m", buf.AllWritten);
	}

	[Fact]
	public void Alternate_tagline_uses_border_color()
	{
		var rawUI = MakeRawUI();
		var bc = MakeBufferConfig(rawUI);
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b"), buf, rawUI, bufferConfig: bc, multiple: true);
		picker.Run();
		Assert.Contains(ANSI.COLOR.BEIGE.Fg, buf.AllWritten);
	}

	#endregion

	#region Color resets on exit

	[Fact]
	public void Inline_exit_resets_foreground()
	{
		var rawUI = MakeRawUI();
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b"), buf, rawUI);
		picker.Run();
		string last_writes = string.Concat(buf.Written.TakeLast(5));
		Assert.Contains("\x1b[39m", last_writes);
	}

	[Fact]
	public void Alternate_exit_emits_full_RESET()
	{
		var rawUI = MakeRawUI();
		var bc = MakeBufferConfig(rawUI);
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b"), buf, rawUI, bufferConfig: bc);
		picker.Run();
		string all = buf.AllWritten;
		int reset_pos = all.LastIndexOf("\x1b[0m");
		int alt_off_pos = all.LastIndexOf("\x1b[?1049l");
		Assert.True(reset_pos >= 0, "Expected \\x1b[0m in output");
		Assert.True(alt_off_pos >= 0, "Expected \\x1b[?1049l in output");
		Assert.True(reset_pos < alt_off_pos, "\\x1b[0m should appear before \\x1b[?1049l");
	}

	#endregion

	#region Mutating console colors changes output

	[Fact]
	public void Changing_RawUI_fg_changes_message_color()
	{
		var rawUI = MakeRawUI(fg: ConsoleColor.Red);
		var buf1 = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		CreatePicker(MakeItems("a", "b"), buf1, rawUI).Run();

		rawUI.ForegroundColor = ConsoleColor.Green;
		var buf2 = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		CreatePicker(MakeItems("a", "b"), buf2, rawUI).Run();

		Assert.Contains(AnsiColor.From(ConsoleColor.Red).Fg, buf1.AllWritten);
		Assert.Contains(AnsiColor.From(ConsoleColor.Green).Fg, buf2.AllWritten);
		Assert.DoesNotContain(AnsiColor.From(ConsoleColor.Red).Fg, buf2.AllWritten);
	}

	[Fact]
	public void Changing_RawUI_bg_changes_message_color()
	{
		var rawUI = MakeRawUI(bg: ConsoleColor.DarkCyan);
		var buf1 = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		CreatePicker(MakeItems("a", "b"), buf1, rawUI).Run();

		rawUI.BackgroundColor = ConsoleColor.DarkMagenta;
		var buf2 = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		CreatePicker(MakeItems("a", "b"), buf2, rawUI).Run();

		Assert.Contains(AnsiColor.From(ConsoleColor.DarkCyan).Bg, buf1.AllWritten);
		Assert.Contains(AnsiColor.From(ConsoleColor.DarkMagenta).Bg, buf2.AllWritten);
		Assert.DoesNotContain(AnsiColor.From(ConsoleColor.DarkCyan).Bg, buf2.AllWritten);
	}

	[Fact]
	public void Changing_RawUI_fg_changes_title_color()
	{
		var rawUI = MakeRawUI(fg: ConsoleColor.Yellow);
		var buf1 = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		CreatePicker(MakeItems("a", "b"), buf1, rawUI, title: "T").Run();

		rawUI.ForegroundColor = ConsoleColor.White;
		var buf2 = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		CreatePicker(MakeItems("a", "b"), buf2, rawUI, title: "T").Run();

		Assert.Contains(AnsiColor.From(ConsoleColor.Yellow).Fg, buf1.AllWritten);
		Assert.Contains(AnsiColor.From(ConsoleColor.White).Fg, buf2.AllWritten);
	}

	[Fact]
	public void Changing_RawUI_colors_changes_both_message_and_title()
	{
		var rawUI = MakeRawUI(fg: ConsoleColor.Red, bg: ConsoleColor.Blue);
		var buf1 = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		CreatePicker(MakeItems("a", "b"), buf1, rawUI, title: "T").Run();

		rawUI.ForegroundColor = ConsoleColor.Cyan;
		rawUI.BackgroundColor = ConsoleColor.DarkGreen;
		var buf2 = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		CreatePicker(MakeItems("a", "b"), buf2, rawUI, title: "T").Run();

		Assert.Contains(AnsiColor.From(ConsoleColor.Red).Fg, buf1.AllWritten);
		Assert.Contains(AnsiColor.From(ConsoleColor.Blue).Bg, buf1.AllWritten);
		Assert.Contains(AnsiColor.From(ConsoleColor.Cyan).Fg, buf2.AllWritten);
		Assert.Contains(AnsiColor.From(ConsoleColor.DarkGreen).Bg, buf2.AllWritten);
	}

	#endregion

	#region No color leakage

	[Fact]
	public void Inline_mode_never_contains_BufferConfig_defaults()
	{
		var rawUI = MakeRawUI(fg: ConsoleColor.Red);
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b"), buf, rawUI);
		picker.Run();
		Assert.DoesNotContain(ANSI.COLOR.BEIGE.Fg, buf.AllWritten);
		Assert.DoesNotContain(ANSI.COLOR.GREEN.Bg, buf.AllWritten);
	}

	#endregion
}
