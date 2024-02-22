import { ModuleRegistry } from "modding/types";

export const AnarchyRowComponent = (moduleRegistry: ModuleRegistry) => (Component: any) => {
    const toolMouseModule = moduleRegistry.registry.get("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx")
    const Section: any = toolMouseModule?.Section;
    const theme = moduleRegistry.registry.get("game-ui/game/components/tool-options/tool-button/tool-button.module.scss")?.classes

    return (props: any) => {
        var result = Component()
        result.props.children?.unshift(<Section title="Anarchy"><button className={theme.button} ><img className={theme.icon} src="coui://ui-mods/images/StandardAnarchy.svg" /></button></Section>)
        return <>{result}</>;
    };
}
