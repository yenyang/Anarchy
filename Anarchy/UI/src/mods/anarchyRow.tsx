import { useModding } from "modding/modding-context";
import { ModuleRegistryExtend } from "modding/types";
import { PropsWithChildren, useEffect, useRef } from "react";
import { MouseEventHandler } from "react";
import { MouseEvent, useCallback } from "react";
import { createPortal } from "react-dom";

export const unselectedImageSource : string = "coui://ui-mods/images/StandardAnarchy.svg";
export const selectedImageSource : string = "coui://ui-mods/images/ColoredAnarchy.svg";

export const select : MouseEventHandler<HTMLButtonElement> = (ev) => {
    ev.currentTarget.classList.contains("selected") ? ev.currentTarget.classList.remove("selected") : ev.currentTarget.classList.add("selected");
} 

export const AnarchyRowComponent : ModuleRegistryExtend = (Component) => {
    return (props) => {
        const { children, ...otherProps} = props || {};
        const { UI } = useModding();
        const { api: { api: { useValue, bindValue, trigger } } } = useModding();
        const anarchyEnabled$ = bindValue<boolean>('Anarchy', 'AnarchyEnabled');
        const anarchyEnabled = useValue(anarchyEnabled$);
        const showToolIcon$ = bindValue<boolean>('Anarchy', 'ShowToolIcon');
        const showToolIcon = useValue(showToolIcon$);
        const handleClick = useCallback ((ev: MouseEvent<HTMLButtonElement>) => {
            select(ev);
            trigger("Anarchy", "AnarchyToggled");
        }, [])
        const parentRef = useRef<HTMLDivElement>();
        const childRef = useRef<HTMLDivElement>();
        

        useEffect(() => {
            if (parentRef.current) {
                const div = document.createElement('div');
                childRef.current =div;
                parentRef.current?.insertBefore(div, parentRef.current?.firstChild);
            }
        }, [parentRef?.current]);

        const InsertedContent = ({ container, children }: PropsWithChildren<{container: any}>) => {
            return !container ? null : createPortal(children, container);
        }
        
        if (!showToolIcon) {
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
                    <div className = "item_bZY" id = "YYA-anarchy-item"> 
                        <div className="item-content_nNz">
                            <div className="label_RZX">Anarchy</div>
                            <div className="content_ZIz">
                                <button id="YYA-Anarchy-Button" className={anarchyEnabled ? "button_KVN selected" : "button_KVN"} onClick={handleClick}>
                                    <img id="YYA-Anarchy-Image" className="icon_Ysc" src={anarchyEnabled ? selectedImageSource : unselectedImageSource}></img>
                                </button>
                            </div>
                        </div>
                    </div>
                </InsertedContent>
            </>
        );
        
    };
    
}
