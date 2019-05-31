import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, findRowIdx, ShowTooltip, ShowDeleteDialog } from '../common'
import { Storage, UserService } from '../services'
import api from '../services/rest'
import { ActionCreator } from '.'
import store from '../main'

interface ISortOptions {
    column: string
    direction: number
}

interface IData {
    data: Array<any>
    total: number
    sortOptions: ISortOptions
}

export interface State {
    result: IData
    reload: boolean
    permissions: any
    roles: any
    sotr: any
    className?: string
}

@typeName('Get4444433445')
class Get extends Action {
    constructor(public result) {        
        super()
        this.result = result
    }
}

@typeName('GetAdministrationUsersSorted')
class GetSorted extends Action {
    constructor(public result) {
        super()
        this.result = result
    }
}

@typeName('Add444434344')
class Add extends Action {
    constructor(public result) {
        super()
    }
}

@typeName('Del544343434445')
class Del extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('ShowEdit343444445')
class ShowEdit extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('HideE3434dit44445')
class HideEdit extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('HideEdi343434tors44445')
export class HideEditors extends Action {
    constructor() {
        super()
    }
}

@typeName('getL8343434sdsds82')
class GetLinks extends Action {
    constructor(public result, public t) {
        super()
    }
}

export const actionCreators = {
    get: (skip, limit, p1?, sortOptions?): ActionCreator => (dispatch, getState) => {
        const filter =
            ((p1) &&
                '&filter=[' +
                    (p1 ? `{"filter":"name","value":"${p1}"}` : '') +
                ']') ||
            '';

        const sort = ((sortOptions) &&
            `&sort={"column" : "${sortOptions.column}", "direction" : "${sortOptions.direction}"}`) || '';
        api.get(`User/GetAllWithLogin?skip=${skip}&limit=${limit}${filter}${sort}`).then(result => {
            dispatch(new Get(result))
        })
    },
    getLinks: (): ActionCreator => (dispatch, getState) => {
        api.get(`UserRole/GetAll?skip=0&limit=1000`).then(res => {
            const roles =
                res &&
                res.data &&
                res.data.map(r => {
                    return { value: r.role.id, label: r.role.name }
                })
            dispatch(new GetLinks(roles, 'roles'))
        })

        api.get(`User/GetAllWithOutLogin?skip=0&limit=1000`).then(res => {
            const sotr =
                res &&
                res.data &&
                res.data.map(r => {
                    return { value: r.id, label: r.name }
                })
            dispatch(new GetLinks(sotr, 'sotr'))
        })
    },
    add: (data): ActionCreator => (dispatch, getState) => {
        api.post(`User/AddOrUpdate`, data).then(result => {
            dispatch(new Add(result))
            store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
        })
    },
    del: (id): ActionCreator => (dispatch, getState) => {
        store.dispatch(
            new ShowDeleteDialog('', () => {
                api.delete(`User/Delete`, { id: id }).then(() => {
                    dispatch(new Del(id))
                })
            })
        )
    },
    showEdit: (id): ActionCreator => (dispatch, getState) => {
        dispatch(new ShowEdit(id))
    },
    hideEdit: (id): ActionCreator => (dispatch, getState) => {
        dispatch(new HideEdit(id))
    },
    hideEditors: (): ActionCreator => (dispatch, getState) => {
        dispatch(new HideEditors())
    },
}

export const reducer: Reducer<State> = (state, action: any) => {
    if (isActionType(action, Get)) {
        return { ...state, result: action.result, reload: false }
    }

    if (isActionType(action, GetSorted)) {
        return { ...state, result: action.result, reload: false }
    }

    if (isActionType(action, GetLinks)) {
        return { ...state, [action.t]: action.result }
    }

    if (isActionType(action, Add)) {
        return { ...state, reload: true }
    }

    if (isActionType(action, Del)) {
        return { ...state, reload: true }
    }

    if (isActionType(action, ShowEdit)) {
        const result = JSON.parse(JSON.stringify(state.result))
        const idx = findRowIdx(result.data, action.id)
        result.data.forEach(r => {
            r.showEdit = false
        })
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
        result.data.forEach(r => {
            r.showAdd = false
            r.showEdit = false
            r.equipments &&
                r.equipments.forEach(ir => {
                    ir.showEdit = false
                })
        })
        return { ...state, result: result }
    }

    return state || { result: { data: [], total: 0 }, reload: false, permissions: [], roles: [], sotr: [] }
}
