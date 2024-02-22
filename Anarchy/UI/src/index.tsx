import { ModRegistrar } from "modding/types";
import { AnarchyPanelComponent } from "mods/anarchyPanel";

const register: ModRegistrar = (moduleRegistry) => {
     moduleRegistry.append('Game', AnarchyPanelComponent);
}

export default register;