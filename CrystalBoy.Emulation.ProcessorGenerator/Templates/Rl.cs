if (carryFlag)
	temp = (%OP1% << 1) | 0x01;
else
	temp = %OP1% << 1;
carryFlag = (%OP1% & 0x80) != 0;
%OP1% = (byte)temp;
zeroFlag = %OP1% == 0;
