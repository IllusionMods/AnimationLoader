//
// MotionIK
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using UnityEngine;

using H;
using Illusion.Extensions;

using BepInEx.Logging;
using HarmonyLib;

using Newtonsoft.Json;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        public class MotionIKDataSerializable
        {
            public static class Type
            {
                public static System.Type Get(string dllName, string typeName)
                {
                    return Assembly.Load(dllName).GetType(typeName);
                }

                public static System.Type Get(string typeName)
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    for (var i = 0; i < assemblies.Length; i++)
                    {
                        var types = assemblies[i].GetTypes();
                        foreach (var type in types)
                        {
                            if (type.Name == typeName)
                            {
                                return type;
                            }
                        }
                    }
                    return null;
                }

                public static System.Type GetFull(string typeFullName)
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    for (var i = 0; i < assemblies.Length; i++)
                    {
                        var types = assemblies[i].GetTypes();
                        foreach (var type in types)
                        {
                            if (type.FullName == typeFullName)
                            {
                                return type;
                            }
                        }
                    }
                    return null;
                }

                public static string GetAssemblyQualifiedName(string typeName)
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    for (var i = 0; i < assemblies.Length; i++)
                    {
                        var types = assemblies[i].GetTypes();
                        foreach (var type in types)
                        {
                            if (type.Name == typeName)
                            {
                                return type.AssemblyQualifiedName;
                            }
                        }
                    }
                    return null;
                }

                public static string GetFullAssemblyQualifiedName(string typeFullName)
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    for (var i = 0; i < assemblies.Length; i++)
                    {
                        var types = assemblies[i].GetTypes();
                        foreach (var type in types)
                        {
                            if (type.FullName == typeFullName)
                            {
                                return type.AssemblyQualifiedName;
                            }
                        }
                    }
                    return null;
                }

                public static FieldInfo[] GetPublicFields(System.Type type)
                {
                    return type.GetFields(BindingFlags.Instance | BindingFlags.Public);
                }

                public static PropertyInfo[] GetPublicProperties(System.Type type)
                {
                    return type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                }
            }
            /// <summary>
            /// Structure to use instead of Unity Vector3 so the class can be
            /// serialized to json
            /// </summary>
            public struct Vector3
            {
                public float x, y, z;

                public Vector3(float x, float y, float z)
                {
                    this.x = x;
                    this.y = y;
                    this.z = z;
                }

                public float this[int index]
                {
                    get
                    {
                        return index switch {
                            0 => x,
                            1 => y,
                            2 => z,
                            _ => throw new IndexOutOfRangeException("Invalid Vector3 index!"),
                        };
                    }
                    set
                    {
                        switch (index)
                        {
                            case 0:
                                x = value;
                                break;
                            case 1:
                                y = value;
                                break;
                            case 2:
                                z = value;
                                break;
                            default:
                                throw new IndexOutOfRangeException("Invalid Vector3 index!");
                        }
                    }
                }
            }

            private Stream SourceStream { get; set; }
            private State[] _states;

            #region Properties from MotionIKData
            public string Name { get; set; }

            public class State
            {
                public string name = "";

                public Parts leftHand = new();

                public Parts rightHand = new();

                public Parts leftFoot = new();

                public Parts rightFoot = new();

                public Frame[] frames;

                public Parts this[int index] => PartsArray[index];

                public Parts[] PartsArray => new Parts[4] {
                    leftHand, rightHand, leftFoot, rightFoot };

                public MotionIKData.State ToState()
                {
                    var result = new MotionIKData.State {
                        name = name
                    };

                    var totalParts = result.parts.Length;
                    for (var j = 0; j < totalParts; j++)
                    {
                        var obj = result.parts[j];
                        var parts = PartsArray[j];
                        obj.param2.sex = parts.param2.sex;
                        obj.param2.target = parts.param2.target;
                        obj.param2.weightPos = parts.param2.weightPos;
                        obj.param2.weightAng = parts.param2.weightAng;
                        obj.param3.chein = parts.param3.chein;
                        obj.param3.weight = parts.param3.weight;
                    }

                    var totalFrames = frames.Length;
                    result.frames = new MotionIKData.Frame[totalFrames];
                    for (var k = 0; k < totalFrames; k++)
                    {
                        var frame = new MotionIKData.Frame();
                        var frameSource = frames[k];
                        frame.frameNo = frameSource.frameNo;
                        frame.editNo = frameSource.editNo;
                        var num4 = frameSource.shapes.Length;
                        frame.shapes = new MotionIKData.Shape[num4];
                        for (var l = 0; l < num4; l++)
                        {
                            var shape = new MotionIKData.Shape();
                            var shapeSource = frameSource.shapes[l];
                            shape.shapeNo = shapeSource.shapeNo;
                            for (var m = 0; m < 2; m++)
                            {
                                shape[m] = new MotionIKData.PosAng();
                                for (var n = 0; n < 3; n++)
                                {
                                    shape[m].pos[n] = shapeSource[m].pos[n];
                                }
                                for (var n = 0; n < 3; n++)
                                {
                                    shape[m].ang[n] = shapeSource[m].ang[n];
                                }
                            }
                            frame.shapes[l] = shape;
                        }
                        result.frames[k] = frame;
                    }
                    return result;
                }
            }

            public class Parts
            {
                public Target param2 = new();

                public Chain param3 = new();
            }

            public class Target
            {
                public int sex;

                public string target = "";

                public float weightPos;

                public float weightAng;

                public static int Length => Type.GetPublicFields(typeof(Target)).Length;

                public object this[int index]
                {
                    get
                    {
                        return index switch {
                            0 => sex,
                            1 => target,
                            2 => weightPos,
                            3 => weightAng,
                            _ => null,
                        };
                    }
                    set
                    {
                        switch (index)
                        {
                            case 0:
                                sex = (value is string strValue0) ? int.Parse(strValue0) : ((int)value);
                                break;
                            case 1:
                                target = (string)value;
                                break;
                            case 2:
                                weightPos = (value is string strValue2) ? float.Parse(strValue2) : ((float)value);
                                break;
                            case 3:
                                weightAng = (value is string strValue3) ? float.Parse(strValue3) : ((float)value);
                                break;
                        }
                    }
                }
            }

            public class Chain
            {
                public string chein = "";

                public float weight;

                public static int Length => Type.GetPublicFields(typeof(Chain)).Length;

                public object this[int index]
                {
                    get
                    {
                        return index switch {
                            0 => chein,
                            1 => weight,
                            _ => null,
                        };
                    }
                    set
                    {
                        switch (index)
                        {
                            case 0:
                                chein = (string)value;
                                break;
                            case 1:
                                weight = (value is string strValue1) ? float.Parse(strValue1) : ((float)value);
                                break;
                        }
                    }
                }
            }

            public class Frame
            {
                public int frameNo = -1;

                public int editNo;

                public Shape[] shapes;
            }

            public class Shape
            {
                public int shapeNo = -1;

                public PosAng small;

                public PosAng large;

                public PosAng this[int index]
                {
                    get
                    {
                        return index switch {
                            0 => small,
                            1 => large,
                            _ => null,
                        };
                    }
                    set
                    {
                        switch (index)
                        {
                            case 0:
                                small = value;
                                break;
                            case 1:
                                large = value;
                                break;
                        }
                    }
                }
            }

            public class PosAng
            {
                public Vector3 pos;

                public Vector3 ang;

                public Vector3 this[int index]
                {
                    get
                    {
                        return index switch {
                            0 => pos,
                            1 => ang,
                            _ => new Vector3(0, 0, 0),
                        };
                    }
                    set
                    {
                        switch (index)
                        {
                            case 0:
                                pos = value;
                                break;
                            case 1:
                                ang = value;
                                break;
                        }
                    }
                }

                public float[] PosArray => new float[3] { pos.x, pos.y, pos.z };

                public float[] AngArray => new float[3] { ang.x, ang.y, ang.z };
            }

            public State[] States  // original states
            {
                get
                {
                    return _states;
                }
                set
                {
                    _states = value;
                }
            }
            #endregion

            public MotionIKDataSerializable()
            {
            }

            public MotionIKDataSerializable(string file)
            {
                var textAsset =
                    GlobalMethod.LoadAllFolderInOneFile<UnityEngine.TextAsset>(
                        "h/list/", file);
                TextAssetSBUHelper(textAsset);
            }

            public MotionIKDataSerializable(UnityEngine.TextAsset textAsset)
            {
                TextAssetSBUHelper(textAsset);
            }

            private void TextAssetSBUHelper(UnityEngine.TextAsset textAsset)
            {
                SourceStream = new MemoryStream(textAsset.bytes);
                Name = textAsset.name;
                Read(SourceStream);
            }

            public void Read(UnityEngine.TextAsset textAsset)
            {
                TextAssetSBUHelper(textAsset);
            }

            private void Read(Stream stream)
            {
                using var binaryReader = new BinaryReader(stream);
                var totalStates = binaryReader.ReadInt32();
                States = new State[totalStates];
                for (var i = 0; i < totalStates; i++)
                {
                    var state = new State {
                        name = binaryReader.ReadString()
                    };
                    var parts = state.PartsArray;
                    foreach (var obj in parts)
                    {
                        obj.param2.sex = binaryReader.ReadInt32();
                        obj.param2.target = binaryReader.ReadString();
                        obj.param2.weightPos = binaryReader.ReadSingle();
                        obj.param2.weightAng = binaryReader.ReadSingle();
                        obj.param3.chein = binaryReader.ReadString();
                        obj.param3.weight = binaryReader.ReadSingle();
                    }
                    var totalFrames = binaryReader.ReadInt32();
                    state.frames = new Frame[totalFrames];
                    for (var k = 0; k < totalFrames; k++)
                    {
                        var frame = new Frame {
                            frameNo = binaryReader.ReadInt32(),
                            editNo = binaryReader.ReadInt32()
                        };
                        var totalShapes = binaryReader.ReadInt32();
                        frame.shapes = new Shape[totalShapes];
                        for (var l = 0; l < totalShapes; l++)
                        {
                            var shape = new Shape {
                                shapeNo = binaryReader.ReadInt32()
                            };
                            for (var m = 0; m < 2; m++)
                            {
                                shape[m] = new PosAng();
                                for (var n = 0; n < 3; n++)
                                {
                                    shape[m].pos[n] = binaryReader.ReadSingle();
                                }
                                for (var n = 0; n < 3; n++)
                                {
                                    shape[m].ang[n] = binaryReader.ReadSingle();
                                }
                            }
                            frame.shapes[l] = shape;
                        }
                        state.frames[k] = frame;
                    }
                    States[i] = state;
                }
            }

            public void CopyFrom(MotionIKData motionIKData)
            {
                if (motionIKData == null)
                {
                    return;
                }

                var totalStates = motionIKData.states.Length;
                States = null;
                States = new MotionIKDataSerializable.State[totalStates];

                for (var i = 0; i < totalStates; i++)
                {
                    var state = new MotionIKDataSerializable.State();
                    var stateSource = motionIKData.states[i];
                    state.name = stateSource.name;
                    var num2 = state.PartsArray.Length;
                    for (var j = 0; j < num2; j++)
                    {
                        var obj = state.PartsArray[j];
                        var parts = stateSource.parts[j];
                        obj.param2.sex = parts.param2.sex;
                        obj.param2.target = parts.param2.target;
                        obj.param2.weightPos = parts.param2.weightPos;
                        obj.param2.weightAng = parts.param2.weightAng;
                        obj.param3.chein = parts.param3.chein;
                        obj.param3.weight = parts.param3.weight;
                    }

                    var num3 = stateSource.frames.Length;
                    state.frames = new MotionIKDataSerializable.Frame[num3];
                    for (var k = 0; k < num3; k++)
                    {
                        var frame = new MotionIKDataSerializable.Frame();
                        var frameSource = stateSource.frames[k];
                        frame.frameNo = frameSource.frameNo;
                        frame.editNo = frameSource.editNo;
                        var num4 = frameSource.shapes.Length;
                        frame.shapes = new MotionIKDataSerializable.Shape[num4];
                        for (var l = 0; l < num4; l++)
                        {
                            var shape = new MotionIKDataSerializable.Shape();
                            var shapeSource = frameSource.shapes[l];
                            shape.shapeNo = shapeSource.shapeNo;
                            for (var m = 0; m < 2; m++)
                            {
                                shape[m] = new MotionIKDataSerializable.PosAng();
                                for (var n = 0; n < 3; n++)
                                {
                                    shape[m].pos[n] = shapeSource[m].pos[n];
                                }
                                for (var num5 = 0; num5 < 3; num5++)
                                {
                                    shape[m].ang[num5] = shapeSource[m].ang[num5];
                                }
                            }
                            frame.shapes[l] = shape;
                        }
                        state.frames[k] = frame;
                    }
                    States[i] = state;
                }
            }

            public MotionIKDataSerializable Copy()
            {
                var motionIKData = new MotionIKDataSerializable();
                var totalStates = States.Length;
                motionIKData.States = new State[totalStates];

                motionIKData.Name = Name;

                for (var i = 0; i < totalStates; i++)
                {
                    var state = new State();
                    var stateSource = States[i];
                    state.name = stateSource.name;
                    var totalParts = state.PartsArray.Length;
                    for (var j = 0; j < totalParts; j++)
                    {
                        var obj = state.PartsArray[j];
                        var parts = stateSource.PartsArray[j];
                        obj.param2.sex = parts.param2.sex;
                        obj.param2.target = parts.param2.target;
                        obj.param2.weightPos = parts.param2.weightPos;
                        obj.param2.weightAng = parts.param2.weightAng;
                        obj.param3.chein = parts.param3.chein;
                        obj.param3.weight = parts.param3.weight;
                    }

                    var totalFrames = stateSource.frames.Length;
                    state.frames = new Frame[totalFrames];
                    for (var k = 0; k < totalFrames; k++)
                    {
                        var frame = new Frame();
                        var frameSource = stateSource.frames[k];
                        frame.frameNo = frameSource.frameNo;
                        frame.editNo = frameSource.editNo;
                        var num4 = frameSource.shapes.Length;
                        frame.shapes = new Shape[num4];
                        for (var l = 0; l < num4; l++)
                        {
                            var shape = new Shape();
                            var shapeSource = frameSource.shapes[l];
                            shape.shapeNo = shapeSource.shapeNo;
                            for (var m = 0; m < 2; m++)
                            {
                                shape[m] = new PosAng();
                                for (var n = 0; n < 3; n++)
                                {
                                    shape[m].pos[n] = shapeSource[m].pos[n];
                                }
                                for (var n = 0; n < 3; n++)
                                {
                                    shape[m].ang[n] = shapeSource[m].ang[n];
                                }
                            }
                            frame.shapes[l] = shape;
                        }
                        state.frames[k] = frame;
                    }
                    motionIKData.States[i] = state;
                }
                return motionIKData;
            }

            public MotionIKData MotionIKData()
            {
                var motionIKData = new MotionIKData();
                var totalStates = States.Length;
                motionIKData.states = new MotionIKData.State[totalStates];

                for (var i = 0; i < totalStates; i++)
                {
                    var state = new MotionIKData.State();
                    var stateSource = States[i];
                    state.name = stateSource.name;
                    var totalParts = state.parts.Length;
                    for (var j = 0; j < totalParts; j++)
                    {
                        var obj = state.parts[j];
                        var parts = stateSource.PartsArray[j];
                        obj.param2.sex = parts.param2.sex;
                        obj.param2.target = parts.param2.target;
                        obj.param2.weightPos = parts.param2.weightPos;
                        obj.param2.weightAng = parts.param2.weightAng;
                        obj.param3.chein = parts.param3.chein;
                        obj.param3.weight = parts.param3.weight;
                    }

                    var totalFrames = stateSource.frames.Length;
                    state.frames = new MotionIKData.Frame[totalFrames];
                    for (var k = 0; k < totalFrames; k++)
                    {
                        var frame = new MotionIKData.Frame();
                        var frameSource = stateSource.frames[k];
                        frame.frameNo = frameSource.frameNo;
                        frame.editNo = frameSource.editNo;
                        var num4 = frameSource.shapes.Length;
                        frame.shapes = new MotionIKData.Shape[num4];
                        for (var l = 0; l < num4; l++)
                        {
                            var shape = new MotionIKData.Shape();
                            var shapeSource = frameSource.shapes[l];
                            shape.shapeNo = shapeSource.shapeNo;
                            for (var m = 0; m < 2; m++)
                            {
                                shape[m] = new MotionIKData.PosAng();
                                for (var n = 0; n < 3; n++)
                                {
                                    shape[m].pos[n] = shapeSource[m].pos[n];
                                }
                                for (var n = 0; n < 3; n++)
                                {
                                    shape[m].ang[n] = shapeSource[m].ang[n];
                                }
                            }
                            frame.shapes[l] = shape;
                        }
                        state.frames[k] = frame;
                    }
                    motionIKData.states[i] = state;
                }
                return motionIKData;
            }

            public string[] StateNames
            {
                get
                {
                    if (States != null)
                    {
                        return States.Select((State p) => p.name).ToArray();
                    }
#if KK
                    return new string[] { };
#else
                    return Array.Empty<string>();
#endif
                }
            }

            public State GetNowState(string stateName)
            {
                if (States == null)
                {
                    return null;
                }

                // Select used to create StateNames preserves order.
                var si = Array.IndexOf(StateNames, stateName);
                if (si == -1)
                {
                    return null;
                }
                return States[si];
            }

            public Frame[] GetNowFrames(string stateName)
            {
                return GetNowState(stateName)?.frames;
            }

            public static MotionIKData.State ToState(State state)
            {
                var result = new MotionIKData.State();
                var stateSource = state;
                result.name = stateSource.name;

                var totalParts = result.parts.Length;
                for (var j = 0; j < totalParts; j++)
                {
                    var obj = result.parts[j];
                    var parts = stateSource.PartsArray[j];
                    obj.param2.sex = parts.param2.sex;
                    obj.param2.target = parts.param2.target;
                    obj.param2.weightPos = parts.param2.weightPos;
                    obj.param2.weightAng = parts.param2.weightAng;
                    obj.param3.chein = parts.param3.chein;
                    obj.param3.weight = parts.param3.weight;
                }

                var totalFrames = stateSource.frames.Length;
                result.frames = new MotionIKData.Frame[totalFrames];
                for (var k = 0; k < totalFrames; k++)
                {
                    var frame = new MotionIKData.Frame();
                    var frameSource = stateSource.frames[k];
                    frame.frameNo = frameSource.frameNo;
                    frame.editNo = frameSource.editNo;
                    var num4 = frameSource.shapes.Length;
                    frame.shapes = new MotionIKData.Shape[num4];
                    for (var l = 0; l < num4; l++)
                    {
                        var shape = new MotionIKData.Shape();
                        var shapeSource = frameSource.shapes[l];
                        shape.shapeNo = shapeSource.shapeNo;
                        for (var m = 0; m < 2; m++)
                        {
                            shape[m] = new MotionIKData.PosAng();
                            for (var n = 0; n < 3; n++)
                            {
                                shape[m].pos[n] = shapeSource[m].pos[n];
                            }
                            for (var n = 0; n < 3; n++)
                            {
                                shape[m].ang[n] = shapeSource[m].ang[n];
                            }
                        }
                        frame.shapes[l] = shape;
                    }
                    result.frames[k] = frame;
                }
                return result;
            }
        }
    }
}
