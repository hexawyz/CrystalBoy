using System.Resources;
using System.Reflection;
using System.Runtime.CompilerServices;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Retail")]
#endif
[assembly: AssemblyCompany("GoldenCrystal")]
[assembly: AssemblyProduct("CrystalBoy")]
[assembly: AssemblyCopyright("Copyright © GoldenCrystal 2008-2016")]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: SuppressIldasm]

[assembly: AssemblyFileVersion("1.6.0.0")]