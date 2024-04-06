import { useLocalization } from "cs2/l10n";
import {ModuleRegistryExtend} from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";

// These establishes the binding with C# side. Without C# side game ui will crash.
export const anarchyEnabled$ = bindValue<boolean>(mod.id, 'AnarchyEnabled');
export const showToolIcon$ = bindValue<boolean>(mod.id, 'ShowToolIcon');

// These contain the coui paths to Unified Icon Library svg assets
export const uilStandard =                          "coui://uil/Standard/";
export const uilColored =                           "coui://uil/Colored/";
export const anarchyEnabledSrc =      uilColored +  "Anarchy.svg";
export const anarchyDisabledSrc =     uilStandard + "Anarchy.svg";

export function handleClick() {
    // This triggers an event on C# side and C# designates the method to implement.
    trigger(mod.id, "AnarchyToggled");
}

export const AnarchyRowComponent: ModuleRegistryExtend = (Component : any) => {
    // I believe you should not put anything here.
    return (props) => {
        // These get the value of the bindings.
        const anarchyEnabled : boolean = useValue(anarchyEnabled$);
        const showToolIcon : boolean = useValue(showToolIcon$);
        
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
                        src={anarchyEnabled ? anarchyEnabledSrc : anarchyDisabledSrc}
                        selected = {anarchyEnabled}
                        multiSelect = {false}   // I haven't tested any other value here
                        disabled = {false}      // I haven't tested any other value here
                        tooltip = {tooltipText}
                        className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                        onSelect={handleClick}
                    />
                </VanillaComponentResolver.instance.Section> 
            );
        }

        return result;
    };
}