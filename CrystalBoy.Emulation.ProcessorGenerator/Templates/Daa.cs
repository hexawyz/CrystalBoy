if (negationFlag)
{
	if (halfCarryFlag || (a & 0x0F) > 0x09)
		a -= 0x06;
	if (carryFlag || a > 0x99)
	{
		a -= 0x60;
		carryFlag = true;
	}
}
else
{
	if (halfCarryFlag || (a & 0x0F) > 0x09)
		a += 0x06;
	if (carryFlag || a > 0x99)
	{
		a += 0x60;
		carryFlag = true;
	}
}
zeroFlag = a == 0;
