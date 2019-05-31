import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, ShowTooltip, ShowDeleteDialog } from '../common'
import { Storage, UserService } from '../services'
import api from '../services/rest'
import { ActionCreator } from '.'
import store from '../main'

interface IData {
    data: Array<any>
    total: number
}

export interface State {
    result: IData
    reload: boolean
    brigades: Array<any>
}

@typeName('GetEmployeesAction')
class GetEmployeesAction extends Action {
    constructor(public result) {
        super()
    }
}

@typeName('GetEmployeeBrigades')
class GetEmployeeBrigades extends Action {
    constructor(public result) {
        super()
    }
}

@typeName('AddEmployeeAction')
class AddEmployeeAction extends Action {
    constructor(public result) {
        super()
    }
} 

@typeName('DeleteEmployeeAction')
class DeleteEmployeeAction extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('ShowAddEmployeeAction')
class ShowAdd extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('HideAddEmployeeAction')
class HideAdd extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('ShowEditEmployeeAction')
class ShowEdit extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('HideEditEmployeeAction')
class HideEdit extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('HideEditorsEmployeeAction')
export class HideEditors extends Action {
    constructor() {
        super()
    }
}

export const actionCreators = {
    getEmployees: (skip, limit, p1?, p2?): ActionCreator => (dispatch, getState) => {
        const filter =
            ((p1 || p2) &&
                '&filter=[' +
                (p1 ? `{"filter":"name","value":"${p1}"},` : '') +
                (p2 ? `{"filter":"personNumber","value":${p2}}` : '') +
                ']') ||
            ''
        api.get(`User/GetAll?skip=${skip}&limit=${limit}${filter}`).then(result => {
            dispatch(new GetEmployeesAction(result))
            api.get(`Brigade/GetBrigades`).then(resultB => {
                let brigades = []
                if (resultB) {
                    brigades = resultB.map(el => {
                        return { label: el.name, value: el.id }
                    })
                    dispatch(new GetEmployeeBrigades(brigades))
                }
            })
        })
    },
    addEmployee: (data): ActionCreator => (dispatch, getState) => {
        api.post(`User/AddStaff`, data).then(result => {
            dispatch(new AddEmployeeAction(result))
            store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
        })
    },
    deleteEmployee: (userId): ActionCreator => (dispatch, getState) => {
        store.dispatch(
            new ShowDeleteDialog('', () => {
                api.delete(`User/DeleteStaff`, { userId: userId }).then(() => {
                    dispatch(new DeleteEmployeeAction(userId))
                })
            })
        )
    },
    showAdd: (id): ActionCreator => (dispatch, getState) => {
        dispatch(new ShowAdd(id))
    },
    hideAdd: (id): ActionCreator => (dispatch, getState) => {
        dispatch(new HideAdd(id))
    },
    showEdit: (id): ActionCreator => (dispatch, getState) => {
        dispatch(new ShowEdit(id))
    },
    hideEdit: (id): ActionCreator => (dispatch, getState) => {
        dispatch(new HideEdit(id))
    },
    hideEditors: (): ActionCreator => (dispatch, getState) => {
        dispatch(new HideEditors())
    }
}

export const reducer: Reducer<State> = (state, action: any) => {
    function findRowIdx(data, id) {
        const row = data.find(r => r.id === id)
        const idx = data.indexOf(row)
        return idx
    }

    function hideEditors(result) {
        return result.data && result.data.forEach(r => {
            r.showAdd = false
            r.showEdit = false
        })
    }

    if (isActionType(action, GetEmployeesAction)) {
        return { ...state, result: action.result, reload: false }
    }

    if (isActionType(action, GetEmployeeBrigades)) {
        return { ...state, brigades: action.result, reload: false }
    }

    if (isActionType(action, AddEmployeeAction)) {
        return { ...state, reload: true }
    }

    if (isActionType(action, DeleteEmployeeAction)) {
        return { ...state, reload: true }
    }
    if (isActionType(action, ShowAdd)) {
        const result = JSON.parse(JSON.stringify(state.result))
        const idx = findRowIdx(result.data, action.id)
        hideEditors(result)
        result.data[idx].showAdd = true
        return { ...state, result: result, reload: false }
    }

    if (isActionType(action, HideAdd)) {
        const result = JSON.parse(JSON.stringify(state.result))
        const idx = findRowIdx(result.data, action.id)
        hideEditors(result)
        result.data[idx].showAdd = false
        return { ...state, result: result, reload: false }
    }
    if (isActionType(action, ShowEdit)) {
        const result = JSON.parse(JSON.stringify(state.result))
        const idx = findRowIdx(result.data, action.id)
        hideEditors(result)
        result.data[idx].showEdit = true
        return { ...state, result: result }
    }

    if (isActionType(action, HideEdit)) {
        const result = JSON.parse(JSON.stringify(state.result))
        const idx = findRowIdx(result.data, action.id)
        result.data[idx].showEdit = false
        return { ...state, result: result }
    }

    if (isActionType(action, HideEditors)) {
        const result = JSON.parse(JSON.stringify(state.result))
        hideEditors(result)
        return { ...state, result: result }
    }

    return state || { result: { data: [], total: 0 }, reload: false, brigades: [] }
}
