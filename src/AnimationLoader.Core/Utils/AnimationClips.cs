//
// Save key of used animations
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

using BepInEx;

using Newtonsoft.Json;

namespace AnimationLoader
{
    [DataContract]
    public class AnimationClips
    {
        [DataMember]
        public Dictionary<string, List<string>> Clips { set; get; }

        private static readonly string _path = Path.Combine(Paths.ConfigPath, "AnimationLoader");
        private static readonly string _fileName = $"{_path}/animationClips.cache";
        private static readonly string _fileNameJson = $"{_path}/animationClips.json";
        private static readonly DataContractSerializer _serializer = new(typeof(AnimationClips));
        private static readonly FileInfo _fileInfo = new(_fileName);

        public AnimationClips()
        {
            Clips = new Dictionary<string,List<string>>();
            Clips.Clear();
        }

        public void SaveNJson()
        {
            Log.Warning($"Calling Save {_fileName}.");
            _fileInfo.Directory.Create();

            using var file = File.CreateText(_fileNameJson);
            var serializer = new JsonSerializer();
            serializer.Serialize(file, Clips);
        }

        public void ReadNJson()
        {
            if (_fileInfo.Exists)
            {
                using var file = File.OpenText(_fileNameJson);
                var serializer = new JsonSerializer();
                var clipsJson = serializer.Deserialize(file, typeof(Dictionary<string, List<string>>));
                this.Clips = (Dictionary<string, List<string>>)clipsJson;
            }
        }

        public void Save()
        {
            _fileInfo.Directory.Create();
            var writer = new FileStream(_fileName, FileMode.Create, FileAccess.Write);
            _serializer.WriteObject(writer, this);
            writer.Close();
        }

        public void Read()
        {
            if (_fileInfo.Exists)
            {
                var reader = new FileStream(_fileName, FileMode.Open, FileAccess.Read);
                var tmp = _serializer.ReadObject(reader) as AnimationClips;
                reader.Close();

                this.Clips = tmp.Clips;
            }
        }
    }
}
