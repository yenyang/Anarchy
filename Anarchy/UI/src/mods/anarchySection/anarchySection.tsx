import { useLocalization } from "cs2/l10n";
import {ModuleRegistryExtend} from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import anarchyDisabledSrc from "./StandardAnarchy.svg";
import anarchyEnabledSrc from "./ColoredAnarchy.svg";

// These establishes the binding with C# side. Without C# side game ui will crash.
export const anarchyEnabled$ = bindValue<boolean>(mod.id, 'AnarchyEnabled');
export const showToolIcon$ = bindValue<boolean>(mod.id, 'ShowToolIcon');

export function handleClick() {
    // This triggers an event on C# side and C# designates the method to implement.
    trigger(mod.id, "AnarchyToggled");
}

export const AnarchyRowComponent: ModuleRegistryExtend = (Component) => {
    // do not put anything here
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

        // If show icon add new section with title, and one button. 
        // Anarchy specific: button source depends on enabled or disabled.
        // Selected is from binding.
        // translated tooltip from above.
        // on select call function that triggers C# event.
        return (
            <>
                { showToolIcon ? 
                    <VanillaComponentResolver.instance.Section title={anarchySectionTitle}>
                        <VanillaComponentResolver.instance.ToolButton
                            src={anarchyEnabled ? anarchyEnabledSrc : anarchyDisabledSrc}
                            selected = {anarchyEnabled}
                            multiSelect = {false}
                            disabled = {false}
                            tooltip = {tooltipText}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            onSelect={handleClick}
                        />
                    </VanillaComponentResolver.instance.Section> 
                    : <></> 
                }
                <Component {...otherProps}>
                    {children}
                </Component> 
            </>
        );
    };
}