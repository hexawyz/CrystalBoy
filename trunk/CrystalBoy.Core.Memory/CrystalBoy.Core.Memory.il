.assembly extern mscorlib
{
	.publickeytoken = (B7 7A 5C 56 19 34 E0 89 )
	.ver 2:0:0:0
}

.assembly CrystalBoy.Core.Memory
{
  .ver 1:0:0:0
}

.module CrystalBoy.Core.Memory.dll
.custom instance void [mscorlib]System.CLSCompliantAttribute::.ctor(bool) = { bool(true) }

.class public abstract auto ansi sealed beforefieldinit CrystalBoy.Core.Memory
       extends [mscorlib]System.Object
{
	.custom instance void [mscorlib]System.CLSCompliantAttribute::.ctor(bool) = { bool(false) }
	
	.method public hidebysig static void Copy(void* destination, void* source, uint32 length) cil managed
	{
		.maxstack  3
		ldarg.0
		ldarg.1
		ldarg.2
		cpblk
		ret
	}
	
	.method public hidebysig static void Set(void* destination, uint8 'value', uint32 length) cil managed
	{
		.maxstack  3
		ldarg.0
		ldarg.1
		ldarg.2
		initblk
		ret
	}
}
