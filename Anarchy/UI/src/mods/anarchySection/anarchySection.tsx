import { useLocalization } from "cs2/l10n";
import {ModuleRegistryExtend} from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { descriptionTooltip } from "mods/elevationControlSections/elevationControlSections";
import locale from "../../lang/en-US.json";

// These establishes the binding with C# side. Without C# side game ui will crash.
const anarchyEnabled$ = bindValue<boolean>(mod.id, 'AnarchyEnabled');
const showToolIcon$ = bindValue<boolean>(mod.id, 'ShowToolIcon');
const ShowPanel$ = bindValue<boolean>(mod.id, "ShowAnarchyToggleOptionsPanel");

// These contain the coui paths to Unified Icon Library svg assets
const uilStandard =                          "coui://uil/Standard/";
const uilColored =                           "coui://uil/Colored/";
const anarchyEnabledSrc =      uilColored +  "Anarchy.svg";
const anarchyDisabledSrc =     uilStandard + "Anarchy.svg";
const optionSrc =               uilStandard + "Gear.svg";
const toolSrc =                 uilStandard + "Tools.svg";


function handleClick(event: string) {
    trigger(mod.id, event);
}

export const AnarchyRowComponent: ModuleRegistryExtend = (Component : any) => {
    // I believe you should not put anything here.
    return (props) => {
        // These get the value of the bindings.
        const anarchyEnabled : boolean = useValue(anarchyEnabled$);
        const showToolIcon : boolean = useValue(showToolIcon$);
        const showPanel : boolean = useValue(ShowPanel$);
        
        // translation handling. Translates using locale keys that are defined in C# or fallback string here.
        const { translate } = useLocalization();
        const anarchySectionTitle = translate("YY_ANARCHY.Anarchy", "Anarchy");
        const tooltipText = translate("YY_ANARCHY_DESCRIPTION.AnarchyButton", "Disables error checks for tools and does not display errors. When applicable, you can place vegetation and props (with DevUI 'Add Object' menu) overlapping or inside the boundaries of other objects and close together.");
        
        // This defines aspects of the components.
        const {children, ...otherProps} = props || {};

        // This gets the original component that we may alter and return.
        var result : JSX.Element = Component();
        // It is important that we coordinate how to handle the tool options panel because it is possibile to create a mod that works for your mod but prevents others from doing the same thing.
        // If show icon add new section with title, and one button. 
        if (showToolIcon) {
            result.props.children?.unshift(
                /* 
                Add a new section before other tool options sections with translated title based of this localization key. Localization key defined in C#.
                Add a new Tool button into that section. Selected is based on Anarchy Enabled binding. 
                Tooltip is translated based on localization key. OnSelect run callback fucntion here to trigger event. 
                Anarchy specific image source changes bases on Anarchy Enabled binding. 
                */
                <VanillaComponentResolver.instance.Section title={anarchySectionTitle}>
                    <VanillaComponentResolver.instance.ToolButton
                        src={optionSrc}
                        selected = {showPanel}
                        multiSelect = {false}   // I haven't tested any other value here
                        disabled = {false}      // I haven't tested any other value here
                        tooltip = {translate(mod.id + ".TOOLTIP_DESCRIPTION[AnarchyOptions]", "Opens a panel for controlling which error checks are never disabled, disabled with Anarchy, or always disabled.")}
                        className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                        focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                        onSelect={() => handleClick("ToggleAnarchyOptionsPanel")}
                    />
                    <VanillaComponentResolver.instance.ToolButton
                        src={toolSrc}
                        multiSelect = {false}   // I haven't tested any other value here
                        disabled = {false}      // I haven't tested any other value here
                        tooltip = {descriptionTooltip(translate("Anarchy.TOOLTIP_TITLE[AnarchyComponentsTool]", locale["Anarchy.TOOLTIP_TITLE[AnarchyComponentsTool]"]), translate("Anarchy.TOOLTIP_DESCRIPTION[AnarchyComponentsTool]", locale["Anarchy.TOOLTIP_DESCRIPTION[AnarchyComponentsTool]"]))}
                        className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                        focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                        onSelect={() => handleClick("ActivateAnarchyComponentsTool")}
                    />
                    <VanillaComponentResolver.instance.ToolButton
                        src={anarchyEnabled ? anarchyEnabledSrc : anarchyDisabledSrc}
                        selected = {anarchyEnabled}
                        multiSelect = {false}   // I haven't tested any other value here
                        disabled = {false}      // I haven't tested any other value here
                        tooltip = {tooltipText}
                        className = {VanillaComponentResolver.instance.toolButtonTheme.button}                        
                        focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                        onSelect={() => handleClick("AnarchyToggled")}
                    />
                </VanillaComponentResolver.instance.Section> 
            );
        }

        return result;
    };
}