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
  stantions: any[]
}

export const actionCreators = {
  getList: (data: any): ActionCreator => (dispatch, getState) => {
    api.post(`dictionary/devicefault`, { ...data }).then(result => {
      dispatch({ type: 'devicefault.getList', payload: result })
    })
  },
  add: (data): any => (dispatch, getState) => {
    return api.put(`dictionary/devicefault`, data).then(result => {
      dispatch({ type: 'devicefault.add', payload: result })
      store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
    })
  },
  del: (IdToDelete): ActionCreator => (dispatch, getState) => {
    return new Promise(resolve => {
      store.dispatch(
        new ShowDeleteDialog('', () => {
          api.put(`dictionary/devicefault`, { IdToDelete }).then(() => {
            dispatch({ type: 'devicefault.del' })
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
          type: 'devicefault.getLinks',
          payload: { type: 'stantions', result: result.rows }
        })
      })
  }
}

export const reducer: Reducer<State> = (state, action: any) => {
  switch (action.type) {
    case 'devicefault.getList':
      return { ...state, data: action.payload, reload: false }

    case 'devicefault.add':
      return { ...state, reload: true }

    case 'devicefault.del':
      return { ...state, reload: true }

    case 'devicefault.getLinks':
      return { ...state, [action.payload.type]: action.payload.result }

    default:
      return (
        state || {
          data: { columns: [], rows: [], total: 0 },
          reload: false,
          stantions: []
        }
      )
  }
}
