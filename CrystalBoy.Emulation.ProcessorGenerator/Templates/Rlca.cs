carryFlag = (a & 0x80) != 0;
if (carryFlag)
	a = (byte)((a << 1) | 0x01);
else
	a <<= 1;
