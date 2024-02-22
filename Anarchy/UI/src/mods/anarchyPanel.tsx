import { useModding } from "modding/modding-context";
import anarchyCSS from './anarchy.module.scss';
import { MouseEventHandler, createElement } from "react";
import { MouseEvent, useCallback } from "react";
import { Number2 } from "common/math";
// import { IconButton  } from "widgets/data-binding/widget-bindings";
// import { LocElement, LocalizedString, locStrId } from "common/localization/localization-bindings";

export const unselectedImageSource : string = "coui://ui-mods/images/StandardAnarchy.svg";
export const selectedImageSource : string = "coui://ui-mods/images/ColoredAnarchy.svg";

export const select : MouseEventHandler<HTMLButtonElement> = (ev) => {
    ev.currentTarget.classList.contains("selected") ? ev.currentTarget.classList.remove("selected") : ev.currentTarget.classList.add("selected");
    
} 

export const AnarchyPanelComponent = () => {
    const { UI } = useModding();
    const { api: { api: { useValue, bindValue, trigger } } } = useModding();
    const anarchyEnabled$ = bindValue<boolean>('Anarchy', 'AnarchyEnabled');
    const anarchyEnabled = useValue(anarchyEnabled$);
    const showToolIcon$ = bindValue<boolean>('Anarchy', 'ShowToolIcon');
    const showToolIcon = useValue(showToolIcon$);

    const position : Number2 = { x: 0.830, y: 0.732};
    const handleClick = useCallback ((ev: MouseEvent<HTMLButtonElement>) => {
        select(ev);
        trigger("Anarchy", "AnarchyToggled");
    }, [])

    /*
    const localizationElement : LocalizedString = {
        id : "AnarchyButton",
        value: "AnarchyButton",
        args: null,
    }

    const unselectedbutton : IconButton = {
        icon: "coui://ui-mods/images/StandardAnarchy.svg", 
        selected: false, 
        disabled: false,
        tutorialTag: "AnarchyButton",
        tooltip: localizationElement as LocElement,
    };*/

    if (showToolIcon) {
        return ( 
            <UI.Panel draggable className={anarchyCSS.anarchyPanel} initialPosition={position}>
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
            </UI.Panel>
        );
    } else {
        return (null);
    }
}