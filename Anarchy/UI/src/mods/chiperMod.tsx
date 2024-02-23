import { useModding } from "modding/modding-context";
import { ModuleRegistry } from "modding/types";
import { Children } from "react";

export const vanillaChirperImageSource : string = "Media/Game/Icons/Chirper.svg";
export const flamingChirperImageSource : string = "coui://ui-mods/images/AnarchyChirper.svg";

export const ChirperModComponent = (moduleRegistry: ModuleRegistry) => (Component: any) => {
    const rightMenuModule = moduleRegistry.registry.get("game-ui/game/components/right-menu/right-menu.tsx");
    
    return (props : any) => {
        const { children, ...otherProps} = props || {};
        const { UI } = useModding();
        const { api: { api: { useValue, bindValue, trigger } } } = useModding();
        const anarchyEnabled$ = bindValue<boolean>('Anarchy', 'AnarchyEnabled');
        const anarchyEnabled = useValue(anarchyEnabled$);
        if (rightMenuModule && anarchyEnabled) {
            console.log(rightMenuModule.chirper);
            rightMenuModule.chirper = flamingChirperImageSource;
            moduleRegistry.registry.set("game-ui/game/components/right-menu/right-menu.tsx", rightMenuModule);
        } else if (rightMenuModule) {
            console.log(rightMenuModule.chirper);
            rightMenuModule.chirper = vanillaChirperImageSource;
            moduleRegistry.registry.set("game-ui/game/components/right-menu/right-menu.tsx", rightMenuModule);
        }
       
        return (
            <Component {...otherProps}>
                {children}
            </Component>
        );
    };
    
}