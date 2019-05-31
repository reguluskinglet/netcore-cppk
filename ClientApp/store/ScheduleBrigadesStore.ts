import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, findRowIdx, ShowTooltip, ShowDeleteDialog } from '../common'
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
  trips: any
  inputs: any
  outputs: any
  brigades: any
  routes: any
}

@typeName('Get4442334523544asdsad3323534445')
class Get extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('Add4444asdasd34344')
class Add extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('Del5443asdasdasd43434445')
class Del extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ShowEdit34asdasd3444445')
class ShowEdit extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('HideE3434diasdasdt44445')
class HideEdit extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('HideEdi343434dasdasdsators44445')
export class HideEditors extends Action {
  constructor() {
    super()
  }
}

@typeName('getL8343434asdasdsadsdsds82')
class GetLinks extends Action {
  constructor(public result, public t) {
    super()
  }
}

export const actionCreators = {
  get: (skip, limit, p1?, p2?): ActionCreator => (dispatch, getState) => {
    api.get(`Trips/GetAllBrigades?skip=${skip}&limit=${limit}`).then(result => {
      dispatch(new Get(result))
    })
  },
  getLinks: (type, id?): ActionCreator => (dispatch, getState) => {
    switch (type) {
      case 'routes':
        api.get(`Route/GetAll?skip=0&limit=1000`).then(result => {
          const days = ['ВС', 'ПН', 'ВТ', 'СР', 'ЧТ', 'ПТ', 'СБ']
          const arr = result.map(e => {
            return { value: e.id, label: `${e.name} (${days[e.day]})` }
          })
          dispatch(new GetLinks(arr, 'routes'))
          dispatch(new GetLinks([], 'inputs'))
          dispatch(new GetLinks([], 'outputs'))
          dispatch(new GetLinks([], 'brigades'))
        })
        break

      case 'trips':
        api.get(`Trips/GetAll?skip=0&limit=1000`).then(result => {
          const days = ['ВС', 'ПН', 'ВТ', 'СР', 'ЧТ', 'ПТ', 'СБ']
          const arr = result.data.map(e => {
            return { value: e.id, label: `${e.tripName} (${days[e.day]})` }
          })
          dispatch(new GetLinks(arr, 'trips'))
          dispatch(new GetLinks([], 'inputs'))
          dispatch(new GetLinks([], 'outputs'))
          dispatch(new GetLinks([], 'brigades'))
        })
        break

      case 'inputs':
        api.get(`/StantionTrips/GetFreeStation?tripId=${id}`).then(result => {
          const arr = result.map(e => {
            return { value: e.stationTripId, label: `${e.stationName} ${e.time}` }
          })
          dispatch(new GetLinks(arr, 'inputs'))
          dispatch(new GetLinks([], 'outputs'))
          dispatch(new GetLinks([], 'brigades'))
        })
        break

      case 'outputs':
        api.get(`/StantionTrips/GetLandingStation?tripId=${id[0]}&stationTripId=${id[1]}`).then(result => {
          const arr = result.map(e => {
            return { value: e.stationTripId, label: `${e.stationName} ${e.time}` }
          })
          dispatch(new GetLinks(arr, 'outputs'))
          dispatch(new GetLinks([], 'brigades'))
        })
        break

      case 'brigades':
        api
          .get(
            `StantionTrips/GetBrigadesFromStationRange?TripId=${id[0]}&stationStartId=${id[1]}&stationEndId=${id[2]}`
          )
          .then(result => {
            const arr = result.map(e => {
              return { value: e.brigadeId, label: `${e.brigadeName}` }
            })
            dispatch(new GetLinks(arr, 'brigades'))
          })
        break

      default:
        break
    }
    // api.get(`UserRole/GetAll?skip=0&limit=1000`).then(res => {
    //   const roles =
    //     res &&
    //     res.data &&
    //     res.data.map(r => {
    //       return { value: r.role.id, label: r.role.name }
    //     })
    //   dispatch(new GetLinks(roles, 'roles'))
    // })
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
  add: (data): ActionCreator => (dispatch, getState) => {
    api
      .get(
        `StantionTrips/AddBrigadeToStations?TripId=${data.tripId}&stationStartId=${data.inputId}&stationEndId=${data.outputId}&brigadeId=${data.brigadeId}`
      )
      .then(result => {
        dispatch(new Add(result))
        store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
      })
  },
  del: (data): ActionCreator => (dispatch, getState) => {
    store.dispatch(
      new ShowDeleteDialog('', () => {
        api
          .get(
            `StantionTrips/DeleteBrigadeFromStations?TripId=${data.tripId}&stationStartId=${data.inputId}&stationEndId=${data.outputId}`
          )
          .then(() => {
            dispatch(new Del(0))
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
  }
}

export const reducer: Reducer<State> = (state, action: any) => {
  if (isActionType(action, Get)) {
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

  return (
    state || {
      result: { data: [], total: 0 },
      reload: false,
      trips: [],
      inputs: [],
      outputs: [],
      brigades: [],
      routes: []
    }
  )
}
