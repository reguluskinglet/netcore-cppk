import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, ShowTooltip, ShowDeleteDialog } from '../common'
import { Storage, UserService } from '../services'
import api from '../services/rest'
import { ActionCreator } from '.'
import store from '../main'

const modelTypeLabels = ['Головной вагон', 'Моторный вагон', 'Прицепной вагон']

interface IData {
  data: Array<any>
  total: number
}

export interface State {
  result: IData
  reload: boolean
  stations: Array<any>
  models: Array<any>
  expandedRows: any
  routes: any
}

@typeName('GetCategoriesadasdsadsAction6')
class GetCategoriesAction extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('AddCategoqedvbrfgbdfgbryAction6')
class AddCategoryAction extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('DelCategoryAdfgbdfgbdfction6')
class DelCategoryAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ExpabfdgbndCategoryfgbdfgbdAction6')
class ExpandCategoryAction extends Action {
  constructor(public id, public result) {
    super()
  }
}

@typeName('UnexpandCategodfgbfdgbryAction6')
class UnexpandCategoryAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ShowAddCdfgbdfgbategoryAction6')
class ShowAddCategoryAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('HideAddCategoryAcdfgbdfgbdfgbtion6')
class HideAddCategoryAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('AddEqCategoryAdfgbdfgbdfgbdfgbction6')
class AddEqCategoryAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ShowEfdgbdfgbdfgbdfgbdit16')
class ShowEdit extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('HideEdbdfgbdfgbdfgbdfbdfit16')
class HideEdit extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ShowInnerEgbfdgbdfgdfgbdgbfddit16')
class ShowInnerEdit extends Action {
  constructor(public id, public innerId, public data) {
    super()
  }
}

@typeName('HideInnerEgbdfgbdfgbdfgbdfgbdfdit16')
class HideInnerEdit extends Action {
  constructor(public id, public innerId) {
    super()
  }
}

@typeName('GetStatidfgbdfgbdfgons6')
class GetStations extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('GetFailsbdfgbdfgbdfgbByEq6')
class GetFailsByEq extends Action {
  constructor(public id, public result) {
    super()
  }
}

@typeName('AddFadfgbdfbgfbdfgbdfgbil6')
class AddFail extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('GetModdfgbdfgbdfgbdfels6')
class GetModels extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('HideEdigbdfgbdfbdfbtors6')
export class HideEditors extends Action {
  constructor() {
    super()
  }
}

@typeName('getL8343434asdasdasdsadasdsadsdsds82')
class GetLinks extends Action {
  constructor(public result, public t) {
    super()
  }
}

export const actionCreators = {
  getCategories: (skip, limit, p1?, p2?): ActionCreator => (dispatch, getState) => {
    const filter =
      ((p1 || p2) &&
        '&filter=[' +
        (p1 ? `{"filter":"RouteId","value":"${p1}"},` : '') +
        (p2 ? `{"filter":"stantionId","value":${p2}}` : '') +
        ']') ||
      ''
    api.get(`Trips/GetAll?skip=${skip}&limit=${limit}${filter}`).then(result => {
      dispatch(new GetCategoriesAction(result))
      const state = getState()
      state.scheduleEvents.expandedRows.forEach(element => {
        dispatch(new ExpandCategoryAction(element, {}))
      })
    })
  },
  getLinks: (type?, id?): ActionCreator => (dispatch, getState) => {
    api.get(`Route/GetAll?skip=0&limit=1000`).then(res => {
      const arr =
        res &&
        res.map(r => {
          return { value: r.id, label: r.name }
        })
      dispatch(new GetLinks(arr, 'routes'))
    })
    // api.get(`User/GetAllWithOutLogin?skip=0&limit=1000`).then(res => {
    //   const sotr =
    //     res &&
    //     res.data &&
    //     res.data.map(r => {
    //       return { value: r.id, label: r.name }
    //     })
    //   dispatch(new GetLinks(sotr, 'sotr'))
    // })
  },
  addCat: (data): ActionCreator => (dispatch, getState) => {
    api.post(`Train/Add`, data).then(result => {
      dispatch(new AddCategoryAction(result))
      store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
    })
  },
  delCat: (id): ActionCreator => (dispatch, getState) => {
    api.delete(`Train/Delete`, { id: id }).then(() => {
      dispatch(new DelCategoryAction(id))
    })
  },
  expandRow: (id, showAdd?): ActionCreator => (dispatch, getState) => {
    // api.get(`Carriage/GetByTrainId?train_id=${id}`).then(result => {
    dispatch(new ExpandCategoryAction(id, {}))
    // if (showAdd) dispatch(new ShowAddCategoryAction(id))
    // })
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
  addVag: (data): ActionCreator => (dispatch, getState) => {
    api.post(`Carriage/Add`, data).then(resultAdd => {
      store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
      api.get(`Carriage/GetByTrainId?train_id=${data.TrainId}`).then(result => {
        dispatch(new ExpandCategoryAction(data.TrainId, result))
      })
    })
  },
  delEq: (eqId, trainId): ActionCreator => (dispatch, getState) => {
    api.delete(`Carriage/Delete`, { id: eqId }).then(() => {
      api.get(`Carriage/GetByTrainId?train_id=${trainId}`).then(result => {
        dispatch(new ExpandCategoryAction(trainId, result))
      })
    })
  },
  showEdit: (id): ActionCreator => (dispatch, getState) => {
    dispatch(new ShowEdit(id))
  },
  hideEdit: (id): ActionCreator => (dispatch, getState) => {
    dispatch(new HideEdit(id))
  },
  showInnerEdit: (id, innerId): ActionCreator => (dispatch, getState) => {
    api.get(`Carriage/GetByIdWithEquipment?id=${innerId}`).then(resultF => {
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
  delFail: (catId, equipmentId, faultId): ActionCreator => (dispatch, getState) => {
    api.post(`Equipment/RemoveFaultFromEquipment`, { equipmentId, faultId }).then(result => {
      api.get(`Fault/GetByEquipmentId?id=${equipmentId}`).then(resultF => {
        dispatch(new ShowInnerEdit(catId, equipmentId, resultF))
      })
    })
  },
  hideEditors: (): ActionCreator => (dispatch, getState) => {
    dispatch(new HideEditors())
  },
  setCheckList: (tripId, stationId, value): ActionCreator => (dispatch, getState) => {
    api.get(`StantionTrips/AddCheckListToStation?stationTripId=${stationId}&checkListType=${value}`).then(result => {
      dispatch(new AddCategoryAction(result))
      store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
    })
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
      r.vagons &&
        r.vagons.forEach(ir => {
          ir.showAdd = false
          ir.showEdit = false
        })
    })
    return data
  }

  if (isActionType(action, GetCategoriesAction)) {
    return { ...state, result: action.result, reload: false }
  }

  if (isActionType(action, GetLinks)) {
    return { ...state, [action.t]: action.result }
  }

  if (isActionType(action, GetStations)) {
    return { ...state, stations: action.result, reload: false }
  }

  if (isActionType(action, GetModels)) {
    return { ...state, models: action.result, reload: false }
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
    const newState = JSON.parse(JSON.stringify(state))
    const result = newState.result
    const idx = findRowIdx(result.data, action.id)
    result.data[idx].expanded = true
    const expandedRows = newState.expandedRows
    const e = expandedRows.find(r => r === action.id)
    !e && expandedRows.push(action.id)
    return { ...state, result: result, reload: false, expandedRows: expandedRows }
  }

  if (isActionType(action, UnexpandCategoryAction)) {
    const newState = JSON.parse(JSON.stringify(state))
    const result = JSON.parse(JSON.stringify(state.result))
    const idx = findRowIdx(result.data, action.id)
    result.data[idx].expanded = false

    const expandedRows = newState.expandedRows
    const e_idx = expandedRows.findIndex(r => r === action.id)
    e_idx >= 0 && expandedRows.splice(e_idx, 1)
    return { ...state, result: result, reload: false, expandedRows: expandedRows }
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
    const innerIdx = findRowIdx(result.data[idx].vagons, action.innerId)
    hideEditors(result.data)
    result.data[idx].vagons[innerIdx].showEdit = true
    result.data[idx].vagons[innerIdx].equipments = action.data.result.equipment
    return { ...state, result: result }
  }

  if (isActionType(action, HideInnerEdit)) {
    const result = JSON.parse(JSON.stringify(state.result))
    const idx = findRowIdx(result.data, action.id)
    const innerIdx = findRowIdx(result.data[idx].vagons, action.innerId)
    result.data[idx].vagons[innerIdx].showEdit = false
    return { ...state, result: result }
  }

  if (isActionType(action, HideEditors)) {
    const result = JSON.parse(JSON.stringify(state.result))
    hideEditors(result.data)
    return { ...state, result: result }
  }

  return (
    state || { result: { data: [], total: 0 }, reload: false, stations: [], models: [], expandedRows: [], routes: [] }
  )
}
