temp = %OP1% + %OP2%;
carryFlag = temp > 0xFFFF;
halfCarryFlag = (%OP1% & 0xFFF) + (%OP2% & 0xFFF) > 0xFFF;
%OP1% = (ushort)temp;
