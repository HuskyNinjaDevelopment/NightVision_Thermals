# FivePD_NightVision_Thermals
Allows the use of NightVsion and Thermal Vision through the use of commands or keybinds.

# Set Up
1) Place the folder containing the .dll file and the config file in the **fivepd/plugins/** folder.
2) If not already present add the line **'./plugins/\*\*/\*.json',** somewhere around line 20 to the file **fivepd/fxmanifest.lua**.

# Config Setup
"goggles-up" and "goggles-down" are helmets that refer to the state of the goggles. If you want to change these be sure you change both or things will look odd in game.\
"force-first-person" forces the game into first person mode for the player while the NVGs or Thermals are active. If you don't want this top happen just set the value to false

# Plugin Load Error
![Error Screenshot](https://user-images.githubusercontent.com/123021459/232183012-5111aa39-35b9-458b-bbf1-8e95d5b5b8de.PNG)

This error will occur when the player loads into the game and the plugin attempts to load. If you are getting this you did not edit the fivepd/fxmanifest.lua file. The file must be edited to include searching within folders in the fivepd/plugins folder. To add this functionality add the line **'./plugins/\*\*/\*.json',** somewhere around line 20.
