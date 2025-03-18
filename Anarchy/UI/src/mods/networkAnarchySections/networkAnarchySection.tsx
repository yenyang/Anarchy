import { useLocalization } from "cs2/l10n";
import {getModule, ModuleRegistryExtend} from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { tool } from "cs2/bindings";
import { CompositionButtonComponent } from "mods/NetworkAnarchyButtons/compositionButtonComponent";
import { LeftButtonComponent } from "mods/NetworkAnarchyButtons/leftButtonComponent";
import { RightButtonComponent } from "mods/NetworkAnarchyButtons/rightButtonComponent";
import { descriptionTooltip } from "mods/elevationControlSections/elevationControlSections";
import locale from "../../lang/en-US.json";
import grassSrc from "./A_GrassWhite.svg";
import styles from "./networkAnarchySection.module.scss";
import { useState } from "react";

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

    /// <summary>
    /// Expands elevation range to large amounts.
    /// </summary>
    ExpandedElevationRange = 256,
}


/// <summary>
/// An enum to handle whether a button is selected and/or hidden.
/// </summary>
export enum ButtonState
{
    /// <summary>
    /// Not selected.
    /// </summary>
    Off = 0,

    /// <summary>
    /// Selected.
    /// </summary>
    On = 1,

    /// <summary>
    /// Not shown.
    /// </summary>
    Hidden = 2,
}

// These establishes the binding with C# side. Without C# side game ui will crash.
const replaceLeftUpgrade$ = bindValue<ButtonState>(mod.id, "ReplaceLeftUpgrade");
const replaceRightUpgrade$ = bindValue<ButtonState>(mod.id, "ReplaceRightUpgrade");
const leftShowUpgrade$ = bindValue<SideUpgrades>(mod.id, "LeftShowUpgrade");
const rightShowUpgrade$ = bindValue<SideUpgrades>(mod.id, "RightShowUpgrade");
const showComposition$ = bindValue<Composition>(mod.id, "ShowComposition");
const replaceComposition$ = bindValue<ButtonState>(mod.id, "ReplaceComposition");
const showElevationStepSlider$ = bindValue<boolean>(mod.id, "ShowElevationStepSlider");

// These contain the coui paths to Unified Icon Library svg assets
const uilStandard =                          "coui://uil/Standard/";
const uilColored =                           "coui://uil/Colored/";


const quaySrc =                     uilStandard + "RoadUpgradeQuay.svg";
const retainingWallSrc =                uilStandard + "RoadUpgradeRetainingWall.svg";
const treesSrc =                    uilStandard + "Trees.svg";
// const grassSrc =                    uilStandard + "Bush.svg";
const wideSidewalkSrc =             uilStandard + "PedestrianPath.svg";
const lightingSrc =                 uilStandard + "LampProps.svg";
const barrierSrc =                  uilStandard + "Lanes.svg";
const wideMedianSrc =               uilStandard + "ToggleMiddleLocked.svg";

const tunnelSrc =                   uilStandard + "NetworkTunnel.svg";
const elevatedSrc =                uilStandard+ "NetworkElevated.svg";
const groundSrc =                  uilStandard + "NetworkGround.svg";
const constantSlopeSrc =               uilStandard + "NetworkSlope.svg";
const noHeightLimitSrc =                uilStandard + "NoHeightLimit.svg";
const replaceSrc = uilStandard + "Replace.svg";
// const noPillarsSrc =                    uilStandard + "NetworkNoPillars.svg";
// const noHeightLimitSrc =                uilStandard + "NoHeightLimit.svg";

function handleClick(event: string, mode: SideUpgrades | Composition) {
    trigger(mod.id, event, mode as number);
}

function handleEvent(event: string) {
    trigger(mod.id, event);
}


const SliderField : any = getModule("game-ui/editor/widgets/fields/number-slider-field.tsx", "FloatSliderField");

export const NetworkAnarchySections: ModuleRegistryExtend = (Component : any) => {
    // I believe you should not put anything here.
    return (props) => {
        // These get the value of the bindings.
        const netToolActive = useValue(tool.activeTool$).id == tool.NET_TOOL;
        const leftShowUpgrade = useValue(leftShowUpgrade$);        
        const rightShowUpgrade = useValue(rightShowUpgrade$);
        const showComposition = useValue(showComposition$);
        const replaceLeftUpgrade = useValue(replaceLeftUpgrade$);
        const replaceRightUpgrade = useValue(replaceRightUpgrade$);
        const replaceComposition = useValue(replaceComposition$);
        const elevationStep = useValue(tool.elevationStep$);
        const showElevationStepSlider = useValue(showElevationStepSlider$);

        // translation handling. Translates using locale keys that are defined in C# or fallback string here.
        const { translate } = useLocalization();
        
        let [digits, setDigits] = useState(elevationStep);
      
        // This defines aspects of the components.
        const {children, ...otherProps} = props || {};
        
        // This gets the original component that we may alter and return.
        var result : JSX.Element = Component();
        if (netToolActive) {
            result.props.children?.push(
               <>
                    {showElevationStepSlider && (
                        <VanillaComponentResolver.instance.Section title={translate("Anarchy.SECTION_TITLE[ElevationStep]", locale["Anarchy.SECTION_TITLE[ElevationStep]"])}>
                            <div className={styles.elevationStepSliderField}>
                                <SliderField value={elevationStep} min={0.01} max={25} fractionDigits={digits} onChange={(e: number) => {(e>=10)? tool.setElevationStep(Math.round(e*10)/10) : tool.setElevationStep(e);  setDigits((e >= 10)? 1 : 2)}}></SliderField>
                            </div>
                        </VanillaComponentResolver.instance.Section>
                    )}
                    { (leftShowUpgrade != SideUpgrades.None || (replaceLeftUpgrade & ButtonState.Hidden) != ButtonState.Hidden) && (
                        <>
                            <VanillaComponentResolver.instance.Section title={translate("Anarchy.SECTION_TITLE[Left]",locale["Anarchy.SECTION_TITLE[Left]"])}>
                                <>
                                    {(replaceLeftUpgrade & ButtonState.Hidden) != ButtonState.Hidden && (
                                        <VanillaComponentResolver.instance.ToolButton
                                            src={replaceSrc}
                                            selected = {(replaceLeftUpgrade & ButtonState.On) == ButtonState.On}
                                            multiSelect = {false}   // I haven't tested any other value here
                                            disabled = {false}      // I haven't tested any other value here
                                            tooltip = {descriptionTooltip(translate("Anarchy.TOOLTIP_TITLE[ReplaceUpgrade]", locale["Anarchy.TOOLTIP_TITLE[ReplaceUpgrade]"]), translate("Anarchy.TOOLTIP_DESCRIPTION[ReplaceUpgrade]", locale["Anarchy.TOOLTIP_DESCRIPTION[ReplaceUpgrade]"]))}
                                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                            onSelect={() => handleEvent("ReplaceLeftUpgrade")}
                                        />
                                    )}
                                    <LeftButtonComponent src={wideSidewalkSrc} localeId={descriptionTooltip(translate("Assets.NAME[Wide Sidewalk]"), translate("Assets.DESCRIPTION[Wide Sidewalk]"))} upgrade={SideUpgrades.WideSidewalk}/>
                                    <LeftButtonComponent src={grassSrc} localeId={descriptionTooltip(translate("Assets.NAME[Grass]"), translate("Assets.DESCRIPTION[Grass]"))} upgrade={SideUpgrades.GrassStrip}/>
                                    <LeftButtonComponent src={treesSrc} localeId={descriptionTooltip(translate("Assets.NAME[Trees]"), translate("Assets.DESCRIPTION[Trees]"))} upgrade={SideUpgrades.Trees}/>
                                    <LeftButtonComponent src={barrierSrc} localeId={descriptionTooltip(translate("Assets.NAME[Sound Barrier]"), translate("Assets.DESCRIPTION[Sound Barrier]"))} upgrade={SideUpgrades.SoundBarrier}/>
                                    <LeftButtonComponent src={quaySrc} localeId={descriptionTooltip(translate("Assets.NAME[Quay01]", locale["Assets.NAME[Quay01]"]), translate("Assets.DESCRIPTION[Quay01]", locale["Assets.DESCRIPTION[Quay01]"]))} upgrade={SideUpgrades.Quay}/>
                                    <LeftButtonComponent src={retainingWallSrc} localeId={descriptionTooltip(translate("Assets.NAME[RetainingWall01]", locale["Assets.NAME[RetainingWall01]"]), translate("Assets.DESCRIPTION[RetainingWall01]", locale["Assets.DESCRIPTION[RetainingWall01]"]))} upgrade={SideUpgrades.RetainingWall}/>
                                </>                                
                            </VanillaComponentResolver.instance.Section>
                        </>
                    )}
                    { (rightShowUpgrade != SideUpgrades.None || (replaceRightUpgrade & ButtonState.Hidden) != ButtonState.Hidden) && (
                        <>
                            <VanillaComponentResolver.instance.Section title ={translate("Anarchy.SECTION_TITLE[Right]",locale["Anarchy.SECTION_TITLE[Right]"])}>
                                <>
                                    {(replaceRightUpgrade & ButtonState.Hidden) != ButtonState.Hidden && (
                                        <VanillaComponentResolver.instance.ToolButton
                                            src={replaceSrc}
                                            selected = {(replaceRightUpgrade & ButtonState.On) == ButtonState.On}
                                            multiSelect = {false}   // I haven't tested any other value here
                                            disabled = {false}      // I haven't tested any other value here
                                            tooltip = {descriptionTooltip(translate("Anarchy.TOOLTIP_TITLE[ReplaceUpgrade]", locale["Anarchy.TOOLTIP_TITLE[ReplaceUpgrade]"]), translate("Anarchy.TOOLTIP_DESCRIPTION[ReplaceUpgrade]", locale["Anarchy.TOOLTIP_DESCRIPTION[ReplaceUpgrade]"]))}
                                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                            onSelect={() => handleEvent("ReplaceRightUpgrade")}
                                        />
                                    )}
                                    <RightButtonComponent src={wideSidewalkSrc} localeId={descriptionTooltip(translate("Assets.NAME[Wide Sidewalk]"), translate("Assets.DESCRIPTION[Wide Sidewalk]"))} upgrade={SideUpgrades.WideSidewalk}/>
                                    <RightButtonComponent src={grassSrc} localeId={descriptionTooltip(translate("Assets.NAME[Grass]"), translate("Assets.DESCRIPTION[Grass]"))} upgrade={SideUpgrades.GrassStrip}/>
                                    <RightButtonComponent src={treesSrc} localeId={descriptionTooltip(translate("Assets.NAME[Trees]"), translate("Assets.DESCRIPTION[Trees]"))} upgrade={SideUpgrades.Trees}/>
                                    <RightButtonComponent src={barrierSrc} localeId={descriptionTooltip(translate("Assets.NAME[Sound Barrier]"), translate("Assets.DESCRIPTION[Sound Barrier]"))} upgrade={SideUpgrades.SoundBarrier}/>
                                    <RightButtonComponent src={quaySrc} localeId={descriptionTooltip(translate("Assets.NAME[Quay01]", locale["Assets.NAME[Quay01]"]), translate("Assets.DESCRIPTION[Quay01]", locale["Assets.DESCRIPTION[Quay01]"]))} upgrade={SideUpgrades.Quay}/>
                                    <RightButtonComponent src={retainingWallSrc} localeId={descriptionTooltip(translate("Assets.NAME[RetainingWall01]", locale["Assets.NAME[RetainingWall01]"]), translate("Assets.DESCRIPTION[RetainingWall01]", locale["Assets.DESCRIPTION[RetainingWall01]"]))} upgrade={SideUpgrades.RetainingWall}/>
                                </>
                            </VanillaComponentResolver.instance.Section>
                        </>
                    )}
                    { (showComposition != Composition.None || (replaceComposition & ButtonState.Hidden) != ButtonState.Hidden) && ( 
                        <VanillaComponentResolver.instance.Section title={translate("Anarchy.SECTION_TITLE[General]",locale["Anarchy.SECTION_TITLE[General]"])}>
                            <>
                                {(replaceComposition & ButtonState.Hidden) != ButtonState.Hidden && (
                                    <VanillaComponentResolver.instance.ToolButton
                                        src={replaceSrc}
                                        selected = {(replaceComposition & ButtonState.On) == ButtonState.On}
                                        multiSelect = {false}   // I haven't tested any other value here
                                        disabled = {false}      // I haven't tested any other value here
                                        tooltip = {descriptionTooltip(translate("Anarchy.TOOLTIP_TITLE[ReplaceUpgrade]", locale["Anarchy.TOOLTIP_TITLE[ReplaceUpgrade]"]), translate("Anarchy.TOOLTIP_DESCRIPTION[ReplaceUpgrade]", locale["Anarchy.TOOLTIP_DESCRIPTION[ReplaceUpgrade]"]))}
                                        className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                        focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                        onSelect={() => handleEvent("ReplaceComposition")}
                                    />
                                )}
                                <CompositionButtonComponent src = {groundSrc} localeId = {descriptionTooltip(translate("Anarchy.TOOLTIP_TITLE[Ground]", locale["Anarchy.TOOLTIP_TITLE[Ground]"]), translate("Anarchy.TOOLTIP_DESCRIPTION[Ground]", locale["Anarchy.TOOLTIP_DESCRIPTION[Ground]"]))} upgrade={Composition.Ground} />
                                <CompositionButtonComponent src = {elevatedSrc} localeId = {descriptionTooltip(translate("Assets.NAME[Elevated01]", locale["Assets.NAME[Elevated01]"]), translate("Assets.DESCRIPTION[Elevated01]", locale["Assets.DESCRIPTION[Elevated01]"]))} upgrade={Composition.Elevated} />
                                <CompositionButtonComponent src = {tunnelSrc} localeId = {descriptionTooltip(translate("Assets.NAME[Tunnel01]"), translate("Assets.DESCRIPTION[Tunnel01]"))} upgrade={Composition.Tunnel} />
                                <CompositionButtonComponent src = {constantSlopeSrc} localeId = {descriptionTooltip(translate("Anarchy.TOOLTIP_TITLE[ConstantSlope]", locale["Anarchy.TOOLTIP_TITLE[ConstantSlope]"]), translate("Anarchy.TOOLTIP_DESCRIPTION[ConstantSlope]", locale["Anarchy.TOOLTIP_DESCRIPTION[ConstantSlope]"]))} upgrade={Composition.ConstantSlope} />
                                <CompositionButtonComponent src = {noHeightLimitSrc} localeId={descriptionTooltip(translate("Anarchy.TOOLTIP_TITLE[ExpandedElevationRange]", locale["Anarchy.TOOLTIP_TITLE[ExpandedElevationRange]"]), translate("Anarchy.TOOLTIP_DESCRIPTION[ExpandedElevationRange]", locale["Anarchy.TOOLTIP_DESCRIPTION[ExpandedElevationRange]"]))} upgrade={Composition.ExpandedElevationRange}></CompositionButtonComponent>
                                <CompositionButtonComponent src = {wideMedianSrc} localeId = {descriptionTooltip(translate("Anarchy.TOOLTIP_TITLE[WideMedian]", locale["Anarchy.TOOLTIP_TITLE[WideMedian]"]), translate("Anarchy.TOOLTIP_DESCRIPTION[WideMedian]", locale["Anarchy.TOOLTIP_DESCRIPTION[WideMedian]"]))} upgrade={Composition.WideMedian} />
                                <CompositionButtonComponent src = {treesSrc} localeId = {descriptionTooltip(translate("Assets.NAME[Trees]"), translate("Assets.DESCRIPTION[Trees]"))} upgrade={Composition.Trees} />
                                <CompositionButtonComponent src = {grassSrc} localeId = {descriptionTooltip(translate("Assets.NAME[Grass]"), translate("Assets.DESCRIPTION[Grass]"))} upgrade={Composition.GrassStrip} />                            
                                <CompositionButtonComponent src = {lightingSrc} localeId = {descriptionTooltip(translate("Assets.NAME[Lighting]"), translate("Assets.DESCRIPTION[Lighting]"))} upgrade={Composition.Lighting} />     
                            </>                       
                        </VanillaComponentResolver.instance.Section>
                    )}
                </>
            );
        }

        return result;
    };
}