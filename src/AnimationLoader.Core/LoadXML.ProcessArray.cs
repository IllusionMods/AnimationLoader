﻿//
// Load XML animation information
//
using System;
using System.Collections.Generic;
//using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
//using System.Xml.Serialization;

//using BepInEx;
//using BepInEx.Logging;

using KKAPI;

//using UnityEngine;

//using static HFlag;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
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
                logLines.Append($"From {zipFileName} {guid}-{version}: " +
                    $"{RootText(root.Name)}\n");
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
                                        (x.Controller == data.ControllerFemale))
                            .FirstOrDefault();
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
                        animation.KoikatsuSunshineReference = string
                            .Copy(data.AnimationName);
                    }
                }
#if KKS
                // Assuming configuration is for KK like originally is and the
                // overrides are for KKS only no the other way around.
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
                logLines.Append($"{GetAnimationKey(data),-30} - " +
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

        internal static Func<XName, string>
            RootText = x => x == KoikatuAPI.GameProcessName ?
            $"Game specific elements of {x}" : $"Root elements of {x}";
    }
}