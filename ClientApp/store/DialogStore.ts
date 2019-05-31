import { typeName, isActionType, Action, Reducer } from 'redux-typed';
import { ActionCreator } from './';
import { ShowDialog, ToogleDialog } from '../common'

export interface DialogState {
    open: boolean
    type: string
    responseMessage: any
    status: number

    action?: (...data) => void

    errorMessage: string
}

export const actionCreators = {
    toggleDialog: (open?: boolean): ActionCreator => (dispatch, getState) => {
        dispatch(new ToogleDialog(open))
    },
    showDialog: (name, data?,action?): ActionCreator => (dispatch, getState) => {
        dispatch(new ShowDialog(data, 0, name, true, action));
    }
}

export const reducer: Reducer<DialogState> = (state, action: any) => {

    if (isActionType(action, ShowDialog))
        return { errorMessage: null, open: action.open, responseMessage: action.responceMessage, type: action.typeDialog, status: action.status, action: action.action };
    else if (isActionType(action, ToogleDialog))
        return { ...state, open: action.open };
    else
        return state || { open: false, type: null, responseMessage: null, status: null, errorMessage: null, };
};