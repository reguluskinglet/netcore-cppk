import { Reducer, isActionType } from 'redux-typed'
import { ActionCreator } from '../../../store'
import api from '../../../services/rest'
import store from '../../../main'
import { ShowDeleteDialog, ShowDialog, ToogleDialog } from '../../../common'
import { typeName, Action } from 'redux-typed'

export interface State {
    inspections: SelectItem[]
}

@typeName("DepoEvents.CreateOrEdit")
export class DepoEventsCreateOrEdit extends Action {
    constructor
        (
            public inspections: SelectItem[]
        ) {
        super();

    }
}

export const actionCreators = {
    addDispatcher: (id: number, action, dataPou?): ActionCreator => (dispatch, getState) => {
        api.get(`DepoEvents/GetById?id=${id}`).then(data=> {
            dispatch(new ShowDialog({ result: data, params: dataPou }, 0, 'dispatcher.createOrEdit', true, action));
            dispatch(new DepoEventsCreateOrEdit(data.depoEventDataSource.inspections))
        })
    },
    saveOrUpdateDispatcher: (data, action): ActionCreator => (dispatch, getState) => {
        api.post(`DepoEvents/AddOrUpdate`, data).then(data => {
            action();
            dispatch(new ToogleDialog(false))
        })
    },
    deleteDispatcher: (data, action): ActionCreator => (dispatch, getState) => {
        api.post(`DepoEvents/Delete`, data).then(data => {
            action();
        })
    },
    getDispatcherInspectionByTrainId: (trainId): ActionCreator => (dispatch, getState) => {
        api.get(`DepoEvents/GetInspections?trainId=${trainId}`).then(data => {
            dispatch(new DepoEventsCreateOrEdit(data))
        })
    }
}

export const reducer: Reducer<State> = (state, action: any) => {

    if (isActionType(action, DepoEventsCreateOrEdit))
        return { inspections: action.inspections }

    return state || { inspections:[] }
}
