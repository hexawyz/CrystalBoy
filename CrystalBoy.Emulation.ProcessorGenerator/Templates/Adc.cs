if (carryFlag)
{
	temp = %OP1% + %OP2% + 1;
	halfCarryFlag = (%OP1% & 0xF) + (%OP2% & 0xF) > 0xE;
}
else
{
	temp = %OP1% + %OP2%;
	halfCarryFlag = (%OP1% & 0xF) + (%OP2% & 0xF) > 0xF;
}
carryFlag = temp > 0xFF;
%OP1% = (byte)temp;
zeroFlag = %OP1% == 0;
