import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, findRowIdx, ShowTooltip, ShowDeleteDialog, RequestAction, ShowDialog, ToogleDialog } from '../../../common'
import { Storage, UserService } from '../../../services'
import api from '../../../services/rest'
import { ActionCreator } from '../../../store'

export interface State {

}

export const actionCreators = {
    getTurnoverById: (id: number, action): ActionCreator => (dispatch, getState) => {

        api.get(`Turnovers/GetTurnoverById?turnoverId=${id}`).then(result => {

            dispatch(new ShowDialog(result, 0, 'planning.goepCreate', true, action));
        })
    },
    saveTurnoverWithDays: (data, action): ActionCreator => (dispatch, getState) => {

        api.post(`SheduleCycle/TurnoverWithDays`, data).then(result => {

            action()
            dispatch(new ToogleDialog(false))
            dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
        })
    },
    deleteTurnoverWithDays: (ids: number[],action): ActionCreator => (dispatch, getState) => {
        Promise.all(ids.map(x => api.delete(`SheduleCycle/TurnoverWithDays?id=${x}`, {}))).then(result => {
            action()
            dispatch(new ShowTooltip('Данные успешно удалены!', 0))
        })
    },

}

export const reducer: Reducer<State> = (state, action: any) => {
    return state || {}
}
