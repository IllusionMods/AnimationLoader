//
// For people who keep their own names for the animations or translate them to oder
// language.
//
// Save animations names to UserData/AnimationLoader/Names so they are not overwritten by
// updates. They can be maintained and updated there.
//
// Only new animations are added to the file.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        private static readonly XmlSerializer xmlNamesSerializer = new(typeof(Names));
        private static Dictionary<string, Names> animationNamesDict = new();

        #region Serializable classes
        [XmlRoot("Names")]
        [Serializable]
        public class Names
        {
            [XmlElement]
            public string guid = string.Empty;

            [XmlElement]
            public string name = string.Empty;

            [XmlElement]
            public string version = string.Empty;

            [XmlElement]
            public string author = string.Empty;

            [XmlElement]
            public string description = string.Empty;

            [XmlElement]
            public string website = string.Empty;

            [XmlArray("AnimationLoader")]
            public List<Animation> Anim = new();
        }

        //
        // A real dictionary was better for programming but less friendly to user interaction
        //
        [XmlRoot("Animation")]
        [Serializable]
        public class Animation
        {
            [XmlElement]
            public int StudioId;
            [XmlElement]
            public string Controller;
            [XmlElement]
            public string Koikatu;
            [XmlElement]
            public string KoikatuReference;
            [XmlElement]
            public string KoikatsuSunshine;
            [XmlElement]
            public string KoikatsuSunshineReference;
#if DEBUG
            [XmlElement]
            public string KatarsysName;
#endif
        }
        #endregion

        /// <summary>
        /// Read animations names xml files
        /// </summary>
        private static void LoadNamesXml()
        {
            if (!UserOverrides.Value)
            {
                return;
            }

            var path = Path.Combine(UserData.Path, "AnimationLoader/Names");
            if (Directory.Exists(path))
            {
                var docs = Directory.GetFiles(path, "*.xml").Select(XDocument.Load).ToList();
                if (docs.Count > 0)
                {
                    Log.Debug($"0001: [{PInfo.PluginName}] Loading animations names.");
                    LoadNamesXmls(docs);
                    return;
                }
            }
            Log.Debug("0002: No names found.");
        }

        /// <summary>
        /// Add the names on the xml files to names dictionary
        /// </summary>
        /// <param name="namesDocs"></param>
        private static void LoadNamesXmls(IEnumerable<XDocument> namesDocs)
        {
            if (!UserOverrides.Value)
            {
                return;
            }

            foreach (var animElem in namesDocs.Select(x => x.Root))
            {
                try
                {
                    var reader = animElem.CreateReader();
                    var names = (Names)xmlNamesSerializer.Deserialize(reader);
                    reader.Close();
                    animationNamesDict.Add(names.guid, names);
                }
                catch (Exception ex)
                {
                    Log.Error($"0003: Error trying to read names file. {ex}");
                }
            }
        }

        /// <summary>
        /// Setup new names for guid
        /// </summary>
        /// <param name="manifest"></param>
        private static void NamesAddGuid(XElement manifest)
        {
            if (!UserOverrides.Value)
            {
                return;
            }

            // new names
            var names = new Names();
            // initialize fields with manifest date
            names.guid = manifest.Element(nameof(names.guid)).Value;
            names.name = manifest.Element(nameof(names.name)).Value;
            names.version = manifest.Element(nameof(names.version)).Value;
            names.author = manifest.Element(nameof(names.author)).Value;
            //names.description = manifest.Element(nameof(names.description)).Value;
            names.description = "Modify the items Koikatu and KoikatsuSunshine only.";
            names.website = manifest.Element(nameof(names.website)).Value;
            // add new names to dictionary
            animationNamesDict.Add(names.guid, names);
        }

        /// <summary>
        /// Save all the names in the names dictionary to xml files
        /// </summary>
        private static void SaveNamesXmls()
        {
            if (!UserOverrides.Value)
            {
                return;
            }

            foreach (var guid in animationNamesDict.Keys)
            {
                var animNames = animationNamesDict[guid];
                SaveNames(animNames, guid);
            }
        }

        /// <summary>
        /// Save names to xml files. One xml file is saved per guid in dictionary.
        /// </summary>
        /// <param name="names"></param>
        /// <param name="guid"></param>
        private static void SaveNames(Names names, string guid, bool overwrite = false)
        {
            if (!UserOverrides.Value)
            {
                return;
            }

            var path = Path.Combine(UserData.Path, "AnimationLoader/Names");
            var fileName = $"{path}/names-{guid}.xml";
            FileInfo file = new(fileName);

            Log.Debug($"0004: Saving names file {fileName}.");

            // Create directory where to save the file.
            // Safe to do even if directory exists.
            file.Directory.Create();
            if (file.Exists && !overwrite)
            {
                Log.Debug($"0005: File {fileName} already exits not overwritten.");
                return;
            }

            XmlSerializer xmlSerializer = new(typeof(Names));
            var writer = new StreamWriter($"{path}/names-{guid}.xml");
            Log.Debug($"004A: Saving ....");
            xmlSerializer.Serialize(writer.BaseStream, names);
            Log.Debug($"004A: Saved.");
            writer.Close();
        }

        /// <summary>
        /// Read the names from an xml file
        /// </summary>
        /// <param name="names"></param>
        /// <param name="guid"></param>
        private static void ReadNames(ref Names names, string guid)
        {
            if (!UserOverrides.Value)
            {
                return;
            }

            var path = Path.Combine(UserData.Path, "AnimationLoader/Names");
            var fileName = $"{path}/names-{guid}.xml";
            FileInfo file = new(fileName);

            if (!file.Exists)
            {
                Log.Debug($"0006: Names file {fileName} not found. Safe to ignore.");
                return;
            }

            XmlSerializer xmlSerializer = new(typeof(Names));
            StreamReader reader = new($"{path}/names-{guid}.xml");
            names = (Names)xmlSerializer.Deserialize(reader);
            reader.Close();
        }

#if DEBUG
        private static readonly Dictionary<string, string> katarsysNameMapper = AnimNameMapper_H();
        private static readonly Dictionary<string, string> katarsysName = ButtonNames();

        public static string KatarsysAnimationName(SwapAnimationInfo animation)
        {
            var name = string.Empty;
            //var controller = string.Empty;

            try
            {
                var controller = katarsysNameMapper[animation.ControllerFemale];
                name = katarsysName[controller];
            }
            catch (Exception ex) { Log.Debug($"0018: {ex.Message} - {animation.ControllerFemale}"); }

            return name;
        }

        public static void KatarsysAnimationNameAnalysis()
        {
            var namesSet = new HashSet<string>();

            namesSet.Clear();

            foreach (var element in katarsysName)
            {
                if (!namesSet.Contains(element.Value))
                {
                    namesSet.Add(element.Value);
                }
                else
                {
                    Log.Error($"Name for {element.Key}-{element.Value} repeated.");
                }
            }

        }

        public static Dictionary<string, string> ButtonNames()
        {
            Dictionary<string, string> dictionary = new();

            dictionary["S_Lay_Footjob_1"] = "Standing, Penis Massage";
            dictionary["S_Lay_Footjob_2"] = "Lying, Massage with 2 feet";
            dictionary["S_Lay_Footjob_3"] = "Kneeling, Boy Leads";
            dictionary["S_Lay_Footjob_4"] = "Standing, Doggy Massage";
            dictionary["S_Lay_Footjob_5"] = "Back Cuddle";
            dictionary["S_Lay_HumpAss_1"] = "Lying, Ass Hump";
            dictionary["S_Lay_HumpAss_2"] = "Kneeling, Ass Hump";
            dictionary["S_Lay_ThighJob_1"] = "Lying, Thigh Job";
            dictionary["S_Lay_ThighJob_2"] = "Lying, Straddle Thigh Job";
            dictionary["S_Lay_StraddleHJ_1"] = "Lying, Straddle";
            dictionary["S_Lay_Rimjob_1"] = "Lying, Rimjob";
            dictionary["S_Lay_MouthPist"] = "Lying, Mouth Fuck";
            dictionary["S_Lay_Lick_1"] = "Lying, Licking";
            dictionary["S_Sit_Footjob_1"] = "Sitting, Penis Massage";
            dictionary["S_Sit_Footjob_5"] = "Standing Massage";
            dictionary["S_Sit_Footjob_2"] = "Cinderella";
            dictionary["S_Sit_Footjob_3"] = "Under Pressure";
            dictionary["S_Sit_Footjob_4"] = "Piledriver Massage";
            dictionary["S_Wall_Footjob_1"] = "Wall, Penis Massage";
            dictionary["S_Wall_BJ_1"] = "Sucking, Forced";
            dictionary["S_Sit_BJ_2"] = "Sucking, Sitting Girl";
            dictionary["S_Sit_ThighJob_1"] = "Back, Thigh Job";
            dictionary["S_Stand_ThighJob_1"] = "Standing, Thigh Job";
            dictionary["S_Desk_HumpAss_1"] = "Back, Ass Hump";
            dictionary["S_Desk_Footjob_1"] = "Desk, Penis Massage";
            dictionary["S_Desk_Footjob_2"] = "Desk, Doggy Massage";
            dictionary["S_Desk_BJ_Back_1"] = "Lying on Table, Blowjob";
            dictionary["S_Desk_BJ_1"] = "Kneeling on Table, Blowjob";
            dictionary["S_Desk_DT_1"] = "Desk, Deepthroat";
            dictionary["S_Desk_DT_2"] = "Desk, Forced Deepthroat";
            dictionary["S_Wall_DT_1"] = "Wall, Forced Deepthroat";
            dictionary["S_Wall_Boobs_1"] = "Grabbed Boobs Fuck";
            dictionary["S_Desk_Handjob_1"] = "On Table, Handjob";
            dictionary["S_Wall_RimAndFing_1"] = "Rimjob & Fingering";
            dictionary["S_Wall_RimAndHand_1"] = "Handjob & Rimjob";
            dictionary["S_Lay_Fingering"] = "Lying, Fingering";
            dictionary["S_Lay_69"] = "69";
            dictionary["S_Lay_YingYang_1"] = "Ying Yang";
            dictionary["S_Stand_Cunni"] = "Standing, Cunnilingus";
            dictionary["S_Lay_Cunni"] = "Lying, Cunnilingus";
            dictionary["S_Lay_FaceCunni_1"] = "Lying, Face Riding Cunni";
            dictionary["S_Lay_RimJob_2"] = "Lying, Rimjob 2";
            dictionary["S_Lay_RimAndFoot_1"] = "Lying, Rim & Foot Jobs";
            dictionary["S_Sit_AnalFinger_1"] = "Anal Fingering";
            dictionary["T_Stand"] = "Stand";
            dictionary["T_Lay"] = "Lying";
            dictionary["T_Lay_Doggy"] = "Four-legged";
            dictionary["T_Sit_Front"] = "Front";
            dictionary["T_Sit_Back"] = "Sitting, Back";
            dictionary["T_Desk_Back"] = "Desk, Back";
            dictionary["T_Desk_On_2"] = "Sitting";
            dictionary["T_Desk_On_3"] = "Crouch";
            dictionary["T_Wall_Back"] = "Wall, Back";
            dictionary["T_Stand_Library_sp"] = "Side";
            dictionary["T_Sit_InStudy_sp"] = "Studying";
            dictionary["S_Lay_KnobPolish"] = "Lying, Glans Fondling";
            dictionary["S_Lay_SideHandjob"] = "Lying, Next To Side HandJob";
            dictionary["S_Lay_FrontHandjob"] = "Lying, Two hands";
            dictionary["S_Lay_KnobLick"] = "Lying, Glans Licking";
            dictionary["S_Lay_BlowJob"] = "Lying, Sucking";
            dictionary["S_Lay_HandedBlowJob"] = "Lying, Helping with Hand";
            dictionary["S_Lay_Paizuri"] = "Lying, Titty-fuck";
            dictionary["S_Lay_Boobs_1"] = "Lying, Grabbed Boobs Fuck";
            dictionary["S_Lay_Press_Paizuri"] = "Lying, Pressed Titty-fuck";
            dictionary["S_Lay_OnTop_Paizuri"] = "Lying, Upper Pressure";
            dictionary["S_Lay_Suck_Paizuri"] = "Lying, Sucked Titty-fuck";
            dictionary["S_Sit_HandJob1"] = "Glans Fondling";
            dictionary["S_Sit_HandJob2"] = "One Hand";
            dictionary["S_Sit_KnobLick"] = "Glans Licking";
            dictionary["S_Sit_NoHandKnobLick"] = "Glans Licking, no hand";
            dictionary["S_Sit_BlowJob"] = "Sucking";
            dictionary["S_Sit_DT_1"] = "Sitting, Forced Deepthroat";
            dictionary["S_Sit_Cunni_1"] = "Pussy Eater";
            dictionary["S_Sit_Paizuri"] = "Titty-fuck";
            dictionary["S_Sit_HandNipLick_1"] = "Sitting, Nip Licking";
            dictionary["S_Sit_Press_Paizuri"] = "Pressed Titty-fuck";
            dictionary["S_Sit_Lick_Paizuri"] = "Licked Titty-fuck";
            dictionary["S_Sit_Suck_Paizuri"] = "Sucked Titty-fuck";
            dictionary["S_Stand_Handjob"] = "Kneeling, Glans Fondling";
            dictionary["S_Stand_Handjob_DeskV"] = "Kneeling";
            dictionary["S_Stand_HandNipLick_1"] = "Nip Licking";
            dictionary["S_Stand_BallLick"] = "Kneeling, Ball Licking";
            dictionary["S_Stand_PenisMassage"] = "Kneeling, Ball Fondling";
            dictionary["S_Stand_Armpit_1"] = "Kneeling, Armpit";
            dictionary["S_Stand_Handjob_2"] = "Standing, Massage";
            dictionary["S_Stand_BlowJob"] = "Kneeling, Using One Hand";
            dictionary["S_Stand_DT_1"] = "Deepthroat";
            dictionary["S_Stand_DT_2"] = "Forced Deepthroat";
            dictionary["S_Stand_HandedBlowJob"] = "Kneeling, Using Two Hands";
            dictionary["S_Stand_NoHandBlowJob"] = "Kneeling, No Hand";
            dictionary["S_Stand_Paizuri"] = "Kneeling, Titty-fuck";
            dictionary["S_Stand_Press_Paizuri"] = "Kneeling, Pressed Titty-fuck";
            dictionary["S_Stand_Suck_Paizuri"] = "Kneeling, Sucked Titty-fuck";
            dictionary["S_Pool_Paizuri_sp"] = "Titty-fuck in Water";
            dictionary["S_Stand_OnaHole"] = "Onahole";
            dictionary["S_Sit_BenchBlowJob"] = "In Front";
            dictionary["S_Sit_SideBlowJob"] = "Next To Side BlowJob";
            dictionary["S_Stand_WallHandJob"] = "Next To Wall HandJob";
            dictionary["P_Lay_Missionary_1"] = "Missionary 2";
            dictionary["P_Lay_Missionary_2"] = "Spread Legs";
            dictionary["P_Lay_Missionary_3"] = "Lifted up Missionary 1";
            dictionary["P_Lay_Missionary_4"] = "Lifted up Missionary 2";
            dictionary["P_Lay_Missionary_5"] = "Missionary 3";
            dictionary["P_Lay_Missionary_6"] = "Spread Eagle";
            dictionary["P_Lay_Missionary_7"] = "Missionary Interlock";
            dictionary["P_Lay_Flexion_1"] = "Flexion";
            dictionary["P_Lay_Front_1"] = "Knee Hug";
            dictionary["P_Lay_Straddle_1"] = "Straddle, Legs on Shoulders";
            dictionary["P_Lay_StraddleBck_1"] = "Straddle Back";
            dictionary["P_Lay_Side"] = "Spooning 1";
            dictionary["P_Lay_Back"] = "Lying, Behind";
            dictionary["P_Lay_Back_2"] = "Seiza Rear Fuck";
            dictionary["P_Lay_Press"] = "Mating Press";
            dictionary["P_Lay_revCowGirl_1"] = "Inverted Cowgirl 1";
            dictionary["P_Lay_revCowGirl_2"] = "Inverted Cowgirl 2";
            dictionary["P_Lay_revCowGirl_3"] = "Inverted Cowgirl 3";
            dictionary["P_Lay_Spoon_1"] = "Spooning 2";
            dictionary["P_Lay_Piledriver_1"] = "Piledriver";
            dictionary["P_Lay_Piledriver_3"] = "Piledriver 2";
            dictionary["P_Lay_Piledriver_2"] = "Femdom Piledriver";
            dictionary["P_Lay_revPiledriver_1"] = "Femdom Back Piledriver";
            dictionary["P_Lay_CowGirl"] = "Cowgirl 2";
            dictionary["P_Lay_CowGirl_2"] = "Cowgirl, Restrain";
            dictionary["P_Lay_CowGirl_3"] = "Cowgirl, Chest to Chest";
            dictionary["P_Lay_CowGirl_4"] = "Cowgirl, Hands on Knees";
            dictionary["P_Lay_CowGirl_5"] = "Cowgirl, Holding Hands";
            dictionary["P_Lay_CowGirl_6"] = "Cowgirl, Holding Hands 2";
            dictionary["P_Lay_Bridge_1"] = "Bridge";
            dictionary["P_Lay_Doggy_1"] = "Doggy Style";
            dictionary["P_Lay_Doggy_2"] = "Grabbed Doggy Style";
            dictionary["P_Lay_Doggy_3"] = "Doggy, Straddle";
            dictionary["P_Lay_Doggy_4"] = "Doggy, Forced";
            dictionary["P_Lay_Doggy_5"] = "Doggy, Face Down";
            dictionary["P_Stand_Front_2"] = "Grabbed Standing";
            dictionary["P_Stand_Back_2"] = "Standing, Behind 2";
            dictionary["P_Stand_Back_1"] = "Standing, Behind 1";
            dictionary["P_Stand_BackLift_1"] = "Standing, Back Carrying";
            dictionary["P_Stand_Back_3"] = "Standing, Doggy";
            dictionary["P_Wall_FloorFront_1"] = "Front, on Floor";
            dictionary["P_Sit_Ride_Back_2"] = "Behind, Four legs";
            dictionary["P_Sit_Ride_Back_3"] = "Behind, Squat";
            dictionary["P_Sit_Flexion"] = "Sitting, Flexion";
            dictionary["P_Sit_Froggy_1"] = "Froggy Style";
            dictionary["P_Lay_BckCuddle_1"] = "Trust Back Cuddle";
            dictionary["P_Lay_BckArmLock_1"] = "Trust Back, Arms Lock";
            dictionary["P_Lay_LotusStack_1"] = "Lotus Stacking";
            dictionary["P_Lay_BckSplits_1"] = "Inverted Cowgirl, Splits";
            dictionary["P_Lay_Nelson_1"] = "Nelson";
            dictionary["P_Stand_Nelson_1"] = "Standing, Nelson";
            dictionary["P_Stand_Front"] = "Standing";
            dictionary["P_Stand_FrontLift"] = "Carrying";
            dictionary["P_Stand_Bridge_1"] = "Standing, Bridge";
            dictionary["P_Sit_Ride_Front"] = "Sitting, Riding";
            dictionary["P_Sit_Ride_Front_2"] = "Cowgirl, Straddle";
            dictionary["P_Sit_Ride_Back"] = "Behind";
            dictionary["P_Sit_Doggy_1"] = "Sitting, Doggy Style";
            dictionary["P_Sit_Doggy_2"] = "Sitting, Grabbed Doggy Style";
            dictionary["P_Sit_CowGirl"] = "Sitting, Cowgirl";
            dictionary["P_Sit_Front_sp"] = "Facing";
            dictionary["P_Desk_Missionary"] = "Desk, Missionary";
            dictionary["P_Desk_Missionary_2"] = "Desk, Missionary 2";
            dictionary["P_Desk_Flexion"] = "Desk, Flexion";
            dictionary["P_Desk_LotusStack_1"] = "Desk, Lotus Stacking";
            dictionary["P_Desk_Doggy_1"] = "Desk, Doggy Style";
            dictionary["P_Desk_Doggy_2"] = "Desk, Grabbed Doggy Style";
            dictionary["P_Desk_Doggy_3"] = "Desk, Doggy Face Down";
            dictionary["P_Desk_Side"] = "Spooning";
            dictionary["P_Wall_Front"] = "Wall, Carrying";
            dictionary["P_Wall_Front_2"] = "Wall, Carrying Forced";
            dictionary["P_Wall_Splits_1"] = "Splits";
            dictionary["P_Wall_Ride_1"] = "Wall, Riding";
            dictionary["P_Wall_Doggy_1"] = "Kneeling Doggy";
            dictionary["P_Wall_Back_1"] = "Wall, Doggy Style";
            dictionary["P_Wall_Back_2"] = "Wall, Grabbed Doggy Style";
            dictionary["P_Wall_Back_sp"] = "Fence Doggy Style";
            dictionary["P_Wall_FrontLift_sp"] = "Fence Grasping";
            dictionary["P_Desk_Back_sp"] = "Vaulting Box, Behind";
            dictionary["P_Wall_Back_3"] = "Behind in Subway";
            dictionary["P_Pool_Back_sp"] = "Pool, Behind";
            dictionary["S_DoubleMouthJob_Main"] = "Double Fella, First Leads";
            dictionary["S_DoubleMouthJob_Second"] = "Double Fella, Second Leads";
            dictionary["S_FellaNdNipLick_Main"] = "Fella + Nip Licking, First Sucks";
            dictionary["S_FellaNdNipLick_Second"] = "Fella + Nip Licking, Second Sucks";
            dictionary["P_DoubleCowgirl_Main"] = "First Rides";
            dictionary["P_DoubleCowgirl_Second"] = "Second Rides";
            dictionary["P_BackAndTease_Main"] = "Doggy Style, First";
            dictionary["P_BackAndTease_Second"] = "Doggy Style, Second";
            dictionary["P_FrontAndTease_Main"] = "Missionary, First";
            dictionary["P_FrontAndTease_Second"] = "Missionary, Second";
            dictionary["P_DoggyAndCunni_Main"] = "Doggy and Cunni, First";
            dictionary["P_DoggyAndCunni_Second"] = "Doggy and Cunni, Second";
            dictionary["S_Dark_ForcedHandjob"] = "Forced HandJob";
            dictionary["S_Dark_ForcedBlowjob"] = "Face-fucking";
            dictionary["S_Dark_HandAndBlowjob"] = "Double Fellatio";
            dictionary["P_Dark_FrontHoldHands"] = "Facing, Grabbed";
            dictionary["P_Dark_SideHoldHands"] = "Spooning, Grabbed";
            dictionary["P_Dark_BackAndSucks"] = "Fuck and Suck";
            dictionary["M_Stand"] = "Masturbation, Standing";
            dictionary["M_Sit"] = "Masturbation, on Chair";
            dictionary["M_OnHurdle"] = "Masturbation, on Hurdle";
            dictionary["M_Lay_Masturbation_1"] = "Masturbation, Kneeling";
            dictionary["M_Wall_AnaFinger_1"] = "Wall, Masturbation Anal Fingering";
            dictionary["M_Sit_AnaFinger_1"] = "Sitting, Masturbation Anal Fingering";
            dictionary["M_Desk_Finger_1"] = "Des, Masturbation Fingering";
            dictionary["L_Lay_Interact"] = "Shell, Lying";
            dictionary["L_Sit_Cunni"] = "Cunnilingus, Sitting";
            dictionary["L_Stand_Touch"] = "Fondle, Standing";
            dictionary["L_Lay_Cunni_1"] = "Cunnilingus, Lying";
            dictionary["L_Lay_Cunni_2"] = "Swap Cunnilingus, Lying";
            dictionary["L_Stand_Cunni_1"] = "Cunnilingus, Standing";
            dictionary["L_Stand_Cunni_2"] = "Swap Cunnilingus, Standing";

            return dictionary;
        }

        public static Dictionary<string, string> AnimNameMapper_H() => new()
        {

            ["khh_f_60"] = "S_Lay_Footjob_1",
            ["khh_f_61"] = "S_Lay_Footjob_2",
            ["khh_f_63"] = "S_Lay_Footjob_3",
            ["khh_f_82"] = "S_Lay_Footjob_4",
            ["khh_f_83"] = "S_Lay_Footjob_5",
            ["khh_f_64"] = "S_Lay_Rimjob_1",
            ["khh_f_65"] = "S_Lay_HumpAss_1",
            ["khh_f_66"] = "S_Lay_StraddleHJ_1",
            ["khh_f_68"] = "S_Lay_MouthPist",
            ["khh_f_69"] = "S_Lay_Lick_1",
            ["khh_f_70"] = "S_Lay_ThighJob_1",
            ["khh_f_72"] = "S_Lay_HumpAss_2",
            ["khh_f_73"] = "S_Stand_ThighJob_1",
            ["khh_f_62"] = "S_Sit_Footjob_1",
            ["khh_f_76"] = "S_Sit_Footjob_2",
            ["khh_f_77"] = "S_Sit_Footjob_3",
            ["khh_f_81"] = "S_Sit_Footjob_4",
            ["khh_f_67"] = "S_Sit_BJ_2",
            ["khh_f_71"] = "S_Sit_ThighJob_1",
            ["khh_f_74"] = "S_Desk_HumpAss_1",
            ["khh_f_75"] = "S_Desk_Footjob_1",
            ["khh_f_78"] = "S_Desk_BJ_Back_1",
            ["khh_f_79"] = "S_Wall_BJ_1",
            ["khh_f_80"] = "S_Wall_Footjob_1",
            ["khh_f_84"] = "S_Desk_Footjob_2",
            ["khh_f_85"] = "S_Lay_ThighJob_2",
            ["khh_f_86"] = "S_Stand_Armpit_1",
            ["khh_f_87"] = "S_Stand_DT_1",
            ["khh_f_88"] = "S_Stand_Handjob_2",
            ["khh_f_89"] = "S_Lay_Boobs_1",
            ["khh_f_90"] = "S_Stand_DT_2",
            ["khh_f_91"] = "S_Lay_RimAndFoot_1",
            ["khh_f_92"] = "S_Sit_DT_1",
            ["khh_f_93"] = "S_Desk_DT_1",
            ["khh_f_94"] = "S_Wall_DT_1",
            ["khh_f_95"] = "S_Desk_DT_2",
            ["khh_f_96"] = "S_Wall_Boobs_1",
            ["khh_f_97"] = "S_Wall_RimAndHand_1",
            ["khh_f_98"] = "S_Sit_Footjob_5",
            ["khh_f_99"] = "S_Desk_Handjob_1",
            ["khh_f_100"] = "S_Desk_BJ_1",
            ["khh_f_102"] = "S_Stand_HandNipLick_1",
            ["khh_f_103"] = "S_Sit_HandNipLick_1",
            ["khs_f_59"] = "P_Wall_Front_2",
            ["khs_f_60"] = "P_Lay_revCowGirl_1",
            ["khs_f_61"] = "P_Stand_Back_1",
            ["khs_f_62"] = "P_Lay_Missionary_3",
            ["khs_f_63"] = "P_Stand_BackLift_1",
            ["khs_f_64"] = "P_Wall_FloorFront_1",
            ["khs_f_65"] = "P_Lay_Spoon_1",
            ["khs_f_66"] = "P_Stand_Back_2",
            ["khs_f_67"] = "P_Lay_Missionary_4",
            ["khs_f_68"] = "P_Lay_Piledriver_1",
            ["khs_f_69"] = "P_Stand_Front_2",
            ["khs_f_70"] = "P_Sit_Ride_Back_2",
            ["khs_f_71"] = "P_Lay_revCowGirl_2",
            ["khs_f_72"] = "P_Lay_CowGirl_2",
            ["khs_f_73"] = "P_Lay_Piledriver_2",
            ["khs_f_75"] = "P_Desk_Flexion",
            ["khs_f_76"] = "P_Lay_CowGirl_3",
            ["khs_f_77"] = "P_Lay_Doggy_3",
            ["khs_f_78"] = "P_Sit_Flexion",
            ["khs_f_79"] = "P_Lay_BckCuddle_1",
            ["khs_f_80"] = "P_Lay_LotusStack_1",
            ["khs_f_81"] = "P_Lay_BckSplits_1",
            ["khs_f_82"] = "P_Desk_LotusStack_1",
            ["khs_f_83"] = "P_Sit_Ride_Back_3",
            ["khs_f_84"] = "P_Wall_Doggy_1",
            ["khs_f_85"] = "P_Lay_Missionary_5",
            ["khs_f_86"] = "P_Lay_revPiledriver_1",
            ["khs_f_87"] = "P_Desk_Missionary_2",
            ["khs_f_88"] = "P_Lay_Piledriver_3",
            ["khs_f_89"] = "P_Wall_Splits_1",
            ["khs_f_90"] = "P_Stand_Back_3",
            ["khs_f_91"] = "P_Sit_Froggy_1",
            ["khs_f_92"] = "P_Lay_CowGirl_4",
            ["khs_f_93"] = "P_Lay_Doggy_4",
            ["khs_f_94"] = "P_Lay_BckArmLock_1",
            ["khs_f_95"] = "P_Lay_Missionary_6",
            ["khs_f_96"] = "P_Sit_Ride_Front_2",
            ["khs_f_97"] = "P_Lay_Nelson_1",
            ["khs_f_98"] = "P_Stand_Nelson_1",
            ["khs_f_99"] = "P_Lay_CowGirl_5",
            ["khs_f_100"] = "P_Lay_Bridge_1",
            ["khs_f_101"] = "P_Wall_Ride_1",
            ["khs_f_102"] = "P_Lay_Missionary_7",
            ["khs_f_103"] = "P_Desk_Doggy_3",
            ["khs_f_104"] = "P_Lay_Doggy_5",
            ["khs_f_105"] = "P_Lay_Straddle_1",
            ["khs_f_106"] = "P_Lay_StraddleBck_1",
            ["khs_f_107"] = "P_Lay_Flexion_1",
            ["khs_f_108"] = "P_Lay_Back_2",
            ["khs_f_109"] = "P_Stand_Bridge_1",
            ["khs_f_110"] = "P_Lay_Front_1",
            ["khs_f_111"] = "P_Lay_CowGirl_6",
            ["khs_f_112"] = "P_Lay_revCowGirl_3",
            ["kha_f_00"] = "T_Stand",
            ["kha_f_01"] = "T_Lay",
            ["kha_f_02"] = "T_Lay_Doggy",
            ["kha_f_05"] = "T_Desk_Back",
            ["kha_f_07"] = "T_Desk_On_2",
            ["kha_f_08"] = "T_Desk_On_3",
            ["kha_f_06"] = "T_Wall_Back",
            ["kha_f_03"] = "T_Sit_Front",
            ["kha_f_04"] = "T_Sit_Back",
            ["kha_f_09"] = "T_Stand_Library_sp",
            ["kha_f_10"] = "T_Sit_InStudy_sp",
            ["khs_f_00"] = "P_Lay_Missionary_1",
            ["khs_f_n00"] = "P_Lay_Missionary_2",
            ["khs_f_02"] = "P_Lay_Doggy_1",
            ["khs_f_n02"] = "P_Lay_Doggy_2",
            ["khs_f_n04"] = "P_Lay_CowGirl",
            ["khs_f_n06"] = "P_Lay_Side",
            ["khs_f_n07"] = "P_Stand_Front",
            ["khs_f_n08"] = "P_Stand_FrontLift",
            ["khs_f_n23"] = "P_Lay_Press",
            ["khs_f_n26"] = "P_Lay_Back",
            ["khs_f_n13"] = "P_Desk_Missionary",
            ["khs_f_14"] = "P_Desk_Doggy_1",
            ["khs_f_n14"] = "P_Desk_Doggy_2",
            ["khs_f_n16"] = "P_Desk_Side",
            ["khs_f_n17"] = "P_Wall_Front",
            ["khs_f_18"] = "P_Wall_Back_1",
            ["khs_f_n18"] = "P_Wall_Back_2",
            ["khs_f_n28"] = "P_Wall_Back_3",
            ["khs_f_n09"] = "P_Sit_Ride_Front",
            ["khs_f_n10"] = "P_Sit_Ride_Back",
            ["khs_f_11"] = "P_Sit_Doggy_1",
            ["khs_f_n11"] = "P_Sit_Doggy_2",
            ["khs_f_n27"] = "P_Sit_CowGirl",
            ["khs_f_n21"] = "P_Wall_Back_sp",
            ["khs_f_n22"] = "P_Wall_FrontLift_sp",
            ["khs_f_n20"] = "P_Pool_Back_sp",
            ["khs_f_n24"] = "P_Sit_Front_sp",
            ["khs_f_n25"] = "P_Desk_Back_sp",
            ["khh3_f_00_02"] = "S_FellaNdNipLick_Main",
            ["khh3_f_00_03"] = "S_FellaNdNipLick_Second",
            ["khh3_f_00_00"] = "S_DoubleMouthJob_Main",
            ["khh3_f_00_01"] = "S_DoubleMouthJob_Second",
            ["khs3_f_00_00"] = "P_DoubleCowgirl_Main",
            ["khs3_f_00_01"] = "P_DoubleCowgirl_Second",
            ["khs3_f_01_00"] = "P_BackAndTease_Main",
            ["khs3_f_01_01"] = "P_BackAndTease_Second",
            ["khs3_f_02_00"] = "P_FrontAndTease_Main",
            ["khs3_f_02_01"] = "P_FrontAndTease_Second",
            ["khs3_f_03_00"] = "P_DoggyAndCunni_Main",
            ["khs3_f_03_01"] = "P_DoggyAndCunni_Second",
            ["khs3_f_02"] = "P_Dark_FrontHoldHands",
            ["khs3_f_03"] = "P_Dark_SideHoldHands",
            ["khs3_f_04"] = "P_Dark_BackAndSucks",
            ["khh3_f_01"] = "S_Dark_ForcedHandjob",
            ["khh3_f_02"] = "S_Dark_ForcedBlowjob",
            ["khh3_f_03"] = "S_Dark_HandAndBlowjob",
            ["khh_f_00"] = "S_Lay_KnobPolish",
            ["khh_f_01"] = "S_Lay_SideHandjob",
            ["khh_f_02"] = "S_Lay_FrontHandjob",
            ["khh_f_16"] = "S_Sit_HandJob1",
            ["khh_f_17"] = "S_Sit_HandJob2",
            ["khh_f_32"] = "S_Stand_Handjob",
            ["khh_f_33"] = "S_Stand_Handjob_DeskV",
            ["khh_f_36"] = "S_Stand_PenisMassage",
            ["khh_f_49"] = "S_Stand_OnaHole",
            ["khh_f_52"] = "S_Stand_WallHandJob",
            ["khh_f_05"] = "S_Lay_KnobLick",
            ["khh_f_07"] = "S_Lay_BlowJob",
            ["khh_f_08"] = "S_Lay_HandedBlowJob",
            ["khh_f_21"] = "S_Sit_KnobLick",
            ["khh_f_22"] = "S_Sit_NoHandKnobLick",
            ["khh_f_24"] = "S_Sit_BlowJob",
            ["khh_f_35"] = "S_Stand_BallLick",
            ["khh_f_39"] = "S_Stand_BlowJob",
            ["khh_f_40"] = "S_Stand_HandedBlowJob",
            ["khh_f_42"] = "S_Stand_NoHandBlowJob",
            ["khh_f_50"] = "S_Sit_BenchBlowJob",
            ["khh_f_51"] = "S_Sit_SideBlowJob",
            ["khh_f_11"] = "S_Lay_Paizuri",
            ["khh_f_12"] = "S_Lay_Press_Paizuri",
            ["khh_f_13"] = "S_Lay_OnTop_Paizuri",
            ["khh_f_15"] = "S_Lay_Suck_Paizuri",
            ["khh_f_27"] = "S_Sit_Paizuri",
            ["khh_f_28"] = "S_Sit_Press_Paizuri",
            ["khh_f_30"] = "S_Sit_Lick_Paizuri",
            ["khh_f_31"] = "S_Sit_Suck_Paizuri",
            ["khh_f_43"] = "S_Stand_Paizuri",
            ["khh_f_44"] = "S_Stand_Press_Paizuri",
            ["khh_f_47"] = "S_Stand_Suck_Paizuri",
            ["khh_f_48"] = "S_Pool_Paizuri_sp",
            ["khn_f_00"] = "Peep_Toilet",
            ["khn_f_01"] = "Peep_Shower",
            ["kht_f_00"] = "M_Desk_Corner",
            ["kht_f_01"] = "M_Sit",
            ["kht_f_02"] = "M_Stand",
            ["kht_f_03"] = "M_OnHurdle",
            ["kht_f_04"] = "M_Lay_Masturbation_1",
            ["kht_f_05"] = "M_Wall_AnaFinger_1",
            ["kht_f_06"] = "M_Sit_AnaFinger_1",
            ["kht_f_07"] = "M_Desk_Finger_1",
            ["khr_f_00_00"] = "L_Lay_Interact",
            ["khr_f_01_00"] = "L_Sit_Cunni",
            ["khr_f_02_00"] = "L_Stand_Touch",
            ["khr_f_03_00"] = "L_Lay_Cunni_1",
            ["khr_f_05_01"] = "L_Lay_Cunni_2",
            ["khr_f_04_00"] = "L_Stand_Cunni_1",
            ["khr_f_06_01"] = "L_Stand_Cunni_2",
            ["khe_f_05_00"] = "S_Lay_Fingering",
            ["khe_f_01"] = "S_Lay_69",
            ["khe_f_02"] = "S_Lay_YingYang_1",
            ["khe_f_03"] = "S_Stand_Cunni",
            ["khe_f_04"] = "S_Lay_Cunni",
            ["khe_f_05"] = "S_Lay_RimJob_2",
            ["khe_f_06"] = "S_Sit_AnalFinger_1",
            ["khe_f_07"] = "S_Lay_FaceCunni_1",
            ["khe_f_08"] = "S_Wall_RimAndFing_1",
            ["khe_f_09"] = "S_Sit_Cunni_1"
        };
#endif

    }
}
