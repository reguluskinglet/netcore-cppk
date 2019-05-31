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
import { set, assign, map } from 'lodash'

interface IData {
  columns: any[]
  rows: Array<any>
  total: number
}

export interface State {
  data: IData
  reload: boolean
  devices: any
  faults: any
  statuses: any
  element: any
}

export const actionCreators = {
  getList: (data: any): ActionCreator => (dispatch, getState) => {
    api.post(`DeviceTask/GetTable`, { ...data }).then(result => {
      dispatch({ type: 'serv.getList', payload: result })
    })
  },
  get: (id: string): ActionCreator => (dispatch, getState) => {
    api.get(`DeviceTask/GetById?id=${id}`).then(result => {
      dispatch({ type: 'serv.get', payload: result })
    })
  },
  save: (): any => (dispatch, getState) => {
    return api.post(`DeviceTask/Add`, getState().serv.element).then(result => {
      dispatch({ type: 'serv.add', payload: result })
      store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
    })
  },
  comment: (): any => (dispatch, getState) => {
    return api
      .post(`DeviceTask/SaveCommentAndStatus`, {
        DeviceTaskId: getState().serv.element.id,
        Status: {
          Id: getState().serv.element.status.id
        },
        Text: getState().serv.element.comment
      })
      .then(result => {
        dispatch({ type: 'serv.add', payload: result })
        store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
      })
  },
  del: (IdToDelete): ActionCreator => (dispatch, getState) => {
    store.dispatch(
      new ShowDeleteDialog('', () => {
        api.delete(`dictionary/stantion`, { IdToDelete }).then(() => {
          dispatch({ type: 'serv.del' })
        })
      })
    )
  },
  setValue: (path, value): ActionCreator => (dispatch, getState) => {
    store.dispatch({ type: 'serv.setValue', payload: { path, value } })
  },
  getLinks: (type?: 'devices' | 'faults' | 'statuses'): any => (
    dispatch,
    getState
  ) => {
    return Promise.all([
      api
        .post(`Device/GetTable`, {
          paging: {
            skip: 0,
            limit: 10000000
          },
          filters: [],
          sortings: []
        })
        .then(result => {
          dispatch({
            type: 'serv.getLinks',
            payload: { type: 'devices', result: result.rows }
          })
        }),

      api.get(`DeviceFault/GetAll`).then(result => {
        dispatch({ type: 'serv.getLinks', payload: { type: 'faults', result } })
      }),

      api.get(`DeviceTask/GetAllStatuses`).then(result => {
        dispatch({
          type: 'serv.getLinks',
          payload: { type: 'statuses', result }
        })
      })
    ])
  },
  clear: (): ActionCreator => (dispatch, getState) => {
    store.dispatch({ type: 'serv.clearElement' })
  }
}

export const reducer: Reducer<State> = (state, action: any) => {
  switch (action.type) {
    case 'serv.getList':
      return { ...state, data: action.payload, element: {} }

    case 'serv.clearElement':
      return { ...state, element: {} }

    case 'serv.get':
      return { ...state, element: action.payload }

    case 'serv.getLinks':
      return { ...state, [action.payload.type]: action.payload.result }

    case 'serv.setValue':
      const element = assign({}, state.element)
      set(element, action.payload.path, action.payload.value)
      return { ...state, element }

    default:
      return (
        state || {
          data: { columns: [], rows: [], total: 0 },
          reload: false,
          devices: [],
          faults: [],
          statuses: [],
          element: {}
        }
      )
  }
}
