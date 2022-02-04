using System.Reflection;
using AnimationLoader;

#region Assembly attributes

/*
 * These attributes define various meta-information of the generated DLL.
 * In general, you don't need to touch these. Instead, edit the values in Info.
 */
[assembly: AssemblyTitle(PInfo.PluginName + " (" + PInfo.GUID + ")")]
[assembly: AssemblyProduct(PInfo.PluginName)]
[assembly: AssemblyVersion(PInfo.Version)]
[assembly: AssemblyFileVersion(PInfo.Version)]

#endregion Assembly attributes

// Log ID: 0022, 0023, 0024, 0026, 0027, 0034

namespace AnimationLoader
{
    internal struct PInfo
    {
        internal const string GUID = "essuhauled.animationloader";
        internal const string PluginDisplayName = "Animation Loader";
        internal const string Version = "1.1.0.2";
#if KK
        internal const string PluginName = "AnimationLoader.Koikatu";
#elif KKS
        internal const string PluginName = "AnimationLoader.KoikatsuSunshine";
#endif
    }
}
