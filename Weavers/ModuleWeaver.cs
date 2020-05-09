using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Weavers
{
    public class ModuleWeaver : BaseModuleWeaver
	{
		public override void Execute()
		{
			var memoryType =
			(
				from type in ModuleDefinition.Types
				where type.FullName == "CrystalBoy.Core.Memory"
				select type
			).Single();

			RewriteCopyMethod
			(
				(
					from method in memoryType.Methods
					where method.Name == "Copy" &&
						method.Parameters.Count == 3 &&
						IsVoid(method.ReturnType) &&
						IsVoidPointer(method.Parameters[0].ParameterType) &&
						IsVoidPointer(method.Parameters[1].ParameterType) &&
						IsUInt32(method.Parameters[2].ParameterType)
					select method
				).Single()
			);

			RewriteSetMethod
			(
				(
					from method in memoryType.Methods
					where method.Name == "Set" &&
						method.Parameters.Count == 3 &&
						IsVoid(method.ReturnType) &&
						IsVoidPointer(method.Parameters[0].ParameterType) &&
						IsByte(method.Parameters[1].ParameterType) &&
						IsUInt32(method.Parameters[2].ParameterType)
					select method
				).Single()
			);

			WriteInfo("Weaving of memory methods completed.");
		}

		private static bool IsVoidPointer(TypeReference type) => type.IsPointer && IsVoid(type.GetElementType());
		private static bool IsVoid(TypeReference type) => type.FullName == "System.Void";
		private static bool IsByte(TypeReference type) => type.FullName == "System.Byte";
		private static bool IsUInt32(TypeReference type) => type.FullName == "System.UInt32";

		private static void RewriteCopyMethod(MethodDefinition method)
		{
			method.Body.Instructions.Clear();

			var ilProcessor = method.Body.GetILProcessor();

			ilProcessor.Emit(OpCodes.Ldarg_0);
			ilProcessor.Emit(OpCodes.Ldarg_1);
			ilProcessor.Emit(OpCodes.Ldarg_2);
			ilProcessor.Emit(OpCodes.Cpblk);
			ilProcessor.Emit(OpCodes.Ret);
		}

		private static void RewriteSetMethod(MethodDefinition method)
		{
			method.Body.Instructions.Clear();

			var ilProcessor = method.Body.GetILProcessor();

			ilProcessor.Emit(OpCodes.Ldarg_0);
			ilProcessor.Emit(OpCodes.Ldarg_1);
			ilProcessor.Emit(OpCodes.Ldarg_2);
			ilProcessor.Emit(OpCodes.Initblk);
			ilProcessor.Emit(OpCodes.Ret);
		}

		public override IEnumerable<string> GetAssembliesForScanning() => Enumerable.Empty<string>();
    }
}
