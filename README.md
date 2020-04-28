# SuperFantasy7
*Video game development repository for CSE389 Video Game Design (NMT Spring 2020)*

SuperFantasy7 is a 2D platformer game presented in 3D graphics. Players navigate procedurally generated dungeons made up of modular prefabricated rooms. The player's goal is to find the two special items hidden in the dungeon and use them to get the third special item in as little time as popular. The game record's the player's top ten best times as well as the random seeds used to generate those dungeons. Players can choose to replay those dungeons specifically or to play a dungeon made from a random seed or one provided by the player. 

## How to Play
The player navigates the dungeon by using the "Move Up", "Move Down", "Move Left", and "Move Right" controls. By default, these are usable in WASD or arrow key configuration. "Move Up" is used to jump, while "Move Left" and "Move Right" are used to move the character from side to side. "Move Down" is used only when using the Grapple Hook (see below for details).

The player's goal is to collect all three special items (purple spheres) hidden in the dungeon. Two of these special items grant the player new abilities that are required to reach the third:

**Grapple Hook:** The Grapple Hook allows the player to swing from grapple points (yellow spheres) in the dungeon. To use the grapple hook, aim the mouse as the target and press the "Use Item" button (default RMB). If there is a clear line of sight from the player to the target, the Grapple Hook will extend and pull the player to the target.

* To release the Grapple Hook, press "Use Item" again (default RMB)
* To move up or down the Grapple Hook (such as by "reeling in" or "letting out" the line), press "Move Up" or "Move Down"
* To swing from the Grapple Hook, press "Move Left" or "Move Right"
    
**Blasting Staff:** The Blasting Staff allows the player to shoot projectiles that destroy yellow walls anywhere in the dungeon. Projectiles fired from the Blasting Staff will ricochet off of the player and the structures that make up the dungeon before fading after a short time. The Blasting Staff can be used to access boost items or clear away obstacles. To fire the Blasting Staff, aim with the mouse and press the "Use Item" button (default RMB). The projectile will fire from the player in the direction of the mouse.

**Switching Items:** If the player has unlocked both the Grapple Hook and the Blasting Staff, they can switch between the two items by pressing the "Cycle Item" button (default MMB).

## Features
1. Dungeons are randomly generated from templates including 30 prefabricated rooms that may appear anywhere in the dungeon any number of times and 4 special rooms that appear exactly once per dungeon.
    * Players complete dungeons by finding two special items and using them to find a third. The first two special items help the player navigate the dungeon. The third stops the clock and marks the dungeon as complete.
    * The dungeon includes moving platforms, spike traps, and unstable platforms that must be navigated via platforming maneuvers such as wall jumps and strategic use of special items.
    * While the special items are not required to navigate any particular room, they allow players to achieve faster times and are required to reach the third special item that marks the dungeon as complete when collected.
2. Players can collect special items to change how they navigate the dungeon.
    * The **Grapple Hook** allows players to swing from grapple points, yellow spheres that appear in the dungeon.
    * The **Blasting Staff** allows palyers to destroy yellow walls, changing how the dungeon is navigated or giving access to boost items.
3. Boost items can be found in the dungeon to give the player a brief boost to run speed or jump power.
    * **Red boost items** increase player run speed by 100% for 10 seconds.
    * **Green boost items** increase player jump power by 50% for 10 seconds.
4. Best times are listed in the main menu. From the Best Times screen, players can choose to replay any dungeon in which they score one of their top-ten best times.
5. Players can choose to specify a seed or use a random seed when playing a new dungeon.

## Planned Features Cut From Implementation
SuperFantasy7 was originally going to feature an overworld area connecting two to three randomly generated dungeons. Each dungeon would have one unlockable special item and would be marked complete after a boss fight using that special item. Rooms in the dungeon would feature more traps (such as moving saw blades) and multiple tiers of minion-type enemies.

Due to difficulties maintaining this scope while working remotely, combat mechanics and the overworld area were cut from the game. To make up for the loss in narrative content of the game, we introduced timing mechanics and the "Best Times" listing. This shifted the game from a platforming adventure game to a platforming race game. Rather than introducing one special item per dungeon, both items were made available and were made required for dungeon completion.

## Known Bugs & Issues
* In some cases, picking up the Blasting Staff will also unlock the Grapple Hook.
