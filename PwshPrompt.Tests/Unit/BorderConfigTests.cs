using System.Collections;
using System.Management.Automation;
using PwshPrompt.Consts;
using PwshPrompt.Configs;
using Xunit;

namespace PwshPrompt.Tests.Unit;

public class BorderConfigTests
{
	public class Pass
	{
		[Fact]
		public void Null_returns_all_defaults()
		{
			BorderConfig bc = BorderConfig.FromParameter(null, "p");
			Assert.Equal("─", bc.HorizontalSide);
			Assert.Equal(" ", bc.VerticalSide);
			Assert.Equal("┌", bc.TopLeft);
			Assert.Equal("┐", bc.TopRight);
			Assert.Equal("└", bc.BottomLeft);
			Assert.Equal("┘", bc.BottomRight);
			Assert.Equal(ANSI.COLOR.BEIGE, bc.Color);
		}

		[Fact]
		public void Partial_hashtable_overrides_only_specified_keys()
		{
			Hashtable ht = new Hashtable { ["Color"] = new object[] { "Red", "Red" } };
			BorderConfig bc = BorderConfig.FromParameter(ht, "p");
			Assert.Equal("─", bc.HorizontalSide);
			Assert.Equal("┌", bc.TopLeft);
			Assert.Equal(new AnsiColor("9", "255;0;0"), bc.Color);
		}

		[Theory]
		[InlineData("HorizontalSide")]
		[InlineData("hs")]
		public void HorizontalSide_long_and_short_keys(string key)
		{
			Hashtable ht = new Hashtable { [key] = "=" };
			BorderConfig bc = BorderConfig.FromParameter(ht, "p");
			Assert.Equal("=", bc.HorizontalSide);
		}

		[Theory]
		[InlineData("VerticalSide")]
		[InlineData("vs")]
		public void VerticalSide_long_and_short_keys(string key)
		{
			Hashtable ht = new Hashtable { [key] = "|" };
			BorderConfig bc = BorderConfig.FromParameter(ht, "p");
			Assert.Equal("|", bc.VerticalSide);
		}

		[Theory]
		[InlineData("TopLeft")]
		[InlineData("tl")]
		public void TopLeft_long_and_short_keys(string key)
		{
			Hashtable ht = new Hashtable { [key] = "+" };
			BorderConfig bc = BorderConfig.FromParameter(ht, "p");
			Assert.Equal("+", bc.TopLeft);
		}

		[Theory]
		[InlineData("TopRight")]
		[InlineData("tr")]
		public void TopRight_long_and_short_keys(string key)
		{
			Hashtable ht = new Hashtable { [key] = "+" };
			BorderConfig bc = BorderConfig.FromParameter(ht, "p");
			Assert.Equal("+", bc.TopRight);
		}

		[Theory]
		[InlineData("BottomLeft")]
		[InlineData("bl")]
		public void BottomLeft_long_and_short_keys(string key)
		{
			Hashtable ht = new Hashtable { [key] = "+" };
			BorderConfig bc = BorderConfig.FromParameter(ht, "p");
			Assert.Equal("+", bc.BottomLeft);
		}

		[Theory]
		[InlineData("BottomRight")]
		[InlineData("br")]
		public void BottomRight_long_and_short_keys(string key)
		{
			Hashtable ht = new Hashtable { [key] = "+" };
			BorderConfig bc = BorderConfig.FromParameter(ht, "p");
			Assert.Equal("+", bc.BottomRight);
		}
	}

	public class Fail
	{
		[Fact]
		public void Unknown_key_throws_PSArgumentException()
		{
			Hashtable ht = new Hashtable { ["UnknownKey"] = "x" };
			Assert.Throws<PSArgumentException>(() => BorderConfig.FromParameter(ht, "p"));
		}
	}
}
