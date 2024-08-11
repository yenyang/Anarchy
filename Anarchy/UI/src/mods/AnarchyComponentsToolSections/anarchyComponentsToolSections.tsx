import { useLocalization } from "cs2/l10n";
import {ModuleRegistryExtend} from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { tool } from "cs2/bindings";
import locale from "../../lang/en-US.json";
import { descriptionTooltip } from "mods/elevationControlSections/elevationControlSections";

/// <summary>
/// Enum for different component types the tool can add or remove.
/// </summary>
enum AnarchyComponentType
{
    /// <summary>
    /// Prevents overridable static objects from being overriden.
    /// </summary>
    PreventOverride = 1,

    /// <summary>
    /// Prevents game systems from moving overrisable static objects.
    /// </summary>
    TransformRecord = 2,
}

/// <summary>
/// An enum for tools selection mode.
/// </summary>
enum SelectionMode
{
    /// <summary>
    /// Single selection.
    /// </summary>
    Single,

    /// <summary>
    /// Radius Selection.
    /// </summary>
    Radius,
}

// These establishes the binding with C# side. Without C# side game ui will crash.
const selectionMode$ = bindValue<SelectionMode>(mod.id, "SelectionMode");
const anarchyComponentType$ = bindValue<AnarchyComponentType>(mod.id, "AnarchyComponentType");
const selectionRadius$ = bindValue<Number>(mod.id, "SelectionRadius"); 

// These contain the coui paths to Unified Icon Library svg assets
const uilStandard =                          "coui://uil/Standard/";
const uilColored =                           "coui://uil/Colored/";
const anarchyEnabledSrc =      uilColored +  "Anarchy.svg";
const anarchyDisabledSrc =     uilStandard + "Anarchy.svg";
const heightLockSrc = uilStandard + "ArrowsHeightLocked.svg";
const singleSrc =          uilStandard + "Dot.svg";
const radiusSrc =                          uilStandard + "Circle.svg";
const arrowDownSrc =         uilStandard +  "ArrowDownThickStroke.svg";
const arrowUpSrc =           uilStandard +  "ArrowUpThickStroke.svg";


function handleClickWithValue(event: string, value: SelectionMode | AnarchyComponentType) {
    trigger(mod.id, event, value);
}

function handleClick(event: string) {
    trigger(mod.id, event);
}

export const AnarchyComponentsToolComponent: ModuleRegistryExtend = (Component : any) => {
    // I believe you should not put anything here.
    return (props) => {
        // These get the value of the bindings.        
        const toolId = useValue(tool.activeTool$).id;
        const selectionMode = useValue(selectionMode$);
        const anarchyComponentType = useValue(anarchyComponentType$);
        const selectionRadius = useValue(selectionRadius$);

        // translation handling. Translates using locale keys that are defined in C# or fallback string here.
        const { translate } = useLocalization();
        const anarchySectionTitle = translate("YY_ANARCHY.Anarchy", "Anarchy");
        const tooltipText = translate("YY_ANARCHY_DESCRIPTION.AnarchyButton", "Disables error checks for tools and does not display errors. When applicable, you can place vegetation and props (with DevUI 'Add Object' menu) overlapping or inside the boundaries of other objects and close together.");
        const elevationLockTitle = translate("Anarchy.TOOLTIP_TITLE[ElevationLock]" ,locale["Anarchy.TOOLTIP_TITLE[ElevationLock]"]);
        const elevationLockDescription = translate("Anarchy.TOOLTIP_DESCRIPTION[ElevationLock]" ,locale["Anarchy.TOOLTIP_DESCRIPTION[ElevationLock]"]);

        // This defines aspects of the components.
        const {children, ...otherProps} = props || {};

        // This gets the original component that we may alter and return.
        var result : JSX.Element = Component();
        // It is important that we coordinate how to handle the tool options panel because it is possibile to create a mod that works for your mod but prevents others from doing the same thing.
        // If show icon add new section with title, and one button. 
        if (toolId == "AnarchyComponentsTool") {
            result.props.children?.unshift(
                <>
                    { selectionMode == SelectionMode.Radius && (
                        <VanillaComponentResolver.instance.Section title={"Radius"}>
                            <VanillaComponentResolver.instance.ToolButton tooltip={"Descrease Radius"} onSelect={() => handleClick("DecreaseRadius")} src={arrowDownSrc} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} className={VanillaComponentResolver.instance.mouseToolOptionsTheme.startButton}></VanillaComponentResolver.instance.ToolButton>
                            <div className={VanillaComponentResolver.instance.mouseToolOptionsTheme.numberField}>{ selectionRadius + " m"}</div>
                            <VanillaComponentResolver.instance.ToolButton tooltip={"Increase Radius"} onSelect={() => handleClick("IncreaseRadius")} src={arrowUpSrc} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} className={VanillaComponentResolver.instance.mouseToolOptionsTheme.endButton} ></VanillaComponentResolver.instance.ToolButton>
                        </VanillaComponentResolver.instance.Section>
                    )}
                    <VanillaComponentResolver.instance.Section title={"Selection"}>
                                <VanillaComponentResolver.instance.ToolButton className={VanillaComponentResolver.instance.toolButtonTheme.button} selected={selectionMode == SelectionMode.Single}       tooltip={"Single"}                          onSelect={() => handleClickWithValue("SelectionMode", SelectionMode.Single)}              src={singleSrc}         focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}  ></VanillaComponentResolver.instance.ToolButton>
                                <VanillaComponentResolver.instance.ToolButton className={VanillaComponentResolver.instance.toolButtonTheme.button} selected={selectionMode == SelectionMode.Radius}       tooltip={"Radius"}                          onSelect={() => handleClickWithValue("SelectionMode", SelectionMode.Radius)}              src={radiusSrc}                 focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}  ></VanillaComponentResolver.instance.ToolButton>
                    </VanillaComponentResolver.instance.Section>
                    <VanillaComponentResolver.instance.Section title={"Components"}>
                        <VanillaComponentResolver.instance.ToolButton
                            src={heightLockSrc}
                            selected={(anarchyComponentType & AnarchyComponentType.TransformRecord) == AnarchyComponentType.TransformRecord}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {descriptionTooltip(elevationLockTitle, elevationLockDescription)}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClickWithValue("AnarchyComponentType", AnarchyComponentType.TransformRecord)}
                        />
                        <VanillaComponentResolver.instance.ToolButton
                            src={(anarchyComponentType & AnarchyComponentType.PreventOverride) == AnarchyComponentType.PreventOverride ? anarchyEnabledSrc : anarchyDisabledSrc}
                            selected = {(anarchyComponentType & AnarchyComponentType.PreventOverride) == AnarchyComponentType.PreventOverride}
                            multiSelect = {false}   // I haven't tested any other value here
                            disabled = {false}      // I haven't tested any other value here
                            tooltip = {tooltipText}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}                        
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            onSelect={() => handleClickWithValue("AnarchyComponentType", AnarchyComponentType.PreventOverride)}
                        />
                    </VanillaComponentResolver.instance.Section> 
                </>
            );
        }

        return result;
    };
}