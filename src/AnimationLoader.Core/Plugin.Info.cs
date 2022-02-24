using System.Reflection;

using AnimationLoader;

#region Assembly attributes

/*
 * These attributes define various meta-information of the generated DLL.
 * In general, you don't need to touch these. Instead, edit the values in Info.
 */
[assembly: AssemblyTitle(SwapAnim.PluginName + " (" + SwapAnim.GUID + ")")]
[assembly: AssemblyProduct(SwapAnim.PluginName)]
[assembly: AssemblyVersion(SwapAnim.Version)]
[assembly: AssemblyFileVersion(SwapAnim.Version)]

#endregion Assembly attributes

// Log ID: 0011, 0019, 0022, 0023, 0024, 0026, 0027, 0034

namespace AnimationLoader
{
    public partial class SwapAnim
    {
        public const string GUID = "essuhauled.animationloader";
#if DEBUG
        public const string PluginDisplayName = "Animation Loader (Debug)";
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
