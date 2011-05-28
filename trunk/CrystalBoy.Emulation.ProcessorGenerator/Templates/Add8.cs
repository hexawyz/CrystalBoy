temp = %OP1% + %OP2%;
carryFlag = temp > 0xFF;
halfCarryFlag = (%OP1% & 0xF) + (%OP2% & 0xF) > 0xF;
%OP1% = (byte)temp;
zeroFlag = %OP1% == 0;
