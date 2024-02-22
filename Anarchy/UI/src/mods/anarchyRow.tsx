import { ModuleRegistry } from "modding/types";
import { useModding } from "modding/modding-context";
import { MouseEventHandler } from "react";
import { MouseEvent, useCallback } from "react";

export const unselectedImageSource : string = "coui://ui-mods/images/StandardAnarchy.svg";
export const selectedImageSource : string = "coui://ui-mods/images/ColoredAnarchy.svg";

export const select : MouseEventHandler<HTMLButtonElement> = (ev) => {
    ev.currentTarget.classList.contains("selected") ? ev.currentTarget.classList.remove("selected") : ev.currentTarget.classList.add("selected");
}

export const { api: { api: { useValue, bindValue, trigger } } } = useModding();
export const anarchyEnabled$ = bindValue<boolean>('Anarchy', 'AnarchyEnabled');
export const anarchyEnabled = useValue(anarchyEnabled$);
export const showToolIcon$ = bindValue<boolean>('Anarchy', 'ShowToolIcon');
export const ShowToolIcon = useValue(showToolIcon$);

export const AnarchyRowComponent = (moduleRegistry: ModuleRegistry) => (Component: any) => {
    const toolMouseModule = moduleRegistry.registry.get("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx");
    const Section: any = toolMouseModule?.Section;
    const theme = moduleRegistry.registry.get("game-ui/game/components/tool-options/tool-button/tool-button.module.scss")?.classes;

    const handleClick = useCallback ((ev: MouseEvent<HTMLButtonElement>) => {
        select(ev);
        trigger("Anarchy", "AnarchyToggled");
    }, [])

    
    return (props: any) => {
        var result = Component();
        result.props.children?.unshift(<Section title="Anarchy"><button className={theme.button} ><img className={anarchyEnabled ? "button_KVN selected" : theme.icon} src={anarchyEnabled ? selectedImageSource : unselectedImageSource} /></button></Section>);
        return <>{result}</>;
    }
}