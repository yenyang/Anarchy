import iconStyles from "./chirperMod.module.scss";
import { bindValue, useValue } from "cs2/api";
import mod from "../../../mod.json";

// These establishes the binding with C# side. Without C# side game ui will crash.
export const anarchyEnabled$ = bindValue<boolean>(mod.id, 'AnarchyEnabled');
export const flamingChirperOption$ = bindValue<boolean>(mod.id, 'FlamingChirperOption');

// These contain the coui paths to Unified Icon Library svg assets
export const uilColored =                         "coui://uil/Colored/";
export const anarchyEnabledSrc =          uilColored +  "AnarchyChirper.svg";

export const ChirperModComponent = () => {

    // These get the value of the bindings.
    const anarchyEnabled : boolean = useValue(anarchyEnabled$);
    const flamingChirperOption : boolean = useValue(flamingChirperOption$);
    
    // This takes the two bools from the bindings and condenses it down to a single bool for both being true.
    const showFlamingChirper : boolean = anarchyEnabled && flamingChirperOption;

    // This either returns an empty JSX component or the flaming chirper image. Sass is used to determine absolute position, size, and to set z-index. Setting pointer events to none was precautionary. 
    return (
        <>
            {showFlamingChirper && (
                <img src={anarchyEnabledSrc} className ={iconStyles.flamingChirperIcon}></img>
            )}
        </>
    );
}