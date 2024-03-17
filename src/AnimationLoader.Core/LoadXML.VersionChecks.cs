//
// Load XML animation information
//
using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using BepInEx.Logging;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        private static void VersionChecks(XElement manifest)
        {
            var guid = manifest?.Element("guid")?.Value;
            var version = manifest?.Element("version")?.Value;
            var author = manifest?.Element("author")?.Value;
            var kplugBundleVersion = manifest?.Element("KPluganimBundle")?.Value;

            _animationLoaderVersion = manifest?.Element("AnimationLoaderVersion");

            if (_animationLoaderVersion is not null)
            {
                var lines = new StringBuilder();
                var bundle = ".";
                var warning = false;
                var alVersion = new Version(_animationLoaderVersion.Value);
                var pVersion = new Version(Version);

                if (kplugBundleVersion != null)
                {
                    var bundleVersion = BundleVersion();
                    var minVersion = new Version(kplugBundleVersion);
                    if (bundleVersion != null)
                    {
                        if (bundleVersion.CompareTo(minVersion) < 0)
                        {
                            bundle = $" KPlug Animation Bundle " +
                                $"version={bundleVersion} minimum={minVersion} some " +
                                $"features may not work upgrade to latest version.";
                            warning = true;
                        }
                        else
                        {
                            bundle = $" KPlug Animation Bundle version={bundleVersion} " +
                                $"minimum={minVersion}.";
                        }
                    }
                }
                if (pVersion != null)
                {
                    if (pVersion.CompareTo(alVersion) < 0)
                    {
                        var tmp = author is not null ? author : "N/A";
                        lines.AppendLine($"0011: Manifest " +
                            $"guid={guid} version={version} author=[{tmp}] " +
                            $"AnimationLoader version={pVersion} minimum={alVersion} " +
                            $"some features may not work upgrade to latest version,{bundle}");
                        warning = true;
                    }
                    else
                    {
                        var tmp = author is not null ? author : "N/A";
                        lines.AppendLine($"0011: Manifest " +
                            $"guid={guid} version={version} author=[{tmp}] " +
                            $"AnimationLoader version={pVersion} minimum={alVersion}" +
                            $"{bundle}");
                    }
                }
                if (warning)
                {
                    Log.Level(LogLevel.Warning | LogLevel.Debug, lines.ToString());
                }
                else
                {
                    Log.Level(LogLevel.Info | LogLevel.Debug, lines.ToString());
                }
            }
        }

        private static Version BundleVersion()
        {
            var manifests = Sideloader.Sideloader.Manifests.Values
                .Select(x => x.manifestDocument);
            if (manifests != null)
            {
                var manifest = manifests
                    .Select(x => x.Root)
                    .Where(x => x?.Element("guid").Value == "kpluganim.bundle")
                    .FirstOrDefault();

                if (manifest != null)
                {
                    return new Version(manifest?.Element("version").Value);
                }
            }
            return null;
        }
    }
}
