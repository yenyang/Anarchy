import { ModRegistrar } from "modding/types";
import { AnarchyRowComponent } from "mods/anarchyRow";
import { ChirperModComponent } from "mods/chirperMod";

const register: ModRegistrar = (moduleRegistry) => {
     console.log('mr', moduleRegistry);
     moduleRegistry.extend("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", 'MouseToolOptions', AnarchyRowComponent(moduleRegistry));
     moduleRegistry.extend("game-ui/game/components/right-menu/right-menu.tsx", "RightMenu", ChirperModComponent(moduleRegistry))
}

export default register;