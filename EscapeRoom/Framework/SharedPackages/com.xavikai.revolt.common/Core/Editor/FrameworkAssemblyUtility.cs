#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>Use Unity's reload-aware assembly list when the editor provides it.</summary>
    public static class FrameworkAssemblyUtility
    {
        public static IEnumerable<Assembly> GetLoadedAssemblies()
        {
#if UNITY_6000_6_OR_NEWER
            return UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies();
#else
            return AppDomain.CurrentDomain.GetAssemblies();
#endif
        }
    }
}
#endif
