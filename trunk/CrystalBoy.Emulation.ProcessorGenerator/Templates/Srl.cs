carryFlag = (%OP1% & 0x01) != 0;
%OP1% = (byte)(%OP1% >> 1);
zeroFlag = %OP1% == 0;
