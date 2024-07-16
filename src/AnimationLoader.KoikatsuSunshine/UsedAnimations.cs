//
// Save key of used animations
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using BepInEx.Logging;


namespace AnimationLoader
{
    [XmlRoot("Animations")]
    [Serializable]
    public class UsedAnimations
    {
        [XmlElement]
        public HashSet<string> Keys = [];

        private static readonly string _path = Path.Combine(UserData.Path, "save");
        private static readonly string _fileName = $"{_path}/animations.xml";
        private static readonly string _bkFileName = $"{_path}/animations.bk";
        private static readonly XmlSerializer _xmlSerializer = new(typeof(UsedAnimations));
        private static readonly FileInfo _fileInfo = new(_fileName);
        private static readonly FileInfo _bkFileInfo = new(_fileName);

        public override string ToString()
        {
            var sb = new StringBuilder();
            var total = Keys.Count;
            var count = 0;

            foreach (var animation in Keys.OrderBy(x => x))
            {
                count++;
                if (count == total)
                {
                    sb.Append(animation);
                }
                else
                {
                    sb.Append($"{animation}, ");
                }
            }
            return "{ " + sb.ToString() + " }";
        }

        public void Save()
        {
            _fileInfo.Directory.Create();
            StreamWriter writer = new(_fileName);
            _xmlSerializer.Serialize(writer.BaseStream, this);
            writer.Close();
        }

        public void Read()
        {
            if (_fileInfo.Exists)
            {
                try
                {
                    StreamReader reader = new(_fileName);
                    var tmp = (UsedAnimations)_xmlSerializer.Deserialize(reader.BaseStream);
                    reader.Close();
                    // This can be removed later for some reason was using a List instead of
                    // a HashSet Removing duplicates.
                    foreach (var e in tmp.Keys)
                    {
                        Keys.Add(e);
                    }

                    // Make backup at this point the xml.Deserialize should have worked 
                    if (!_bkFileInfo.Exists)
                    {
                        _fileInfo.CopyTo(_bkFileName);
                    }
                    else
                    {
                        // This assumes no manual editing (maybe crc32 someday)
                        if (_bkFileInfo.Length != _fileInfo.Length)
                        {
                            _fileInfo.CopyTo(_bkFileName, true);
                        }
                    }
                }
                catch
                {
                    Log.Level(LogLevel.Error, $"[UsedAnimations.Read] File: {_fileName} corrupt " +
                        "the file will be overwritten on game exit.");

                    if (_bkFileInfo.Exists)
                    {
                        try
                        {
                            Log.Level(LogLevel.Error, $"[UsedAnimations.Read] Trying backup.");
                            StreamReader reader = new(_bkFileName);
                            var tmp = (UsedAnimations)_xmlSerializer.Deserialize(reader.BaseStream);
                            reader.Close();
                            // This can be removed later for some reason was using a List instead of
                            // a HashSet Removing duplicates.
                            foreach (var e in tmp.Keys)
                            {
                                Keys.Add(e);
                            }
                        }
                        catch
                        {
                            Log.Level(LogLevel.Error, $"[UsedAnimations.Read] Can't read backup.");
                        }
                    }
                }
            }
        }
    }
}
