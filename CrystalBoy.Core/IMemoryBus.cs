namespace CrystalBoy.Core
{
    public interface IMemoryBus : IMemory, IPortMap
	{
		int LcdCycleCount { get; }
		bool AddCycles(int count);

		MemoryType GetMapping(ushort offset);
		MemoryType GetMapping(ushort offset, out int bank);
	}
}
