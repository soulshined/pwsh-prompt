using System.Collections;
using System.Management.Automation;
using PwshPrompt.Consts;
using PwshPrompt.Tests.Helpers;
using PwshPrompt.Configs;
using PwshPrompt.Enums;
using PwshPrompt.Types;
using Xunit;

namespace PwshPrompt.Tests.Unit;

public class PaginationConfigTests
{
	private static TestPSHostRawUI DefaultRawUI() => new();

	public class Defaults
	{
		[Fact]
		public void Item_text_is_empty_circle()
		{
			PaginationConfig pc = PaginationConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal("○", pc.Item.Text);
		}

		[Fact]
		public void SelectedItem_text_is_filled_circle()
		{
			PaginationConfig pc = PaginationConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal("●", pc.SelectedItem.Text);
		}

		[Fact]
		public void PrevPage_text_is_empty()
		{
			PaginationConfig pc = PaginationConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal(string.Empty, pc.PrevPage.Text);
		}

		[Fact]
		public void NextPage_text_is_empty()
		{
			PaginationConfig pc = PaginationConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal(string.Empty, pc.NextPage.Text);
		}

		[Fact]
		public void TotalPage_text_is_empty()
		{
			PaginationConfig pc = PaginationConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal(string.Empty, pc.TotalPage.Text);
		}

		[Fact]
		public void TotalPage_style_is_dim()
		{
			PaginationConfig pc = PaginationConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Equal(TextStyle.Dim, pc.TotalPage.Style);
		}

		[Fact]
		public void Item_fg_is_null()
		{
			PaginationConfig pc = PaginationConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Null(pc.Item.ForegroundColor);
		}

		[Fact]
		public void Item_bg_is_null()
		{
			PaginationConfig pc = PaginationConfig.FromParameter(null, "p", DefaultRawUI());
			Assert.Null(pc.Item.BackgroundColor);
		}
	}

	public class Keys
	{
		[Theory]
		[InlineData("SelectedItem")]
		[InlineData("selected")]
		public void SelectedItem_long_and_short_keys(string key)
		{
			Hashtable ht = new Hashtable { [key] = "●" };
			PaginationConfig pc = PaginationConfig.FromParameter(ht, "p", DefaultRawUI());
			Assert.Equal("●", pc.SelectedItem.Text);
		}

		[Theory]
		[InlineData("PrevPage")]
		[InlineData("prev")]
		public void PrevPage_long_and_short_keys(string key)
		{
			Hashtable ht = new Hashtable { [key] = "<" };
			PaginationConfig pc = PaginationConfig.FromParameter(ht, "p", DefaultRawUI());
			Assert.Equal("<", pc.PrevPage.Text);
		}

		[Theory]
		[InlineData("NextPage")]
		[InlineData("next")]
		public void NextPage_long_and_short_keys(string key)
		{
			Hashtable ht = new Hashtable { [key] = ">" };
			PaginationConfig pc = PaginationConfig.FromParameter(ht, "p", DefaultRawUI());
			Assert.Equal(">", pc.NextPage.Text);
		}

		[Theory]
		[InlineData("TotalPage")]
		[InlineData("total")]
		public void TotalPage_long_and_short_keys(string key)
		{
			Hashtable ht = new Hashtable { [key] = "total" };
			PaginationConfig pc = PaginationConfig.FromParameter(ht, "p", DefaultRawUI());
			Assert.Equal("total", pc.TotalPage.Text);
		}
	}

	public class Fail
	{
		[Fact]
		public void Unknown_key_throws_PSArgumentException()
		{
			Hashtable ht = new Hashtable { ["UnknownKey"] = "x" };
			Assert.Throws<PSArgumentException>(() =>
				PaginationConfig.FromParameter(ht, "p", DefaultRawUI()));
		}
	}
}
