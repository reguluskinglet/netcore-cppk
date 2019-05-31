import { Reducer } from 'redux-typed'
import { ActionCreator } from '../../../store'
import api from '../../../services/rest'
import store from '../../../main'
import { ShowDeleteDialog, RequestAction, RequestEvent, GridType, ShowDialog, TimelineTypeEnum, ToogleDialog } from '../../../common'
import moment from 'moment'

export interface State {
    date?: any
}

export const actionCreators = {
    get: (date?): ActionCreator => (dispatch, getState) => {

        var currentDate = date ? date : new Date();

        var requestDate = moment(currentDate,'YYYY-MM-DDTHH:mm:ss').format('YYYY-MM-DDT00:00:00') + 'Z'

        api.post(`ChangedShelude/GetTimeRange`, { date: requestDate }).then(result => {
            dispatch(new RequestAction(RequestEvent.Body, GridType.pou, { data: result, date: date }))
        })

        dispatch(new RequestAction(RequestEvent.Body))

        //api.get(`/Service/GetTimeRange`).then(result => {
        //    dispatch(new RequestAction(RequestEvent.Body, GridType.pou, { data: result, date: date}))
        //})

        //dispatch(new RequestAction(RequestEvent.Body))
    },
    getTimeRangeData: (id: number, type: TimelineTypeEnum, routeId: number, dialogName, action): ActionCreator => (dispatch, getState) => {

        api.post(`ChangedShelude/GetRouteInformationTable`, { planedRouteTrainId: routeId, timelineTypeEnum: type, entityId: id}).then(result => {

            dispatch(new ShowDialog({ timeLineType: type, result: result }, 0, dialogName, true, action));
        })
    },
    changeTimeLine: (data, type: TimelineTypeEnum, action): ActionCreator => (dispatch, getState) => {

        var url = null;

        switch (type) {
            case TimelineTypeEnum.TimeRangeTo2:
            case TimelineTypeEnum.TimeSto:
                url = 'ChangedShelude/ChangeInspection';
                break;
            case TimelineTypeEnum.TimeBrigade:
                url = 'ChangedShelude/ChangeBrigade';
                break;
            case TimelineTypeEnum.TimeRangeTrip:
            case TimelineTypeEnum.TimeRangeTripTransfer:
                url = 'ChangedShelude/ChangeStantion';
                break;
            default: {
                console.log(data)
                return;
            }
        }

        api.post(url, data).then(result => {
            action();
            dispatch(new ToogleDialog(false))
        })
    }
}

export const reducer: Reducer<State> = (state, action: any) => {

    return state || {}
}
