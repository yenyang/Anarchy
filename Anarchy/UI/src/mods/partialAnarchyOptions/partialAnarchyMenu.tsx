
import { Button, Panel, Portal } from "cs2/ui";
import styles from "./partialAnarchyMenu.module.scss"
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import { bindValue, trigger, useValue } from "cs2/api";
import mod from "../../../mod.json";
import { getModule } from "cs2/modding";
import { ErrorCheckComponent } from "mods/errorCheckComponent/errorCheckComponent";
import { ErrorCheck } from "Domain/errorCheck";
import { game } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";

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
    const { translate } = useLocalization();

    const leftErrorChecks = ErrorChecks.slice(0,Math.ceil(ErrorChecks.length/2));
    const rightErrorChecks = ErrorChecks.slice(Math.ceil(ErrorChecks.length/2))
    return (
        <>
            {ShowPanel && !isPhotoMode && showToolIcon && (
                <Portal>
                    <Panel
                        className={styles.panel}
                        header={(
                            <VanillaComponentResolver.instance.Section title={translate(mod.id+".SECTION_TITLE["+"AnarchyOptions"+"]")}>
                                <Button className={roundButtonHighlightStyle.button} variant="icon" onSelect={() => handleClick("ToggleAnarchyOptionsPanel")} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}>
                                    <img src={closeSrc}></img>
                                </Button>
                            </VanillaComponentResolver.instance.Section>
                        )}>
                        <div className={styles.rowGroup}>
                            <div className={styles.columnGroup}>
                                <div className={styles.subtitleRow}>
                                    <div className={styles.subtitleLeft}>{translate(mod.id+".SECTION_TITLE["+"ErrorCheck"+"]", "Error Check")}</div>
                                    <span className={styles.subtitleSpanMiddle}></span>
                                    <div className={styles.subtitleRight}>{translate(mod.id+".SECTION_TITLE["+"Disabled"+"]", "Disabled?")}</div>
                                </div>
                                { leftErrorChecks.map((currentErrorCheck) => (
                                    <ErrorCheckComponent errorCheck={currentErrorCheck}></ErrorCheckComponent> 
                                ))}
                            </div>
                            <div className={styles.columnGroup}>
                                <div className={styles.subtitleRow}>
                                    <div>{translate(mod.id+".SECTION_TITLE["+"ErrorCheck"+"]", "Error Check")}</div>
                                    <span className={styles.subtitleSpanMiddle}></span>
                                    <div>{translate(mod.id+".SECTION_TITLE["+"Disabled"+"]", "Disabled?")}</div>
                                    <span className={styles.subtitleSpanRight}></span>
                                </div>
                                { rightErrorChecks.map((currentErrorCheck) => (
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