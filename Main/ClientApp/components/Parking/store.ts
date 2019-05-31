import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, findRowIdx, ShowTooltip, ShowDeleteDialog } from '../../common'
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
    api.post(`dictionary/parking`, { ...data }).then(result => {
      dispatch({ type: 'parking.getList', payload: result })
    })
  },
  add: (data): any => (dispatch, getState) => {
    return api.put(`dictionary/parking`, data).then(result => {
      dispatch({ type: 'parking.add', payload: result })
      store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
    })
  },
  del: (IdToDelete): any => (dispatch, getState) => {
    return new Promise(resolve => {
      store.dispatch(
        new ShowDeleteDialog('', () => {
          api.put(`dictionary/parking`, { IdToDelete }).then(() => {
            dispatch({ type: 'parking.del' })
            resolve()
          })
        })
      )
    })
  },
  getLinks: (skip, limit): ActionCreator => (dispatch, getState) => {
    api
      .post(`dictionary/stantion`, {
        paging: { skip: skip, limit: limit },
        sortings: [],
        filters: []
      })
      .then(result => {
        dispatch({ type: 'parking.getLinks', payload: { type: 'stantions', result: result.rows } })
      })
  }
}

export const reducer: Reducer<State> = (state, action: any) => {
  switch (action.type) {
    case 'parking.getList':
      return { ...state, data: action.payload, reload: false }

    case 'parking.add':
      return { ...state, reload: true }

    case 'parking.del':
      return { ...state, reload: true }

    case 'parking.getLinks':
      return { ...state, [action.payload.type]: action.payload.result }

    default:
      return state || { data: { columns: [], rows: [], total: 0 }, reload: false, stantions: [] }
  }
}
