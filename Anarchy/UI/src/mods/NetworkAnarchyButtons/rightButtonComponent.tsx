import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { bindValue, trigger, useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { Composition, SideUpgrades } from "mods/networkAnarchySections/networkAnarchySection";

function handleClick(event: string, mode: SideUpgrades | Composition) {
    trigger(mod.id, event, mode as number);
}

const rightUpgrade$ = bindValue<SideUpgrades>(mod.id, "RightUpgrade");
const showUpgrade$ = bindValue<SideUpgrades>(mod.id, "RightShowUpgrade");

export const RightButtonComponent = (props: { src : string,  localeId : string | JSX.Element, upgrade: SideUpgrades }) => {
    const { translate } = useLocalization();

    const rightUpgrade = useValue(rightUpgrade$);
    const showUpgrade = useValue(showUpgrade$);
    
    return (
        <>
            {(showUpgrade & props.upgrade) == props.upgrade && (
                <VanillaComponentResolver.instance.ToolButton
                    src={props.src}
                    selected = {(rightUpgrade & props.upgrade) == props.upgrade}
                    multiSelect = {false}   // I haven't tested any other value here
                    disabled = {false}      // I haven't tested any other value here
                    tooltip = {props.localeId}
                    className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                    onSelect={() => handleClick("RightUpgrade", props.upgrade)}
                />
            )}
        </>
    );
}