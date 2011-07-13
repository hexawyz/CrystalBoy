#region Copyright Notice
// This file is part of CrystalBoy.
// Copyright © 2008-2011 Fabien Barbier
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

namespace CrystalBoy.Emulation
{
	public enum SuperGameBoyCommand : byte
	{
		PAL01 = 0x00,
		PAL23 = 0x01,
		PAL03 = 0x02,
		PAL12 = 0x03,
		ATTR_BLK = 0x04,
		ATTR_LIN = 0x05,
		ATTR_DIV = 0x06,
		ATTR_CHR = 0x07,
		SOUND = 0x08,
		SOU_TRN = 0x09,
		PAL_SET = 0x0a,
		PAL_TRN = 0x0b,
		ATRC_EN = 0x0c,
		TEST_EN = 0x0d,
		ICON_EN = 0x0e,
		DATA_SND = 0x0f,
		DATA_TRN = 0x10,
		MLT_REQ = 0x11,
		JUMP = 0x12,
		CHR_TRN = 0x13,
		PCT_TRN = 0x14,
		ATTR_TRN = 0x15,
		ATTR_SET = 0x16,
		MASK_EN = 0x17,
		OBJ_TRN = 0x18,
		PAL_PRI = 0x19
	}
}
