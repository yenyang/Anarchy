import {ModuleRegistryExtend} from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import mod from "../../../mod.json";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";
import { useLocalization } from "cs2/l10n";
import { getModule } from "cs2/modding";
import locale from "../../lang/en-US.json"
import plusOrMinusSrc from "../../images/PlusOrMinus.svg";

// These contain the coui paths to Unified Icon Library svg assets
const couiStandard =                         "coui://uil/Standard/";
const arrowDownSrc =         couiStandard +  "ArrowDownThickStroke.svg";
const arrowUpSrc =           couiStandard +  "ArrowUpThickStroke.svg";
const resetSrc =            couiStandard + "Reset.svg";
const heightLockSrc = couiStandard + "ArrowsHeightLocked.svg";

// These establishes the binding with C# side. Without C# side game ui will crash.
const ElevationValue$ =     bindValue<number> (mod.id, 'ElevationValue');
const ElevationStep$ =      bindValue<number> (mod.id, 'ElevationStep');
const ElevationScale$ =     bindValue<number> (mod.id, 'ElevationScale');
const LockElevation$ =     bindValue<boolean> (mod.id, 'LockElevation');
const IsInappropriate$ =         bindValue<boolean>(mod.id, 'IsInappropriate');
const ShowElevationOption$ = bindValue<boolean>(mod.id, 'ShowElevationSettingsOption');
const ObjectToolValidMode$ = bindValue<boolean>(mod.id, 'ObjectToolValidMode');
const DisableElevationLock$ = bindValue<boolean>(mod.id, 'DisableElevationLock');
const ElevationVariance$ = bindValue<number> (mod.id, 'ElevationVariance');
const ElevationVarianceStep$ = bindValue<number> (mod.id, 'ElevationVarianceStep');
const ShowElevationVariance$ = bindValue<boolean>(mod.id, 'ShowElevationVariance');

// Stores the default values for the step arrays. Must be descending order.
const defaultValues : number[] = [10, 2.5, 1.0, 0.1];
const defaultVarianceValues : number[] = [10, 2.5, 1.0, 0.1];

// This functions trigger an event on C# side and C# designates the method to implement.
function handleClick(eventName: string) {
    trigger(mod.id, eventName);
}

const descriptionToolTipStyle = getModule("game-ui/common/tooltip/description-tooltip/description-tooltip.module.scss", "classes");
    

// This is working, but it's possible a better solution is possible.
export function descriptionTooltip(tooltipTitle: string | null, tooltipDescription: string | null) : JSX.Element {
    return (
        <>
            <div className={descriptionToolTipStyle.title}>{tooltipTitle}</div>
            <div className={descriptionToolTipStyle.content}>{tooltipDescription}</div>
        </>
    );
}

export const ElevationControlComponent: ModuleRegistryExtend = (Component : any) => {
    // I believe you should not put anything here.
    return (props) => {
        const {children, ...otherProps} = props || {};

        // These get the value of the bindings.
        const toolId = useValue(tool.activeTool$).id;
        var toolAppropriate = (toolId == tool.OBJECT_TOOL || toolId == "Line Tool");

        const ElevationValue = useValue(ElevationValue$);
        const ElevationStep = useValue(ElevationStep$);
        const ElevationScale = useValue(ElevationScale$);
        const LockElevation = useValue(LockElevation$);
        const IsInappropriate = useValue(IsInappropriate$);
        const ShowElevationOption = useValue(ShowElevationOption$);
        const ObjectToolValidMode = useValue(ObjectToolValidMode$);
        const DisableElevationLock = useValue(DisableElevationLock$);
        const ElevationVariance = useValue(ElevationVariance$);
        const ElevationVarianceStep = useValue(ElevationVarianceStep$);
        const ShowElevationVariance = useValue(ShowElevationVariance$);

        if (toolId == tool.OBJECT_TOOL && !ObjectToolValidMode) 
        {
            toolAppropriate = false;
        }

        // translation handling. Translates using locale keys that are defined in C# or fallback string here.
        const { translate } = useLocalization();
        const elevationSectionTitle = translate("Toolbar.ELEVATION_TITLE" , "Elevation");
        const elevationUpDescription = translate("Anarchy.TOOLTIP_DESCRIPTION[IncreaseElevation]" ,locale["Anarchy.TOOLTIP_DESCRIPTION[IncreaseElevation]"]);
        const elevationDownDescription = translate("Anarchy.TOOLTIP_DESCRIPTION[DecreaseElevation]" ,locale["Anarchy.TOOLTIP_DESCRIPTION[DecreaseElevation]"]);
        const elevationStepDescription = translate("Anarchy.TOOLTIP_DESCRIPTION[ElevationStep]" ,locale["Anarchy.TOOLTIP_DESCRIPTION[ElevationStep]"]);
        const elevationLockTitle = translate("Anarchy.TOOLTIP_TITLE[ElevationLock]" ,locale["Anarchy.TOOLTIP_TITLE[ElevationLock]"]);
        const elevationLockDescription = translate("Anarchy.TOOLTIP_DESCRIPTION[ElevationLock]" ,locale["Anarchy.TOOLTIP_DESCRIPTION[ElevationLock]"]);
        const resetElevationDescription = translate( "Anarchy.TOOLTIP_DESCRIPTION[ResetElevation]",locale["Anarchy.TOOLTIP_DESCRIPTION[ResetElevation]"]);

        var result = Component();
        
        if (toolAppropriate && !IsInappropriate && ShowElevationOption) 
        {
            result.props.children?.push
            (
                /* 
                Add a new section before other tool options sections with translated title based of localization key from binding. Localization key defined in C#.
                Adds up and down buttons and field with step button. All buttons have translated tooltips. OnSelect triggers C# events. Src paths are local imports.
                values must be decending. SelectedValue is from binding. 
                */
                <>
                    <VanillaComponentResolver.instance.Section title={elevationSectionTitle}>
                          <VanillaComponentResolver.instance.ToolButton 
                            className={VanillaComponentResolver.instance.toolButtonTheme.button} 
                            selected = {LockElevation}
                            tooltip={descriptionTooltip(elevationLockTitle, elevationLockDescription)} 
                            onSelect={() => handleClick("LockElevationToggled")} 
                            src={heightLockSrc}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            disabled={DisableElevationLock}
                        ></VanillaComponentResolver.instance.ToolButton>
                         <VanillaComponentResolver.instance.ToolButton 
                            className={VanillaComponentResolver.instance.toolButtonTheme.button} 
                            selected = {ShowElevationVariance}
                            tooltip={descriptionTooltip(translate("Anarchy.SECTION_TITLE[ElevationVariance]" ,locale["Anarchy.SECTION_TITLE[ElevationVariance]"]) , translate("Anarchy.TOOLTIP_DESCRIPTION[ShowElevationVariance]", locale["Anarchy.TOOLTIP_DESCRIPTION[ShowElevationVariance]"]))} 
                            onSelect={() => { handleClick("ToggleShowElevationVariance"); console.log("hello") } } 
                            src={plusOrMinusSrc}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                         ></VanillaComponentResolver.instance.ToolButton>
                        <VanillaComponentResolver.instance.ToolButton 
                            className={VanillaComponentResolver.instance.mouseToolOptionsTheme.startButton} 
                            tooltip={elevationDownDescription} 
                            onSelect={() => handleClick("DecreaseElevation")} 
                            src={arrowDownSrc}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                        ></VanillaComponentResolver.instance.ToolButton>
                        <div className={VanillaComponentResolver.instance.mouseToolOptionsTheme.numberField}>{ ElevationValue.toFixed(ElevationScale) + " m"}</div>
                        <VanillaComponentResolver.instance.ToolButton 
                            className={VanillaComponentResolver.instance.mouseToolOptionsTheme.endButton} 
                            tooltip={elevationUpDescription} 
                            onSelect={() => handleClick("IncreaseElevation")} 
                            src={arrowUpSrc}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                        ></VanillaComponentResolver.instance.ToolButton>
                        <VanillaComponentResolver.instance.StepToolButton tooltip={elevationStepDescription} onSelect={() => handleClick("ElevationStep")} values={defaultValues} selectedValue={ElevationStep}></VanillaComponentResolver.instance.StepToolButton>
                        <VanillaComponentResolver.instance.ToolButton 
                                className={VanillaComponentResolver.instance.toolButtonTheme.button} 
                                tooltip={resetElevationDescription} 
                                onSelect={() => handleClick("ResetElevationToggled")} 
                                src={resetSrc}
                                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                        ></VanillaComponentResolver.instance.ToolButton>
                    </VanillaComponentResolver.instance.Section>   
                    <>    
                        {ShowElevationVariance && (    
                            <VanillaComponentResolver.instance.Section title={translate("Anarchy.SECTION_TITLE[ElevationVariance]" ,locale["Anarchy.SECTION_TITLE[ElevationVariance]"])}>
                                <VanillaComponentResolver.instance.ToolButton 
                                    className={VanillaComponentResolver.instance.mouseToolOptionsTheme.startButton} 
                                    tooltip={translate("Anarchy.TOOLTIP_DESCRIPTION[DecreaseElevationVariance]" ,locale["Anarchy.TOOLTIP_DESCRIPTION[DecreaseElevationVariance]"])} 
                                    onSelect={() => handleClick("DecreaseElevationVariance")} 
                                    src={arrowDownSrc}
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                    disabled={ElevationVariance == 0}
                                ></VanillaComponentResolver.instance.ToolButton>
                                <div className={VanillaComponentResolver.instance.mouseToolOptionsTheme.numberField}>{ ElevationVariance.toFixed(ElevationScale) + " m"}</div>
                                <VanillaComponentResolver.instance.ToolButton 
                                    className={VanillaComponentResolver.instance.mouseToolOptionsTheme.endButton} 
                                    tooltip={translate("Anarchy.TOOLTIP_DESCRIPTION[IncreaseElevationVariance]" ,locale["Anarchy.TOOLTIP_DESCRIPTION[IncreaseElevationVariance]"])} 
                                    onSelect={() => handleClick("IncreaseElevationVariance")} 
                                    src={arrowUpSrc}
                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                ></VanillaComponentResolver.instance.ToolButton>
                                <VanillaComponentResolver.instance.StepToolButton tooltip={translate("Anarchy.TOOLTIP_DESCRIPTION[ElevationVarianceStep]" ,locale["Anarchy.TOOLTIP_DESCRIPTION[ElevationVarianceStep]"])} onSelect={() => handleClick("ElevationVarianceStep")} values={defaultVarianceValues} selectedValue={ElevationVarianceStep}></VanillaComponentResolver.instance.StepToolButton>
                                <VanillaComponentResolver.instance.ToolButton 
                                        className={VanillaComponentResolver.instance.toolButtonTheme.button} 
                                        tooltip={resetElevationDescription} 
                                        onSelect={() => handleClick("ResetElevationVariance")} 
                                        src={resetSrc}
                                        focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                ></VanillaComponentResolver.instance.ToolButton>
                            </VanillaComponentResolver.instance.Section>   
                        )}     
                    </>       
                </>
            );
        }
        
        return result;
    };
}