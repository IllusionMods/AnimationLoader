using System.Reflection;
//using AnimationLoader;

#region Assembly attributes

/*
 * These attributes define various meta-information of the generated DLL.
 * In general, you don't need to touch these. Instead, edit the values in Info.
 */
[assembly: AssemblyTitle(AnimationLoader.PInfo.PluginName + " (" + AnimationLoader.PInfo.GUID + ")")]
[assembly: AssemblyProduct(AnimationLoader.PInfo.PluginName)]
[assembly: AssemblyVersion(AnimationLoader.PInfo.Version)]
[assembly: AssemblyFileVersion(AnimationLoader.PInfo.Version)]

#endregion Assembly attributes

// Log ID: 0011, 0019, 0022, 0023, 0024, 0026, 0027, 0034

namespace AnimationLoader
{
    public struct PInfo
    {
        public const string GUID = "essuhauled.animationloader";
#if TEST
        public const string PluginDisplayName = "Animation Loader Test";
#elif DEBUG
        public const string PluginDisplayName = "Animation Loader Debug";
#else
        public const string PluginDisplayName = "Animation Loader";
#endif
        public const string Version = "1.1.2.0";
#if KK
        public const string PluginName = "AnimationLoader.Koikatu";
#else
        public const string PluginName = "AnimationLoader.KoikatsuSunshine";
#endif
    }
}
