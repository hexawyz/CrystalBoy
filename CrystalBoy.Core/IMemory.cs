namespace CrystalBoy.Core
{
    public interface IMemory
	{
		byte this[ushort offset] { get; set; }
		byte this[byte offsetLow, byte offsetHigh] { get; set; }

		byte ReadByte(ushort offset);
		void WriteByte(ushort offset, byte value);

		byte ReadByte(byte offsetLow, byte offsetHigh);
		void WriteByte(byte offsetLow, byte offsetHigh, byte value);
	}
}
