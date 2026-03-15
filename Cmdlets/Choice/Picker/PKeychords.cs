using System.Collections.Frozen;
using PwshPrompt.Types;
using System.Management.Automation;

namespace PwshPrompt.IO.Choice;

internal sealed partial class Picker
{
	private static readonly FrozenDictionary<Keychord, Func<Picker, LoopSignal<int[]>>> KeyMap =
		new Dictionary<Keychord, Func<Picker, LoopSignal<int[]>>>
		{
			[new(ConsoleKey.C, ConsoleModifiers.Control)] = static (self) =>
			{
				self._buffer.WriteLine();
				throw new PipelineStoppedException();
			},
			[new(ConsoleKey.UpArrow)] = static (self) =>
			{
				int prev = self.CurrentIndex, prev_page = self.CurrentPage;
				self.MoveCursor(-1);
				if (self.CurrentIndex == prev) return LoopSignal<int[]>.Continue;
				if (self.BufferConfig.HasValue && self.CurrentPage == prev_page)
					self.RedrawNavigation(prev);
				else
					self.Render();
				return LoopSignal<int[]>.Continue;
			},
			[new(ConsoleKey.DownArrow)] = static (self) =>
			{
				int prev = self.CurrentIndex, prev_page = self.CurrentPage;
				self.MoveCursor(1);
				if (self.CurrentIndex == prev) return LoopSignal<int[]>.Continue;
				if (self.BufferConfig.HasValue && self.CurrentPage == prev_page)
					self.RedrawNavigation(prev);
				else
					self.Render();
				return LoopSignal<int[]>.Continue;
			},
			[new(ConsoleKey.Enter)] = static (self) =>
			{
				if (self.SupportsMultiple) {
					int[] result = self.SelectedIndices.ToArray();
					Array.Sort(result);
					return LoopSignal<int[]>.Return(result);
				}
				return LoopSignal<int[]>.Return(new[] { self.CurrentIndex });
			},
			[new(ConsoleKey.Escape)] = static (_) => LoopSignal<int[]>.Return((int[]?)null),
			[new(ConsoleKey.Spacebar)] = static (self) =>
			{
				if (!self.SupportsMultiple)
					return LoopSignal<int[]>.Continue;
				if (!self.SelectedIndices.Remove(self.CurrentIndex))
					self.SelectedIndices.Add(self.CurrentIndex);
				if (self.BufferConfig.HasValue)
					self.RedrawToggle();
				else
					self.Render();
				return LoopSignal<int[]>.Continue;
			},
			[new(ConsoleKey.Home)] = static (self) =>
			{
				int prev = self.CurrentIndex;
				self.CurrentIndex = self.CurrentPage * self.PageSize;
				if (prev == self.CurrentIndex) return LoopSignal<int[]>.Continue;
				if (self.BufferConfig.HasValue)
					self.RedrawNavigation(prev);
				else
					self.Render();
				return LoopSignal<int[]>.Continue;
			},
			[new(ConsoleKey.End)] = static (self) =>
			{
				int prev = self.CurrentIndex;
				self.CurrentIndex = Math.Min(
					(self.CurrentPage + 1) * self.PageSize - 1,
					self.Items.Length - 1);
				if (prev == self.CurrentIndex) return LoopSignal<int[]>.Continue;
				if (self.BufferConfig.HasValue)
					self.RedrawNavigation(prev);
				else
					self.Render();
				return LoopSignal<int[]>.Continue;
			},
			[new(ConsoleKey.PageDown)] = static (self) =>
			{
				if (self.TotalPages <= 1 || self.CurrentPage >= self.TotalPages - 1)
					return LoopSignal<int[]>.Continue;
				self.CurrentPage++;
				self.CurrentIndex = self.CurrentPage * self.PageSize;
				self.Render();
				return LoopSignal<int[]>.Continue;
			},
			[new(ConsoleKey.PageUp)] = static (self) =>
			{
				if (self.TotalPages <= 1 || self.CurrentPage <= 0)
					return LoopSignal<int[]>.Continue;
				self.CurrentPage--;
				self.CurrentIndex = self.CurrentPage * self.PageSize;
				self.Render();
				return LoopSignal<int[]>.Continue;
			},
			[new(ConsoleKey.Home, ConsoleModifiers.Control)] = static (self) =>
			{
				if (self.TotalPages <= 1 || self.CurrentPage <= 0)
					return LoopSignal<int[]>.Continue;
				self.CurrentPage = 0;
				self.CurrentIndex = 0;
				self.Render();
				return LoopSignal<int[]>.Continue;
			},
			[new(ConsoleKey.End, ConsoleModifiers.Control)] = static (self) =>
			{
				if (self.TotalPages <= 1 || self.CurrentPage >= self.TotalPages - 1)
					return LoopSignal<int[]>.Continue;
				self.CurrentPage = self.TotalPages - 1;
				self.CurrentIndex = self.CurrentPage * self.PageSize;
				self.Render();
				return LoopSignal<int[]>.Continue;
			},
			[new(ConsoleKey.F1)] = static (self) =>
			{
				if (self.Items[self.CurrentIndex].Description is not null)
					self.RenderDetailsView();
				return LoopSignal<int[]>.Continue;
			},
			[new(ConsoleKey.A, ConsoleModifiers.Control)] = static (self) =>
			{
				if (self.SupportsMultiple) {
					int page_start = self.CurrentPage * self.PageSize;
					int page_end = Math.Min(page_start + self.PageSize, self.Items.Length);

					bool all_selected = true;
					for (int i = page_start; i < page_end; i++)
					{
						if (!self.SelectedIndices.Contains(i))
						{
							all_selected = false;
							break;
						}
					}

					if (all_selected)
						for (int i = page_start; i < page_end; i++)
							self.SelectedIndices.Remove(i);
					else
						for (int i = page_start; i < page_end; i++)
							if (!self.SelectedIndices.Contains(i))
								self.SelectedIndices.Add(i);

					if (self.BufferConfig.HasValue)
						self.RedrawPageItemsAndTag();
					else
						self.Render();
				}

				return LoopSignal<int[]>.Continue;
			},
		}.ToFrozenDictionary();
}
