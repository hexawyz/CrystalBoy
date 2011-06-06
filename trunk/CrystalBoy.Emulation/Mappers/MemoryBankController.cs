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

namespace CrystalBoy.Emulation.Mappers
{
	/// <summary>Represents common features of MBC mappers.</summary>
	/// <remarks>This class shall be used as a base class for MBC-derived mappers emulation.</remarks>
	public abstract class MemoryBankController : Mapper
	{
		protected int romBankInternal, ramBankInternal;
		protected bool ramEnabledInternal;

		/// <summary>Initializes a new instance of the <see cref="MemoryBankController"/> class.</summary>
		/// <param name="bus">The memory bus associated with this instance.</param>
		public MemoryBankController(GameBoyMemoryBus bus)
			: base(bus) { }

		public override void Reset()
		{
			romBankInternal = 1;
			ramBankInternal = 0;
			ramEnabledInternal = false;
			MapRomBank(false, 0);
			MapRomBank(true, 1);
			UnmapRam();
		}

		public virtual int RomBank
		{
			get { return romBankInternal; }
			protected set
			{
				romBankInternal = value;

				MapRomBank(true, romBankInternal);
			}
		}

		public virtual int RamBank
		{
			get { return ramBankInternal; }
			protected set
			{
				ramBankInternal = value;

				if (ramEnabledInternal) MapRamBank(ramBankInternal);
			}
		}

		public virtual bool RamEnabled
		{
			get { return ramEnabledInternal; }
			protected set
			{
				if (ramEnabledInternal = value) MapRamBank(ramBankInternal);
				else UnmapRam();
			}
		}
	}
}
