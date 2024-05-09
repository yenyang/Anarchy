import { ModuleRegistryExtend } from "cs2/modding";
import { tool } from "cs2/bindings";

export const ToolOptionsVisibility: ModuleRegistryExtend = (Component: any) => {
    return () => Component() || tool.activeTool$.value.id == tool.NET_TOOL;
}