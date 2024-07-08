
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
import classNames from "classnames";

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

export const PartialAnarchyMenuComponent = () => {
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
                            <div className={classNames(styles.columnGroup, styles.leftColumn)}>
                                <div className={styles.subtitleRow}>{translate(mod.id+".SECTION_TITLE["+"ErrorCheck"+"]", "Error Check")}</div>                                
                                { leftErrorChecks.map((currentErrorCheck) => (
                                    <div className={styles.definedHeight}>{translate(currentErrorCheck.LocaleKey)}</div>
                                ))}
                            </div>
                            <div className={styles.columnGroup}>
                                <div className={classNames(styles.subtitleRow, styles.centeredSubTitle)}>{translate(mod.id+".SECTION_TITLE["+"Disabled"+"]", "Disabled?")}</div>
                                { leftErrorChecks.map((currentErrorCheck) => (
                                    <ErrorCheckComponent errorCheck={currentErrorCheck}></ErrorCheckComponent> 
                                ))}
                            </div>
                            <div className={styles.columnGroup}>
                                <div className={styles.subtitleRow}>{translate(mod.id+".SECTION_TITLE["+"ErrorCheck"+"]", "Error Check")}</div>
                                { rightErrorChecks.map((currentErrorCheck) => (
                                    <div className={styles.definedHeight}>{translate(currentErrorCheck.LocaleKey)}</div>
                                ))}
                                { rightErrorChecks.length < leftErrorChecks.length && (
                                    <div className={styles.definedHeight}></div>
                                )}
                            </div>
                            <div className={classNames(styles.columnGroup, styles.rightColumn)}>
                                <div className={classNames(styles.subtitleRow, styles.centeredSubTitle)}>{translate(mod.id+".SECTION_TITLE["+"Disabled"+"]", "Disabled?")}</div>
                                { rightErrorChecks.map((currentErrorCheck) => (
                                    <ErrorCheckComponent errorCheck={currentErrorCheck}></ErrorCheckComponent> 
                                ))}
                                { rightErrorChecks.length < leftErrorChecks.length && (
                                    <div className={styles.definedHeight}></div>
                                )}
                            </div>
                        </div>
                    </Panel>
                </Portal>
            )}
        </>
    );
}