import { useModding } from "modding/modding-context";
import { ModRegistrar } from "modding/types";
import { AnarchyRowComponent } from "mods/anarchyRow";
import { ModuleRegistry } from "modding/types";
import { AnarchyPanelComponent } from "mods/anarchyPanel";

const register: ModRegistrar = (moduleRegistry) => {
    // While launching game in UI development mode (include --uiDeveloperMode in the launch options)
    // - Access the dev tools by opening localhost:9444 in chrome browser.
    // - You should see a hello world output to the console.
    // - use the useModding() hook to access exposed UI, api and native coherent engine interfaces. 
    // Good luck and have fun!
    console.log('mr', moduleRegistry);
    moduleRegistry.extend('game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx', 'MouseToolOptions', AnarchyRowComponent);
    moduleRegistry.extend('game-ui/game/components/tool-options/tool-options-panel.tsx', 'ToolOptionsPanel', AnarchyRowComponent);
    moduleRegistry.extend('game-ui/game/components/tool-options/gamepad-tool-options/gamepad-tool-options.tsx', 'GamepadToolOptions', AnarchyRowComponent);
    moduleRegistry.extend('game-ui/menu/components/main-menu-screen/main-menu-screen.tsx', 'MainMenuNavigation', AnarchyRowComponent);
    moduleRegistry.append('Game', AnarchyPanelComponent);
}

export default register;