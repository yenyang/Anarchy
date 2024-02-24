import { useModding } from "modding/modding-context";
import { ModuleRegistry } from "modding/types";
import { MouseEvent, useCallback } from "react";

export const unselectedImageSource : string = "coui://uil/Standard/Anarchy.svg";
export const selectedImageSource : string = "coui://uil/Colored/Anarchy.svg";

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
