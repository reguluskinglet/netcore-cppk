import { Reducer, isActionType, typeName, Action } from 'redux-typed'
import { ActionCreator } from '../../../store'
import { ShowDeleteDialog, RequestAction, RequestEvent, GridType, ShowDialog, TimelineTypeEnum, ToogleDialog, ShowTooltip } from '../../../common'
import api from '../../../services/rest'
import storage from '../../../services/storage'
import moment from 'moment'

export interface State {
    stantionsStart?: SelectItem[]
    stantionsEnd?: SelectItem[]
    planedRouteTrainId?: number
}

@typeName("Nmpn.CreateOrEdit")
export class NmpnCreateOrEdit extends Action {
    constructor
        (
        public data: any
        ) {
        super();
    }
}

@typeName("Nmpn.UnloadState")
export class NmpnUnloadState extends Action {
}

var userInfo = storage.getUserInfo();

export const actionCreators = {

    nmpnUnloadState: (): ActionCreator => (dispatch, getState) => {
        dispatch(new NmpnUnloadState())
    },
    get: (startDate, endDate): ActionCreator => (dispatch, getState) => {

        var currentStartDate = startDate,// startDate ? startDate : new Date(),
            currentEdnDate = endDate;// = endDate ? endDate : new Date();

        if (endDate == null) {
            currentEdnDate = startDate;
        }
        else if (startDate == null) {
            currentStartDate = endDate;
        }

        var requestStartDate: any = moment(currentStartDate, 'DD.MM.YYYY');
        var requestEndDate: any = moment(currentEdnDate, 'DD.MM.YYYY');

        requestEndDate.hours(23).minutes(59).seconds(59);

        if (endDate == null)
            requestEndDate.days(31)
        else if (startDate == null)
            requestStartDate.days(-31)

        requestStartDate = requestStartDate.format('YYYY-MM-DDT00:00:00') + 'Z';
        requestEndDate = requestEndDate.format('YYYY-MM-DDTHH:mm:ss') + 'Z';

        api.post(`ScheludePlaned/GetPlanedRouteTrainsTable?skip=0&limit=9999`, { startTime: requestStartDate, endTime: requestEndDate })
            .then(data => {
                dispatch(new RequestAction(RequestEvent.Body, GridType.nmpn, { result: data, startDate: requestStartDate, endDate: requestEndDate }))
            })

        dispatch(new RequestAction(RequestEvent.Body))
    },
    addTrainToRoute: (date: string, dayDate: NmpnDaysData, route: NmpnRoute, action): ActionCreator => (dispatch, getState) => {

        Promise.all([api.get(`Train/GetAll?skip=0&limit=9999`), api.get(`User/GetAll?skip=0&limit=9999`)]).then((result) => {

            var trasinSelectItem: SelectItem[] = result[0].data.map(x => {
                return { text: x.name, value: x.id }
            })

            var userSelectItem: SelectItem[] = result[1].data.map(x => {
                return { text: x.name, value: x.id }
            })

            if (dayDate.planedRouteTrainId)
                dispatch(new NmpnCreateOrEdit({ planedRouteTrainId: dayDate.planedRouteTrainId}))

            //if (dayDate.planedRouteTrainId)
            //    actionCreators.getInputStation(date, dayDate.planedRouteTrainId, dispatch)

            dispatch(new ShowDialog({
                users: userSelectItem,
                trains: trasinSelectItem,
                date: date,
                dayDate: dayDate,
                route: route
            }, 0, 'nmpn.create', true, action));
        })


        //api.post(`ScheludePlaned/GetInputStation`, {
        //    day: date,
        //    inputStationId: null,
        //    planedRouteTrainId: dayDate.planedRouteTrainId,
        //}),

        //api.post(`/ScheludePlaned/GetPlanedRouteTrainsTable?skip=0&limit=9999`, { startTime: requestStartDate, endTime: requestEndDate })
        //    .then(data => {
        //        dispatch(new RequestAction(RequestEvent.Body, GridType.nmpn, { result: data, startDate: requestStartDate, endDate: requestEndDate }))
        //    })

        //dispatch(new RequestAction(RequestEvent.Body))
    },
    getInputStation: (userId: number, date: string, planedRouteTrainId: number): ActionCreator => (dispatch, getState) => {
        if (!userId)
            dispatch(new NmpnCreateOrEdit({ stantionsEnd: [], stantionsStart:[] }))
        else
            api.post('ScheludePlaned/GetInputStation', {
                day: date,
                planedRouteTrainId: planedRouteTrainId,
                userId: userId
            }).then(d => {

                var startStantions: SelectItem[] = d.map(x => {
                    return { value: x.id, text: x.name }
                })

                dispatch(new NmpnCreateOrEdit({ planedRouteTrainId: planedRouteTrainId, stantionsStart: startStantions }))
            }).fail(x => {
                dispatch(new NmpnCreateOrEdit({ planedRouteTrainId: planedRouteTrainId, stantionsStart: [] }))
            })
    },
    saveTrainToRoute: (date: string, trainId: number, routeId: number, action): ActionCreator => (dispatch, getState) => {

        api.post('ScheludePlaned/PlanedRouteTrainsTable', {
            createDate: date,
            date: date,
            trainId: trainId,
            routeId: routeId
        }).then(result => {
            action();
            dispatch(new NmpnCreateOrEdit({ planedRouteTrainId: result.id }))
           // actionCreators.getInputStation(date, result.id, dispatch)
        })


        //api.post('PlaneBrigadeTrain/AddOrUpdate', {
        //    date: date,
        //    planedRouteTrainId: dayDate.planedRouteTrainId,
        //    stantionEndId: stantionEndId,
        //    stantionStartId: stantionStartId,
        //    userId: userId
        //})
    },
    addPlaneBrigadeTrain: (date: string, planedRouteTrainId: number, stantionUsers: StantionUser[], removeUserIds: number[], action): ActionCreator => (dispatch, getState) => {

        const getUserIds = (user: SelectItemPlaneBrigadeTrain[]) => {
            return user.filter(x => x.planeBrigadeTrainsId === undefined).map(u => u.value)
        }

        var stantions = stantionUsers.filter(x => getUserIds(x.users).length > 0);

        Promise.all(removeUserIds.map(x => api.get(`PlaneBrigadeTrain/Delete?id=${x}`))).then(x => {
            Promise.all(stantions.map(x => api.post('PlaneBrigadeTrain/AddOrUpdate', {
                date: date,
                planedRouteTrainId: planedRouteTrainId,
                stantionEndId: x.stantionEnd.value,
                stantionStartId: x.stantionStart.value,
                userIds: getUserIds(x.users)
            }))).then(r => {
                action();
                dispatch(new ToogleDialog(false))
                dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
            })
        })
    },
    getOutputStation: (userId, date: string, planedRouteTrainId: number, inputStationId: number): ActionCreator => (dispatch, getState) => {

        if (!inputStationId)
            dispatch(new NmpnCreateOrEdit({ stantionsEnd: [] }))
        else
            api.post('ScheludePlaned/GetOutputStation', {
                day: date,
                inputStationId: inputStationId,
                planedRouteTrainId: planedRouteTrainId,
                userId: userId
            }).then(result => {

                var endStantions: SelectItem[] = result.map(x => {
                    return { value: x.id, text: x.name }
                })

                dispatch(new NmpnCreateOrEdit({ stantionsEnd: endStantions }))
            })
    },
    removeTrains: (planedRouteTrainId: number,action): ActionCreator => (dispatch, getState) => {
        api.delete(`ScheludePlaned/PlanedRouteTrainsTable?id=${planedRouteTrainId}`, {}).then(result => {
            action()
            dispatch(new ToogleDialog(false))
            dispatch(new ShowTooltip('Данные успешно удалены!', 0))
        })
    }
}

const defaultState = {
    stantionsEnd: [],
    stantionsStart: [],
    planedRouteTrainId: undefined
}

export const reducer: Reducer<State> = (state, action: any) => {

    if (isActionType(action, NmpnCreateOrEdit))
        return { ...state, ...action.data }
    if (isActionType(action, NmpnUnloadState))
        return defaultState

    return state || defaultState
}