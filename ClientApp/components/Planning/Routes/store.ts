import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, findRowIdx,ShowTooltip,ShowDeleteDialog,ShowDialog,ToogleDialog} from '../../../common'
import { Storage, UserService } from '../../../services'
import api from '../../../services/rest'
import { ActionCreator } from '../../../store'
import store from '../../../main'

interface IData {
}

export interface State {
}

export const actionCreators = {
    getTripById: (id?, action?): ActionCreator => (dispatch, getState) => {
        api.get(`Trips/GetById?id=${id}`).then(result => {
            dispatch(new ShowDialog(result, 0, 'planning.tripCreate', true, action));
        })
    },
    addOrUpdateTripToRoute: (data,isEdit, action): ActionCreator => (dispatch, getState) => {

        var request;

        if (isEdit)
            request = api.put('dictionary/TripWithDays', data)
        else
            request = api.post(`SheduleCycle/AddOrUpdateTripToRoute`, data);

        request.then(result => {
            action();
            dispatch(new ToogleDialog(false))
            dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
        })
    },
    deleteTrip: (ids: number[], action): ActionCreator => (dispatch, getState) => {

        Promise.all(ids.map(x => api.put(`dictionary/TripWithDays`, { idToDelete: x }))).then(result => {
            action()
            dispatch(new ShowTooltip('Данные успешно удалены!', 0))
        })
    },
}

export const reducer: Reducer<State> = (state, action: any) => {

    return state || {}
}
