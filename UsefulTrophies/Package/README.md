![Splash](https://raw.githubusercontent.com/probablykory/useful-trophies/main/splash.jpg)  
Improve your stats by consuming trophies while reducing inventory clutter.  Originally by Khairex.

# Useful Trophies

Has this ever happened to you?

Chests filling up with trophies you don't know what to do with,
Your bonfire is already filled to the brim with "sacrifices" to your favorite god,
You are running out of places to store these heads!

All the while, you wish your stats were a bit higher...

Well look no further!

---

This mod gives you the ability to consume the heads of your enemies!
Each Trophy consumed will increase a random skill by how rare the trophy is!
The amount varies by trophy head, and can be configured.

Boss Trophies also have an extended use.
When a boss trophy is consumed, you gain their guardian buff for an extended duration!

You can also optionally consume the boss summon items for a shorter buff duration, put those ancient seeds to good use!

Cash Money!
Trophies now can also be sold for gold!
(Boss trophies by default have a value of 0 to prevent them from being sold. feel free to give them a reasonable value)

---

### Quick note from probablykory
Khairex's UsefulTrophies is one of my favorite mods, but after waiting a couple weeks to see if they'd update for Hildir, I decided to fork and republish.  Due to several changes, I've changed the plugin GUID, version and icon to reflect this mod's rebirth.  Please see changelog to see more details of my changes.

---
## Installation (manual)

If you are installing this manually, do the following

1. Extract the archive into a folder. **Do not extract into the game folder.**
2. Move the contents of `plugins` folder into `<GameDirectory>\BepInEx\plugins`.
3. Run the game.

---
### Configuration
For best results, edit the config using the in-game config mod `Configuration Manager`.

```
CanConsumeBossTrophies: you can turn this off to prevent boss trophies from being consumed. 
For those who worry about accidentally consuming their hard earned trophy before handing it in

CanConsumeBossSummonItems: Disable if you don't want to accidentally eat the items you need to summon bosses

BossPowerDuration: The amount of buff time consuming a boss trophy gives you

IsSellingEnabled: Disable if you don't want to sell trophies

Trophies: The list of trophy Prefab names to Experience values to Gold values.

SummoningItems: The list of summoning item Prefab names to Duration of boss power buff.
```

---

### Changelist

1.0.6
* Added a skills whitelist and some detection code to see which skills are maxed or have skillups locked.  Provides better compatibility with the Professions mod, and potentially other mods which lock XP skillups.

<details>
<summary><i>View changelog history</i></summary>
<br/>

1.0.5
* Replaced Skills list with a reverse-skill lookup relying on localization to provide the name of the skill.  This should work correctly with Jotunn and SkillManager based skills.

1.0.4
* Added additional 3rd party skills to custom skill list.

1.0.3
* Bugfix: Added several defenses against all the Null Reference exceptions people were seeing.
* Grammer corrections.
* Added a handful of Therzie's early trophies to the default config.
* Added Herbalist to the list of custom skills.

1.0.2
* Bugfix: added a proximity check to prevent consuming summoning item when used near offering bowl.

1.0.1
* Bugfix for intermittent exception when consuming summoning items.
* Bugfix for exception when consuming items via the hotbar.

1.0.0 
* Forked from khairex's github repo
* Changed configuration to use Prefab names instead of token strings, to support 3rd party mod trophies.
* Added a custom configuration drawer for the Trophies and Summoning Items lists.
* Corrected the skill-up message when consuming a trophy, to display the english skill name of 3rd party skills.

</details>

---

#### Donations
I spend countless hours every day working on, updating, and fixing mods for everyone to enjoy.  While I will never ask for anyone to pay me to make a mod or add a feature, any [support](https://paypal.me/probablyk) is greatly appreciated!

Alternatively, buy the original author of UsefulTrophies a [coffee](https://www.buymeacoffee.com/khairex)!
