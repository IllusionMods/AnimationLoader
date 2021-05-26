using System;
using System.Xml.Serialization;

namespace AnimationLoader.Koikatu
{
    [XmlRoot("Animation")]
    [Serializable]
    public class SwapAnimationInfo
    {
        [XmlElement]
        public string PathFemale;

        [XmlElement]
        public string ControllerFemale;
        
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
        public int DonorPoseId;
        
        [XmlElement]
        public int? NeckDonorId;
        
        [XmlElement]
        public string FileMotionNeck;

        [XmlElement]
        public bool? IsFemaleInitiative;

        [XmlElement]
        public string FileSiruPaste;

        [XmlElement]
        public int? MotionIKDonor;
    }

    public enum KindHoushi
    {
        Hand = 0,
        Mouth = 1,
        Breasts = 2
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
        Ground3P = 1100,
    }
}