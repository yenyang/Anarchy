
import { Button, Panel, Portal } from "cs2/ui";
import styles from "./partialAnarchyMenu.module.scss"
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import { bindValue, trigger, useValue } from "cs2/api";
import mod from "../../../mod.json";
import { getModule } from "cs2/modding";
import { ErrorCheckComponent } from "mods/errorCheckComponent/errorCheckComponent";
import { ErrorCheck } from "Domain/errorCheck";

const uilStandard =                         "coui://uil/Standard/";

const closeSrc =         uilStandard +  "XClose.svg";
const arrowUpSrc =           uilStandard +  "ArrowUpThickStroke.svg";

const ErrorChecks$ =           bindValue<ErrorCheck[]> (mod.id, "ErrorChecks");

function handleClick(event: string) {
    trigger(mod.id, event);
}

const roundButtonHighlightStyle = getModule("game-ui/common/input/button/themes/round-highlight-button.module.scss", "classes");

export const PartialAnarchyMenyComponent = () => {
    
    const ErrorChecks = useValue(ErrorChecks$);
    return (
        <>
            <Portal>
                <Panel
                    className={styles.panel}
                    header={(
                        <VanillaComponentResolver.instance.Section title={"Anarchy Toggle Options"}>
                            <Button className={roundButtonHighlightStyle.button} variant="icon" onSelect={() => handleClick("Close")} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}>
                                <img src={closeSrc}></img>
                            </Button>
                        </VanillaComponentResolver.instance.Section>
                    )}>
                    <div className={styles.rowGroup}>
                        <div className={styles.columnGroup}>
                            <div className={styles.subtitleRow}>
                                <div className={styles.subtitleLeft}>Error Type</div>
                                <div className={styles.subtitleRight}>Disabled?</div>
                            </div>
                            { ErrorChecks.map((currentErrorCheck) => (
                                <ErrorCheckComponent errorCheck={currentErrorCheck}></ErrorCheckComponent> 
                            ))}
                        </div>
                    </div>
                </Panel>
            </Portal>
        </>
    );
}