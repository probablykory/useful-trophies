using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace UsefulTrophies
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class UsefulTrophies : BaseUnityPlugin, IPlugin
    {
        public const string PluginAuthor = "probablykory";
        public const string PluginName = "UsefulTrophies";
        public const string PluginVersion = "1.0.6";
        public const string PluginGUID = PluginAuthor + "." + PluginName;

        public Harmony Harmony { get; } = new Harmony(PluginGUID);
        public static UsefulTrophies Instance;


        public List<TrophyConfig> TrophyConfigs = new List<TrophyConfig>();
        public Dictionary<string, int> TrophyCoinValues = new Dictionary<string, int>();
        public Dictionary<string, int> TrophyXPValues = new Dictionary<string, int>();
        public Dictionary<string, int> SecondaryBossPowerValues = new Dictionary<string, int>();


        public ConfigEntry<bool> CanConsumeBossSummonItemsEntry { get; set; } = null;
        public bool CanConsumeBossSummonItems { get {
            return (bool)(CanConsumeBossSummonItemsEntry?.Value) ? true : false;
        } }

        public ConfigEntry<bool> CanConsumeBossTrophiesEntry { get; set; } = null;
        public bool CanConsumeBossTrophies { get {
            return (bool)(CanConsumeBossTrophiesEntry?.Value) ? true : false;
        } }
        public ConfigEntry<bool> IsSellingEnabledEntry { get; set; } = null;
        public bool IsSellingEnabled { get {
            return (bool)(IsSellingEnabledEntry?.Value) ? true : false;
        } }
        public ConfigEntry<int> BossPowerDurationEntry { get; set; } = null;
        public int BossPowerDuration { get {
            return BossPowerDurationEntry?.Value > 0 ? (int)BossPowerDurationEntry?.Value : 0;
        } }

        public ConfigEntry<string> TrophyXPGoldValues { get; set; } = null;
        public ConfigEntry<string> SecondaryBossPowerDurations { get; set; } = null;

        public const string DefaultTrophyXPGoldValues =
            "TrophyDeer:8:15," +
            "TrophyBoar:4:10," +
            "TrophyNeck:10:10," +
            "TrophyGreydwarf:5:10," +
            "TrophyFox_TW:15:20," + 
            "TrophyGreydwarfBrute:25:15," +
            "TrophyGreydwarfShaman:15:15," +
            "TrophyRazorback_TW:15:15," +
            "TrophyBlackBear_TW:15:15," + 
            "TrophySkeleton:10:10," +
            "TrophySkeletonPoison:25:15," +
            "TrophyFrostTroll:30:50," +
            "TrophySurtling:25:15," +
            "TrophyLeech:25:15," +
            "TrophyDraugr:20:15," +
            "TrophyDraugrElite:30:30," +
            "TrophyBlob:20:20," +
            "TrophyRottingElk_TW:20:20," + 
            "TrophyWraith:30:30," +
            "TrophyAbomination:100:100," +
            "TrophyWolf:35:25," +
            "TrophyFenring:40:30," +
            "TrophyHatchling:40:35," +
            "TrophySGolem:50:500," +
            "TrophyUlv:50:50," +
            "TrophyCultist:50:100," +
            "TrophyGoblin:50:35," +
            "TrophyGoblinBrute:50:50," +
            "TrophyGoblinShaman:50:50," +
            "TrophyLox:120:200," +
            "TrophyGrowth:50:50," +
            "TrophyDeathsquito:80:25," +
            "TrophySerpent:150:250," +
            "TrophyHare:50:40," +
            "TrophyGjall:100:150," +
            "TrophyTick:80:50," +
            "TrophyDvergr:300:300," +
            "TrophySeeker:150:50," +
            "TrophySeekerBrute:300:100," +
            "TrophyEikthyr:30:0," +
            "TrophyTheElder:80:0," +
            "TrophyBonemass:300:0," +
            "TrophyDragonQueen:500:0," +
            "TrophyGoblinKing:700:0," +
            "TrophySeekerQueen:1000:0," +
            "TrophySkeletonHildir:80:100," +
            "TrophyCultist_Hildir:300:200," +
            "TrophyGoblinBruteBrosBrute:600:300," +
            "TrophyGoblinBruteBrosShaman:600:300";

        public const string DefaultSecondaryBossPowerDurations =
            "TrophyDeer:120," +
            "AncientSeed:120," +
            "WitheredBone:120," +
            "DragonEgg:300," +
            "GoblinTotem:120," +
            "DvergrKeyFragment:120";
        
        public Dictionary<string, string> BossPowerDict = new Dictionary<string, string>()
        {
            {"TrophyEikthyr", "GP_Eikthyr"},
            {"TrophyTheElder", "GP_TheElder"},
            {"TrophyBonemass", "GP_Bonemass"},
            {"TrophyDragonQueen", "GP_Moder"},
            {"TrophyGoblinKing", "GP_Yagluth"},
            {"TrophySeekerQueen", "GP_Queen"},
        };
        
        public Dictionary<string, string> SecondaryPowerDict = new Dictionary<string, string>()
        {
            {"TrophyDeer", "GP_Eikthyr"},
            {"AncientSeed", "GP_TheElder"},
            {"WitheredBone", "GP_Bonemass"},
            {"DragonEgg", "GP_Moder"},
            {"GoblinTotem", "GP_Yagluth"},
            {"DvergrKeyFragment", "GP_Queen"}
        };

        public List<Skills.SkillType> WhitelistedSkills = new List<Skills.SkillType>();
        
        private void Awake()
        {
            Instance = this;

            var category = "General";

            CanConsumeBossSummonItemsEntry  = Instance.Config(category, "CanConsumeBossSummonItems", true, "Allows you to consume boss summoning items for a short boss power buff.");
            CanConsumeBossTrophiesEntry = Instance.Config(category, "CanConsumeBossTrophies", true, "Allows you to consume boss trophies.");
            BossPowerDurationEntry = Instance.Config(category, "BossPowerDuration", 720, "The duration of boss power buff when you consume its trophy.");
            IsSellingEnabledEntry = Instance.Config(category, "IsSellingEnabled", true, "Allows you to sell trophies to traders.");
            TrophyXPGoldValues = Instance.Config(category, "Trophies", DefaultTrophyXPGoldValues,
                 new ConfigDescription($"The trophy prefab names and their XP and Gold values.",
                 new AcceptableValueConfigNote("You must use valid spawn item codes or this will not work."),
                 ConfigHelper.GetTags(ConfigDrawers.DrawTrophyConfigTable())));
            SecondaryBossPowerDurations = Instance.Config(category, "SummoningItems", DefaultSecondaryBossPowerDurations,
                 new ConfigDescription($"The summon item prefab names and duration values.",
                 new AcceptableValueConfigNote("You must use valid spawn item codes or this will not work."),
                 ConfigHelper.GetTags(ConfigDrawers.DrawBossPowerConfigTable())));

            Instance.Config(category, "resetWhitelist", 0, 
                new ConfigDescription("", null, new object[] {
                new ConfigurationManagerAttributes
                {
                    HideSettingName = new bool?(true),
                    HideDefaultButton = new bool?(true),
                    CustomDrawer = new Action<ConfigEntryBase>(ConfigDrawers.DrawConfigActionButton("Reset list of boostable skills", new Action(() => Instance.DetectAndWhitelistSkills())))
                }}));

            TrophyXPGoldValues.SettingChanged += RefreshDictionaries;
            SecondaryBossPowerDurations.SettingChanged += RefreshDictionaries;
            RefreshDictionaries(null, null);

            Harmony.PatchAll();
        }

        private void RefreshDictionaries(object sender, EventArgs e)
        {
            List<TrophyConfig> trophies = TrophyEntry.Deserialize(Instance.TrophyXPGoldValues.Value).ToList();
            List<BossPowerConfig> powers = BossPowerEntry.Deserialize(Instance.SecondaryBossPowerDurations.Value).ToList();

            TrophyCoinValues = trophies.ToDictionary(t => t.Prefab, t => t.Value);
            TrophyXPValues = trophies.ToDictionary(t => t.Prefab, t => t.Experience);
            SecondaryBossPowerValues = powers.ToDictionary(t => t.Prefab, t => t.Duration);
        }

        public void DetectAndWhitelistSkills(Player player = null)
        {
            if (player == null)
                player = Player.m_localPlayer;
            if (player == null)
                return;

            Instance.WhitelistedSkills.Clear();

            Skills skills = player.GetSkills();
            float level;
            float accumulator;

            FieldInfo msiField = AccessTools.Field(typeof(MessageHud), nameof(MessageHud.m_instance));
            MessageHud hudInstance = msiField.GetValue(null) as MessageHud;
            msiField.SetValue(null, null);
            EffectList playerLvlEffects = player.m_skillLevelupEffects;
            player.m_skillLevelupEffects = new EffectList();

            try
            {
                foreach (Skills.Skill skill in skills.GetSkillList())
                {
                    level = skill.m_level;
                    accumulator = skill.m_accumulator;

                    skills.RaiseSkill(skill.m_info.m_skill);

                    if (level == skill.m_level && accumulator == skill.m_accumulator)
                    {
                        skill.m_level = level;
                        skill.m_accumulator = accumulator;
                    }
                    else
                    {
                        Instance.WhitelistedSkills.Add(skill.m_info.m_skill);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Caught error while detecting skill whitelist. Exception:\n{ex}");
                Instance.WhitelistedSkills.Clear();
                Instance.WhitelistedSkills.AddRange(skills.GetSkillList().Select(s => s.m_info.m_skill));
            }
            finally
            {
                msiField.SetValue(null, hudInstance);
                player.m_skillLevelupEffects = playerLvlEffects;
            }
        }
    }
}
