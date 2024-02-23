import { ModRegistrar } from "modding/types";
import { AnarchyRowComponent } from "mods/anarchyRow";

const register: ModRegistrar = (moduleRegistry) => {
     console.log('mr', moduleRegistry);
     moduleRegistry.extend("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", 'MouseToolOptions', AnarchyRowComponent(moduleRegistry));
}

export default register;