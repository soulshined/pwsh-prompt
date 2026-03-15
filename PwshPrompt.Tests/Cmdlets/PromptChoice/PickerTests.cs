using System.Management.Automation;
using System.Management.Automation.Host;
using PwshPrompt.Configs;
using PwshPrompt.Enums;
using PwshPrompt.IO.Choice;
using PwshPrompt.Tests.Helpers;
using PwshPrompt.Types;
using Xunit;
using static PwshPrompt.Tests.Helpers.TestBuffer;

namespace PwshPrompt.Tests.Cmdlets.PromptChoice;

public class PickerTests
{
	private static readonly PSHostRawUserInterface RawUI = new TestPSHostRawUI();

	private static Item[] MakeItems(params string[] values)
		=> values.Select(v => new Item { Value = v, DisplayText = v }).ToArray();

	private static Item[] MakeItemsWithHotKey(params (string value, char hotkey)[] entries)
		=> entries.Select(e => new Item { Value = e.value, DisplayText = e.value, HotKey = e.hotkey }).ToArray();

	private static Label MakeLabel(string text)
		=> new(RawUI, null, text);

	private static Picker CreatePicker(
		Item[] items,
		TestBuffer buffer,
		bool multiple = false,
		CycleMode cycleMode = CycleMode.Next,
		int defaultIndex = 0,
		BufferConfig? bufferConfig = null,
		string message = "Pick",
		string? title = null)
	{
		Label? titleLabel = title != null ? new Label(RawUI, bufferConfig, title) : null;
		return new Picker(
			items,
			new Label(RawUI, bufferConfig, message),
			titleLabel,
			cycleMode,
			bufferConfig,
			defaultIndex,
			multiple,
			RawUI,
			buffer);
	}

	#region Single Selection (inline)

	[Fact]
	public void Enter_selects_default_index()
	{
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf);
		int[]? result = picker.Run();
		Assert.NotNull(result);
		Assert.Equal(new[] { 0 }, result);
	}

	[Fact]
	public void Down_then_Enter_selects_second()
	{
		var buf = new TestBuffer(
			Key(ConsoleKey.DownArrow),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 1 }, result);
	}

	[Fact]
	public void Up_at_top_with_Stop_stays()
	{
		var buf = new TestBuffer(
			Key(ConsoleKey.UpArrow),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf, cycleMode: CycleMode.Stop);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 0 }, result);
	}

	[Fact]
	public void Up_at_top_with_Cycle_wraps_to_last()
	{
		var buf = new TestBuffer(
			Key(ConsoleKey.UpArrow),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf, cycleMode: CycleMode.Cycle);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 2 }, result);
	}

	[Fact]
	public void Escape_returns_null()
	{
		var buf = new TestBuffer(Key(ConsoleKey.Escape));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf);
		int[]? result = picker.Run();
		Assert.Null(result);
	}

	[Fact]
	public void Hotkey_selects_item()
	{
		var items = MakeItemsWithHotKey(("Apple", 'a'), ("Banana", 'b'), ("Cherry", 'c'));
		var buf = new TestBuffer(Key(ConsoleKey.A, 'a'));
		var picker = CreatePicker(items, buf);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 0 }, result);
	}

	[Fact]
	public void Digit_1_selects_first_item()
	{
		var buf = new TestBuffer(Key(ConsoleKey.D1, '1'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 0 }, result);
	}

	[Fact]
	public void Digit_3_selects_third_item()
	{
		var buf = new TestBuffer(Key(ConsoleKey.D3, '3'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 2 }, result);
	}

	[Fact]
	public void Default_index_respected()
	{
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf, defaultIndex: 2);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 2 }, result);
	}

	#endregion

	#region Multiple Selection (inline)

	[Fact]
	public void Space_toggles_then_Enter_returns()
	{
		var buf = new TestBuffer(
			Key(ConsoleKey.Spacebar, ' '),
			Key(ConsoleKey.DownArrow),
			Key(ConsoleKey.Spacebar, ' '),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf, multiple: true);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 0, 1 }, result);
	}

	[Fact]
	public void Enter_with_no_selection_returns_empty()
	{
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf, multiple: true);
		int[]? result = picker.Run();
		Assert.NotNull(result);
		Assert.Empty(result);
	}

	[Fact]
	public void Space_twice_deselects()
	{
		var buf = new TestBuffer(
			Key(ConsoleKey.Spacebar, ' '),
			Key(ConsoleKey.Spacebar, ' '),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf, multiple: true);
		int[]? result = picker.Run();
		Assert.NotNull(result);
		Assert.Empty(result);
	}

	[Fact]
	public void Hotkey_toggles_in_multiple_mode()
	{
		var items = MakeItemsWithHotKey(("Apple", 'a'), ("Banana", 'b'));
		var buf = new TestBuffer(
			Key(ConsoleKey.A, 'a'),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf, multiple: true);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 0 }, result);
	}

	[Fact]
	public void Ctrl_A_selects_all_on_page()
	{
		var buf = new TestBuffer(
			Key(ConsoleKey.A, '\x01', ConsoleModifiers.Control),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf, multiple: true);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 0, 1, 2 }, result);
	}

	[Fact]
	public void Ctrl_A_twice_deselects_all()
	{
		var buf = new TestBuffer(
			Key(ConsoleKey.A, '\x01', ConsoleModifiers.Control),
			Key(ConsoleKey.A, '\x01', ConsoleModifiers.Control),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf, multiple: true);
		int[]? result = picker.Run();
		Assert.NotNull(result);
		Assert.Empty(result);
	}

	#endregion

	#region Alternate Screen Buffer

	private static BufferConfig MakeBufferConfig()
		=> BufferConfig.FromParameter(new System.Collections.Hashtable(), "AlternateBuffer", RawUI);

	[Fact]
	public void Alternate_screen_enabled_on_start()
	{
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf, bufferConfig: MakeBufferConfig());
		picker.Run();
		Assert.Contains("\x1b[?1049h", buf.AllWritten);
	}

	[Fact]
	public void Alternate_screen_disabled_on_exit()
	{
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf, bufferConfig: MakeBufferConfig());
		picker.Run();
		Assert.Contains("\x1b[?1049l", buf.AllWritten);
	}

	[Fact]
	public void Border_rendered_in_alternate()
	{
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf, bufferConfig: MakeBufferConfig());
		picker.Run();
		string output = buf.AllWritten;
		Assert.Contains("\u250c", output); // ┌
		Assert.Contains("\u2510", output); // ┐
		Assert.Contains("\u2514", output); // └
		Assert.Contains("\u2518", output); // ┘
	}

	[Fact]
	public void Enter_selects_in_alternate_mode()
	{
		var buf = new TestBuffer(
			Key(ConsoleKey.DownArrow),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf, bufferConfig: MakeBufferConfig());
		int[]? result = picker.Run();
		Assert.Equal(new[] { 1 }, result);
	}

	[Fact]
	public void Escape_returns_null_in_alternate()
	{
		var buf = new TestBuffer(Key(ConsoleKey.Escape));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf, bufferConfig: MakeBufferConfig());
		int[]? result = picker.Run();
		Assert.Null(result);
	}

	[Fact]
	public void Multiple_toggle_in_alternate()
	{
		var buf = new TestBuffer(
			Key(ConsoleKey.Spacebar, ' '),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf, multiple: true, bufferConfig: MakeBufferConfig());
		int[]? result = picker.Run();
		Assert.Equal(new[] { 0 }, result);
	}

	[Fact]
	public void Down_navigation_in_alternate()
	{
		var buf = new TestBuffer(
			Key(ConsoleKey.DownArrow),
			Key(ConsoleKey.DownArrow),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c"), buf, bufferConfig: MakeBufferConfig());
		int[]? result = picker.Run();
		Assert.Equal(new[] { 2 }, result);
	}

	#endregion

	#region Pagination

	private static string[] ManyItems(int count)
		=> Enumerable.Range(1, count).Select(i => $"item{i}").ToArray();

	[Fact]
	public void PageDown_advances_page()
	{
		var buf = new TestBuffer(
			Key(ConsoleKey.PageDown),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems(ManyItems(30)), buf);
		int[]? result = picker.Run();
		Assert.NotNull(result);
		Assert.True(result[0] > 0, "Should be on page 2");
	}

	[Fact]
	public void PageUp_goes_back()
	{
		var buf = new TestBuffer(
			Key(ConsoleKey.PageDown),
			Key(ConsoleKey.PageUp),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems(ManyItems(30)), buf);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 0 }, result);
	}

	[Fact]
	public void Home_jumps_to_first_on_page()
	{
		var buf = new TestBuffer(
			Key(ConsoleKey.DownArrow),
			Key(ConsoleKey.DownArrow),
			Key(ConsoleKey.Home),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c", "d", "e"), buf);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 0 }, result);
	}

	[Fact]
	public void End_jumps_to_last_on_page()
	{
		var buf = new TestBuffer(
			Key(ConsoleKey.End),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("a", "b", "c", "d", "e"), buf);
		int[]? result = picker.Run();
		Assert.NotNull(result);
		Assert.True(result[0] >= 2, "Should be at or near last item on page");
	}

	[Fact]
	public void Ctrl_Home_jumps_to_first_page()
	{
		var buf = new TestBuffer(
			Key(ConsoleKey.PageDown),
			Key(ConsoleKey.PageDown),
			Key(ConsoleKey.Home, '\0', ConsoleModifiers.Control),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems(ManyItems(30)), buf);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 0 }, result);
	}

	[Fact]
	public void Ctrl_End_jumps_to_last_page()
	{
		var buf = new TestBuffer(
			Key(ConsoleKey.End, '\0', ConsoleModifiers.Control),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems(ManyItems(30)), buf);
		int[]? result = picker.Run();
		Assert.NotNull(result);
		Assert.True(result[0] > 0, "Should be on last page");
	}

	#endregion

	#region Rendering Assertions

	[Fact]
	public void Cursor_hidden_on_start()
	{
		var buf = new TestBuffer(Key(ConsoleKey.Escape));
		var picker = CreatePicker(MakeItems("a", "b"), buf);
		picker.Run();
		Assert.Contains("\x1b[?25l", buf.AllWritten);
	}

	[Fact]
	public void Cursor_shown_on_exit()
	{
		var buf = new TestBuffer(Key(ConsoleKey.Escape));
		var picker = CreatePicker(MakeItems("a", "b"), buf);
		picker.Run();
		Assert.Contains("\x1b[?25h", buf.AllWritten);
	}

	[Fact]
	public void Item_text_appears_in_output()
	{
		var buf = new TestBuffer(Key(ConsoleKey.Escape));
		var picker = CreatePicker(MakeItems("Apple", "Banana"), buf);
		picker.Run();
		string output = buf.AllWritten;
		Assert.Contains("Apple", output);
		Assert.Contains("Banana", output);
	}

	[Fact]
	public void Selected_item_has_reverse_video()
	{
		var buf = new TestBuffer(Key(ConsoleKey.Escape));
		var picker = CreatePicker(MakeItems("a", "b"), buf);
		picker.Run();
		Assert.Contains("\x1b[7m", buf.AllWritten);
	}

	[Fact]
	public void Message_text_appears_in_output()
	{
		var buf = new TestBuffer(Key(ConsoleKey.Escape));
		var picker = CreatePicker(MakeItems("a"), buf, message: "Choose wisely");
		picker.Run();
		Assert.Contains("Choose", buf.AllWritten);
		Assert.Contains("wisely", buf.AllWritten);
	}

	[Fact]
	public void Title_text_appears_in_output()
	{
		var buf = new TestBuffer(Key(ConsoleKey.Escape));
		var picker = CreatePicker(MakeItems("a"), buf, title: "MyTitle");
		picker.Run();
		Assert.Contains("MyTitle", buf.AllWritten);
	}

	[Fact]
	public void Legend_appears_in_output()
	{
		var buf = new TestBuffer(Key(ConsoleKey.Escape));
		var picker = CreatePicker(MakeItems("a", "b"), buf);
		picker.Run();
		Assert.Contains("Enter Confirm", buf.AllWritten);
	}

	[Fact]
	public void Multiple_indicator_shown()
	{
		var buf = new TestBuffer(Key(ConsoleKey.Escape));
		var picker = CreatePicker(MakeItems("a", "b"), buf, multiple: true);
		picker.Run();
		Assert.Contains("○", buf.AllWritten); // ○
	}

	#endregion

	#region Edge Cases

	[Fact]
	public void Single_item_no_navigation()
	{
		var buf = new TestBuffer(
			Key(ConsoleKey.DownArrow),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(MakeItems("only"), buf);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 0 }, result);
	}

	[Fact]
	public void Ctrl_C_throws_PipelineStoppedException()
	{
		var buf = new TestBuffer(Key(ConsoleKey.C, '\x03', ConsoleModifiers.Control));
		var picker = CreatePicker(MakeItems("a", "b"), buf);
		Assert.Throws<PipelineStoppedException>(() => picker.Run());
	}

	#endregion
}
