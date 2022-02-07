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
    //[CollectionDataContract(Name = "Clips", ItemName = "clip")]
    [DataContract(Name = "AnimationLoader", Namespace = "https://github.com/IllusionMods/AnimationLoader")]
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

    [DataContract]
    public class AnimationClips2
    {
        [DataMember]
        public Dictionary<string, string> Clips { set; get; }

        private static readonly string _path = Path.Combine(Paths.ConfigPath, "AnimationLoader");
        private static readonly string _fileName = $"{_path}/animationClips.xml";
        private static readonly DataContractSerializer _serializer = new(typeof(AnimationClips2));
        private static readonly FileInfo _fileInfo = new(_fileName);

        public AnimationClips2()
        {
            Clips = new Dictionary<string, string>();
            Clips.Clear();
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
                var tmp = _serializer.ReadObject(reader) as AnimationClips2;
                reader.Close();

                this.Clips = tmp.Clips;
            }
        }
    }

    [DataContract]
    public class AnimationClipsByType
    {
        [DataMember]
        public Dictionary<string, List<string>> Clips { set; get; }

        private static readonly string _path = Path.Combine(Paths.ConfigPath, "AnimationLoader");
        private static readonly string _fileName = $"{_path}/animationClipsByType.xml";
        private static readonly DataContractSerializer _serializer = new(typeof(AnimationClipsByType));
        private static readonly FileInfo _fileInfo = new(_fileName);

        public AnimationClipsByType()
        {
            Clips = new Dictionary<string, List<string>>();
            Clips.Clear();
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
                    var reader = new FileStream(_fileName, FileMode.Open, FileAccess.Read);
                    var tmp = _serializer.ReadObject(reader) as AnimationClipsByType;
                    reader.Close();

                    this.Clips = tmp.Clips;
                }
                catch (Exception ex)
                {
                    Log.Error($"Error ByType reading {ex}");
                }
            }
        }
    }

    public static class SClips
    {
        public static readonly Dictionary<string, List<string>> Clips = new() {
            { "houshi-Hand", new List<string> {
                "L_Idle",
                "L_OLoop",
                "L_OUT_A",
                "L_OUT_Loop",
                "L_OUT_Start",
                "L_SLoop1",
                "L_SLoop2",
                "L_Stop_Idle",
                "L_WLoop1",
                "L_WLoop2",
                "M_Idle",
                "M_OLoop",
                "M_OUT_A",
                "M_OUT_Loop",
                "M_OUT_Start",
                "M_SLoop1",
                "M_SLoop2",
                "M_Stop_Idle",
                "M_WLoop1",
                "M_WLoop2",
                "S_Idle",
                "S_OLoop",
                "S_OUT_A",
                "S_OUT_Loop",
                "S_OUT_Start",
                "S_SLoop1",
                "S_SLoop2",
                "S_Stop_Idle",
                "S_WLoop1",
                "S_WLoop2" }
            },
            { "houshi-Mouth", new List<string> {
                "L_Drink",
                "L_Drink_A",
                "L_Drink_IN",
                "L_Idle",
                "L_IN_Loop",
                "L_IN_Start",
                "L_OLoop",
                "L_Oral_Idle",
                "L_Oral_Idle_IN",
                "L_OUT_A",
                "L_OUT_Loop",
                "L_OUT_Start",
                "L_SLoop1",
                "L_SLoop2",
                "L_Stop_Idle",
                "L_Vomit",
                "L_Vomit_A",
                "L_Vomit_IN",
                "L_WLoop1",
                "L_WLoop2",
                "M_Drink",
                "M_Drink_A",
                "M_Drink_IN",
                "M_Idle",
                "M_IN_Loop",
                "M_IN_Start",
                "M_OLoop",
                "M_Oral_Idle",
                "M_Oral_Idle_IN",
                "M_OUT_A",
                "M_OUT_Loop",
                "M_OUT_Start",
                "M_SLoop1",
                "M_SLoop2",
                "M_Stop_Idle",
                "M_Vomit",
                "M_Vomit_A",
                "M_Vomit_IN",
                "M_WLoop1",
                "M_WLoop2",
                "S_Drink",
                "S_Drink_A",
                "S_Drink_IN",
                "S_Idle",
                "S_IN_Loop",
                "S_IN_Start",
                "S_OLoop",
                "S_Oral_Idle",
                "S_Oral_Idle_IN",
                "S_OUT_A",
                "S_OUT_Loop",
                "S_OUT_Start",
                "S_SLoop1",
                "S_SLoop2",
                "S_Stop_Idle",
                "S_Vomit",
                "S_Vomit_A",
                "S_Vomit_IN",
                "S_WLoop1",
                "S_WLoop2" }
            },
            { "houshi-Breasts", new List<string> {
                "L_Idle",
                "L_OLoop",
                "L_OUT_A",
                "L_OUT_Loop",
                "L_OUT_Start",
                "L_SLoop1",
                "L_SLoop2",
                "L_Stop_Idle",
                "L_WLoop1",
                "L_WLoop2",
                "M_Idle",
                "M_OLoop",
                "M_OUT_A",
                "M_OUT_Loop",
                "M_OUT_Start",
                "M_SLoop1",
                "M_SLoop2",
                "M_Stop_Idle",
                "M_WLoop1",
                "M_WLoop2",
                "S_Idle",
                "S_OLoop",
                "S_OUT_A",
                "S_OUT_Loop",
                "S_OUT_Start",
                "S_SLoop1",
                "S_SLoop2",
                "S_Stop_Idle",
                "S_WLoop1",
                "S_WLoop2" }
            },
            { "sonyu", new List<string> {
                "L_Drop",
                "L_Idle",
                "L_IN_A",
                "L_Insert",
                "L_InsertIdle",
                "L_M_IN_Loop",
                "L_M_IN_Start",
                "L_M_OUT_Loop",
                "L_M_OUT_Start",
                "L_OLoop",
                "L_OUT_A",
                "L_Pull",
                "L_SF_IN_Loop",
                "L_SF_IN_Start",
                "L_SLoop1",
                "L_SLoop2",
                "L_SS_IN_A",
                "L_SS_IN_Loop",
                "L_SS_IN_Start",
                "L_WF_IN_Loop",
                "L_WF_IN_Start",
                "L_WLoop1",
                "L_WLoop2",
                "L_WS_IN_A",
                "L_WS_IN_Loop",
                "L_WS_IN_Start",
                "M_Drop",
                "M_Idle",
                "M_IN_A",
                "M_Insert",
                "M_InsertIdle",
                "M_M_IN_Loop",
                "M_M_IN_Start",
                "M_M_OUT_Loop",
                "M_M_OUT_Start",
                "M_OLoop",
                "M_OUT_A",
                "M_Pull",
                "M_SF_IN_Loop",
                "M_SF_IN_Start",
                "M_SLoop1",
                "M_SLoop2",
                "M_SS_IN_A",
                "M_SS_IN_Loop",
                "M_SS_IN_Start",
                "M_WF_IN_Loop",
                "M_WF_IN_Start",
                "M_WLoop1",
                "M_WLoop2",
                "M_WS_IN_A",
                "M_WS_IN_Loop",
                "M_WS_IN_Start",
                "S_Drop",
                "S_Idle",
                "S_IN_A",
                "S_Insert",
                "S_InsertIdle",
                "S_M_IN_Loop",
                "S_M_IN_Start",
                "S_M_OUT_Loop",
                "S_M_OUT_Start",
                "S_OLoop",
                "S_OUT_A",
                "S_Pull",
                "S_SF_IN_Loop",
                "S_SF_IN_Start",
                "S_SLoop1",
                "S_SLoop2",
                "S_SS_IN_A",
                "S_SS_IN_Loop",
                "S_SS_IN_Start",
                "S_WF_IN_Loop",
                "S_WF_IN_Start",
                "S_WLoop1",
                "S_WLoop2",
                "S_WS_IN_A",
                "S_WS_IN_Loop",
                "S_WS_IN_Start",
                "L_A_Drop",
                "L_A_Idle",
                "L_A_IN_A",
                "L_A_Insert",
                "L_A_InsertIdle",
                "L_A_M_IN_Loop",
                "L_A_M_IN_Start",
                "L_A_M_OUT_A",
                "L_A_M_OUT_Loop",
                "L_A_M_OUT_Start",
                "L_A_OLoop",
                "L_A_Pull",
                "L_A_SF_IN_Loop",
                "L_A_SF_IN_Start",
                "L_A_SLoop1",
                "L_A_SLoop2",
                "L_A_SS_IN_A",
                "L_A_SS_IN_Loop",
                "L_A_SS_IN_Start",
                "L_A_WF_IN_Loop",
                "L_A_WF_IN_Start",
                "L_A_WLoop1",
                "L_A_WLoop2",
                "L_A_WS_IN_A",
                "L_A_WS_IN_Loop",
                "L_A_WS_IN_Start",
                "M_A_Drop",
                "M_A_Idle",
                "M_A_IN_A",
                "M_A_Insert",
                "M_A_InsertIdle",
                "M_A_M_IN_Loop",
                "M_A_M_IN_Start",
                "M_A_M_OUT_A",
                "M_A_M_OUT_Loop",
                "M_A_M_OUT_Start",
                "M_A_OLoop",
                "M_A_Pull",
                "M_A_SF_IN_Loop",
                "M_A_SF_IN_Start",
                "M_A_SLoop1",
                "M_A_SLoop2",
                "M_A_SS_IN_A",
                "M_A_SS_IN_Loop",
                "M_A_SS_IN_Start",
                "M_A_WF_IN_Loop",
                "M_A_WF_IN_Start",
                "M_A_WLoop1",
                "M_A_WLoop2",
                "M_A_WS_IN_A",
                "M_A_WS_IN_Loop",
                "M_A_WS_IN_Start",
                "S_A_Drop",
                "S_A_Idle",
                "S_A_IN_A",
                "S_A_Insert",
                "S_A_InsertIdle",
                "S_A_M_IN_Loop",
                "S_A_M_IN_Start",
                "S_A_M_OUT_A",
                "S_A_M_OUT_Loop",
                "S_A_M_OUT_Start",
                "S_A_OLoop",
                "S_A_Pull",
                "S_A_SF_IN_Loop",
                "S_A_SF_IN_Start",
                "S_A_SLoop1",
                "S_A_SLoop2",
                "S_A_SS_IN_A",
                "S_A_SS_IN_Loop",
                "S_A_SS_IN_Start",
                "S_A_WF_IN_Loop",
                "S_A_WF_IN_Start",
                "S_A_WLoop1",
                "S_A_WLoop2",
                "S_A_WS_IN_A",
                "S_A_WS_IN_Loop",
                "S_A_WS_IN_Start" }
            }
        };
    }
}
