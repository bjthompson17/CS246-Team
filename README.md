# Dragons and Dungeons _Virtual Tabletop_
##### This project uses [Unity 2019.4.12f1](https://unity.com/) and we recommend using this build of Unity to open this project

A colaborative project by:
- Brian Thompson
- Hunter Stokes
- Spencer Nettles
- Ethan Mangum

This project was originally our attempt to build a virtual tabletop for the roleplaying game [Dungeons and Dragons](https://dnd.wizards.com/), and some of the frame work for that is present. In this README, we will assume that you have a basic knowledge of how to run Unity, as there is better [for using Unity than can be provided here](https://unity.com/learn), and how Dungeons and Dragons work. 

### Features 

![GUI](pict_dnd/gui.jpg)

#####1. Adding Tokens to the Field
To add tokens to the field, press the (+) symbol in the top left corner. This will expand a small menu. Click "Add Token" and a token will apear. These tokens have colision with each other.

![(+)Symbol](pict_dnd/plus_symbol.jpg)

![Mini Menu](pict_dnd/mini_menu.jpg)

![Token on Field](pict_dnd/token_present.jpg)

#####2. Token Funtionalities 
To access the funtionalities of the Token, right click it and the token menu will open. 

![Token Menu](pict_dnd/token_menu.jpg)

Click select to have the stats menu below update. A charater can be further edited by selecting edit charater. Each skill has the option to select whether or not the cahrater is profiecent. Proficency bonuses are handled in the background. Here you may change the name, adjust the speed, change the max health, and give the expereince.The experience will affect the level and profecency bonus.

![Charater Edit](pict_dnd/edit_menu.jpg)

You may make custom modifiers as well, granting them names and preset macros to your customization. 
Click submit changes when finished. 

Veiwing the charater will bring up the same menu as edit. 


#####3. Terminal
The terminal has all our RegEx functions to help calculate things such as roles and mathmatics usd within Dungeons and Dragons.you may access it either throgh the (+) symbol and clicking "Toggle Terminal"
or clicking the (<) to the right of the GUI. Here you have access to our built in calculatior, where you may roll dice, add constants, and while you have a character selected, add ability score modifiers. 

Rolling dice is done through the phrase (x)D(y), where x is the number of dice and y is the number of sides on the dice (these must be whole number non-negative intergers).  See the example below.

![Dice Example 1](pict_dnd/dice_roll.jpg)


![Dice Example 2](pict_dnd/dice_ex_2.jpg)


You may also add, subtract, multiply or divide constants to dice roll, either by typing the interger, or typing out the associated basic stat (STR, DEX, CON, WIS, INT, CHR).
