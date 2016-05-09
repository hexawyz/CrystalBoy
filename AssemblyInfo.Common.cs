using System.Resources;
using System.Reflection;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Retail")]
#endif
[assembly: AssemblyCompany("GoldenCrystal")]
[assembly: AssemblyProduct("CrystalBoy")]
[assembly: AssemblyCopyright("Copyright © GoldenCrystal 2008-2016")]
#if !NEUTRAL_RESOURCES_LANGUAGE_OVERRIDE
[assembly: NeutralResourcesLanguage("en-US")]
#endif

[assembly: AssemblyFileVersion("1.6.0.0")]