carryFlag = (a & 0x01) != 0;
if (carryFlag)
	a = (byte)((a >> 1) | 0x80);
else
	a >>= 1;
