import { ModRegistrar } from "cs2/modding";
import { AnarchyRowComponent } from "mods/anarchySection/anarchySection";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../mod.json";

const register: ModRegistrar = (moduleRegistry) => {
     // console.log('mr', moduleRegistry);
     VanillaComponentResolver.setRegistry(moduleRegistry);
     moduleRegistry.extend("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", 'MouseToolOptions', AnarchyRowComponent);
     console.log(mod.id + " UI module registrations completed.");
}

export default register;