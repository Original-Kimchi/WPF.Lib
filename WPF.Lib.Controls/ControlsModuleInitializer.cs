using System.Runtime.CompilerServices;
using WPF.Lib.Controls.Behaviors;

namespace WPF.Lib.Controls;

internal static class ControlsModuleInitializer
{
#pragma warning disable CA2255 // Required to register the library-wide WPF class handler when the assembly loads.
    [ModuleInitializer]
#pragma warning restore CA2255
    internal static void Initialize()
    {
        NestedScrollBehavior.Register();
    }
}
