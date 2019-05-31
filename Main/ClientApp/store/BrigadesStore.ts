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
}

@typeName('GetCategoriesAction2')
class GetCategoriesAction extends Action {
    constructor(public result) {
        super()
    }
}

@typeName('AddCategoryAction2')
class AddCategoryAction extends Action {
    constructor(public result) {
        super()
    }
}

@typeName('DelCategoryAction2')
class DelCategoryAction extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('ShowAddCategoryAction2')
class ShowAddCategoryAction extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('HideAddCategoryAction2')
class HideAddCategoryAction extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('ShowEdit2')
class ShowEdit extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('HideEdit2')
class HideEdit extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('HideEditors2')
export class HideEditors extends Action {
    constructor() {
        super()
    }
}

export const actionCreators = {
    getCategories: (skip, limit, p1?, p2?): ActionCreator => (dispatch, getState) => {
        const filter =
            ((p1 || p2) &&
                '&filter=[' +
                (p1 ? `{"filter":"name","value":"${p1}"},` : '') +
                (p2 ? `{"filter":"brigadeType","value":${p2}}` : '') +
                ']') ||
            ''
        api.get(`Brigade/GetAll?skip=${skip}&limit=${limit}${filter}`).then(result => {
            dispatch(new GetCategoriesAction(result))
        })
    },
    addCat: (data): ActionCreator => (dispatch, getState) => {
        api.post(`Brigade/Add`, data).then(result => {
            dispatch(new AddCategoryAction(result))
            store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
        })
    },
    delCat: (id): ActionCreator => (dispatch, getState) => {
        store.dispatch(
            new ShowDeleteDialog('', () => {
                api.delete(`Brigade/Delete`, { id: id }).then(() => {
                    dispatch(new DelCategoryAction(id))
                })
            })
        )
    },
    showAdd: (id): ActionCreator => (dispatch, getState) => {
        dispatch(new ShowAddCategoryAction(id))
    },
    hideAdd: (id): ActionCreator => (dispatch, getState) => {
        dispatch(new HideAddCategoryAction(id))
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

    function hideEditors(data) {
        data.forEach(r => {
            r.showAdd = false
            r.showEdit = false
            r.equipments &&
                r.equipments.forEach(ir => {
                    ir.showAdd = false
                    ir.showEdit = false
                })
        })
        return data
    }

    if (isActionType(action, GetCategoriesAction)) {
        return { ...state, result: action.result, reload: false }
    }

    if (isActionType(action, AddCategoryAction)) {
        return { ...state, reload: true }
    }

    if (isActionType(action, DelCategoryAction)) {
        return { ...state, reload: true }
    }

    if (isActionType(action, ShowAddCategoryAction)) {
        const result = JSON.parse(JSON.stringify(state.result))
        const idx = findRowIdx(result.data, action.id)
        hideEditors(result.data)
        result.data[idx].showAdd = true
        return { ...state, result: result, reload: false }
    }

    if (isActionType(action, HideAddCategoryAction)) {
        const result = JSON.parse(JSON.stringify(state.result))
        const idx = findRowIdx(result.data, action.id)
        result.data[idx].showAdd = false
        return { ...state, result: result, reload: false }
    }

    if (isActionType(action, ShowEdit)) {
        const result = JSON.parse(JSON.stringify(state.result))
        const idx = findRowIdx(result.data, action.id)
        hideEditors(result.data)
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
        hideEditors(result.data)
        return { ...state, result: result }
    }

    return state || { result: { data: [], total: 0 }, reload: false }
}
