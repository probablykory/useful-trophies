![Splash](https://raw.githubusercontent.com/probablykory/useful-trophies/main/splash.jpg)  
Improve your stats while reducing inventory clutter by consuming trophies.  Now with ServerSync!

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
For best results, edit the config using the in-game config mod `Configuration Manager`.  Now using ServerSync!

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

#### Donations
I spend countless hours every day working on, updating, and fixing mods for everyone to enjoy.  While I will never ask for anyone to pay me to make a mod or add a feature, any [support](https://paypal.me/probablyk) is greatly appreciated!
