using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsefulTrophies
{
    public static class Utils
    {
        public static string FromSkill(Skills.SkillType skill)
        {
            return Localization.instance.Localize("$skill_" + skill.ToString().ToLower());
        }

        public static Skills.SkillType FromName(string englishName) => (Skills.SkillType)Math.Abs(englishName.GetStableHashCode());
    }
}

