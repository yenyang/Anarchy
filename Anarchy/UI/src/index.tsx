import { ModRegistrar } from "modding/types";
// import { AnarchyPanelComponent } from "mods/anarchyPanel";
import { AnarchyRowComponent } from "mods/anarchyRow";

const register: ModRegistrar = (moduleRegistry) => {
     moduleRegistry.extend("game-ui/game/components/tool-options/tool-options-panel.tsx", 'ToolOptionsPanel', AnarchyRowComponent);
}

export default register;