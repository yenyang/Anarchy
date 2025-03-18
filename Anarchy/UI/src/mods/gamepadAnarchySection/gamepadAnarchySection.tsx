import { useLocalization } from "cs2/l10n";
import {getModule, ModuleRegistryExtend} from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";  
import { Theme } from "cs2/bindings";
import { descriptionTooltip } from "mods/elevationControlSections/elevationControlSections";
import locale from "../../lang/en-US.json";
import { AutoNavigationScope, MultiChildFocusController } from "cs2/input";

// These establishes the binding with C# side. Without C# side game ui will crash.
const anarchyEnabled$ = bindValue<boolean>(mod.id, 'AnarchyEnabled');
const showToolIcon$ = bindValue<boolean>(mod.id, 'ShowToolIcon');
const ShowPanel$ = bindValue<boolean>(mod.id, "ShowAnarchyToggleOptionsPanel");
const disableAnarchyComponentsTool$ = bindValue<boolean>(mod.id, 'DisableElevationLock');

// These contain the coui paths to Unified Icon Library svg assets
const uilStandard =                          "coui://uil/Standard/";
const uilColored =                           "coui://uil/Colored/";
const anarchyEnabledSrc =      uilColored +  "Anarchy.svg";
const anarchyDisabledSrc =     uilStandard + "Anarchy.svg";
const optionSrc =               uilStandard + "Gear.svg";
const toolSrc =                 uilStandard + "Tools.svg";


const GamepadSection : any = getModule(
    "game-ui/game/components/tool-options/gamepad-tool-options/gamepad-tool-options.tsx",
    "Section"
);
// {focusKey: e, title: t, readonly: n, hidden: s, leftDisabled: i, rightDisabled: a, hideArrows: o, uiTag: r, uiTagLeft: l, uiTagRight: c, children: u, onSelectLeft: d, onSelectRight: m

const GamepadToolStyles : Theme | any = getModule(
    "game-ui/game/components/tool-options/gamepad-tool-options/gamepad-tool-options.module.scss",
    "classes"
);

/*
group: "group_fXk",
item: "item_RBL item-focused_FuT",
"item-content": "item-content__FJ",
itemContent: "item-content__FJ",
readonly: "readonly_jlY",
label: "label_EcW",
content: "content_I1Y",
"arrow-button": "arrow-button_V0s",
arrowButton: "arrow-button_V0s",
field: "field_hZA",
"wide-field": "wide-field_jm1 field_hZA",
wideField: "wide-field_jm1 field_hZA",
hidden: "hidden_o8e",
"color-field": "color-field_zGk",
colorField: "color-field_zGk"
*/

function handleClick(event: string) {
    trigger(mod.id, event);
}

export const GamepadAnarchyRowComponent: ModuleRegistryExtend = (Component : any) => {
    // I believe you should not put anything here.
    return (props) => {
        // These get the value of the bindings.
        const anarchyEnabled : boolean = useValue(anarchyEnabled$);
        const showToolIcon : boolean = useValue(showToolIcon$);
        const disableAnarchyComponentsTool = useValue(disableAnarchyComponentsTool$);
        
        
        // translation handling. Translates using locale keys that are defined in C# or fallback string here.
        const { translate } = useLocalization();
        const anarchySectionTitle = translate("YY_ANARCHY.Anarchy", "Anarchy");
        const tooltipText = translate("YY_ANARCHY_DESCRIPTION.AnarchyButton", "Disables error checks for tools and does not display errors. When applicable, you can place vegetation and props (with DevUI 'Add Object' menu) overlapping or inside the boundaries of other objects and close together.");
        
        const {children, ...otherProps} = props || {};

        var result : JSX.Element = Component();
        if (showToolIcon) {
            result.props.children.props.children?.unshift(                
                <AutoNavigationScope>
                    <GamepadSection title={anarchySectionTitle} onSelectLeft={() => handleClick("AnarchyToggled")} onSelectRight={() => handleClick("AnarchyToggled")}>
                        <VanillaComponentResolver.instance.ToolButton
                            src={anarchyEnabled ? anarchyEnabledSrc : anarchyDisabledSrc}
                            selected = {anarchyEnabled}
                            multiSelect = {true}   // I haven't tested any other value here
                            disabled = {false}      
                            tooltip = {tooltipText}
                            uiTag="AnarchyToggle"
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}                        
                            focusKey={VanillaComponentResolver.instance.FOCUS_AUTO}
                            onSelect={() => handleClick("AnarchyToggled")}
                        />
                    </GamepadSection> 
                </AutoNavigationScope>
            );
        }

        return result;
    };
}

/*
<>
                
                        <VanillaComponentResolver.instance.ToolButton
                            src={toolSrc}
                            multiSelect = {true}   // I haven't tested any other value here
                            disabled = {disableAnarchyComponentsTool}      
                            tooltip = {descriptionTooltip(translate("Anarchy.TOOLTIP_TITLE[AnarchyComponentsTool]", locale["Anarchy.TOOLTIP_TITLE[AnarchyComponentsTool]"]), translate("Anarchy.TOOLTIP_DESCRIPTION[AnarchyComponentsTool]", locale["Anarchy.TOOLTIP_DESCRIPTION[AnarchyComponentsTool]"]))}
                            className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                            focusKey={VanillaComponentResolver.instance.FOCUS_AUTO}
                            uiTag="AnarchyComponentsTool"
                            onSelect={() => handleClick("ActivateAnarchyComponentsTool")}
                        />
            </>*/