import { ModRegistrar } from "modding/types";
// import { AnarchyPanelComponent } from "mods/anarchyPanel";
import { AnarchyRowComponent } from "mods/anarchyRow";
import { ChirperModComponent } from "mods/chiperMod";

const register: ModRegistrar = (moduleRegistry) => {
     console.log('mr', moduleRegistry);
     moduleRegistry.extend("game-ui/game/components/tool-options/tool-options-panel.tsx", 'ToolOptionsPanel', AnarchyRowComponent);
     moduleRegistry.extend("game-ui/game/components/chirper/chirper-panel.tsx", "ChirperPanel", ChirperModComponent)
}

export default register;