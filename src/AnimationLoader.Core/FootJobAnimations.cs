//
// FootJobAnimations.cs
//

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

using BepInEx;

namespace AnimationLoader
{
    public partial class SwapAnim
    {
        internal static FootJobAnimations _footJobAnimations = [];

        [DataContract(Name = "FootJobAnimations", Namespace = "https://github.com/IllusionMods/AnimationLoader")]
        public class FootJobAnimations : IEnumerable
        {
            [DataMember]
            private HashSet<string> Animations;

            private static readonly string _path = Path.Combine(Paths.ConfigPath, "AnimationLoader/FootJob");
            private static readonly string _fileName = $"{_path}/FootJobAnimations.xml";
            private static readonly DataContractSerializer _serializer = new(typeof(FootJobAnimations));
            private static readonly FileInfo _fileInfo = new(_fileName);

            public int Count => Animations.Count;

            public FootJobAnimations()
            {
                Animations = [];
                Animations.Clear();
            }

            public IEnumerator GetEnumerator()
            {
                return Animations.GetEnumerator();
            }

            public void Add(string item)
            {
                Animations.Add(item);
            }

            public void Clear()
            {
                Animations.Clear();
            }

            public bool Contains(string item)
            {
                return Animations.Contains(item);
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
                    using var fileStream = System.IO.File.Open(_fileName, FileMode.Open, FileAccess.Read);
                    var tmp = _serializer.ReadObject(fileStream) as FootJobAnimations;
                    fileStream.Close();

                    Animations = tmp?.Animations;
                }
                else
                {
                    Log.Error("[FootJobAnimations.Read] File not found.");
                }
#if DEBUG
                var logLines = new StringBuilder();

                foreach (var fj in _footJobAnimations)
                {
                    logLines.AppendLine(fj.ToString());
                }
                Log.Debug($"0012: Foot jobs.\n{logLines}");
#endif
            }
        }
    }
}
