import { ModRegistrar } from "cs2/modding";
import { AnarchyRowComponent } from "mods/anarchySection/anarchySection";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import { ChirperModComponent } from "mods/chirperMod/chirperMod";
import mod from "../mod.json";

const register: ModRegistrar = (moduleRegistry) => {
     // To find modules in the registry un comment the next line and go to the console on localhost:9444. You must have -uiDeveloperMode launch option enabled.
     // console.log('mr', moduleRegistry);

     // The vanilla component resolver is a singleton that helps extrant and maintain components from game that were not specifically exposed.
     VanillaComponentResolver.setRegistry(moduleRegistry);

     // This extends mouse tooltip options with Anarchy section and toggle. It may or may not work with gamepads.
     moduleRegistry.extend("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", 'MouseToolOptions', AnarchyRowComponent);

     // This appends the right bottom floating menu with a chirper image that is just floating above the vanilla chirper image. Hopefully noone moves it.
     moduleRegistry.append('GameBottomRight', ChirperModComponent);

     // This is just to verify using UI console that all the component registriations was completed.
     console.log(mod.id + " UI module registrations completed.");
}

export default register;