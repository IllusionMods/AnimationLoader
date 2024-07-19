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
        private static readonly string _bkFileName = $"{_path}/AnimationsUsage.bk";
        private static readonly DataContractSerializer _serializer = new(typeof(AnimationsUseStats));
        private static readonly FileInfo _fileInfo = new(_fileName);
        private static readonly FileInfo _bkFileInfo = new(_fileName);

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
                try
                {
                    using var fileStream = File.Open(_fileName, FileMode.Open, FileAccess.Read);
                    var tmp = _serializer.ReadObject(fileStream) as AnimationsUseStats;
                    fileStream.Close();

                    Stats = tmp?.Stats;

                    // Make backup at this point data read
                    if (!_bkFileInfo.Exists)
                    {
                        _fileInfo.CopyTo(_bkFileName, true);
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
                    Log.Error($"[AnimationsUseStats.Read] File: {_fileInfo.FullName} corrupt " +
                        "trying to read from backup.");
                    if (_bkFileInfo.Exists) {
                        try
                        {
                            using var fileStream = File.Open(_bkFileName, FileMode.Open, FileAccess.Read);
                            var tmp = _serializer.ReadObject(fileStream) as AnimationsUseStats;
                            fileStream.Close();

                            Stats = tmp?.Stats;
                        }
                        catch
                        {
                            Log.Error($"[AnimationsUseStats.Read] File: Can't read from backup " +
                            "the file will be overwritten on game exit.");
                        }
                    }
                }
            }
            else
            {
                Log.Error($"[AnimationsUseStats.Read] File: {_fileInfo.FullName} does not exits. " +
                    "Trying a backup.");
                if (_bkFileInfo.Exists)
                {
                    try
                    {
                        using var fileStream = File.Open(_bkFileName, FileMode.Open, FileAccess.Read);
                        var tmp = _serializer.ReadObject(fileStream) as AnimationsUseStats;
                        fileStream.Close();

                        Stats = tmp?.Stats;
                    }
                    catch
                    {
                        Log.Error($"[AnimationsUseStats.Read] File: Can't read from backup " +
                        "the file will be overwritten on game exit.");
                    }
                }
                {
                    Log.Error($"[AnimationsUseStats.Read] No badkup found.");
                }
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
