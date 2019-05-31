import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, ShowTooltip, ShowDeleteDialog } from '../../common'
import { Storage, UserService } from '../../services'
import api from '../../services/rest'
import { ActionCreator } from '../../store'
import store from '../../main'
import { filter } from 'lodash';

interface IData {
  data: Array<any>
  total: number
}

export interface State {
  result: IData
  reload: boolean
  stations: Array<any>
}

@typeName('GetParkingsAction6')
class GetParkingsAction extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('AddParkingsAction6')
class AddParkingAction extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('DelParkingAction6')
class DelParkingAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ShowAddParkingAction6')
class ShowAddParkingAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('HideAddParkingAction6')
class HideAddParkingAction extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ShowParkingEdit16')
class ShowEdit extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('HideParkingEdit16')
class HideEdit extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('GetParkingStations6')
class GetStations extends Action {
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
  getParkings: (skip, limit, p1?, p2?): ActionCreator => (dispatch, getState) => {
    const filter =
      ((p1 || p2) &&
        '&filter=[' +
          (p1 ? `{"filter":"name","value":"${p1}"},` : '') +
          (p2 ? `{"filter":"stantionId","value":${p2}}` : '') +
          ']') || ''
    api.get(`Parking/GetAll?skip=${skip}&limit=${limit}${filter}`).then(result => {
      dispatch(new GetParkingsAction(result))
      api.get(`Stantion/GetDepot`).then(resultF => {
        let stations = []
        if (resultF) {
          stations = resultF.map(el => {
            return { label: el.name, value: el.id }
          })
          dispatch(new GetStations(stations))
        }
      })
    })
  },
  addParking: (data): ActionCreator => (dispatch, getState) => {
    api.post(`Parking/Add`, data).then(result => {
      dispatch(new AddParkingAction(result))
      store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
    })
  },
  delParking: (id): ActionCreator => (dispatch, getState) => {
    store.dispatch(
      new ShowDeleteDialog('', () => {
        api.delete(`Parking/Delete`, { id: id }).then(() => {
          dispatch(new DelParkingAction(id))
        })
      })
    )
  },
  showAdd: (id): ActionCreator => (dispatch, getState) => {
    dispatch(new ShowAddParkingAction(id))
  },
  hideAdd: (id): ActionCreator => (dispatch, getState) => {
    dispatch(new HideAddParkingAction(id))
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
  function findRowIdx(data, id) {
    const row = data.find(r => r.id === id)
    const idx = data.indexOf(row)
    return idx
  }

  function hideEditors(result) {
    return result.data || result.data.forEach(r => {
      r.showAdd = false
      r.showEdit = false
    })
  }

  if (isActionType(action, GetParkingsAction)) {
    return { ...state, result: action.result, reload: false }
  }

  if (isActionType(action, GetStations)) {
    return { ...state, stations: action.result, reload: false }
  }

  if (isActionType(action, AddParkingAction)) {
    return { ...state, reload: true }
  }

  if (isActionType(action, DelParkingAction)) {
    return { ...state, reload: true }
  }

  if (isActionType(action, ShowAddParkingAction)) {
    const result = JSON.parse(JSON.stringify(state.result))
    const idx = findRowIdx(result.data, action.id)
    hideEditors(result)
    result.data[idx].showAdd = true
    return { ...state, result: result, reload: false }
  }

  if (isActionType(action, HideAddParkingAction)) {
    const result = JSON.parse(JSON.stringify(state.result))
    const idx = findRowIdx(result.data, action.id)
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

  return state || { result: { data: [], total: 0 }, reload: false, stations: [] }
}
