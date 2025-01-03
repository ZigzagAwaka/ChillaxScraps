## 1.5.4
- **Updated**
    - Removed the charging batteries effect of Ocarina's Sun's Song
    - Buffed Ocarina's Sun's Song clearing weather effect so it can now be used with 2 times less altitude
    - Greatly increased stamina regeneration by Cup Noodle
    - Freddy Fazbear and Super Sneakers got some *special* upgrades when used by unlucky players
- **Fixed**
    - Changed how time is calculated when reversing time with Ocarina's Song of Time, should fix some issues
    - Added a custom condition for [ShipInventory](https://thunderstore.io/c/lethal-company/p/WarperSan/ShipInventory/) to prevent certain items from being stored

## 1.5.3
- **Updated**
    - Dance Note will now play a small sound for the local player to know when the dance is over
    - Death Note and Dance Note will now enter a recharge state after beeing used a certain amount of time: during this state the texture of the book will become gray and it's going to slowly turn back to normal over time, after that it can be used again
    - Adjusted Freddy Fazbear audio music curve during the invisibility phase
- **Fixed**
    - Fixed Uno Reverse Card DX sometimes teleporting players at the wrong position
    - Fixed a `NullReferenceException` for Uno Reverse Card DX when it's trying to sync the state of the card for late join players
    - Fixed item tool tips sometimes overlaping with other tool tips

## 1.5.2
- **Updated**
    - Dance Note will now kill players performing the vanilla "point" emote (yeah, it's not a dance!)
    - Added a config to change the Freddy Fazbear chance of starting the invisibility phase
    - Restored the *meme sound* of the original mod as a sound variation played by Freddy Fazbear during the normal phase
- **Fixed**
    - Grab collider of Freddy Fazbear is now disabled during the invisibility phase
    - Optimized all "get players" effects (now faster and works in LAN)

## 1.5.1
- **Updated**
    - Made the uno cards collider 2 times bigger so it's easier to grab

# 1.5.0 New scraps
- **Added**
    - Imported from the original ChillaxScraps mod: Freddy Fazbear
    - Added: Uno Reverse Card DX (previously known as the Red Uno Reverse Card)
- **Updated**
    - Freddy Fazbear
        - This item has been fully reworked
        - Is now a bit dangerous...
    - Uno Reverse Card DX
        - Very different from the og Red Uno Card
        - You'll have to use it to see the new effect
    - Death Note
        - Unlucky effect will now be activated at 80% chance (previously it was 100%)
    - Super Sneakers
        - Changed how the jump boost effect is calculated to avoid conflicts with other sources of jump boost effects (BetterStamina is sill not compatible)
    - Nokia
        - Music audio can now be heard by monsters
    - Misc
        - Updated default spawn chance for some items
- **Fixed**
    - Fixed Dance Note sometimes not killing non-dancing players
    - Dance Note state is now synced for late join players

##

<details><summary>Old versions (click to reveal)</summary>

###

# 1.4.0 New scraps
- **Added**
    - Imported from the original ChillaxScraps mod: Dance Note and Nokia
- **Updated**
    - Dance Note
        - Can now be used up to 6 times globaly, each use with a different effect music (will not be destroyed after beeing used)
        - Restored particle effects that were not playing in the original mod
        - Fixed the "area kill" effect not working in the original mod, and made it only happen if the player was not dancing
    - Nokia
        - This item has been fully reworked
        - Now with some custom fun effects
    - Death Note
        - Will now play the famous Death Note music theme when a player opens the book
    - Misc
        - All new items have received some various fix (similarly to all previously imported scraps)

## 1.3.4
- **Added**
    - Added a config to set custom min,max scrap value for all items (can be left empty for default values)
- **Fixed**
    - Ocarina's Song of Storms is now compatible with [WeatherRegistry](https://thunderstore.io/c/lethal-company/p/mrov/WeatherRegistry/)

## 1.3.3
- **Updated**
    - Increased Moai Statue audio
- **Fixed**
    - Fixed a soft dependency issue

## 1.3.2
- **Updated**
    - Better teleportation code for Emergency meeting, Uno Reverse Card and Ocarina
    - Better audio code for all items
    - Improve Totem of Undying effect: now gives you 0.5s of invincibility when it's activated, should fix modded cause of death not working
- **Fixed**
    - Various fix for Ocarina's Song of Storms
        - Fixed tornados still beeing active even if the weather is changed by something else
        - Prevent the creation of lightning bolts inside objects/houses
        - Fixed rain sometimes beeing created inside the facility

## 1.3.1
- **Updated**
    - Ocarina's Sun's Song has a new variation effet when you use it in altitude
    - Adjusted the launch angle of Ocarina's Song of Soaring effect
    - Compatibility with [PremiumScraps](https://thunderstore.io/c/lethal-company/p/Zigzag/PremiumScraps/) if you have it installed
        - Death Note got a *special* upgrade when used by unlucky players
        - Unlucky players can be chosen in the PremiumScraps config file
- **Fixed**
    - Fixed Ocarina's Song of Storms "super stormy" effect persisting even if the weather is changed by something else
    - Fixed "special Zelda enemies" summoned by Ocarina's songs only having their sound changed for the host (Thank you [Xu Xiaolan](https://thunderstore.io/c/lethal-company/p/XuXiaolan/) for the help!)
    - Fixed The Master Sword yellow crystal beeing a bit too yellow
    - Small optimization of Ocarina particles

# 1.3.0 The Ocarina update
- **Added**
    - Added a custom effect for every song of the Ocarina !
        - Song effects are kept secret... You need to play the song yourself to discover the behaviour !
        - All songs have a specific number of allowed usage per moons (some are 1 time use, some are 2). This can be disabled in the config but it's preferable to not modify this as it will become unbalanced
        - There is special compatibility effects with [CodeRebirth](https://thunderstore.io/c/lethal-company/p/XuXiaolan/CodeRebirth/)
- **Fixed**
    - The Ocarina animation got a rework, hopefully it's working great now
    - Fixed Ocarina still playing audio if you cancel it by using the ship's terminal or when you place it in the ship's cupboard

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
    - [Lunxara](https://www.twitch.tv/lunxara) has reported that it's possible to use the Death Note on players that are no longer in the lobby, I didn't find a way to replicate this issue but I still modified the code to hopefully fix it

## 1.2.1
- **Updated**
    - The feature of the Boink added in the last update *"Have a small chance of launching you in the wrong direction"* has been reverted by default, but can be re-enabled with the newly added "Evil Boink" config
- **Fixed**
    - Fixed The Master Sword dropping all your items when you are unworthy
    - [A Glitched Npc](https://www.twitch.tv/a_glitched_npc) has reported that the Death Note UI is displayed for other players when the item is used by the host, I didn't find a way to replicate this issue but I still modified the code to hopefully fix it (but it's probably a mod incompatibility thing)

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
        - Completly removed the red variant in the code (it was supposed to be already removed but was still spawning in game)
        - Using it in orbit or if there is no players to swap with (if all other players are dead for example) will cancel the effect and display a message
        - Model and texture have been reworked
    - Froggy Chair
        - Can spawn using one of the 6 new color variations
    - All items
        - Various fix
- **Fixed**
    - Various fix from the original ChillaxScraps mod for all imported items : this includes the purge of the "floatiness/flying position" bug, some rotation and position adjustments, the addition of custom sound to some grab and drop animation, as well as other things

</details>
