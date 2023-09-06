using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace UsefulTrophies
{
    [HarmonyPatch]
    public static class Patches
    {
        [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui), typeof(Player), typeof(ItemDrop.ItemData)), HarmonyPrefix]
        private static bool InventoryGridUpdateGui(InventoryGrid __instance, Player player, ItemDrop.ItemData dragItem)
        {
            if (!UsefulTrophies.Instance.IsSellingEnabled) return true;

            if (player != null)
            {
                foreach (ItemDrop.ItemData item in player?.GetInventory()?.GetAllItems())
                {
                    string prefab = item?.m_dropPrefab?.name;
                    //Debug.Log($"item name set to {prefab}!");
                    if (UsefulTrophies.Instance.TrophyCoinValues.TryGetValue(prefab, out int value))
                    {
                        item.m_shared.m_value = value;
                    }
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetTooltip), typeof(ItemDrop.ItemData), typeof(int), typeof(bool), typeof(float)), HarmonyPostfix]
        public static string ItemDataGetTooltip(string __result, ItemDrop.ItemData item)
        {
            string result = __result;
            string name = item.m_dropPrefab.name;

            if (UsefulTrophies.Instance.TrophyXPGoldValues.Value.Contains(name) || (UsefulTrophies.Instance.CanConsumeBossSummonItems && UsefulTrophies.Instance.SecondaryBossPowerValues.ContainsKey(name)))
            {
                StringBuilder stringBuilder = new StringBuilder(256);
                stringBuilder.Append(__result);

                if (UsefulTrophies.Instance.TrophyXPValues.TryGetValue(name, out int xp))
                {
                    stringBuilder.Append(string.Format("\nApplies <color=orange>{0}</color> skill points", xp));
                    //Debug.Log($"{name} has XP {xp}!");
                }

                string bossPower = "";
                int duration = 0;
                if (UsefulTrophies.Instance.CanConsumeBossTrophies && UsefulTrophies.Instance.BossPowerDict.TryGetValue(name, out bossPower))
                {
                    duration = UsefulTrophies.Instance.BossPowerDuration;
                    //Debug.Log($"{name} has bosspower of {bossPower} {duration}!");
                }
                if ((bossPower.IsNullOrWhiteSpace() || duration == 0) &&
                    UsefulTrophies.Instance.CanConsumeBossSummonItems &&
                    UsefulTrophies.Instance.SecondaryPowerDict.TryGetValue(name, out bossPower))
                {
                    UsefulTrophies.Instance.SecondaryBossPowerValues.TryGetValue(name, out duration);
                    //Debug.Log($"{name} has secondary power of {bossPower} {duration}!");
                }

                if (!bossPower.IsNullOrWhiteSpace() && duration > 0)
                {
                    bossPower = bossPower.Substring(3);
                    if (xp > 0)
                    {
                        stringBuilder.Append("\n");
                    }
                    stringBuilder.Append("\nContains the power of <color=orange>" + bossPower + "</color>");

                }

                result = __result = stringBuilder.ToString();
                //Debug.Log($"{name} tooltip set to {result}!");
            }

            return result;
        }


        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UseItem)), HarmonyPrefix]
        private static bool HumanoidUseItem(Humanoid __instance, Inventory inventory, ItemDrop.ItemData item, bool fromInventoryGui, Inventory ___m_inventory, ZSyncAnimation ___m_zanim)
        {
            string prefabName = item.m_dropPrefab.name;

            if (inventory == null)
            {
                inventory = ___m_inventory;
            }
            if (!inventory.ContainsItem(item))
            {
                return false;
            }

            if (UsefulTrophies.Instance.CanConsumeBossSummonItems && UsefulTrophies.Instance.SecondaryPowerDict.TryGetValue(prefabName, out string powerName))
            {
                StatusEffect bossPower = ObjectDB.instance.GetStatusEffect(powerName.GetStableHashCode());
                if (!UsefulTrophies.Instance.SecondaryBossPowerValues.TryGetValue(prefabName, out int powerTime))
                {
                    powerTime = 120;
                }
                
                ApplyStatusEffect(bossPower.Clone(), (float)powerTime, __instance.transform.position);

                if (!UsefulTrophies.Instance.TrophyXPGoldValues.Value.Contains(prefabName))
                {
                    // Consume Item 
                    Debug.Log($"Consume {prefabName} Secondary Boss Item!");
                    inventory.RemoveOneItem(item);
                    __instance.m_consumeItemEffects.Create(Player.m_localPlayer.transform.position, Quaternion.identity, null, 1f, -1);
                    ___m_zanim.SetTrigger("eat");
                    return false;
                }
            }
            
            // Only Override function for Trophy Items
            if (UsefulTrophies.Instance.TrophyXPGoldValues.Value.Contains(prefabName))
            {
                //string enemy = itemName.Substring(13);

                if (!UsefulTrophies.Instance.CanConsumeBossTrophies) return true;
                    
                if (UsefulTrophies.Instance.BossPowerDict.TryGetValue(prefabName, out powerName))
                {
                    StatusEffect bossPower = ObjectDB.instance.GetStatusEffect(powerName.GetStableHashCode());
                            
                    // Protection against eating boss heads near unactivated boss stones
                    BossStone[] bossStones = GameObject.FindObjectsOfType<BossStone>();
                        
                    // If we are near any boss stones, the find will find some
                    if (bossStones.Length > 1 && bossStones[0].m_itemStand.m_netViewOverride.IsValid())
                    {
                        // There may be duplicate stones, so find at least one active stone to confirm we have it handed in
                        bool bossActived = false;
                        foreach (var stone in bossStones)
                        {
                            if (stone.IsActivated() && bossPower.m_name == stone.m_itemStand.m_guardianPower.m_name)
                            {
                                bossActived = true;
                                break;
                            }
                        }

                        if (!bossActived)
                        {
                            Debug.Log("Prevented Player from consuming boss trophy near BossStone");
                            return true;
                        }
                    }
                        
                    // Copy power so we dont effect the original data
                    ApplyStatusEffect(bossPower.Clone(), UsefulTrophies.Instance.BossPowerDuration, __instance.transform.position);
                }
                                
                // Prioritize Hover Objects (item stands/altars)
                GameObject hoverObject = __instance.GetHoverObject();
                Hoverable hoverable = hoverObject ? hoverObject.GetComponentInParent<Hoverable>() : null;
                if (hoverable != null && !fromInventoryGui)
                {
                    Interactable componentInParent = hoverObject.GetComponentInParent<Interactable>();
                    if (componentInParent != null && componentInParent.UseItem(__instance, item))
                    {
                        return false;
                    }
                }

                Debug.Log($"Consume {prefabName}!");

                // Get a Random Skill from the Player's Skill Pool
                List<Skills.Skill> skills = __instance.GetSkills().GetSkillList();
                Skills.Skill randomSkill = skills[UnityEngine.Random.Range(0, skills.Count)];
                string skillName = Utils.FromSkill(randomSkill.m_info.m_skill);

                float skillFactor = 10f;
                if (UsefulTrophies.Instance.TrophyXPValues.TryGetValue(prefabName, out int dictSkillFactor))
                {
                    skillFactor = (float)dictSkillFactor;
                }
                else
                {
                    Debug.Log($"No XP value for prefab {prefabName}!");
                }
                
                Debug.Log($"Raising {skillName} by {skillFactor}");

                float req = GetNextLevelRequirement(randomSkill) - randomSkill.m_accumulator;
                
                // Increase Skill by Configured Skill Factor
                __instance.RaiseSkill(randomSkill.m_info.m_skill, skillFactor);
                skillFactor -= req;

                // Handle multi-levelUps
                while (skillFactor > 0f)
                {
                    req = GetNextLevelRequirement(randomSkill);
                    __instance.RaiseSkill(randomSkill.m_info.m_skill, skillFactor);
                    skillFactor -= req;
                }

                // Consume Item 
                inventory.RemoveOneItem(item);
                __instance.m_consumeItemEffects.Create(Player.m_localPlayer.transform.position, Quaternion.identity, null, 1f, -1);
                ___m_zanim.SetTrigger("eat");

                // Notify Player of the Stat Increase
                __instance.Message(MessageHud.MessageType.Center, $"You increase skill with {skillName}", 0, null);
                return false;
            }

            return true;
        }

        //Player.GetPlayersInRange is now private static - use reflection
        private static MethodInfo getPlayersInRange = null;
        private static void ApplyStatusEffect(StatusEffect statusEffect, float time, Vector3 position)
        {
            if (getPlayersInRange == null)
            {
                getPlayersInRange = typeof(Player).GetMethod(nameof(Player.GetPlayersInRange), BindingFlags.Static | BindingFlags.NonPublic);
            }

            statusEffect.m_ttl = time;
            List<Player> players = new List<Player>();

            getPlayersInRange?.Invoke(null, new object[] { position, 10f, players });
            foreach (Player player in players)
            {
                player.GetSEMan().AddStatusEffect(statusEffect, true);
            }
        }
        
        private static float GetNextLevelRequirement(Skills.Skill skill) => (float) ((double) Mathf.Pow(skill.m_level + 1f, 1.5f) * 0.5 + 0.5);
    }
}
