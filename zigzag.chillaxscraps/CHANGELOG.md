# 1.3.0 The Ocarina update
- **Added**
    - Added a custom effect for every song of the Ocarina !
        - Song effects are kept secret... You need to play the song yourself to discover the behaviour !
        - All songs have a specific number of allowed usage per moons (some are 1 time use, some are 2). This can be disabled in the config but it's preferable to not modify this as it will become unbalanced
        - Thanks [Mrov](https://thunderstore.io/c/lethal-company/p/mrov/) for the lightning bolts code !
        - Thanks [A Glitched Npc](https://www.twitch.tv/a_glitched_npc) for testing and for effect ideas !
- **Fixed**
    - The Ocarina animation got a rework, hopefully it's working great now
    - Fixed Ocarina still playing audio if you cancel it by using the ship's terminal or when you drop it in the ship's cupboard

## 1.2.4
- **Fixed**
    - Fixed every damage and heal not working as intended if (somehow) you have more than max health

## 1.2.3
- **Updated**
    - Changed how The Master Sword reacts to unworthy players

## 1.2.2
- **Updated**
    - Added a new config "Ocarina unique songs", false by default. You can activate it to give every player a randomly selected song assigned to them (note that with this enabled, it's not possible to select other songs anymore)
- **Fixed**
    - I tried another fix for the Ocarina animation, but this time it's stronger
    - [lunxara](https://www.twitch.tv/lunxara) has reported that it's possible to use the Death Note on players that are no longer in the lobby, I didn't find a way to replicate this issue but I still modified the code to hopefully fix it

## 1.2.1
- **Updated**
    - The feature of the Boink added in the last update *"Have a small chance of launching you in the wrong direction"* has been reverted by default, but can be re-enabled with the newly added "Evil Boink" config
- **Fixed**
    - Fixed The Master Sword dropping all your items when you are unworthy
    - [a_glitched_npc](https://www.twitch.tv/a_glitched_npc) has reported that the Death Note UI is displayed for other players when the item is used by the host, I didn't find a way to replicate this issue but I still modified the code to hopefully fix it (but it's probably a mod incompatibility thing)
- **Information**
    - It appears that the jump boost effect given by Super Sneakers does not work when you have [BetterStamina](https://thunderstore.io/c/lethal-company/p/FlipMods/BetterStamina/) installed, I'm still searching a way to fix that

# 1.2.0 Improvements
- **Added**
    - Imported from the original ChillaxScraps mod: Totem of Undying
- **Updated**
    - Totem of Undying
        - The code for this item is completly new, it now works exactly like in Minecraft
        - Multiple fix that I can't remember but trust me there is no issues 😎
    - Boink
        - Now requires battery to be used
        - Have a small chance of launching you in the wrong direction
        - Audio is now properly assigned to the item
- **Fixed**
    - I tried a fix for the Ocarina animation (in particular, the rotation of the item when you use it)

##

<details><summary>Old versions (click to reveal)</summary>

###

## 1.1.1
- **Updated**
    - Added a config to set The Master Sword's damage
    - Added custom scrap icons to Eevee, Froggy Chair and Moai Statue
    - Changed how music is played with the Ocarina: you now have to hold the button to play a sound and it will be stopped when you release it

# 1.1.0 New scraps
- **Added**
    - Imported from the original ChillaxScraps mod: Emergency meeting, Super Sneakers, The Master Sword and Ocarina
- **Updated**
    - Emergency meeting
        - Using it in orbit or if there is no players in the facility will cancel the effect and display a message
        - Updated material values
    - Super Sneakers
        - You can now activate or deactivate the jump boost effect by using the item : this consumes battery over time but can be charged in the ship
        - When activated, putting the item in your pocket will keep the effect active, this will only reset when droped, deactivated, when out of batteries, or on certain conditions
    - The Master Sword
        - Now with a custom effect : only the hero can grab and use the sword 🙂
        - It's supposed to be the sword that banished evil so it now deals more damage
        - Changed sound to be the ones from Zelda OoT
        - Updated material values
    - Ocarina
        - Now with a special animation when playing music with it
        - You can now select what song to play (small music notes if none are selected)
        - Tweaked sounds volume and added new ones
        - Model and texture have been reworked
    - Death Note
        - Changed how control tips are displayed to the local player and modified some messages
        - If you try to use it in orbit, you will now be punished
        - Removed daytime entities from the targetable enemies list
    - Moai Statue
        - Updated material values to make it look better
    - Misc
        - Updated to v65/v66
        - All new items have received some various fix (similarly to the last update)

## 1.0.1
- **Fixed**
    - Fixed Death Note and Cup Noodle audio beeing played on the host player instead of the local player

# 1.0.0 Initial release
- **Added**
    - Imported from the original ChillaxScraps mod: Death Note, Boink, Eevee, Cup Noodle, Moai Statue, Uno Reverse Card and Froggy Chair
- **Updated**
    - Death Note
        - Can be used multiple times, one use per player, so watch out for your friends 🤫
        - Info message is displayed if you try to use it in orbit
    - Eevee
        - Updated grab animation
    - Cup Noodle
        - Now with a special animation when used
        - Healing effect is now visually synchronized to all players
        - If used in orbit, will have no effect but will not be consumed
    - Moai Statue
        - It's now BIG !
        - Updated grab animation
        - Can spawn using one of the 4 new color variations (1 common, 2 rares and 1 ultra rare)
    - Uno Reverse Card
        - Completly removed the red variant in the code (it was supposed to be already removed but was still spawning in game), in the future I will go back to this and rework this specific variant, but for now only the blue card can be found
        - Using it in orbit or if there is no players to swap with (if all other players are dead for example) will cancel the effect and display a message
        - Model and texture have been reworked
    - Froggy Chair
        - Can spawn using one of the 6 new color variations
    - All items
        - Various fix
- **Fixed**
    - Various fix from the original ChillaxScraps mod for all imported items : this includes the purge of the "floatiness/flying position" bug, some rotation and position adjustments, the addition of custom sound to some grab and drop animation, as well as other things

</details>
