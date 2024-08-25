import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { bindValue, trigger, useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { Composition, SideUpgrades } from "mods/networkAnarchySections/networkAnarchySection";
import { upgrade } from "cs2/bindings";

function handleClick(event: string, mode: SideUpgrades | Composition) {
    trigger(mod.id, event, mode as number);
}

const composition$ = bindValue<Composition>(mod.id, "Composition");
const showComposition$ = bindValue<Composition>(mod.id, "ShowComposition");

export const CompositionButtonComponent = (props: { src : string,  localeId : string | JSX.Element, upgrade: Composition }) => {
    const { translate } = useLocalization();

    const composition = useValue(composition$);    
    const showComposition = useValue(showComposition$);
    
    return (
        <>
            {(showComposition & props.upgrade) == props.upgrade && (
                <VanillaComponentResolver.instance.ToolButton
                    src={props.src}
                    selected = {(composition & props.upgrade) == props.upgrade}
                    multiSelect = {false}   // I haven't tested any other value here
                    disabled = {false}      // I haven't tested any other value here
                    tooltip = {props.localeId}
                    className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                    onSelect={() => handleClick("Composition", props.upgrade)}
                />
            )}
        </>
    );
}