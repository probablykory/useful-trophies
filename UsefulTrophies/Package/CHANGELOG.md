## Please always delete/move your old probablykory.UsefulTrophies.cfg file to apply the latest changes!

### 2.2.0
 * Added ServerSync, multiplayer support.
 * Added a config watcher, enabling realtime config updates, and filewatching on servers.

### 2.1.0
 * Updated for Ashlands, added new trophies and bosspower.  Delete your config if you're having issues with new trophies.
 * Large version bump to ensure bots (and sometimes people) realize this is the latest UsefulTrophies mod, not Khairex's or Maximus'

### 1.0.7
 * Fixed access exception a handful of players encountered with 1.0.6 update.

### 1.0.6
 * Added a skills whitelist and some detection code to see which skills are maxed or have skillups locked.  Provides better compatibility with the Professions mod, and potentially other mods which lock XP skillups.
 * Added a reset button to the F1 config to rerun the whitelist detection code.

### 1.0.5
 * Replaced Skills list with a reverse-skill lookup relying on localization to provide the name of the skill.  This should work correctly with Jotunn and SkillManager based skills.

### 1.0.4
 * Added additional 3rd party skills to custom skill list.

### 1.0.3
 * Bugfix: Added several defenses against all the Null Reference exceptions people were seeing.
 * Grammer corrections.
 * Added a handful of Therzie's early trophies to the default config.
 * Added Herbalist to the list of custom skills.

### 1.0.2
 * Bugfix: added a proximity check to prevent consuming summoning item when used near offering bowl.

### 1.0.1
 * Bugfix for intermittent exception when consuming summoning items.
 * Bugfix for exception when consuming items via the hotbar.

### 1.0.0 
 * Initial Version.  Forked from khairex's github repo
 * Changed configuration to use Prefab names instead of token strings, to support 3rd party mod trophies.
 * Added a custom configuration drawer for the Trophies and Summoning Items lists.
 * Corrected the skill-up message when consuming a trophy, to display the english skill name of 3rd party skills.
