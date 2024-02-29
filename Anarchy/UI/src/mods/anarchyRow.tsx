import { useModding } from "modding/modding-context";
import { ModuleRegistry } from "modding/types";
import { MouseEvent, useCallback } from "react";

export const unselectedImageSource : string = "coui://uil/Standard/Anarchy.svg";
export const selectedImageSource : string = "coui://uil/Colored/Anarchy.svg";

export const AnarchyRowComponent = (moduleRegistry: ModuleRegistry) => (Component: any) => {
    // The module registrys are found by logging console.log('mr', moduleRegistry); in the index file and finding appropriate one.
    const toolMouseModule = moduleRegistry.registry.get("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx");
    const toolButtonModule = moduleRegistry.registry.get("game-ui/game/components/tool-options/tool-button/tool-button.tsx")!!;
    const theme = moduleRegistry.registry.get("game-ui/game/components/tool-options/tool-button/tool-button.module.scss")?.classes;
    // These are found in the minified JS file after searching for module.
    const Section: any = toolMouseModule?.Section;
    const ToolButton: any = toolButtonModule?.ToolButton;

    return (props: any) => {
        const { children, ...otherProps} = props || {};
        const { api: { api: { useValue, bindValue, trigger } } } = useModding();
        const { engine } = useModding();

        // This establishes the binding with C# side. Without C# side game ui will crash.
        const anarchyEnabled$ = bindValue<boolean>('Anarchy', 'AnarchyEnabled');
        const anarchyEnabled = useValue(anarchyEnabled$);

        // This establishes the binding with C# side. Without C# side game ui will crash.
        const showToolIcon$ = bindValue<boolean>('Anarchy', 'ShowToolIcon');
        const showToolIcon = useValue(showToolIcon$);


        const handleClick = useCallback ((ev: MouseEvent<HTMLButtonElement>) => {
            // This triggers an event on C# side and C# designates the method to implement.
            trigger("Anarchy", "AnarchyToggled");
        }, [])
        
        // This will return original component and children if there is nothing to insert.
        if (!showToolIcon) {
            return (
                <Component {...otherProps}>
                    {children}
                </Component>
            );
        }
        
        var result = Component();
        result.props.children?.unshift(
            /* 
            Add a new section before other tool options sections with translated title based of this localization key. Localization key defined in C#.
            Add a new Tool button into that section. Selected is based on Anarchy Enabled binding. 
            Tooltip is translated based on localization key. OnSelect run callback fucntion here to trigger event. 
            Anarchy specific image source changes bases on Anarchy Enabled binding. 
            */
            <Section title={engine.translate("YY_ANARCHY.Anarchy")}>
                <ToolButton className = {theme.button} selected = {anarchyEnabled} tooltip = {engine.translate("YY_ANARCHY_DESCRIPTION.AnarchyButton")} onSelect={handleClick} src={anarchyEnabled ? selectedImageSource : unselectedImageSource}></ToolButton>
            </Section>)
        return <>{result}</>;
    };
}
