#region Copyright Notice
// This file is part of CrystalBoy.
// Copyright (C) 2008 Fabien Barbier
// 
// CrystalBoy is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// CrystalBoy is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using CrystalBoy.Core;

namespace CrystalBoy.Disassembly.Windows.Forms
{
	[ToolboxBitmap(typeof(DisassemblyView))]
	public partial class DisassemblyView : UserControl
	{
		#region Instruction Structure

		struct Instruction
		{
			public Instruction(ushort offset, ushort length, string text, bool isBreakpoint)
			{
				Offset = offset;
				Length = length;
				Text = text;
				IsBreakpoint = isBreakpoint;
			}

			public ushort Offset;
			public ushort Length;
			public string Text;
			public bool IsBreakpoint;
		}

		#endregion

		static Font defaultFont = new Font("Courier New", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
		static Bitmap defaultSymbolsBitmap = new Bitmap(typeof(DisassemblyView).Assembly.GetManifestResourceStream(typeof(DisassemblyView), "symbols.png"));
		static Rectangle breakpointSymbolRectangle = new Rectangle(0, 0, 16, 16),
			currentInstructionSymbolRectangle = new Rectangle(16, 0, 16, 16);

		IMemory memory;
		IDebuggable debuggableMemory;
		List<Instruction> instructionCache;
		ushort maximumOffset, selectedOffset, currentInstructionOffset;
		bool showCurrentInstruction;
		int lineHeight;
		Color selectionForeColor, selectionBackColor,
			currentInstructionForeColor, currentInstructionBackColor,
			breakpointForeColor, breakpointBackColor;
		Brush selectionBrush, breakpointBrush, currentInstructionBrush;
		Bitmap symbolsBitmap;

		#region Constructor

		public DisassemblyView()
		{
			symbolsBitmap = new Bitmap(defaultSymbolsBitmap.Width, defaultSymbolsBitmap.Height, PixelFormat.Format32bppArgb);
			maximumOffset = 0xFFFF;
			instructionCache = new List<Instruction>();
			SetStyle(ControlStyles.AllPaintingInWmPaint |
				ControlStyles.ResizeRedraw |
				ControlStyles.UserPaint |
				ControlStyles.Opaque |
				ControlStyles.Selectable |
				ControlStyles.StandardClick |
				ControlStyles.UserMouse, true);
			SetStyle(ControlStyles.ContainerControl, false);
			DoubleBuffered = true;
			ResetFont();
			ResetBackColor();
			ResetSelectionForeColor();
			ResetSelectionBackColor();
			ResetCurrentInstructionForeColor();
			ResetCurrentInstructionBackColor();
			ResetBreakpointForeColor();
			ResetBreakpointBackColor();
			base.AutoScroll = false;
			HScroll = false;
			VScroll = true;
			UpdateScrollBars();
		}

		#endregion

		#region Properties

		#region Appearance

		#region Font

		public override Font Font
		{
			get
			{
				return base.Font;
			}
			set
			{
				if (value != base.Font)
				{
					if (value != null)
					{
						base.Font = value;
						MeasureLine();
					}
					else if (base.Font != defaultFont)
					{
						base.Font = defaultFont;
						MeasureLine();
					}
				}
			}
		}

		public bool ShouldSerializeFont()
		{
			return (base.Font != defaultFont);
		}

		public override void ResetFont()
		{
			base.Font = defaultFont;
			MeasureLine();
		}

		#endregion

		#region Colors

		#region Selection

		[Category("Appearance")]
		[DefaultValue(typeof(Color), "HighlightText")]
		public Color SelectionForeColor
		{
			get
			{
				return selectionForeColor;
			}
			set
			{
				if (value != selectionForeColor)
				{
					selectionForeColor = value;
					Invalidate();
				}
			}
		}

		public virtual void ResetSelectionForeColor()
		{
			if (selectionForeColor != SystemColors.HighlightText)
			{
				selectionForeColor = SystemColors.HighlightText;
				Invalidate();
			}
		}

		[Category("Appearance")]
		[DefaultValue(typeof(Color), "Highlight")]
		public Color SelectionBackColor
		{
			get
			{
				return selectionBackColor;
			}
			set
			{
				if (value != selectionBackColor)
				{
					selectionBackColor = value;
					Invalidate();
				}
			}
		}

		public virtual void ResetSelectionBackColor()
		{
			if (selectionBackColor != SystemColors.Highlight)
			{
				selectionBackColor = SystemColors.Highlight;
				selectionBrush = new SolidBrush(selectionBackColor);
				Invalidate();
			}
		}

		#endregion

		#region Current Instruction

		[Category("Appearance")]
		[DefaultValue(typeof(Color), "Black")]
		public Color CurrentInstructionForeColor
		{
			get
			{
				return currentInstructionForeColor;
			}
			set
			{
				if (value != currentInstructionForeColor)
				{
					currentInstructionForeColor = value;
					Invalidate();
				}
			}
		}

		public virtual void ResetCurrentInstructionForeColor()
		{
			if (currentInstructionForeColor != Color.Black)
			{
				currentInstructionForeColor = Color.Black;
				Invalidate();
			}
		}

		[Category("Appearance")]
		[DefaultValue(typeof(Color), "Lime")]
		public Color CurrentInstructionBackColor
		{
			get
			{
				return currentInstructionBackColor;
			}
			set
			{
				if (value != currentInstructionBackColor)
				{
					currentInstructionBackColor = value;
					UpdateCurrentInstructionSymbol();
					Invalidate();
				}
			}
		}

		public virtual void ResetCurrentInstructionBackColor()
		{
			if (currentInstructionBackColor != Color.Lime)
			{
				currentInstructionBackColor = Color.Lime;
				currentInstructionBrush = new SolidBrush(currentInstructionBackColor);
				UpdateCurrentInstructionSymbol();
			}
		}

		private void UpdateCurrentInstructionSymbol()
		{
			UpdateSymbol(currentInstructionSymbolRectangle, currentInstructionBackColor);
		}

		#endregion

		#region Breakpoint

		[Category("Appearance")]
		[DefaultValue(typeof(Color), "White")]
		public Color BreakpointForeColor
		{
			get
			{
				return breakpointForeColor;
			}
			set
			{
				if (value != breakpointForeColor)
				{
					breakpointForeColor = value;
					Invalidate();
				}
			}
		}

		public virtual void ResetBreakpointForeColor()
		{
			if (breakpointForeColor != Color.White)
			{
				breakpointForeColor = Color.White;
				Invalidate();
			}
		}

		[Category("Appearance")]
		[DefaultValue(typeof(Color), "DarkRed")]
		public Color BreakpointBackColor
		{
			get
			{
				return breakpointBackColor;
			}
			set
			{
				if (value != breakpointBackColor)
				{
					breakpointBackColor = value;
					UpdateBreakpointSymbol();
					Invalidate();
				}
			}
		}

		public virtual void ResetBreakpointBackColor()
		{
			if (breakpointBackColor != Color.DarkRed)
			{
				breakpointBackColor = Color.DarkRed;
				breakpointBrush = new SolidBrush(breakpointBackColor);
				UpdateBreakpointSymbol();
			}
		}

		private void UpdateBreakpointSymbol()
		{
			UpdateSymbol(breakpointSymbolRectangle, breakpointBackColor);
		}

		#endregion

		#region Background

		[Category("Appearance")]
		[DefaultValue(typeof(Color), "Window")]
		public override Color BackColor
		{
			get
			{
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
			}
		}

		public override void ResetBackColor()
		{
			base.BackColor = SystemColors.Window;
		}

		#endregion

		#endregion

		#endregion

		#region Behaviour

		#region Scrolling

		[CLSCompliant(false)]
		[Category("Behavior")]
		[DefaultValue((object)(ushort)65535)]
		public ushort MaximumOffset
		{
			get
			{
				return maximumOffset;
			}
			set
			{
				if (maximumOffset <= 0)
					throw new ArgumentOutOfRangeException("value");

				maximumOffset = value;

				UpdateScrollBars();
			}
		}

		public override bool AutoScroll
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		#endregion

		#region Current Instruction

		[Category("Behavior")]
		[DefaultValue(false)]
		public bool ShowCurrentInstruction
		{
			get
			{
				return showCurrentInstruction;
			}
			set
			{
				if (value != showCurrentInstruction)
				{
					showCurrentInstruction = value;
					Invalidate();
				}
			}
		}

		[CLSCompliant(false)]
		[Category("Behavior")]
		[DefaultValue((object)(ushort)0)]
		public ushort CurrentInstructionOffset
		{
			get
			{
				return currentInstructionOffset;
			}
			set
			{
				if (memory == null)
					return;

				currentInstructionOffset = value;

				Invalidate();
			}
		}

		#endregion

		[CLSCompliant(false)]
		[Category("Behavior")]
		[DefaultValue((object)(ushort)0)]
		public ushort SelectedOffset
		{
			get
			{
				return selectedOffset;
			}
			set
			{
				if (memory == null)
					return;

				selectedOffset = value;

				Invalidate();
			}
		}

		[CLSCompliant(false)]
		[Category("Behavior")]
		public IMemory Memory
		{
			get
			{
				return memory;
			}
			set
			{
				if (value != memory)
				{
					if (debuggableMemory != null)
						debuggableMemory.BreakpointUpdate -= OnBreakpointUpdate;
					memory = value;
					selectedOffset = 0;
					currentInstructionOffset = 0;
					AutoScrollPosition = Point.Empty;
					debuggableMemory = memory as IDebuggable;
					if (debuggableMemory != null)
						debuggableMemory.BreakpointUpdate += OnBreakpointUpdate;
					UpdateScrollBars();
					ClearCache();
					UpdateCache();
				}
			}
		}

		#endregion

		#endregion

		#region Cache Management

		private unsafe void UpdateSymbol(Rectangle symbolRectangle, Color symbolColor)
		{
			BitmapData sourceBitmapData, destinationBitmapData;

			lock (defaultSymbolsBitmap)
			{
				sourceBitmapData = defaultSymbolsBitmap.LockBits(symbolRectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
				destinationBitmapData = symbolsBitmap.LockBits(symbolRectangle, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

				int width = symbolRectangle.Width,
					height = symbolRectangle.Height;

				byte* srcLine = (byte*)sourceBitmapData.Scan0,
					dstLine = (byte*)destinationBitmapData.Scan0;
				uint* srcPixel, dstPixel;

				uint mask = (uint)symbolColor.ToArgb();

				for (int i = 0; i < height; i++)
				{
					srcPixel = (uint*)srcLine;
					dstPixel = (uint*)dstLine;

					for (int j = 0; j < width; j++)
						*dstPixel++ = *srcPixel++ & mask;

					srcLine += sourceBitmapData.Stride;
					dstLine += destinationBitmapData.Stride;
				}

				symbolsBitmap.UnlockBits(destinationBitmapData);
				defaultSymbolsBitmap.UnlockBits(sourceBitmapData);
			}
		}

		private void MeasureLine()
		{
			Size textSize = TextRenderer.MeasureText(" ", Font);

			lineHeight = textSize.Height + 2;

			if (lineHeight < 18) // 16 px + 2px for images + margin
				lineHeight = 18;

			UpdateScrollBars();
		}

		private void OnBreakpointUpdate(object sender, EventArgs e)
		{
			Refresh();
		}

		public override void Refresh()
		{
			RefreshCache();
			base.Refresh();
		}

		private void RefreshCache()
		{
			ClearCache();
			UpdateCache();
		}

		private void ClearCache()
		{
			instructionCache.Clear();
		}

		private void UpdateCache()
		{
			Instruction instruction;
			ushort offset, length;
			int lineCount;
			string text;
			bool cacheChanged, debuggable;

			if (memory == null)
				return;

			debuggable = debuggableMemory != null;

			// Do not change the cache by default
			cacheChanged = false;

			// Calculate the desired offset
			offset = (ushort)(-AutoScrollPosition.Y / lineHeight);
			// Calculate the desired cache size
			lineCount = 1 + ClientRectangle.Height / lineHeight;

			// Find the desired offset in the list if possible
			for (int i = 0; i < instructionCache.Count; i++)
			{
				instruction = instructionCache[i];

				if (instruction.Offset == offset)
				{
					if (i > 0)
					{
						instructionCache.RemoveRange(0, i);
						cacheChanged = true;
					}
					break;
				}
				else if (instruction.Offset > offset || i == instructionCache.Count - 1 && instruction.Offset < offset)
				{
					instructionCache.Clear();
					cacheChanged = true;
					break;
				}
			}

			// If the cache is not empty, change the parameters in order to fill it to the desired size
			if (instructionCache.Count > 0)
			{
				instruction = instructionCache[instructionCache.Count - 1];
				offset = (ushort)(instruction.Offset + instruction.Length);
				lineCount -= instructionCache.Count;
			}

			if (lineCount > 0)
				cacheChanged = true;

			// Fill the cache to the desired size
			for (int i = 0; i < lineCount; i++)
			{
				text = Utility.Disassemble(memory, offset, DisassembleFlags.Default, out length);
				instructionCache.Add(new Instruction(offset, length, text, debuggable && debuggableMemory.IsBreakPoint(offset)));

				if (length == 0)
					return;
				offset += length;
			}

			if (cacheChanged)
				Invalidate();
		}

		#endregion

		#region Scroll Management

		private void UpdateScrollBars()
		{
			AutoScrollMinSize = new Size(0, (maximumOffset + 1) * lineHeight);
			VerticalScroll.SmallChange = lineHeight;
			VerticalScroll.LargeChange = 10 * lineHeight;
			VerticalScroll.Visible = true;
			VerticalScroll.Enabled = memory != null;
		}

		#region Scroll Offsets

		private int GetScrollDownOffset()
		{
			if (instructionCache.Count > 0)
				return lineHeight * instructionCache[0].Length;
			else
				return VerticalScroll.SmallChange;
		}

		private int GetPageDownOffset()
		{
			if (instructionCache.Count > 0)
			{
				int lastLine = Math.Min(instructionCache.Count - 1, (ClientRectangle.Height - 1) / lineHeight);
				Instruction lastInstruction = instructionCache[lastLine];

				return lineHeight * (lastInstruction.Length + lastInstruction.Offset - instructionCache[0].Offset);
			}
			else
				return VerticalScroll.LargeChange;
		}

		#endregion

		#region Manual Scrolling Functions

		private void ScrollDown()
		{
			int desiredPosition = -AutoScrollPosition.Y + GetScrollDownOffset();

			if (desiredPosition > VerticalScroll.Maximum)
				desiredPosition = VerticalScroll.Maximum;

			AutoScrollPosition = new Point(0, desiredPosition);

			UpdateCache();
		}

		private void ScrollUp()
		{
			int desiredPosition = -AutoScrollPosition.Y - VerticalScroll.SmallChange;

			if (desiredPosition < VerticalScroll.Minimum)
				desiredPosition = VerticalScroll.Minimum;

			AutoScrollPosition = new Point(0, desiredPosition);

			UpdateCache();
		}

		private void PageDown()
		{
			int desiredPosition = -AutoScrollPosition.Y + GetPageDownOffset();

			if (desiredPosition > VerticalScroll.Maximum)
				desiredPosition = VerticalScroll.Maximum;

			AutoScrollPosition = new Point(0, desiredPosition);

			UpdateCache();
		}

		private void PageUp()
		{
			int desiredPosition = -AutoScrollPosition.Y - VerticalScroll.LargeChange;

			if (desiredPosition < VerticalScroll.Minimum)
				desiredPosition = VerticalScroll.Minimum;

			AutoScrollPosition = new Point(0, desiredPosition);

			UpdateCache();
		}

		#endregion

		#region Cache and Visibility Information

		private bool IsLineVisible(int index)
		{
			return index >= 0 && lineHeight * index < ClientSize.Height;
		}

		private int GetOffsetCacheIndex(ushort offset)
		{
			for (int i = 0; i < instructionCache.Count; i++)
			{
				Instruction instruction = instructionCache[i];

				if (instruction.Offset == offset)
					return i;
				else if (instruction.Offset > offset)
					return -1;
			}
			return -1;
		}

		#endregion

		#region Utility

		[CLSCompliant(false)]
		public bool IsOffsetVisible(ushort offset)
		{
			int index = GetOffsetCacheIndex(offset);

			if (index >= 0)
				return IsLineVisible(index);
			else
				return false;
		}

		public bool IsSelectionVisible()
		{
			return IsOffsetVisible(selectedOffset);
		}

		[CLSCompliant(false)]
		public void ScrollOffsetIntoView(ushort offset)
		{
			int index = GetOffsetCacheIndex(offset);

			if (index > 0)
			{
				if (IsLineVisible(index))
				{
					if (index + 1 > ClientSize.Height / lineHeight)
					{
						AutoScrollPosition = new Point(0, (instructionCache[0].Offset + instructionCache[0].Length) * lineHeight);
						instructionCache.RemoveAt(0);
					}
					else
						return;
				}
				else
				{
					AutoScrollPosition = new Point(0, selectedOffset * lineHeight);
					instructionCache.RemoveRange(0, Math.Max(0, ClientSize.Height / lineHeight - index));
				}
			}
			else
				AutoScrollPosition = new Point(0, selectedOffset * lineHeight);

			UpdateCache();
		}

		public void ScrollSelectionIntoView()
		{
			ScrollOffsetIntoView(selectedOffset);
		}

		#endregion

		#endregion

		#region Event Handling

		#region Scrolling

		protected override void OnScroll(ScrollEventArgs se)
		{
			// Only process the input if there is something to display
			if (memory == null)
				return;

			if (se.ScrollOrientation == ScrollOrientation.HorizontalScroll)
				return;

			switch (se.Type)
			{
				case ScrollEventType.SmallDecrement:
					se.NewValue = se.OldValue - VerticalScroll.SmallChange;
					if (se.NewValue < VerticalScroll.Minimum)
						se.NewValue = VerticalScroll.Minimum;
					break;
				case ScrollEventType.SmallIncrement:
					se.NewValue = se.OldValue + GetScrollDownOffset();
					if (se.NewValue > VerticalScroll.Maximum)
						se.NewValue = VerticalScroll.Maximum;
					break;

				case ScrollEventType.LargeDecrement:
					se.NewValue = se.OldValue - VerticalScroll.LargeChange;
					if (se.NewValue < VerticalScroll.Minimum)
						se.NewValue = VerticalScroll.Minimum;
					break;
				case ScrollEventType.LargeIncrement:
					se.NewValue = se.OldValue + GetPageDownOffset();
					if (se.NewValue > VerticalScroll.Maximum)
						se.NewValue = VerticalScroll.Maximum;
					break;

				case ScrollEventType.First:
					se.NewValue = VerticalScroll.Minimum;
					break;
				case ScrollEventType.Last:
					se.NewValue = VerticalScroll.Maximum;
					break;

				case ScrollEventType.EndScroll:
				case ScrollEventType.ThumbPosition:
				case ScrollEventType.ThumbTrack:
					break;
			}

			base.OnScroll(se);

			AutoScrollPosition = new Point(0, se.NewValue);

			UpdateCache();
		}

		#endregion

		#region Sizing

		protected override void OnLayout(LayoutEventArgs levent)
		{
			AdjustFormScrollbars(true);
			if (LayoutEngine.Layout(this, levent) && Parent != null)
				Parent.PerformLayout(levent.AffectedControl, levent.AffectedProperty);
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			if (this.IsHandleCreated)
				UpdateCache();
			base.OnSizeChanged(e);
		}

		#endregion

		#region Mouse

		private void HandleMouse(int x, int y)
		{
			int line = y / lineHeight;

			if (memory == null)
				return;

			if (IsLineVisible(line))
			{
				selectedOffset = instructionCache[line].Offset;
				Invalidate();
			}
			else if (line < 0)
			{
				ScrollUp();
				selectedOffset = instructionCache[0].Offset;
			}
			else
			{
				ScrollDown();
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			Focus();
			if (e.Button == MouseButtons.Left)
				HandleMouse(e.X, e.Y);
			base.OnMouseDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				HandleMouse(e.X, e.Y);
			base.OnMouseMove(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				HandleMouse(e.X, e.Y);
			base.OnMouseUp(e);
		}

		#endregion

		#region Keyboard

		protected override bool IsInputKey(Keys keyData)
		{
			if (keyData == Keys.Up ||
				keyData == Keys.Down ||
				keyData == Keys.PageUp ||
				keyData == Keys.PageDown)
				return true;
			else
				return base.IsInputKey(keyData);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			int index;

			if (memory == null)
				goto SkipEvent;

			if (e.KeyCode == Keys.Up)
			{
				index = GetOffsetCacheIndex(selectedOffset);

				if (index > 0)
					selectedOffset = instructionCache[index - 1].Offset;
				else
					selectedOffset--;

				ScrollSelectionIntoView();
				Invalidate();

				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Down)
			{
				index = GetOffsetCacheIndex(selectedOffset);

				if (index >= 0 && index + 1 < instructionCache.Count)
					selectedOffset = instructionCache[index + 1].Offset;
				else
					selectedOffset++;

				ScrollSelectionIntoView();
				Invalidate();

				e.Handled = true;
			}

		SkipEvent:
			base.OnKeyDown(e);
		}

		#endregion

		#region Painting

		protected override void OnPaint(PaintEventArgs e)
		{
			int lineWidth = ClientRectangle.Width,
				lineTop = 0,
				clientHeight = ClientRectangle.Height;
			int lineCount = clientHeight / lineHeight;
			Color foreColor = ForeColor,
				backColor, textColor;

			e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);

			for (int i = 0; i < instructionCache.Count; i++)
			{
				Instruction instruction = instructionCache[i];
				Rectangle lineRectangle = new Rectangle(18, lineTop, lineWidth - 18, lineHeight);
				
				if (showCurrentInstruction && instruction.Offset == currentInstructionOffset)
				{
					e.Graphics.FillRectangle(currentInstructionBrush, lineRectangle);
					textColor = currentInstructionForeColor;
					backColor = currentInstructionBackColor;
				}
				else if (instruction.IsBreakpoint)
				{
					e.Graphics.FillRectangle(breakpointBrush, lineRectangle);
					textColor = breakpointForeColor;
					backColor = breakpointBackColor;
				}
				else if (instruction.Offset == selectedOffset)
				{
					e.Graphics.FillRectangle(selectionBrush, lineRectangle);
					textColor = selectionForeColor;
					backColor = selectionBackColor;
				}
				else
					textColor = foreColor;

				if (showCurrentInstruction && instruction.Offset == currentInstructionOffset)
					e.Graphics.DrawImage(symbolsBitmap, 1, lineTop + (lineHeight - 16) / 2, currentInstructionSymbolRectangle, GraphicsUnit.Pixel);
				else if (instruction.IsBreakpoint)
					e.Graphics.DrawImage(symbolsBitmap, 1, lineTop + (lineHeight - 16) / 2, breakpointSymbolRectangle, GraphicsUnit.Pixel);
				TextRenderer.DrawText(e.Graphics, instruction.Text, Font, lineRectangle, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.ExpandTabs);

				if (instruction.Offset == selectedOffset)
					ControlPaint.DrawFocusRectangle(e.Graphics, lineRectangle, textColor, selectionBackColor);

				lineTop += lineHeight;

				if (lineTop > clientHeight)
					break;
			}

			base.OnPaint(e);
		}

		#endregion

		#endregion
	}
}