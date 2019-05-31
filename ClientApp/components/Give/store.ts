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
import { set, assign } from 'lodash'

interface IData {
  columns: any[]
  rows: Array<any>
  total: number
}

export interface State {
  data: IData
  reload: boolean
  hist: IData
  gps: IData
  charge: IData
  element: any
}

export const actionCreators = {
  getList: (data: any): ActionCreator => (dispatch, getState) => {
    api.post(`Device/GetTable`, { ...data }).then(result => {
      dispatch({ type: 'give.getList', payload: result })
    })
  },
  get: (id: string): ActionCreator => (dispatch, getState) => {
    api.get(`Device/GetById?id=${id}`).then(result => {
      dispatch({ type: 'give.get', payload: result })
    })
  },
  save: (): any => (dispatch, getState) => {
    return api
      .post(`Device/AddOrUpdate`, getState().give.element)
      .then(result => {
        dispatch({ type: 'give.add', payload: result })
        store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
      })
  },
  del: (IdToDelete): any => (dispatch, getState) => {
    return new Promise(resolve =>
      store.dispatch(
        new ShowDeleteDialog('', () => {
          api
            .post(`Device/Delete?id=${IdToDelete}`, { IdToDelete })
            .then(() => {
              dispatch({ type: 'give.del' })
              resolve()
            })
        })
      )
    )
  },
  setValue: (path, value): ActionCreator => (dispatch, getState) => {
    store.dispatch({ type: 'give.setValue', payload: { path, value } })
  },
  getLinks: (
    type: 'hist' | 'gps' | 'charge',
    { currentPage, pageSize, sortings, filters }
  ): ActionCreator => (dispatch, getState) => {
    let path
    switch (type) {
      case 'hist':
        path = 'DeviceOperation/GetDeviceHistoryTable'
        break
      case 'gps':
        path = 'DeviceValue/GetDeviceLocationsTable'
        break
      case 'charge':
        path = 'DeviceValue/GetDeviceChargesTable'
        break
    }
    api
      .post(path, {
        paging: { skip: currentPage * pageSize, limit: pageSize },
        sortings,
        filters,
        deviceId: getState().give.element.id
      })
      .then(result => {
        dispatch({ type: 'give.getLinks', payload: { type, result } })
      })
  },
  clear: (): ActionCreator => (dispatch, getState) => {
    store.dispatch({ type: 'give.clearElement' })
  }
}

export const reducer: Reducer<State> = (state, action: any) => {
  switch (action.type) {
    case 'give.getList':
      return { ...state, data: action.payload, element: {} }

    case 'give.clearElement':
      return {
        ...state,
        element: {}
      }

    case 'give.get':
      return { ...state, element: action.payload }

    case 'give.getLinks':
      return { ...state, [action.payload.type]: action.payload.result }

    case 'give.setValue':
      const element = assign({}, state.element)
      set(element, action.payload.path, action.payload.value)
      return { ...state, element }

    default:
      return (
        state || {
          data: { columns: [], rows: [], total: 0 },
          reload: false,
          hist: { columns: [], rows: [], total: 0 },
          gps: { columns: [], rows: [], total: 0 },
          charge: { columns: [], rows: [], total: 0 },
          element: {}
        }
      )
  }
}
