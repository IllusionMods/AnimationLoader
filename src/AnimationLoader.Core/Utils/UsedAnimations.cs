//
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
        public HashSet<string> Keys = [];

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
            StreamWriter writer = new(_fileName);
            _xmlSerializer.Serialize(writer.BaseStream, this);
            writer.Close();
        }

        public void Read()
        {
            if (_fileInfo.Exists)
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
            }
        }
    }
}
