if (carryFlag)
	temp = (a >> 1) | 0x80;
else
	temp = a >> 1;
carryFlag = (a & 0x01) != 0;
a = (byte)temp;
