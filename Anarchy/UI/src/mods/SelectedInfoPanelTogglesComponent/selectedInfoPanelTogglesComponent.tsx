import { getModule } from "cs2/modding";
import { Theme, FocusKey, UniqueFocusKey } from "cs2/bindings";
import { bindValue, trigger, useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import locale from "../../lang/en-US.json";

interface InfoSectionComponent {
	group: string;
	tooltipKeys: Array<string>;
	tooltipTags: Array<string>;
}


const uilStandard =                          "coui://uil/Standard/";
const uilColored =                           "coui://uil/Colored/";
const anarchyEnabledSrc =      uilColored +  "Anarchy.svg";
const anarchyDisabledSrc =     uilStandard + "Anarchy.svg";
const transformRecordSrc =      uilStandard + "NoHeightLimit.svg";

const hasPreventOverride$ = bindValue<boolean>(mod.id, 'HasPreventOverride');
const hasTransformRecord$ = bindValue<boolean>(mod.id, 'HasTransformRecord');

const InfoSectionTheme: Theme | any = getModule(
	"game-ui/game/components/selected-info-panel/shared-components/info-section/info-section.module.scss",
	"classes"
);

const InfoRowTheme: Theme | any = getModule(
	"game-ui/game/components/selected-info-panel/shared-components/info-row/info-row.module.scss",
	"classes"
)

const InfoSection: any = getModule( 
    "game-ui/game/components/selected-info-panel/shared-components/info-section/info-section.tsx",
    "InfoSection"
)

const InfoRow: any = getModule(
    "game-ui/game/components/selected-info-panel/shared-components/info-row/info-row.tsx",
    "InfoRow"
)

function handleClick(eventName : string) {
    // This triggers an event on C# side and C# designates the method to implement.
    trigger(mod.id, eventName);
}

const FocusDisabled$: FocusKey = getModule(
	"game-ui/common/focus/focus-key.ts",
	"FOCUS_DISABLED"
);

const descriptionToolTipStyle = getModule("game-ui/common/tooltip/description-tooltip/description-tooltip.module.scss", "classes");
    

// This is working, but it's possible a better solution is possible.
function descriptionTooltip(tooltipTitle: string | null, tooltipDescription: string | null) : JSX.Element {
    return (
        <>
            <div className={descriptionToolTipStyle.title}>{tooltipTitle}</div>
            <div className={descriptionToolTipStyle.content}>{tooltipDescription}</div>
        </>
    );
}

export const SelectedInfoPanelTogglesComponent = (componentList: any): any => {
    // I believe you should not put anything here.
	componentList["Anarchy.Systems.SelectedInfoPanelTogglesSystem"] = (e: InfoSectionComponent) => {
        // These get the value of the bindings.
        const hasPreventOverride : boolean = useValue(hasPreventOverride$);
        const hasTransformRecord : boolean = useValue(hasTransformRecord$);
        // translation handling. Translates using locale keys that are defined in C# or fallback string here.
        const { translate } = useLocalization();
        const anarchySectionTitle = translate("YY_ANARCHY.Anarchy", locale["YY_ANARCHY.Anarchy"]);
        const preventOverrideTooltipKey = translate("Anarchy.TOOLTIP_TITLE[PreventOverrideButton]" ,locale["Anarchy.TOOLTIP_TITLE[PreventOverrideButton]"]);
        const preventOverrideTooltipDescription = translate("Anarchy.TOOLTIP_DESCRIPTION[PreventOverrideButton]" ,locale["Anarchy.TOOLTIP_DESCRIPTION[PreventOverrideButton]"]);
        const transformRecordTooltipKey = translate("Anarchy.TOOLTIP_TITLE[TransformRecordButton]" ,locale["Anarchy.TOOLTIP_TITLE[TransformRecordButton]"]);
        const transformRecordTooltipDescription = translate("Anarchy.TOOLTIP_DESCRIPTION[TransformRecordButton]" ,locale["Anarchy.TOOLTIP_DESCRIPTION[TransformRecordButton]"]);
        const anarchyModComponentsTooltipKey = translate("Anarchy.TOOLTIP_TITLE[AnarchyModComponets]" ,locale["Anarchy.TOOLTIP_TITLE[AnarchyModComponets]"]);


        return 	<InfoSection focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} disableFocus={true} className={InfoSectionTheme.infoSection}>
                        <InfoRow 
                            left={anarchySectionTitle}
                            right=
                            {
                                <>
                                    <VanillaComponentResolver.instance.ToolButton
                                        src={hasPreventOverride ? anarchyEnabledSrc : anarchyDisabledSrc}
                                        selected = {hasPreventOverride}
                                        multiSelect = {false}   // I haven't tested any other value here
                                        disabled = {false}      // I haven't tested any other value here
                                        tooltip = {descriptionTooltip(preventOverrideTooltipKey, preventOverrideTooltipDescription)}
                                        className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                        onSelect={() => handleClick("PreventOverrideButtonToggled")}
                                    />
                                    <VanillaComponentResolver.instance.ToolButton
                                        src={transformRecordSrc}
                                        selected = {hasTransformRecord}
                                        multiSelect = {false}   // I haven't tested any other value here
                                        disabled = {false}      // I haven't tested any other value here
                                        tooltip = {descriptionTooltip(transformRecordTooltipKey, transformRecordTooltipDescription)}
                                        className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                        onSelect={() => handleClick("TransformRecordButtonToggled")}
                                    />
                                </>
                            }
                            tooltip={anarchyModComponentsTooltipKey}
                            uppercase={true}
                            disableFocus={true}
                            subRow={false}
                            className={InfoRowTheme.infoRow}
                        ></InfoRow>
                </InfoSection>
				;
    }

	return componentList as any;
}

/*
      var t = e.icon
              , n = e.left
              , r = e.right
              , i = e.tooltip
              , o = e.link
              , a = e.uppercase
              , s = void 0 !== a && a
              , l = e.subRow
              , c = void 0 !== l && l
              , u = e.disableFocus
              , d = void 0 !== u && u
              , f = e.className;
              /*
/*


                        /*
/*
let VS = {
            "info-row": "info-row_QQ9 item-focused_FuT",
            infoRow: "info-row_QQ9 item-focused_FuT",
            "disable-focus-highlight": "disable-focus-highlight_I85",
            disableFocusHighlight: "disable-focus-highlight_I85",
            link: "link_ICj",
            tooltipRow: "tooltipRow_uIh",
            left: "left_RyE",
            hasIcon: "hasIcon_iZ3",
            right: "right_ZUb",
            icon: "icon_ugE",
            uppercase: "uppercase_f0y",
            subRow: "subRow_NJI"
        };

		 let fS = {
            "info-section": "info-section_I7V",
            infoSection: "info-section_I7V",
            content: "content_Cdk item-focused_FuT",
            column: "column_aPB",
            divider: "divider_rfM",
            "no-margin": "no-margin_K7I",
            noMargin: "no-margin_K7I",
            "disable-focus-highlight": "disable-focus-highlight_ik3",
            disableFocusHighlight: "disable-focus-highlight_ik3",
            "info-wrap-box": "info-wrap-box_Rt4",
            infoWrapBox: "info-wrap-box_Rt4"
        };
		*/