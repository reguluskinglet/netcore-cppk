import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import {
  RequestEvent,
  findRowIdx,
  ShowTooltip,
  ShowDeleteDialog
} from '../../common'
import { Storage, UserService } from '../../services'
import api from '../../services/rest'
import { ActionCreator } from '../../store'
import store from '../../main'

interface IData {
  columns: any[]
  rows: Array<any>
  total: number
}

export interface State {
  data: IData
  reload: boolean
  stations: any[]
  trips: any[]
  trip: any
}

export const actionCreators = {
  getList: (data: any): ActionCreator => (dispatch, getState) => {
    api.post(`dictionary/TripWithDays`, { ...data }).then(result => {
      dispatch({ type: 'routes.getList', payload: result })
    })
  },
  add: (data): any => (dispatch, getState) => {
    return api.put(`dictionary/TripWithDays`, data).then(result => {
      dispatch({ type: 'routes.add', payload: result })
      store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
    })
  },
  del: (IdToDelete): any => (dispatch, getState) => {
    return new Promise(resolve => {
      store.dispatch(
        new ShowDeleteDialog('', () => {
          api.put(`dictionary/TripWithDays`, { IdToDelete }).then(() => {
            dispatch({ type: 'routes.del' })
            resolve()
          })
        })
      )
    })
  },
  getLinks: (): ActionCreator => (dispatch, getState) => {
    api
      .post(`dictionary/stantion`, {
        paging: { skip: 0, limit: 100000 },
        sortings: [],
        filters: []
      })
      .then(result => {
        dispatch({
          type: 'routes.getLinks',
          payload: { type: 'stations', result: result.rows }
        })
      })
  },
  clearTrip: (): ActionCreator => (dispatch, getState) => {
    dispatch({ type: 'routes.clearTrip' })
  },
  addNewTrip: (data): any => (dispatch, getState) => {
    return api
      .post(`SheduleCycle/AddOrUpdateTripToRoute`, data)
      .then(result => {
        dispatch({ type: 'routes.addNewTrip' })
      })
  },
  getStation: (search): ActionCreator => (dispatch, getState) => {
    const filter =
      (search &&
        '&filter=[' +
          (search ? `{"filter":"name","value":"${search}"},` : '') +
          ']') ||
      ''
    api.get(`Stantion/GetAll?skip=0&limit=1000${filter}`).then(res => {
      let m = []
      if (res.data) {
        m = res.data.map(el => {
          return { label: el.name, value: el.id }
        })
        dispatch({
          type: 'routes.getLinks',
          payload: { type: 'stations', result: m }
        })
      }
    })
  }
}

export const reducer: Reducer<State> = (state, action: any) => {
  switch (action.type) {
    case 'routes.getList':
      return { ...state, data: action.payload, reload: false }

    case 'routes.addNewTrip':
      return { ...state }

    case 'routes.add':
      return { ...state, reload: true }

    case 'routes.del':
      return { ...state, reload: true }

    case 'routes.getLinks':
      return { ...state, [action.payload.type]: action.payload.result }

    case 'routes.clearTrip':
      return { ...state, trip: null }

    default:
      return (
        state || {
          data: { columns: [], rows: [], total: 0 },
          reload: false,
          stations: [],
          trips: [],
          trip: null
        }
      )
  }
}
