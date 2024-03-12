//
// Save key of used animations
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

using static AnimationLoader.SwapAnim;

namespace AnimationLoader
{
    [DataContract(Name = "AnimationsUsage", Namespace = "https://github.com/IllusionMods/AnimationLoader")]
    public class AnimationsUseStats
    {
        [DataMember]
        public Dictionary<string, int> Stats { set; get; }

        private static readonly string _path = Path.Combine(UserData.Path, "AnimationLoader/Usage");
        private static readonly string _fileName = $"{_path}/AnimationsUsage.xml";
        private static readonly DataContractSerializer _serializer = new(typeof(AnimationsUseStats));
        private static readonly FileInfo _fileInfo = new(_fileName);

        public int this[string key]
        {
            get { return Stats[key]; }
            set { Stats[key] = value; }
        }

        public int Count => Stats.Count;

        public AnimationsUseStats()
        {
            Stats = [];
            Stats.Clear();
        }

        public bool TryGetValue(
            string key,
            out int value)
        {
            return Stats.TryGetValue(key, out value);
        }

        public void Init(List<HSceneProc.AnimationListInfo>[] lstAnimInfo)
        {
            foreach (var c in lstAnimInfo)
            {
                foreach (var a in c)
                {
                    var key = GetAnimationKey(a);
                    if (!Stats.ContainsKey(key))
                    {
                        Stats.Add(key, 0);
                    }
                }
            }
        }

        public override string ToString()
        {
            var animations = new StringBuilder();

            foreach (var e in Stats)
            {
                animations.AppendLine($"Key={e.Key} Value={e.Value}");
            }

            return animations.ToString();
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
                using var fileStream = File.Open(_fileName, FileMode.Open, FileAccess.Read);
                var tmp = _serializer.ReadObject(fileStream) as AnimationsUseStats;
                fileStream.Close();

                Stats = tmp?.Stats;
            }
        }

        public IOrderedEnumerable<KeyValuePair<string, int>> Sorted()
        {
            var sortedByUsage = Stats
                .OrderByDescending(u => u.Value)
                .ThenBy(n => n.Key);

            return sortedByUsage;
        }
    }
}
