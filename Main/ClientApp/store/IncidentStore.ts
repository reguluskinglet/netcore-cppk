import { typeName, isActionType, Action, Reducer } from 'redux-typed';
import { ActionCreator } from '.';
import { RequestEvent } from '../common'
import { UserService, Storage } from '../services';
import { push } from 'react-router-redux'
import api from '../services/rest'

export interface IncidentState {
    message?: string
}

@typeName("ReciveIncidentAction")
class ReciveIncidentAction extends Action {
    constructor
        (
        public message
    ) {
        super();
    }
}


export const actionCreators = {
    getData: (): ActionCreator => (dispatch, getState) => {
        api.get('getData').then(data => {
            dispatch(new ReciveIncidentAction(data.message))
        })
    }
};


export const reducer: Reducer<IncidentState> = (state, action: any) => {

    if (isActionType(action, ReciveIncidentAction))
        return { message: action.message };

    return state || { message: undefined };
};