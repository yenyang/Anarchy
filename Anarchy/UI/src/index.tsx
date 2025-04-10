import { ModRegistrar } from "cs2/modding";
import { AnarchyRowComponent } from "mods/anarchySection/anarchySection";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import { ChirperModComponent } from "mods/chirperMod/chirperMod";
import { SelectedInfoPanelTogglesComponent } from "mods/SelectedInfoPanelTogglesComponent/selectedInfoPanelTogglesComponent";
import mod from "../mod.json";
import { ElevationControlComponent } from "mods/elevationControlSections/elevationControlSections";
import { ToolOptionsVisibility } from "mods/ToolOptionsVisible/toolOptionsVisible";
import { PartialAnarchyMenuComponent } from "mods/partialAnarchyOptions/partialAnarchyMenu";
import { NetworkAnarchySections } from "mods/networkAnarchySections/networkAnarchySection";
import { AnarchyComponentsToolComponent } from "mods/AnarchyComponentsToolSections/anarchyComponentsToolSections";
import { GamepadAnarchyRowComponent } from "mods/gamepadAnarchySection/gamepadAnarchySection";

const register: ModRegistrar = (moduleRegistry) => {
     // To find modules in the registry un comment the next line and go to the console on localhost:9444. You must have -uiDeveloperMode launch option enabled.
     // console.log('mr', moduleRegistry);

     // The vanilla component resolver is a singleton that helps extrant and maintain components from game that were not specifically exposed.
     VanillaComponentResolver.setRegistry(moduleRegistry);

     // This extends mouse tool options with Anarchy section and toggle. It may or may not work with gamepads.
     moduleRegistry.extend("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", 'MouseToolOptions', AnarchyRowComponent);

     
     moduleRegistry.extend("game-ui/game/components/tool-options/gamepad-tool-options/gamepad-tool-options.tsx", 'GamepadToolOptions', GamepadAnarchyRowComponent);

     moduleRegistry.extend("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", 'MouseToolOptions', ElevationControlComponent);

     // This appends the right bottom floating menu with a chirper image that is just floating above the vanilla chirper image. Hopefully noone moves it.
     moduleRegistry.append('GameBottomRight', ChirperModComponent);

     moduleRegistry.extend("game-ui/game/components/selected-info-panel/selected-info-sections/selected-info-sections.tsx", 'selectedInfoSectionComponents', SelectedInfoPanelTogglesComponent);

     moduleRegistry.extend("game-ui/game/components/tool-options/tool-options-panel.tsx", 'useToolOptionsVisible', ToolOptionsVisibility);

     moduleRegistry.append('Game', PartialAnarchyMenuComponent);

     moduleRegistry.append('Editor', PartialAnarchyMenuComponent);

     // This extends mouse tooltip options with network Anarchy sections and toggles. It may or may not work with gamepads.
     moduleRegistry.extend("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", 'MouseToolOptions', NetworkAnarchySections);

     moduleRegistry.extend("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", 'MouseToolOptions', AnarchyComponentsToolComponent);

     // This is just to verify using UI console that all the component registriations was completed.
     console.log(mod.id + " UI module registrations completed.");
}




export default register;