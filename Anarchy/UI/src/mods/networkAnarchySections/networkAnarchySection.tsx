import { useLocalization } from "cs2/l10n";
import {ModuleRegistryExtend} from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { tool } from "cs2/bindings";
import { CompositionButtonComponent } from "mods/NetworkAnarchyButtons/compositionButtonComponent";
import { LeftButtonComponent } from "mods/NetworkAnarchyButtons/leftButtonComponent";
import { RightButtonComponent } from "mods/NetworkAnarchyButtons/rightButtonComponent";

/// <summary>
/// An enum for network cross section modes.
/// </summary>
export enum SideUpgrades
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
export enum Composition
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
    /// Adds a wide median.
    /// </summary>
    WideMedian = 16,

    /// <summary>
    /// Median Trees.
    /// </summary>
    Trees = 32,

    /// <summary>
    /// Median Grass Strip.
    /// </summary>
    GrassStrip = 64,

    /// <summary>
    /// Lighting
    /// </summary>
    Lighting = 128,
}

// These establishes the binding with C# side. Without C# side game ui will crash.
const leftUpgrade$ = bindValue<SideUpgrades>(mod.id, "LeftUpgrade");
const rightUpgrade$ = bindValue<SideUpgrades>(mod.id, "RightUpgrade");
const showUpgrade$ = bindValue<SideUpgrades>(mod.id, "ShowUpgrade");
const showComposition$ = bindValue<Composition>(mod.id, "ShowComposition");

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
const wideMedianSrc =               uilStandard + "ToggleMiddleLocked.svg";

const tunnelSrc =                   uilStandard + "NetworkTunnel.svg";
const elevatedSrc =                uilStandard+ "NetworkElevated.svg";
const groundSrc =                  uilStandard + "NetworkGround.svg";
const constantSlopeSrc =               uilStandard + "NetworkSlope.svg";
// const noPillarsSrc =                    uilStandard + "NetworkNoPillars.svg";
// const noHeightLimitSrc =                uilStandard + "NoHeightLimit.svg";

const leftUpgradeEvent = "LeftUpgrade";
const rightUpgradeEvent = "RightUpgrade";

function handleClick(event: string, mode: SideUpgrades | Composition) {
    trigger(mod.id, event, mode as number);
}

export const NetworkAnarchySections: ModuleRegistryExtend = (Component : any) => {
    // I believe you should not put anything here.
    return (props) => {
        // These get the value of the bindings.
        const leftUpgrade = useValue(leftUpgrade$);
        const rightUpgrade = useValue(rightUpgrade$);
        const netToolActive = useValue(tool.activeTool$).id == tool.NET_TOOL;
        const showUpgrade = useValue(showUpgrade$);
        const showComposition = useValue(showComposition$);

        // translation handling. Translates using locale keys that are defined in C# or fallback string here.
        const { translate } = useLocalization();
        
      
        // This defines aspects of the components.
        const {children, ...otherProps} = props || {};

        // This gets the original component that we may alter and return.
        var result : JSX.Element = Component();
        if (netToolActive) {
            result.props.children?.push(
               <>
                    { (showUpgrade != SideUpgrades.None) && (
                        <>
                            <VanillaComponentResolver.instance.Section title={"Left"}>
                                <LeftButtonComponent src={wideSidewalkSrc} localeId="tooltip" upgrade={SideUpgrades.WideSidewalk}/>
                                <LeftButtonComponent src={grassSrc} localeId="tooltip" upgrade={SideUpgrades.GrassStrip}/>
                                <LeftButtonComponent src={treesSrc} localeId="tooltip" upgrade={SideUpgrades.Trees}/>
                                <LeftButtonComponent src={barrierSrc} localeId="tooltip" upgrade={SideUpgrades.SoundBarrier}/>
                                <LeftButtonComponent src={quaySrc} localeId="tooltip" upgrade={SideUpgrades.Quay}/>
                                <LeftButtonComponent src={retainingWallSrc} localeId="tooltip" upgrade={SideUpgrades.RetainingWall}/>
                            </VanillaComponentResolver.instance.Section>
                            <VanillaComponentResolver.instance.Section title ={"Right"}>
                                <RightButtonComponent src={wideSidewalkSrc} localeId="tooltip" upgrade={SideUpgrades.WideSidewalk}/>
                                <RightButtonComponent src={grassSrc} localeId="tooltip" upgrade={SideUpgrades.GrassStrip}/>
                                <RightButtonComponent src={treesSrc} localeId="tooltip" upgrade={SideUpgrades.Trees}/>
                                <RightButtonComponent src={barrierSrc} localeId="tooltip" upgrade={SideUpgrades.SoundBarrier}/>
                                <RightButtonComponent src={quaySrc} localeId="tooltip" upgrade={SideUpgrades.Quay}/>
                                <RightButtonComponent src={retainingWallSrc} localeId="tooltip" upgrade={SideUpgrades.RetainingWall}/>
                            </VanillaComponentResolver.instance.Section>
                        </>
                    )}
                    { showComposition != Composition.None && (
                        <VanillaComponentResolver.instance.Section title={"General"}>
                            <CompositionButtonComponent src = {groundSrc} localeId = {"tooltip"} upgrade={Composition.Ground} />
                            <CompositionButtonComponent src = {elevatedSrc} localeId = {"tooltip"} upgrade={Composition.Elevated} />
                            <CompositionButtonComponent src = {tunnelSrc} localeId = {"tooltip"} upgrade={Composition.Tunnel} />
                            <CompositionButtonComponent src = {constantSlopeSrc} localeId = {"tooltip"} upgrade={Composition.ConstantSlope} />
                            <CompositionButtonComponent src = {wideMedianSrc} localeId = {"tooltip"} upgrade={Composition.WideMedian} />
                            <CompositionButtonComponent src = {treesSrc} localeId = {"tooltip"} upgrade={Composition.Trees} />
                            <CompositionButtonComponent src = {grassSrc} localeId = {"tooltip"} upgrade={Composition.GrassStrip} />                            
                            <CompositionButtonComponent src = {lightingSrc} localeId = {"tooltip"} upgrade={Composition.Lighting} />                            
                        </VanillaComponentResolver.instance.Section>
                    )}
                </>
            );
        }

        return result;
    };
}