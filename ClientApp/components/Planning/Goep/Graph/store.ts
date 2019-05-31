import { typeName, isActionType, Action, Reducer } from 'redux-typed';
import { ActionCreator } from '../../../../store'
import api from '../../../../services/rest'
import store from '../../../../main'
import { ShowDeleteDialog, RequestEvent, RequestAction, ShowDialog, ToogleDialog, GridType } from '../../../../common'

export interface State {
}

export const actionCreators = {
    get: (id, skip, limit): ActionCreator => (dispatch, getState) => {
        api.get(`SheduleCycle/GetRoutesWithTimeline?turnoverId=${id}&skip=${skip}&limit=${limit}`)
            .then(result => {
                dispatch(new RequestAction(RequestEvent.Body, GridType.goep, result))
            })

        dispatch(new RequestAction(RequestEvent.Body))
    },
    getRouteById: (id: number, action): ActionCreator => (dispatch, getState) => {

        api.get(`Route/GetAll?skip=0&limit=1000&filter=[{"filter":"id","value":"${id}"}]`).then(result => {

            var res = result.data[0];

            dispatch(new ShowDialog({ mileage: res.mileage, name: res.name, id: id, turnoverId: res.turnoverId }, 0, 'graphaaddroute', true, action));
        })
    },
    addRoute: (data, action): ActionCreator => (dispatch, getState) => {
        api.post(`Route/AddOrUpdate`, data).then(result => {
            //api.get(`SheduleCycle/GetRoutesWithTimeline?turnoverId=${data.turnoverId}&skip=${0}&limit=${9999}`)
            //    .then(result => {
            //        dispatch(new RequestAction(RequestEvent.Body, true, result))
            //    })

            //dispatch(new RequestAction(RequestEvent.Body, false))
            action();
            dispatch(new ToogleDialog(false))
        })
    },
    addEvent: (data, action): ActionCreator => (dispatch, getState) => {
        api.post(`InspectionRoutes/AddOrUpdate`, data)
            .then(result => {
                action();
                dispatch(new ToogleDialog(false))
            })
    },
    removeRoute: (id, action): ActionCreator => (dispatch, getState) => {
        store.dispatch(
            new ShowDeleteDialog('', () => {
                api.get(`Route/ClearRouteTurnoverId?routeId=${id}`).then(() => {
                    action();
                })
            })
        )
    },
    removeEvent: (data: { id: number, enumType: number, tripOnRouteId: number }, action): ActionCreator => (dispatch, getState) => {

        //var url = data.enumType == 2
        //    ? `SheduleCycle/RemoveTripFromRoute?tripOnrouteId=${data.tripOnRouteId}`
        //    : `InspectionRoutes/Delete?id=${data.id}`

        var url = `SheduleCycle/RemoveTripFromRoute?tripOnrouteId=${data.tripOnRouteId}`

        store.dispatch(new ShowDeleteDialog('', () => {api.get(url).then(result => {
                    action();
                })
            })
        )
    },
    getTrips: (turnoverId, routeId, action): any => (dispatch, getState) => {
        return api.get(`SheduleCycle/TripsByTurnoverIdAndDays?turnoverId=${turnoverId}&routeId=${routeId}`)
            .then(result => {
                dispatch(new ShowDialog({ data: result, routeId: routeId}, 0, 'addTripRoute', true, action));
            })
    },
    addOrUpdateTripToRoute: (data: { routeId: number, tripIds: any[] }, action?): any => (dispatch, getState) => {
        return api
            .post(`SheduleCycle/AddOrUpdateTripToRoute`, data)
            .then(result => {
                    action();
                    dispatch(new ToogleDialog(false))
            })
    },
}

export const reducer: Reducer<State> = (state, action: any) => {
    return state || {}
}
