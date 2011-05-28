if (carryFlag)
	temp = (a << 1) | 0x01;
else
	temp = a << 1;
carryFlag = (a & 0x80) != 0;
a = (byte)temp;
