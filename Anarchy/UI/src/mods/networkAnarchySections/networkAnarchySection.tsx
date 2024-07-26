import { useLocalization } from "cs2/l10n";
import {ModuleRegistryExtend} from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { tool } from "cs2/bindings";

/// <summary>
/// An enum for network cross section modes.
/// </summary>
enum SideUpgrades
{
    /// <summary>
    /// Vanilla placement.
    /// </summary>
    None,

    /// <summary>
    /// Attempted Quay placement.
    /// </summary>
    Quay = 1,

    /// <summary>
    /// Attempted RetainingWall Placement.
    /// </summary>
    RetainingWall = 2,

    /// <summary>
    /// Adds street trees.
    /// </summary>
    Trees = 4,

    /// <summary>
    /// Adds Grass Strips.
    /// </summary>
    GrassStrip = 8,

    /// <summary>
    /// Adds Wide Sidewalk
    /// </summary>
    WideSidewalk = 16,

    /// <summary>
    /// Adds Sound Barrier.
    /// </summary>
    SoundBarrier = 32,
}

/// <summary>
/// An enum for network composition.
/// </summary>
enum Composition
{
    /// <summary>
    /// Vanilla Placement,
    /// </summary>
    None,

    /// <summary>
    /// Forced ground placement.
    /// </summary>
    Ground = 1,

    /// <summary>
    /// Forced elevated placement.
    /// </summary>
    Elevated = 2,

    /// <summary>
    /// Forced tunnel placement.
    /// </summary>
    Tunnel = 4,

    /// <summary>
    /// Forced constant slope.
    /// </summary>
    ConstantSlope = 8,

    /// <summary>
    /// Remove pillars.
    /// </summary>
    NoPillars = 16,

    /// <summary>
    /// Remove Height Limits and clearances.
    /// </summary>
    NoHeightLimits = 32,
}

// These establishes the binding with C# side. Without C# side game ui will crash.
const leftUpgrade$ = bindValue<SideUpgrades>(mod.id, "LeftUpgrade");
const rightUpgrade$ = bindValue<SideUpgrades>(mod.id, "RightUpgrade");
const composition$ = bindValue<Composition>(mod.id, "Composition");
const showUpgrade$ = bindValue<SideUpgrades>(mod.id, "ShowUpgrade");


// These contain the coui paths to Unified Icon Library svg assets
const uilStandard =                          "coui://uil/Standard/";
const uilColored =                           "coui://uil/Colored/";


const quaySrc =                     uilStandard + "RoadUpgradeQuay.svg";
const retainingWallSrc =                uilStandard + "RoadUpgradeRetainingWall.svg";
const treesSrc =                    uilStandard + "Trees.svg";
const grassSrc =                    uilStandard + "Bush.svg";
const wideSidewalkSrc =             uilStandard + "PedestrianPath.svg";
const lightingSrc =                 uilStandard + "LampProps.svg";
const barrierSrc =                  uilStandard + "Lanes.svg";

const tunnelSrc =                   uilStandard + "NetworkTunnel.svg";
const elevatedSrc =                uilStandard+ "NetworkElevated.svg";
const groundSrc =                  uilStandard + "NetworkGround.svg";
const constantSlopeSrc =               uilStandard + "NetworkSlope.svg";
const noPillarsSrc =                    uilStandard + "NetworkNoPillars.svg";
const noHeightLimitSrc =                uilStandard + "NoHeightLimit.svg";

const leftUpgradeEvent = "LeftUpgrade";
const rightUpgradeEvent = "RightUpgrade";
const compositionEvent = "Composition";

function handleClick(event: string, mode: SideUpgrades | Composition) {
    trigger(mod.id, event, mode as number);
}

export const NetworkAnarchySections: ModuleRegistryExtend = (Component : any) => {
    // I believe you should not put anything here.
    return (props) => {
        // These get the value of the bindings.
        const leftUpgrade = useValue(leftUpgrade$);
        const rightUpgrade = useValue(rightUpgrade$);
        const composition = useValue(composition$);
        const netToolActive = useValue(tool.activeTool$).id == tool.NET_TOOL;
        const showUpgrade = useValue(showUpgrade$);

        // translation handling. Translates using locale keys that are defined in C# or fallback string here.
        const { translate } = useLocalization();
        
      
        // This defines aspects of the components.
        const {children, ...otherProps} = props || {};

        // This gets the original component that we may alter and return.
        var result : JSX.Element = Component();
        if (netToolActive) {
            result.props.children?.push(
               <>
                    <VanillaComponentResolver.instance.Section title={"Left"}>
                        <>
                            { (showUpgrade & SideUpgrades.WideSidewalk) == SideUpgrades.WideSidewalk && (
                                <VanillaComponentResolver.instance.ToolButton
                                    src={wideSidewalkSrc}
                                    selected = {(leftUpgrade & SideUpgrades.WideSidewalk) == SideUpgrades.WideSidewalk}
                                    multiSelect = {false}   // I haven't tested any other value here
                                    disabled = {false}      // I haven't tested any other value here
                                    tooltip = {"tooltip"}
                                    className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    onSelect={() => handleClick(leftUpgradeEvent, SideUpgrades.WideSidewalk)}
                                />
                            )}
                            { (showUpgrade & SideUpgrades.GrassStrip) == SideUpgrades.GrassStrip && (
                                <VanillaComponentResolver.instance.ToolButton
                                    src={grassSrc}
                                    selected = {(leftUpgrade & SideUpgrades.GrassStrip) == SideUpgrades.GrassStrip}
                                    multiSelect = {false}   // I haven't tested any other value here
                                    disabled = {false}      // I haven't tested any other value here
                                    tooltip = {"tooltip"}
                                    className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    onSelect={() => handleClick(leftUpgradeEvent, SideUpgrades.GrassStrip)}
                                />
                            )}
                            { (showUpgrade & SideUpgrades.Trees) == SideUpgrades.Trees && (
                                <VanillaComponentResolver.instance.ToolButton
                                    src={treesSrc}
                                    selected = {(leftUpgrade & SideUpgrades.Trees) == SideUpgrades.Trees}
                                    multiSelect = {false}   // I haven't tested any other value here
                                    disabled = {false}      // I haven't tested any other value here
                                    tooltip = {"tooltip"}
                                    className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    onSelect={() => handleClick(leftUpgradeEvent, SideUpgrades.Trees)}
                                />
                            )}
                            { (showUpgrade & SideUpgrades.SoundBarrier) == SideUpgrades.SoundBarrier && (
                                <VanillaComponentResolver.instance.ToolButton
                                    src={barrierSrc}
                                    selected = {leftUpgrade == SideUpgrades.SoundBarrier}
                                    multiSelect = {false}   // I haven't tested any other value here
                                    disabled = {false}      // I haven't tested any other value here
                                    tooltip = {"tooltip"}
                                    className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    onSelect={() => handleClick(leftUpgradeEvent, SideUpgrades.SoundBarrier)}
                                />
                            )}
                            { (showUpgrade & SideUpgrades.Quay) == SideUpgrades.Quay && (
                                <VanillaComponentResolver.instance.ToolButton
                                    src={quaySrc}
                                    selected = {leftUpgrade == SideUpgrades.Quay}
                                    multiSelect = {false}   // I haven't tested any other value here
                                    disabled = {false}      // I haven't tested any other value here
                                    tooltip = {"tooltip"}
                                    className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    onSelect={() => handleClick(leftUpgradeEvent, SideUpgrades.Quay)}
                                />
                            )}
                            { (showUpgrade & SideUpgrades.RetainingWall) == SideUpgrades.RetainingWall && (
                                <VanillaComponentResolver.instance.ToolButton
                                    src={retainingWallSrc}
                                    selected = {leftUpgrade == SideUpgrades.RetainingWall}
                                    multiSelect = {false}   // I haven't tested any other value here
                                    disabled = {false}      // I haven't tested any other value here
                                    tooltip = {"tooltip"}
                                    className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    onSelect={() => handleClick(leftUpgradeEvent, SideUpgrades.RetainingWall)}
                                />
                            )}
                        </>
                    </VanillaComponentResolver.instance.Section>
                    <VanillaComponentResolver.instance.Section title ={"Right"}>
                        <>
                        { (showUpgrade & SideUpgrades.WideSidewalk) == SideUpgrades.WideSidewalk && (
                                <VanillaComponentResolver.instance.ToolButton
                                    src={wideSidewalkSrc}
                                    selected = {(rightUpgrade & SideUpgrades.WideSidewalk) == SideUpgrades.WideSidewalk}
                                    multiSelect = {false}   // I haven't tested any other value here
                                    disabled = {false}      // I haven't tested any other value here
                                    tooltip = {"tooltip"}
                                    className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    onSelect={() => handleClick(rightUpgradeEvent, SideUpgrades.WideSidewalk)}
                                />
                            )}
                            { (showUpgrade & SideUpgrades.GrassStrip) == SideUpgrades.GrassStrip && (
                                <VanillaComponentResolver.instance.ToolButton
                                    src={grassSrc}
                                    selected = {(rightUpgrade & SideUpgrades.GrassStrip) == SideUpgrades.GrassStrip}
                                    multiSelect = {false}   // I haven't tested any other value here
                                    disabled = {false}      // I haven't tested any other value here
                                    tooltip = {"tooltip"}
                                    className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    onSelect={() => handleClick(rightUpgradeEvent, SideUpgrades.GrassStrip)}
                                />
                            )}
                            { (showUpgrade & SideUpgrades.Trees) == SideUpgrades.Trees && (
                                <VanillaComponentResolver.instance.ToolButton
                                    src={treesSrc}
                                    selected = {(rightUpgrade & SideUpgrades.Trees) == SideUpgrades.Trees}
                                    multiSelect = {false}   // I haven't tested any other value here
                                    disabled = {false}      // I haven't tested any other value here
                                    tooltip = {"tooltip"}
                                    className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    onSelect={() => handleClick(rightUpgradeEvent, SideUpgrades.Trees)}
                                />
                            )}
                            { (showUpgrade & SideUpgrades.SoundBarrier) == SideUpgrades.SoundBarrier && (
                                <VanillaComponentResolver.instance.ToolButton
                                    src={barrierSrc}
                                    selected = {rightUpgrade == SideUpgrades.SoundBarrier}
                                    multiSelect = {false}   // I haven't tested any other value here
                                    disabled = {false}      // I haven't tested any other value here
                                    tooltip = {"tooltip"}
                                    className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    onSelect={() => handleClick(rightUpgradeEvent, SideUpgrades.SoundBarrier)}
                                />
                            )}
                            { (showUpgrade & SideUpgrades.Quay) == SideUpgrades.Quay && (
                                <VanillaComponentResolver.instance.ToolButton
                                    src={quaySrc}
                                    selected = {rightUpgrade == SideUpgrades.Quay}
                                    multiSelect = {false}   // I haven't tested any other value here
                                    disabled = {false}      // I haven't tested any other value here
                                    tooltip = {"tooltip"}
                                    className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    onSelect={() => handleClick(rightUpgradeEvent, SideUpgrades.Quay)}
                                />
                            )}
                            { (showUpgrade & SideUpgrades.RetainingWall) == SideUpgrades.RetainingWall && (
                                <VanillaComponentResolver.instance.ToolButton
                                    src={retainingWallSrc}
                                    selected = {rightUpgrade == SideUpgrades.RetainingWall}
                                    multiSelect = {false}   // I haven't tested any other value here
                                    disabled = {false}      // I haven't tested any other value here
                                    tooltip = {"tooltip"}
                                    className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    onSelect={() => handleClick(rightUpgradeEvent, SideUpgrades.RetainingWall)}
                                />
                            )}    
                        </>                   
                    </VanillaComponentResolver.instance.Section>
                    <VanillaComponentResolver.instance.Section title={"Composition"}>
                        <VanillaComponentResolver.instance.ToolButton
                            src={groundSrc}
                            selected = {(composition & Composition.Ground) == Composition.Ground}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick(compositionEvent, Composition.Ground)}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            src={elevatedSrc}
                            selected = {(composition & Composition.Elevated) == Composition.Elevated}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick(compositionEvent, Composition.Elevated)}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            src={tunnelSrc}
                            selected = {(composition & Composition.Tunnel) == Composition.Tunnel}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick(compositionEvent, Composition.Tunnel)}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            src={constantSlopeSrc}
                            selected = {(composition & Composition.ConstantSlope) == Composition.ConstantSlope}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick(compositionEvent, Composition.ConstantSlope)}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            src={noPillarsSrc}
                            selected = {(composition & Composition.NoPillars) == Composition.NoPillars}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick(compositionEvent, Composition.NoPillars)}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            src={noHeightLimitSrc}
                            selected = {(composition & Composition.NoHeightLimits) == Composition.NoHeightLimits}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {"tooltip"}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClick(compositionEvent, Composition.NoHeightLimits)}
                        />
                    </VanillaComponentResolver.instance.Section>
                </>
            );
        }

        return result;
    };
}