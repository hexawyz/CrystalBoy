using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrystalBoy.Emulator
{
	internal class DpiAwareForm : Form
	{
		//private sealed class ControlOriginalProperties
		//{
		//	public readonly Font Font;
		//	public readonly Size Size;
		//	public readonly Size ImageScalingSize;

		//	public ControlOriginalProperties(Control control)
		//	{
		//		Font = control.Font;
		//		Size = control.Size;

		//		ImageScalingSize = ((control as ToolStrip)?.ImageScalingSize).GetValueOrDefault();
		//	}
		//}

		//private sealed class ToolStripItemOriginalProperties
		//{
		//	public readonly int Height;

		//	public ToolStripItemOriginalProperties(ToolStripItem item)
		//	{
		//		Height = item.Height;
		//	}
		//}

		//private const int WM_DPICHANGED = 0x02E0;

		//private static readonly ConditionalWeakTable<Control, ControlOriginalProperties> _controlOriginalProperties = new ConditionalWeakTable<Control, ControlOriginalProperties>();
		//private static readonly ConditionalWeakTable<ToolStripItem, ToolStripItemOriginalProperties> _toolStripItemOriginalProperties = new ConditionalWeakTable<ToolStripItem, ToolStripItemOriginalProperties>();
		//private readonly float _originalDpi;
		//private float _currentDpi;
		//private bool _scanDone;

		//public DpiAwareForm()
		//{
		//	AutoScaleMode = AutoScaleMode.Dpi;
		//	AutoScaleDimensions = new SizeF(96, 96);
		//	AutoSizeMode = AutoSizeMode.GrowAndShrink;
		//	_currentDpi = _originalDpi = CurrentAutoScaleDimensions.Width;
		//}

		//protected override void WndProc(ref Message m)
		//{
		//	if (m.Msg == WM_DPICHANGED)
		//	{
		//		m.Result = HandleDpiChanged(m.WParam, m.LParam);
		//	}
		//	else
		//	{
		//		base.WndProc(ref m);
		//	}
		//}

		//private unsafe IntPtr HandleDpiChanged(IntPtr wParam, IntPtr lParam)
		//{
		//	ushort dpiX = (ushort)wParam;
		//	ushort dpiY = (ushort)((uint)wParam >> 16);

		//	var scalingFactor = new SizeF(dpiX / _currentDpi, dpiY / _currentDpi);

		//	if (scalingFactor.Width != 1)
		//	{
		//		Font = ScaleFont(GetOriginalProperties(this).Font, scalingFactor.Width);
		//		ScaleFix(this, dpiX / _originalDpi);
		//		Scale(scalingFactor);

		//		_currentDpi = dpiX;

		//		var newRect = *(NativeMethods.Rect*)lParam;

		//		SetBounds(newRect.Left, newRect.Top, newRect.Right - newRect.Left, newRect.Bottom - newRect.Top);
		//	}

		//	return IntPtr.Zero;
		//}

		//private static ControlOriginalProperties GetOriginalProperties(Control control)
		//{
		//	return _controlOriginalProperties.GetValue(control, c => new ControlOriginalProperties(c));
		//}

		//private static ToolStripItemOriginalProperties GetOriginalProperties(ToolStripItem item)
		//{
		//	return _toolStripItemOriginalProperties.GetValue(item, i => new ToolStripItemOriginalProperties(i));
		//}

		//private static void ScaleFix(Control control, float scalingFactor)
		//{
		//	var toolStrip = control as ToolStrip;

		//	if (toolStrip != null)
		//	{
		//		var originalProperties = GetOriginalProperties(control);

		//		toolStrip.Font = ScaleFont(originalProperties.Font, scalingFactor);
		//		toolStrip.ImageScalingSize = new Size((int)(originalProperties.ImageScalingSize.Width * scalingFactor), (int)(originalProperties.ImageScalingSize.Height * scalingFactor));

		//		foreach (ToolStripItem item in toolStrip.Items)
		//		{
		//			ScaleFix(item, scalingFactor);
		//		}
		//	}

		//	foreach (Control c in control.Controls)
		//	{
		//		ScaleFix(c, scalingFactor);
		//	}
		//}

		//private static void DetectOriginalProperties(ToolStripItem item)
		//{
		//	GetOriginalProperties(item);

		//	var toolStripDropDownItem = item as ToolStripDropDownItem;

		//	if (toolStripDropDownItem != null)
		//	{
		//		foreach (ToolStripItem i in toolStripDropDownItem.DropDownItems)
		//		{
		//			DetectOriginalProperties(i);
		//		}
		//	}
		//}

		//private static void ScaleFix(ToolStripItem item, float scalingFactor)
		//{
		//	//var originalProperties = GetOriginalProperties(item);
			
		//	//item.Height = (int)(originalProperties.Height * scalingFactor);
			
		//	var toolStripDropDownItem = item as ToolStripDropDownItem;

		//	if (toolStripDropDownItem != null)
		//	{
		//		foreach (ToolStripItem i in toolStripDropDownItem.DropDownItems)
		//		{
		//			ScaleFix(i, scalingFactor);
		//		}
		//	}
		//}

		//private static Font ScaleFont(Font font, float scalingFactor) => scalingFactor != 1 ?
		//	new Font(font.FontFamily, font.Size * scalingFactor, font.Style, font.Unit, font.GdiCharSet, font.GdiVerticalFont) :
		//	font;

		//protected virtual void OnDpiChanged()
		//{
		//}
	}
}
