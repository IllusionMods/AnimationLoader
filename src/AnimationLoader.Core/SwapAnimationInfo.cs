using System;
using System.Xml.Linq;
using System.Xml.Serialization;

using UnityEngine;

using KKAPI;

using static HFlag;


namespace AnimationLoader
{
    [XmlRoot("Animation")]
    [Serializable]
    public class SwapAnimationInfo
    {
        private bool _isRelease = false;

        [XmlIgnore]
        public string Guid;

        [XmlElement]
        public int StudioId = -1;
        
        [XmlElement]
        public string PathFemale;

        [XmlElement]
        public string ControllerFemale;

        [XmlElement]
        public string PathFemale1;

        [XmlElement]
        public string ControllerFemale1;

        [XmlElement]
        public string PathMale;

        [XmlElement]
        public string ControllerMale;

        [XmlElement]
        public string AnimationName;

        [XmlElement]
        public HFlag.EMode Mode;
        
        [XmlElement]
        public KindHoushi kindHoushi;
        
        [XmlArray]
        [XmlArrayItem("category", Type = typeof(PositionCategory))]
        public PositionCategory[] categories = new PositionCategory[0];
        
        [XmlElement]
        public int DonorPoseId = -1;

        // NeckDonorId set to -2 so it can be overwritten in manifest when needed
        [XmlElement]
        public int NeckDonorId = -2;

        [XmlElement]
        public int NeckDonorIdFemale = -1;

        [XmlElement]
        public int NeckDonorIdFemale1 = -1;

        [XmlElement]
        public int NeckDonorIdMale = -1;

        [XmlElement]
        public string FileMotionNeck;

        [XmlElement]
        public string FileMotionNeckMale;

        [XmlElement]
        public bool? IsFemaleInitiative;

        [XmlElement]
        public string FileSiruPaste;

        [XmlElement]
        public int MotionIKDonor = -1;

        [XmlElement]
        public int MotionIKDonorMale = -1;

        [XmlElement]
        public int ExpTaii = -1;

        [XmlElement]
        public Vector3 PositionHeroine = Vector3.zero;

        [XmlElement]
        public Vector3 PositionPlayer = Vector3.zero;

        [XmlElement]
        public string SpecificFor = string.Empty;

        public bool IsRelease {
            get 
            {
                return _isRelease;
            }
            set 
            { 
                _isRelease = value; 
            }
        }
    }

    [XmlRoot(KoikatuAPI.GameProcessName)]
    [Serializable]
    public class OverrideInfo
    {
        [XmlIgnore]
        public string Guid;

        [XmlElement]
        public int StudioId = -1;

        [XmlElement]
        public string PathFemale;

        [XmlElement]
        public string ControllerFemale;

        [XmlElement]
        public string PathFemale1;

        [XmlElement]
        public string ControllerFemale1;

        [XmlElement]
        public string PathMale;

        [XmlElement]
        public string ControllerMale;

        [XmlElement]
        public string AnimationName;

        [XmlElement]
        public EMode Mode = EMode.none;

        [XmlElement]
        public KindHoushi kindHoushi = KindHoushi.none;

        [XmlArray]
        [XmlArrayItem("category", Type = typeof(PositionCategory))]
        public PositionCategory[] categories = new PositionCategory[0];

        [XmlElement]
        public int DonorPoseId = -1;

        // NeckDonorId set to -2 so it can be overwritten in manifest when needed
        [XmlElement]
        public int NeckDonorId = -2;

        [XmlElement]
        public int NeckDonorIdFemale = -1;

        [XmlElement]
        public int NeckDonorIdFemale1 = -1;

        [XmlElement]
        public int NeckDonorIdMale = -1;

        [XmlElement]
        public string FileMotionNeck;

        [XmlElement]
        public string FileMotionNeckMale;

        [XmlElement]
        public bool? IsFemaleInitiative;

        [XmlElement]
        public string FileSiruPaste;

        [XmlElement]
        public int MotionIKDonor = -1;

        [XmlElement]
        public int MotionIKDonorMale = -1;

        [XmlElement]
        public int ExpTaii = -1;

        [XmlElement]
        public Vector3 PositionHeroine = Vector3.zero;

        [XmlElement]
        public Vector3 PositionPlayer = Vector3.zero;
    }

    public enum KindHoushi
    {
        Hand = 0,
        Mouth = 1,
        Breasts = 2,
        none = -1
    }

    public enum PositionCategory
    {
        LieDown = 0,
        Stand = 1,
        SitChair = 2,
        Stool = 3,
        SofaBench = 4,
        BacklessBench = 5,
        SchoolDesk = 6,
        Desk = 7,
        Wall = 8,
        StandPool = 9,
        SitDesk = 10,
        SquadDesk = 11,
        Pool = 1004,
        MischievousCaress = 1003,
        Ground3P = 1100,
        AquariumCrowded = 1304,
        LieDown3P = 3000,
        SitChair3P = 3001
    }
}
