import { useModding } from "modding/modding-context";
import { ModuleRegistry } from "modding/types";
import { MouseEvent, useCallback } from "react";

export const unselectedImageSource : string = "coui://ui-mods/images/StandardAnarchy.svg";
export const selectedImageSource : string = "coui://ui-mods/images/ColoredAnarchy.svg";

export const AnarchyRowComponent = (moduleRegistry: ModuleRegistry) => (Component: any) => {
    const toolMouseModule = moduleRegistry.registry.get("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx")
    const Section: any = toolMouseModule?.Section;
    const theme = moduleRegistry.registry.get("game-ui/game/components/tool-options/tool-button/tool-button.module.scss")?.classes
    return (props: any) => {
        const { children, ...otherProps} = props || {};
        const { api: { api: { useValue, bindValue, trigger } } } = useModding();
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
        result.props.children?.unshift(<Section title="Anarchy"><button className={anarchyEnabled ? theme.button + " selected": theme.button} onClick={handleClick}><img className={theme.icon} src={anarchyEnabled ? selectedImageSource : unselectedImageSource}/></button></Section>)
        return <>{result}</>;
    };
}
