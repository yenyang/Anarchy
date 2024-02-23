import { useModding } from "modding/modding-context";
import { ModuleRegistryExtend } from "modding/types";
import { PropsWithChildren, useEffect, useRef } from "react";
import { createPortal } from "react-dom";

export const vanillaChirerImageSource : string = "Media/Game/Icons/Chirper.svg";
export const flamingChirperImageSource : string = "coui://ui-mods/images/AnarchyChirper.svg";

export const ChirperModComponent : ModuleRegistryExtend = (Component) => {
    return (props) => {
        const { children, ...otherProps} = props || {};
        const { UI } = useModding();
        const { api: { api: { useValue, bindValue, trigger } } } = useModding();
        const anarchyEnabled$ = bindValue<boolean>('Anarchy', 'AnarchyEnabled');
        const anarchyEnabled = useValue(anarchyEnabled$);
       
        const parentRef = useRef<HTMLDivElement>();
        const childRef = useRef<HTMLDivElement>();
        
        useEffect(() => {
            if (parentRef.current) {
                const div = document.createElement('div');
                childRef.current = div;
                // if (parentRef.current.firstChild.)
                parentRef.current?.insertBefore(div, parentRef.current?.firstChild);
            }
        }, [parentRef?.current]);

        const InsertedContent = ({ container, children }: PropsWithChildren<{container: any}>) => {
            return !container ? null : createPortal(children, container);
        }
        
        if (!anarchyEnabled) {
            return (
                <Component {...otherProps}>
                    {children}
                </Component>
            );
        }

        return (
            <>
                <Component {...otherProps} ref={parentRef} />
                <InsertedContent container={childRef.current}>
                    <img className="icon_qLJ icon_soN icon_Iwk" src={anarchyEnabled ? flamingChirperImageSource : flamingChirperImageSource}></img>
                </InsertedContent>
            </>
        );
        
    };
    
}