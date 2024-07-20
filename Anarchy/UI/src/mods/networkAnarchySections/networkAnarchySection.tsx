import { useLocalization } from "cs2/l10n";
import {ModuleRegistryExtend} from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { tool } from "cs2/bindings";

// These establishes the binding with C# side. Without C# side game ui will crash.

// These contain the coui paths to Unified Icon Library svg assets
const uilStandard =                          "coui://uil/Standard/";
const uilColored =                           "coui://uil/Colored/";

const elevatedSrc =                uilStandard+ "NetworkElevated.svg";
const groundSrc =                  uilStandard + "NetworkGround.svg";
const quaySrc =                     uilStandard + "RoadUpgradeQuay.svg";
const retainingWallSrc =                uilStandard + "RoadUpgradeRetainingWall.svg";
const tunnelSrc =                   uilStandard + "NetworkTunnel.svg";

const constantSlopeSrc =               uilStandard + "NetworkSlope.svg";
const noPillarsSrc =                    uilStandard + "NetworkNoPillars.svg";
const noHeightLimitSrc =                uilStandard + "NoHeightLimit.svg";

function handleClick(event: string) {
    trigger(mod.id, event);
}

export const networkAnarchySections: ModuleRegistryExtend = (Component : any) => {
    // I believe you should not put anything here.
    return (props) => {
        // These get the value of the bindings.
        
        const netToolActive = useValue(tool.activeTool$).id == tool.NET_TOOL;

        // translation handling. Translates using locale keys that are defined in C# or fallback string here.
        const { translate } = useLocalization();
        
      
        // This defines aspects of the components.
        const {children, ...otherProps} = props || {};

        // This gets the original component that we may alter and return.
        var result : JSX.Element = Component();
        // It is important that we coordinate how to handle the tool options panel because it is possibile to create a mod that works for your mod but prevents others from doing the same thing.
        // If show icon add new section with title, and one button. 
        if (netToolActive) {
            result.props.children?.push(
                /* 
                Add a new section before other tool options sections with translated title based of this localization key. Localization key defined in C#.
                Add a new Tool button into that section. Selected is based on Anarchy Enabled binding. 
                Tooltip is translated based on localization key. OnSelect run callback fucntion here to trigger event. 
                Anarchy specific image source changes bases on Anarchy Enabled binding. 
                */
               <>
                    <VanillaComponentResolver.instance.Section title={"Right"}>
                        <VanillaComponentResolver.instance.ToolButton
                            src={groundSrc}
                            selected = {false}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick("ground")}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            src={elevatedSrc}
                            selected = {false}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick("ground")}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            src={quaySrc}
                            selected = {false}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick("ground")}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            src={retainingWallSrc}
                            selected = {false}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick("ground")}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            src={tunnelSrc}
                            selected = {false}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick("ground")}
                        />
                    </VanillaComponentResolver.instance.Section>
                    <VanillaComponentResolver.instance.Section title ={"Left"}>
                        <VanillaComponentResolver.instance.ToolButton
                            src={groundSrc}
                            selected = {false}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick("ground")}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            src={elevatedSrc}
                            selected = {false}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick("ground")}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            src={quaySrc}
                            selected = {false}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick("ground")}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            src={retainingWallSrc}
                            selected = {false}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick("ground")}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            src={tunnelSrc}
                            selected = {false}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick("ground")}
                        />
                    </VanillaComponentResolver.instance.Section>
                    <VanillaComponentResolver.instance.Section title={"Auxilary"}>
                        <VanillaComponentResolver.instance.ToolButton
                            src={constantSlopeSrc}
                            selected = {false}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick("ground")}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            src={noPillarsSrc}
                            selected = {false}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick("ground")}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            src={noHeightLimitSrc}
                            selected = {false}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick("ground")}
                        />
                    </VanillaComponentResolver.instance.Section>
                </>
            );
        }

        return result;
    };
}