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

public class PickerDescriptionTests
{
	private static readonly PSHostRawUserInterface RawUI = new TestPSHostRawUI();

	private static Item[] MakeItemsWithDescription(params (string value, string? description)[] entries)
		=> entries.Select(e => new Item
		{
			Value = e.value,
			DisplayText = e.value,
			Description = e.description
		}).ToArray();

	private static Item[] MakeItemsWithHelp(params (string value, string? help)[] entries)
		=> entries.Select(e => new Item
		{
			Value = e.value,
			DisplayText = e.value,
			HelpMessage = e.help
		}).ToArray();

	private static Item[] MakeItemsWithBoth(params (string value, string? help, string? description)[] entries)
		=> entries.Select(e => new Item
		{
			Value = e.value,
			DisplayText = e.value,
			HelpMessage = e.help,
			Description = e.description
		}).ToArray();

	private static BufferConfig MakeBufferConfig()
		=> BufferConfig.FromParameter(new System.Collections.Hashtable(), "AlternateBuffer", RawUI);

	private static Picker CreatePicker(
		Item[] items,
		TestBuffer buffer,
		bool multiple = false,
		CycleMode cycleMode = CycleMode.Next,
		int defaultIndex = 0,
		BufferConfig? bufferConfig = null,
		string message = "Pick")
	{
		return new Picker(
			items,
			new Label(RawUI, bufferConfig, message),
			null,
			cycleMode,
			bufferConfig,
			defaultIndex,
			multiple,
			RawUI,
			buffer);
	}

	#region Help Message Inline

	[Fact]
	public void Help_message_appears_inline_when_short()
	{
		var items = MakeItemsWithHelp(("Redis", "Fast cache"), ("PostgreSQL", null));
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		picker.Run();
		Assert.Contains("Fast cache", buf.AllWritten);
	}

	[Fact]
	public void Help_message_inline_is_dimmed()
	{
		var items = MakeItemsWithHelp(("Redis", "Fast cache"), ("PostgreSQL", null));
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		picker.Run();
		string output = buf.AllWritten;
		int dim_pos = output.IndexOf("\x1b[2m");
		int help_pos = output.IndexOf("Fast cache");
		Assert.True(dim_pos >= 0 && help_pos > dim_pos, "Help message should be preceded by DIM sequence");
	}

	[Fact]
	public void Help_message_inline_on_non_current_item()
	{
		var items = MakeItemsWithHelp(("Redis", null), ("PostgreSQL", "Relational DB"));
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		picker.Run();
		Assert.Contains("Relational DB", buf.AllWritten);
	}

	[Fact]
	public void Help_message_inline_in_alternate_screen()
	{
		var items = MakeItemsWithHelp(("Redis", "Fast cache"), ("PostgreSQL", null));
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf, bufferConfig: MakeBufferConfig());
		picker.Run();
		Assert.Contains("Fast cache", buf.AllWritten);
	}

	#endregion

	#region Help Message Status Bar Fallback

	[Fact]
	public void Help_message_in_status_bar_when_value_too_long()
	{
		string long_value = new string('X', 60);
		string help = "This is the help text";
		var items = MakeItemsWithHelp((long_value, help), ("Short", null));
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		picker.Run();
		Assert.Contains(help, buf.AllWritten);
	}

	[Fact]
	public void Status_bar_help_message_is_italic()
	{
		string long_value = new string('X', 60);
		string help = "Italic help text";
		var items = MakeItemsWithHelp((long_value, help), ("Short", null));
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		picker.Run();
		string italic_ansi = TextStyle.Italic.ToAnsi();
		string output = buf.AllWritten;
		int italic_pos = output.IndexOf(italic_ansi);
		int help_pos = output.IndexOf("Italic help text");
		Assert.True(italic_pos >= 0, "Expected italic ANSI sequence in output");
		Assert.True(help_pos > italic_pos, "Help text should follow italic sequence");
	}

	[Fact]
	public void Status_bar_help_message_italic_in_alternate()
	{
		string long_value = new string('X', 60);
		string help = "Alternate italic help";
		var items = MakeItemsWithHelp((long_value, help), ("Short", null));
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf, bufferConfig: MakeBufferConfig());
		picker.Run();
		string italic_ansi = TextStyle.Italic.ToAnsi();
		Assert.Contains(italic_ansi, buf.AllWritten);
		Assert.Contains(help, buf.AllWritten);
	}

	[Fact]
	public void No_help_message_means_no_italic_in_output()
	{
		var items = MakeItemsWithHelp(("Redis", null), ("PostgreSQL", null));
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		picker.Run();
		string italic_ansi = TextStyle.Italic.ToAnsi();
		Assert.DoesNotContain(italic_ansi, buf.AllWritten);
	}

	#endregion

	#region Description Preview Line (removed — inline description concept was dropped)
	#endregion

	#region F1 Details View

	[Fact]
	public void F1_opens_details_view_and_Esc_returns()
	{
		var items = MakeItemsWithDescription(
			("Redis", "In-memory key-value store.\nSupports pub/sub."),
			("PostgreSQL", null));
		var buf = new TestBuffer(
			Key(ConsoleKey.F1),
			Key(ConsoleKey.Escape),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 0 }, result);
	}

	[Fact]
	public void F1_no_op_when_no_description()
	{
		var items = MakeItemsWithDescription(
			("Redis", null),
			("PostgreSQL", null));
		var buf = new TestBuffer(
			Key(ConsoleKey.F1),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 0 }, result);
	}

	[Fact]
	public void Details_view_shows_description_text()
	{
		var items = MakeItemsWithDescription(
			("Redis", "In-memory key-value store.\nSupports pub/sub and streams."),
			("PostgreSQL", null));
		var buf = new TestBuffer(
			Key(ConsoleKey.F1),
			Key(ConsoleKey.Escape),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		picker.Run();
		string output = buf.AllWritten;
		Assert.Contains("In-memory key-value store.", output);
		Assert.Contains("Supports pub/sub and streams.", output);
	}

	[Fact]
	public void Details_view_shows_item_title()
	{
		var items = MakeItemsWithDescription(
			("Redis", "Some description"),
			("PostgreSQL", null));
		var buf = new TestBuffer(
			Key(ConsoleKey.F1),
			Key(ConsoleKey.Escape),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		picker.Run();
		Assert.Contains("Redis", buf.AllWritten);
	}

	[Fact]
	public void Details_view_shows_description_tagline()
	{
		var items = MakeItemsWithDescription(
			("Redis", "Some description"),
			("PostgreSQL", null));
		var buf = new TestBuffer(
			Key(ConsoleKey.F1),
			Key(ConsoleKey.Escape),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		picker.Run();
		Assert.Contains("Description", buf.AllWritten);
	}

	[Fact]
	public void Details_view_shows_Esc_legend()
	{
		var items = MakeItemsWithDescription(
			("Redis", "Some description"),
			("PostgreSQL", null));
		var buf = new TestBuffer(
			Key(ConsoleKey.F1),
			Key(ConsoleKey.Escape),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		picker.Run();
		Assert.Contains("Esc Go back", buf.AllWritten);
	}

	[Fact]
	public void Details_view_scrolls_down_and_up()
	{
		string long_desc = string.Join("\n", Enumerable.Range(1, 50).Select(i => $"Line {i} of description"));
		var items = MakeItemsWithDescription(
			("Redis", long_desc),
			("PostgreSQL", null));
		var buf = new TestBuffer(
			Key(ConsoleKey.F1),
			Key(ConsoleKey.DownArrow),
			Key(ConsoleKey.DownArrow),
			Key(ConsoleKey.UpArrow),
			Key(ConsoleKey.Escape),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 0 }, result);
	}

	[Fact]
	public void Details_view_Ctrl_C_throws()
	{
		var items = MakeItemsWithDescription(
			("Redis", "Some description"),
			("PostgreSQL", null));
		var buf = new TestBuffer(
			Key(ConsoleKey.F1),
			Key(ConsoleKey.C, '\x03', ConsoleModifiers.Control));
		var picker = CreatePicker(items, buf);
		Assert.Throws<PipelineStoppedException>(() => picker.Run());
	}

	[Fact]
	public void Details_view_in_alternate_screen()
	{
		var items = MakeItemsWithDescription(
			("Redis", "In-memory key-value store.\nSupports pub/sub."),
			("PostgreSQL", null));
		var buf = new TestBuffer(
			Key(ConsoleKey.F1),
			Key(ConsoleKey.Escape),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf, bufferConfig: MakeBufferConfig());
		int[]? result = picker.Run();
		Assert.Equal(new[] { 0 }, result);
		Assert.Contains("In-memory key-value store.", buf.AllWritten);
		Assert.Contains("Description", buf.AllWritten);
	}

	[Fact]
	public void Details_view_scroll_indicators_appear_when_long()
	{
		string long_desc = string.Join("\n", Enumerable.Range(1, 50).Select(i => $"Line {i} of description"));
		var items = MakeItemsWithDescription(
			("Redis", long_desc),
			("PostgreSQL", null));
		var buf = new TestBuffer(
			Key(ConsoleKey.F1),
			Key(ConsoleKey.DownArrow),
			Key(ConsoleKey.Escape),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		picker.Run();
		string output = buf.AllWritten;
		Assert.Contains("↑ more", output);
		Assert.Contains("↓ more", output);
	}

	#endregion

	#region Legend

	[Fact]
	public void Legend_shows_F1_Details_when_description_exists()
	{
		var items = MakeItemsWithDescription(
			("Redis", "Some description"),
			("PostgreSQL", null));
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		picker.Run();
		Assert.Contains("F1 Details", buf.AllWritten);
	}

	[Fact]
	public void Legend_hides_F1_when_no_description()
	{
		var items = MakeItemsWithDescription(
			("Redis", null),
			("PostgreSQL", null));
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		picker.Run();
		Assert.DoesNotContain("F1 Details", buf.AllWritten);
	}

	[Fact]
	public void Legend_F1_appears_in_alternate_screen()
	{
		var items = MakeItemsWithDescription(
			("Redis", "Some description"),
			("PostgreSQL", null));
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf, bufferConfig: MakeBufferConfig());
		picker.Run();
		Assert.Contains("F1 Details", buf.AllWritten);
	}

	#endregion

	#region Navigation with Descriptions

	[Fact]
	public void Navigate_from_item_with_description_to_without()
	{
		var items = MakeItemsWithDescription(
			("Redis", "Has description"),
			("PostgreSQL", null),
			("MongoDB", null));
		var buf = new TestBuffer(
			Key(ConsoleKey.DownArrow),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 1 }, result);
	}

	[Fact]
	public void Navigate_from_item_without_description_to_with()
	{
		var items = MakeItemsWithDescription(
			("Redis", null),
			("PostgreSQL", "Has description"),
			("MongoDB", null));
		var buf = new TestBuffer(
			Key(ConsoleKey.DownArrow),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 1 }, result);
	}

	[Fact]
	public void Navigate_between_described_items_in_alternate()
	{
		var items = MakeItemsWithDescription(
			("Redis", "Redis desc"),
			("PostgreSQL", "PG desc"),
			("MongoDB", null));
		var buf = new TestBuffer(
			Key(ConsoleKey.DownArrow),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf, bufferConfig: MakeBufferConfig());
		int[]? result = picker.Run();
		Assert.Equal(new[] { 1 }, result);
	}

	#endregion

	#region Combined Help + Description

	[Fact]
	public void Item_with_both_help_and_description()
	{
		var items = MakeItemsWithBoth(
			("Redis", "Fast cache", "In-memory key-value store."),
			("PostgreSQL", null, null));
		var buf = new TestBuffer(Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		picker.Run();
		string output = buf.AllWritten;
		Assert.Contains("Fast cache", output);
		Assert.Contains("F1 Details", output);
	}

	[Fact]
	public void F1_after_navigating_shows_correct_description()
	{
		var items = MakeItemsWithDescription(
			("Redis", "Redis description"),
			("PostgreSQL", "PG description"),
			("MongoDB", null));
		var buf = new TestBuffer(
			Key(ConsoleKey.DownArrow),
			Key(ConsoleKey.F1),
			Key(ConsoleKey.Escape),
			Key(ConsoleKey.Enter, '\r'));
		var picker = CreatePicker(items, buf);
		int[]? result = picker.Run();
		Assert.Equal(new[] { 1 }, result);
		Assert.Contains("PG description", buf.AllWritten);
	}

	#endregion

	#region Help Overlay

	[Fact]
	public void Help_overlay_includes_F1_entry()
	{
		var items = MakeItemsWithDescription(
			("Redis", "Some description"),
			("PostgreSQL", null));
		var buf = new TestBuffer(
			Key(ConsoleKey.Oem2, '?'),
			Key(ConsoleKey.Escape),
			Key(ConsoleKey.Escape));
		var picker = CreatePicker(items, buf, bufferConfig: MakeBufferConfig());
		picker.Run();
		Assert.Contains("View full description", buf.AllWritten);
	}

	#endregion
}
