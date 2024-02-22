/// <reference types="react" />
declare module "common/utils/equality" {
    export type EqualityComparer<T> = (a: T, b: T) => boolean;
    export function defaultEqualityComparer<T>(a: T, b: T): boolean;
    interface Entity {
        index: number;
        version: number;
    }
    export function entityKey({ index, version }: Entity): string;
    export function parseEntityKey(value: any): Entity | undefined;
    export function entityEquals(a: Entity | null | undefined, b: Entity | null | undefined): boolean;
    export function isNullOrEmpty(s: string | null | undefined): boolean;
    /**
     * Performs equality by iterating through keys on an object and returning false
     * when any key has values which are not strictly equal between the arguments.
     * Returns true when the values of all keys are strictly equal.
     */
    export function shallowEqual(a: any, b: any, depth?: number): boolean;
    export function stringifySorted(obj: any): any;
    export function stringifySortedIgnoreBindingType(obj: any): any;
    export function useMemoizedValue<T>(value: T, equalityComparer: EqualityComparer<T>): T;
}
declare module "common/data-binding/binding" {
    import { MutableRefObject } from 'react';
    export interface ValueBinding<T> {
        readonly value: T;
        subscribe(listener?: BindingListener<T>): ValueSubscription<T>;
        dispose(): void;
    }
    export interface MapBinding<K, V> {
        getValue(key: K): V;
        subscribe(key: K, listener?: BindingListener<V>): ValueSubscription<V>;
        dispose(): void;
    }
    export interface EventBinding<T> {
        subscribe(listener: BindingListener<T>): Subscription;
        dispose(): void;
    }
    export interface BindingListener<T> {
        (value: T): void;
    }
    export interface Subscription {
        dispose(): void;
    }
    export interface ValueSubscription<T> extends Subscription {
        readonly value: T;
        setChangeListener(listener: BindingListener<T>): void;
    }
    export function bindValue<T>(group: string, name: string, fallbackValue?: T): ValueBinding<T>;
    export function bindLocalValue<T>(initialValue: T): LocalValueBinding<T>;
    export function bindMap<K, V>(group: string, name: string, keyStringifier?: (key: K) => string): MapBinding<K, V>;
    export function bindEvent<T>(group: string, name: string): EventBinding<T>;
    export function bindTrigger(group: string, name: string): () => void;
    export function bindTriggerWithArgs<T extends any[] = []>(group: string, name: string): T extends [] ? unknown : (...args: T) => void;
    export function trigger(group: string, name: string, ...args: any[]): void;
    export function call<T>(group: string, name: string, ...args: any[]): Promise<T>;
    /** Subscribe to a ValueBinding. Return fallback value or throw an error if the binding is not registered on the C# side */
    export function useValue<V>(binding: ValueBinding<V>): V;
    export function useReducedValue<T, V>(binding: ValueBinding<V>, reducer: (current: T, next: V) => T, initial: T): T;
    export function useValueRef<V>(binding: ValueBinding<V>): MutableRefObject<V>;
    /** Subscribe to a MapBinding value. throw an error if the binding is not registered on the C# side */
    export function useMapValue<K, V>(binding: MapBinding<K, V>, key: K): V;
    export function useMapValues<K, V>(binding: MapBinding<K, V>, keys: K[]): V[];
    class LocalValueBinding<T> implements ValueBinding<T> {
        private readonly listeners;
        private disposed;
        private _value;
        constructor(initialValue: T);
        get registered(): boolean;
        get value(): T;
        subscribe: (listener?: BindingListener<T>) => {
            readonly value: T;
            setChangeListener: (listener: BindingListener<T>) => void;
            dispose(): void;
        };
        dispose: () => void;
        update: (newValue: T) => void;
    }
}
declare module "common/data-binding/binding-utils" {
    export type Typeless<T> = Omit<T, '__Type'>;
    export interface Typed<T extends string> {
        __Type: T;
    }
    export function getType<T extends string>(typed: Typed<T>): T;
    export function isBindingType<T extends string>(obj: any, typeName: T): obj is Typed<T>;
    export function typed<T extends {}, U extends string>(arg: T, typeName: U): T & Typed<U>;
    export type TypeFromMap<T extends Record<string, any>> = {
        [K in keyof T]: K extends string ? T[K] & Typed<K> : never;
    }[keyof T];
}
declare module "common/data-binding/common-types" {
    export interface Entity {
        index: number;
        version: number;
    }
    export const nullEntity: Entity;
}
declare module "game/data-binding/camera-bindings" {
    import { Entity } from "common/data-binding/common-types";
    export const focusedEntity$: import("common/data-binding/binding").ValueBinding<Entity>;
    export function focusEntity(entity: Entity): void;
}
declare module "game/data-binding/city-info-bindings" {
    export const residentialLowDemand$: import("common/data-binding/binding").ValueBinding<number>;
    export const residentialMediumDemand$: import("common/data-binding/binding").ValueBinding<number>;
    export const residentialHighDemand$: import("common/data-binding/binding").ValueBinding<number>;
    export const commercialDemand$: import("common/data-binding/binding").ValueBinding<number>;
    export const industrialDemand$: import("common/data-binding/binding").ValueBinding<number>;
    export const officeDemand$: import("common/data-binding/binding").ValueBinding<number>;
    export const residentialLowFactors$: import("common/data-binding/binding").ValueBinding<Factor[]>;
    export const residentialMediumFactors$: import("common/data-binding/binding").ValueBinding<Factor[]>;
    export const residentialHighFactors$: import("common/data-binding/binding").ValueBinding<Factor[]>;
    export const commercialFactors$: import("common/data-binding/binding").ValueBinding<Factor[]>;
    export const industrialFactors$: import("common/data-binding/binding").ValueBinding<Factor[]>;
    export const officeFactors$: import("common/data-binding/binding").ValueBinding<Factor[]>;
    export const happiness$: import("common/data-binding/binding").ValueBinding<number>;
    export const happinessFactors$: import("common/data-binding/binding").ValueBinding<Factor[]>;
    export interface Factor {
        factor: string;
        weight: number;
    }
}
declare module "common/math" {
    export const INT_MIN_VALUE = -2147483648;
    export const INT_MAX_VALUE = 2147483647;
    export const UINT_MAX_VALUE = 4294967295;
    export const FLOAT_MIN_VALUE = -3.402823e+38;
    export const FLOAT_MAX_VALUE = 3.402823e+38;
    export function clamp(num: number, min: number, max: number): number;
    export function lerp(a: number, b: number, t: number): number;
    export function roundDecimal(num: number, precision: number): number;
    export interface Number2 {
        readonly x: number;
        readonly y: number;
    }
    export const zero2: Number2;
    export const left: Number2;
    export const up: Number2;
    export const right: Number2;
    export const down: Number2;
    export function number2Equals(a: Number2, b: Number2): boolean;
    export function clamp2(num: Number2, min: number, max: number): Number2;
    export function normalize2(num: Number2): Number2;
    export function difference2(a: Number2, b: Number2): Number2;
    export function lerp2(a: Number2, b: Number2, t: number): Number2;
    export function trunc2(num: Number2): Number2;
    export function distance2(value: Number2): number;
    export function distanceSqr2(value: Number2): number;
    export function formatNumber2({ x, y }: Number2, precision: number): string;
    export function fromMouseEvent({ clientX, clientY }: {
        clientX: number;
        clientY: number;
    }): Number2;
    export interface Number3 {
        readonly x: number;
        readonly y: number;
        readonly z: number;
    }
    export const zero3: Number3;
    export function number3Equals(a: Number3, b: Number3): boolean;
    export function formatNumber3({ x, y, z }: Number3, precision: number): string;
    export interface Number4 {
        readonly x: number;
        readonly y: number;
        readonly z: number;
        readonly w: number;
    }
    export const zero4: Number4;
    export function number4Equals(a: Number4, b: Number4): boolean;
    export function formatNumber4({ x, y, z, w }: Number4, precision: number): string;
    export interface Bounds1 {
        readonly min: number;
        readonly max: number;
    }
    export const zeroBounds1: Bounds1;
    export function bounds1Equals(a: Bounds1, b: Bounds1): boolean;
    export interface Bounds2 {
        readonly min: Number2;
        readonly max: Number2;
    }
    export const zeroBounds2: Bounds2;
    export function bounds2Equals(a: Bounds2, b: Bounds2): boolean;
    export interface Bounds3 {
        readonly min: Number3;
        readonly max: Number3;
    }
    export const zeroBounds3: Bounds3;
    export function bounds3Equals(a: Bounds3, b: Bounds3): boolean;
    export type LongNumber = [number, number];
    export const zeroLong: LongNumber;
    export function longEquals(a: LongNumber, b: LongNumber): boolean;
    export function longToBigInt(value: LongNumber): bigint;
    export function bigIntToLong(value: bigint): LongNumber;
    export interface Bezier4x3 {
        a: Number3;
        b: Number3;
        c: Number3;
        d: Number3;
    }
}
declare module "common/color" {
    /** RGBA color, all values between 0 and 1 */
    export interface Color {
        readonly r: number;
        readonly g: number;
        readonly b: number;
        readonly a: number;
    }
    export const black: Color;
    export const white: Color;
    export const transparent: Color;
    export function rgb(r: number, g: number, b: number): Color;
    export function rgba(r: number, g: number, b: number, a: number): Color;
    export function isColorEqual(colorA: Color, colorB: Color): boolean;
    export function isAchromatic({ r, g, b }: Color): boolean;
    export function withAlpha(color: Color, a: number): Color;
    export function formatColor({ r, g, b, a }: Color): string;
    export function parseRgba(rgbaString: string): Color | null;
    export function formatHexColor({ r, g, b }: Color): string;
    export function parseHexColor(hex: string): Color | null;
    /** HSVA color, all values between 0 and 1 */
    export interface HsvaColor {
        readonly h: number;
        readonly s: number;
        readonly v: number;
        readonly a: number;
    }
    export function hsvaToRgba({ h, s, v, a }: HsvaColor): Color;
    export function rgbaToHsva({ r, g, b, a }: Color, fallbackHue?: number): HsvaColor;
    export function lerpColor(a: Color, b: Color, t: number): Color;
    export function lerpHsva(a: HsvaColor, b: HsvaColor, t: number): HsvaColor;
    export interface Gradient {
        stops: GradientStop[];
    }
    export interface GradientStop {
        offset: number;
        color: string | Color;
    }
    export function buildCssLinearGradient(gradient: Gradient, direction?: string): string;
}
declare module "common/localization/unit" {
    export enum Unit {
        Integer = "integer",
        IntegerRounded = "integerRounded",
        IntegerPerMonth = "integerPerMonth",
        IntegerPerHour = "integerPerHour",
        FloatSingleFraction = "floatSingleFraction",
        FloatTwoFractions = "floatTwoFractions",
        FloatThreeFractions = "floatThreeFractions",
        Percentage = "percentage",
        PercentageSingleFraction = "percentageSingleFraction",
        Angle = "angle",
        Length = "length",
        Area = "area",
        Volume = "volume",
        VolumePerMonth = "volumePerMonth",
        Weight = "weight",
        WeightPerCell = "weightPerCell",
        WeightPerMonth = "weightPerMonth",
        Power = "power",
        Energy = "energy",
        DataRate = "dataRate",
        DataBytes = "dataBytes",
        DataMegabytes = "dataMegabytes",
        Money = "money",
        MoneyPerCell = "moneyPerCell",
        MoneyPerMonth = "moneyPerMonth",
        MoneyPerHour = "moneyPerHour",
        MoneyPerDistance = "moneyPerDistance",
        MoneyPerDistancePerMonth = "moneyPerDistancePerMonth",
        BodiesPerMonth = "bodiesPerMonth",
        XP = "xp",
        Temperature = "temperature",
        NetElevation = "netElevation"
    }
}
declare module "common/localization/localization-bindings" {
    import { Typed, TypeFromMap } from "common/data-binding/binding-utils";
    import { Unit } from "common/localization/unit";
    export enum LocalizationDebugMode {
        None = 0,
        Id = 1,
        Fallback = 2
    }
    export const locales$: import("common/data-binding/binding").ValueBinding<string[]>;
    export const debugMode$: import("common/data-binding/binding").ValueBinding<LocalizationDebugMode>;
    export const activeDictionaryChanged$: import("common/data-binding/binding").EventBinding<unknown>;
    export const indexCounts$: import("common/data-binding/binding").MapBinding<string, number>;
    export function selectLocale(localeId: string): void;
    export enum LocElementType {
        Bounds = "Game.UI.Localization.LocalizedBounds",
        Fraction = "Game.UI.Localization.LocalizedFraction",
        Number = "Game.UI.Localization.LocalizedNumber",
        String = "Game.UI.Localization.LocalizedString"
    }
    export interface LocElements {
        [LocElementType.Bounds]: LocalizedBounds;
        [LocElementType.Fraction]: LocalizedFraction;
        [LocElementType.Number]: LocalizedNumber;
        [LocElementType.String]: LocalizedString;
    }
    export type LocElement = TypeFromMap<LocElements>;
    interface LocalizedBounds {
        min: number;
        max: number;
        unit?: Unit;
    }
    interface LocalizedFraction {
        value: number;
        total: number;
        unit?: Unit;
    }
    interface LocalizedNumber {
        value: number;
        unit?: Unit;
        signed: boolean;
    }
    interface LocalizedString {
        id: string | null;
        value: string | null;
        args: Record<string, LocElement> | null;
    }
    export function locStrId(id: string): LocalizedString & Typed<LocElementType.String>;
    export function locStrValue(value: string): LocalizedString & Typed<LocElementType.String>;
}
declare module "common/property-field/typed-property" {
    import { Unit } from "common/localization/unit";
    import { Number2 } from "common/math";
    export type NumericProperty = NumberProperty | Number2Property;
    export type Property = NumericProperty | StringProperty;
    export const NUMBER_PROPERTY = "Game.UI.Common.NumberProperty";
    export interface NumberProperty {
        labelId: string;
        unit: Unit;
        value: number;
        signed: boolean;
    }
    export const NUMBER2_PROPERTY = "Game.UI.Common.Number2Property";
    export interface Number2Property {
        labelId: string;
        unit: Unit;
        value: Number2;
        signed: boolean;
    }
    export const STRING_PROPERTY = "Game.UI.Common.StringProperty";
    export interface StringProperty {
        labelId: string;
        valueId: string;
    }
    export type Properties = {
        [NUMBER_PROPERTY]: NumberProperty;
        [NUMBER2_PROPERTY]: Number2Property;
        [STRING_PROPERTY]: StringProperty;
    };
}
declare module "game/data-binding/prefab/prefab-effects" {
    import { TypeFromMap } from "common/data-binding/binding-utils";
    import { Unit } from "common/localization/unit";
    export enum PrefabEffectType {
        CityModifier = "prefabs.CityModifierEffect",
        LocalModifier = "prefabs.LocalModifierEffect",
        LeisureProvider = "prefabs.LeisureProviderEffect",
        AdjustHappinessEffect = "prefabs.AdjustHappinessEffect"
    }
    export interface PrefabEffects {
        [PrefabEffectType.CityModifier]: CityModifierEffect;
        [PrefabEffectType.LocalModifier]: LocalModifierEffect;
        [PrefabEffectType.LeisureProvider]: LeisureProviderEffect;
        [PrefabEffectType.AdjustHappinessEffect]: AdjustHappinessEffect;
    }
    export type PrefabEffect = TypeFromMap<PrefabEffects>;
    export interface CityModifierEffect {
        modifiers: CityModifier[];
    }
    export interface CityModifier {
        type: CityModifierType;
        delta: number;
        unit: Unit;
    }
    export enum CityModifierType {
        Attractiveness = "Attractiveness",
        CrimeAccumulation = "CrimeAccumulation",
        PoliceStationUpkeep = "PoliceStationUpkeep",
        DisasterWarningTime = "DisasterWarningTime",
        DisasterDamageRate = "DisasterDamageRate",
        DiseaseProbability = "DiseaseProbability",
        ParkEntertainment = "ParkEntertainment",
        CriminalMonitorProbability = "CriminalMonitorProbability",
        IndustrialAirPollution = "IndustrialAirPollution",
        IndustrialGroundPollution = "IndustrialGroundPollution",
        IndustrialGarbage = "IndustrialGarbage",
        RecoveryFailChange = "RecoveryFailChange",
        OreResourceAmount = "OreResourceAmount",
        OilResourceAmount = "OilResourceAmount",
        UniversityInterest = "UniversityInterest",
        OfficeSoftwareDemand = "OfficeSoftwareDemand",
        IndustrialElectronicsDemand = "IndustrialElectronicsDemand",
        OfficeSoftwareEfficiency = "OfficeSoftwareEfficiency",
        IndustrialElectronicsEfficiency = "IndustrialElectronicsEfficiency",
        TelecomCapacity = "TelecomCapacity",
        Entertainment = "Entertainment",
        HighwayTrafficSafety = "HighwayTrafficSafety",
        PrisonTime = "PrisonTime",
        CrimeProbability = "CrimeProbability",
        CollegeGraduation = "CollegeGraduation",
        UniversityGraduation = "UniversityGraduation",
        ImportCost = "ImportCost",
        LoanInterest = "LoanInterest",
        BuildingLevelingCost = "BuildingLevelingCost",
        ExportCost = "ExportCost",
        TaxiStartingFee = "TaxiStartingFee",
        IndustrialEfficiency = "IndustrialEfficiency",
        OfficeEfficiency = "OfficeEfficiency",
        PollutionHealthAffect = "PollutionHealthAffect",
        HospitalEfficiency = "HospitalEfficiency"
    }
    export interface LocalModifierEffect {
        modifiers: LocalModifier[];
    }
    export interface LocalModifier {
        type: LocalModifierType;
        delta: number;
        unit: Unit;
        radius: number;
    }
    export enum LocalModifierType {
        CrimeAccumulation = "CrimeAccumulation",
        ForestFireResponseTime = "ForestFireResponseTime",
        ForestFireHazard = "ForestFireHazard",
        Wellbeing = "Wellbeing",
        Health = "Health"
    }
    export interface LeisureProviderEffect {
        type: string;
        efficiency: number;
    }
    export interface AdjustHappinessEffect {
        targets: string[];
        wellbeingEffect: number;
        healthEffect: number;
    }
}
declare module "game/data-binding/prefab/prefab-properties" {
    import { TypeFromMap } from "common/data-binding/binding-utils";
    import { Properties } from "common/property-field/typed-property";
    export const CONSUMPTION_PROPERTY = "prefabs.ConsumptionProperty";
    export interface ConsumptionProperty {
        electricityConsumption: number;
        waterConsumption: number;
        garbageAccumulation: number;
    }
    export const POLLUTION_PROPERTY = "prefabs.PollutionProperty";
    export interface PollutionProperty {
        groundPollution: Pollution;
        airPollution: Pollution;
        noisePollution: Pollution;
    }
    export enum Pollution {
        none = 0,
        low = 1,
        medium = 2,
        high = 3
    }
    export const ELECTRICITY_PROPERTY = "prefabs.ElectricityProperty";
    export interface ElectricityProperty {
        labelId: string;
        minCapacity: number;
        maxCapacity: number;
        voltage: Voltage;
    }
    export enum Voltage {
        low = 0,
        high = 1,
        both = 2
    }
    export const TRANSPORT_STOP_PROPERTY = "prefabs.TransportStopProperty";
    export interface TransportStopProperty {
        stops: {
            [key: string]: number;
        };
    }
    export interface PrefabProperties extends Properties {
        [CONSUMPTION_PROPERTY]: ConsumptionProperty;
        [POLLUTION_PROPERTY]: PollutionProperty;
        [ELECTRICITY_PROPERTY]: ElectricityProperty;
        [TRANSPORT_STOP_PROPERTY]: TransportStopProperty;
    }
    export type PrefabProperty = TypeFromMap<PrefabProperties>;
}
declare module "game/data-binding/prefab/prefab-requirements" {
    import { TypeFromMap } from "common/data-binding/binding-utils";
    import { Entity } from "common/data-binding/common-types";
    interface PrefabRequirementBase {
        entity: Entity;
        locked: boolean;
    }
    export interface MilestoneRequirement extends PrefabRequirementBase {
        index: number;
    }
    export interface DevTreeNodeRequirement extends PrefabRequirementBase {
        name: string;
    }
    export interface StrictObjectBuiltRequirement extends PrefabRequirementBase {
        labelId: string | null;
        progress: number;
        icon: string;
        requirement: string;
        minimumCount: number;
    }
    export interface ZoneBuiltRequirement extends PrefabRequirementBase {
        labelId: string | null;
        progress: number;
        icon: string;
        requiredTheme: string | null;
        requiredZone: string | null;
        requiredType: AreaType;
        minimumSquares: number;
        minimumCount: number;
        minimumLevel: number;
    }
    export enum AreaType {
        none = 0,
        residential = 1,
        commercial = 2,
        industrial = 3
    }
    export interface CitizenRequirement extends PrefabRequirementBase {
        labelId: string | null;
        progress: number;
        minimumPopulation: number;
        minimumHappiness: number;
    }
    export interface ProcessingRequirement extends PrefabRequirementBase {
        labelId: string | null;
        progress: number;
        icon: string;
        resourceType: string;
        minimumProducedAmount: number;
    }
    export interface ObjectBuiltRequirement extends UnlockRequirement {
        name: string;
        minimumCount: number;
    }
    export interface UnlockRequirement extends PrefabRequirementBase {
        labelId: string | null;
        progress: number;
    }
    export interface TutorialRequirement extends PrefabRequirementBase {
    }
    export enum PrefabRequirementType {
        Milestone = "prefabs.MilestoneRequirement",
        DevTreeNode = "prefabs.DevTreeNodeRequirement",
        StrictObjectBuilt = "prefabs.StrictObjectBuiltRequirement",
        ZoneBuilt = "prefabs.ZoneBuiltRequirement",
        Citizen = "prefabs.CitizenRequirement",
        Processing = "prefabs.ProcessingRequirement",
        ObjectBuilt = "prefabs.ObjectBuiltRequirement",
        Unlock = "prefabs.UnlockRequirement",
        Tutorial = "prefabs.TutorialRequirement"
    }
    export interface PrefabRequirements {
        [PrefabRequirementType.Milestone]: MilestoneRequirement;
        [PrefabRequirementType.DevTreeNode]: DevTreeNodeRequirement;
        [PrefabRequirementType.StrictObjectBuilt]: StrictObjectBuiltRequirement;
        [PrefabRequirementType.ZoneBuilt]: ZoneBuiltRequirement;
        [PrefabRequirementType.Citizen]: CitizenRequirement;
        [PrefabRequirementType.Processing]: ProcessingRequirement;
        [PrefabRequirementType.ObjectBuilt]: ObjectBuiltRequirement;
        [PrefabRequirementType.Unlock]: UnlockRequirement;
        [PrefabRequirementType.Tutorial]: TutorialRequirement;
    }
    export type PrefabRequirement = TypeFromMap<PrefabRequirements>;
}
declare module "game/data-binding/prefab/prefab-bindings" {
    import { Entity } from "common/data-binding/common-types";
    import { NumericProperty } from "common/property-field/typed-property";
    import { PrefabEffect } from "game/data-binding/prefab/prefab-effects";
    import { PrefabProperty } from "game/data-binding/prefab/prefab-properties";
    import { PrefabRequirement } from "game/data-binding/prefab/prefab-requirements";
    export interface Theme {
        entity: Entity;
        name: string;
        icon: string;
    }
    export interface UnlockingRequirements {
        requireAny: PrefabRequirement[];
        requireAll: PrefabRequirement[];
    }
    export interface PrefabDetails {
        entity: Entity;
        name: string;
        uiTag: string;
        icon: string;
        dlc: string | null;
        preview: string | null;
        titleId: string;
        descriptionId: string | null;
        locked: boolean;
        uniquePlaced: boolean;
        constructionCost: NumericProperty | null;
        effects: PrefabEffect[];
        properties: PrefabProperty[];
        requirements: UnlockingRequirements;
    }
    export const themes$: import("common/data-binding/binding").ValueBinding<Theme[]>;
    export const prefabDetails$: import("common/data-binding/binding").MapBinding<Entity, PrefabDetails | null>;
    export const manualUITags$: import("common/data-binding/binding").ValueBinding<ManualUITagsConfiguration | null>;
    export const emptyPrefabDetails: PrefabDetails;
    export interface ManualUITagsConfiguration {
        chirperPanel: string;
        chirperPanelButton: string;
        chirperPanelChirps: string;
        cityInfoPanel: string;
        cityInfoPanelButton: string;
        cityInfoPanelDemandPage: string;
        cityInfoPanelDemandTab: string;
        cityInfoPanelPoliciesPage: string;
        cityInfoPanelPoliciesTab: string;
        economyPanelBudgetBalance: string;
        economyPanelBudgetExpenses: string;
        economyPanelBudgetPage: string;
        economyPanelBudgetRevenue: string;
        economyPanelBudgetTab: string;
        economyPanelButton: string;
        economyPanelLoansAccept: string;
        economyPanelLoansPage: string;
        economyPanelLoansSlider: string;
        economyPanelLoansTab: string;
        economyPanelProductionPage: string;
        economyPanelProductionResources: string;
        economyPanelProductionTab: string;
        economyPanelServicesBudget: string;
        economyPanelServicesList: string;
        economyPanelServicesPage: string;
        economyPanelServicesTab: string;
        economyPanelTaxationEstimate: string;
        economyPanelTaxationPage: string;
        economyPanelTaxationRate: string;
        economyPanelTaxationTab: string;
        economyPanelTaxationType: string;
        eventJournalPanel: string;
        eventJournalPanelButton: string;
        infoviewsButton: string;
        infoviewsMenu: string;
        infoviewsPanel: string;
        infoviewsFireHazard: string;
        lifePathPanel: string;
        lifePathPanelBackButton: string;
        lifePathPanelButton: string;
        lifePathPanelChirps: string;
        lifePathPanelDetail: string;
        mapTilePanel: string;
        mapTilePanelButton: string;
        mapTilePanelResources: string;
        mapTilePanelPurchase: string;
        photoModePanel: string;
        photoModePanelButton: string;
        photoModePanelHideUI: string;
        photoModePanelTakePicture: string;
        photoModeTab: string;
        photoModePanelTitle: string;
        photoModeCinematicCameraToggle: string;
        cinematicCameraPanel: string;
        cinematicCameraPanelCaptureKey: string;
        cinematicCameraPanelPlay: string;
        cinematicCameraPanelStop: string;
        cinematicCameraPanelHideUI: string;
        cinematicCameraPanelSaveLoad: string;
        cinematicCameraPanelReset: string;
        cinematicCameraPanelTimelineSlider: string;
        cinematicCameraPanelTransformCurves: string;
        cinematicCameraPanelPropertyCurves: string;
        cinematicCameraPanelPlaybackDurationSlider: string;
        progressionPanel: string;
        progressionPanelButton: string;
        progressionPanelDevelopmentNode: string;
        progressionPanelDevelopmentPage: string;
        progressionPanelDevelopmentService: string;
        progressionPanelDevelopmentTab: string;
        progressionPanelDevelopmentUnlockableNode: string;
        progressionPanelDevelopmentUnlockNode: string;
        progressionPanelMilestoneRewards: string;
        progressionPanelMilestoneRewardsMoney: string;
        progressionPanelMilestoneRewardsDevPoints: string;
        progressionPanelMilestoneRewardsMapTiles: string;
        progressionPanelMilestonesList: string;
        progressionPanelMilestonesPage: string;
        progressionPanelMilestonesTab: string;
        progressionPanelMilestoneXP: string;
        radioPanel: string;
        radioPanelAdsToggle: string;
        radioPanelButton: string;
        radioPanelNetworks: string;
        radioPanelStations: string;
        radioPanelVolumeSlider: string;
        statisticsPanel: string;
        statisticsPanelButton: string;
        statisticsPanelMenu: string;
        statisticsPanelTimeScale: string;
        toolbarBulldozerBar: string;
        toolbarDemand: string;
        toolbarSimulationDateTime: string;
        toolbarSimulationSpeed: string;
        toolbarSimulationToggle: string;
        toolbarUnderground: string;
        toolOptions: string;
        toolOptionsBrushSize: string;
        toolOptionsBrushStrength: string;
        toolOptionsElevation: string;
        toolOptionsElevationDecrease: string;
        toolOptionsElevationIncrease: string;
        toolOptionsElevationStep: string;
        toolOptionsModes: string;
        toolOptionsModesComplexCurve: string;
        toolOptionsModesContinuous: string;
        toolOptionsModesGrid: string;
        toolOptionsModesReplace: string;
        toolOptionsModesSimpleCurve: string;
        toolOptionsModesStraight: string;
        toolOptionsParallelMode: string;
        toolOptionsParallelModeOffset: string;
        toolOptionsParallelModeOffsetDecrease: string;
        toolOptionsParallelModeOffsetIncrease: string;
        toolOptionsSnapping: string;
        toolOptionsThemes: string;
        toolOptionsUnderground: string;
        transportationOverviewPanel: string;
        transportationOverviewPanelButton: string;
        transportationOverviewPanelLegend: string;
        transportationOverviewPanelLines: string;
        transportationOverviewPanelTabCargo: string;
        transportationOverviewPanelTabPublicTransport: string;
        transportationOverviewPanelTransportTypes: string;
        selectedInfoPanel: string;
        selectedInfoPanelTitle: string;
        selectedInfoPanelPolicies: string;
        selectedInfoPanelDelete: string;
        pauseMenuButton: string;
        upgradeGrid: string;
        actionHints: string;
    }
}
declare module "game/data-binding/infoview-types" {
    import { Color, Gradient } from "common/color";
    import { Entity } from "common/data-binding/common-types";
    import { LocElement } from "common/localization/localization-bindings";
    import { UnlockingRequirements } from "game/data-binding/prefab/prefab-bindings";
    export interface Infoview {
        entity: Entity;
        id: string;
        icon: string | null;
        locked: boolean;
        uiTag: string;
        group: number;
        editor: boolean;
        requirements: UnlockingRequirements;
    }
    export interface ActiveInfoview {
        entity: Entity;
        id: string;
        icon: string;
        uiTag: string;
        infomodes: Infomode[];
        editor: boolean;
    }
    export interface Infomode {
        entity: Entity;
        id: string;
        uiTag: string;
        active: boolean;
        priority: number;
        color: Color | null;
        gradientLegend: InfomodeGradientLegend | null;
        colorLegends: InfomodeColorLegend[];
        type: string;
    }
    export interface InfomodeGradientLegend {
        lowLabel: LocElement | null;
        highLabel: LocElement | null;
        gradient: Gradient;
    }
    export interface InfomodeColorLegend {
        color: Color;
        label: LocElement;
    }
}
declare module "game/data-binding/infoview-bindings" {
    import { Entity } from "common/data-binding/common-types";
    import { ActiveInfoview, Infoview } from "game/data-binding/infoview-types";
    import { UnlockingRequirements } from "game/data-binding/prefab/prefab-bindings";
    export const infoviews$: import("common/data-binding/binding").ValueBinding<Infoview[]>;
    export const activeInfoview$: import("common/data-binding/binding").ValueBinding<ActiveInfoview | null>;
    export function setActiveInfoview(entity: Entity): void;
    export function clearActiveInfoview(): void;
    export function setInfomodeActive(entity: Entity, active: boolean, priority: number): void;
    export const electricityConsumption$: import("common/data-binding/binding").ValueBinding<number>;
    export const electricityProduction$: import("common/data-binding/binding").ValueBinding<number>;
    export const electricityTransmitted$: import("common/data-binding/binding").ValueBinding<number>;
    export const electricityExport$: import("common/data-binding/binding").ValueBinding<number>;
    export const electricityImport$: import("common/data-binding/binding").ValueBinding<number>;
    export const electricityAvailability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const electricityTransmission$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const electricityTrade$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const batteryCharge$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const waterCapacity$: import("common/data-binding/binding").ValueBinding<number>;
    export const waterConsumption$: import("common/data-binding/binding").ValueBinding<number>;
    export const sewageCapacity$: import("common/data-binding/binding").ValueBinding<number>;
    export const sewageConsumption$: import("common/data-binding/binding").ValueBinding<number>;
    export const waterExport$: import("common/data-binding/binding").ValueBinding<number>;
    export const waterImport$: import("common/data-binding/binding").ValueBinding<number>;
    export const sewageExport$: import("common/data-binding/binding").ValueBinding<number>;
    export const sewageAvailability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const waterAvailability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const waterTrade$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const elementaryEligible$: import("common/data-binding/binding").ValueBinding<number>;
    export const highSchoolEligible$: import("common/data-binding/binding").ValueBinding<number>;
    export const collegeEligible$: import("common/data-binding/binding").ValueBinding<number>;
    export const universityEligible$: import("common/data-binding/binding").ValueBinding<number>;
    export const elementaryCapacity$: import("common/data-binding/binding").ValueBinding<number>;
    export const highSchoolCapacity$: import("common/data-binding/binding").ValueBinding<number>;
    export const collegeCapacity$: import("common/data-binding/binding").ValueBinding<number>;
    export const universityCapacity$: import("common/data-binding/binding").ValueBinding<number>;
    export const educationData$: import("common/data-binding/binding").ValueBinding<ChartData>;
    export const elementaryStudents$: import("common/data-binding/binding").ValueBinding<number>;
    export const highSchoolStudents$: import("common/data-binding/binding").ValueBinding<number>;
    export const collegeStudents$: import("common/data-binding/binding").ValueBinding<number>;
    export const universityStudents$: import("common/data-binding/binding").ValueBinding<number>;
    export const elementaryAvailability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const highSchoolAvailability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const collegeAvailability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const universityAvailability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const transportSummaries$: import("common/data-binding/binding").ValueBinding<TransportSummaries>;
    export const averageHealth$: import("common/data-binding/binding").ValueBinding<number>;
    export const cemeteryUse$: import("common/data-binding/binding").ValueBinding<number>;
    export const cemeteryCapacity$: import("common/data-binding/binding").ValueBinding<number>;
    export const deathRate$: import("common/data-binding/binding").ValueBinding<number>;
    export const processingRate$: import("common/data-binding/binding").ValueBinding<number>;
    export const sickCount$: import("common/data-binding/binding").ValueBinding<number>;
    export const patientCount$: import("common/data-binding/binding").ValueBinding<number>;
    export const patientCapacity$: import("common/data-binding/binding").ValueBinding<number>;
    export const healthcareAvailability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const deathcareAvailability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const cemeteryAvailability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const garbageProcessingRate$: import("common/data-binding/binding").ValueBinding<number>;
    export const landfillCapacity$: import("common/data-binding/binding").ValueBinding<number>;
    export const processingAvailability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const landfillAvailability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const garbageProductionRate$: import("common/data-binding/binding").ValueBinding<number>;
    export const storedGarbage$: import("common/data-binding/binding").ValueBinding<number>;
    export const parkingCapacity$: import("common/data-binding/binding").ValueBinding<number>;
    export const parkingIncome$: import("common/data-binding/binding").ValueBinding<number>;
    export const parkedCars$: import("common/data-binding/binding").ValueBinding<number>;
    export const parkingAvailability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const trafficFlow$: import("common/data-binding/binding").ValueBinding<number[]>;
    export const averageGroundPollution$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const averageAirPollution$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const averageWaterPollution$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const averageNoisePollution$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const averageFireHazard$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const averageCrimeProbability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const jailAvailability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const prisonAvailability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const crimeProducers$: import("common/data-binding/binding").ValueBinding<number>;
    export const crimeProbability$: import("common/data-binding/binding").ValueBinding<number>;
    export const jailCapacity$: import("common/data-binding/binding").ValueBinding<number>;
    export const arrestedCriminals$: import("common/data-binding/binding").ValueBinding<number>;
    export const inJail$: import("common/data-binding/binding").ValueBinding<number>;
    export const prisonCapacity$: import("common/data-binding/binding").ValueBinding<number>;
    export const prisoners$: import("common/data-binding/binding").ValueBinding<number>;
    export const inPrison$: import("common/data-binding/binding").ValueBinding<number>;
    export const criminals$: import("common/data-binding/binding").ValueBinding<number>;
    export const crimePerMonth$: import("common/data-binding/binding").ValueBinding<number>;
    export const escapedRate$: import("common/data-binding/binding").ValueBinding<number>;
    export const averageLandValue$: import("common/data-binding/binding").ValueBinding<number>;
    export const residentialLevels$: import("common/data-binding/binding").ValueBinding<ChartData>;
    export const commercialLevels$: import("common/data-binding/binding").ValueBinding<ChartData>;
    export const industrialLevels$: import("common/data-binding/binding").ValueBinding<ChartData>;
    export const officeLevels$: import("common/data-binding/binding").ValueBinding<ChartData>;
    export const shelteredCount$: import("common/data-binding/binding").ValueBinding<number>;
    export const shelterCapacity$: import("common/data-binding/binding").ValueBinding<number>;
    export const shelterAvailability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const attractiveness$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const averageHotelPrice$: import("common/data-binding/binding").ValueBinding<number>;
    export const tourismRate$: import("common/data-binding/binding").ValueBinding<number>;
    export const weatherEffect$: import("common/data-binding/binding").ValueBinding<number>;
    export const mailProductionRate$: import("common/data-binding/binding").ValueBinding<number>;
    export const collectedMail$: import("common/data-binding/binding").ValueBinding<number>;
    export const deliveredMail$: import("common/data-binding/binding").ValueBinding<number>;
    export const postServiceAvailability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const population$: import("common/data-binding/binding").ValueBinding<number>;
    export const employed$: import("common/data-binding/binding").ValueBinding<number>;
    export const jobs$: import("common/data-binding/binding").ValueBinding<number>;
    export const unemployment$: import("common/data-binding/binding").ValueBinding<number>;
    export const birthRate$: import("common/data-binding/binding").ValueBinding<number>;
    export const movedIn$: import("common/data-binding/binding").ValueBinding<number>;
    export const movedAway$: import("common/data-binding/binding").ValueBinding<number>;
    export const ageData$: import("common/data-binding/binding").ValueBinding<ChartData>;
    export const commercialProfitability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const industrialProfitability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const officeProfitability$: import("common/data-binding/binding").ValueBinding<IndicatorValue>;
    export const topImportNames$: import("common/data-binding/binding").ValueBinding<string[]>;
    export const topImportColors$: import("common/data-binding/binding").ValueBinding<string[]>;
    export const topImportData$: import("common/data-binding/binding").ValueBinding<ChartData>;
    export const topExportNames$: import("common/data-binding/binding").ValueBinding<string[]>;
    export const topExportColors$: import("common/data-binding/binding").ValueBinding<string[]>;
    export const topExportData$: import("common/data-binding/binding").ValueBinding<ChartData>;
    export const availableOil$: import("common/data-binding/binding").ValueBinding<number>;
    export const availableOre$: import("common/data-binding/binding").ValueBinding<number>;
    export const availableForest$: import("common/data-binding/binding").ValueBinding<number>;
    export const availableFertility$: import("common/data-binding/binding").ValueBinding<number>;
    export const oilExtractionRate$: import("common/data-binding/binding").ValueBinding<number>;
    export const oreExtractionRate$: import("common/data-binding/binding").ValueBinding<number>;
    export const forestExtractionRate$: import("common/data-binding/binding").ValueBinding<number>;
    export const fertilityExtractionRate$: import("common/data-binding/binding").ValueBinding<number>;
    export const forestRenewalRate$: import("common/data-binding/binding").ValueBinding<number>;
    export const fertilityRenewalRate$: import("common/data-binding/binding").ValueBinding<number>;
    export const workplacesData$: import("common/data-binding/binding").ValueBinding<ChartData>;
    export const employeesData$: import("common/data-binding/binding").ValueBinding<ChartData>;
    export const worksplaces$: import("common/data-binding/binding").ValueBinding<number>;
    export const workers$: import("common/data-binding/binding").ValueBinding<number>;
    export interface IndicatorValue {
        min: number;
        max: number;
        current: number;
    }
    export interface ChartData {
        values: number[];
        total: number;
    }
    export interface TransportSummaries {
        passengerSummaries: PassengerSummary[];
        cargoSummaries: CargoSummary[];
    }
    export interface PassengerSummary {
        id: string;
        icon: string;
        locked: boolean;
        lineCount: number;
        touristCount: number;
        citizenCount: number;
        requirements: UnlockingRequirements;
    }
    export interface CargoSummary {
        id: string;
        icon: string;
        locked: boolean;
        lineCount: number;
        cargoCount: number;
        requirements: UnlockingRequirements;
    }
    export function useInfoviewToggle(infoviewId: string): (() => void) | undefined;
}
declare module "common/focus/focus-key" {
    /**
     * Special focus key that disables the focus of the component.
     */
    export const FOCUS_DISABLED: unique symbol;
    /**
     * Special focus key that assigns an internally generated, unique focus key to the component.
     *
     * This is useful if the component is inside of a `NavigationScope` and there is no need to manually control focus,
     * or the focus key is defined by a higher level `FocusKeyOverride` component.
     */
    export const FOCUS_AUTO: unique symbol;
    export type FocusKey = typeof FOCUS_DISABLED | typeof FOCUS_AUTO | UniqueFocusKey;
    export type UniqueFocusKey = FocusSymbol | string | number;
    export class FocusSymbol {
        readonly debugName: string;
        readonly r: number;
        constructor(debugName: string);
        toString(): string;
    }
    export function useUniqueFocusKey(focusKey: FocusKey, debugName: string): UniqueFocusKey | null;
}
declare module "editor/data-binding/editor-types" {
    import { Number2 } from "common/math";
    export interface AssetType {
        id: string;
        imageUrl: string | undefined;
    }
    export enum WeightedMode {
        None = 0,
        In = 1,
        Out = 2,
        Both = 3
    }
    export enum WrapMode {
        Default = 0,
        Clamp = 1,
        Once = 1,
        Loop = 2,
        PingPong = 4,
        ClampForever = 8
    }
    export interface IKeyframe {
        time: number;
        value: number;
        inTangent: number;
        outTangent: number;
        inWeight: number;
        outWeight: number;
        weightedMode: WeightedMode;
        readonly?: boolean;
    }
    export interface AnimationCurve {
        keys: IKeyframe[];
        preWrapMode: WrapMode;
        postWrapMode: WrapMode;
        label?: string;
        color?: string;
        hidePath?: boolean;
        deviationFrom?: number;
        readonly?: boolean;
    }
    export const defaultKeyframe: IKeyframe;
    export interface Season {
        name: string;
        startTime: number;
        tempNightDay: Number2;
        tempDeviationNightDay: Number2;
        cloudChance: number;
        cloudAmount: number;
        cloudAmountDeviation: number;
        precipitationChance: number;
        precipitationAmount: number;
        precipitationAmountDeviation: number;
        turbulence: number;
        auroraAmount: number;
        auroraChance: number;
    }
}
declare module "editor/widgets/fields/animation-curve-field/types" {
    import { Bounds2, Number2 } from "common/math";
    import { AnimationCurve, IKeyframe } from "editor/data-binding/editor-types";
    export interface AnimationCurveAPI {
        data: AnimationCurveData;
        onAddKeyframe?: (time: number, value: number, curveIndex?: number) => Promise<number>;
        onMoveKeyframe?: (index: number, keyframe: IKeyframe, smooth?: boolean, curveIndex?: number) => Promise<number>;
        onRemoveKeyframe?: (index: number, curveIndex?: number) => void;
        onSetKeyframes?: (keyframes: IKeyframe[], curveIndex?: number) => void;
    }
    export interface AnimationCurveData {
        bounds: Bounds2;
        curves: AnimationCurve[];
    }
    export interface BezierValue {
        index: number;
        position: Number2;
        controlPoint: Number2;
        tangent: number;
        isLine: boolean;
    }
    export interface BezierSegment {
        start: BezierValue;
        end: BezierValue;
    }
    export enum NodeMode {
        FreeSmooth = 0,
        Flat = 1,
        Broken = 2
    }
    export enum TangentMode {
        Free = 0,
        Linear = 1,
        Constant = 2,
        Weighted = 3
    }
    export interface INode {
        index: number;
        nodeMode: NodeMode;
        position: Number2;
        inPosition?: Number2;
        inTangentMode?: TangentMode;
        outPosition?: Number2;
        outTangentMode?: TangentMode;
    }
    export const defaultNode: INode;
    export interface Point {
        keyframe: IKeyframe;
        node: INode;
    }
    export enum HandleType {
        Position = 0,
        InHandle = 1,
        OutHandle = 2
    }
    export enum Tangent {
        In = 0,
        Out = 1
    }
    export interface Handle {
        index: number;
        type: HandleType;
    }
    export type KeyframeParser = (curr: IKeyframe, next: IKeyframe, index: number, keyframes: IKeyframe[], curveIndex: number) => IKeyframe;
    export type LabelFormatter = (value: number) => string | number | JSX.Element;
}
declare module "editor/widgets/fields/seasons-field/types" {
    import { AnimationCurve, IKeyframe, Season } from "editor/data-binding/editor-types";
    import { AnimationCurveAPI } from "editor/widgets/fields/animation-curve-field/types";
    export type SeasonsField = {
        seasons: Season[];
        curves: {
            temperature: AnimationCurve;
            precipitation: AnimationCurve;
            cloudiness: AnimationCurve;
            aurora: AnimationCurve;
            fog: AnimationCurve;
        };
    };
    export type SeasonsCurves = {
        temperature: [
            final: AnimationCurve,
            day: AnimationCurve,
            dayDeviation: AnimationCurve,
            night: AnimationCurve,
            nightDeviation: AnimationCurve
        ];
        cloudiness: [
            final: AnimationCurve,
            chance: AnimationCurve,
            amount: AnimationCurve,
            amountDeviation: AnimationCurve,
            turbulence: AnimationCurve
        ];
        precipitation: [
            final: AnimationCurve,
            chance: AnimationCurve,
            amount: AnimationCurve,
            amountDeviation: AnimationCurve,
            turbulence: AnimationCurve
        ];
        aurora: [
            final: AnimationCurve,
            chance: AnimationCurve,
            amount: AnimationCurve
        ];
        fog: [
            final: AnimationCurve
        ];
    };
    export type OnUpdateSeasonProperty = (seasonIndex: number, type: keyof SeasonsCurves, curveIndex: number, keyframe: IKeyframe) => Promise<number>;
    export interface SeasonsFieldBindings extends Omit<AnimationCurveAPI, 'data' | 'onMoveKeyframe' | 'onSetKeyframes'> {
        onUpdateSeasonProperty: OnUpdateSeasonProperty;
    }
    export interface SeasonsFieldProps extends SeasonsFieldBindings {
        value: SeasonsCurves;
        label?: JSX.Element;
        seasons: Season[];
    }
}
declare module "widgets/data-binding/widget-bindings" {
    import { Color } from "common/color";
    import { LocElement } from "common/localization/localization-bindings";
    import { Bezier4x3, Bounds1, Bounds2, Bounds3, LongNumber, Number2, Number3, Number4 } from "common/math";
    import { AnimationCurve, IKeyframe, Season } from "editor/data-binding/editor-types";
    import { SeasonsField } from "editor/widgets/fields/seasons-field/types";
    export enum WidgetType {
        Column = "Game.UI.Widgets.Column",
        Row = "Game.UI.Widgets.Row",
        Scrollable = "Game.UI.Widgets.Scrollable",
        PageView = "Game.UI.Widgets.PageView",
        PageLayout = "Game.UI.Widgets.PageLayout",
        Divider = "Game.UI.Widgets.Divider",
        Label = "Game.UI.Widgets.Label",
        Breadcrumbs = "Game.UI.Widgets.Breadcrumbs",
        Button = "Game.UI.Widgets.Button",
        ButtonRow = "Game.UI.Widgets.ButtonRow",
        IconButton = "Game.UI.Widgets.IconButton",
        IconButtonGroup = "Game.UI.Widgets.IconButtonGroup",
        Group = "Game.UI.Widgets.Group",
        ExpandableGroup = "Game.UI.Widgets.ExpandableGroup",
        PagedList = "Game.UI.Widgets.PagedList",
        ValueField = "Game.UI.Widgets.ValueField",
        LocalizedValueField = "Game.UI.Widgets.LocalizedValueField",
        ToggleField = "Game.UI.Widgets.ToggleField",
        IntInputField = "Game.UI.Widgets.IntInputField",
        IntSliderField = "Game.UI.Widgets.IntSliderField",
        Int2InputField = "Game.UI.Widgets.Int2InputField",
        Int3InputField = "Game.UI.Widgets.Int3InputField",
        Int4InputField = "Game.UI.Widgets.Int4InputField",
        UIntInputField = "Game.UI.Widgets.UIntInputField",
        UIntSliderField = "Game.UI.Widgets.UIntSliderField",
        TimeSliderField = "Game.UI.Widgets.TimeSliderField",
        TimeBoundsSliderField = "Game.UI.Widgets.TimeBoundsSliderField",
        FloatInputField = "Game.UI.Widgets.FloatInputField",
        FloatSliderField = "Game.UI.Widgets.FloatSliderField",
        Float2InputField = "Game.UI.Widgets.Float2InputField",
        Float2SliderField = "Game.UI.Widgets.Float2SliderField",
        Float3InputField = "Game.UI.Widgets.Float3InputField",
        Float3SliderField = "Game.UI.Widgets.Float3SliderField",
        EulerAnglesField = "Game.UI.Widgets.EulerAnglesField",
        Float4InputField = "Game.UI.Widgets.Float4InputField",
        Float4SliderField = "Game.UI.Widgets.Float4SliderField",
        Bounds1SliderField = "Game.UI.Widgets.Bounds1SliderField",
        Bounds1InputField = "Game.UI.Widgets.Bounds1InputField",
        Bounds2InputField = "Game.UI.Widgets.Bounds2InputField",
        Bounds3InputField = "Game.UI.Widgets.Bounds3InputField",
        Bezier4x3Field = "Game.UI.Widgets.Bezier4x3Field",
        StringInputField = "Game.UI.Widgets.StringInputField",
        ColorField = "Game.UI.Widgets.ColorField",
        AnimationCurveField = "Game.UI.Widgets.AnimationCurveField",
        EnumField = "Game.UI.Widgets.EnumField",
        FlagsField = "Game.UI.Widgets.FlagsField",
        PopupValueField = "Game.UI.Widgets.PopupValueField",
        DropdownField = "Game.UI.Widgets.DropdownField",
        DirectoryPickerButton = "Game.UI.Widgets.DirectoryPickerButton",
        SeasonsField = "Game.UI.Widgets.SeasonsField",
        ImageField = "Game.UI.Widgets.ImageField"
    }
    export type PathSegment = string | number;
    export type Path = PathSegment[];
    export function combinePath(path: Path, segment: PathSegment): Path;
    export function usePathFocusKey(path: Path): string;
    export interface WidgetIdentifier {
        group: string;
        path: Path;
    }
    export interface Named {
        displayName: LocElement;
    }
    export interface TooltipTarget {
        tooltip?: LocElement | null;
    }
    export interface WidgetTutorialTarget {
        tutorialTag: string | null;
    }
    export interface Expandable {
        expanded: boolean;
    }
    export interface Flexible {
        flex: FlexLayout;
    }
    export interface FlexLayout {
        grow: number;
        shrink: number;
        basis: number;
    }
    export interface Divider {
    }
    export interface Label extends Named, TooltipTarget {
        level: number;
        pageId?: string;
        sectionId?: string;
        beta?: boolean;
    }
    export interface Breadcrumbs {
    }
    export interface Button extends Named, TooltipTarget {
        disabled: boolean;
    }
    export interface ButtonRow {
    }
    export interface IconButton extends TooltipTarget, WidgetTutorialTarget {
        icon: string;
        selected: boolean;
        disabled: boolean;
    }
    export interface IconButtonGroup {
    }
    export type Column = Flexible;
    export type Row = Flexible;
    export type Scrollable = Flexible;
    export interface PageView extends Flexible {
        currentPage: number;
    }
    export interface PageLayout extends Flexible {
        title: LocElement;
        hasBackAction: boolean;
    }
    export enum TooltipPos {
        Title = 0,
        Cotnainer = 1
    }
    export interface Group extends Named, TooltipTarget {
        tooltipPos?: TooltipPos;
    }
    export interface ExpandableGroup extends Group, Expandable {
    }
    export interface PagedList extends Named, TooltipTarget, Expandable {
        resizable: boolean;
        sortable: boolean;
        length: number;
        currentPageIndex: number;
        pageCount: number;
        childStartIndex: number;
        childEndIndex: number;
        disabled: boolean;
    }
    export interface Field<T> extends Named, TooltipTarget, WidgetTutorialTarget {
        value: T;
        disabled?: boolean;
        hidden?: boolean;
    }
    export type ValueField = Field<string>;
    export type LocalizedValueField = Field<LocElement>;
    export type ToggleField = Field<boolean>;
    export interface IntInputField extends Field<number> {
        min?: number;
        max?: number;
        step?: number;
        stepMultiplier?: number;
    }
    export interface IntSliderField extends Field<number> {
        min: number;
        max: number;
        step?: number;
        stepMultiplier?: number;
        unit?: string | null;
        scaleDragVolume?: boolean;
    }
    export type Int2InputField = Field<Number2>;
    export type Int3InputField = Field<Number3>;
    export type Int4InputField = Field<Number4>;
    export interface UIntInputField extends Field<number> {
        min?: number;
        max?: number;
        step?: number;
        stepMultiplier?: number;
    }
    export interface UIntSliderField extends Field<number> {
        min: number;
        max: number;
        step?: number;
        stepMultiplier?: number;
        unit?: string | null;
        scaleDragVolume?: boolean;
    }
    export interface TimeSliderField extends Field<number> {
        min: number;
        max: number;
    }
    export interface TimeBoundsSliderField extends Field<Bounds1> {
        min: number;
        max: number;
        allowMinGreaterMax?: boolean;
    }
    export interface FloatInputField extends Field<number> {
        min?: number;
        max?: number;
        fractionDigits?: number;
        step?: number;
        stepMultiplier?: number;
    }
    export interface FloatSliderFieldBase<T> extends Field<T> {
        min: number;
        max: number;
        fractionDigits?: number;
        step?: number;
        unit?: string | null;
        scaleDragVolume?: boolean;
    }
    export interface FloatSliderField extends FloatSliderFieldBase<number> {
    }
    export type Float2InputField = Field<Number2>;
    export type Float2SliderField = FloatSliderFieldBase<Number2>;
    export type Float3InputField = Field<Number3>;
    export type Float3SliderField = FloatSliderFieldBase<Number3>;
    export type EulerAnglesField = Field<Number3>;
    export type Float4InputField = Field<Number4>;
    export type Float4SliderField = FloatSliderFieldBase<Number4>;
    export interface Bounds1InputField extends Field<Bounds1> {
        min?: number;
        max?: number;
        fractionDigits?: number;
        step?: number;
        allowMinGreaterMax?: boolean;
    }
    export interface Bounds1SliderField extends Field<Bounds1> {
        min: number;
        max: number;
        fractionDigits?: number;
        step?: number;
        allowMinGreaterMax?: boolean;
    }
    export interface Bounds2InputField extends Field<Bounds2> {
        allowMinGreaterMax?: boolean;
    }
    export interface Bounds3InputField extends Field<Bounds3> {
        allowMinGreaterMax?: boolean;
    }
    export type Bezier4x3Field = Field<Bezier4x3>;
    export type StringInputField = Field<string>;
    export interface ColorField extends Field<Color> {
        hdr?: boolean;
        showAlpha: boolean;
    }
    export type AnimationCurveFieldWidget = Field<AnimationCurve | AnimationCurve[]>;
    export type SeasonsFieldWidget = SeasonsField;
    export interface EnumField extends Field<LongNumber> {
        enumMembers: EnumMember<LongNumber>[];
    }
    export type FlagsField = EnumField;
    export interface EnumMember<T> {
        value: T;
        displayName: LocElement;
        disabled?: boolean;
    }
    export function toBigIntMember({ displayName, value, disabled }: EnumMember<LongNumber>): EnumMember<bigint>;
    export interface DropdownField<T> extends Field<T> {
        items: DropdownItem<T>[];
    }
    export interface DropdownItem<T> {
        value: T;
        displayName: LocElement;
        disabled?: boolean;
    }
    export interface PopupValueField extends Named, TooltipTarget, Expandable {
        displayValue: LocElement;
        disabled: boolean;
    }
    export interface DirectoryPickerButton extends Named, TooltipTarget {
        label: LocElement;
        value: string;
        selectedDirectory: string;
        displayValue: string;
        disabled: boolean;
    }
    export interface ImageField extends TooltipTarget {
        uri: string;
        label: LocElement;
    }
    export function invoke({ group, path }: WidgetIdentifier): void;
    export function setValue<T = any>({ group, path }: WidgetIdentifier, value: T): void;
    export function setExpanded({ group, path }: WidgetIdentifier, expanded: boolean): void;
    export function openDirectory({ group, path }: WidgetIdentifier): void;
    export function addListElement({ group, path }: WidgetIdentifier): void;
    export function duplicateListElement({ group, path }: WidgetIdentifier, index: number): void;
    export function moveListElement({ group, path }: WidgetIdentifier, fromIndex: number, toIndex: number): void;
    export function deleteListElement({ group, path }: WidgetIdentifier, index: number): void;
    export function clearList({ group, path }: WidgetIdentifier): void;
    export function setCurrentPageIndex({ group, path }: WidgetIdentifier, pageIndex: number): void;
    export function moveKeyframe(group: string, path: Path, index: number, key: IKeyframe, smooth?: boolean, curveIndex?: number): Promise<number>;
    export function addKeyframe(group: string, path: Path, time: number, value: number, curveIndex?: number): Promise<number>;
    export function removeKeyframe(group: string, path: Path, index: number, curveIndex?: number): void;
    export function setKeyframes(group: string, path: Path, keyframes: IKeyframe[], curveIndex?: number): void;
    export function addSeason(group: string, path: Path, startTime: number): Promise<number>;
    export function updateSeason(group: string, path: Path, value: Season): Promise<number>;
    export function deleteSeason(group: string, path: Path, index: number): void;
}
declare module "widgets/widget" {
    import { TypeFromMap } from "common/data-binding/binding-utils";
    export interface Widget<P> {
        path: string | number;
        props: P;
        children: Widget<any>[];
    }
    export type WidgetFromMap<T extends Record<string, any>> = Widget<TypeFromMap<T>>;
}
declare module "menu/data-binding/options-bindings" {
    import { LocElement } from "common/localization/localization-bindings";
    import { Breadcrumbs, Button, ButtonRow, Divider, DropdownField, EnumField, Field, FloatSliderField, IntSliderField, Label, LocalizedValueField, Named, ToggleField, ValueField, WidgetIdentifier, WidgetType } from "widgets/data-binding/widget-bindings";
    import { WidgetFromMap } from "widgets/widget";
    export enum OptionsWidgetType {
        ButtonWithConfirmation = "Game.UI.Menu.ButtonWithConfirmation",
        InputBindingField = "Game.UI.Menu.InputBindingField",
        ScreenResolutionDropdownField = "Game.UI.Menu.ScreenResolutionDropdownField",
        DropdownField = "Game.UI.Widgets.DropdownField",
        ModdingToolchainButton = "Game.UI.Menu.ModdingToolchainButton",
        ModdingToolchainDependency = "Game.UI.Menu.ModdingToolchainDependency"
    }
    export interface OptionsWidgets {
        [WidgetType.Button]: Button;
        [WidgetType.ButtonRow]: ButtonRow;
        [OptionsWidgetType.ButtonWithConfirmation]: ButtonWithConfirmation;
        [WidgetType.FloatSliderField]: FloatSliderField;
        [WidgetType.IntSliderField]: IntSliderField;
        [WidgetType.Divider]: Divider;
        [WidgetType.Label]: Label;
        [WidgetType.Breadcrumbs]: Breadcrumbs;
        [WidgetType.ToggleField]: ToggleField;
        [WidgetType.EnumField]: EnumField;
        [OptionsWidgetType.InputBindingField]: InputBindingField;
        [WidgetType.ValueField]: ValueField;
        [WidgetType.LocalizedValueField]: LocalizedValueField;
        [OptionsWidgetType.ScreenResolutionDropdownField]: ScreenResolutionDropdownField;
        [OptionsWidgetType.DropdownField]: DropdownField<any>;
        [OptionsWidgetType.ModdingToolchainButton]: ModdingToolchainButton;
        [OptionsWidgetType.ModdingToolchainDependency]: ModdingToolchainDependencyField;
    }
    export type OptionsWidget = WidgetFromMap<OptionsWidgets>;
    export interface ScreenResolutionDropdownField extends DropdownField<ScreenResolution> {
    }
    export interface ButtonWithConfirmation extends Button {
        confirmationMessage?: LocElement | null;
    }
    export interface InputBindingField extends Field<string[]> {
        group: string;
        canBeEmpty: boolean;
        onSelect: () => void;
        onUnset?: () => void;
    }
    export interface ModdingToolchainButton extends Button {
    }
    interface ModdingToolchainDependencyFieldValue {
        state: ModdingToolchainDependencyState;
        progress: number;
        name: LocElement;
        details: LocElement;
        action?: ModdingToolchainDeploymentAction;
        version: LocElement;
    }
    export interface ModdingToolchainDependencyField extends Field<ModdingToolchainDependencyFieldValue> {
    }
    export const pages$: import("common/data-binding/binding").ValueBinding<Page[]>;
    export const optionsWidgets$: import("common/data-binding/binding").ValueBinding<OptionsWidget[]>;
    export const selectedPage$: import("common/data-binding/binding").ValueBinding<string>;
    export const selectedSection$: import("common/data-binding/binding").ValueBinding<string>;
    export const displayConfirmationVisible$: import("common/data-binding/binding").ValueBinding<boolean>;
    export const displayConfirmationTime$: import("common/data-binding/binding").ValueBinding<number>;
    export const interfaceStyle$: import("common/data-binding/binding").ValueBinding<string>;
    export const interfaceTransparency$: import("common/data-binding/binding").ValueBinding<number>;
    export const interfaceScaling$: import("common/data-binding/binding").ValueBinding<boolean>;
    export const textScale$: import("common/data-binding/binding").ValueBinding<number>;
    export const unlockHighlightsEnabled$: import("common/data-binding/binding").ValueBinding<boolean>;
    export const chirperPopupsEnabled$: import("common/data-binding/binding").ValueBinding<boolean>;
    export const inputHintsType$: import("common/data-binding/binding").ValueBinding<InputHintsType>;
    export const radioVolume$: import("common/data-binding/binding").ValueBinding<number>;
    export const moddingToolProgress$: import("common/data-binding/binding").ValueBinding<number>;
    export const moddingToolProgressType$: import("common/data-binding/binding").ValueBinding<ProgressType>;
    export const moddingToolState$: import("common/data-binding/binding").ValueBinding<ModdingToolchainStatus>;
    export const moddingToolButtonState$: import("common/data-binding/binding").ValueBinding<ModdingToolchainButtonState>;
    export const unitSettings$: import("common/data-binding/binding").ValueBinding<UnitSettings>;
    export function rebindInput({ group, path }: WidgetIdentifier): void;
    export function unsetInputBinding({ group, path }: WidgetIdentifier): void;
    export function selectPage(page: string, section: string, showAdvanced?: boolean): void;
    export function filteredWidgets(widgetIds: number[], query?: string): void;
    export function dependencyAction({ path }: WidgetIdentifier, action: ModdingToolchainDeploymentAction): void;
    export enum ProgressType {
        Off = 0,
        Bar = 1,
        Spin = 2
    }
    export enum InputHintsType {
        AutoDetect = 0,
        Xbox = 1,
        PS = 2
    }
    export enum ModdingToolchainStatus {
        Idle = 0,
        Downloading = 1,
        Installing = 2,
        WaitingForLicense = 3,
        WaitingForClose = 4,
        Updating = 5,
        Uninstalling = 6,
        NotEnoughSpaceError = 7,
        DownloadError = 8,
        InstallError = 9,
        GetLicenseError = 10,
        CloseError = 11,
        DeployError = 12,
        UnistallError = 13
    }
    export enum ModdingToolchainButtonState {
        Install = 0,
        Uninstall = 1,
        Update = 2,
        Repair = 3,
        Cancel = 4
    }
    export enum ModdingToolchainDeploymentAction {
        None = 0,
        Install = 2,
        Update = 4,
        Repair = 8,
        Uninstall = 16
    }
    export enum ModdingToolchainDependencyState {
        Unknown = -1,
        Installed = 0,
        NotInstalled = 1,
        Outdated = 2,
        Installing = 3,
        Downloading = 4,
        Removing = 5,
        Queued = 6
    }
    export interface RefreshRate {
        numerator: number;
        denominator: number;
    }
    export interface ScreenResolution {
        width: number;
        height: number;
        refreshRate: RefreshRate;
    }
    export const confirmDisplay: () => void;
    export const revertDisplay: () => void;
    export interface Page {
        id: string;
        sections: Section[];
        beta?: boolean;
    }
    export interface Section {
        id: string;
        items: Array<Named & {
            id: number;
            isAdvanced: boolean;
            searchHidden: boolean;
        }>;
    }
    export interface UnitSettings {
        timeFormat: TimeFormat;
        temperatureUnit: TemperatureUnit;
        unitSystem: UnitSystem;
    }
    export enum TimeFormat {
        TwentyFourHours = 0,
        TwelveHours = 1
    }
    export enum TemperatureUnit {
        Celsius = 0,
        Fahrenheit = 1,
        Kelvin = 2
    }
    export enum UnitSystem {
        Metric = 0,
        Freedom = 1
    }
    export const defaultUnitSettings: UnitSettings;
}
declare module "common/localization/localization" {
    import React from 'react';
    import { UnitSettings } from "menu/data-binding/options-bindings";
    export interface Localization {
        translate(id: string, fallback?: string | null): string | null;
        unitSettings: UnitSettings;
    }
    export const LocalizationContext: React.Context<Localization>;
    export function useCachedLocalization(): Localization;
}
declare module "common/localization/loc-component" {
    import { FunctionComponent, MemoExoticComponent, ReactNode } from 'react';
    import { Localization } from "common/localization/localization";
    export interface LocComponent<P = unknown> extends MemoExoticComponent<FunctionComponent<P>> {
        renderString: LocStringRenderer<P>;
        propsAreEqual: PropsAreEqual<P>;
    }
    export type LocStringRenderer<P> = (loc: Localization, props: P) => string;
    export type PropsAreEqual<P> = (prevProps: P, nextProps: P) => boolean;
    export type LocReactNode = JSX.Element | string;
    export function useTranslation(element: ReactNode): string;
    export function renderLocElement(loc: Localization, element: ReactNode, fragmentSeparator?: string): string;
    export function areLocElementsEqual(a: ReactNode, b: ReactNode): boolean;
    export function createLocComponent<P>(displayName: string, renderString: LocStringRenderer<P>, propsAreEqual: PropsAreEqual<P>): LocComponent<P>;
}
declare module "common/utils/substitute" {
    export function substitute(str: string, args: {
        [key: string]: string;
    }): string;
}
declare module "common/localization/localized-string" {
    import { Localization } from "common/localization/localization";
    import { LocReactNode } from "common/localization/loc-component";
    interface LocalizedStringProps {
        id: string | null;
        fallback?: string | null;
        args?: Record<string, LocReactNode> | null;
    }
    export const LocalizedString: import("common/localization/loc-component").LocComponent<LocalizedStringProps>;
    export function renderLocalizedString(loc: Localization, id: string | null | undefined, fallback: string | null | undefined, args: Record<string, LocReactNode> | null | undefined): string;
    export function areArgsEqual<A extends readonly string[]>(argNames: readonly A[number][], a: Record<A[number], LocReactNode>, b: Record<A[number], LocReactNode>): boolean;
}
declare module "common/localization/loc-dictionary" {
    import { LocComponent, LocReactNode, LocStringRenderer } from "common/localization/loc-component";
    export function createLocDictionary<T extends LocDictionary>(dictionary: T): {
        [group in keyof T]: {
            [key in keyof T[group]]: T[group][key] extends LocDictionaryEntry<infer P> ? LocComponent<P> : never;
        };
    };
    interface LocDictionary {
        [group: string]: LocGroup;
    }
    interface LocGroup {
        [key: string]: LocDictionaryEntry<any>;
    }
    interface LocDictionaryEntry<P> {
        create(id: string): LocStringRenderer<P>;
        propsAreEqual(prevProps: P, nextProps: P): boolean;
    }
    export type LocalizedStringArgs<A extends readonly string[] = any> = {
        fallback?: string | null;
    } & Record<A[number], LocReactNode>;
    type SingleProps<A extends readonly string[]> = LocalizedStringArgs<A>;
    export class Single<A extends readonly string[] = []> implements LocDictionaryEntry<SingleProps<A>> {
        private readonly argNames;
        constructor(...argNames: A);
        create(id: string): LocStringRenderer<SingleProps<A>>;
        propsAreEqual(prevProps: SingleProps<A>, nextProps: SingleProps<A>): boolean;
    }
    type HashedProps<A extends readonly string[]> = {
        hash: string;
    } & LocalizedStringArgs<A>;
    export class Hashed<A extends readonly string[] = []> implements LocDictionaryEntry<HashedProps<A>> {
        private readonly argNames;
        constructor(...argNames: A);
        create(id: string): LocStringRenderer<HashedProps<A>>;
        propsAreEqual(prevProps: HashedProps<A>, nextProps: HashedProps<A>): boolean;
    }
    type IndexedProps<A extends readonly string[]> = {
        index: number;
    } & LocalizedStringArgs<A>;
    export class Indexed<A extends readonly string[] = []> implements LocDictionaryEntry<IndexedProps<A>> {
        private readonly argNames;
        constructor(...argNames: A);
        create(id: string): LocStringRenderer<IndexedProps<A>>;
        propsAreEqual(prevProps: IndexedProps<A>, nextProps: IndexedProps<A>): boolean;
    }
    type HashedIndexedProps<A extends readonly string[]> = {
        hash: string;
        index: number;
    } & LocalizedStringArgs<A>;
    export class HashedIndexed<A extends readonly string[] = []> implements LocDictionaryEntry<HashedIndexedProps<A>> {
        private readonly argNames;
        constructor(...argNames: A);
        create(id: string): LocStringRenderer<HashedIndexedProps<A>>;
        propsAreEqual(prevProps: HashedIndexedProps<A>, nextProps: HashedIndexedProps<A>): boolean;
    }
}
declare module "common/localization/loc.generated" {
    export const Loc: {
        Achievements: {
            DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        AirPollutionInfoPanel: {
            AVERAGE_AIR_POLLUTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        AnimationCurve: {
            TIME_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            VALUE_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Asset: {
            DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NAME: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Assets: {
            ADDRESS_NAME_FORMAT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"NUMBER" | "ROAD", import("common/localization/loc-component").LocReactNode>>;
            ALLEY_NAME: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ANIMAL_NAME_DOG: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BIOME: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BRIDGE_NAME: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_NAME_FEMALE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_NAME_FORMAT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"FIRST_NAME" | "LAST_NAME", import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_NAME_MALE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_SURNAME_FEMALE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_SURNAME_HOUSEHOLD: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_SURNAME_MALE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_NAME: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CLIMATE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DAM_NAME: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DISTRICT_NAME: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DISTRICT_NAME_NEW: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HIGHWAY_NAME: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NAME: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NAMED_ADDRESS_NAME_FORMAT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"NAME" | "NUMBER" | "ROAD", import("common/localization/loc-component").LocReactNode>>;
            ROUTE_NAME: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<"NUMBER", import("common/localization/loc-component").LocReactNode>>;
            STREET_NAME: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SUB_SERVICE_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            THEME: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UPGRADE_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UPGRADE_NAME: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Budget: {
            TOOLTIP_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_TAX: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_TAX_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_TAX: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_TAX_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_UPKEEPS_PIECHART: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Chirper: {
            AIR_POLLUTION_HIGH: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AIR_POLLUTION_NEGATIVE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AURORA_BOREALIS: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BIRTH_RATE_DOWN: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BIRTH_RATE_UP: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BRAND: import("common/localization/loc-component").LocComponent<{
                hash: string;
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BRAND_RENTED_AGRICULTURE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            BRAND_RENTED_BANK: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            BRAND_RENTED_BAR: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            BRAND_RENTED_FACTORY: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            BRAND_RENTED_FORESTRY: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            BRAND_RENTED_GAS_STATION: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            BRAND_RENTED_HOTEL: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            BRAND_RENTED_MINE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            BRAND_RENTED_OFFICE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            BRAND_RENTED_OIL: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            BRAND_RENTED_RESTAURANT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            BRAND_RENTED_STORE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            BRAND_RENTED_WAREHOUSE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            CITY_SERVICE_EDUCATION_EXPORT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_SERVICE_EDUCATION_IMPORT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_SERVICE_ELECTRICITY_EXPORT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_SERVICE_ELECTRICITY_IMPORT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_SERVICE_FIRE_RESCUE_EXPORT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_SERVICE_FIRE_RESCUE_IMPORT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_SERVICE_GARBAGE_IMPORT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_SERVICE_HEALTHCARE_EXPORT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_SERVICE_HEALTHCARE_IMPORT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_SERVICE_POLICE_EXPORT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_SERVICE_POLICE_IMPORT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_SERVICE_POST_EXPORT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_SERVICE_POST_IMPORT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_SERVICE_WATER_SEWAGE_EXPORT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_SERVICE_WATER_SEWAGE_IMPORT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            COMMERCIAL_LEVEL_DOWN: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            COMMERCIAL_LEVEL_UP: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CRIME_NEGATIVE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CRIME_RATE_HIGH: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DIRTY_WATER: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_BLACKOUTS: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FIRST_POWER_PLANT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_TARGET_0", import("common/localization/loc-component").LocReactNode>>;
            GARBAGE_PILING_UP: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GOOD_EDUCATION_SERVICE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GOOD_WELFARE_SERVICE_COVERAGE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GROUND_POLLUTION_NEGATIVE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HIGHEST_TEMP_15: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HIGHEST_TEMP_NEG_15: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HIGH_DEATH_RATE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HOUSING_SHORTAGE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INDUSTRIAL_LEVEL_DOWN: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INDUSTRIAL_LEVEL_UP: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEISURE_TIME_NEGATIVE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEISURE_TIME_POSITIVE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOWEST_TEMP_15: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOWEST_TEMP_20: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOW_DEATH_RATE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOW_HEALTH: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOW_NUMBER_OF_WORKERS: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOW_WELLBEING: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MOSTLY_CLOUDY_WEATHER: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MOSTLY_RAINY_WEATHER: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MOSTLY_SNOWY_WEATHER: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MOSTLY_STORMY_WEATHER: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MOSTLY_SUNNY_WEATHER: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NOISE_POLLUTION: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NOISE_POLLUTION_NEGATIVE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NO_ELECTRICITY: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NO_WATER: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NO_WATER_SERVICE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OFFICE_LEVEL_DOWN: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OFFICE_LEVEL_UP: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OPENING_MESSAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PANEL_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PARK_COVERAGE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POLICE_CRIME_RATE_HIGH: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POLICIES: import("common/localization/loc-component").LocComponent<{
                hash: string;
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POLICIES_SERVICE: import("common/localization/loc-component").LocComponent<{
                hash: string;
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POOR_EDUCATION_SERVICE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RELIABLE_ELECTRICITY_SUPPLY: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RELIABLE_HEALTHCARE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RELIABLE_INTERNET_SERVICE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RELIABLE_POST_SERVICE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESIDENTIAL_LEVEL_DOWN: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESIDENTIAL_LEVEL_UP: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_CITIZEN: import("common/localization/loc-component").LocComponent<{
                hash: string;
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_COMMUNICATIONS: import("common/localization/loc-component").LocComponent<{
                hash: string;
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_ELECTRICITY: import("common/localization/loc-component").LocComponent<{
                hash: string;
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_GARBAGE: import("common/localization/loc-component").LocComponent<{
                hash: string;
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_ROADS: import("common/localization/loc-component").LocComponent<{
                hash: string;
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_TRANSPORTATION: import("common/localization/loc-component").LocComponent<{
                hash: string;
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_WATER: import("common/localization/loc-component").LocComponent<{
                hash: string;
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SEWAGE_NEGATIVE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SMALL_HOME: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STORAGES_FILLING_UP: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TEMPERATURE_COLDER: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOURISM_DOWN_20: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOURISM_UP_20: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TRAFFIC_ACCIDENT_NEARBY: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TRAFFIC_JAMS: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNCONNECTED_CITY: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNEMPLOYMENT_RATE_HIGH: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNPAID_LOAN: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNRELIABLE_ELECTRICITY_SUPPLY: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNRELIABLE_HEALTHCARE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNRELIABLE_INTERNET_SERVICE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNRELIABLE_POST_SERVICE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            VALENTINE_DAY: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_NEGATIVE_SERVICE_COVERAGE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_POLLUTION_NEGATIVE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        CinematicCamera: {
            CAPTURE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DELETE_DISCLAIMER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"NAME", import("common/localization/loc-component").LocReactNode>>;
            DELETE_KEY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DELETE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FOCAL_LENGTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HIDE_UI: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            KEY_FORMAT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"KEY", import("common/localization/loc-component").LocReactNode>>;
            LOAD: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAD_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOOP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NEW_SEQUENCE_PLACEHOLDER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OVERRIDE_DISCLAIMER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"NAME", import("common/localization/loc-component").LocReactNode>>;
            PLAYBACK_DURATION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESET_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SAVE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SAVE_LOAD_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SAVE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SEGMENT_DURATION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SIMULATION_SPEED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNSAVED_DISCLAIMER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        CityInfoPanel: {
            DEMAND_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEMAND_FACTOR: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEMAND_FACTOR_NEGATIVE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEMAND_FACTOR_POSITIVE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEMAND_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HAPPINESS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAB: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_DEMAND: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_HAPPINESS_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_HAPPINESS_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_POLICIES_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_POLICIES_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_DEMAND: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Climate: {
            SEASON: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Common: {
            ACTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BOUNDS_CELSIUS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"MIN" | "MAX", import("common/localization/loc-component").LocReactNode>>;
            BOUNDS_FAHRENHEIT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"MIN" | "MAX", import("common/localization/loc-component").LocReactNode>>;
            BOUNDS_KELVIN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"MIN" | "MAX", import("common/localization/loc-component").LocReactNode>>;
            BOUNDS_KILOWATT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"MIN" | "MAX", import("common/localization/loc-component").LocReactNode>>;
            BOUNDS_MEGAWATT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"MIN" | "MAX", import("common/localization/loc-component").LocReactNode>>;
            BOUNDS_PERCENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"MIN" | "MAX", import("common/localization/loc-component").LocReactNode>>;
            BOUNDS_TEMPERATURE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"MIN" | "MAX", import("common/localization/loc-component").LocReactNode>>;
            CANCEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CLEAR_SEARCH_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CLOSE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CONFIRMATION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CONTINUE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DATE_TIME_FORMAT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"MONTH" | "YEAR" | "HOUR" | "MINUTE", import("common/localization/loc-component").LocReactNode>>;
            DECIMAL_SEPARATOR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DELETE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DIALOG_ACTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DIALOG_MESSAGE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DIALOG_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DISABLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DLC_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DONT_SHOW_AGAIN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ENABLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ERROR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ERROR_DIALOG_CONTINUE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ERROR_DIALOG_COPY_DETAILS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ERROR_DIALOG_QUIT_GAME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ERROR_DIALOG_SAVE_QUIT_GAME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ERROR_DIALOG_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EXIT_DIALOG_TEXT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EXIT_DIALOG_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EXIT_TO_DESKTOP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EXIT_TO_MENU: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FOCUS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FRACTION_BODIES_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_CUBIC_METER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_CUBIC_METER_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_GALLON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_GALLON_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_INTEGER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_INTEGER_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_KG: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_KG_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_KILOWATT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_KILOWATT_HOURS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_MEGAWATT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_MEGAWATT_HOURS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_POUND: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_POUND_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_SHORT_TON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_SHORT_TON_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_TON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_TON_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            FRACTION_XP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "TOTAL", import("common/localization/loc-component").LocReactNode>>;
            GO_BACK_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HAPPINESS_FACTORS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HAPPINESS_FACTOR_NEGATIVE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HAPPINESS_FACTOR_POSITIVE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INFORMATION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INFOVIEW_MENU_TOGGLE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LANGUAGE_NAME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOCKED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MEDIUM_DATE_FORMAT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"MONTH" | "YEAR", import("common/localization/loc-component").LocReactNode>>;
            MONTH: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MONTH_SHORT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NO: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NONE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OK: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PAUSE_MENU_TOGGLE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SAVEABILITY_REASON: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SELECT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SUBMIT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            THOUSANDS_SEPARATOR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TIMESTAMP_FORMAT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TIMESTAMP_FORMAT_12: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TIME_FORMAT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"HOUR" | "MINUTE", import("common/localization/loc-component").LocReactNode>>;
            TIME_FORMAT_12: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"HOUR" | "MINUTE" | "PERIOD", import("common/localization/loc-component").LocReactNode>>;
            TIME_PERIOD_AM: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TIME_PERIOD_PM: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNLOCKED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            VALUE_ACRE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_ANGLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_BYTE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_CELSIUS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_CUBIC_METER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_CUBIC_METER_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_FAHRENHEIT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_FOOT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_GALLON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_GALLON_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_GIGABIT_PER_SECOND: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_GIGABYTE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_KELVIN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_KG_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_KILOBYTE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_KILOGRAM: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_KILOMETER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_KILOTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_KILOWATT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_MEGABYTE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_MEGAWATT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_MEGAWATT_HOURS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_METER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_MILE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_MILLION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_MONEY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_MONEY_PER_CELL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_MONEY_PER_HOUR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_MONEY_PER_KILOMETER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_MONEY_PER_KILOMETER_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_MONEY_PER_MILE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_MONEY_PER_MILE_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_MONEY_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE", import("common/localization/loc-component").LocReactNode>>;
            VALUE_MONTHS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE", import("common/localization/loc-component").LocReactNode>>;
            VALUE_PERCENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_PER_HOUR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_PER_SQUARE_METER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_POUND: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_POUND_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_SHORT_KILOTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_SHORT_TON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_SHORT_TON_PER_CELL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_SHORT_TON_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_SQUARE_FOOT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_SQUARE_KILOMETER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_SQUARE_METER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_TEMPERATURE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_TERABYTE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_THOUSAND: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_TON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_TON_PER_CELL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_TON_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_XP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_YARD: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "SIGN", import("common/localization/loc-component").LocReactNode>>;
            VALUE_YEAR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE", import("common/localization/loc-component").LocReactNode>>;
            VALUE_YEARS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE", import("common/localization/loc-component").LocReactNode>>;
            WARNING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            YES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        CompanyInfoPanel: {
            COMMERCIAL_PROFITABILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INDUSTRIAL_PROFITABILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OFFICE_PROFITABILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        DisasterInfoPanel: {
            CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EVACUATED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SHELTER_AVAILABILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        EconomyPanel: {
            BUDGET_BALANCE_LABEL: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUDGET_ITEM: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUDGET_ITEM_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUDGET_LOANS_BUTTON_ACCEPT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUDGET_LOANS_LABEL_AMOUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUDGET_LOANS_LABEL_CURRENT_INTEREST: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUDGET_LOANS_LABEL_DAILY_INTEREST: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUDGET_LOANS_LABEL_DAILY_PAYMENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUDGET_LOANS_LABEL_DAYS_REMAINING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUDGET_LOANS_LABEL_GET_MORE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUDGET_LOANS_LABEL_NEW_LOAN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUDGET_LOANS_LABEL_PAY_BACK: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUDGET_SUB_ITEM: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUDGET_TITLE_LOANS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EXPENSES_SECTION_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INCOME_SECTION_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAN_CHART_BUDGET_WARNING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAN_CHART_BUDGET_WARNING_MODIFIED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAN_CHART_DEFICIT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"TOTAL", import("common/localization/loc-component").LocReactNode>>;
            LOAN_CHART_LEGEND_EXPENSES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAN_CHART_LEGEND_INCOME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAN_CHART_LEGEND_LOAN_INTEREST: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAN_CHART_RELATIVE_COST: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"RELATIVE_LOAN_COST", import("common/localization/loc-component").LocReactNode>>;
            LOAN_CHART_REMAINING_SURPLUS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"TOTAL", import("common/localization/loc-component").LocReactNode>>;
            LOAN_CHART_SURPLUS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"TOTAL", import("common/localization/loc-component").LocReactNode>>;
            LOAN_FORM_ACCEPT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAN_FORM_LESS_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAN_FORM_LIMIT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"LOAN_LIMIT", import("common/localization/loc-component").LocReactNode>>;
            LOAN_FORM_MORE_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAN_FORM_RESET: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAN_PAGE_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAN_PAGE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAN_TABLE_AMOUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAN_TABLE_INTEREST_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAN_TABLE_MONTHLY_COST: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MONTHLY_BALANCE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_LABEL_EXPORT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_LABEL_IMPORT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_LABEL_INTERNAL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_LABEL_TOTAL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_PAGE_DEFICIT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_PAGE_EXPORT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_PAGE_IMPORT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_PAGE_PRODUCTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_PAGE_PRODUCTIONLINK: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_PAGE_SURPLUS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_TAB_IMMATERIAL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_TAB_MATERIAL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_TOOLTIP_COMMERCIAL_CONSUMPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_TOOLTIP_COMMERCIAL_WEALTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_TOOLTIP_INDUSTRIAL_CONSUMPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_TOOLTIP_INDUSTRIAL_PRODUCTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_TOOLTIP_INDUSTRIAL_WEALTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_TOOLTIP_OFFICE_CONSUMPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION_TOOLTIP_OFFICE_WEALTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESOURCE_CATEGORY: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_BUILDING_EMPLOYMENT_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_FEES_LABEL_DAILY_EXPENSES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_FEES_LABEL_DAILY_INCOME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_FEES_LABEL_EXPORT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_FEES_LABEL_FEES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_FEES_LABEL_IMPORT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_FEES_LABEL_SERVICE_FEE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_LABEL_TOTAL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_LABEL_UPKEEP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_LABEL_WAGES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_RESOURCE_FEE_CONSUMPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_RESOURCE_FEE_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_RESOURCE_FEE_EFFICIENCY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_RESOURCE_FEE_HAPPINESS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_RESOURCE_FEE_LABEL: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_RESOURCE_LABEL: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_TITLE_BUDGET: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_TITLE_BUDGETS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_TITLE_BUILDINGS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_TITLE_EXPENSES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_TITLE_IMPORT_BUDGET: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_TITLE_INCOME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_TITLE_INFO: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICES_TITLE_QUALITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICE_BUDGET_EFFICIENCY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICE_BUDGET_SLIDER_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICE_BUDGET_SLIDER_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICE_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICE_EXPORT_REVENUE_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICE_FEES_REVENUE_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICE_IMPORT_EXPENSE_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SERVICE_UPKEEP_EXPENSE_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAB: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAXATION_COMMERCIAL_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAXATION_INDUSTRIAL_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAXATION_OFFICE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAXATION_RESIDENTIAL_LABEL_JOBLEVEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAXATION_RESIDENTIAL_SLIDER_JOBLEVEL: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAXATION_RESIDENTIAL_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAXATION_TAX_GROUP: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAX_AREA_COLUMN_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAX_AREA_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAX_AREA_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAX_INCOME_ESTIMATE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAX_INCOME_TOTAL_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAX_RANGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"MIN" | "MAX", import("common/localization/loc-component").LocReactNode>>;
            TAX_RATE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Editor: {
            ACTIVE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ADD_COMPONENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ADD_FAVORITE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ADD_TRANSLATION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSETS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_CATEGORY_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BACK_TO_EDITOR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BETA_BANNER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BORDER_RIVER_WATER_SOURCES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BORDER_SEA_WATER_SOURCES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CAMERA_ANGLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CAMERA_PIVOT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CAMERA_STARTING_POSITION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CAMERA_ZOOM: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CAPTURE_CAMERA_POSITION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CHANGE_PREFAB_TYPE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CHECKLIST: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CHECKLIST_AIR_CONNECTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CHECKLIST_ELECTRICITY_CONNECTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CHECKLIST_FERTILE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CHECKLIST_FOREST: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CHECKLIST_OIL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CHECKLIST_OPTIONAL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CHECKLIST_ORE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CHECKLIST_REQUIRED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CHECKLIST_ROAD_CONNECTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CHECKLIST_STARTING_TILES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CHECKLIST_TRAIN_CONNECTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CHECKLIST_WATER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CLEAR_ALL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CLEAR_RESOURCE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CLIMATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CLIMATE_SETTINGS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CLOUDINESS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CONSTANT_LEVEL_WATER_SOURCES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CONSTANT_RATE_WATER_SOURCES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CURRENT_YEAR_AS_DEFAULT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DELETE_COMPONENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DRIVES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DUPLICATE_TEMPLATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DUPLICATE_TEMPLATE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELEMENT_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"COUNT", import("common/localization/loc-component").LocReactNode>>;
            EXPORT_HEIGHTMAP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EXPORT_WORLDMAP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FILE_NAME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FILTERS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FLOOD_HEIGHT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HEIGHT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HEIGHTMAPS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HEIGHT_OFFSET: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HEIGHT_SCALE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            IMPORT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            IMPORTING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            IMPORT_HEIGHTMAP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            IMPORT_RESOURCE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            IMPORT_WORLDMAP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INCORRECT_HEIGHTMAP_MESSAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"HEIGHT" | "WIDTH", import("common/localization/loc-component").LocReactNode>>;
            INCORRECT_HEIGHTMAP_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INSTALOD_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LIST_ADD_ITEM: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LIST_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"COUNT", import("common/localization/loc-component").LocReactNode>>;
            LIST_ITEM_COLLAPSE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LIST_ITEM_DELETE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LIST_ITEM_DUPLICATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LIST_ITEM_EXPAND: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LIST_ITEM_MOVE_DOWN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LIST_ITEM_MOVE_UP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAD: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAD_MAP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOCATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAP_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAP_NAME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAP_NAME_AS_DEFAULT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAP_OFFSET: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAP_SETTINGS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAP_SIZE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MATERIALS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NEW_MAP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NEXT_PAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NONE_VALUE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OBJECTCONTAINER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OPEN_DIRECTORY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PAGE_FORMAT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"TOTAL" | "CURRENT", import("common/localization/loc-component").LocReactNode>>;
            POLLUTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POSITION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PREVIEW: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PREVIOUS_PAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PROCESSING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RADIUS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            REMOVE_FAVORITE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            REMOVE_WORLDMAP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESET_CLOUDINESS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESET_TIME_OF_DAY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESET_TIME_OF_YEAR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESOURCES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESOURCE_TEXTURE_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESOURCE_TOOLS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SAVE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SAVE_MAP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SEARCH_PLACEHOLDER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SELECT_DIRECTORY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SELECT_STARTING_TILES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SELECT_TEMPLATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STARTING_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STARTING_TIME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STARTING_YEAR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STOP_SELECTING_STARTING_TILES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TERRAIN_TOOLS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            THEME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            THUMBNAIL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TIME_OF_DAY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TIME_OF_YEAR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOL: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNIQUE_MAP_ID: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_SETTINGS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_SIMULATION_SPEED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WIDGETS: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WORKSPACE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WORLD_OFFSET: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WORLD_SIZE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        EducationInfoPanel: {
            AVAILABILITY: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EDUCATION_DISTRIBUTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EDUCATION_LEVEL: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELIGIBLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEVEL: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STUDENTS: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STUDENT_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        ElectricityInfoPanel: {
            BATTERY_CHARGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_AVAILABILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_EXPORT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_IMPORT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_TRADE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_TRANSMISSION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_TRANSMITTED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TEMPERATURE_EFFECT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        EventJournal: {
            EFFECT: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EVENT_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OPENING_MESSAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PANEL_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        FireAndRescueInfoPanel: {
            AVERAGE_FIRE_HAZARD: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        GameListScreen: {
            BIOME_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUILDABLE_AREA_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_DETAILS_SECTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CLIMATE_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CONFIRM_OVERWRITE_MESSAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CONFIRM_UNSAVED_MESSAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CONNECTIONS_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EQUATOR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GAME_OPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GAME_OPTIONS_SECTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GAME_OPTION_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HEMISPHERE_N: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HEMISPHERE_S: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INVALID_NAME_MESSAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LAST_MODIFIED_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LATITUDE_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAD_GAME_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAD_GAME_SCREEN_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAP_DETAILS_SECTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAP_FILTER: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAP_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MONEY_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NEW_GAME_SCREEN_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POPULATION_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESOURCES_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SAVE_GAME_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SAVE_GAME_SCREEN_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SAVE_NAME_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            START_GAME_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            THEME_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TUTORIAL_OPTIONS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_AVAILABILITY_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            XP_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        GarbageInfoPanel: {
            GARBAGE_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LANDFILL_AVAILABILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PROCESSING_STATUS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STORED_GARBAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        GroundPollutionInfoPanel: {
            AVERAGE_GROUND_POLLUTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        HealthcareInfoPanel: {
            AVERAGE_HEALTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CEMETERY_AVAILABILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CREMATORIUM_AVAILABILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEATH_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HEALTHCARE_AVAILABILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            N_A: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OCCUPIED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PATIENTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PROCESSING_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SICK_OR_INJURED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        ISO: {
            COUNTRY: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        InfoPanels: {
            CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CONSUMPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OUTPUT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PROCESSING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STORED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Infoviews: {
            AVAILABILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AVERAGE_AIR_POLLUTION_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AVERAGE_COMMERCIAL_PROFIT_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AVERAGE_FIRE_HAZARD_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AVERAGE_GROUND_POLLUTION_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AVERAGE_INDUSTRIAL_PROFIT_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AVERAGE_LAND_VALUE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AVERAGE_NOISE_POLLUTION_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AVERAGE_OFFICE_PROFIT_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AVERAGE_WATER_POLLUTION_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BATTERY_CHARGE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CEMETERY_AVAILABILITY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_ATTRACTIVENESS_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CLOSE_INFO_VIEW: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            COLLEGE_AVAILABILITY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            COMMERCIAL_LEVEL_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CREMATORIUM_AVAILABILITY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CRIME_PROBABILITY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EDUCATION_DISTRIBUTION_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_AVAILABILITY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_TRADE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_TRANSMISSION_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELEMENTARY_SCHOOL_AVAILABILITY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EXPORT_DISTRIBUTION_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FERTILE_LAND_RESOURCE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GARBAGE_PROCESSING_STATUS_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HAPPINESS_FACTORS_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HEALTHCARE_AVAILABILITY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HIGH_SCHOOL_AVAILABILITY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            IMPORT_DISTRIBUTION_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INDUSTRIAL_LEVEL_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INFOMODE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INFOMODE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INFOMODE_TYPE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INFOVIEW: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            JAIL_AVAILABILITY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LABEL: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LABEL_VALUE_FORMAT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "LABEL", import("common/localization/loc-component").LocReactNode>>;
            LANDFILL_AVAILABILITY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAIL_SERVICE_AVAILABILITY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAP_LEGEND: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OFFICE_LEVEL_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OIL_RESOURCE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ORE_RESOURCE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PARKING_AVAILABILITY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POPULATION_DISTRIBUTION_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POPULATION_STATISTICS_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRISON_AVAILABILITY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            REMOVE_USER_INFOVIEW_BUTTON_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESIDENTIAL_LEVEL_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SEWAGE_TRADE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SEWAGE_TREATMENT_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SHELTER_AVAILABILITY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TITLE_INFOVIEWS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TRAFFIC_FLOW: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNIVERSITY_AVAILABILITY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            USER_INFOMODE_SELECTOR_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_AVAILABILITY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_STORAGE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_TRADE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WOOD_RESOURCE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WORKPLACE_DISTRIBUTION_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        LandValueInfoPanel: {
            AVERAGE_LAND_VALUE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        LevelInfoPanel: {
            COMMERCIAL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INDUSTRIAL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEVEL1: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEVEL2: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEVEL3: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEVEL4: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEVEL5: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OFFICE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESIDENTIAL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        LifePath: {
            BACK_BUTTON_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_BECAME_UNEMPLOYED: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_BIRTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_COMMITED_ROBBERY: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_DIED: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_FAILED_SCHOOL: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_GOT_ARRESTED: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_GOT_INJURED: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_GOT_INJURED_BY_EVENT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_GOT_IN_DANGER_BY_EVENT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_GOT_SICK: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_GOT_SICK_BY_EVENT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_GOT_TRAPPED: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_GOT_TRAPPED_BY_EVENT: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_GRADUATED: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_IN_DANGER: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_LOST_FAMILY_MEMBER: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_MADE_BABY_COUPLE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1" | "LINK_2", import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_MADE_BABY_SINGLE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_MOVED_HOUSE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_MOVED_OUT_OF_CITY: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_PARTNERED_UP: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_SENTENCED_TO_PRISON: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_SEPARATED: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_STARTED_SCHOOL: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_STARTED_WORKING: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            DETAIL_PANEL_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"NAME", import("common/localization/loc-component").LocReactNode>>;
            LIST_PANEL_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAX_EXCEEDED_WARNING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OCCUPATION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"OCCUPATION" | "WORKPLACE_LINK" | "WORKPLACE_NAME", import("common/localization/loc-component").LocReactNode>>;
            OPENING_MESSAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOURIST_LEFT_CITY: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<"LINK_1", import("common/localization/loc-component").LocReactNode>>;
            UNFOLLOW_BUTTON_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Loading: {
            HINTMESSAGE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOADING_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Main: {
            TOOLTIP_DESCRIPTION_ADVISOR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_CAMERA_MODES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_CHIRPER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_CINEMATIC_CAMERA: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_CITY_ECONOMY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_CITY_INFO: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_CITY_NAME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_CITY_STATISTICS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_CURRENT_MILESTONE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_DATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_DEMAND: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_ELEVATION_LOWER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_ELEVATION_RAISE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_EVENT_JOURNAL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_FOLLOWED_CITIZENS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_FREE_CAMERA: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_GAME_SPEED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_INFO_VIEWS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_LINES_OVERVIEW: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_MAP_TILES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_MAX_MILESTONE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_MONEY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_NOTIFICATIONS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_PAUSE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_PAUSE_MENU: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_PHOTO_MODE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_PLAY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_POPULATION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_PROGRESSION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_RADIO: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_UNDERGROUND: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_UNLIMITED_MONEY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_WEATHER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_XP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_ADVISOR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_CAMERA_MODES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_CHIRPER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_CINEMATIC_CAMERA: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_CITY_ECONOMY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_CITY_INFO: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_CITY_NAME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_CITY_STATISTICS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_CURRENT_MILESTONE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_DATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_DEMAND: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_ELEVATION_LOWER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_ELEVATION_RAISE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_EVENT_JOURNAL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_FOLLOWED_CITIZENS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_FREE_CAMERA: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_GAME_SPEED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_INFO_VIEWS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_LINES_OVERVIEW: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_MAP_TILES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_MONEY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_NOTIFICATIONS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_PAUSE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_PAUSE_MENU: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_PHOTO_MODE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_PLAY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_POPULATION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_PROGRESSION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_RADIO: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_UNDERGROUND: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_WEATHER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_XP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        MapTilePurchase: {
            AVAILABLE_EXPANSION_PERMITS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE", import("common/localization/loc-component").LocReactNode>>;
            AVAILABLE_WATER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUILDABLE_AREA: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            COST: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PERMITS_USED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PURCHASE_STATUS: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PURCHASE_STATUS_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESOURCE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESOURCES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_COST: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_RESOURCE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TILES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_COST: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_RESOURCE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_TILES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Maps: {
            MAP_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAP_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OUTSIDE_CONNECTIONS: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESOURCE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Menu: {
            ACHIEVEMENTS_WARNING_GAME_OPTIONS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ACHIEVEMENTS_WARNING_MODS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            APPLICATION_QUIT_MESSAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_ADD_LINK: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_ADD_PREVIEW_SCREENSHOT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_COMPLETE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_ERROR_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_ERROR_LINK: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_ERROR_NAME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_FAILURE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_FORUM_LINK: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"INDEX", import("common/localization/loc-component").LocReactNode>>;
            ASSET_FORUM_LINKS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_LONG_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_NAME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_PREPARING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_PREVIEW_SCREENSHOTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_REMOVE_PREVIEW_SCREENSHOT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_SCREENSHOT_EMPTY_SUBTITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_SCREENSHOT_EMPTY_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_SHORT_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_SUBMITTING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_TYPE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ASSET_UPLOAD: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BACK: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BETA_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_NAME_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CLOUD_TARGET: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CLOUD_TARGET_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CONFIRM_DELETE_SAVE_WARNING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CONTINUE_GAME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CREDITS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DLC_TAB: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EDITOR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EMPTY_SLOT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EXIT_APPLICATION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EXIT_GAME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GDKCLOUD_SAVE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAD_GAME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAD_GAME_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAP_OPTIONS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MODS_TAB: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NAME_FIELD_PLACEHOLDER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NEWS_PANEL_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NEW_GAME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NOTIFICATIONS_OPENING_MESSAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NOTIFICATIONS_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NOTIFICATION_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NOTIFICATION_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OPTIONS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PARADOXCLOUD_SAVE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PDX_MODS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RANDOM_CITY_NAME_BUTTON_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            READONLY_SAVE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESUME_GAME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SAVE_GAME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SAVE_LIST: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SAVE_OPTIONS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SAVE_ORDERING: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SAVE_ORDERING_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SELECT_MAP_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SIMULATION_DATE_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            START_GAME_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STEAMCLOUD_SAVE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SWITCH_USER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNLIMITED_MONEY_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        NaturalResourcesInfoPanel: {
            AVAILABLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FERTILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FOREST: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OIL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ORE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RENEWAL_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        NoisePollutionInfoPanel: {
            AVERAGE_NOISE_POLLUTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Notifications: {
            DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Options: {
            ANTIALIASINGMETHOD: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AUTORELOADMODE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AUTOSAVECOUNT: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AUTOSAVEINTERVAL: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CURSORMODE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEPTHOFFIELDMODE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEPTHOFFIELDRESOLUTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DESCRIPTION_BOX_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DISPLAYMODE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DISPLAY_MODE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DISPLAY_SETTINGS_CONFIRM: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DISPLAY_SETTINGS_PROMPT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DISPLAY_SETTINGS_REVERT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"TIME", import("common/localization/loc-component").LocReactNode>>;
            DLSSQUALITY: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DYNRESUPSCALEFILTER: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ERROR_TOOLCHAIN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ERROR_TOOLCHAIN_DEPENDENCY_DOWNLOAD: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"DEPENDENCY_NAME", import("common/localization/loc-component").LocReactNode>>;
            ERROR_TOOLCHAIN_DEPENDENCY_DOWNLOAD_DETAILS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"DEPENDENCY_NAME" | "DETAILS", import("common/localization/loc-component").LocReactNode>>;
            ERROR_TOOLCHAIN_DEPENDENCY_INSTALL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"DEPENDENCY_NAME", import("common/localization/loc-component").LocReactNode>>;
            ERROR_TOOLCHAIN_DEPENDENCY_INSTALL_DETAILS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"DEPENDENCY_NAME" | "DETAILS", import("common/localization/loc-component").LocReactNode>>;
            ERROR_TOOLCHAIN_DEPENDENCY_UNINSTALL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"DEPENDENCY_NAME", import("common/localization/loc-component").LocReactNode>>;
            ERROR_TOOLCHAIN_DEPENDENCY_UNINSTALL_DETAILS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"DEPENDENCY_NAME" | "DETAILS", import("common/localization/loc-component").LocReactNode>>;
            ERROR_TOOLCHAIN_INSTALL_UNKNOWN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ERROR_TOOLCHAIN_NO_SPACE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ERROR_TOOLCHAIN_NO_SPACE_DETAILS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"DETAILS", import("common/localization/loc-component").LocReactNode>>;
            ERROR_TOOLCHAIN_UNINSTALL_UNKNOWN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FILTERMODE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FPSMODE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FSR2QUALITY: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GROUP: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INPUTHINTSTYPE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INPUT_CONFLICT_NO_RESOLUTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"CONTROL" | "BINDING", import("common/localization/loc-component").LocReactNode>>;
            INPUT_CONFLICT_PROMPT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"CONTROL" | "BINDING", import("common/localization/loc-component").LocReactNode>>;
            INPUT_CONFLICT_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INPUT_CONTROL: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INPUT_CONTROL_PS: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INPUT_CONTROL_UNSET: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INPUT_MODIFIER_SEPARATOR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INPUT_REBIND_PROMPT: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INPUT_REBIND_PROMPT_PS: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INPUT_REBIND_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"BINDING", import("common/localization/loc-component").LocReactNode>>;
            INTERFACE_STYLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEVEL: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MOD_TOOLCHAIN_STATUS: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MSAASAMPLES: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OPTION_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PERFORMANCEPREFERENCE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SCREEN_RESOLUTION_FORMAT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"HEIGHT" | "WIDTH" | "REFRESH_RATE", import("common/localization/loc-component").LocReactNode>>;
            SCREEN_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SEARCH: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SECTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SKINNING: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SMAAQUALITYLEVEL: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STATE_TOOLCHAIN: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<"VERSION", import("common/localization/loc-component").LocReactNode>>;
            SWAP_BINDINGS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAB: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TEMPERATUREUNIT: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TIMEFORMAT: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNITSYSTEM: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNSET_BINDINGS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNSET_BINDING_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WARNING: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WARN_TOOLCHAIN_DEPENDENCY_UNINSTALL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"DEPENDENCY_NAME", import("common/localization/loc-component").LocReactNode>>;
            WARN_TOOLCHAIN_DOTNET_UNINSTALL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"DOTNET_VERSION", import("common/localization/loc-component").LocReactNode>>;
            WARN_TOOLCHAIN_INSTALL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"UNITY_VERSION" | "HOST", import("common/localization/loc-component").LocReactNode>>;
            WARN_TOOLCHAIN_INSTALL_DOTNET: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"DOTNET_VERSION" | "HOST", import("common/localization/loc-component").LocReactNode>>;
            WARN_TOOLCHAIN_INSTALL_MOD_PROJECT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WARN_TOOLCHAIN_INSTALL_NEW: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"DEPENDENCIES", import("common/localization/loc-component").LocReactNode>>;
            WARN_TOOLCHAIN_INSTALL_NODEJS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"HOST" | "NODEJS_VERSION", import("common/localization/loc-component").LocReactNode>>;
            WARN_TOOLCHAIN_INSTALL_PROJECT_TEMPLATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WARN_TOOLCHAIN_INSTALL_UNITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"UNITY_VERSION" | "HOST", import("common/localization/loc-component").LocReactNode>>;
            WARN_TOOLCHAIN_INSTALL_UNITY_LICENSE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WARN_TOOLCHAIN_MIN_VERSION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"MIN_VERSION", import("common/localization/loc-component").LocReactNode>>;
            WARN_TOOLCHAIN_NODEJS_UNINSTALL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"NODEJS_VERSION", import("common/localization/loc-component").LocReactNode>>;
            WARN_TOOLCHAIN_UNINSTALL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"UNITY_VERSION", import("common/localization/loc-component").LocReactNode>>;
            WARN_TOOLCHAIN_UNINSTALL_NEW: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"DEPENDENCIES", import("common/localization/loc-component").LocReactNode>>;
            WARN_TOOLCHAIN_UNITY_LICENSE_RETURN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WARN_TOOLCHAIN_UNITY_UNINSTALL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"UNITY_VERSION", import("common/localization/loc-component").LocReactNode>>;
        };
        OutsideConnectionsInfoPanel: {
            TOP_EXPORTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOP_IMPORTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Overlay: {
            BACK_ACTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CHANGE_USER_ACTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CONTROLLER_DISCONNECTED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CONTROLLER_PAIRING_CHANGED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CORRUPT_SAVE_DATA: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DELETED_ITEMS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ENGAGEMENT_PROMPT_GAMEPAD: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ENGAGEMENT_PROMPT_KEYBOARD: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PLEASE_LOG_IN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PLEASE_WAIT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRESS_ANY_KEY_ACTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PROCEED_ACTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            QUIT_ACTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            START_GAME_ACTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SWITCH_USER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            USER_LOGGED_OUT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            VALIDATION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Paradox: {
            ACCOUNT_LINK_CONFIRMATION_TEXT: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ACCOUNT_LINK_OVERWRITE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ACCOUNT_LINK_PROMPT_SHORT: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ACCOUNT_LINK_PROMPT_TEXT: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ACCOUNT_LINK_PROMPT_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BIRTH_DATE_DAY_FIELD_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BIRTH_DATE_FIELD_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BIRTH_DATE_MONTH_FIELD_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BIRTH_DATE_YEAR_FIELD_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            COUNTRY_FIELD_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EMAIL_FIELD_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ERROR: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FORGOT_PASSWORD_LINK: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LINK_ACCOUNT_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOADING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOGIN_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOGIN_FORM_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOGIN_FROM_REGISTRATION_LINK: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOGIN_INCENTIVE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOGOUT_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MARKETING_PERMISSION_TOGGLE_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MODS_NEED_INTERNET: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NO_INTERNET_CONNECTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PARADOX_ACCOUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PASSWORD_FIELD_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PASSWORD_RESET_CONFIRMATION_TEXT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"EMAIL", import("common/localization/loc-component").LocReactNode>>;
            PDX_ACCOUNT_LINK_OVERWRITE_PROMPT_TEXT: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PDX_MODS_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PDX_PLATFORM_ACCOUNT_LINK_OVERWRITE_PROMPT_TEXT: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PLATFORM_ACCOUNT_LINK_OVERWRITE_PROMPT_TEXT: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRIVACY_POLICY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            REGISTRATION_CONFIRMATION_TEXT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            REGISTRATION_CONFIRMATION_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            REGISTRATION_FORM_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            REGISTRATION_FROM_LOGIN_LINK: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            REGISTRATION_HINT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            REMEMBER_ME_TOGGLE_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SDK_DISABLED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SKIP_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SUBMIT_REGISTRATION_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TERMS_OF_USE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TERMS_PRIVACY_NOTICE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UGC_DISABLED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNKNOWN_ERROR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNLINK_ACCOUNT_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        PhotoMode: {
            CAPTURE_PROPERTY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CLOUDEROSIONNOISE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEPTHOFFIELDMODE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ENABLE_PROPERTY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ENVIRONMENT_DISCLAIMER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FILMGRAINLOOKUP: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GATEFITMODE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HIDE_UI: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PROPERTY_LIMIT_MAX: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PROPERTY_LIMIT_MIN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PROPERTY_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PROPERTY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESET_PROPERTY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SENSORTYPE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAB: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAKE_PHOTO: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOGGLE_FIRST_PERSON_MODE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOGGLE_ORBIT_MODE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        PoliceInfoPanel: {
            ARRESTED_CRIMINALS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AVERAGE_CRIME_PROBABILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CRIME_SUCCESS_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CURRENT_NUMBER_OF_CRIMINALS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            IN_JAIL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            IN_PRISON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            JAIL_AVAILABILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NUMBER_OF_CRIMES_PER_MONTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRISONERS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRISON_AVAILABILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Policy: {
            DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        PopulationInfoPanel: {
            ADULTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AGE_DISTRIBUTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BIRTH_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CHILDREN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEATH_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EMPLOYED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            JOBS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MOVED_AWAY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MOVED_IN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POPULATION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SENIORS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TEENS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNEMPLOYMENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        PostInfoPanel: {
            COLLECTED_MAIL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DELIVERED_MAIL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAIL_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAIL_SERVICE_AVAILABILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Progression: {
            AVAILABLE_DEV_POINTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE", import("common/localization/loc-component").LocReactNode>>;
            BUILDING_UNLOCK_EVENT_PANEL_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUILD_LATER_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUILD_NOW_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEVELOPMENT_POINTS_FIELD_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEV_TREE_UNLOCK_BADGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HAPPINESS_REQUIREMENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE", import("common/localization/loc-component").LocReactNode>>;
            INCLUDES_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAN_LIMIT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAP_TILES_FIELD_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAX_MILESTONE_FIELD_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MILESTONE_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MILESTONE_DETAIL_SUBTITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"INDEX", import("common/localization/loc-component").LocReactNode>>;
            MILESTONE_NAME: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MILESTONE_PROGRESS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"CURRENT" | "TARGET", import("common/localization/loc-component").LocReactNode>>;
            MILESTONE_REWARDS_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MILESTONE_UNLOCK_PANEL_TITLE_PRIMARY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"INDEX", import("common/localization/loc-component").LocReactNode>>;
            MILESTONE_UNLOCK_PANEL_TITLE_SECONDARY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MILESTONE_UNLOCK_REQUIREMENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"NAME" | "INDEX", import("common/localization/loc-component").LocReactNode>>;
            NODE_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NODE_DEV_POINTS_REQUIREMENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NODE_LOCKED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NODE_MILESTONE_REQUIREMENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"INDEX", import("common/localization/loc-component").LocReactNode>>;
            NODE_NAME: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NODE_PARENT_REQUIREMENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NODE_UNLOCKED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NODE_UNLOCK_REQUIREMENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"NAME", import("common/localization/loc-component").LocReactNode>>;
            OBJECT_BUILT_REQUIREMENT: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<"CURRENT" | "TARGET", import("common/localization/loc-component").LocReactNode>>;
            PANEL_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POINTS_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"POINTS", import("common/localization/loc-component").LocReactNode>>;
            POPULATION_REQUIREMENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE", import("common/localization/loc-component").LocReactNode>>;
            PROCESSING_REQUIREMENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"CURRENT" | "RESOURCE" | "TARGET", import("common/localization/loc-component").LocReactNode>>;
            PROGRESSION_PANEL_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PURCHASE_BUTTON_AVAILABLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PURCHASE_BUTTON_LOCKED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PURCHASE_BUTTON_MILESTONE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"MILESTONE", import("common/localization/loc-component").LocReactNode>>;
            PURCHASE_BUTTON_POINTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PURCHASE_BUTTON_UNLOCKED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PURCHASE_COST: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"COST", import("common/localization/loc-component").LocReactNode>>;
            RADIO_MAST_REQUIREMENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"CURRENT" | "TARGET", import("common/localization/loc-component").LocReactNode>>;
            REQUIREMENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            REQUIREMENTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            REQUIRES_ALL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            REQUIRES_ANY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            REWARD_FIELD_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STRICT_OBJECT_BUILT_REQUIREMENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"NAME" | "CURRENT" | "TARGET", import("common/localization/loc-component").LocReactNode>>;
            TAB_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_EXPANSION_PERMITS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_MONEY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_POINTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_POINTS_COST: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION_TAB: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_EXPANSION_PERMITS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_MONEY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_POINTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_POINTS_COST: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE_TAB: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TUTORIAL_REQUIREMENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNLOCK_NODE_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            XP_REASON: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ZONE_BUILT_REQUIREMENT_CELLS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"CURRENT" | "TARGET" | "ZONE", import("common/localization/loc-component").LocReactNode>>;
            ZONE_BUILT_REQUIREMENT_CELLS_LEVEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"CURRENT" | "LEVEL" | "TARGET" | "ZONE", import("common/localization/loc-component").LocReactNode>>;
            ZONE_BUILT_REQUIREMENT_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"CURRENT" | "TARGET" | "ZONE", import("common/localization/loc-component").LocReactNode>>;
            ZONE_BUILT_REQUIREMENT_COUNT_LEVEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"CURRENT" | "LEVEL" | "TARGET" | "ZONE", import("common/localization/loc-component").LocReactNode>>;
        };
        Properties: {
            ADJUST_HAPPINESS_MODIFIER_EFFECT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"TARGET" | "DELTA" | "TYPE", import("common/localization/loc-component").LocReactNode>>;
            ADJUST_HAPPINESS_MODIFIER_TARGET: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ADJUST_HAPPINESS_MODIFIER_TYPE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AMBULANCE_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ATTRACTION_EFFECT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE", import("common/localization/loc-component").LocReactNode>>;
            ATTRACTIVENESS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BATTERY_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BATTERY_POWER_OUTPUT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CARGO_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_MODIFIER: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_MODIFIER_EFFECT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"DELTA" | "TYPE", import("common/localization/loc-component").LocReactNode>>;
            COMFORT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CONSTRUCTION_COST: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DECEASED_PROCESSING_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DECEASED_STORAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_CONSUMPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EVACUATION_BUS_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FIRE_ENGINE_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FIRE_HELICOPTER_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GARBAGE_ACCUMULATION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GARBAGE_PROCESSING_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GARBAGE_STORAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GARBAGE_TRUCK_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HEARSE_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            JAIL_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEISURE_PROVIDER_EFFECT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"TYPE" | "EFFICIENCY", import("common/localization/loc-component").LocReactNode>>;
            LEISURE_TYPE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOCAL_MODIFIER: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOCAL_MODIFIER_EFFECT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"RADIUS" | "DELTA" | "TYPE", import("common/localization/loc-component").LocReactNode>>;
            MAIL_BOX_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAIL_SORTING_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAIL_STORAGE_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAINTENANCE_VEHICLES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAP_RESOURCE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAX_UPKEEP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MEDICAL_HELICOPTER_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NETWORK_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NETWORK_RANGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PATIENT_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PATROL_CAR_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POLICE_HELICOPTER_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POST_TRUCK_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POST_VAN_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POWER_LINE_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POWER_PLANT_OUTPUT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POWER_VOLTAGE_FORMAT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"VALUE" | "VOLTAGE", import("common/localization/loc-component").LocReactNode>>;
            PRISON_VAN_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            REQUIRED_RESOURCE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESOURCE_CONSUMPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SEWAGE_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SEWAGE_PIPE_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SEWAGE_PURIFICATION_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SHELTER_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STUDENT_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TRANSFORMER_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TRANSFORMER_INPUT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TRANSFORMER_OUTPUT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TRANSPORT_STOP_COUNT: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TRANSPORT_VEHICLE_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UPKEEP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            VOLTAGE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_CONSUMPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_PIPES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_PIPE_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_PIPE_TYPE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_PURIFICATION_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_STORAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Radio: {
            ADS_BUTTON_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EMERGENCY_MESSAGE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FOCUS_EMERGENCY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MUTE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NETWORK_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NETWORK_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NEXT_BUTTON_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PANEL_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PAUSE_BUTTON_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PLAY_BUTTON_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PREVIOUS_BUTTON_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PROGRAM_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNMUTE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            VOLUME_BUTTON_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Resources: {
            TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        RoadsInfoPanel: {
            PARKED_CARS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PARKING_AVAILABILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PARKING_INCOME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TRAFFIC_FLOW: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        SelectedInfoPanel: {
            ACTIVE_VEHICLES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ADVISER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AIR_POLLUTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ALL_IS_WELL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ANIMAL_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ANIMAL_TYPE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ATTRACTIVENESS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ATTRACTIVENESS_BASE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ATTRACTIVENESS_FACTORS: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AVERAGE_HOUSEHOLD_WEALTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BATTERY_STATE_CHARGING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BATTERY_STATE_DISCHARGING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BATTERY_STATE_IDLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BEGIN_EMPTYING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUILDING_FOR_RENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CARGO_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CARGO_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CARGO_TRANSPORT_ROUTE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CARGO_TRANSPORT_VEHICLE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_AGE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_AGE_FEMALE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_AGE_MALE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_AGE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_CONDITION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_CONDITION_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_CONDITION_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_CONDITION_TITLE_FEMALE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_CONDITION_TITLE_MALE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_DESTINATION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_EDUCATION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_EDUCATION_FEMALE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_EDUCATION_MALE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_EDUCATION_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_HAPPINESS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_HAPPINESS_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_HAPPINESS_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_HAPPINESS_TITLE_FEMALE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_HAPPINESS_TITLE_MALE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_HOUSEHOLD: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_JOB_LEVEL: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_JOB_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_OCCUPATION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_OCCUPATION_FEMALE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_OCCUPATION_MALE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_OCCUPATION_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_RESIDENCE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_SCHOOL_TITLE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_STATE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_STATE_FEMALE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_STATE_MALE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_TYPE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_TYPE_FEMALE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_TYPE_MALE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_WEALTH: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_WEALTH_FEMALE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_WEALTH_MALE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_WEALTH_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZEN_WORKPLACE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CLOSE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            COLOR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            COMPANY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            COMPANY_EXTRACTED: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            COMPANY_PRODUCES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            COMPANY_PROFITABILITY_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            COMPANY_REQUIRES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            COMPANY_SELLS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            COMPANY_STORES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CONFIRM_DELETE_WARNING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CREATE_DISTRICT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEATHCARE_BODIES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEATHCARE_PROCESSING_SPEED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEATHCARE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEATHCARE_VEHICLES: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEATHCARE_VEHICLE_DEAD: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEATHCARE_VEHICLE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DELETE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DELIVERY_VEHICLE_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DESTINATION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DESTROYED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DESTROYED_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DESTROYED_STATUS: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DESTROYED_TREE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DESTROYED_TREE_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DETAILS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DEVELOPER_INFO_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DISPATCHED_VEHICLES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DRIVER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DURATION_GREATER_YEAR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EDUCATED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EDUCATION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EDUCATION_DROPOUT_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EDUCATION_GRADUATION_TIME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EDUCATION_LEVELS: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EDUCATION_NO_GRADUATIONS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EDUCATION_STUDENTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EFFICIENCY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EFFICIENCY_FACTORS: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_BATTERY_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_BATTERY_CHARGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_BATTERY_FLOW: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_BATTERY_STATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_POWER_PRODUCTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_POWER_USAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_TRANSFORMER_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EMPLOYEES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EMPLOYEE_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"AMOUNT", import("common/localization/loc-component").LocReactNode>>;
            EMPTY_IN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"DURATION", import("common/localization/loc-component").LocReactNode>>;
            EXPENSES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FIRE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FIRE_VEHICLES: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FIRE_VEHICLE_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FOCUS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FOLLOW_CITIZEN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            FULL_IN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"DURATION", import("common/localization/loc-component").LocReactNode>>;
            GARBAGE_MANAGEMENT_PROCESSING_SPEED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GARBAGE_MANAGEMENT_STORAGE_LABEL: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GARBAGE_MANAGEMENT_STORED_GARBAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GARBAGE_MANAGEMENT_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GARBAGE_MANAGEMENT_VEHICLES: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GARBAGE_VEHICLE_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GRAVES_IN_USE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GROUND_POLLUTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HEALTHCARE_PATIENTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HEALTHCARE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HEALTHCARE_VEHICLES: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HEALTHCARE_VEHICLE_PATIENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HEALTHCARE_VEHICLE_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HIGHTLY_EDUCATED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HOUSEHOLDS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HOUSEHOLD_MEMBERS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEVEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEVEL_MAX: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEVEL_PROGRESS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LINE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LINES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LINE_USAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LINE_VISUALIZER_LENGTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LINE_VISUALIZER_STOPS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LINE_VISUALIZER_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LINE_VISUALIZER_VEHICLES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOAD_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOCAL_SERVICES_NONE_ASSIGNED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOCAL_SERVICES_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LOT_TOOL_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAIL_PROCESSING_SPEED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAIL_SORTING_SPEED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAIL_STORED_MAIL: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAIL_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAIL_VEHICLES: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAINTENANCED_VEHICLES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAINTENANCE_VEHICLE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAX_FOLLOWED_CITIZENS_REACHED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NEXT_STOP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NOISE_POLLUTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NOTIFICATIONS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NO_ACTIVE_POLICIES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OPEN_POSITIONS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OPERATING_DISTRICTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ORIGIN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OUTSIDE_CONNECTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OUT_OF_SERVICE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            OWNED_BY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PARKING_FEE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PARKING_PARKED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PARKING_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PARK_MAINTENANCE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PARK_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PASSENGERS: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PASSENGERS_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PASSENGERS_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POLICE_PRISONERS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POLICE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POLICE_VEHICLES: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POLICE_VEHICLE_CRIMINAL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POLICE_VEHICLE_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POLICIES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POLLUTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POLLUTION_LEVELS: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POLLUTION_LEVELS_AIR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POLLUTION_LEVELS_GROUND: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POLLUTION_LEVELS_NOISE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POORLY_EDUCATED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POST_VEHICLE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRISON_PRISONERS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRISON_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRISON_VEHICLES: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRIVATE_VEHICLE_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PRODUCTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PUBLIC_TRANSPORT_LINE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PUBLIC_TRANSPORT_VEHICLE_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            REBUILD: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RELOCATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            REMOVE_OPERATING_DISTRICT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESIDENTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESOURCE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ROAD_CONDITION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ROAD_CONDITION_FORMAT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"WORST" | "BEST" | "AVG", import("common/localization/loc-component").LocReactNode>>;
            ROAD_LENGTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ROAD_TRAFFIC_FLOW: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ROAD_TRAFFIC_VOLUME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ROAD_UPKEEP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ROUTE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SCHEDULE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SCHEDULE_CONTINUOUS_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SCHEDULE_DAY_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SCHEDULE_NIGHT_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SELECT_OPERATING_DISTRICT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SELECT_VEHICLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SELECT_VEHICLE_SECONDARY: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SEWAGE_OUTLET_USAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SEWAGE_PROCESSING_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SEWAGE_PURIFICATION_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SEWAGE_STORAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SEWAGE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SHELTER_OCCUPANTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SHELTER_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SHELTER_VEHICLES: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STOP_EMPTYING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TICKET_PRICE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TICKET_PRICE_FREE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TRAFFIC_ACCIDENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNDER_CONSTRUCTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNEDUCATED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNFOCUS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNFOLLOW_CITIZEN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UPGRADE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UPGRADES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UPGRADE_ALREADY_BUILT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UPGRADE_LOCKED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UPGRADE_LOCKED_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UPGRADE_NO_MONEY_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UPGRADE_PURCHASED_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UPGRADE_TYPE_LABEL: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UPKEEP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UPKEEPFORMAT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"RESOURCE" | "AMOUNT", import("common/localization/loc-component").LocReactNode>>;
            UPKEEP_MAINTENANCE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"BUILDING", import("common/localization/loc-component").LocReactNode>>;
            UPKEEP_MAINTENANCE_INACTIVE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"BUILDING", import("common/localization/loc-component").LocReactNode>>;
            UPKEEP_TOTAL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            VEHICLES: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            VEHICLE_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            VEHICLE_STATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            VEHICLE_STATES: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WAGES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WAITING_PASSENGERS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WAREHOUSE_STORAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_OUTPUT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_PUMPING_CAPACITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_PUMP_POLLUTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_PUMP_USAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_STORAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_STORAGE_CHANGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"TIME" | "CHANGE", import("common/localization/loc-component").LocReactNode>>;
            WATER_STORAGE_FULL_IN: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_STORAGE_LASTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WELL_EDUCATED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WORKPLACES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WORKPLACE_COUNT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"AMOUNT", import("common/localization/loc-component").LocReactNode>>;
            WORK_SHIFT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Services: {
            DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NAME: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Statistics: {
            PANEL_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STATISTIC: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        StatisticsPanel: {
            STAT_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TIME_SCALE: import("common/localization/loc-component").LocComponent<{
                index: number;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TIME_SCALE_NOW: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TIME_SCALE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        SubServices: {
            NAME: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        ToolOptions: {
            COLOR_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            COLOR_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DECREASE_BRUSH_SIZE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DECREASE_BRUSH_STRENGTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DECREASE_ELEVATION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_ELEVATION_STEP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_INCREASE_BRUSH_SIZE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_INCREASE_BRUSH_STRENGTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_INCREASE_ELEVATION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_SNAP_ALL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Toolbar: {
            ASSET_ALREADY_BUILT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BRUSH_SIZE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BRUSH_STRENGTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CALENDAR_PANEL_TOGGLE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CHIRPER_PANEL_TOGGLE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_INFO_PANEL_TOGGLE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITY_NAME_FIELD_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CURRENT_TREND: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DECREASE_PARALLEL_OFFSET: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ECONOMY_PANEL_TOGGLE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELEVATION_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INCREASE_PARALLEL_OFFSET: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            JOURNAL_PANEL_TOGGLE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LIFEPATH_PANEL_TOGGLE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MAP_TILE_PURCHASE_TOGGLE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MONEY_FIELD_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PARALLEL_MODE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PARALLEL_OFFSET_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PARALLEL_ROAD_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PAUSE_BUTTON_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PHOTO_MODE_TOGGLE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PLAY_BUTTON_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POPULATION_FIELD_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PROGRESSION_PANEL_TOGGLE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SIMULATION_PAUSED: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SNAPPING_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SPEED_BUTTON_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STATISTICS_PANEL_TOGGLE_TOOLTIP: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            THEME_PANEL_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOGGLE_PARALLEL_MODE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOGGLE_UNDERGROUND_MODE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOL_MODE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOL_OPTIONS_PANEL_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            UNDERGROUND_MODE_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            XP_MESSAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"XP" | "REASON", import("common/localization/loc-component").LocReactNode>>;
            XP_PROGRESS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"CURRENT" | "NEXT", import("common/localization/loc-component").LocReactNode>>;
        };
        Tools: {
            BATTERY_CHARGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BATTERY_FLOW: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CONFIRM_BULLDOZE_WARNING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CONSTRUCTION_COST_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_CONSUMPTION_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_FLOW_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ELECTRICITY_PRODUCTION_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EXTRACTOR_CLIMATE_REQUIRED_AVAILABLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EXTRACTOR_CLIMATE_REQUIRED_UNAVAILABLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EXTRACTOR_MAP_FEATURE_REQUIRED_AVAILABLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EXTRACTOR_MAP_FEATURE_REQUIRED_MISSING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EXTRACTOR_PRODUCTION_DEFICIT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            EXTRACTOR_PRODUCTION_SURPLUS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GROUNDWATER_VOLUME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            GROUND_WATER_RESERVOIR_USAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INFO: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            MOVING_OBJECT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<"OBJECT", import("common/localization/loc-component").LocReactNode>>;
            REFUND_AMOUNT_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            RESOURCES_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SELECTING_DISTRICTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SEWAGE_CONSUMPTION_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SEWAGE_FLOW_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STORAGECAPACITY_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOL_MODE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WARNING: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_CONSUMPTION_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_FLOW_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_OUTPUT_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_REFRESH_RATE_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        TourismInfoPanel: {
            ATTRACTIVENESS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            AVERAGE_HOTEL_PRICE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOURISM_RATE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WEATHER_EFFECT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Transport: {
            LEGEND_ACTIVE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEGEND_ACTIVITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEGEND_CARGO: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEGEND_COLOR: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEGEND_DELETE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEGEND_FOCUS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEGEND_LENGTH: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEGEND_NAME: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEGEND_PASSENGERS: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEGEND_SHOW: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEGEND_STOPS: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEGEND_USAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEGEND_VEHICLES: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LEGEND_VISIBLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LINES: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LINES_OVERVIEW_BUTTON: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LINES_OVERVIEW_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NO_LINES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            NO_ROUTES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ROUTES: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TAB: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_ACTIVE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_COLOR: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DELETE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_DETAILS: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_SCHEDULE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOOLTIP_VISIBLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        TransportInfoPanel: {
            CARGO_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CARGO_TRANSPORT_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            CITIZENS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LINES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PUBLIC_TRANSPORT_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ROUTES_LABEL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SUMMARY_TOOLTIP_CARGO: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SUMMARY_TOOLTIP_PUBLICTRANSPORT: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOTAL: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TOURISTS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        Tutorials: {
            ADVISOR_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BEFORE_STARTING: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BEFORE_STARTING_CONFIRM: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BEGIN_WITH_TUTORIALS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUTTON_TOOLTIP_DONE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUTTON_TOOLTIP_NEXT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BUTTON_TOOLTIP_PREVIOUS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DESCRIPTION_GAMEPAD: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DESCRIPTION_KEYBOARD: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DISABLE_CONFIRMATION_TEXT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            DISABLE_CONFIRMATION_TEXT_GAMEPAD: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            ENABLE_TUTORIALS_CONFIRM: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            HINTS_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INTRO: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INTRO_SUBTITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            INTRO_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LIST_REMINDER_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            LIST_REMINDER_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TASKS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TODO_INTRO: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TODO_INTRO_CONFIRM: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TODO_OUTRO: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TODO_OUTRO_CONTINUE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TODO_OUTRO_GAMEPAD: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TODO_OUTRO_NEW_CITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            TRIGGER: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        UpgradesMenu: {
            TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        VirtualKeyboard: {
            TITLE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        WaterInfoPanel: {
            SEWAGE_TRADE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            SEWAGE_TREATMENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            STORMWATER_TREATMENT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_AVAILABILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_EXPORT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_IMPORT: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_STORAGE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WATER_TRADE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        WaterPollutionInfoPanel: {
            AVERAGE_WATER_POLLUTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        WhatNew: {
            BEACH_PROPERTIES_P1_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BEACH_PROPERTIES_P2_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BEACH_PROPERTIES_P3_DESCRIPTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        WhatsNew: {
            BEACH_PROPERTIES_P1_HEADER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BEACH_PROPERTIES_P1_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BEACH_PROPERTIES_P2_HEADER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BEACH_PROPERTIES_P2_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BEACH_PROPERTIES_P3_HEADER: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            BEACH_PROPERTIES_P3_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WHATS_NEW: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        WorkplacesInfoPanel: {
            AVAILABILITY: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WORKERS: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WORKPLACES: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            WORKPLACE_DISTRIBUTION: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
        ZoningFactors: {
            NEGATIVE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            PANEL_TITLE: import("common/localization/loc-component").LocComponent<{
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
            POSITIVE: import("common/localization/loc-component").LocComponent<{
                hash: string;
            } & {
                fallback?: string | null | undefined;
            } & Record<never, import("common/localization/loc-component").LocReactNode>>;
        };
    };
}
declare module "common/localization/localized-entity-name" {
    import React from 'react';
    import { Typed } from "common/data-binding/binding-utils";
    enum NameType {
        Custom = "names.CustomName",
        Localized = "names.LocalizedName",
        Formatted = "names.FormattedName"
    }
    export type Name = CustomName | LocalizedName | FormattedName;
    interface CustomName extends Typed<NameType.Custom> {
        name: string;
    }
    interface LocalizedName extends Typed<NameType.Localized> {
        nameId: string;
    }
    interface FormattedName extends Typed<NameType.Formatted> {
        nameId: string;
        nameArgs: {
            [key: string]: string;
        };
    }
    interface LocalizedNameProps {
        value: Name;
    }
    export const LocalizedEntityName: React.FC<LocalizedNameProps>;
    export function useLocalizedName(data: Name | null): string;
    export interface NameFormat {
        (data: Name | null): string;
    }
    export function useNameFormat(): (data: Name | null) => string;
    export function nameEquals(name: Name, otherName: Name): boolean;
}
declare module "common/utils/arrays" {
    export function firstOrNull<T>(array: T[] | null | undefined): T | null;
    export function firstPropertyOrNull<T, K extends keyof T>(array: (T | null)[] | null | undefined, propertyName: K): T[K] | null;
    export function getNextItem<T>(array: T[], currentIndex: number, loop: boolean): T | null;
    export function getNextIndex(current: number, length: number, loop: boolean): number | null;
    export function getPreviousItem<T>(array: T[], currentIndex: number, loop: boolean): T | null;
    export function getPreviousIndex(current: number, length: number, loop: boolean): number | null;
    export const mapRange: (callbackFn: (index: number) => JSX.Element, count: number, startIndex?: number) => JSX.Element[];
    export function containsArray<T>(arrays: T[][], array: T[]): boolean;
    export function notNull<T>(value: T | null): value is T;
}
declare module "game/components/selected-info-panel/shared-components/notification/notification-data" {
    export interface NotificationData {
        key: string;
        count: number;
        iconPath: string;
    }
}
declare module "game/data-binding/policy-bindings" {
    import { Entity } from "common/data-binding/common-types";
    import { Unit } from "common/localization/unit";
    import { Bounds1 } from "common/math";
    import { UnlockingRequirements } from "game/data-binding/prefab/prefab-bindings";
    export const cityPolicies$: import("common/data-binding/binding").ValueBinding<PolicyData[]>;
    export function setCityPolicy(entity: Entity, active: boolean, value?: number): void;
    export function setPolicy(entity: Entity, active: boolean, value?: number): void;
    export interface PolicyData {
        id: string;
        icon: string;
        entity: Entity;
        active: boolean;
        locked: boolean;
        uiTag: string;
        requirements: UnlockingRequirements;
        data: PolicySliderData | null;
    }
    export interface PolicySliderData {
        value: number;
        range: Bounds1;
        default: number;
        step: number;
        unit: Unit;
    }
    export interface PolicySliderProp {
        sliderData: PolicySliderData;
    }
}
declare module "game/data-binding/selected-info-bindings" {
    import { Color } from "common/color";
    import { Typed, TypeFromMap } from "common/data-binding/binding-utils";
    import { Entity } from "common/data-binding/common-types";
    import { FocusKey } from "common/focus/focus-key";
    import { Name } from "common/localization/localized-entity-name";
    import { Number2 } from "common/math";
    import { PrefabEffect } from "game/data-binding/prefab/prefab-effects";
    import { NotificationData } from "game/components/selected-info-panel/shared-components/notification/notification-data";
    import { Factor } from "game/data-binding/city-info-bindings";
    import { ChartData } from "game/data-binding/infoview-bindings";
    import { PolicyData, PolicySliderData } from "game/data-binding/policy-bindings";
    import { PrefabRequirement } from "game/data-binding/prefab/prefab-requirements";
    export const selectedEntity$: import("common/data-binding/binding").ValueBinding<Entity>;
    export const selectedUITag$: import("common/data-binding/binding").ValueBinding<string>;
    export const activeSelection$: import("common/data-binding/binding").ValueBinding<boolean>;
    export const selectedInfoPosition$: import("common/data-binding/binding").ValueBinding<Number2>;
    export const topSections$: import("common/data-binding/binding").ValueBinding<((ResourceSection & Typed<SectionType.Resource>) | (LocalServicesSection & Typed<SectionType.LocalServices>) | (ActionsSection & Typed<SectionType.Actions>) | (DescriptionSection & Typed<SectionType.Description>) | (DeveloperSection & Typed<SectionType.Developer>) | (ResidentsSection & Typed<SectionType.Residents>) | (HouseholdSidebarSection & Typed<SectionType.HouseholdSidebar>) | (DistrictsSection & Typed<SectionType.Districts>) | (TitleSection & Typed<SectionType.Title>) | (NotificationsSection & Typed<SectionType.Notifications>) | (PoliciesSection & Typed<SectionType.Policies>) | (ProfitabilitySection & Typed<SectionType.Profitability>) | (AverageHappinessSection & Typed<SectionType.AverageHappiness>) | (ScheduleSection & Typed<SectionType.Schedule>) | (LineSection & Typed<SectionType.Line>) | (LinesSection & Typed<SectionType.Lines>) | (ColorSection & Typed<SectionType.Color>) | (LineVisualizerSection & Typed<SectionType.LineVisualizer>) | (TicketPriceSection & Typed<SectionType.TicketPrice>) | (VehicleCountSection & Typed<SectionType.VehicleCount>) | (AttractivenessSection & Typed<SectionType.Attractiveness>) | (EfficiencySection & Typed<SectionType.Efficiency>) | (EmployeesSection & Typed<SectionType.Employees>) | (UpkeepSection & Typed<SectionType.Upkeep>) | (LevelSection & Typed<SectionType.Level>) | (EducationSection & Typed<SectionType.Education>) | (PollutionSection & Typed<SectionType.Pollution>) | (HealthcareSection & Typed<SectionType.Healthcare>) | (DeathcareSection & Typed<SectionType.Deathcare>) | (GarbageSection & Typed<SectionType.Garbage>) | (PoliceSection & Typed<SectionType.Police>) | (VehiclesSection & Typed<SectionType.Vehicles>) | (DispatchedVehiclesSection & Typed<SectionType.DispatchedVehicles>) | (ElectricitySection & Typed<SectionType.Electricity>) | (TransformerSection & Typed<SectionType.Transformer>) | (BatterySection & Typed<SectionType.Battery>) | (WaterSection & Typed<SectionType.Water>) | (SewageSection & Typed<SectionType.Sewage>) | (FireSection & Typed<SectionType.Fire>) | (PrisonSection & Typed<SectionType.Prison>) | (ShelterSection & Typed<SectionType.Shelter>) | (ParkingSection & Typed<SectionType.Parking>) | (ParkSection & Typed<SectionType.Park>) | (MailSection & Typed<SectionType.Mail>) | (RoadSection & Typed<SectionType.Road>) | (CompanySection & Typed<SectionType.Company>) | (StorageSection & Typed<SectionType.Storage>) | (PrivateVehicleSection & Typed<SectionType.PrivateVehicle>) | (PublicTransportVehicleSection & Typed<SectionType.PublicTransportVehicle>) | (CargoTransportVehicleSection & Typed<SectionType.CargoTransportVehicle>) | (DeliveryVehicleSection & Typed<SectionType.DeliveryVehicle>) | (HealthcareVehicleSection & Typed<SectionType.HealthcareVehicle>) | (FireVehicleSection & Typed<SectionType.FireVehicle>) | (PoliceVehicleSection & Typed<SectionType.PoliceVehicle>) | (MaintenanceVehicleSection & Typed<SectionType.MaintenanceVehicle>) | (DeathcareVehicleSection & Typed<SectionType.DeathcareVehicle>) | (PostVehicleSection & Typed<SectionType.PostVehicle>) | (GarbageVehicleSection & Typed<SectionType.GarbageVehicle>) | (PassengersSection & Typed<SectionType.Passengers>) | (CargoSection & Typed<SectionType.Cargo>) | (StatusSection & Typed<SectionType.Status>) | (CitizenSection & Typed<SectionType.Citizen>) | (SelectVehiclesSection & Typed<SectionType.SelectVehicles>) | (DestroyedBuildingSection & Typed<SectionType.DestroyedBuilding>) | (DestroyedTreeSection & Typed<SectionType.DestroyedTree>) | (ComfortSection & Typed<SectionType.Comfort>) | null)[]>;
    export const middleSections$: import("common/data-binding/binding").ValueBinding<((ResourceSection & Typed<SectionType.Resource>) | (LocalServicesSection & Typed<SectionType.LocalServices>) | (ActionsSection & Typed<SectionType.Actions>) | (DescriptionSection & Typed<SectionType.Description>) | (DeveloperSection & Typed<SectionType.Developer>) | (ResidentsSection & Typed<SectionType.Residents>) | (HouseholdSidebarSection & Typed<SectionType.HouseholdSidebar>) | (DistrictsSection & Typed<SectionType.Districts>) | (TitleSection & Typed<SectionType.Title>) | (NotificationsSection & Typed<SectionType.Notifications>) | (PoliciesSection & Typed<SectionType.Policies>) | (ProfitabilitySection & Typed<SectionType.Profitability>) | (AverageHappinessSection & Typed<SectionType.AverageHappiness>) | (ScheduleSection & Typed<SectionType.Schedule>) | (LineSection & Typed<SectionType.Line>) | (LinesSection & Typed<SectionType.Lines>) | (ColorSection & Typed<SectionType.Color>) | (LineVisualizerSection & Typed<SectionType.LineVisualizer>) | (TicketPriceSection & Typed<SectionType.TicketPrice>) | (VehicleCountSection & Typed<SectionType.VehicleCount>) | (AttractivenessSection & Typed<SectionType.Attractiveness>) | (EfficiencySection & Typed<SectionType.Efficiency>) | (EmployeesSection & Typed<SectionType.Employees>) | (UpkeepSection & Typed<SectionType.Upkeep>) | (LevelSection & Typed<SectionType.Level>) | (EducationSection & Typed<SectionType.Education>) | (PollutionSection & Typed<SectionType.Pollution>) | (HealthcareSection & Typed<SectionType.Healthcare>) | (DeathcareSection & Typed<SectionType.Deathcare>) | (GarbageSection & Typed<SectionType.Garbage>) | (PoliceSection & Typed<SectionType.Police>) | (VehiclesSection & Typed<SectionType.Vehicles>) | (DispatchedVehiclesSection & Typed<SectionType.DispatchedVehicles>) | (ElectricitySection & Typed<SectionType.Electricity>) | (TransformerSection & Typed<SectionType.Transformer>) | (BatterySection & Typed<SectionType.Battery>) | (WaterSection & Typed<SectionType.Water>) | (SewageSection & Typed<SectionType.Sewage>) | (FireSection & Typed<SectionType.Fire>) | (PrisonSection & Typed<SectionType.Prison>) | (ShelterSection & Typed<SectionType.Shelter>) | (ParkingSection & Typed<SectionType.Parking>) | (ParkSection & Typed<SectionType.Park>) | (MailSection & Typed<SectionType.Mail>) | (RoadSection & Typed<SectionType.Road>) | (CompanySection & Typed<SectionType.Company>) | (StorageSection & Typed<SectionType.Storage>) | (PrivateVehicleSection & Typed<SectionType.PrivateVehicle>) | (PublicTransportVehicleSection & Typed<SectionType.PublicTransportVehicle>) | (CargoTransportVehicleSection & Typed<SectionType.CargoTransportVehicle>) | (DeliveryVehicleSection & Typed<SectionType.DeliveryVehicle>) | (HealthcareVehicleSection & Typed<SectionType.HealthcareVehicle>) | (FireVehicleSection & Typed<SectionType.FireVehicle>) | (PoliceVehicleSection & Typed<SectionType.PoliceVehicle>) | (MaintenanceVehicleSection & Typed<SectionType.MaintenanceVehicle>) | (DeathcareVehicleSection & Typed<SectionType.DeathcareVehicle>) | (PostVehicleSection & Typed<SectionType.PostVehicle>) | (GarbageVehicleSection & Typed<SectionType.GarbageVehicle>) | (PassengersSection & Typed<SectionType.Passengers>) | (CargoSection & Typed<SectionType.Cargo>) | (StatusSection & Typed<SectionType.Status>) | (CitizenSection & Typed<SectionType.Citizen>) | (SelectVehiclesSection & Typed<SectionType.SelectVehicles>) | (DestroyedBuildingSection & Typed<SectionType.DestroyedBuilding>) | (DestroyedTreeSection & Typed<SectionType.DestroyedTree>) | (ComfortSection & Typed<SectionType.Comfort>) | null)[]>;
    export const bottomSections$: import("common/data-binding/binding").ValueBinding<((ResourceSection & Typed<SectionType.Resource>) | (LocalServicesSection & Typed<SectionType.LocalServices>) | (ActionsSection & Typed<SectionType.Actions>) | (DescriptionSection & Typed<SectionType.Description>) | (DeveloperSection & Typed<SectionType.Developer>) | (ResidentsSection & Typed<SectionType.Residents>) | (HouseholdSidebarSection & Typed<SectionType.HouseholdSidebar>) | (DistrictsSection & Typed<SectionType.Districts>) | (TitleSection & Typed<SectionType.Title>) | (NotificationsSection & Typed<SectionType.Notifications>) | (PoliciesSection & Typed<SectionType.Policies>) | (ProfitabilitySection & Typed<SectionType.Profitability>) | (AverageHappinessSection & Typed<SectionType.AverageHappiness>) | (ScheduleSection & Typed<SectionType.Schedule>) | (LineSection & Typed<SectionType.Line>) | (LinesSection & Typed<SectionType.Lines>) | (ColorSection & Typed<SectionType.Color>) | (LineVisualizerSection & Typed<SectionType.LineVisualizer>) | (TicketPriceSection & Typed<SectionType.TicketPrice>) | (VehicleCountSection & Typed<SectionType.VehicleCount>) | (AttractivenessSection & Typed<SectionType.Attractiveness>) | (EfficiencySection & Typed<SectionType.Efficiency>) | (EmployeesSection & Typed<SectionType.Employees>) | (UpkeepSection & Typed<SectionType.Upkeep>) | (LevelSection & Typed<SectionType.Level>) | (EducationSection & Typed<SectionType.Education>) | (PollutionSection & Typed<SectionType.Pollution>) | (HealthcareSection & Typed<SectionType.Healthcare>) | (DeathcareSection & Typed<SectionType.Deathcare>) | (GarbageSection & Typed<SectionType.Garbage>) | (PoliceSection & Typed<SectionType.Police>) | (VehiclesSection & Typed<SectionType.Vehicles>) | (DispatchedVehiclesSection & Typed<SectionType.DispatchedVehicles>) | (ElectricitySection & Typed<SectionType.Electricity>) | (TransformerSection & Typed<SectionType.Transformer>) | (BatterySection & Typed<SectionType.Battery>) | (WaterSection & Typed<SectionType.Water>) | (SewageSection & Typed<SectionType.Sewage>) | (FireSection & Typed<SectionType.Fire>) | (PrisonSection & Typed<SectionType.Prison>) | (ShelterSection & Typed<SectionType.Shelter>) | (ParkingSection & Typed<SectionType.Parking>) | (ParkSection & Typed<SectionType.Park>) | (MailSection & Typed<SectionType.Mail>) | (RoadSection & Typed<SectionType.Road>) | (CompanySection & Typed<SectionType.Company>) | (StorageSection & Typed<SectionType.Storage>) | (PrivateVehicleSection & Typed<SectionType.PrivateVehicle>) | (PublicTransportVehicleSection & Typed<SectionType.PublicTransportVehicle>) | (CargoTransportVehicleSection & Typed<SectionType.CargoTransportVehicle>) | (DeliveryVehicleSection & Typed<SectionType.DeliveryVehicle>) | (HealthcareVehicleSection & Typed<SectionType.HealthcareVehicle>) | (FireVehicleSection & Typed<SectionType.FireVehicle>) | (PoliceVehicleSection & Typed<SectionType.PoliceVehicle>) | (MaintenanceVehicleSection & Typed<SectionType.MaintenanceVehicle>) | (DeathcareVehicleSection & Typed<SectionType.DeathcareVehicle>) | (PostVehicleSection & Typed<SectionType.PostVehicle>) | (GarbageVehicleSection & Typed<SectionType.GarbageVehicle>) | (PassengersSection & Typed<SectionType.Passengers>) | (CargoSection & Typed<SectionType.Cargo>) | (StatusSection & Typed<SectionType.Status>) | (CitizenSection & Typed<SectionType.Citizen>) | (SelectVehiclesSection & Typed<SectionType.SelectVehicles>) | (DestroyedBuildingSection & Typed<SectionType.DestroyedBuilding>) | (DestroyedTreeSection & Typed<SectionType.DestroyedTree>) | (ComfortSection & Typed<SectionType.Comfort>) | null)[]>;
    export const titleSection$: import("common/data-binding/binding").ValueBinding<TitleSection | null>;
    export const developerSection$: import("common/data-binding/binding").ValueBinding<DeveloperSection | null>;
    export const lineVisualizerSection$: import("common/data-binding/binding").ValueBinding<LineVisualizerSection | null>;
    export const householdSidebarSection$: import("common/data-binding/binding").ValueBinding<HouseholdSidebarSection | null>;
    export const tooltipTags$: import("common/data-binding/binding").ValueBinding<string[]>;
    export const selectedRoute$: import("common/data-binding/binding").ValueBinding<Entity>;
    export const selectEntity: (entity: Entity) => void;
    export const setSelectedRoute: (entity: Entity) => void;
    export const clearSelection: () => void;
    export enum SectionType {
        Resource = "Game.UI.InGame.ResourceSection",
        LocalServices = "Game.UI.InGame.LocalServicesSection",
        Actions = "Game.UI.InGame.ActionsSection",
        Description = "Game.UI.InGame.DescriptionSection",
        Developer = "Game.UI.InGame.DeveloperSection",
        Residents = "Game.UI.InGame.ResidentsSection",
        HouseholdSidebar = "Game.UI.InGame.HouseholdSidebarSection",
        Districts = "Game.UI.InGame.DistrictsSection",
        Title = "Game.UI.InGame.TitleSection",
        Notifications = "Game.UI.InGame.NotificationsSection",
        Policies = "Game.UI.InGame.PoliciesSection",
        Profitability = "Game.UI.InGame.ProfitabilitySection",
        AverageHappiness = "Game.UI.InGame.AverageHappinessSection",
        Schedule = "Game.UI.InGame.ScheduleSection",
        Line = "Game.UI.InGame.LineSection",
        Lines = "Game.UI.InGame.LinesSection",
        Color = "Game.UI.InGame.ColorSection",
        LineVisualizer = "Game.UI.InGame.LineVisualizerSection",
        TicketPrice = "Game.UI.InGame.TicketPriceSection",
        VehicleCount = "Game.UI.InGame.VehicleCountSection",
        Attractiveness = "Game.UI.InGame.AttractivenessSection",
        Efficiency = "Game.UI.InGame.EfficiencySection",
        Employees = "Game.UI.InGame.EmployeesSection",
        Upkeep = "Game.UI.InGame.UpkeepSection",
        Level = "Game.UI.InGame.LevelSection",
        Education = "Game.UI.InGame.EducationSection",
        Pollution = "Game.UI.InGame.PollutionSection",
        Healthcare = "Game.UI.InGame.HealthcareSection",
        Deathcare = "Game.UI.InGame.DeathcareSection",
        Garbage = "Game.UI.InGame.GarbageSection",
        Police = "Game.UI.InGame.PoliceSection",
        Vehicles = "Game.UI.InGame.VehiclesSection",
        DispatchedVehicles = "Game.UI.InGame.DispatchedVehiclesSection",
        Electricity = "Game.UI.InGame.ElectricitySection",
        Transformer = "Game.UI.InGame.TransformerSection",
        Battery = "Game.UI.InGame.BatterySection",
        Water = "Game.UI.InGame.WaterSection",
        Sewage = "Game.UI.InGame.SewageSection",
        Fire = "Game.UI.InGame.FireSection",
        Prison = "Game.UI.InGame.PrisonSection",
        Shelter = "Game.UI.InGame.ShelterSection",
        Parking = "Game.UI.InGame.ParkingSection",
        Park = "Game.UI.InGame.ParkSection",
        Mail = "Game.UI.InGame.MailSection",
        Road = "Game.UI.InGame.RoadSection",
        Company = "Game.UI.InGame.CompanySection",
        Storage = "Game.UI.InGame.StorageSection",
        PrivateVehicle = "Game.UI.InGame.PrivateVehicleSection",
        PublicTransportVehicle = "Game.UI.InGame.PublicTransportVehicleSection",
        CargoTransportVehicle = "Game.UI.InGame.CargoTransportVehicleSection",
        DeliveryVehicle = "Game.UI.InGame.DeliveryVehicleSection",
        HealthcareVehicle = "Game.UI.InGame.HealthcareVehicleSection",
        FireVehicle = "Game.UI.InGame.FireVehicleSection",
        PoliceVehicle = "Game.UI.InGame.PoliceVehicleSection",
        MaintenanceVehicle = "Game.UI.InGame.MaintenanceVehicleSection",
        DeathcareVehicle = "Game.UI.InGame.DeathcareVehicleSection",
        PostVehicle = "Game.UI.InGame.PostVehicleSection",
        GarbageVehicle = "Game.UI.InGame.GarbageVehicleSection",
        Passengers = "Game.UI.InGame.PassengersSection",
        Cargo = "Game.UI.InGame.CargoSection",
        Load = "Game.UI.InGame.LoadSection",
        Status = "Game.UI.InGame.StatusSection",
        Citizen = "Game.UI.InGame.CitizenSection",
        DummyHuman = "Game.UI.InGame.DummyHumanSection",
        Animal = "Game.UI.InGame.AnimalSection",
        SelectVehicles = "Game.UI.InGame.SelectVehiclesSection",
        DestroyedBuilding = "Game.UI.InGame.DestroyedBuildingSection",
        DestroyedTree = "Game.UI.InGame.DestroyedTreeSection",
        Comfort = "Game.UI.InGame.ComfortSection"
    }
    export interface SelectedInfoSections {
        [SectionType.Resource]: ResourceSection;
        [SectionType.LocalServices]: LocalServicesSection;
        [SectionType.Actions]: ActionsSection;
        [SectionType.Description]: DescriptionSection;
        [SectionType.Developer]: DeveloperSection;
        [SectionType.Residents]: ResidentsSection;
        [SectionType.HouseholdSidebar]: HouseholdSidebarSection;
        [SectionType.Districts]: DistrictsSection;
        [SectionType.Title]: TitleSection;
        [SectionType.Notifications]: NotificationsSection;
        [SectionType.Policies]: PoliciesSection;
        [SectionType.Profitability]: ProfitabilitySection;
        [SectionType.AverageHappiness]: AverageHappinessSection;
        [SectionType.Schedule]: ScheduleSection;
        [SectionType.Line]: LineSection;
        [SectionType.Lines]: LinesSection;
        [SectionType.Color]: ColorSection;
        [SectionType.LineVisualizer]: LineVisualizerSection;
        [SectionType.TicketPrice]: TicketPriceSection;
        [SectionType.VehicleCount]: VehicleCountSection;
        [SectionType.Attractiveness]: AttractivenessSection;
        [SectionType.Efficiency]: EfficiencySection;
        [SectionType.Employees]: EmployeesSection;
        [SectionType.Upkeep]: UpkeepSection;
        [SectionType.Level]: LevelSection;
        [SectionType.Education]: EducationSection;
        [SectionType.Pollution]: PollutionSection;
        [SectionType.Healthcare]: HealthcareSection;
        [SectionType.Deathcare]: DeathcareSection;
        [SectionType.Garbage]: GarbageSection;
        [SectionType.Police]: PoliceSection;
        [SectionType.Vehicles]: VehiclesSection;
        [SectionType.DispatchedVehicles]: DispatchedVehiclesSection;
        [SectionType.Electricity]: ElectricitySection;
        [SectionType.Transformer]: TransformerSection;
        [SectionType.Battery]: BatterySection;
        [SectionType.Water]: WaterSection;
        [SectionType.Sewage]: SewageSection;
        [SectionType.Fire]: FireSection;
        [SectionType.Prison]: PrisonSection;
        [SectionType.Shelter]: ShelterSection;
        [SectionType.Parking]: ParkingSection;
        [SectionType.Park]: ParkSection;
        [SectionType.Mail]: MailSection;
        [SectionType.Road]: RoadSection;
        [SectionType.Company]: CompanySection;
        [SectionType.Storage]: StorageSection;
        [SectionType.PrivateVehicle]: PrivateVehicleSection;
        [SectionType.PublicTransportVehicle]: PublicTransportVehicleSection;
        [SectionType.CargoTransportVehicle]: CargoTransportVehicleSection;
        [SectionType.DeliveryVehicle]: DeliveryVehicleSection;
        [SectionType.HealthcareVehicle]: HealthcareVehicleSection;
        [SectionType.FireVehicle]: FireVehicleSection;
        [SectionType.PoliceVehicle]: PoliceVehicleSection;
        [SectionType.MaintenanceVehicle]: MaintenanceVehicleSection;
        [SectionType.DeathcareVehicle]: DeathcareVehicleSection;
        [SectionType.PostVehicle]: PostVehicleSection;
        [SectionType.GarbageVehicle]: GarbageVehicleSection;
        [SectionType.Passengers]: PassengersSection;
        [SectionType.Cargo]: CargoSection;
        [SectionType.Load]: LoadSection;
        [SectionType.Status]: StatusSection;
        [SectionType.Citizen]: CitizenSection;
        [SectionType.DummyHuman]: DummyHumanSection;
        [SectionType.Animal]: AnimalSection;
        [SectionType.SelectVehicles]: SelectVehiclesSection;
        [SectionType.DestroyedBuilding]: DestroyedBuildingSection;
        [SectionType.DestroyedTree]: DestroyedTreeSection;
        [SectionType.Comfort]: ComfortSection;
    }
    export type SelectedInfoSection = TypeFromMap<SelectedInfoSections>;
    export interface SelectedInfoSectionBase {
        group: string;
        tooltipKeys: string[];
        tooltipTags: string[];
    }
    export interface ResourceSection extends SelectedInfoSectionBase {
        resourceAmount: number;
        resourceKey: string;
    }
    export interface LocalServicesSection extends SelectedInfoSectionBase {
        localServiceBuildings: LocalServiceBuilding[];
    }
    export interface ActionsSection extends SelectedInfoSectionBase {
        focusing: boolean;
        following: boolean;
        followable: boolean;
        moveable: boolean;
        deletable: boolean;
        disabled: boolean;
        disableable: boolean;
        emptying: boolean;
        emptiable: boolean;
        hasLotTool: boolean;
    }
    export interface DescriptionSection extends SelectedInfoSectionBase {
        localeKey: string;
        effects: PrefabEffect[];
    }
    export interface DeveloperSection extends SelectedInfoSectionBase {
        subsections: DeveloperSubsection[];
    }
    export interface ResidentsSection extends SelectedInfoSectionBase {
        isHousehold: boolean;
        householdCount: number;
        maxHouseholds: number;
        residentCount: number;
        petCount: number;
        wealthKey: string;
        residence: Name;
        residenceEntity: Entity;
        residenceKey: string;
        educationData: ChartData;
        ageData: ChartData;
    }
    export interface HouseholdSidebarSection extends SelectedInfoSectionBase {
        householdSidebarVariant: HouseholdSidebarVariant;
        residence: HouseholdSidebarItem;
        households: HouseholdSidebarItem[];
        householdMembers: HouseholdSidebarItem[];
        householdPets: HouseholdSidebarItem[];
    }
    export interface DistrictsSection extends SelectedInfoSectionBase {
        districtMissing: boolean;
        districts: District[];
    }
    export interface TitleSection extends SelectedInfoSectionBase {
        name: Name | null;
        vkName: Name | null;
        vkLocaleKey: string;
        icon: string | null;
    }
    export interface NotificationsSection extends SelectedInfoSectionBase {
        notifications: NotificationData[];
    }
    export interface PoliciesSection extends SelectedInfoSectionBase {
        policies: PolicyData[];
    }
    export interface ProfitabilitySection extends SelectedInfoSectionBase {
        profitability: NotificationData;
        profitabilityFactors: Factor[];
    }
    export interface AverageHappinessSection extends SelectedInfoSectionBase {
        averageHappiness: NotificationData;
        happinessFactors: Factor[];
    }
    export interface ScheduleSection extends SelectedInfoSectionBase {
        schedule: number;
    }
    export interface LineSection extends SelectedInfoSectionBase {
        length: number;
        stops: number;
        usage: number;
        cargo: number;
    }
    export interface LinesSection extends SelectedInfoSectionBase {
        hasLines: boolean;
        lines: Line[];
        hasPassengers: boolean;
        passengers: number;
    }
    export interface ColorSection extends SelectedInfoSectionBase {
        color: Color;
    }
    export interface LineVisualizerSection extends SelectedInfoSectionBase {
        color: Color;
        stops: LineStop[];
        vehicles: LineVehicle[];
        segments: LineSegment[];
        stopCapacity: number;
    }
    export interface TicketPriceSection extends SelectedInfoSectionBase {
        sliderData: PolicySliderData;
    }
    export interface VehicleCountSection extends SelectedInfoSectionBase {
        vehicleCount: number;
        activeVehicles: number;
        vehicleCounts: Number2[];
    }
    export interface SelectVehiclesSection extends SelectedInfoSectionBase {
        primaryVehicle: VehiclePrefab | null;
        secondaryVehicle: VehiclePrefab | null;
        primaryVehicles: VehiclePrefab[];
        secondaryVehicles: VehiclePrefab[] | null;
    }
    export interface AttractivenessSection extends SelectedInfoSectionBase {
        attractiveness: number;
        baseAttractiveness: number;
        factors: AttractivenessFactor[];
    }
    export interface EfficiencySection extends SelectedInfoSectionBase {
        efficiency: number;
        factors: EfficiencyFactor[];
    }
    export interface EmployeesSection extends SelectedInfoSectionBase {
        employeeCount: number;
        maxEmployees: number;
        educationDataEmployees: ChartData;
        educationDataWorkplaces: ChartData;
    }
    export interface UpkeepSection extends SelectedInfoSectionBase {
        upkeeps: UpkeepItem[];
        wages: number;
        total: number;
        inactive: boolean;
    }
    export interface LevelSection extends SelectedInfoSectionBase {
        level: number;
        maxLevel: number;
        isUnderConstruction: boolean;
        progress: number;
    }
    export interface EducationSection extends SelectedInfoSectionBase {
        studentCount: number;
        studentCapacity: number;
        graduationTime: number;
        failProbability: number;
    }
    export interface PollutionSection extends SelectedInfoSectionBase {
        groundPollutionKey: Pollution;
        airPollutionKey: Pollution;
        noisePollutionKey: Pollution;
    }
    export enum Pollution {
        none = 0,
        low = 1,
        medium = 2,
        high = 3
    }
    export interface HealthcareSection extends SelectedInfoSectionBase {
        patientCount: number;
        patientCapacity: number;
    }
    export interface DeathcareSection extends SelectedInfoSectionBase {
        bodyCount: number;
        bodyCapacity: number;
        processingSpeed: number;
        processingCapacity: number;
    }
    export interface GarbageSection extends SelectedInfoSectionBase {
        garbage: number;
        garbageCapacity: number;
        processingSpeed: number;
        processingCapacity: number;
        loadKey: string;
    }
    export interface PoliceSection extends SelectedInfoSectionBase {
        prisonerCount: number;
        prisonerCapacity: number;
    }
    export interface VehiclesSection extends SelectedInfoSectionBase {
        vehicleKey: string;
        vehicleCount: number;
        availableVehicleCount: number;
        vehicleCapacity: number;
        vehicleList: Vehicle[];
    }
    export interface DispatchedVehiclesSection extends SelectedInfoSectionBase {
        vehicleList: Vehicle[];
    }
    export interface ElectricitySection extends SelectedInfoSectionBase {
        capacity: number;
        production: number;
    }
    export interface TransformerSection extends SelectedInfoSectionBase {
        capacity: number;
        flow: number;
    }
    export interface BatterySection extends SelectedInfoSectionBase {
        batteryCharge: number;
        batteryCapacity: number;
        flow: number;
        remainingTime: number;
    }
    export interface WaterSection extends SelectedInfoSectionBase {
        pollution: number;
        capacity: number;
        lastProduction: number;
    }
    export interface SewageSection extends SelectedInfoSectionBase {
        capacity: number;
        lastProcessed: number;
        lastPurified: number;
        purification: number;
    }
    export interface FireSection extends SelectedInfoSectionBase {
        vehicleEfficiency: number;
        disasterResponder: boolean;
    }
    export interface PrisonSection extends SelectedInfoSectionBase {
        prisonerCount: number;
        prisonerCapacity: number;
    }
    export interface ShelterSection extends SelectedInfoSectionBase {
        sheltered: number;
        shelterCapacity: number;
    }
    export interface ParkingSection extends SelectedInfoSectionBase {
        parkedCars: number;
        parkingCapacity: number;
    }
    export interface ParkSection extends SelectedInfoSectionBase {
        maintenance: number;
    }
    export interface MailSection extends SelectedInfoSectionBase {
        sortingRate: number;
        sortingCapacity: number;
        localAmount: number;
        unsortedAmount: number;
        outgoingAmount: number;
        storedAmount: number;
        storageCapacity: number;
        localKey: string;
        unsortedKey: string;
        type: MailSectionType;
    }
    export enum MailSectionType {
        PostFacility = 0,
        MailBox = 1
    }
    export interface RoadSection extends SelectedInfoSectionBase {
        volumeData: number[];
        flowData: number[];
        length: number;
        bestCondition: number;
        worstCondition: number;
        condition: number;
        upkeep: number;
    }
    export interface CompanySection extends SelectedInfoSectionBase {
        companyName: Name | null;
        input1: string | null;
        input2: string | null;
        output: string | null;
        sells: string | null;
        stores: string | null;
    }
    export interface StorageSection extends SelectedInfoSectionBase {
        stored: number;
        capacity: number;
        resources: Resource[];
    }
    export interface DestroyedBuildingSection extends SelectedInfoSectionBase {
        destroyer: string | null;
        cleared: boolean;
        progress: number;
        status: string;
    }
    export interface DestroyedTreeSection extends SelectedInfoSectionBase {
        destroyer: string | null;
    }
    export interface ComfortSection extends SelectedInfoSectionBase {
        comfort: number;
    }
    interface VehicleSection extends SelectedInfoSectionBase {
        stateKey: string;
        owner: Location;
        fromOutside: boolean;
        nextStop: Location;
    }
    export interface VehicleSectionProps extends SelectedInfoSectionProps {
        stateKey: string;
        owner: Location;
        fromOutside: boolean;
        nextStop: Location;
    }
    interface VehicleWithLineSection extends VehicleSection {
        line: Name | null;
        lineEntity: Entity;
    }
    export interface VehicleWithLineSectionProps extends VehicleSectionProps {
        line: Name | null;
        lineEntity: Entity;
    }
    export interface PrivateVehicleSection extends VehicleSection {
        keeper: Name | null;
        keeperEntity: Entity | null;
        vehicleKey: string;
    }
    export interface PublicTransportVehicleSection extends VehicleWithLineSection {
        vehicleKey: string;
    }
    export interface CargoTransportVehicleSection extends VehicleWithLineSection {
    }
    export interface DeliveryVehicleSection extends VehicleSection {
        resourceKey: string;
        vehicleKey: string;
    }
    export interface HealthcareVehicleSection extends VehicleSection {
        patient: Name | null;
        patientEntity: Entity | null;
        vehicleKey: string;
    }
    export interface FireVehicleSection extends VehicleSection {
        vehicleKey: string;
    }
    export interface PoliceVehicleSection extends VehicleSection {
        criminal: Name | null;
        criminalEntity: Entity | null;
        vehicleKey: string;
    }
    export interface MaintenanceVehicleSection extends VehicleSection {
        workShift: number;
    }
    export interface DeathcareVehicleSection extends VehicleSection {
        dead: Name | null;
        deadEntity: Entity | null;
    }
    export interface PostVehicleSection extends VehicleSection {
    }
    export interface GarbageVehicleSection extends VehicleSection {
        vehicleKey: string;
    }
    export interface PassengersSection extends SelectedInfoSectionBase {
        expanded: boolean;
        passengers: number;
        maxPassengers: number;
        pets: number;
        vehiclePassengerKey: string;
    }
    export interface CargoSection extends SelectedInfoSectionBase {
        expanded: boolean;
        cargo: number;
        capacity: number;
        resources: Resource[];
        cargoKey: string;
    }
    export interface LoadSection extends SelectedInfoSectionBase {
        __Type: 'Game.UI.InGame.LoadSection';
        expanded: boolean;
        load: number;
        capacity: number;
        loadKey: string;
    }
    export interface StatusSection extends SelectedInfoSectionBase {
        conditions: NotificationData[];
        notifications: NotificationData[];
        happiness: NotificationData | null;
    }
    export interface CitizenSection extends SelectedInfoSectionBase {
        citizenKey: string;
        stateKey: string;
        household: Name | null;
        householdEntity: Entity | null;
        residence: Name | null;
        residenceEntity: Entity | null;
        residenceKey: string;
        workplace: Name | null;
        workplaceEntity: Entity | null;
        workplaceKey: string;
        occupationKey: string;
        jobLevelKey: string;
        school: Name | null;
        schoolEntity: Entity | null;
        schoolLevel: number;
        educationKey: string;
        ageKey: string;
        wealthKey: string;
        destination: Name | null;
        destinationEntity: Entity | null;
        destinationKey: string;
    }
    export interface DummyHumanSection extends SelectedInfoSectionBase {
        __Type: 'Game.UI.InGame.DummyHumanSection';
        origin: Name | null;
        originEntity: Entity | null;
        destination: Name | null;
        destinationEntity: Entity | null;
    }
    export interface AnimalSection extends SelectedInfoSectionBase {
        __Type: 'Game.UI.InGame.AnimalSection';
        typeKey: string;
        owner: Name | null;
        ownerEntity: Entity | null;
        destination: Name | null;
        destinationEntity: Entity | null;
    }
    export enum DeveloperSubsectionType {
        GenericInfo = "Game.UI.InGame.GenericInfo",
        CapacityInfo = "Game.UI.InGame.CapacityInfo",
        InfoList = "Game.UI.InGame.InfoList"
    }
    export interface DeveloperSubsections {
        [DeveloperSubsectionType.GenericInfo]: GenericInfo;
        [DeveloperSubsectionType.CapacityInfo]: CapacityInfo;
        [DeveloperSubsectionType.InfoList]: InfoList;
    }
    export type DeveloperSubsection = TypeFromMap<DeveloperSubsections>;
    export interface GenericInfo {
        label: string;
        value: string;
        target: Entity | null;
    }
    export interface CapacityInfo {
        label: string;
        value: number;
        max: number;
    }
    export interface InfoList {
        label: string;
        list: Item[];
    }
    export interface District {
        name: Name;
        entity: Entity;
    }
    export interface UpgradeInfo {
        id: string;
        typeIdKey: string;
        icon: string;
        cost: number;
        locked: boolean;
        forbidden: boolean;
        entity: Entity;
    }
    export interface Item {
        text: string;
        entity: Entity | null;
    }
    export interface Resource {
        key: string;
        amount: number;
    }
    export interface SelectedInfoSectionProps {
        group: string;
        tooltipKeys: string[];
        tooltipTags: string[];
        focusKey?: FocusKey;
    }
    export interface Line {
        name: Name;
        color: Color;
        entity: Entity;
    }
    export type LineItem = LineStop | LineVehicle;
    export const LINE_STOP = "Game.UI.InGame.LineVisualizerSection+LineStop";
    export interface LineStop extends Typed<typeof LINE_STOP> {
        entity: Entity;
        name: Name;
        position: number;
        cargo: number;
        isCargo: boolean;
        isOutsideConnection: boolean;
    }
    export const LINE_VEHICLE = "Game.UI.InGame.LineVisualizerSection+LineVehicle";
    export interface LineVehicle extends Typed<typeof LINE_VEHICLE> {
        entity: Entity;
        name: Name;
        position: number;
        cargo: number;
        capacity: number;
        isCargo: boolean;
    }
    export interface LineSegment {
        start: number;
        end: number;
        broken: boolean;
    }
    export interface Vehicle {
        entity: Entity;
        name: Name;
        stateKey: string;
        vehicleKey: string;
    }
    export interface VehiclePrefab {
        entity: Entity;
        id: string;
        locked: boolean;
        requirements: PrefabRequirement[];
        thumbnail: string;
    }
    export interface LocalServiceBuilding {
        name: Name;
        serviceIcon: string | null;
        entity: Entity;
    }
    export interface EfficiencyFactor {
        factor: string;
        value: number;
        result: number;
    }
    export interface AttractivenessFactor {
        localeKey: string;
        delta: number;
    }
    export interface UpkeepItem {
        amount: number;
        price: number;
        prefabNameID: string;
        localeKey: string;
    }
    export interface Location {
        entity: Entity;
        name: Name;
    }
    export interface HouseholdSidebarItem {
        entity: Entity;
        name: Name;
        iconPath: string;
        selected: boolean;
        memberCount: number | null;
    }
    export enum HouseholdSidebarVariant {
        Citizen = "Citizen",
        Household = "Household",
        Building = "Building"
    }
    export function useTooltipParagraph(tooltipIdHashKey: string): string | null;
    export function useTooltipParagraphs(tooltipIdHashKeys: (string | null)[]): string[] | null;
    export function useGeneratedTooltipParagraphs(group: string, tooltipKeys: string[], tooltipTags: string[], hideGroupParagraph?: boolean): string[] | null;
}
declare module "api/index" {
    import { bindValue, call, trigger, useValue } from "common/data-binding/binding";
    import * as camera from "game/data-binding/camera-bindings";
    import * as cityInfo from "game/data-binding/city-info-bindings";
    import * as infoview from "game/data-binding/infoview-bindings";
    import * as selectedInfo from "game/data-binding/selected-info-bindings";
    export const api: {
        bindValue: typeof bindValue;
        trigger: typeof trigger;
        call: typeof call;
        useValue: typeof useValue;
    };
    export const bindings: {
        camera: typeof camera;
        cityInfo: typeof cityInfo;
        infoview: typeof infoview;
        selectedInfo: typeof selectedInfo;
    };
    export const types: {};
}
declare module "common/hooks/resize-events" {
    import { Number2 } from "common/math";
    export type SizeGetter = (element: HTMLElement) => Number2;
    export function OFFSET_SIZE(element: HTMLElement): {
        x: number;
        y: number;
    };
    export function CLIENT_SIZE(element: HTMLElement): {
        x: number;
        y: number;
    };
    export function SCROLL_SIZE(element: HTMLElement): {
        x: number;
        y: number;
    };
    export function useElementSize(ref: React.RefObject<HTMLElement>, sizeGetter?: SizeGetter, enabled?: boolean): Number2 | null;
    export function useElementRect(ref: React.RefObject<HTMLElement | SVGElement>, enabled?: boolean): DOMRect | null;
    export function useResizeEventListener(listener: () => void, enabled?: boolean): void;
}
declare module "common/hooks/use-window-size" {
    import { Number2 } from "common/math";
    function useWindowSize(enabled?: boolean): Number2;
    export default useWindowSize;
}
declare module "common/hooks/use-mouse-drag-events" {
    import React from 'react';
    export interface MouseDragEvent {
        clientX: number;
        clientY: number;
        currentTarget: Element | undefined;
    }
    /**
     * Hook for listening to a drag event in a component, e.g. in a slider or a color wheel.
     * @param handleDragStart  Called when the `mousedown` event is triggered on the thumb.
     * Return `true` if the hook should start listening to `mousemove`/`mouseup` events.
     * @param handleDrag Called when the mouse is moved while the left mouse button is pressed.
     * @param handleDragEnd Called when the left mouse button is release.d
     * @returns [drag state boolean, mousedown event handler for the thumb]
     */
    export function useMouseDragEvents(handleDragStart: (e: MouseDragEvent) => boolean, handleDrag: (e: MouseDragEvent) => void, handleDragEnd: (e: MouseDragEvent) => void): [boolean, (e: React.MouseEvent) => void];
}
declare module "common/input/drag-handle" {
    import React from 'react';
    interface DragHandleProps {
        onDrag: (x: number, y: number, xOffset: number, yOffset: number) => void;
    }
    export const DragHandle: ({ onDrag, children }: React.PropsWithChildren<DragHandleProps>) => JSX.Element;
}
declare module "common/hooks/use-merged-ref" {
    export function useMergedRef<T>(a: React.Ref<T> | null | undefined, b: React.Ref<T> | null | undefined): React.RefCallback<T>;
    export function setRef<T>(ref: React.Ref<T> | null | undefined, value: T): void;
}
declare module "common/utils/cancel-token" {
    type CancelCallback = () => void;
    export class CancelToken {
        private _callback;
        complete(): void;
        cancel(): void;
        onCancel(callback: CancelCallback): void;
    }
    export function useCancelToken(): CancelToken;
}
declare module "common/utils/css-transitions" {
    import { CSSProperties } from 'react';
    export interface CssTransition {
        duration: number;
        delay: number;
        timingFunction: CubicBezier;
    }
    export function getCssTransitionTime(element: any): number;
    export function getCssTransition(element: any, propertyName: string): CssTransition | null;
    export function getModifiedTransitionStyles(element: any, propertyName: string, transition: CssTransition): CSSProperties;
    export function splitCssTransition(transition: CssTransition, time: number): CssTransition;
    type CubicBezier = [number, number, number, number];
    export function parseBezier(timingFunction: string | undefined): CubicBezier | null;
    export function splitBezier([x1, y1, x2, y2]: CubicBezier, t: number): CubicBezier;
    export function formatBezier([x1, y1, x2, y2]: CubicBezier): string;
}
declare module "common/utils/promise" {
    import { CancelToken } from "common/utils/cancel-token";
    export function delay(time: number, token: CancelToken): Promise<boolean>;
    export function nextFrame(token: CancelToken): Promise<boolean>;
}
declare module "common/utils/react" {
    import React from 'react';
    interface RefReactElement<T, P> extends React.ReactElement<P> {
        ref?: React.Ref<T>;
    }
    export function isValidRefElement<T, P = any>(object: {} | null | undefined): object is RefReactElement<T, P>;
}
declare module "common/animations/transition-context" {
    import React from 'react';
    export enum TransitionState {
        in = 0,
        enter = 1,
        exit = 2
    }
    export interface TransitionContextProps {
        readonly state: TransitionState;
        onMount(): void;
        onUnmount(): void;
    }
    export const defaultContext: TransitionContextProps;
    export const TransitionContext: React.Context<TransitionContextProps>;
    export const ExitTransitionActive: React.Context<boolean>;
}
declare module "common/animations/class-name-transition" {
    import React from 'react';
    export interface TransitionStyles {
        base?: string;
        enter?: string;
        enterActive?: string;
        exit?: string;
        exitActive?: string;
    }
    export const emptyStyles: TransitionStyles;
    interface ClassNameTransitionProps {
        styles: TransitionStyles | null;
        enterDuration?: number;
        exitDuration?: number;
    }
    export const ClassNameTransition: ({ styles, enterDuration, exitDuration, children, }: React.PropsWithChildren<ClassNameTransitionProps>) => JSX.Element;
}
declare module "common/data-binding/audio-bindings" {
    export enum UISound {
        selectItem = "select-item",
        dragSlider = "drag-slider",
        hoverItem = "hover-item",
        expandPanel = "expand-panel",
        grabSlider = "grabSlider",
        selectDropdown = "select-dropdown",
        selectToggle = "select-toggle",
        focusInputField = "focus-input-field",
        signatureBuildingEvent = "signature-building-event",
        bulldoze = "bulldoze",
        bulldozeEnd = "bulldoze-end",
        relocateBuilding = "relocate-building",
        mapTilePurchaseMode = "map-tile-purchase-mode",
        mapTilePurchaseModeEnd = "map-tile-purchase-mode-end",
        xpEvent = "xp-event",
        milestoneEvent = "milestone-event",
        economy = "economy",
        chirpEvent = "chirp-event",
        likeChirp = "like-chirp",
        chirper = "chirper",
        purchase = "purchase",
        enableBuilding = "enable-building",
        disableBuilding = "disable-building",
        pauseSimulation = "pause-simulation",
        resumeSimulation = "resume-simulation",
        simulationSpeed1 = "simulation-speed-1",
        simulationSpeed2 = "simulation-speed-2",
        simulationSpeed3 = "simulation-speed-3",
        togglePolicy = "toggle-policy",
        takeLoan = "take-loan",
        removeItem = "remove-item",
        toggleInfoMode = "toggle-info-mode",
        takePhoto = "take-photo",
        tutorialTriggerCompleteEvent = "tutorial-trigger-complete-event",
        selectRadioNetwork = "select-radio-network",
        selectRadioStation = "select-radio-station",
        generateRandomName = "generate-random-name",
        decreaseElevation = "decrease-elevation",
        increaseElevation = "increase-elevation",
        selectPreviousItem = "select-previous-item",
        selectNextItem = "select-next-item",
        openPanel = "open-panel",
        closePanel = "close-panel",
        openMenu = "open-menu",
        closeMenu = "close-menu"
    }
    export function playUISound(sound: UISound | string | null | undefined, volume?: number): void;
}
declare module "common/animations/transition-sounds" {
    import { UISound } from "common/data-binding/audio-bindings";
    export interface TransitionSounds {
        enter?: UISound | string | null;
        exit?: UISound | string | null;
    }
    export const panelTransitionSounds: TransitionSounds;
    export const menuTransitionSounds: TransitionSounds;
    export function useTransitionSounds(transitionSounds: TransitionSounds | null | undefined): void;
}
declare module "common/focus/controller/focus-controller" {
    import React from 'react';
    import { UniqueFocusKey } from "common/focus/focus-key";
    export interface FocusController {
        isChildFocused(focusKey: UniqueFocusKey): boolean;
        registerChild(focusKey: UniqueFocusKey, element: FocusController): void;
        unregisterChild(focusKey: UniqueFocusKey): void;
        attachCallback(callback: FocusCallback): void;
        detachCallback(callback: FocusCallback): void;
        attachTo(controller: FocusController): void;
        detach(): void;
        getBounds(): FocusDOMRect | null;
        getFocusedBounds(): FocusDOMRect | null;
        debugTrace(): string;
        deepDebugTrace(): string;
    }
    export interface FocusDOMRect {
        left: number;
        top: number;
        right: number;
        bottom: number;
        width: number;
        height: number;
    }
    export interface FocusCallback {
        (selfFocused: boolean, currentFocus: FocusController | null): void;
    }
    export enum FocusActivation {
        Always = "always",
        AnyChildren = "anyChildren",
        FocusedChild = "focusedChild"
    }
    export const FocusContext: React.Context<FocusController>;
    export const disabledFocusController: FocusController;
}
declare module "common/focus/controller/focus-controller-base" {
    import { UniqueFocusKey } from "common/focus/focus-key";
    import { FocusCallback, FocusController, FocusDOMRect } from "common/focus/controller/focus-controller";
    export abstract class FocusControllerBase implements FocusController {
        private readonly propagateCurrent;
        private _parentController;
        private _enabled;
        private _focusKey;
        private _lastFocused;
        private readonly callbacks;
        constructor(propagateCurrent?: boolean);
        isChildFocused: (focusKey: UniqueFocusKey) => boolean;
        get focused(): boolean;
        protected abstract isChildFocusedImpl(focusKey: UniqueFocusKey): boolean;
        abstract registerChild(focusKey: UniqueFocusKey, element: FocusController): void;
        abstract unregisterChild(focusKey: UniqueFocusKey): void;
        attachCallback: (callback: FocusCallback) => void;
        detachCallback: (callback: FocusCallback) => void;
        abstract getBounds(): FocusDOMRect | null;
        abstract getFocusedBounds(): FocusDOMRect | null;
        attachTo(controller: FocusController): void;
        detach(): void;
        protected get enabled(): boolean;
        protected set enabled(value: boolean);
        protected get focusKey(): UniqueFocusKey | null;
        protected set focusKey(focusKey: UniqueFocusKey | null);
        protected updateChildren: (currentFocus: FocusController | null) => void;
        protected onFocusUpdate: FocusCallback;
        protected onFocusEnterImpl(_: FocusController | null): void;
        private _tryAttach;
        private _tryDetach;
        debugTrace(): string;
        deepDebugTrace(): string;
        protected get debugName(): string;
        protected abstract get debugFocusedChild(): FocusController | null;
    }
}
declare module "common/focus/controller/multi-child-focus-controller" {
    import { UniqueFocusKey } from "common/focus/focus-key";
    import { FocusActivation, FocusController } from "common/focus/controller/focus-controller";
    import { FocusControllerBase } from "common/focus/controller/focus-controller-base";
    export function useMultiChildFocusController(focusKey: UniqueFocusKey | null, activation: FocusActivation): MultiChildFocusController;
    export interface RefocusCallback {
        (previousElement: FocusController | null): void;
    }
    export class MultiChildFocusController extends FocusControllerBase {
        private readonly activation;
        private readonly children;
        private _focusedChildKey;
        onRefocus: RefocusCallback | null;
        constructor(focusKey: UniqueFocusKey | null, activation: FocusActivation);
        get focusedChildKey(): UniqueFocusKey | null;
        set focusedChildKey(nextFocusedChildKey: UniqueFocusKey | null);
        has(focusKey: UniqueFocusKey): boolean;
        get(focusKey: UniqueFocusKey): FocusController | undefined;
        entries(): Iterable<[UniqueFocusKey, FocusController]>;
        isChildFocusedImpl: (focusKey: UniqueFocusKey) => boolean;
        registerChild: (focusKey: UniqueFocusKey, element: FocusController) => void;
        unregisterChild: (focusKey: UniqueFocusKey) => void;
        getBounds(): {
            left: number;
            top: number;
            right: number;
            bottom: number;
            width: number;
            height: number;
        } | null;
        getFocusedBounds(): import("common/focus/controller/focus-controller").FocusDOMRect | null;
        onFocusEnterImpl(previousElement: FocusController | null): void;
        protected get debugFocusedChild(): FocusController | null;
    }
}
declare module "common/focus/navigation" {
    import { Number2 } from "common/math";
    import { FocusDOMRect } from "common/focus/controller/focus-controller";
    import { MultiChildFocusController } from "common/focus/controller/multi-child-focus-controller";
    import { UniqueFocusKey } from "common/focus/focus-key";
    export enum NavigationDirection {
        Horizontal = "horizontal",
        Vertical = "vertical",
        Both = "both",
        None = "none"
    }
    export function transformNavigationInput(value: Number2, dir: NavigationDirection): Number2;
    export function getClosestKey(controller: MultiChildFocusController, pos: Number2, anchor: Number2): UniqueFocusKey | null;
    export function getClosestKeyInDirection(controller: MultiChildFocusController, pos: Number2, dir: Number2, anchor: Number2, ignoreKey?: UniqueFocusKey): UniqueFocusKey | null;
    export const focusAnchorCenter: Number2;
    export const focusAnchorTop: Number2;
    export const focusAnchorLeft: Number2;
    export const focusAnchorBottom: Number2;
    export const focusAnchorRight: Number2;
    export function getElementFocusPosition(rect: FocusDOMRect | null, anchor: Number2): Number2 | null;
}
declare module "common/focus/controller/pass-through-focus-controller" {
    import { UniqueFocusKey } from "common/focus/focus-key";
    import { FocusController } from "common/focus/controller/focus-controller";
    import { FocusControllerBase } from "common/focus/controller/focus-controller-base";
    export function usePassThroughFocusController(debugName: string, enabled?: boolean, childFocused?: boolean): PassThroughFocusController;
    class PassThroughFocusController extends FocusControllerBase {
        private childElement;
        private _childFocused;
        get enabled(): boolean;
        set enabled(value: boolean);
        get childFocused(): boolean;
        set childFocused(value: boolean);
        private _debugName;
        get debugName(): string;
        set debugName(value: string);
        isChildFocusedImpl: (focusKey: UniqueFocusKey) => boolean;
        registerChild: (focusKey: UniqueFocusKey, element: FocusController) => void;
        unregisterChild: (focusKey: UniqueFocusKey) => void;
        getBounds: () => import("common/focus/controller/focus-controller").FocusDOMRect | null;
        getFocusedBounds(): import("common/focus/controller/focus-controller").FocusDOMRect | null;
        protected get debugFocusedChild(): FocusController | null;
    }
}
declare module "common/focus/focus-hooks" {
    import React from 'react';
    import { FocusCallback, FocusController } from "common/focus/controller/focus-controller";
    export function useFocused(focusController: FocusController): boolean;
    export function useFocusedRef(focusController: FocusController): React.RefObject<boolean>;
    export function useFocusCallback(focusController: FocusController, callback: FocusCallback | null | undefined): void;
}
declare module "common/focus/focus-node" {
    import React from 'react';
    import { FocusController } from "common/focus/controller/focus-controller";
    export interface FocusNodeProps {
        controller: FocusController;
    }
    export const FocusNode: ({ controller, children }: React.PropsWithChildren<FocusNodeProps>) => JSX.Element;
}
declare module "common/input-events/input-actions" {
    import { Number2 } from "common/math";
    export type Action = () => void | boolean;
    export type Action1D = (value: number) => void | boolean;
    export type Action2D = (value: Number2) => void | boolean;
    interface InputActionsDefinition {
        'Move Horizontal': Action1D;
        'Change Slider Value': Action1D;
        'Change Tool Option': Action1D;
        'Move Vertical': Action1D;
        'Switch Radio Station': Action1D;
        'Scroll Vertical': Action1D;
        'Select': Action;
        'Purchase Dev Tree Node': Action;
        'Select Chirp Sender': Action;
        'Save Game': Action;
        'Expand Group': Action;
        'Collapse Group': Action;
        'Select Route': Action;
        'Remove Operating District': Action;
        'Upgrades Menu': Action;
        'Purchase Map Tile': Action;
        'Unfollow Citizen': Action;
        'Like Chirp': Action;
        'Unlike Chirp': Action;
        'Enable Info Mode': Action;
        'Disable Info Mode': Action;
        'Toggle Tool Color Picker': Action;
        'Cinematic Mode': Action;
        'Photo Mode': Action;
        'Back': Action;
        'Leave Underground Mode': Action;
        'Leave Info View': Action;
        'Switch Tab': Action1D;
        'Switch DLC': Action1D;
        'Switch Ordering': Action1D;
        'Switch Radio Network': Action1D;
        'Change Time Scale': Action1D;
        'Switch Page': Action1D;
        'Tool Options': Action;
        'Switch Toolmode': Action;
        'Toggle Snapping': Action;
        'Previous Tutorial Phase': Action;
        'Continue Tutorial': Action;
        'Focus Tutorial List': Action;
        'Pause Simulation': Action;
        'Resume Simulation': Action;
        'Switch Speed': Action;
        'Speed 1': Action;
        'Speed 2': Action;
        'Speed 3': Action;
        'Bulldozer': Action;
        'Change Elevation': Action1D;
        'Advisor': Action;
        'Quicksave': Action;
        'Quickload': Action;
        'Focus Selected Object': Action;
        'Hide UI': Action;
        'Map tile Purchase Panel': Action;
        'Info View': Action;
        'Progression Panel': Action;
        'Economy Panel': Action;
        'City Information Panel': Action;
        'Statistic Panel': Action;
        'Transportation Overview Panel': Action;
        'Chirper Panel': Action;
        'Lifepath Panel': Action;
        'Event Journal Panel': Action;
        'Radio Panel': Action;
        'Photo Mode Panel': Action;
        'Take Photo': Action;
        'Pause Menu': Action;
        'Load Game': Action;
        'Start Game': Action;
        'Save Options': Action;
        'Switch User': Action;
        'Unset Binding': Action;
        'Switch Savegame Location': Action1D;
        'Debug UI': Action;
        'Debug Prefab Tool': Action;
        'Debug Change Field': Action1D;
        'Debug Multiplier': Action1D;
    }
    export type InputAction = keyof InputActionsDefinition;
    export type InputActions = {
        [K in InputAction]?: InputActionsDefinition[K] | null;
    };
}
declare module "common/input-events/input-stack" {
    import { InputAction } from "common/input-events/input-actions";
    export class InputStack {
        _items: InputStackItem[];
        contains(action: InputAction): boolean;
        indexOf(action: InputAction): number;
        push(action: InputAction, callback: Function): void;
        removeWhere(predicate: (action: InputAction) => boolean): void;
        clear(): void;
        dispatchInputEvent(action: InputAction, value: any): boolean;
        debugPrint(): void;
    }
    class InputStackItem {
        readonly action: InputAction;
        readonly callback: Function;
        constructor(action: InputAction, callback: Function);
    }
}
declare module "common/input-events/input-controller" {
    import React from 'react';
    import { InputStack } from "common/input-events/input-stack";
    export interface InputController {
        attachChild(controller: InputController): void;
        detachChild(controller: InputController): void;
        transformStack(stack: InputStack): void;
        setDirty(): void;
    }
    export const defaultInputController: InputController;
    export const InputContext: React.Context<InputController>;
    export type InputStackTransformer = (stack: InputStack) => void;
    export function useInputController(enabled: boolean, transformer: InputStackTransformer | null): InputController;
    export class InputControllerImpl implements InputController {
        private _parent;
        private _child;
        private _transformer;
        get transformer(): InputStackTransformer | null;
        set transformer(transformer: InputStackTransformer | null);
        attachTo(controller: InputController): void;
        detach(): void;
        attachChild(controller: InputController): void;
        detachChild(controller: InputController): void;
        transformStack(stack: InputStack): void;
        setDirty(): void;
    }
    export class RootInputControllerImpl implements InputController {
        private stack;
        private onStackChanged;
        private _child;
        constructor(stack: InputStack, onStackChanged: () => void);
        attachChild(controller: InputController): void;
        detachChild(controller: InputController): void;
        transformStack(): void;
        setDirty(): void;
    }
}
declare module "common/input-events/input-action-consumer" {
    import React from 'react';
    import { InputActions } from "common/input-events/input-actions";
    interface InputActionConsumerProps {
        actions: InputActions | null;
        disabled?: boolean;
    }
    export const InputActionConsumer: React.NamedExoticComponent<React.PropsWithChildren<InputActionConsumerProps>>;
    interface SingleActionConsumerProps {
        disabled?: boolean;
        onAction?: () => void;
    }
    /** When the Gamepad "A" button is pressed */
    export const SelectConsumer: ({ disabled, children, onAction }: React.PropsWithChildren<SingleActionConsumerProps>) => JSX.Element;
    interface ExpandConsumerProps extends SingleActionConsumerProps {
        expanded: boolean;
        expandable: boolean;
    }
    /** When the Gamepad "X" button is pressed */
    export const ExpandConsumer: ({ expanded, expandable, disabled, children, onAction }: React.PropsWithChildren<ExpandConsumerProps>) => JSX.Element;
    /** When the Keyboard "ESC" or Gamepad "B" button is pressed */
    export const BackConsumer: ({ disabled, children, onAction }: React.PropsWithChildren<SingleActionConsumerProps>) => JSX.Element;
}
declare module "common/focus/navigation-scope" {
    import React from 'react';
    import { FocusActivation, FocusController } from "common/focus/controller/focus-controller";
    import { MultiChildFocusController } from "common/focus/controller/multi-child-focus-controller";
    import { FocusKey, UniqueFocusKey } from "common/focus/focus-key";
    import { NavigationDirection } from "common/focus/navigation";
    interface NavigationScopeProps {
        focusKey?: FocusKey;
        debugName?: string;
        focused: UniqueFocusKey | null;
        direction?: NavigationDirection;
        activation?: FocusActivation;
        onChange: (key: UniqueFocusKey | null) => void;
        onRefocus?: (controller: MultiChildFocusController, lastElement: FocusController | null) => UniqueFocusKey | null;
    }
    /**
     * A stateless component that allows the user to navigate between multiple focusable children with a gamepad.
     *
     * The `onRefocus` callback controls what happens when the focus is lost within the scope.
     *
     * The focus behavior of the scope can be controlled by the `activation` prop.
     *
     * Optionally, a `focusKey` for the component itself can be set.
     */
    export const NavigationScope: ({ focusKey, debugName, focused, direction, activation, children, onChange, onRefocus }: React.PropsWithChildren<NavigationScopeProps>) => JSX.Element;
    export function refocusClosestKeyIfNoFocus(focusController: MultiChildFocusController, lastElement: FocusController | null): UniqueFocusKey | null;
    export function refocusClosestKey(focusController: MultiChildFocusController, lastElement: FocusController | null): UniqueFocusKey | null;
}
declare module "common/focus/auto-navigation-scope" {
    import React from 'react';
    import { FocusActivation, FocusController } from "common/focus/controller/focus-controller";
    import { MultiChildFocusController } from "common/focus/controller/multi-child-focus-controller";
    import { FocusKey, UniqueFocusKey } from "common/focus/focus-key";
    import { NavigationDirection } from "common/focus/navigation";
    interface AutoNavigationScopeProps {
        focusKey?: FocusKey;
        initialFocused?: UniqueFocusKey | null;
        direction?: NavigationDirection;
        activation?: FocusActivation;
        onRefocus?: (controller: MultiChildFocusController, lastElement: FocusController | null) => UniqueFocusKey | null;
    }
    /**
     * Automatic navigation in lists, grids and forms.
     */
    export const AutoNavigationScope: ({ focusKey, initialFocused, direction, activation, children, onRefocus }: React.PropsWithChildren<AutoNavigationScopeProps>) => JSX.Element;
}
declare module "common/dom-node/dom-node" {
    import React, { CSSProperties, ReactNode } from 'react';
    import { RefCallback } from 'react';
    interface DOMNodeContextProps {
        ref?: RefCallback<HTMLElement>;
        style?: CSSProperties;
        className?: string;
    }
    export const DOMNodeContext: React.Context<DOMNodeContextProps | null>;
    export const DOMNodeProvider: ({ children }: React.PropsWithChildren) => JSX.Element;
    interface DOMNodeModifierProps {
        style?: CSSProperties;
        className?: string;
        children?: ReactNode;
    }
    export const DOMNodeModifier: React.MemoExoticComponent<React.ForwardRefExoticComponent<DOMNodeModifierProps & React.RefAttributes<HTMLElement>>>;
}
declare module "common/data-binding/input-bindings" {
    import { Number2 } from "common/math";
    export enum ControlScheme {
        keyboardAndMouse = 0,
        gamepad = 1
    }
    export enum GamepadType {
        Xbox = 0,
        PS = 1
    }
    export interface InputHint {
        name: string;
        bindings: string[];
        modifiers: string[];
    }
    export interface TutorialInputHintQuery {
        map: string;
        action: string;
        controlScheme: number;
    }
    export const mouseOverUI$: import("common/data-binding/binding").ValueBinding<boolean>;
    export const controlScheme$: import("common/data-binding/binding").ValueBinding<ControlScheme>;
    export const gamepadPointerPosition$: import("common/data-binding/binding").ValueBinding<Number2>;
    export const cameraMoving$: import("common/data-binding/binding").ValueBinding<boolean>;
    export const cameraBarrier$: import("common/data-binding/binding").EventBinding<unknown>;
    export const toolBarrier$: import("common/data-binding/binding").EventBinding<unknown>;
    export const inputHints$: import("common/data-binding/binding").ValueBinding<InputHint[]>;
    export const onInputHintPerformed: (action: string) => void;
    export const tutorialHints$: import("common/data-binding/binding").MapBinding<TutorialInputHintQuery, InputHint[]>;
    export const gamepadType$: import("common/data-binding/binding").ValueBinding<GamepadType>;
    export const setActiveTextFieldRect: (x: number, y: number, width: number, height: number) => void;
    export const requireTextFieldInputBarrier$: import("common/data-binding/binding").ValueBinding<boolean>;
    export function onGamepadPointerEvent(pointerOverUI: boolean): void;
}
declare module "common/hooks/use-control-scheme" {
    export function useKeyboardAndMouseActive(): boolean;
    export function useGamepadActive(): boolean;
}
declare module "common/scrolling/scrollable-context" {
    import React from 'react';
    export interface ScrollableContextProps {
        scrollTo(x: number, y: number): void;
        scrollBy(x: number, y: number): void;
        smoothScrollTo(x: number, y: number): void;
        scrollIntoView(element: Element): void;
        updateThumbs(): void;
    }
    export const ScrollableContext: React.Context<ScrollableContextProps>;
}
declare module "common/focus/controller/element-focus-context" {
    import { UniqueFocusKey } from "common/focus/focus-key";
    import { FocusActivation, FocusController } from "common/focus/controller/focus-controller";
    import { FocusControllerBase } from "common/focus/controller/focus-controller-base";
    export function useElementFocusController(focusKey: UniqueFocusKey | null, elementRef: React.RefObject<HTMLElement | SVGElement | null>, activation?: FocusActivation, allowChildren?: boolean): ElementFocusController;
    interface BoundsCallback {
        (): DOMRect | null;
    }
    class ElementFocusController extends FocusControllerBase {
        readonly getBounds: BoundsCallback;
        private readonly activation;
        private readonly allowChildren;
        private childFocusKey;
        private childElement;
        constructor(focusKey: UniqueFocusKey | null, getBounds: BoundsCallback, activation: FocusActivation, allowChildren: boolean);
        isChildFocusedImpl: (childFocusKey: UniqueFocusKey) => boolean;
        registerChild: (childFocusKey: UniqueFocusKey, element: FocusController) => void;
        unregisterChild: (childFocusKey: UniqueFocusKey) => void;
        getFocusedBounds(): DOMRect | import("common/focus/controller/focus-controller").FocusDOMRect | null;
        protected get debugFocusedChild(): FocusController | null;
    }
}
declare module "common/focus/focus-div" {
    import React from 'react';
    import { UISound } from "common/data-binding/audio-bindings";
    import { FocusActivation } from "common/focus/controller/focus-controller";
    import { FocusKey } from "common/focus/focus-key";
    interface PassiveFocusDivProps extends React.HTMLAttributes<HTMLDivElement> {
        onFocusChange?: (focused: boolean) => void;
        focusSound?: UISound | string | null;
    }
    /**
     * A passive div element that has a `focused` class name when the focusable child inside of it is focused.
     *
     * It can contain zero or one focusable children.
     *
     * The component itself cannot be focused, it purely acts as a wrapper around a focusable child.
     * That means if there is no focusable child inside of it, it can never receive the `focused` class name.
     */
    export const PassiveFocusDiv: React.ForwardRefExoticComponent<PassiveFocusDivProps & React.RefAttributes<HTMLDivElement>>;
    interface ActiveFocusDivProps extends PassiveFocusDivProps {
        focusKey?: FocusKey;
        debugName?: string;
        activation?: FocusActivation;
    }
    /**
     * A focusable div element. It has a `focused` class name while it is focused.
     *
     * It can contain zero or one focusable children.
     *
     * Unlike the `PassiveFocusDiv`, the element itself can be focused, even if there are no elements inside of it.
     */
    export const ActiveFocusDiv: React.ForwardRefExoticComponent<ActiveFocusDivProps & React.RefAttributes<HTMLDivElement>>;
}
declare module "common/focus/controller/key-focus-controller" {
    import { UniqueFocusKey } from "common/focus/focus-key";
    import { FocusController } from "common/focus/controller/focus-controller";
    import { FocusControllerBase } from "common/focus/controller/focus-controller-base";
    export function useKeyFocusController(focusKey: UniqueFocusKey | null): KeyFocusController;
    class KeyFocusController extends FocusControllerBase {
        private childFocusKey;
        private childElement;
        constructor(focusKey: UniqueFocusKey | null);
        isChildFocusedImpl: (childFocusKey: UniqueFocusKey) => boolean;
        registerChild: (focusKey: UniqueFocusKey, element: FocusController) => void;
        unregisterChild: (focusKey: UniqueFocusKey) => void;
        getBounds: () => import("common/focus/controller/focus-controller").FocusDOMRect | null;
        getFocusedBounds(): import("common/focus/controller/focus-controller").FocusDOMRect | null;
        protected get debugFocusedChild(): FocusController | null;
    }
}
declare module "common/focus/focus-key-override" {
    import React from 'react';
    import { FocusKey } from "common/focus/focus-key";
    interface FocusKeyOverrideProps {
        focusKey: FocusKey | undefined;
    }
    /**
     * A passive component that overrides the focusKey of the child (so it will be registered to the parent using the `focusKey` prop)
     *
     * It can contain zero or one focusable children.
     *
     * The component itself cannot be focused, it purely acts as a wrapper around a focusable child.
     */
    export const FocusKeyOverride: ({ focusKey, children }: React.PropsWithChildren<FocusKeyOverrideProps>) => JSX.Element;
}
declare module "common/focus/controller/root-focus-controller" {
    import { UniqueFocusKey } from "common/focus/focus-key";
    import { FocusController } from "common/focus/controller/focus-controller";
    import { FocusControllerBase } from "common/focus/controller/focus-controller-base";
    export function useRootFocusController(): RootFocusController;
    class RootFocusController extends FocusControllerBase {
        private childElement;
        constructor();
        get debugName(): string;
        isChildFocusedImpl: (focusKey: UniqueFocusKey) => boolean;
        registerChild: (focusKey: UniqueFocusKey, element: FocusController) => void;
        unregisterChild: (focusKey: UniqueFocusKey) => void;
        getBounds: () => null;
        getFocusedBounds(): null;
        protected get debugFocusedChild(): FocusController | null;
    }
}
declare module "common/focus/focus-root" {
    import React from 'react';
    export const FocusRoot: ({ children }: React.PropsWithChildren) => JSX.Element;
}
declare module "common/hooks/use-shallow-memoize" {
    export function useShallowMemoize<T>(value: T, depth?: number): T;
}
declare module "common/image/preload" {
    export function preloadImages<T extends Record<string, string> | string[]>(urls: T): T;
    export function usePreloadedImages<T extends Record<string, string>>(urls: T): T;
}
declare module "common/panel/panel-theme" {
    export interface PanelTheme extends PanelTitleBarTheme {
        panel: string;
        header: string;
        content: string;
        footer: string;
    }
    export interface PanelTitleBarTheme {
        titleBar: string;
        title: string;
        icon: string;
        iconSpace: string;
        closeButton: string;
        closeIcon: string;
        toggle: string;
        toggleIcon: string;
        toggleIconExpanded: string;
    }
    export function usePanelTheme(partialTheme: Partial<PanelTheme>): PanelTheme;
}
declare module "common/panel/panel-context" {
    import React from 'react';
    import { PanelTheme } from "common/panel/panel-theme";
    export interface PanelContextProps {
        theme: PanelTheme;
        onClose?: () => void;
    }
    export const PanelContext: React.Context<PanelContextProps>;
    export interface CollapsiblePanelContextProps extends PanelContextProps {
        expanded: boolean;
        toggleExpanded: () => void;
    }
    export const CollapsiblePanelContext: React.Context<CollapsiblePanelContextProps>;
}
declare module "common/panel/panel" {
    import React, { ReactNode } from 'react';
    import { TransitionStyles } from "common/animations/class-name-transition";
    import { TransitionSounds } from "common/animations/transition-sounds";
    import { FocusKey } from "common/focus/focus-key";
    import { PanelTheme } from "common/panel/panel-theme";
    export interface PanelProps extends React.HTMLAttributes<HTMLDivElement> {
        focusKey?: FocusKey;
        header?: ReactNode;
        footer?: ReactNode;
        theme?: Partial<PanelTheme>;
        transition?: TransitionStyles | null;
        transitionSounds?: TransitionSounds | null;
        contentClassName?: string;
        onClose?: () => void;
        allowFocusExit?: boolean;
    }
    export const Panel: React.ForwardRefExoticComponent<PanelProps & React.RefAttributes<HTMLDivElement>>;
}
declare module "common/panel/draggable-panel/draggable-panel" {
    import { Number2 } from "common/math";
    import { PanelProps } from "common/panel/panel";
    import './draggable-panel.scss';
    export interface DraggablePanelProps extends PanelProps {
        initialPosition?: Number2;
    }
    export const DraggablePanel: ({ header, initialPosition, className, style, children, ...props }: DraggablePanelProps) => JSX.Element;
}
declare module "ui/panel" {
    import { PropsWithChildren } from 'react';
    import { DraggablePanelProps as _DraggablePanelProps } from "common/panel/draggable-panel/draggable-panel";
    import { PanelProps as _PanelProps } from "common/panel/panel";
    export interface PanelProps extends _PanelProps {
        draggable?: false | undefined;
    }
    export interface DraggablePanelProps extends _DraggablePanelProps {
        draggable: true;
    }
    export const Panel: (props: PropsWithChildren<PanelProps | DraggablePanelProps>) => JSX.Element;
}
declare module "ui/index" {
    export const UI: {
        Panel: (props: import("react").PropsWithChildren<import("ui/panel").PanelProps | import("ui/panel").DraggablePanelProps>) => JSX.Element;
    };
}
declare module "modding/types" {
    import { ComponentType } from 'react';
    import * as API from "api/index";
    import { UI } from "ui/index";
    import engine from 'cohtml/cohtml';
    export type ModuleRegistryExtend = <T extends ComponentType<any>>(curr: T) => (props: any) => JSX.Element;
    export type ModuleRegistryAppend = ComponentType<any> | (() => JSX.Element);
    export type ModuleRegistry = {
        get(modulePath: string, exportName: string): any;
        add(modulePath: string, module: Record<string, any>): void;
        override(modulePath: string, exportName: string, newValue: any): void;
        extend(modulePath: string, exportNameOrSCSSValue: string | any, extendCb?: ModuleRegistryExtend): void;
        append(target: 'Menu' | 'Editor' | 'Game', appendedComponent: ModuleRegistryAppend, _?: never): void;
        append(modulePath: string, exportName: string, appendedComponent?: ModuleRegistryAppend | null): void;
        registry: Map<string, Record<string, any>>;
        find(query: string | RegExp): [path: string, ...exports: string[]][];
        reset(): void;
    };
    export interface ModdingContext {
        engine: typeof engine;
        UI: typeof UI;
        api: typeof API;
    }
    export type ModRegistrar<T = ModdingContext, K = T & ModdingContext> = (moduleRegistry: ModuleRegistry) => void;
}
declare module "modding/modding-context" {
    import { PropsWithChildren } from 'react';
    import { ModdingContext, ModRegistrar } from "modding/types";
    const ModdingContext: import("react").Context<ModdingContext>;
    interface ModdingProviderProps {
        registry: Array<ModRegistrar>;
        ready: boolean;
        mods: Array<string>;
    }
    export const ModdingProvider: ({ children, mods, registry }: PropsWithChildren<ModdingProviderProps>) => JSX.Element;
    export const useModding: () => ModdingContext;
}
declare module "modding/index" {
    export { useModding } from "modding/modding-context";
}
