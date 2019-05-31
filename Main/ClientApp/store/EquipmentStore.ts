import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, ShowTooltip, ShowDeleteDialog } from '../common'
import { Storage, UserService } from '../services'
import api from '../services/rest'
import { ActionCreator } from '.'
import store from '../main'
import { sortBy } from 'lodash'

interface IData {
  data: Array<any>
  total: number
}

export interface EquipmentState {
  result: IData
  reload: boolean
  fails: Array<any>
  expandedRows: any
  aktCategories: any
}

@typeName('GetCategoriesAction')
class GetCategoriesAction extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('AddCategoryAction')
class AddCategoryAction extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('DelCategoryAction')
class DelCategoryAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ExpandCategoryAction')
class ExpandCategoryAction extends Action {
  constructor(public id, public result) {
    super()
  }
}

@typeName('UnexpandCategoryAction')
class UnexpandCategoryAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ShowAddCategoryAction')
class ShowAddCategoryAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('HideAddCategoryAction')
class HideAddCategoryAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('AddEqCategoryAction')
class AddEqCategoryAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ShowEdit1')
class ShowEdit extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('HideEdit1')
class HideEdit extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ShowInnerEdit1')
class ShowInnerEdit extends Action {
  constructor(public id, public innerId, public fails) {
    super()
  }
}

@typeName('HideInnerEdit1')
class HideInnerEdit extends Action {
  constructor(public id, public innerId) {
    super()
  }
}

@typeName('GetFails')
class GetFails extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('GetFailsByEq')
class GetFailsByEq extends Action {
  constructor(public id, public result) {
    super()
  }
}

@typeName('AddFail')
class AddFail extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('HideEditors1')
export class HideEditors extends Action {
  constructor() {
    super()
  }
}

@typeName('GetAkts')
export class GetAkts extends Action {
  constructor(public result) {
    super()
  }
}

export const actionCreators = {
  getCategories: (skip, limit, p1?, p2?): ActionCreator => (dispatch, getState) => {
    const filter =
      ((p1 || p2) &&
        '&filter=[' +
          (p1 ? `{"filter":"name","value":"${p1}"},` : '') +
          (p2 ? `{"filter":"equipmentname","value":"${p2}"},` : '') +
          ']') ||
      ''
    api.get(`CategoryEquipment/GetAll?skip=${skip}&limit=${limit}${filter}`).then(result => {
      dispatch(new GetCategoriesAction(result))

      api.get(`Fault/GetAll?skip=0&limit=1000`).then(resultF => {
        let fails = []
        if (resultF.data) {
          fails = resultF.data.map(el => {
            return { label: el.name, value: el.id }
          })
          dispatch(new GetFails(fails))
        }
      })

      api.get(`ActCategory/GetAll`).then(resultF => {
        let fails = []
        if (resultF) {
          resultF = sortBy(resultF, [
            function(item) {
              return item.description
            },
            function(item) {
              return item.name
            }
          ])
          let s = ''
          let fails = []
          let i = -1
          resultF.forEach(el => {
            if (s !== el.description) {
              s = el.description
              i--
              fails.push({ label: el.description, value: i })
            }
            fails.push({ label: '---- ' + el.name, value: el.id })
          })
          dispatch(new GetAkts(fails))
        }
      })
    })
  },
  addCat: (data): ActionCreator => (dispatch, getState) => {
    api.post(`CategoryEquipment/Add`, data).then(result => {
      dispatch(new AddCategoryAction(result))
      store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
    })
  },
  delCat: (id): ActionCreator => (dispatch, getState) => {
    store.dispatch(
      new ShowDeleteDialog('', () => {
        api.delete(`CategoryEquipment/Delete`, { id: id }).then(() => {
          dispatch(new DelCategoryAction(id))
        })
      })
    )
  },
  expandRow: (id, p1, showAdd?): ActionCreator => (dispatch, getState) => {
    const filter = (p1 && '&filter=[' + (p1 ? `{"filter":"Name","value":"${p1}"},` : '') + ']') || ''
    api.get(`Equipment/GetByCategoryId?category_id=${id}&skip=0&limit=1000${filter}`).then(result => {
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
  addEq: (data, p1): ActionCreator => (dispatch, getState) => {
    api.post(`Equipment/Add`, data).then(resultAdd => {
      store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
      data.fails &&
        data.fails.forEach(fail => {
          api.post(`Equipment/AddFaultToEquipment`, { equipmentId: resultAdd.id, faultId: fail })
        })
      const filter = (p1 && '&filter=[' + (p1 ? `{"filter":"Name","value":"${p1}"},` : '') + ']') || ''
      api.get(`Equipment/GetByCategoryId?category_id=${data.CategoryId}&skip=0&limit=1000${filter}`).then(result => {
        dispatch(new ExpandCategoryAction(data.CategoryId, result))
      })
    })
  },
  delEq: (eqId, catId, p1): ActionCreator => (dispatch, getState) => {
    store.dispatch(
      new ShowDeleteDialog('', () => {
        const filter = (p1 && '&filter=[' + (p1 ? `{"filter":"Name","value":"${p1}"},` : '') + ']') || ''
        api.delete(`Equipment/Delete`, { id: eqId }).then(() => {
          api.get(`Equipment/GetByCategoryId?category_id=${catId}&skip=0&limit=1000${filter}`).then(result => {
            dispatch(new ExpandCategoryAction(catId, result))
          })
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
  showInnerEdit: (id, innerId): ActionCreator => (dispatch, getState) => {
    api.get(`Fault/GetByEquipmentId?id=${innerId}`).then(resultF => {
      dispatch(new ShowInnerEdit(id, innerId, resultF))
    })
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
  createFail: (catId, equipmentId, faultName): ActionCreator => (dispatch, getState) => {
    // api
    //   .post(`Fault/Add`, {
    //     Name: name,
    //     faultType: 2
    //   })
    //   .then(res => {
    api
      .get(`Equipment/AddNewFaultToEquipment?equipmentId=${equipmentId}&faultName=${faultName}&faultType=2`)
      // })
      .then(result => {
        store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
        api.get(`Fault/GetAll?skip=0&limit=1000`).then(resultF => {
          let fails = []
          if (resultF.data) {
            fails = resultF.data.map(el => {
              return { label: el.name, value: el.id }
            })
            dispatch(new GetFails(fails))
          }
        })
        api.get(`Fault/GetByEquipmentId?id=${equipmentId}`).then(resultF => {
          dispatch(new ShowInnerEdit(catId, equipmentId, resultF))
        })
      })
  },
  delFail: (catId, equipmentId, faultId): ActionCreator => (dispatch, getState) => {
    api.post(`Equipment/RemoveFaultFromEquipment`, { equipmentId, faultId }).then(result => {
      api.get(`Fault/GetByEquipmentId?id=${equipmentId}`).then(resultF => {
        dispatch(new ShowInnerEdit(catId, equipmentId, resultF))
      })
    })
  },
  hideEditors: (): ActionCreator => (dispatch, getState) => {
    dispatch(new HideEditors())
  }
}

export const reducer: Reducer<EquipmentState> = (state, action: any) => {
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
    // const result = JSON.parse(JSON.stringify(action.result))
    // state.expandedRows &&
    //   state.expandedRows.forEach(element => {
    //     const idx = result.data && result.data.findIndex(r => r.id === element)
    //     result.data && (result.data[idx].expanded = true)
    //   })
    return { ...state, result: action.result, reload: false }
  }

  if (isActionType(action, GetFails)) {
    return { ...state, fails: action.result, reload: false }
  }

  if (isActionType(action, GetAkts)) {
    return { ...state, aktCategories: action.result, reload: false }
  }

  // if (isActionType(action, GetFailsByEq)) {
  //   const result = JSON.parse(JSON.stringify(state.result))
  //   const idx = findRowIdx(result.data, action.id)
  //   const cat = result.data[idx]
  //   const eq = (findRowIdx.fails = action.result)
  //   return { ...state, result: result, reload: false }
  // }

  if (isActionType(action, AddCategoryAction)) {
    return { ...state, reload: true }
  }

  if (isActionType(action, DelCategoryAction)) {
    return { ...state, reload: true }
  }

  if (isActionType(action, ExpandCategoryAction)) {
    const result = JSON.parse(JSON.stringify(state.result))
    const idx = findRowIdx(result.data, action.id)
    result.data[idx].expanded = true
    result.data[idx].equipments = action.result.data
    result.data[idx].showAdd = false

    const expandedRows = state.expandedRows
    const er = expandedRows.find(r => r === action.id)
    !er && expandedRows.push(action.id)

    return { ...state, result: result, reload: false, expandedRows }
  }

  if (isActionType(action, UnexpandCategoryAction)) {
    const result = JSON.parse(JSON.stringify(state.result))
    const idx = findRowIdx(result.data, action.id)
    result.data[idx].expanded = false

    const expandedRows = state.expandedRows
    expandedRows.splice(expandedRows.indexOf(action.id), 1)

    return { ...state, result: result, reload: false, expandedRows }
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

  if (isActionType(action, ShowInnerEdit)) {
    const result = JSON.parse(JSON.stringify(state.result))
    const idx = findRowIdx(result.data, action.id)
    const innerIdx = findRowIdx(result.data[idx].equipments, action.innerId)
    hideEditors(result.data)
    result.data[idx].equipments[innerIdx].showEdit = true
    result.data[idx].equipments[innerIdx].fails = action.fails
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

  return state || { result: { data: [], total: 0 }, reload: false, fails: [], expandedRows: [], aktCategories: [] }
}
