import { useModding } from "modding/modding-context";
import { ModuleRegistry } from "modding/types";

export const vanillaChirperImageSource : string = "Media/Game/Icons/Chirper.svg";
export const flamingChirperImageSource : string = "coui://uil/Colored/AnarchyChirper.svg";

export const activateFlamingChirper = () => {
    // This is a hack and not encouraged.
    var y = document.getElementsByTagName("img");
    for (let i = 0; i < y.length; i++) {
        if (y[i].src == vanillaChirperImageSource) y[i].src = flamingChirperImageSource;
    }
}

export const resetChirper = () => {
    // This is a hack and not encouraged.
    var y = document.getElementsByTagName("img");
    for (let i = 0; i < y.length; i++) {
        if (y[i].src == flamingChirperImageSource) y[i].src = vanillaChirperImageSource;
    }
} 

export const ChirperModComponent = (moduleRegistry: ModuleRegistry) => (Component: any) => {
    
    return (props : any) => {
        const { children, ...otherProps} = props || {};
        const { api: { api: { useValue, bindValue, trigger } } } = useModding();
        const anarchyEnabled$ = bindValue<boolean>('Anarchy', 'AnarchyEnabled');
        const anarchyEnabled = useValue(anarchyEnabled$);
        if (anarchyEnabled) {
            activateFlamingChirper();
        } else {
            resetChirper();
        }
       
        return (
            <Component {...otherProps}>
                {children}
            </Component>
        );
    };
    
}