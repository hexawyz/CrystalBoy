using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace CrystalBoy.Core.Windows.Forms
{
    [ToolboxBitmap(typeof(DisassemblyView))]
    public partial class DisassemblyView : UserControl
    {
        private readonly struct Instruction
        {
            public Instruction(ushort offset, ushort length, string text, bool isBreakpoint)
            {
                Offset = offset;
                Length = length;
                Text = text;
                IsBreakpoint = isBreakpoint;
            }

            public readonly ushort Offset;
            public readonly ushort Length;
            public readonly string Text;
            public readonly bool IsBreakpoint;
        }

        new private static readonly Font DefaultFont = new Font("Courier New", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly Bitmap DefaultSymbolsBitmap = new Bitmap(typeof(DisassemblyView).Assembly.GetManifestResourceStream(typeof(DisassemblyView), "symbols.png"));
        private static readonly Rectangle BreakpointSymbolRectangle = new Rectangle(0, 0, 16, 16);
        private static readonly Rectangle CurrentInstructionSymbolRectangle = new Rectangle(16, 0, 16, 16);

        private IMemory _memory;
        private IDebuggable _debuggableMemory;
        private List<Instruction> _instructionCache;
        private ushort _maximumOffset;
        private ushort _selectedOffset;
        private ushort _currentInstructionOffset;
        private bool _showCurrentInstruction;
        private int _lineHeight;
        private Color _selectionForeColor;
        private Color _selectionBackColor;
        private Color _currentInstructionForeColor;
        private Color _currentInstructionBackColor;
        private Color _breakpointForeColor;
        private Color _breakpointBackColor;
        private Brush _selectionBrush;
        private Brush _breakpointBrush;
        private Brush _currentInstructionBrush;
        private readonly Bitmap _symbolsBitmap;

        #region Constructor

        public DisassemblyView()
        {
            _symbolsBitmap = new Bitmap(DefaultSymbolsBitmap.Width, DefaultSymbolsBitmap.Height, PixelFormat.Format32bppArgb);
            _maximumOffset = 0xFFFF;
            _instructionCache = new List<Instruction>();
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
            get => base.Font;
            set
            {
                if (value != base.Font)
                {
                    if (value is object)
                    {
                        base.Font = value;
                        MeasureLine();
                    }
                    else if (base.Font != DefaultFont)
                    {
                        base.Font = DefaultFont;
                        MeasureLine();
                    }
                }
            }
        }

        public bool ShouldSerializeFont() => base.Font != DefaultFont;

        public override void ResetFont()
        {
            base.Font = DefaultFont;
            MeasureLine();
        }

        #endregion

        #region Colors

        #region Selection

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "HighlightText")]
        public Color SelectionForeColor
        {
            get => _selectionForeColor;
            set
            {
                if (value != _selectionForeColor)
                {
                    _selectionForeColor = value;
                    Invalidate();
                }
            }
        }

        public virtual void ResetSelectionForeColor()
        {
            if (_selectionForeColor != SystemColors.HighlightText)
            {
                _selectionForeColor = SystemColors.HighlightText;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Highlight")]
        public Color SelectionBackColor
        {
            get => _selectionBackColor;
            set
            {
                if (value != _selectionBackColor)
                {
                    _selectionBackColor = value;
                    Invalidate();
                }
            }
        }

        public virtual void ResetSelectionBackColor()
        {
            if (_selectionBackColor != SystemColors.Highlight)
            {
                _selectionBackColor = SystemColors.Highlight;
                _selectionBrush = new SolidBrush(_selectionBackColor);
                Invalidate();
            }
        }

        #endregion

        #region Current Instruction

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Black")]
        public Color CurrentInstructionForeColor
        {
            get => _currentInstructionForeColor;
            set
            {
                if (value != _currentInstructionForeColor)
                {
                    _currentInstructionForeColor = value;
                    Invalidate();
                }
            }
        }

        public virtual void ResetCurrentInstructionForeColor()
        {
            if (_currentInstructionForeColor != Color.Black)
            {
                _currentInstructionForeColor = Color.Black;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Lime")]
        public Color CurrentInstructionBackColor
        {
            get => _currentInstructionBackColor;
            set
            {
                if (value != _currentInstructionBackColor)
                {
                    _currentInstructionBackColor = value;
                    UpdateCurrentInstructionSymbol();
                    Invalidate();
                }
            }
        }

        public virtual void ResetCurrentInstructionBackColor()
        {
            if (_currentInstructionBackColor != Color.Lime)
            {
                _currentInstructionBackColor = Color.Lime;
                _currentInstructionBrush = new SolidBrush(_currentInstructionBackColor);
                UpdateCurrentInstructionSymbol();
            }
        }

        private void UpdateCurrentInstructionSymbol()
        {
            UpdateSymbol(CurrentInstructionSymbolRectangle, _currentInstructionBackColor);
        }

        #endregion

        #region Breakpoint

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "White")]
        public Color BreakpointForeColor
        {
            get => _breakpointForeColor;
            set
            {
                if (value != _breakpointForeColor)
                {
                    _breakpointForeColor = value;
                    Invalidate();
                }
            }
        }

        public virtual void ResetBreakpointForeColor()
        {
            if (_breakpointForeColor != Color.White)
            {
                _breakpointForeColor = Color.White;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "DarkRed")]
        public Color BreakpointBackColor
        {
            get => _breakpointBackColor;
            set
            {
                if (value != _breakpointBackColor)
                {
                    _breakpointBackColor = value;
                    UpdateBreakpointSymbol();
                    Invalidate();
                }
            }
        }

        public virtual void ResetBreakpointBackColor()
        {
            if (_breakpointBackColor != Color.DarkRed)
            {
                _breakpointBackColor = Color.DarkRed;
                _breakpointBrush = new SolidBrush(_breakpointBackColor);
                UpdateBreakpointSymbol();
            }
        }

        private void UpdateBreakpointSymbol()
        {
            UpdateSymbol(BreakpointSymbolRectangle, _breakpointBackColor);
        }

        #endregion

        #region Background

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Window")]
        public override Color BackColor
        {
            get => base.BackColor;
            set => base.BackColor = value;
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

        [Category("Behavior")]
        [DefaultValue((object)(ushort)65535)]
        public ushort MaximumOffset
        {
            get => _maximumOffset;
            set
            {
                if (_maximumOffset <= 0)
                    throw new ArgumentOutOfRangeException("value");

                _maximumOffset = value;

                UpdateScrollBars();
            }
        }

        public override bool AutoScroll
        {
            get => false;
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
            get => _showCurrentInstruction;
            set
            {
                if (value != _showCurrentInstruction)
                {
                    _showCurrentInstruction = value;
                    Invalidate();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue((object)(ushort)0)]
        public ushort CurrentInstructionOffset
        {
            get => _currentInstructionOffset;
            set
            {
                if (_memory is null)
                    return;

                _currentInstructionOffset = value;

                Invalidate();
            }
        }

        #endregion

        [Category("Behavior")]
        [DefaultValue((object)(ushort)0)]
        public ushort SelectedOffset
        {
            get => _selectedOffset;
            set
            {
                if (_memory is null)
                    return;

                _selectedOffset = value;

                Invalidate();
            }
        }

        [Category("Behavior")]
        public IMemory Memory
        {
            get => _memory;
            set
            {
                if (value != _memory)
                {
                    if (_debuggableMemory is object)
                        _debuggableMemory.BreakpointUpdate -= OnBreakpointUpdate;
                    _memory = value;
                    _selectedOffset = 0;
                    _currentInstructionOffset = 0;
                    AutoScrollPosition = Point.Empty;
                    _debuggableMemory = _memory as IDebuggable;
                    if (_debuggableMemory is object)
                        _debuggableMemory.BreakpointUpdate += OnBreakpointUpdate;
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
            BitmapData sourceBitmapData;
            BitmapData destinationBitmapData;

            lock (DefaultSymbolsBitmap)
            {
                sourceBitmapData = DefaultSymbolsBitmap.LockBits(symbolRectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                destinationBitmapData = _symbolsBitmap.LockBits(symbolRectangle, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

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

                _symbolsBitmap.UnlockBits(destinationBitmapData);
                DefaultSymbolsBitmap.UnlockBits(sourceBitmapData);
            }
        }

        private void MeasureLine()
        {
            var textSize = TextRenderer.MeasureText(" ", Font);

            _lineHeight = textSize.Height + 2;

            if (_lineHeight < 18) // 16 px + 2px for images + margin
                _lineHeight = 18;

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
            _instructionCache.Clear();
        }

        private void UpdateCache()
        {
            Instruction instruction;
            ushort offset;
            ushort length;
            int lineCount;
            string text;
            bool cacheChanged, debuggable;

            if (_memory is null)
                return;

            debuggable = _debuggableMemory is object;

            // Do not change the cache by default
            cacheChanged = false;

            // Calculate the desired offset
            offset = (ushort)(-AutoScrollPosition.Y / _lineHeight);
            // Calculate the desired cache size
            lineCount = 1 + ClientRectangle.Height / _lineHeight;

            // Find the desired offset in the list if possible
            for (int i = 0; i < _instructionCache.Count; i++)
            {
                instruction = _instructionCache[i];

                if (instruction.Offset == offset)
                {
                    if (i > 0)
                    {
                        _instructionCache.RemoveRange(0, i);
                        cacheChanged = true;
                    }
                    break;
                }
                else if (instruction.Offset > offset || i == _instructionCache.Count - 1 && instruction.Offset < offset)
                {
                    _instructionCache.Clear();
                    cacheChanged = true;
                    break;
                }
            }

            // If the cache is not empty, change the parameters in order to fill it to the desired size
            if (_instructionCache.Count > 0)
            {
                instruction = _instructionCache[_instructionCache.Count - 1];
                offset = (ushort)(instruction.Offset + instruction.Length);
                lineCount -= _instructionCache.Count;
            }

            if (lineCount > 0)
                cacheChanged = true;

            // Fill the cache to the desired size
            for (int i = 0; i < lineCount; i++)
            {
                text = Utility.Disassemble(_memory, offset, DisassembleFlags.Default, out length);
                _instructionCache.Add(new Instruction(offset, length, text, debuggable && _debuggableMemory.IsBreakpoint(offset)));

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
            AutoScrollMinSize = new Size(0, (_maximumOffset + 1) * _lineHeight);
            VerticalScroll.SmallChange = _lineHeight;
            VerticalScroll.LargeChange = 10 * _lineHeight;
            VerticalScroll.Visible = true;
            VerticalScroll.Enabled = _memory is object;
        }

        #region Scroll Offsets

        private int GetScrollDownOffset()
            => _instructionCache.Count > 0 ?
                _lineHeight * _instructionCache[0].Length :
                VerticalScroll.SmallChange;

        private int GetPageDownOffset()
        {
            if (_instructionCache.Count > 0)
            {
                int lastLine = Math.Min(_instructionCache.Count - 1, (ClientRectangle.Height - 1) / _lineHeight);
                Instruction lastInstruction = _instructionCache[lastLine];

                return _lineHeight * (lastInstruction.Length + lastInstruction.Offset - _instructionCache[0].Offset);
            }
            else
            {
                return VerticalScroll.LargeChange;
            }
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
            => index >= 0 && _lineHeight * index < ClientSize.Height;

        private int GetOffsetCacheIndex(ushort offset)
        {
            for (int i = 0; i < _instructionCache.Count; i++)
            {
                Instruction instruction = _instructionCache[i];

                if (instruction.Offset == offset)
                    return i;
                else if (instruction.Offset > offset)
                    return -1;
            }
            return -1;
        }

        #endregion

        #region Utility

        public bool IsOffsetVisible(ushort offset)
            => GetOffsetCacheIndex(offset) is int index && index >= 0 && IsLineVisible(index);

        public bool IsSelectionVisible()
            => IsOffsetVisible(_selectedOffset);

        public void ScrollOffsetIntoView(ushort offset)
        {
            int index = GetOffsetCacheIndex(offset);

            if (index > 0)
            {
                if (IsLineVisible(index))
                {
                    if (index + 1 > ClientSize.Height / _lineHeight)
                    {
                        AutoScrollPosition = new Point(0, (_instructionCache[0].Offset + _instructionCache[0].Length) * _lineHeight);
                        _instructionCache.RemoveAt(0);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    AutoScrollPosition = new Point(0, _selectedOffset * _lineHeight);
                    _instructionCache.RemoveRange(0, Math.Max(0, ClientSize.Height / _lineHeight - index));
                }
            }
            else
            {
                AutoScrollPosition = new Point(0, _selectedOffset * _lineHeight);
            }

            UpdateCache();
        }

        public void ScrollSelectionIntoView()
        {
            ScrollOffsetIntoView(_selectedOffset);
        }

        #endregion

        #endregion

        #region Event Handling

        #region Scrolling

        protected override void OnScroll(ScrollEventArgs se)
        {
            // Only process the input if there is something to display
            if (_memory is null)
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
            if (LayoutEngine.Layout(this, levent) && Parent is object)
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
            int line = y / _lineHeight;

            if (_memory is null)
                return;

            if (IsLineVisible(line))
            {
                _selectedOffset = _instructionCache[line].Offset;
                Invalidate();
            }
            else if (line < 0)
            {
                ScrollUp();
                _selectedOffset = _instructionCache[0].Offset;
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
            => keyData == Keys.Up ||
                keyData == Keys.Down ||
                keyData == Keys.PageUp ||
                keyData == Keys.PageDown ||
                base.IsInputKey(keyData);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            int index;

            if (_memory is null)
                goto SkipEvent;

            if (e.KeyCode == Keys.Up)
            {
                index = GetOffsetCacheIndex(_selectedOffset);

                if (index > 0)
                    _selectedOffset = _instructionCache[index - 1].Offset;
                else
                    _selectedOffset--;

                ScrollSelectionIntoView();
                Invalidate();

                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                index = GetOffsetCacheIndex(_selectedOffset);

                if (index >= 0 && index + 1 < _instructionCache.Count)
                    _selectedOffset = _instructionCache[index + 1].Offset;
                else
                    _selectedOffset++;

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
            int lineCount = clientHeight / _lineHeight;
            Color foreColor = ForeColor;
            Color backColor;
            Color textColor;

            e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);

            for (int i = 0; i < _instructionCache.Count; i++)
            {
                Instruction instruction = _instructionCache[i];
                Rectangle lineRectangle = new Rectangle(18, lineTop, lineWidth - 18, _lineHeight);

                if (_showCurrentInstruction && instruction.Offset == _currentInstructionOffset)
                {
                    e.Graphics.FillRectangle(_currentInstructionBrush, lineRectangle);
                    textColor = _currentInstructionForeColor;
                    backColor = _currentInstructionBackColor;
                }
                else if (instruction.IsBreakpoint)
                {
                    e.Graphics.FillRectangle(_breakpointBrush, lineRectangle);
                    textColor = _breakpointForeColor;
                    backColor = _breakpointBackColor;
                }
                else if (instruction.Offset == _selectedOffset)
                {
                    e.Graphics.FillRectangle(_selectionBrush, lineRectangle);
                    textColor = _selectionForeColor;
                    backColor = _selectionBackColor;
                }
                else
                    textColor = foreColor;

                if (_showCurrentInstruction && instruction.Offset == _currentInstructionOffset)
                    e.Graphics.DrawImage(_symbolsBitmap, 1, lineTop + (_lineHeight - 16) / 2, CurrentInstructionSymbolRectangle, GraphicsUnit.Pixel);
                else if (instruction.IsBreakpoint)
                    e.Graphics.DrawImage(_symbolsBitmap, 1, lineTop + (_lineHeight - 16) / 2, BreakpointSymbolRectangle, GraphicsUnit.Pixel);
                TextRenderer.DrawText(e.Graphics, instruction.Text, Font, lineRectangle, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.ExpandTabs);

                if (instruction.Offset == _selectedOffset)
                    ControlPaint.DrawFocusRectangle(e.Graphics, lineRectangle, textColor, _selectionBackColor);

                lineTop += _lineHeight;

                if (lineTop > clientHeight)
                    break;
            }

            base.OnPaint(e);
        }

        #endregion

        #endregion
    }
}