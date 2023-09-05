using BepInEx.Bootstrap;
using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UsefulTrophies
{
    public class TrophyConfig
    {
        public string Prefab;
        public int Experience = 0;
        public int Value = 0;
    }

    public static class TrophyEntry
    {
        public static TrophyConfig[] Deserialize(string trophies)
        {
            return trophies.Split(',').Select(r =>
            {
                string[] parts = r.Split(':');
                return new TrophyConfig
                {
                    Prefab = parts[0],
                    Experience = parts.Length > 1 && int.TryParse(parts[1], out int exp) ? exp : 1,
                    Value = parts.Length > 2 && int.TryParse(parts[2], out int val) ? val : 0
                };
            }).ToArray();
        }

        public static string Serialize(TrophyConfig[] trophies)
        {
            return string.Join(",", trophies.Select(r =>
                r.Value > 0 ?
                    $"{r.Prefab}:{r.Experience}:{r.Value}" :
                    $"{r.Prefab}:{r.Experience}"));
        }
    }

    public class BossPowerConfig
    {
        public string Prefab;
        public int Duration = 0;
    }

    public static class BossPowerEntry
    {
        public static BossPowerConfig[] Deserialize(string powers)
        {
            return powers.Split(',').Select(r =>
            {
                string[] parts = r.Split(':');
                return new BossPowerConfig
                {
                    Prefab = parts[0],
                    Duration = parts.Length > 1 && int.TryParse(parts[1], out int duration) ? duration : 1
                };
            }).ToArray();
        }

        public static string Serialize(BossPowerConfig[] powers)
        {
            return string.Join(",", powers.Select(r =>
                    $"{r.Prefab}:{r.Duration}"));
        }
    }

    public interface IPlugin
    {
        ConfigFile Config { get; }
    }

    // use bepinex ConfigEntry settings
    internal static class ConfigHelper
    {
        public static ConfigEntry<string> ItemRequirementsConfig() { return null; }

        //public static ConfigurationManagerAttributes GetAdminOnlyFlag(bool isAdminOnly = false) { return GetAdminOnlyFlag(); }

        public static ConfigurationManagerAttributes GetTags(Action<ConfigEntryBase> action)
        {
            return new ConfigurationManagerAttributes() { CustomDrawer = action};
        }

        public static ConfigurationManagerAttributes GetTags()
        {
            return new ConfigurationManagerAttributes();
        }

        public static ConfigEntry<T> Config<T>(this IPlugin instance, string group, string name, T value, ConfigDescription description)
        {
            return instance.Config.Bind(group, name, value, description);
        }

        public static ConfigEntry<T> Config<T>(this IPlugin instance, string group, string name, T value, string description) => Config(instance, group, name, value, new ConfigDescription(description, null, GetTags()));
    }

    public class AcceptableValueConfigNote : AcceptableValueBase
    {
        public virtual string Note { get; }

        public AcceptableValueConfigNote(string note) : base(typeof(string))
        {
            if (string.IsNullOrEmpty(note))
            {
                throw new ArgumentException("A string with atleast 1 character is needed", "Note");
            }
            this.Note = note;
        }

        // passthrough overrides
        public override object Clamp(object value) { return value; }
        public override bool IsValid(object value) { return !string.IsNullOrEmpty(value as string); }

        public override string ToDescriptionString()
        {
            return "# Note: " + Note;
        }
    }

    public static class ConfigDrawers
    {
        private static BaseUnityPlugin configManager = null;

        private static BaseUnityPlugin GetConfigManager()
        {
            if (ConfigDrawers.configManager == null)
            {
                PluginInfo configManagerInfo;
                if (Chainloader.PluginInfos.TryGetValue("com.bepis.bepinex.configurationmanager", out configManagerInfo) && configManagerInfo.Instance)
                {
                    ConfigDrawers.configManager = configManagerInfo.Instance;
                }
            }

            return ConfigDrawers.configManager;
        }

        private static int GetRightColumnWidth()
        {
            int result = 130;
            BaseUnityPlugin configManager = GetConfigManager();
            if (configManager != null)
            {
                PropertyInfo pi = configManager?.GetType().GetProperty("RightColumnWidth", BindingFlags.Instance | BindingFlags.NonPublic);
                if (pi != null)
                {
                    result = (int)pi.GetValue(configManager);
                }
            }

            return result;
        }

        public static Action<ConfigEntryBase> DrawTrophyConfigTable()
        {
            return cfg =>
            {
                List<TrophyConfig> newTrophies = new List<TrophyConfig>();
                bool wasUpdated = false;

                int RightColumnWidth = GetRightColumnWidth();

                GUILayout.BeginVertical();

                List<TrophyConfig> trophies = TrophyEntry.Deserialize((string)cfg.BoxedValue).ToList();

                foreach (var trophy in trophies)
                {
                    GUILayout.BeginHorizontal();

                    string newPrefab = GUILayout.TextField(trophy.Prefab, new GUIStyle(GUI.skin.textField) { fixedWidth = RightColumnWidth - 40 - 40 - 21 - 21 - 9 });
                    string prefabName = string.IsNullOrEmpty(newPrefab) ? trophy.Prefab : newPrefab;
                    wasUpdated = wasUpdated || prefabName != trophy.Prefab;


                    int xp = trophy.Experience;
                    if (int.TryParse(GUILayout.TextField(xp.ToString(), new GUIStyle(GUI.skin.textField) { fixedWidth = 40 }), out int newXP) && newXP != xp)
                    {
                        xp = newXP;
                        wasUpdated = true;
                    }

                    int gold = trophy.Value;
                    if (int.TryParse(GUILayout.TextField(gold.ToString(), new GUIStyle(GUI.skin.textField) { fixedWidth = 40 }), out int newValue) && newValue != xp)
                    {
                        gold = newValue;
                        wasUpdated = true;
                    }

                    if (GUILayout.Button("x", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }))
                    {
                        wasUpdated = true;
                    }
                    else
                    {
                        newTrophies.Add(new TrophyConfig { Prefab = prefabName, Experience = xp, Value = gold });
                    }

                    if (GUILayout.Button("+", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }))
                    {
                        wasUpdated = true;
                        newTrophies.Add(new TrophyConfig { Prefab = "<Prefab Name>", Experience = 10, Value = 10 });
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();

                if (wasUpdated)
                {
                    cfg.BoxedValue = TrophyEntry.Serialize(newTrophies.ToArray());
                }
            };
        }

        public static Action<ConfigEntryBase> DrawBossPowerConfigTable()
        {
            return cfg =>
            {
                List<BossPowerConfig> newBossPowers = new List<BossPowerConfig>();
                bool wasUpdated = false;

                int RightColumnWidth = GetRightColumnWidth();

                GUILayout.BeginVertical();

                List<BossPowerConfig> bossPowers = BossPowerEntry.Deserialize((string)cfg.BoxedValue).ToList();

                foreach (var bossPower in bossPowers)
                {
                    GUILayout.BeginHorizontal();

                    string newPrefab = GUILayout.TextField(bossPower.Prefab, new GUIStyle(GUI.skin.textField) { fixedWidth = RightColumnWidth - 40 - 21 - 21 - 9 });
                    string prefabName = string.IsNullOrEmpty(newPrefab) ? bossPower.Prefab : newPrefab;
                    wasUpdated = wasUpdated || prefabName != bossPower.Prefab;


                    int duration = bossPower.Duration;
                    if (int.TryParse(GUILayout.TextField(duration.ToString(), new GUIStyle(GUI.skin.textField) { fixedWidth = 40 }), out int mewDuration) && mewDuration != duration)
                    {
                        duration = mewDuration;
                        wasUpdated = true;
                    }

                    if (GUILayout.Button("x", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }))
                    {
                        wasUpdated = true;
                    }
                    else
                    {
                        newBossPowers.Add(new BossPowerConfig { Prefab = prefabName, Duration = duration });
                    }

                    if (GUILayout.Button("+", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }))
                    {
                        wasUpdated = true;
                        newBossPowers.Add(new BossPowerConfig { Prefab = "", Duration = 120 });
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();

                if (wasUpdated)
                {
                    cfg.BoxedValue = BossPowerEntry.Serialize(newBossPowers.ToArray());
                }
            };
        }

    }
}