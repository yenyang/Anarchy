import { getModule } from "cs2/modding";
import styles from "./errorCheckComponent.module.scss";
import { Button } from "cs2/ui";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { bindValue, trigger, useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { ErrorCheck } from "Domain/errorCheck";
import { useState } from "react";
import classNames from "classnames";

const uilStandard =                         "coui://uil/Standard/";
const arrowLeftSrc =           uilStandard +  "ArrowLeftThickStroke.svg";
const arrowRightSrc =           uilStandard +  "ArrowRightThickStroke.svg";

const ErrorChecks$ =           bindValue<ErrorCheck[]> (mod.id, "ErrorChecks");
const MultipleUniques$ =        bindValue<boolean>(mod.id, "MultipleUniques");


function handleClick(index: number, newState : number) {
    trigger(mod.id, "ChangeDisabledState", index, newState);
}

const roundButtonHighlightStyle = getModule("game-ui/common/input/button/themes/round-highlight-button.module.scss", "classes");

export const ErrorCheckComponent = (props: { errorCheck : ErrorCheck }) => {
    const { translate } = useLocalization();
    
    const ErrorChecks = useValue(ErrorChecks$);
    const MultipleUniques = useValue(MultipleUniques$);

    function getDisableStateText(state: number) : string | null {
        let stateText = translate("YY_ANARCHY.AnarchyButton", "Anarchy");
        if (state == 2 || (MultipleUniques && props.errorCheck.ID == 18)) {
            stateText = translate("Anarchy.UI_TEXT[Always]", "Always");
        } else if (state == 0 ) {
            stateText = translate("Anarchy.UI_TEXT[Never]", "Never") ;
        } 
        return stateText;
    }

    let [disableState, changeState] = useState<number>(ErrorChecks[props.errorCheck.Index].DisabledState);

    return (
        <div className={classNames(styles.rowGroup, styles.definedHeight)}>
            { disableState >= 1 && (!MultipleUniques || props.errorCheck.ID != 18)? 
                (
                    <Button className={roundButtonHighlightStyle.button} variant="icon" onSelect={() => {handleClick(props.errorCheck.Index, disableState-1); changeState(disableState-1);} } focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}>
                        <img src={arrowLeftSrc}></img>
                    </Button>
                ) : 
                ( 
                    <span className={styles.spacer}></span>
                )
            }
            <div className={styles.disableState}>{getDisableStateText(disableState)}</div>
            { disableState <= 1 && (!MultipleUniques || props.errorCheck.ID != 18)? 
                (
                    <Button className={roundButtonHighlightStyle.button} variant="icon" onSelect={() => {handleClick(props.errorCheck.Index, disableState+1); changeState(disableState+1);}} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}>
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