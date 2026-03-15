using System.Collections;
using System.Management.Automation;
using PwshPrompt.Consts;
using PwshPrompt.Tests.Helpers;
using PwshPrompt.Configs;
using PwshPrompt.Enums;
using PwshPrompt.Types;
using Xunit;

namespace PwshPrompt.Tests.Unit;

public class ItemConfigTests
{
	private static TestPSHostRawUI DefaultRawUI() => new();

	public class Defaults
	{
		[Fact]
		public void Item_text_is_empty()
		{
			ItemConfig ic = ItemConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal(string.Empty, ic.Item.Text);
		}

		[Fact]
		public void Item_fg_is_beige()
		{
			ItemConfig ic = ItemConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal(ANSI.COLOR.BEIGE, ic.Item.ForegroundColor);
		}

		[Fact]
		public void Item_bg_is_green()
		{
			ItemConfig ic = ItemConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal(ANSI.COLOR.GREEN, ic.Item.BackgroundColor);
		}

		[Fact]
		public void Item_style_is_none()
		{
			ItemConfig ic = ItemConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal(TextStyle.None, ic.Item.Style);
		}

		[Fact]
		public void SelectedItem_text_is_empty()
		{
			ItemConfig ic = ItemConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal(string.Empty, ic.SelectedItem.Text);
		}

		[Fact]
		public void SelectedItem_fg_is_beige()
		{
			ItemConfig ic = ItemConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal(ANSI.COLOR.BEIGE, ic.SelectedItem.ForegroundColor);
		}

		[Fact]
		public void SelectedItem_style_is_reverse()
		{
			ItemConfig ic = ItemConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal(TextStyle.Reverse, ic.SelectedItem.Style);
		}

		[Fact]
		public void MultipleIndicator_enabled_text_is_filled_circle()
		{
			ItemConfig ic = ItemConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal("◉", ic.multipleIndicator.Enabled.Text);
		}

		[Fact]
		public void MultipleIndicator_disabled_text_is_empty_circle()
		{
			ItemConfig ic = ItemConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal("○", ic.multipleIndicator.Disabled.Text);
		}

		[Fact]
		public void SelectedMultipleIndicator_enabled_text_is_filled_circle()
		{
			ItemConfig ic = ItemConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal("◉", ic.selectedMultipleIndicator.Enabled.Text);
		}

		[Fact]
		public void SelectedMultipleIndicator_disabled_text_is_empty_circle()
		{
			ItemConfig ic = ItemConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal("○", ic.selectedMultipleIndicator.Disabled.Text);
		}
	}

	public class Fail
	{
		[Fact]
		public void Unknown_key_throws_PSArgumentException()
		{
			Hashtable ht = new Hashtable { ["UnknownKey"] = "x" };
			Assert.Throws<PSArgumentException>(() =>
				ItemConfig.FromParameter(ht, "p", DefaultRawUI()));
		}
	}
}
