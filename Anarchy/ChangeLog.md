## Patch V1.7.10
* Prevent subelements from being overriden while default tool is active. Important while using EDT transform menu while near subelements.
* Anarchy components tool now works on subelements. 
* While Selecting subelements with Debug Toggle you can now add or remove Anarchy or Elevation Lock components. Both are added automatically when moving a subelement with EDT.
* Added very limited and basic game pad support. Just Anarchy toggle. (While using Gamepad, make sure to disable Network Anarchy content or at least disable replacing upgrades by default).
* Removed I18N Everywhere dependency. 
* Translations for officially supported languages are handled internally with embedded resource files. 
* Fix Eight Lane road with median cannot have upgrades to left, right, and general applied in some configurations supported by vanilla.
* Fix replacing water and sewer mains with combined services or vice versa breaks the utility network.
* Anarchy keybinding set to Tool Usage. This will display a conflict warning with Activating/Deactivating Buildings if bindings are not changed.