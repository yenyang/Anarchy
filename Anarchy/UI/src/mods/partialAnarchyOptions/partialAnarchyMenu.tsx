
import { Button, Panel, Portal } from "cs2/ui";
import styles from "./partialAnarchyMenu.module.scss"
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import { bindValue, trigger, useValue } from "cs2/api";
import mod from "../../../mod.json";
import { getModule } from "cs2/modding";
import { ErrorCheckComponent } from "mods/errorCheckComponent/errorCheckComponent";
import { ErrorCheck } from "Domain/errorCheck";
import { game } from "cs2/bindings";

const uilStandard =                         "coui://uil/Standard/";

const closeSrc =         uilStandard +  "XClose.svg";
const arrowUpSrc =           uilStandard +  "ArrowUpThickStroke.svg";

const ErrorChecks$ =           bindValue<ErrorCheck[]> (mod.id, "ErrorChecks");
const ShowPanel$ = bindValue<boolean>(mod.id, "ShowAnarchyToggleOptionsPanel");
const showToolIcon$ = bindValue<boolean>(mod.id, 'ShowToolIcon');

function handleClick(event: string) {
    trigger(mod.id, event);
}

const roundButtonHighlightStyle = getModule("game-ui/common/input/button/themes/round-highlight-button.module.scss", "classes");

export const PartialAnarchyMenyComponent = () => {
    const ShowPanel = useValue(ShowPanel$);
    const ErrorChecks = useValue(ErrorChecks$);
    const isPhotoMode = useValue(game.activeGamePanel$)?.__Type == game.GamePanelType.PhotoMode;
    const showToolIcon : boolean = useValue(showToolIcon$);
    return (
        <>
            {ShowPanel && !isPhotoMode && showToolIcon && (
                <Portal>
                    <Panel
                        className={styles.panel}
                        header={(
                            <VanillaComponentResolver.instance.Section title={"Anarchy Options"}>
                                <Button className={roundButtonHighlightStyle.button} variant="icon" onSelect={() => handleClick("ToggleAnarchyOptionsPanel")} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}>
                                    <img src={closeSrc}></img>
                                </Button>
                            </VanillaComponentResolver.instance.Section>
                        )}>
                        <div className={styles.rowGroup}>
                            <div className={styles.columnGroup}>
                                <div className={styles.subtitleRow}>
                                    <div>Error Type</div>
                                    <span className={styles.subtitleSpanMiddle}></span>
                                    <div>Disabled?</div>
                                    <span className={styles.subtitleSpanRight}></span>
                                </div>
                                { ErrorChecks.map((currentErrorCheck) => (
                                    <ErrorCheckComponent errorCheck={currentErrorCheck}></ErrorCheckComponent> 
                                ))}
                            </div>
                        </div>
                    </Panel>
                </Portal>
            )}
        </>
    );
}