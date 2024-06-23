import { getModule } from "cs2/modding";
import styles from "./errorCheckComponent.module.scss";
import { Button } from "cs2/ui";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { trigger } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { ErrorCheck } from "Domain/errorCheck";
import { useState } from "react";

const uilStandard =                         "coui://uil/Standard/";
const arrowLeftSrc =           uilStandard +  "ArrowLeftThickStroke.svg";
const arrowRightSrc =           uilStandard +  "ArrowRightThickStroke.svg";


function handleClick(index: number, newState : number) {
    trigger(mod.id, "ChangeDisabledState", index, newState);
}

const roundButtonHighlightStyle = getModule("game-ui/common/input/button/themes/round-highlight-button.module.scss", "classes");

export const ErrorCheckComponent = (props: { errorCheck : ErrorCheck }) => {
    const { translate } = useLocalization();
    

    function getDisableStateText(state: number) : string {
        let stateText = "Anarchy";
        if (state == 0) {
            stateText = "Never" ;
        } else if (state == 2) {
            stateText = "Always";
        }
        return stateText;
    }

    let [disableState, changeState] = useState<number>(props.errorCheck.DisabledState);

    return (
        <div className={styles.rowGroup}>
            <div className={styles.errorCheckName}>{translate(props.errorCheck.LocaleKey)}</div>
            { disableState - 1 >= 0 ? 
                (
                    <Button className={roundButtonHighlightStyle.button + " " + styles.smallButton} variant="icon" onSelect={() => {handleClick(props.errorCheck.Index, disableState-1); changeState(disableState-1);} } focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}>
                        <img src={arrowLeftSrc}></img>
                    </Button>
                ) : 
                ( 
                    <span className={styles.spacer}></span>
                )
            }
            <div className={styles.disableState}>{getDisableStateText(disableState)}</div>
            { disableState + 1 <= 2 ? 
                (
                    <Button className={roundButtonHighlightStyle.button + " " + styles.smallButton} variant="icon" onSelect={() => {handleClick(props.errorCheck.Index, disableState+1); changeState(disableState+1);}} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}>
                        <img src={arrowRightSrc}></img>
                    </Button>
                ) :
                ( 
                    <span className={styles.spacer}></span>
                )
            }
        </div>
      );
}