import { useModding } from "modding/modding-context";
import { ModuleRegistry } from "modding/types";
import { MouseEvent, useCallback } from "react";
import { ModdingContext } from "modding/modding-context";

export const unselectedImageSource : string = "coui://ui-mods/images/StandardAnarchy.svg";
export const selectedImageSource : string = "coui://ui-mods/images/ColoredAnarchy.svg";

export const activateFlamingChirper = () => {
    // This is a hack and not encouraged.
    var y = document.getElementsByTagName("img");
    for (let i = 0; i < y.length; i++) {
        if (y[i].src == "coui://GameUI/Media/Game/Icons/Chirper.svg" || y[i].src == "Media/Game/Icons/Chirper.svg") y[i].src = "coui://ui-mods/images/AnarchyChirper.svg";
    }
}

export const resetChirper = () => {
    // This is a hack and not encouraged.
    var y = document.getElementsByTagName("img");
    for (let i = 0; i < y.length; i++) {
        if (y[i].src == "coui://ui-mods/images/AnarchyChirper.svg") y[i].src = "Media/Game/Icons/Chirper.svg";
    }
} 


export const AnarchyRowComponent = (moduleRegistry: ModuleRegistry) => (Component: any) => {
    const toolMouseModule = moduleRegistry.registry.get("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx");
    const Section: any = toolMouseModule?.Section;
    const theme = moduleRegistry.registry.get("game-ui/game/components/tool-options/tool-button/tool-button.module.scss")?.classes;
    const toolButtonModule = moduleRegistry.registry.get("game-ui/game/components/tool-options/tool-button/tool-button.tsx")!!;
    const ToolButton: any = toolButtonModule?.ToolButton;

    return (props: any) => {
        const { children, ...otherProps} = props || {};
        const { api: { api: { useValue, bindValue, trigger } } } = useModding();
        const { engine } = useModding();
        const anarchyEnabled$ = bindValue<boolean>('Anarchy', 'AnarchyEnabled');
        const anarchyEnabled = useValue(anarchyEnabled$);
        const showToolIcon$ = bindValue<boolean>('Anarchy', 'ShowToolIcon');
        const showToolIcon = useValue(showToolIcon$);
        const handleClick = useCallback ((ev: MouseEvent<HTMLButtonElement>) => {
            trigger("Anarchy", "AnarchyToggled");
        }, [])
        
        if (!showToolIcon) {
            return (
                <Component {...otherProps}>
                    {children}
                </Component>
            );
        }
        
        var result = Component()
        result.props.children?.unshift(
            <Section title={engine.translate("YY_ANARCHY.Anarchy")}>
                <ToolButton className = {theme.button} selected = {anarchyEnabled} tooltip = {engine.translate("YY_ANARCHY_DESCRIPTION.AnarchyButton")} onSelect={handleClick} src={anarchyEnabled ? selectedImageSource : unselectedImageSource}></ToolButton>
            </Section>)
        return <>{result}</>;
    };
}
