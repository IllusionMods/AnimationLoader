﻿using System;
using System.Xml.Linq;
using System.Xml.Serialization;

using static HFlag;


namespace AnimationLoader
{
    [XmlRoot("Animation")]
    [Serializable]
    public class SwapAnimationInfo
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
        public int NeckDonorId = -1;
        
        [XmlElement]
        public string FileMotionNeck;

        [XmlElement]
        public bool? IsFemaleInitiative;

        [XmlElement]
        public string FileSiruPaste;

        [XmlElement]
        public int MotionIKDonor = -1;

        [XmlElement]
        public XElement GameSpecificOverrides;
    }

    [XmlRoot("KoikatsuSunshine")]
    [Serializable]
    public class KKSOverrideInfo
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
        public int DonorPoseId;

        [XmlElement]
        public int NeckDonorId = -1;

        [XmlElement]
        public string FileMotionNeck;

        [XmlElement]
        public bool? IsFemaleInitiative;

        [XmlElement]
        public string FileSiruPaste;

        [XmlElement]
        public int MotionIKDonor = -1;
    }

    [XmlRoot("Koikatu")]
    [Serializable]
    public class KKOverrideInfo
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
        public int DonorPoseId;

        [XmlElement]
        public int NeckDonorId = -1;

        [XmlElement]
        public string FileMotionNeck;

        [XmlElement]
        public bool? IsFemaleInitiative;

        [XmlElement]
        public string FileSiruPaste;

        [XmlElement]
        public int MotionIKDonor = -1;
    }

    public enum KindHoushi
    {
        Hand = 0,
        Mouth = 1,
        Breasts = 2,
        none = -1,
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
        none = -1,
    }
}
