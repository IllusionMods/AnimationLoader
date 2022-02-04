﻿//
// Save key of used animations
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace AnimationLoader
{
    [XmlRoot("Animations")]
    [Serializable]
    public class UsedAnimations
    {
        [XmlElement]
        public List<string> Keys = new();

        private static readonly string _path = Path.Combine(UserData.Path, "save");
        private static readonly string _fileName = $"{_path}/animations.xml";
        private static readonly XmlSerializer _xmlSerializer = new(typeof(UsedAnimations));
        private static readonly FileInfo _fileInfo = new(_fileName);

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
            if (_fileInfo.Exists)
            {
                Log.Debug($"0018: Overwriting file {_fileName}.");
            }
            StreamWriter writer = new(_fileName);
            _xmlSerializer.Serialize(writer.BaseStream, this);
            writer.Close();
        }

        public void Read()
        {
            if (_fileInfo.Exists)
            {
                Log.Debug($"0019: Reading File {_fileName}.");
                StreamReader reader = new(_fileName);
                var tmp = (UsedAnimations)_xmlSerializer.Deserialize(reader.BaseStream);
                reader.Close();
                Keys = tmp.Keys;
            }
        }
    }
}
