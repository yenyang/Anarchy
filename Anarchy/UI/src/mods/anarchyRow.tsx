import { ModuleRegistry } from "modding/types";
import { useModding } from "modding/modding-context";
import { MouseEventHandler } from "react";
import { MouseEvent, useCallback } from "react";

export const unselectedImageSource : string = "coui://ui-mods/images/StandardAnarchy.svg";
export const selectedImageSource : string = "coui://ui-mods/images/ColoredAnarchy.svg";

export const select : MouseEventHandler<HTMLButtonElement> = (ev) => {
    ev.currentTarget.classList.contains("selected") ? ev.currentTarget.classList.remove("selected") : ev.currentTarget.classList.add("selected");
    
}

export const AnarchyRowComponent = (moduleRegistry: ModuleRegistry) => (Component: any) => {
    const toolMouseModule = moduleRegistry.registry.get("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx");
    const Section: any = toolMouseModule?.Section;
    const theme = moduleRegistry.registry.get("game-ui/game/components/tool-options/tool-button/tool-button.module.scss")?.classes;

    const { api: { api: { useValue, bindValue, trigger } } } = useModding();
    const anarchyEnabled$ = bindValue<boolean>('Anarchy', 'AnarchyEnabled');
    const anarchyEnabled = useValue(anarchyEnabled$);
    const showToolIcon$ = bindValue<boolean>('Anarchy', 'ShowToolIcon');
    const ShowToolIcon = useValue(showToolIcon$);

    const handleClick = useCallback ((ev: MouseEvent<HTMLButtonElement>) => {
        select(ev);
        trigger("Anarchy", "AnarchyToggled");
    }, [])

    if (ShowToolIcon) 
    {
        return (props: any) => {
            var result = Component()
            result.props.children?.unshift(<Section title="Anarchy"><button className={theme.button} ><img className={anarchyEnabled ? "button_KVN selected" : theme.icon} src={anarchyEnabled ? selectedImageSource : unselectedImageSource} /></button></Section>)
            return <>{result}</>;
        }
    }
    else 
    {
        return (props: any) => {
            var result = Component()
            return <>{result}</>;
        }
    }
}