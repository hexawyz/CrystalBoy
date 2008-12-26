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

if (carryFlag)
{
	temp = %OP1% + %OP2% + 1;
	halfCarryFlag = (%OP1% & 0xF) + (%OP2% & 0xF) > 0xE;
}
else
{
	temp = %OP1% + %OP2%;
	halfCarryFlag = (%OP1% & 0xF) + (%OP2% & 0xF) > 0xF;
}
carryFlag = temp > 0xFF;
%OP1% = (byte)temp;
zeroFlag = %OP1% == 0;
