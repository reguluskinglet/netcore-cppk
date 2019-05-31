import { typeName, isActionType, Action, Reducer } from 'redux-typed';
import { ActionCreator } from '.';
import { RequestEvent} from '../common'
import { UserService, Storage } from '../services';
import { push } from 'react-router-redux'

export interface UsersState {
    requestEvent: RequestEvent,
    errorMessage?: string
}

@typeName("LoginUser")
class LoginUserAction extends Action {
    constructor
        (
        public requestEvent = RequestEvent.None,
        public errorMessage?: string,

    ) {
        super();
    }
}


export const actionCreators = {
    logIn: (login, password, redirectTo): ActionCreator => (dispatch, getState) => {
        UserService.auth(login, password).then((data: IAuthResult) => {
            dispatch(push(redirectTo))
        }).fail(response => {
            var status = response.status

            if (status == 401)
                dispatch(new LoginUserAction(RequestEvent.None, "Неверное имя пользователя или пароль"))
        })

        dispatch(new LoginUserAction(RequestEvent.Start))
    }
};


export const reducer: Reducer<UsersState> = (state, action: any) => {

    if (isActionType(action, LoginUserAction))
        return { requestEvent: action.requestEvent, errorMessage: action.errorMessage };

    return state || { requestEvent: RequestEvent.None };
};