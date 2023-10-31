using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsefulTrophies
{
    public static class Utils
    {
        // Keeping, but for now appears to be unnecessary
        //public static List<string> CustomSkills = new List<string>()
        //{
        //    "Agility",
        //    "Alchemy",
        //    "Blacksmithing",
        //    "Building",
        //    "Cooking",
        //    "Diving",
        //    "Dual Axes",
        //    "Dual Clubs",
        //    "Dual Knives",
        //    "Dual Offhand",
        //    "Dual Swords",
        //    "Enchantment",
        //    "Endurance",
        //    "Evasion",
        //    "Exploration",
        //    "Farming",
        //    "Fermenting",
        //    "Foraging",
        //    "Herbalist",
        //    "Hunting",
        //    "Jewelcrafting",
        //    "Lumberjacking",
        //    "Mining",
        //    "PackHorse",
        //    "Ranching",
        //    "Sailing",
        //    "Tenacity",
        //    "ThirdEye",
        //    "Vitality",
        //};

        public static string FromSkill(Skills.SkillType skill)
        {
            return Localization.instance.Localize("$skill_" + skill.ToString().ToLower());
        }

        public static Skills.SkillType FromName(string englishName) => (Skills.SkillType)Math.Abs(englishName.GetStableHashCode());
    }
}

