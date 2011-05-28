carryFlag = (%OP1% & 0x80) != 0;
if (carryFlag)
	%OP1% = (byte)((%OP1% << 1) | 0x01);
else
	%OP1% <<= 1;
zeroFlag = %OP1% == 0;
