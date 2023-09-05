using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsefulTrophies
{

    // TODO - Search out any additional skill mods to ensure names display correctly
    public enum CustomSkills
    {
        Alchemy,
        Blacksmithing,
        Building,
        Cooking,
        Evasion,
        Exploration,
        Farming,
        Foraging,
        Jewelcrafting,
        Lumberjacking,
        Mining,
        PackHorse,
        Ranching,
        Sailing,
        Tenacity,
        Vitality,
    }

    public static class Utils
    {
        public static string FromSkill(Skills.SkillType skill)
        {
            foreach (var s in ((CustomSkills[])Enum.GetValues(typeof(CustomSkills))).Select(s => s.ToString()))
            {
                if (skill == FromName(s))
                {
                    return s;
                }
            }
            return skill.ToString();
        }

        public static Skills.SkillType FromName(string englishName) => (Skills.SkillType)Math.Abs(englishName.GetStableHashCode());
    }
}

