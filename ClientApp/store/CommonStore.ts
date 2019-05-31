import { typeName, isActionType, Action, Reducer } from 'redux-typed'
import { ActionCreator } from '.'
import { RequestEvent, RequestAction, ShowTooltip, HideTooltip, ShowDeleteDialog, LoadingOverlay, GridType } from '../common'

export interface CommonState {
    requestEvent: RequestEvent
    message?: any
    t?: number
    show?: boolean
    callback?: any

    isShowLoadingOverlay: boolean

    isLoaded?: GridType
    data:any
}

export const actionCreators = {
    hideTooltip: (): ActionCreator => (dispatch, getState) => {
        dispatch(new HideTooltip())
    },
    showLoading: (isShow: boolean): ActionCreator => (dispatch, getState) => {
        dispatch(new LoadingOverlay(isShow))
    }
}

export const reducer: Reducer<CommonState> = (state, action: any) => {

    if (isActionType(action, LoadingOverlay))
        return { ...state, isShowLoadingOverlay: action.showLoading }
    if (isActionType(action, RequestAction))
        return { ...state, requestEvent: action.event, isLoaded: action.showLoading, data: action.data }
    if (isActionType(action, ShowTooltip))
        return { ...state, message: action.message, t: action.t, show: true }
    if (isActionType(action, HideTooltip))
        return { ...state, show: false }
    if (isActionType(action, ShowDeleteDialog))
        return { ...state, message: action.message, callback: action.callback }

    return state || { requestEvent: RequestEvent.None, isShowLoadingOverlay: false, isLoaded: null, data: null }
}
