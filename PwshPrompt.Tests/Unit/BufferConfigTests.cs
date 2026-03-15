using System.Collections;
using System.Management.Automation;
using PwshPrompt.Consts;
using PwshPrompt.Tests.Helpers;
using PwshPrompt.Configs;
using Xunit;

namespace PwshPrompt.Tests.Unit;

public class BufferConfigTests
{
	private static TestPSHostRawUI DefaultRawUI() => new();

	public class Pass
	{
		[Fact]
		public void Null_returns_default_colors()
		{
			BufferConfig bc = BufferConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal(ANSI.COLOR.BEIGE, bc.ForegroundColor);
			Assert.Equal(ANSI.COLOR.GREEN, bc.BackgroundColor);
		}

		[Fact]
		public void Null_returns_default_border()
		{
			BufferConfig bc = BufferConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal("─", bc.Border.HorizontalSide);
			Assert.Equal(ANSI.COLOR.BEIGE, bc.Border.Color);
		}

		[Fact]
		public void ForegroundColor_key_overrides_default()
		{
			Hashtable ht = new Hashtable
			{
				["ForegroundColor"] = new object[] { "Red", "Red" }
			};
			BufferConfig bc = BufferConfig.FromParameter(ht, "p", DefaultRawUI());
			Assert.Equal(new AnsiColor("9", "255;0;0"), bc.ForegroundColor);
		}

		[Fact]
		public void Fg_short_key_accepted()
		{
			Hashtable ht = new Hashtable
			{
				["fg"] = new object[] { "Red", "Red" }
			};
			BufferConfig bc = BufferConfig.FromParameter(ht, "p", DefaultRawUI());
			Assert.Equal(new AnsiColor("9", "255;0;0"), bc.ForegroundColor);
		}

		[Fact]
		public void BackgroundColor_key_overrides_default()
		{
			Hashtable ht = new Hashtable
			{
				["BackgroundColor"] = new object[] { "0", "0;0;0" }
			};
			BufferConfig bc = BufferConfig.FromParameter(ht, "p", DefaultRawUI());
			Assert.Equal(new AnsiColor("0", "0;0;0"), bc.BackgroundColor);
		}

		[Fact]
		public void Nested_Border_key_delegated_correctly()
		{
			Hashtable ht = new Hashtable
			{
				["Border"] = new Hashtable { ["HorizontalSide"] = "=" }
			};
			BufferConfig bc = BufferConfig.FromParameter(ht, "p", DefaultRawUI());
			Assert.Equal("=", bc.Border.HorizontalSide);
		}

		[Fact]
		public void Nested_Item_key_delegated_correctly()
		{
			Hashtable ht = new Hashtable
			{
				["Item"] = new Hashtable { ["Item"] = "hello" }
			};
			// Should not throw; Item is parsed via ItemConfig.FromParameter
			BufferConfig bc = BufferConfig.FromParameter(ht, "p", DefaultRawUI());
			Assert.Equal(ANSI.COLOR.BEIGE, bc.ForegroundColor); // just verify it parsed
		}
	}

	public class Fail
	{
		[Fact]
		public void Unknown_key_throws_PSArgumentException()
		{
			Hashtable ht = new Hashtable { ["UnknownKey"] = "x" };
			Assert.Throws<PSArgumentException>(() =>
				BufferConfig.FromParameter(ht, "p", DefaultRawUI()));
		}
	}
}
