Anarchy disables error checks for tools in a way that the errors are not shown at all.
This mod allows you to place vegetation and props (using DevUI 'Add Object' menu or other mods) overlapping or inside the boundaries of other objects and close together.
For consistency within the community, please do not use the term Anarchy to mean something else for CSL2.
Sully has prepared an Amazing! demo video about detailing with Anarchy on Youtube.

## Dependencies
Unified Icon Library
I18n Everywhere (Soft dependency required for loading languages other than English.)

## Donations
If you want to say thank you with a donation you can do so on Paypal.

## Translations
I am looking for volunteers to help translate the mod into the official languages. For those interested please find the project in the discord link. CSL2:CODEMODS -> mods-wip -> Anarchy: Translations.
Full or Partial Localization: Spanish, German, Chinese Simplified, Brazilian Portuguese, Italian, Polish, Korean, Russian, and French. See credits for list of translators.
  
# Detailed Descrption
The mod also has: 
* Optional Tool icon
* Keyboard shortcut (Ctrl+A)
* Optional flaming chirper
* Option to automatically enable with bulldozer
* Optional mouse tooltip
* Opt-In Option to allow multiple copies of unique buildings using toolbar menu. Effects of multiple buildings stack!
* Option to set minimum clearance below elevated networks even while Anarchy is active in case you don't remove the zoning under a low bridge. It would be better just to remove the zoning.
* Opt-In Option to automatically disable Anarchy toggle while brushing objects such as trees.
* Set relative Elevation with Object tool and line tool for props, trees, and plants. (Options to disable and automatically reset elevation) keybinds: up arrow -> elevation up, down arrow -> elevation down, shift + r -> reset elevation to 0, shift + e -> change elevation step. 
* Elevation lock to prevent game systems from changing position of props, trees, plants, and decals. You can still change position with mods.
* Anarchy and Elevation lock can be added or removed after placement via the selected info panel.

Currently it applies to these tools:
* Object Tool (Option to automatically disable Anarchy toggle while brushing objects. and Set relative elevation while plopping or brushing.)
* Net Tool (While using the net tool Anarchy will now let you violate the clearance of other networks. I don't recommend having zoning under low bridges.)
* Area Tool (Can exceed limits for specialized industry areas)
* Bulldoze Tool (Option to default Anarchy to ON when activated)
* Terrain Tool (Cross the line within playable area.)
* Upgrade Tool
* Line Tool by Algernon (Set relative elevation).

You can activate anarchy with the keyboard shortcut "Ctrl+A" or with the optional tool icon that only appears while using the above tools.
Note for azerty users the shortcut is CTRL+Q.

You can tell anarchy is active using optional Flaming Chirper, the tool icon, or a tooltip.

The following errors will not occur while Anarchy is enabled:
* Overlap Existing
* Invalid Shape
* Long Distance
* Tight Curve
* Already Upgraded
* In Water
* No Water
* Exceeds City Limits (This provides Cross the line Functionality)
* Not On Shoreline
* Already Exists
* Short Distance
* Low Elevation
* Small Area
* Steep Slope
* Not On Border
* No Groundwater
* On Fire
* Exceeds Lot Limits (Editor Only)

If you find an error that you think should be added or if you find a tool that this should also be included, please let me know. 

## Props and Trees
Placing standalone props is an unsupported feature of the game. You need DevUI to access the 'Add Object' menu via the home button to place standalone props. Other mods may make it easier to find and place props.

With Anarchy enabled, you can place props and trees overlapping or inside the boundaries of buildings, nets, areas, other trees, other props, etc. Props and trees placed with Anarchy enabled cannot be overriden later (even if later Anarchy is disabled), but can be removed with bulldozer or brush.

Props overlapping with buildings or nets may sometimes be culled by the game, and disappear until reloading or something interacts with or near them.
The mod has an option to routinely refresh props that were culled so they don't disappear. This should not significantly impact performance anymore but you can still disable it or adjust the frequency.
You can also manually trigger a prop refresh using a button in the options menu.

Pro tip: Use the brush mode to remove trees and standalone props. If you unselect the brush snapping option for "Remove only matching type", and right click you can remove them within a radius and it only targets standalone props and trees.

## Disclaimer
This mod does NOT allow you to do everything including:
* If the vanilla net tool would remove an existing network, it will still do that.
* Even if the mod disables the error check, the UI may still prevent you from doing something.
* Not much testing is done on the effects of this mod on maps created using the unfinished editor.

**Please save frequently, in multiple files, and learn to use responsibly.**

## Support
I will respond on the code modding channels on **Cities: Skylines Modding Discord**

## Credits 
* yenyang - Mod Author
* Chameleon TBN - Testing, Feedback, Icons, and Logo
* Sully - Testing, Feedback, and Promotional Material.
* Klyte45, Algernon - Help with UI, Cooperative Development and Code Sharing
* Bad Peanut - Image Credit for Flaming Chirper
* T.D.W., krzychu124, Triton Supreme, and Quboid - Cooperative Development and Code Sharing
* Localization: Nyoko, Citadino, Dome, and Eryalito (Spanish), Hendrix (German), RilkeXS (Chinese Simplified), Luis Fernando de Paula and felipecollucci (Portuguese), Maxi and raistlin46 (Partial Italian), karmel68 and Lisek (Polish), Tanat and TwotoolusFLY_LSh.st (Korean), OWSEEX (Russian) and Quoifleur, Karg, and Edou24 (French). 
* Testing, Feedback - Dante, starrysum, HarbourMaster Jay, Dome, Tigon Ologdring, BruceyBoy, RaftermanNZ, Elektrotek, SpaceChad 