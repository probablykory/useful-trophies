﻿using BepInEx.Bootstrap;
using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ServerSync;
using System.IO;
using UnityEngine.Rendering;

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
        ConfigSync ConfigSync { get; }
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

        public static ConfigEntry<T> Config<T>(this IPlugin self, string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);

            ConfigEntry<T> configEntry = self.Config.Bind(group, name, value, extendedDescription);
            SyncedConfigEntry<T> syncedConfigEntry = self.ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        public static ConfigEntry<T> Config<T>(this IPlugin instance, string group, string name, T value, string description, bool synchronizedSetting = true) => Config(instance, group, name, value, new ConfigDescription(description, null, GetTags()), synchronizedSetting);


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
                        newBossPowers.Add(new BossPowerConfig { Prefab = "<Prefab Name>", Duration = 120 });
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

        public static Action<ConfigEntryBase> DrawConfigActionButton(string buttonName, Action buttonAction)
        {
            return cfg =>
            {
                GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());

                if (GUILayout.Button(buttonName, new GUILayoutOption[] { GUILayout.ExpandWidth(true) }))
                {
                    if (buttonAction != null)
                    {
                        buttonAction();
                    }
                }
                
                GUILayout.EndVertical();
            };
        }

    }


    public class ConfigWatcher
    {
        private BaseUnityPlugin configurationManager;
        private IPlugin plugin;

        public ConfigWatcher(IPlugin plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            this.plugin = plugin;
            CheckForConfigManager();
        }

        private void InitializeWatcher()
        {
            string file = Path.GetFileName(plugin.Config.ConfigFilePath);
            string path = Path.GetDirectoryName(plugin.Config.ConfigFilePath);

            var watcher = new Watcher(path, file);
            watcher.FileChanged += OnFileChanged;
        }

        private void CheckForConfigManager()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) //isHeadless
            {
                InitializeWatcher();
            }
            else
            {
                PluginInfo configManagerInfo;
                if (Chainloader.PluginInfos.TryGetValue("com.bepis.bepinex.configurationmanager", out configManagerInfo) && configManagerInfo.Instance)
                {
                    this.configurationManager = configManagerInfo.Instance;
                    EventInfo eventinfo = this.configurationManager.GetType().GetEvent("DisplayingWindowChanged");
                    if (eventinfo != null)
                    {
                        Action<object, object> local = new Action<object, object>(this.OnConfigManagerDisplayingWindowChanged);
                        Delegate converted = Delegate.CreateDelegate(eventinfo.EventHandlerType, local.Target, local.Method);
                        eventinfo.AddEventHandler(this.configurationManager, converted);
                    }
                }
                else
                {
                    InitializeWatcher();
                }
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            string path = plugin.Config.ConfigFilePath;

            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                plugin.Config.SaveOnConfigSet = false;
                plugin.Config.Reload();
                plugin.Config.SaveOnConfigSet = true;
            }
            catch
            {
                Debug.LogError("There was an issue with your " + Path.GetFileName(path) + " file.");
                Debug.LogError("Please check the format and spelling.");
                return;
            }
        }

        private void OnConfigManagerDisplayingWindowChanged(object sender, object e)
        {
            PropertyInfo pi = this.configurationManager.GetType().GetProperty("DisplayingWindow");
            bool cmActive = (bool)pi.GetValue(this.configurationManager, null);

            if (!cmActive)
            {
                plugin.Config.SaveOnConfigSet = false;
                plugin.Config.Reload();
                plugin.Config.SaveOnConfigSet = true;
            }
        }
    }

    public class Watcher
    {
        public event Action<object, FileSystemEventArgs>? FileChanged;

        public bool EnableRaisingEvents
        {
            get { return fileSystemWatcher == null ? false : fileSystemWatcher.EnableRaisingEvents; }
            set { if (fileSystemWatcher != null) { fileSystemWatcher.EnableRaisingEvents = value; } }
        }

        private FileSystemWatcher fileSystemWatcher = null!;

        public Watcher(string path, string filter)
        {
            if (path == null) { throw new ArgumentNullException("path"); }
            if (filter == null) { throw new ArgumentNullException("filter"); }

            fileSystemWatcher = new FileSystemWatcher(path, filter);
            fileSystemWatcher.Changed += OnCreatedChangedOrRenamed;
            fileSystemWatcher.Created += OnCreatedChangedOrRenamed;
            fileSystemWatcher.Renamed += OnCreatedChangedOrRenamed;
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void OnCreatedChangedOrRenamed(object sender, FileSystemEventArgs args)
        {
            FileChanged?.Invoke(sender, args);
        }
    }
}