# Neko 2D Editor

## What is it?  
Neko 2D Editor is a 2D level editor used to create HAD files.  
The 2D editor isn’t quite as robust as the 3D editor, but it can be super useful for sketching out levels.  
The 2D editor is also currently the only place where you can allocate additional textures.  

## What are HAD files?  
HAD files are super small files used to hold level data in as small a package as possible.  
This does limit the flexibility of the levels somewhat (e.g., all doors are set on the same level), but each level comes in at under 4k uncompressed and will compress down to under 500 bytes.  

## What can I make levels for?  
I created the HAD file format and the Neko Neo editor to make levels for my mini FPS, **Ruyn**.  
Currently, this is the only game available with HAD support, but I plan on making more.  

## What was the Neko 2D Editor written in?  
WinForms and C#  

---

## User Guide  

Hopefully, the level editor is mostly self-explanatory and easy to use, but there is some weirdness here and there.  

### Walls and Steps  
The way the walls and steps work is probably the most important concept to get your head around. It’s pretty simple but can be a little awkward to explain.  

To help keep the super-compact file size, the blocks work in a slightly different way from most editors.  
Neko works on the concept of **“walls”** and **“steps.”** A wall always goes up to the ceiling (you can choose the ceiling height in the 4th box down on the right), and you can only have one ceiling height for each level.  
So, use walls for your level layout.  

Steps are just that—steps. Use these for things like platforms and stairs.  
Do you see the input box for **cell height**? Use this to set the height of each step, from 1-7. To draw a wall rather than a step, set this value to 0.  

To draw a wall, just click the wall texture you want to draw and click on one of the cells on the big map on the left and start drawing. To remove a wall, just right-click.  

**Note:** You can set the global step height in the **Step Height** box. This will multiply the size of each of the blocks. I find 2 is a good starting point.  

### Doors  
To place a door, just click the **Door Texture** you want to place and click the cell where you want to place it.  
To lock the door behind a key, just click **Lock 1, Lock 2, or Lock 3** in the bottom right box and then click on the door you want to lock. Then, place the corresponding key into the level (see Placing Items below).  

### Allocating Textures  
Another concession made to accommodate the file size is that each level can only use 7 textures at once, but there are 16 textures available to you. You can select your texture allocation using the up/down boxes under the **Texture Allocation** heading.  

### Placing Items  
Placing items is just like placing a wall: click on the item you want to place and then click the cell where you would like to place it. To remove it, right-click on it.  

### Testing the Level  
To test the level, just click the **Play Level** button in the bottom right.  
You can use the checkboxes above to launch with certain flags set:  
- **FS** = full screen  
- **God mode** = can’t die  
- **Preview** = fly around the level in the engine  

### Saving and Loading  
To save and load levels, just click the **File** menu item at the top right of the main window and choose either the save or load option.  

**IMPORTANT:** You can save the file under any name you like, but the game will only recognize files named in the following format: **Level + a two-digit number** (e.g., Level01, Level10). After a level ends, the game will look for the next level in sequence. If it cannot find the next level, the game will display the ending sequence.  

**NOTE:** The level uploader will only upload files in this format.  

**IMPORTANT NOTE:** The first level must always be named **Level00.** Subsequent levels should follow the format: Level00, Level01, Level02, etc.  

---

## Thank You  
Thanks for taking a look at my little project! If you have any problems or suggestions, please send them to paul.marrable@flumpstudios.co.uk :)  
