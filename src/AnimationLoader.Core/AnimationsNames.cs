//
// For people who keep there own names for the animations.
//
// Save animations names to UserData/AnimationLoader/Names so they are not overwritten by
// updates. They can be maintained updated there.
// TODO: add names for new animations in updates. OK.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        private static readonly XmlSerializer xmlNamesSerializer = new(typeof(Names));
        private static Dictionary<string, Names> animationNamesDict;

        #region Serializable classes
        [XmlRoot("Names")]
        [Serializable]
        public class Names
        {
            [XmlElement]
            public string guid = string.Empty;

            [XmlElement]
            public string name = string.Empty;

            [XmlElement]
            public string version = string.Empty;

            [XmlElement]
            public string author = string.Empty;

            [XmlElement]
            public string description = string.Empty;

            [XmlElement]
            public string website = string.Empty;

            [XmlArray("AnimationLoader")]
            public List<Animation> Anim = new();
        }

        //
        // A real dictionary was better for programming but less friendly to user interaction
        //
        [XmlRoot("Animation")]
        [Serializable]
        public class Animation
        {
            [XmlElement]
            public int StudioId;
            [XmlElement]
            public string Controller;
            [XmlElement]
            public string Koikatu;
            [XmlElement]
            public string KoikatsuSunshine;
        }
        #endregion

        /// <summary>
        /// Read animations names xml files
        /// </summary>
        private static void LoadNamesXml()
        {
            // Make sure is initialized.
            animationNamesDict = new();
            var path = Path.Combine(UserData.Path, "AnimationLoader/Names");
            if (Directory.Exists(path))
            {
                var docs = Directory.GetFiles(path, "*.xml").Select(XDocument.Load).ToList();
                if (docs.Count > 0)
                {
                    Log.Debug($"0001: [{PInfo.PluginName}] Loading animations names.");
                    LoadNamesXmls(docs);
                    return;
                }
            }
            Log.Debug("0002: No names found.");
        }

        /// <summary>
        /// Add the names on the xml files to names dictionary
        /// </summary>
        /// <param name="namesDocs"></param>
        private static void LoadNamesXmls(IEnumerable<XDocument> namesDocs)
        {
            foreach (var animElem in namesDocs.Select(x => x.Root))
            {
                try
                {
                    var reader = animElem.CreateReader();
                    var names = (Names)xmlNamesSerializer.Deserialize(reader);
                    reader.Close();
                    animationNamesDict.Add(names.guid, names);
                }
                catch (Exception ex)
                {
                    Log.Error($"0003: Error trying to read names file. {ex}");
                }
            }
        }

        /// <summary>
        /// Setup new names for guid
        /// </summary>
        /// <param name="manifest"></param>
        private static void NamesAddGuid(XElement manifest)
        {
            // new names
            var names = new Names();
            // initialize fields with manifest date
            names.guid = manifest.Element(nameof(names.guid)).Value;
            names.name = manifest.Element(nameof(names.name)).Value;
            names.version = manifest.Element(nameof(names.version)).Value;
            names.author = manifest.Element(nameof(names.author)).Value;
            names.description = manifest.Element(nameof(names.description)).Value;
            names.website = manifest.Element(nameof(names.website)).Value;
            // add new names to dictionary
            animationNamesDict.Add(names.guid, names);
        }

        /// <summary>
        /// Save all the names in the names dictionary to xml files
        /// </summary>
        private static void SaveNamesXmls()
        {
            foreach (var guid in animationNamesDict.Keys)
            {
                var animNames = animationNamesDict[guid];
                SaveNames(animNames, guid);
            }
        }

        /// <summary>
        /// Save names to xml files. One xml file is saved per guid in dictionary.
        /// </summary>
        /// <param name="names"></param>
        /// <param name="guid"></param>
        private static void SaveNames(Names names, string guid, bool overwrite = false)
        {
            var path = Path.Combine(UserData.Path, "AnimationLoader/Names");
            var fileName = $"{path}/names-{guid}.xml";
            FileInfo file = new(fileName);

            Log.Debug($"0004: Saving names file {fileName}.");

            // Create directory where to save the file.
            // Safe to do even if directory exists.
            file.Directory.Create();
            if (file.Exists && !overwrite)
            {
                Log.Debug($"0005: File {fileName} already exits not overwritten.");
                return;
            }

            XmlSerializer xmlSerializer = new(typeof(Names));
            var writer = new StreamWriter($"{path}/names-{guid}.xml");
            xmlSerializer.Serialize(writer.BaseStream, names);
            writer.Close();
        }

        /// <summary>
        /// Read the names from an xml file
        /// </summary>
        /// <param name="names"></param>
        /// <param name="guid"></param>
        private static void ReadNames(ref Names names, string guid)
        {
            var path = Path.Combine(UserData.Path, "AnimationLoader/Names");
            var fileName = $"{path}/names-{guid}.xml";
            FileInfo file = new(fileName);

            if (!file.Exists)
            {
                Log.Debug($"0006: Names file {fileName} not found. Safe to ignore.");
                return;
            }

            XmlSerializer xmlSerializer = new(typeof(Names));
            StreamReader reader = new($"{path}/names-{guid}.xml");
            names = (Names)xmlSerializer.Deserialize(reader);
            reader.Close();
        }
    }
}
