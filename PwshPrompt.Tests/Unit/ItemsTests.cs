using System.Collections;
using System.Management.Automation;
using PwshPrompt.Consts;
using PwshPrompt.Enums;
using PwshPrompt.Types;
using Xunit;

namespace PwshPrompt.Tests.Unit;

public class ItemsTests
{
	public class Pass
	{
		[Fact]
		public void String_array_produces_items_with_value()
		{
			Items choices = Items.FromParameter(new object[] { "a", "b", "c" }, "p");
			Assert.Equal(3, choices.Count);
			Assert.Equal("a", choices[0].Value);
			Assert.Equal("b", choices[1].Value);
			Assert.Equal("c", choices[2].Value);
		}

		[Fact]
		public void Int_array_produces_items()
		{
			Items choices = Items.FromParameter(new object[] { 1, 2, 3 }, "p");
			Assert.Equal(1, choices[0].Value);
			Assert.Equal(2, choices[1].Value);
		}

		[Fact]
		public void PSObject_wrapped_strings_are_unwrapped()
		{
			Items choices = Items.FromParameter(
				new object[] { new PSObject("x"), new PSObject("y") }, "p");
			Assert.Equal("x", choices[0].Value);
			Assert.Equal("y", choices[1].Value);
		}

		[Fact]
		public void Hashtable_with_Value_key_produces_item()
		{
			Hashtable ht = new Hashtable { ["Value"] = "opt1" };
			Items choices = Items.FromParameter(new object[] { ht }, "p");
			Assert.Equal("opt1", choices[0].Value);
		}

		[Fact]
		public void Hashtable_with_HotKey_char_parsed()
		{
			Hashtable ht = new Hashtable { ["Value"] = "x", ["HotKey"] = 'a' };
			Items choices = Items.FromParameter(new object[] { ht }, "p");
			Assert.Equal('a', choices[0].HotKey);
		}

		[Fact]
		public void Hashtable_with_HotKey_single_char_string_parsed()
		{
			Hashtable ht = new Hashtable { ["Value"] = "x", ["HotKey"] = "a" };
			Items choices = Items.FromParameter(new object[] { ht }, "p");
			Assert.Equal('a', choices[0].HotKey);
		}

		[Fact]
		public void Hashtable_with_ForegroundColor_array_parsed()
		{
			Hashtable ht = new Hashtable
			{
				["Value"] = "x",
				["ForegroundColor"] = new object[] { "Red", "Red" }
			};
			Items choices = Items.FromParameter(new object[] { ht }, "p");
			Assert.Equal(new AnsiColor("9", "255;0;0"), choices[0].ForegroundColor);
		}

		[Fact]
		public void Hashtable_fg_short_key_accepted()
		{
			Hashtable ht = new Hashtable
			{
				["Value"] = "x",
				["fg"] = new object[] { "230", "189;179;149" }
			};
			Items choices = Items.FromParameter(new object[] { ht }, "p");
			Assert.NotNull(choices[0].ForegroundColor);
		}

		[Fact]
		public void Hashtable_with_Description_HelpMessage_Style_parsed()
		{
			Hashtable ht = new Hashtable
			{
				["Value"] = "x",
				["Description"] = "desc",
				["HelpMessage"] = "help",
				["Style"] = "Bold"
			};
			Items choices = Items.FromParameter(new object[] { ht }, "p");
			Item item = choices[0];
			Assert.Equal("desc", item.Description);
			Assert.Equal("help", item.HelpMessage);
			Assert.Equal(TextStyle.Bold, item.Style);
		}
	}

	public class DisplayText
	{
		[Fact]
		public void String_items_resolve_DisplayText()
		{
			Items choices = Items.FromParameter(new object[] { "hello", "world" }, "p");
			Assert.Equal("hello", choices[0].DisplayText);
			Assert.Equal("world", choices[1].DisplayText);
		}

		[Fact]
		public void Int_items_resolve_DisplayText()
		{
			Items choices = Items.FromParameter(new object[] { 42, 99 }, "p");
			Assert.Equal("42", choices[0].DisplayText);
			Assert.Equal("99", choices[1].DisplayText);
		}

		[Fact]
		public void Hashtable_item_resolves_DisplayText()
		{
			Hashtable ht = new() { ["Value"] = "plain" };
			Items choices = Items.FromParameter(new object[] { ht }, "p");
			Assert.Equal("plain", choices[0].DisplayText);
		}

		[Fact]
		public void PSObject_wrapped_items_resolve_DisplayText()
		{
			Items choices = Items.FromParameter(
				new object[] { new PSObject("alpha"), new PSObject("beta") }, "p");
			Assert.Equal("alpha", choices[0].DisplayText);
			Assert.Equal("beta", choices[1].DisplayText);
		}
	}

	public class Fail
	{
		[Fact]
		public void Mixed_string_and_int_types_throws()
		{
			Assert.Throws<PSArgumentException>(() =>
				Items.FromParameter(new object[] { "a", 1 }, "p"));
		}

		[Fact]
		public void Hashtable_without_Value_throws()
		{
			Hashtable ht = new Hashtable { ["Description"] = "no value" };
			Assert.Throws<PSArgumentException>(() =>
				Items.FromParameter(new object[] { ht }, "p"));
		}

		[Fact]
		public void Hashtable_with_unknown_key_throws()
		{
			Hashtable ht = new Hashtable { ["Value"] = "x", ["Unknown"] = "y" };
			Assert.Throws<PSArgumentException>(() =>
				Items.FromParameter(new object[] { ht }, "p"));
		}
	}
}
