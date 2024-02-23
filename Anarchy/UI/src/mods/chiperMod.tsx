import { useModding } from "modding/modding-context";
import { ModuleRegistryExtend } from "modding/types";
import { Children } from "react";

export const vanillaChirerImageSource : string = "Media/Game/Icons/Chirper.svg";
export const flamingChirperImageSource : string = "coui://ui-mods/images/AnarchyChirper.svg";

export const ChirperModComponent : ModuleRegistryExtend = (Component) => {
    return (props) => {
        const { children, ...otherProps} = props || {};
        const { UI } = useModding();
        const { api: { api: { useValue, bindValue, trigger } } } = useModding();
        const anarchyEnabled$ = bindValue<boolean>('Anarchy', 'AnarchyEnabled');
        const anarchyEnabled = useValue(anarchyEnabled$);
        
        console.log("chirmperModcomponent");
        console.log(Children.count(children));
        
        return (
            <Component {...otherProps}>
                {children}
            </Component>
        );
    };
    
}