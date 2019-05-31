import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, ShowTooltip, ShowDeleteDialog } from '../../common'
import { Storage, UserService } from '../../services'
import api from '../../services/rest'
import { ActionCreator } from '../../store'
import store from '../../main'
import { filter } from 'lodash';

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
  akts: any[]
}

@typeName('GetCategoriesAction6')
class GetCategoriesAction extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('AddCategoryAction6')
class AddCategoryAction extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('DelCategoryAction6')
class DelCategoryAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ExpandCategoryAction6')
class ExpandCategoryAction extends Action {
  constructor(public id, public result) {
    super()
  }
}

@typeName('UnexpandCategoryAction6')
class UnexpandCategoryAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ShowAddCategoryAction6')
class ShowAddCategoryAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('HideAddCategoryAction6')
class HideAddCategoryAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('AddEqCategoryAction6')
class AddEqCategoryAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ShowEdit16')
class ShowEdit extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('HideEdit16')
class HideEdit extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ShowInnerEdit16')
class ShowInnerEdit extends Action {
  constructor(public id, public innerId, public data) {
    super()
  }
}

@typeName('HideInnerEdit16')
class HideInnerEdit extends Action {
  constructor(public id, public innerId) {
    super()
  }
}

@typeName('GetStations6')
class GetStations extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('GetFailsByEq6')
class GetFailsByEq extends Action {
  constructor(public id, public result) {
    super()
  }
}

@typeName('AddFail6')
class AddFail extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('GetModels6')
class GetModels extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('HideEditors6')
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
          (p2 ? `{"filter":"stantionId","value":${p2}}` : '') +
          ']') ||
      ''
    api.get(`Train/GetAll?skip=${skip}&limit=${limit}${filter}`).then(result => {
      dispatch(new GetCategoriesAction(result))
      api.get(`Stantion/GetDepot`).then(resultF => {
        let stations = []
        if (resultF) {
          stations = resultF.map(el => {
            return { label: el.name, value: el.id }
          })
          dispatch(new GetStations(stations))
        }
      })
      api.get(`Model/GetAll?skip=0&limit=1000`).then(resultM => {
        let models = []
        if (resultM.data) {
          models = resultM.data.map(el => {
            const label = `${el.name} (${modelTypeLabels[el.modelType]})`
            return { label: label, value: el.id }
          })
          dispatch(new GetModels(models))
        }
      })
    })
  },
  addCat: (data): ActionCreator => (dispatch, getState) => {
    api.post(`Train/Add`, data).then(result => {
      dispatch(new AddCategoryAction(result))
      store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
    })
  },
  delCat: (id): ActionCreator => (dispatch, getState) => {
    store.dispatch(
      new ShowDeleteDialog('', () => {
        api.delete(`Train/Delete`, { id: id }).then(() => {
          dispatch(new DelCategoryAction(id))
        })
      })
    )
  },
  expandRow: (id, showAdd?): ActionCreator => (dispatch, getState) => {
    api.get(`Carriage/GetByTrainId?train_id=${id}`).then(result => {
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
  addVag: (data): ActionCreator => (dispatch, getState) => {
    api.post(`Carriage/Add`, data).then(resultAdd => {
      store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
      api.get(`Carriage/GetByTrainId?train_id=${data.TrainId}`).then(result => {
        dispatch(new ExpandCategoryAction(data.TrainId, result))
      })
    })
  },
  delEq: (eqId, trainId): ActionCreator => (dispatch, getState) => {
    store.dispatch(
      new ShowDeleteDialog('', () => {
        api.delete(`Carriage/Delete`, { id: eqId }).then(() => {
          api.get(`Carriage/GetByTrainId?train_id=${trainId}`).then(result => {
            dispatch(new ExpandCategoryAction(trainId, result))
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
  getAkts: train => async dispatch => {
    const res = await api.get('TechPass/GetAll?skip=0&limit=500&filter=[' +
    (train ? `{"filter":"trainId","value":"${train}"}` : '') +
    ']')
    dispatch({ type: 'trains.getAkts', payload: res })
  },
  addAkt: data => async (dispatch, getState) => {
    const res = await api.post('TechPass/AddOrUpdate', data)
    const state = getState()
    dispatch({ type: 'trains.getAkts', payload: {data: [...state.trains.akts, res]} })
    return res
  },
  delAkt: id => async (dispatch, getState) => {
    await api.delete('TechPass/Delete', { id })
    const state = getState()
    dispatch({ type: 'trains.getAkts', payload: {data: filter(state.trains.akts, akt=>akt.id !== id)} })
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
    const result = JSON.parse(JSON.stringify(state.result))
    const idx = findRowIdx(result.data, action.id)
    result.data[idx].expanded = true
    result.data[idx].vagons = action.result
    result.data[idx].showAdd = false
    return { ...state, result: result, reload: false }
  }

  if (isActionType(action, UnexpandCategoryAction)) {
    const result = JSON.parse(JSON.stringify(state.result))
    const idx = findRowIdx(result.data, action.id)
    result.data[idx].expanded = false
    return { ...state, result: result, reload: false }
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
    result.data[idx].vagons[innerIdx].equipments = action.data.equipment || action.data.result.equipment
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

  if (action.type === 'trains.getAkts') {
    return {
      ...state,
      akts: action.payload.data
    }
  }

  return state || { result: { data: [], total: 0 }, reload: false, stations: [], models: [], akts: [] }
}
