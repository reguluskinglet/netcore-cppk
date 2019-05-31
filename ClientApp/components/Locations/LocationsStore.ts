import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, ShowTooltip, ShowDeleteDialog } from '../../common'
import { Storage, UserService } from '../../services'
import api from '../../services/rest'
import { ActionCreator } from '../../store'
import store from '../../main'
import _, { forEach, keys } from 'lodash'
import * as remove from 'lodash/remove'

interface IData {
    data: Array<any>
    total: number
    model: any
}

export interface State {
    result: IData
    reload: boolean
    equipments: Array<any>
    modelId: number
    expandedRows: any
    equipments2: any
    rows: any
    showAddId: number
}

@typeName('GetCategoriesAction3')
class GetCategoriesAction extends Action {
    constructor(public result) {
        super()
    }
}

@typeName('AddCategoryAction3')
class AddCategoryAction extends Action {
    constructor(public result) {
        super()
    }
}

@typeName('DelCategoryAction3')
class DelCategoryAction extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('ExpandCategoryAction3')
class ExpandCategoryAction extends Action {
    constructor(public id, public result) {
        super()
    }
}

@typeName('UnexpandCategoryAction3')
class UnexpandCategoryAction extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('ShowAddCategoryAction3')
class ShowAddCategoryAction extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('HideAddCategoryAction3')
class HideAddCategoryAction extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('AddEqCategoryAction3')
class AddEqCategoryAction extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('ShowEdit13')
class ShowEdit extends Action {
    constructor(public id, public cl) {
        super()
    }
}

@typeName('HideEdit13')
class HideEdit extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('ShowInnerEdit13')
class ShowInnerEdit extends Action {
    constructor(public id, public innerId, public fails) {
        super()
    }
}

@typeName('HideInnerEdit13')
class HideInnerEdit extends Action {
    constructor(public id, public innerId) {
        super()
    }
}

@typeName('GetFails3')
class GetFails extends Action {
    constructor(public result) {
        super()
    }
}

@typeName('GetFailsByEq3')
class GetFailsByEq extends Action {
    constructor(public id, public result) {
        super()
    }
}

@typeName('AddFail3')
class AddFail extends Action {
    constructor(public result) {
        super()
    }
}

@typeName('HideEditors3')
export class HideEditors extends Action {
    constructor() {
        super()
    }
}

export const actionCreators = {
    getCategories: (modelId, parentId, skip, limit, p1?, p2?): ActionCreator => (dispatch, getState) => {
        const filter =
            (p1 &&
                '&filter=[' +
                (p1 ? `{"filter":"name","value":"${p1}"},` : '') +
                // (p2 ? `{"filter":"faultType","value":${p2}}` : '') +
                ']') || ''

        api
            .get(`Equipment/GetEquipmentWithCheckLists?model_id=${modelId}&skip=${skip}&limit=${limit}${filter}`)
            .then(result => {
                // api.get(`CheckListEquipment/GetList?equipmentModelId=${modelId}`).then(cl=>{
                //   debugger
                dispatch(new GetCategoriesAction(result))
                // })

                api.get(`Equipment/GetAll?skip=0&limit=1000`).then(resultF => {
                    let fails = []
                    if (resultF.data) {
                        fails = resultF.data.map(el => {
                            return { label: el.name, value: el.id }
                        })
                        dispatch(new GetFails(fails))
                    }
                })
            })
    },
    add: (data, id?): ActionCreator => (dispatch, getState) => {
        // const location = { ...data, algorithms: [...data.algoritm0, ...data.algoritm1, ...data.algoritm2] }
        api.post(`Equipment/AddUpdateEquipmentWithCheckLists`, data).then(result => {
            store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
            if (!id) {
                dispatch(new AddCategoryAction(result))
            } else {
                api
                    .get(`Equipment/GetEquipmentWithCheckLists?model_id=${data.modelId}&parent_id=${id}&skip=0&limit=1000`)
                    .then(result1 => {
                        dispatch(new ExpandCategoryAction(id, result1))
                    })
            }
        })
    },
    del: (data, id): ActionCreator => (dispatch, getState) => {
        store.dispatch(
            new ShowDeleteDialog('', () => {
                api.delete(`Model/DeleteEquipmentFromModel`, { id: id }).then(() => {
                    dispatch(new DelCategoryAction(id))
                    api
                        .get(`Equipment/GetEquipmentWithCheckLists?model_id=${data.modelId}&parent_id=${data.parentId}&skip=0&limit=1000`)
                        .then(result1 => {
                            dispatch(new ExpandCategoryAction(data.parentId, result1))
                        })
                })
            })
        )
    },
    expandRow: (id, modelId, parentId, p1, showAdd?): ActionCreator => (dispatch, getState) => {
        const filter =
            (p1 &&
                '&filter=[' +
                (p1 ? `{"filter":"name","value":"${p1}"},` : '') +
                // (p2 ? `{"filter":"faultType","value":${p2}}` : '') +
                ']') ||
            ''
        api
            .get(`Equipment/GetEquipmentWithCheckLists?model_id=${modelId}&parent_id=${parentId}&skip=0&limit=1000${filter}`)
            .then(result => {
                dispatch(new ExpandCategoryAction(id, result))
                if (showAdd) dispatch(new ShowAddCategoryAction(id))
            })
    },
    unexpandRow: (id): ActionCreator => (dispatch, getState) => {
        dispatch(new UnexpandCategoryAction(id))
    },
    showAdd: (id): ActionCreator => (dispatch, getState) => {
        dispatch(new ShowAddCategoryAction(id))
    },
    hideAdd: (id): ActionCreator => (dispatch, getState) => {
        dispatch(new HideAddCategoryAction(id))
    },
    addLocation: (data): ActionCreator => (dispatch, getState) => {
        api.post(`Equipment/Add`, data).then(resultAdd => {
            data.fails &&
                data.fails.forEach(fail => {
                    api.post(`Equipment/AddFaultToEquipment`, { equipmentId: resultAdd.id, faultId: fail })
                })
            api.get(`Equipment/GetByCategoryId?category_id=${data.CategoryId}&skip=0&limit=1000`).then(result => {
                dispatch(new ExpandCategoryAction(data.CategoryId, result))
            })
        })
    },
    delEq: (modelId, eqId, row): ActionCreator => (dispatch, getState) => {
        store.dispatch(
            new ShowDeleteDialog('', () => {
                api.delete(`Model/DeleteEquipmentFromModel`, { id: eqId }).then(() => {
                    api
                        .get(`Equipment/GetEquipmentWithCheckLists?model_id=${modelId}&parent_id=${row.parentId}&skip=0&limit=1000`)
                        .then(result => {
                            dispatch(new ExpandCategoryAction(row.parentId, result))
                        })
                })
            })
        )
    },
    showEdit: (id): any => async (dispatch, getState) => {
        const cl = await api.get(`CheckListEquipment/GetList?equipmentModelId=${id}`);
        dispatch(new ShowEdit(id, cl));
        return cl;
    },
    hideEdit: (id): ActionCreator => (dispatch, getState) => {
        dispatch(new HideEdit(id))
    },
    showInnerEdit: (id, innerId): ActionCreator => (dispatch, getState) => {
        // api.get(`Fault/GetByEquipmentId?id=${innerId}`).then(resultF => {
        dispatch(new ShowInnerEdit(id, innerId, null))
        // })
    },
    hideInnerEdit: (id, innerId): ActionCreator => (dispatch, getState) => {
        dispatch(new HideInnerEdit(id, innerId))
    },
    addFail: (catId, equipmentId, faultId): ActionCreator => (dispatch, getState) => {
        api.post(`Equipment/AddFaultToEquipment`, { equipmentId, faultId }).then(result => {
            api.get(`Fault/GetByEquipmentId?id=${equipmentId}`).then(resultF => {
                dispatch(new ShowInnerEdit(catId, equipmentId, resultF))
            })
        })
    },
    delFail: (catId, equipmentId, faultId): ActionCreator => (dispatch, getState) => {
        store.dispatch(
            new ShowDeleteDialog('', () => {
                api.post(`Equipment/RemoveFaultFromEquipment`, { equipmentId, faultId }).then(result => {
                    api.get(`Fault/GetByEquipmentId?id=${equipmentId}`).then(resultF => {
                        dispatch(new ShowInnerEdit(catId, equipmentId, resultF))
                    })
                })
            })
        )
    },
    hideEditors: (): ActionCreator => (dispatch, getState) => {
        dispatch(new HideEditors())
    },
    addAlg: (id, data): any => async dispatch => {
        const res = await api.post(`CheckListEquipment/AddChecklist`, {
            equipmentModelId: id,
            ...data,
            taskLevel: 2
        })
        return res
    },
    delAlg: (id): any => async dispatch => {
        await api.post(`CheckListEquipment/DeleteChecklist?id=${id}`, {
            equipmentModelId: id
        })
        return id
    }
}

export const reducer: Reducer<State> = (state, action: any) => {
    function findRowIdx(data, id) {
        const row = data.find(r => r.id === id)
        const idx = data.indexOf(row)
        return idx
    }

    function hideEditors(data) {
        forEach(keys(data), id => {
            data[id].showAdd = false
            data[id].showEdit = false
            data[id].equipments &&
                data[id].equipments.forEach(ir => {
                    ir.showAdd = false
                    ir.showEdit = false
                })
        })
        return data
    }

    if (isActionType(action, GetCategoriesAction)) {
        const data = _.keyBy(action.result.data, 'id')
        return { ...state, result: action.result, reload: false, rows: data }
    }

    if (isActionType(action, GetFails)) {
        return { ...state, equipments: action.result, reload: false }
    }

    // if (isActionType(action, GetFailsByEq)) {
    //   const result = JSON.parse(JSON.stringify(state.result))
    //   const idx = findRowIdx(result.data, action.id)
    //   const cat = result.data[idx]
    //   const eq = (findRowIdx.fails = action.result)
    //   return { ...state, result: result, reload: false }
    // }

    if (isActionType(action, AddCategoryAction)) {
        return { ...state, reload: true, showAddId: -1 }
    }

    if (isActionType(action, DelCategoryAction)) {
        return { ...state, reload: false }
    }

    if (isActionType(action, ExpandCategoryAction)) {
        const result = JSON.parse(JSON.stringify(state.result))
        const data = [...state.result.data]
        const { id } = action
        const equipments = action.result.data

        const { equipments2 = {}, rows = {} } = state
        const parentId = _.get(action.result, 'data[0].parentId')
        parentId && (equipments2[parentId] = equipments)
        const row = rows[id]
        const updatedRow = { ...row, equipments, expanded: true, showAdd: false }
        const newRows = { ...rows, [id]: updatedRow, ..._.keyBy(equipments, 'id') }

        const expandedRows = [...state.expandedRows]
        expandedRows.push(row)

        return {
            ...state,
            result: result,
            reload: false,
            equipments2,
            rows: newRows,
            showAddId: -1,
            expandedRows: expandedRows
        }
    }

    if (isActionType(action, UnexpandCategoryAction)) {
        const { rows = {} } = state
        const row = rows[action.id]
        const updatedRow = { ...row, expanded: false }

        const expandedRows = [...state.expandedRows]
        remove(expandedRows, x => x.id == action.id)

        return { ...state, reload: false, rows: { ...rows, [action.id]: updatedRow }, expandedRows: expandedRows }
    }

    if (isActionType(action, ShowAddCategoryAction)) {
        const { rows = {} } = state
        const row = rows[action.id]
        const updatedRow = { ...row, expanded: true }

        return { ...state, reload: false, rows: { ...rows, [action.id]: updatedRow }, showAddId: action.id }
    }

    if (isActionType(action, HideAddCategoryAction)) {
        const result = JSON.parse(JSON.stringify(state.result))
        const idx = findRowIdx(result.data, action.id)
        result.data[idx].showAdd = false
        return { ...state, result: result, reload: false }
    }

    if (isActionType(action, ShowEdit)) {
        // const result = JSON.parse(JSON.stringify(state.result))
        // let idx = findRowIdx(result.data, action.id)
        // hideEditors(result.data)
        // result.data[idx].showEdit = true

        // debugger
        const rows = JSON.parse(JSON.stringify(state.rows))
        hideEditors(rows)
        rows[action.id].showEdit = true
        rows[action.id].algorithms = action.cl

        return {
            ...state, //result: result,
            rows
        }
    }

    if (isActionType(action, HideEdit)) {
        // const result = JSON.parse(JSON.stringify(state.result))
        // const idx = findRowIdx(result.data, action.id)
        // result.data[idx].showEdit = false
        // return { ...state, result: result }

        const rows = JSON.parse(JSON.stringify(state.rows))
        hideEditors(rows)
        rows[action.id].showEdit = false

        return {
            ...state, //result: result,
            rows
        }
    }

    if (isActionType(action, ShowInnerEdit)) {
        const result = JSON.parse(JSON.stringify(state.result))
        const idx = findRowIdx(result.data, action.id)
        const innerIdx = findRowIdx(result.data[idx].equipments, action.innerId)
        hideEditors(result.data)
        result.data[idx].equipments[innerIdx].showEdit = true
        // result.data[idx].equipments[innerIdx].fails = action.fails

        debugger
        const rows = JSON.parse(JSON.stringify(state.rows))
        // hideEditors(rows)
        rows[action.id].showEdit = true

        return { ...state, result: result }
    }

    if (isActionType(action, HideInnerEdit)) {
        const result = JSON.parse(JSON.stringify(state.result))
        const idx = findRowIdx(result.data, action.id)
        const innerIdx = findRowIdx(result.data[idx].equipments, action.innerId)
        result.data[idx].equipments[innerIdx].showEdit = false
        return { ...state, result: result }
    }

    if (isActionType(action, HideEditors)) {
        const result = JSON.parse(JSON.stringify(state.result))
        hideEditors(result.data)
        return { ...state, result: result }
    }

    return (
        state || {
            result: { data: [], total: 0, model: {} },
            reload: false,
            equipments: [],
            modelId: null,
            expandedRows: [],
            equipments2: {},
            rows: {},
            showAddId: -1
        }
    )
}
