//
// Load XML animation information
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

using BepInEx;
using BepInEx.Logging;
using KKAPI;

using UnityEngine;
using static HFlag;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        private const string ManifestRootElement = "AnimationLoader";
        private const string ManifestArrayItem = "Animation";
        private const string ManifestGSArrayItem = KoikatuAPI.GameProcessName;

        private static readonly XmlSerializer xmlSerializer = new(typeof(SwapAnimationInfo));
        // TODO: Read game overrides when running in KK
#if KKS
        private const string ManifestOverride = "GameSpecificOverrides";
        private static readonly XmlSerializer xmlOverrideSerializer = new(typeof(OverrideInfo));
#endif
        private static XElement _animRoot;
        private static XElement _animRootGS;

        private static bool _saveNames = false;

        private static void LoadTestXml()
        {
            var path = Path.Combine(Paths.ConfigPath, "AnimationLoader");
            if(Directory.Exists(path))
            {
                var docs = Directory.GetFiles(path, "*.xml").Select(XDocument.Load).ToList();
                if(docs.Count > 0)
                {
                    Log.Level(LogLevel.Message, $"0014: [{PluginName}] Loading test " +
                        $"animations");
                    LoadXmls(docs);
                    return;
                }
            }
            Log.Level(LogLevel.Message, "0015: Make a manifest format .xml in the " +
                "config/AnimationLoader folder to test animations");
        }

        private static void LoadXmls(IEnumerable<XDocument> manifests)
        {
            animationDict = new Dictionary<EMode, List<SwapAnimationInfo>>();
            var count = 0;
            var overrideNames = false;
            var logLines = new StringBuilder();

            // Select the only manifests that AnimationLoader will process
            foreach (var manifest in manifests
                .Select(x => x.Root)
                .Where(x => x?.Element(ManifestRootElement) != null))
            {
                _animRoot = manifest?.Element(ManifestRootElement);

                // Look for game specific configuration
                _animRootGS = manifest?
                    .Element(ManifestRootElement)?
                    .Element(KoikatuAPI.GameProcessName);

                if ((_animRoot is null) && (_animRootGS is null))
                {
                    continue;
                }
                var guid = manifest?.Element("guid").Value;
                var version = manifest?.Element("version").Value;

                if (UserOverrides.Value)
                {
                    // setup for names
                    overrideNames = true;
                    _saveNames = false;
                    if (!animationNamesDict.ContainsKey(guid))
                    {
                        NamesAddGuid(manifest);
                        overrideNames = false;
                    }
                    else
                    {
                        // There are names to override the ones in the manifest
                        overrideNames = true;
                    }
                }

                // Process elements valid for any game
                count += ProcessArray(_animRoot, guid, version, overrideNames, ref logLines);
                if (_animRootGS is null)
                {
                    if (_saveNames)
                    {
                        SaveNames(animationNamesDict[guid], guid, true);
                    }
                    continue;
                }

                // Process game specific animations
                var _animSpecific = _animRoot?.Elements(ManifestGSArrayItem);

                if (_animSpecific is not null)
                {
                    foreach (var gameSpecificElement in _animSpecific)
                    {
                        if (gameSpecificElement is null)
                        {
                            continue;
                        }
                        count += ProcessArray(
                            gameSpecificElement,
                            guid,
                            version,
                            overrideNames,
                            ref logLines);
                    }
                }

                if (_saveNames)
                {
                    SaveNames(animationNamesDict[guid], guid, true);
                }
            }
            if (count > 0)
            {
#if KKS
                Utilities.AlDicExpAddTaii();
#endif
                logLines.Append($"\n{count} animations processed from manifests.\n");
#if DEBUG
                Log.Info($"0016: Animations loaded:\n\n{logLines}");
#else
                Log.Debug($"0016: Animations loaded:\n\n{logLines}");
#endif
            }
            else
            {
                Log.Level(LogLevel.Message, "0017: No animation manifests found.");
                Log.Level(LogLevel.Warning, "0017: No animation manifests found.");
            }
        }

        private static int ProcessArray(
            XElement root, 
            string guid, 
            string version,
            bool overrideNames,
            ref StringBuilder logLines)
        {
            var count = 0;

            if (Sideloader.Sideloader.ZipArchives.TryGetValue(guid, out var zipFileName))
            {
                logLines.Append($"From {zipFileName} {guid}-{version}: {RootText(root.Name)}\n");
            }
            else
            {
                logLines.Append($"From {guid}-{version}: {RootText(root.Name)}\n");
            }
            foreach (var animElem in root.Elements(ManifestArrayItem))
            {
                if (animElem == null)
                {
                    continue;
                }
                var animation = new Animation();
                var reader = animElem.CreateReader();
                var overrideName = overrideNames;
                var data = (SwapAnimationInfo)xmlSerializer.Deserialize(reader);
                data.Guid = guid;
                reader.Close();

                if (!data.SpecificFor.IsNullOrWhiteSpace())
                {
                    if (!data.SpecificFor.Equals(KoikatuAPI.GameProcessName))
                    {
                        continue;
                    }
                }

                if (UserOverrides.Value)
                {
                    if (overrideName)
                    {
                        // user override name
                        // Temp to continue testing
                        animation = animationNamesDict[guid].Anim
                            .Where(x => (x.StudioId == data.StudioId) &&
                                        (x.Controller == data.ControllerFemale)).FirstOrDefault();
                        if (animation == null)
                        {
                            overrideName = false;
                            animation = new Animation();
                        }
                        else
                        {
#if KKS
                            data.AnimationName = animation.KoikatsuSunshine;
#endif
#if KK
                            data.AnimationName = animation.Koikatu;
#endif
                        }
                    }

                    if (!overrideName)
                    {
                        // new name
                        animation.StudioId = data.StudioId;
                        animation.Controller = string.Copy(data.ControllerFemale);
                        animation.Koikatu = string.Copy(data.AnimationName);
                        animation.KoikatuReference = string.Copy(data.AnimationName);
                        animation.KoikatsuSunshine = string.Copy(data.AnimationName);
                        animation.KoikatsuSunshineReference = string.Copy(data.AnimationName);
                    }
                }
#if KKS
                // Assuming configuration is for KK like originally is and the overrides are for
                // KKS only no the other way around.
                // TODO: Changing it so it can be the other way around also.
                var overrideRoot = animElem?
                    .Element(ManifestOverride)?
                    .Element(KoikatuAPI.GameProcessName);
                if (overrideRoot != null)
                {
                    var overrideReader = overrideRoot.CreateReader();
                    var overrideData = (OverrideInfo)xmlOverrideSerializer
                        .Deserialize(overrideReader);
                    overrideReader.Close();
                    DoOverrides(ref data, overrideData, ref animation, overrideName);
                }
#endif
                if (!animationDict.TryGetValue(data.Mode, out var list))
                {
                    animationDict[data.Mode] = list = new List<SwapAnimationInfo>();
                }
                list.Add(data);
                logLines.Append($"{AnimationInfo.GetKey(data),-30} - " +
                    $"{Utilities.Translate(data.AnimationName)}\n");
                count++;
                if (UserOverrides.Value)
                {
                    // Save names no names were read
                    // TODO: re-save with new animations names
                    if (!overrideName)
                    {
                        animationNamesDict[guid].Anim.Add(animation);
                        if (!_saveNames)
                        {
                            _saveNames = true;
                        }
                    }
                }
            }
            logLines.Append('\n');
            return count;
        }

        internal static Func<XName, string> RootText = x => x == KoikatuAPI.GameProcessName ?
            $"Game specific elements of {x}" : $"Root elements of {x}";

#if KKS
        private static void DoOverrides(
            ref SwapAnimationInfo data, 
            OverrideInfo overrides,
            ref Animation animation,
            bool overrideName)
        {
            if (overrides.PathFemale != null)
            {
                data.PathFemale = string.Copy(overrides.PathFemale);
            }
            if (overrides.ControllerFemale != null)
            {
                data.ControllerFemale = string.Copy(overrides.ControllerFemale);
            }
            if (overrides.PathMale != null)
            {
                data.PathMale = string.Copy(overrides.PathMale);
            }
            if (overrides.ControllerMale != null)
            {
                data.ControllerMale = string.Copy(overrides.ControllerMale);
            }
            if ((overrides.AnimationName != null) && !overrideName)
            {
                data.AnimationName = string.Copy(overrides.AnimationName);
                if (UserOverrides.Value)
                {
                    animation.KoikatsuSunshine = string.Copy(overrides.AnimationName);
                    animation.KoikatsuSunshineReference = string.Copy(overrides.AnimationName);
                }
            }
            if (overrides.Mode >= 0)
            {
                data.Mode = overrides.Mode;
            }
            if (overrides.kindHoushi >= 0)
            {
                data.kindHoushi = overrides.kindHoushi;
            }
            if (overrides.categories != null)
            {
                overrides.categories.CopyTo(data.categories, 0);
            }
            if (overrides.DonorPoseId >= 0)
            {
                data.DonorPoseId = overrides.DonorPoseId;
            }
            if (overrides.NeckDonorId >= 0)
            {
                data.NeckDonorId = overrides.NeckDonorId;
            }
            if (overrides.FileMotionNeck != null)
            {
                data.FileMotionNeck = string.Copy(overrides.FileMotionNeck);
            }
            if (overrides.IsFemaleInitiative != null)
            {
                data.IsFemaleInitiative = overrides.IsFemaleInitiative;
            }
            if (overrides.FileSiruPaste != null)
            {
                data.FileSiruPaste = string.Copy(overrides.FileSiruPaste);
            }
            if (overrides.MotionIKDonor >= 0)
            {
                data.MotionIKDonor = overrides.MotionIKDonor;
            }
            if (overrides.ExpTaii >= 0) 
            { 
                data.ExpTaii = overrides.ExpTaii; 
            }
            if (overrides.IsAnal != null)
            {
                data.IsAnal = overrides.IsAnal;
            }
            if (overrides.PositionHeroine != Vector3.zero)
            {
                data.PositionHeroine = overrides.PositionHeroine;
            }
            if (overrides.PositionPlayer != Vector3.zero)
            {
                data.PositionPlayer = overrides.PositionPlayer;
            }
        }
#endif
    }
}
