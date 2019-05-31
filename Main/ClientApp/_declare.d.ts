interface StantionUser {
    stantionStart: SelectItem
    stantionEnd: SelectItem
    users: SelectItemPlaneBrigadeTrain[]
}

interface Nmpn {
    daysData: NmpnDaysData[]
    route: NmpnRoute
    routeDays: number[]
}

interface NmpnRoute {
    depoEvents: any
    description: any
    id: number
    mileage: any
    name: string
    planedRouteTrains: any
    tripOnRoutes: any
    turnover: any
    turnoverId: number
}

interface NmpnDaysData {
    date: any
    dateString: any
    planedRouteTrainId: number
    train: {
        depoEvents: any
        description: any
        direction: any
        directionId: any
        id: number
        name: string
        stantion: any
        stantionId: number
        updateDate: string
    }
    users: {
        name: string
        planeBrigadeTrainsId: number
        userStations: NmpnUserStantion
    }[]
}

interface NmpnUserStantion {
    inputName: string
    inputTime: any
    outputName: string
    outputTime: any
}

interface DepoEventDtoUi {
    id: number
    parkingId: number
    inspectionId: number
    userId: number
    inspectionTxt: string
    trainId: number
    routeId: number
    inTime: any
    outTime: any
    parkingTime: any
    repairStopTime: any
    testStartTime: any
    testStopTime: any
    depoEventDataSource?: DepoEventDataSource
}

interface DepoEventDataSource {
    users: SelectItem[]
    trains: SelectItemDependent[]
    routes: SelectItem[]
    stantions: SelectItem[]
    parkings: SelectItemDependent[]
    inspections: SelectItem[]
}


interface TurnoverUI {
    id: number
    name: string
    directionId: number
    days: number[]
    directions: SelectItem[]
}

interface TripUi {
    id: number
    name: string
    days: number[]
    tripType: boolean
    stantions: TripStantion[]
    dataSource: {
        stantions: SelectItem[]
    }
}

interface TripStantion {
    id: number
    inTime:any
    outTime: any
    name: string
}

interface JournalTaskCreate
{
    user: string
    avaibleExecutors: number[]
    trains: SelectItem[],
    carriages: SelectItem[],
    equipments: SelectItem[],
    faults: SelectItem[]
}


interface JournalTask {
    id: number
    data: string
    equipmentName: string
    initiatorName: string
    trainName: string
    carriageSerial: string
    faultName: string

    taskType: number
    statusId: number
    brigadeType: number

    carriageModelTypeId: number //Тип модели Вагона

    taskLevel: number

    possibleTaskStatuses: number[]

    history: JournalTaskHistory[]

    //new
    faults: JournalTaskFault[]
    inspections: JournalTaskInspection[]
}

interface JournalTaskInspection {
    id: number
    dateStart: any
    dateEnd: any
    user: string
    type: number //TO-1 To-2 Приемка Сдача
    brigadeType: number
    //Если выполнена приемка\сдача поезда локомотивной бригадой (заполненно поле в TrainTaskAttributes CheckListEquipmentId)
    //То необходимо в текст писать: 
    //{ Оборудование } → { поле в CheckListEquipments NameTask } → { значение по алгоритму число или Да\Нет в зависимости от ValueType } → { текущее установленное значение TrainTaskAttributes Value}
    //Тамбур → Наличие огнетушителей → 12 → 6
    //Для ТО-1 ТО-2 и тд
    //Смотрим что добавилось в рамках мероприятия TaskLevel или FaultId, пишим в текст
    //Неисправность → Что то сломалось
    //Уровень критичности → Высокий
    texts: string[] //Список всех измененных атрибутов в рамках инспекции
}

interface JournalTaskFault {
    id: number
    date: any
    user: string
    text: string
}

interface JournalTaskFile extends JournalTaskHistoryFile {
    path: string
    thumbPath: string
    trainTaskCommentId: number
}

interface JournalTaskHistory {
    date: any
    user: string
    userBrigadeType: number
    type: string
    oldStatus: number
    newStatus: number
    oldExecutorBrigadeType: number
    newExecutorBrigadeType: number
    text: string
    files: JournalTaskHistoryFile[]
}

interface JournalTaskHistoryFile {
    id: number
    documentType: number
    name: string
}

interface TimeRangeTrip {
    trip: string
    stantions: TimeRangeTipStantions[]
    dataSource: {
        trains: SelectItem[]
    }
}

interface TimeRangeTipStantions{
    id: number
    name: string
    train: string
    startPlan: any
    endPlan: any,
    trainId: number
    startFact: any
    endFact: any
    canceled: boolean
    planeStationOnTripId: number
}

interface TimeRangeBrigade {
    users: TimeRangeBrigadeUsers[]
    dataSource: {
        stantions: SelectItem[],
        users: SelectItem[]
    }
}

interface TimeRangeBrigadeUsers {
    id: number
    startId: number
    endId: number
    userId: number
    planeBrigadeTrainId: number

    user: string
    start: string
    end: string
    canseled: boolean
}

interface TimeRangeData {
    id: number
    routeName: string
    trainName: string
    plan: TimeRangeDataResult
    fact: TimeRangeDataResult
}

interface TimeRangeDataResult {
    dateStart: any
    dateEnd: any
    canseled: boolean
}

interface CommonState
{
    isLoading: boolean
}

interface TripsByTurnoverIdAndDays{
    days: number[]
    description: string
    endStationName: string
    endTime: string
    id: number
    name: string
    stantionOnTrips: any[]
    startStationName: string
    startTime: string
    tripOnRoutes: any[]
    updateDate: string
}

interface GraphData {
    data: GraphDataRoute[]
    days: any[],
    name: string
    directionName: string
    turnoverName: string
}

interface GraphDataRoute {
    mileage: number
    routeId: number
    routeName: string
    timeLines: TimeLine[]
}

//interface GraphTimeLine {
//    id: number
//    borderColor: string
//    color: string
//    description: string
//    endTime: string
//    enumType: number
//    inspectionRouteId: number
//    starTime: string
//    tripOnRouteId: number,
//    additionalTimeLineData: {
//        tripStartStationName: string,
//        tripEndStationName: string,
//        stantions: TimeLineStantion[],
//        description: string
//    }
//}

interface PouData {
    data: PouTimeRange[]
    total: number
}

interface PouTimeRange {
    changeTimeLines: TimeLine[]
    planTimeLines: TimeLine[]
    mileage: number
    planedRouteId: number
    routeName: string
    trains: {
        planed: string, change: { current: string, previous: string[] }
    }
}

interface TimeLine{
    id
    borderColor: string
    color: string
    description: string //Типо тут символ
    endTime: string
    starTime: string
    enumType: number
    changed: boolean
    additionalTimeLineData: {
        tripStartStationName: string,
        tripEndStationName: string,
        stantions: TimeLineStantion[],
        description: string
    }
}

interface TimeLineStantion
{
    name: string
    time: string
    canseled: boolean
}


interface IUserInfo {
    id: number
    name: string
}

interface IAuthResult {
    access_token: string
    user_info: IUserInfo
}

interface GridData<TRow> {
    pager: Pager
    rows: TRow[]
    maxLevel: number,
    count: number
}

interface Pager {
    currentPage: number
    firstElementOnPage: number
    firstPageInPagesFrame: number
    hasSides: boolean
    lastElementOnPage: number
    lastPageInPagesFrame: number
    pageNumber: number
    pageSize: number
    rowCount: number
    totalElementsCount: number
    totalPagesCount: number

    data?: any
}

interface GridColumn {
    displayName: string
    name: string
    width?: number
}

interface SortOptions {
    column: string
    direction: number
}

interface GridSourceData
{
    dataSource: any
}

interface SelectItem {
    value: any
    text: string
}

interface SelectItemDependent extends SelectItem {
    dependentId?: any
    fullName?: string
}

interface SelectItemPlaneBrigadeTrain extends SelectItem {
    planeBrigadeTrainsId?: number
}