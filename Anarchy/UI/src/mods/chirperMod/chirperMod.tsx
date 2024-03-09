import anarchyEnabledSrc from "./AnarchyChirper.svg";
import iconStyles from "./chirperMod.module.scss";
import { bindValue, useValue } from "cs2/api";
import mod from "../../../mod.json";

// These establishes the binding with C# side. Without C# side game ui will crash.
export const anarchyEnabled$ = bindValue<boolean>(mod.id, 'AnarchyEnabled');
export const flamingChirperOption$ = bindValue<boolean>(mod.id, 'FlamingChirperOption');

export const ChirperModComponent = () => {

    // These get the value of the bindings.
    const anarchyEnabled : boolean = useValue(anarchyEnabled$);
    const flamingChirperOption : boolean = useValue(flamingChirperOption$);
    
    const showFlamingChirper : boolean = anarchyEnabled && flamingChirperOption;

    return (
        <>
            {showFlamingChirper && (
                <img src={anarchyEnabledSrc} className ={iconStyles.flamingChirperIcon}></img>
            )}
        </>
    );
}