if (carryFlag)
{
	carryFlag = %OP1% - %OP2% < 1;
	halfCarryFlag = (%OP1% & 0xF) - (%OP2% & 0xF) < 1;
	%OP1% = (byte)(%OP1% - %OP2% - 1);
	zeroFlag = %OP1% == 0;
}
else
{
	carryFlag = %OP1% < %OP2%;
	halfCarryFlag = (%OP1% & 0xF) < (%OP2% & 0xF);
	zeroFlag = %OP1% == %OP2%;
	%OP1% -= %OP2%;
}
