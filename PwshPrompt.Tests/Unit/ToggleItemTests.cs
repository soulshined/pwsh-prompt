using System.Collections;
using System.Management.Automation;
using PwshPrompt.Tests.Helpers;
using PwshPrompt.Types;
using Xunit;

namespace PwshPrompt.Tests.Unit;

public class ToggleItemTests
{
	private static TestPSHostRawUI DefaultRawUI() => new();

	public class Pass
	{
		[Fact]
		public void Null_returns_default_enabled_text()
		{
			ToggleItem ti = ToggleItem.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal("◉", ti.Enabled.Text);
		}

		[Fact]
		public void Null_returns_default_disabled_text()
		{
			ToggleItem ti = ToggleItem.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal("○", ti.Disabled.Text);
		}

		[Fact]
		public void Enabled_key_sets_enabled_label()
		{
			Hashtable ht = new Hashtable { ["Enabled"] = "YES" };
			ToggleItem ti = ToggleItem.FromParameter(ht, "p", DefaultRawUI());
			Assert.Equal("YES", ti.Enabled.Text);
		}

		[Fact]
		public void Disabled_key_sets_disabled_label()
		{
			Hashtable ht = new Hashtable { ["Disabled"] = "NO" };
			ToggleItem ti = ToggleItem.FromParameter(ht, "p", DefaultRawUI());
			Assert.Equal("NO", ti.Disabled.Text);
		}

		[Fact]
		public void On_short_key_accepted()
		{
			Hashtable ht = new Hashtable { ["on"] = "on-text" };
			ToggleItem ti = ToggleItem.FromParameter(ht, "p", DefaultRawUI());
			Assert.Equal("on-text", ti.Enabled.Text);
		}

		[Fact]
		public void Off_short_key_accepted()
		{
			Hashtable ht = new Hashtable { ["off"] = "off-text" };
			ToggleItem ti = ToggleItem.FromParameter(ht, "p", DefaultRawUI());
			Assert.Equal("off-text", ti.Disabled.Text);
		}
	}

	public class Fail
	{
		[Fact]
		public void Unknown_key_throws_PSArgumentException()
		{
			Hashtable ht = new Hashtable { ["UnknownKey"] = "x" };
			Assert.Throws<PSArgumentException>(() =>
				ToggleItem.FromParameter(ht, "p", DefaultRawUI()));
		}
	}
}
