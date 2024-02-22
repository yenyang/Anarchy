import { ModRegistrar } from "modding/types";
import { AnarchyPanelComponent } from "mods/anarchyPanel";
// import { AnarchyRowComponent } from "mods/anarchyRow";

const register: ModRegistrar = (moduleRegistry) => {
    // moduleRegistry.extend("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", 'MouseToolOptions', AnarchyRowComponent(moduleRegistry));
     moduleRegistry.append('Game', AnarchyPanelComponent);
}

export default register;