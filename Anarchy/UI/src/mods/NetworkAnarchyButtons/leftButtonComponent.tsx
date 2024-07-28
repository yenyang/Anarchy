import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { bindValue, trigger, useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { Composition, SideUpgrades } from "mods/networkAnarchySections/networkAnarchySection";

function handleClick(event: string, mode: SideUpgrades | Composition) {
    trigger(mod.id, event, mode as number);
}

const leftUpgrade$ = bindValue<SideUpgrades>(mod.id, "LeftUpgrade");
const showUpgrade$ = bindValue<SideUpgrades>(mod.id, "ShowUpgrade");

export const LeftButtonComponent = (props: { src : string,  localeId : string, upgrade: SideUpgrades }) => {
    const { translate } = useLocalization();

    const leftUpgrade = useValue(leftUpgrade$);
    const showUpgrade = useValue(showUpgrade$);
    
    return (
        <>
            {(showUpgrade & props.upgrade) == props.upgrade && (
                <VanillaComponentResolver.instance.ToolButton
                    src={props.src}
                    selected = {(leftUpgrade & props.upgrade) == props.upgrade}
                    multiSelect = {false}   // I haven't tested any other value here
                    disabled = {false}      // I haven't tested any other value here
                    tooltip = {props.localeId}
                    className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                    onSelect={() => handleClick("LeftUpgrade", props.upgrade)}
                />
            )}
        </>
    );
}