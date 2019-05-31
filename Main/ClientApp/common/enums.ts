export enum RequestEvent {
    None = 0,
    Start = 1,
    End = 2,

    Body = 3
}

export const enum CommandType {
    None = 0,
    Page = 1,
    Save = 2,
    Clear = 3
}


export enum GridType {
    none,
    journal,
    goep,
    pou,
    route,
    dispatcher,
    nmpn
}

export const enum FilterType {
    Text = 0,
    Autocomplite = 1
}

export interface FilterInfo {
    filterType: FilterType
    conditionName: string
    data: any
}

export enum ReferenceBookType {
    users = 0,
    trains = 1,
    carriages = 2,
    equipments = 3,
    stantions = 4,
    parkings = 5
}

export enum ReportType {
    a = 0,
    b = 1,
    c = 2,
    d = 3
}

export enum DocumentType {
    Other = 0,

    Image = 1,

    Sound = 2
}

export enum TimelineTypeEnum {

    //ТО-2
    TimeRangeTo2 = 1,

    //Рейс
    TimeRangeTrip = 2,

    //Критичный инцидент
    TimeTaskCritical = 3,

    //СТО
    TimeSto = 4,

    //ТО-1
    TimeRangeTo1 = 5,

    //Приемка поезда
    Inspection = 6,

    //Сдача поезда
    Surrender = 7,

    //Постановка на канал
    Channeling = 8,

    //Вход в Депо
    EntryDepo = 9,

    //Посадка высадка бригады
    TimeBrigade = 10,

    //Перегонный рейс
    TimeRangeTripTransfer = 11,

    //Отмена Рейса
    TimeRangeCancelTrip = 12,

    //Изменен поезд
    ChangeTrain = 13
}

export const getTimeLineDescription = (tl: TimelineTypeEnum) => {
    switch (tl) {
        case TimelineTypeEnum.TimeRangeTo2:
            return 'ТО-2';
        case TimelineTypeEnum.TimeRangeTrip:
            return 'Рейс';
        case TimelineTypeEnum.TimeTaskCritical:
            return 'Критичный инцидент';
        case TimelineTypeEnum.TimeSto:
            return 'СТО';

        case TimelineTypeEnum.TimeRangeTo1:
            return 'ТО-1';
        case TimelineTypeEnum.Inspection:
            return 'Приемка поезда';
        case TimelineTypeEnum.Surrender:
            return 'Сдача\Выпуск поезда';
        case TimelineTypeEnum.Channeling:
            return 'Постановка на канал';

        case TimelineTypeEnum.EntryDepo:
            return 'Вход в Депо';
        case TimelineTypeEnum.TimeBrigade:
            return 'Посадка бригады';
        case TimelineTypeEnum.TimeRangeTripTransfer:
            return 'Перегонный рейс';
        case TimelineTypeEnum.TimeRangeCancelTrip:
            return 'Отмененно';
        case TimelineTypeEnum.ChangeTrain:
            return 'Изменен поезд';
    }
}