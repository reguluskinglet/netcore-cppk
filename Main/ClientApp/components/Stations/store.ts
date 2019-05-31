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
}

export const actionCreators = {
  getList: (data: any): ActionCreator => (dispatch, getState) => {
    api.post(`dictionary/stantion`, { ...data }).then(result => {
      dispatch({ type: 'stations.getList', payload: result })
    })
  },
  add: (data): ActionCreator => (dispatch, getState) => {
    api.put(`dictionary/stantion`, data).then(result => {
      dispatch({ type: 'stations.add', payload: result })
      store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
    })
  },
  del: (IdToDelete): ActionCreator => (dispatch, getState) => {
    store.dispatch(
      new ShowDeleteDialog('', () => {
        api.put(`dictionary/stantion`, { IdToDelete }).then(() => {
          dispatch({ type: 'stations.del' })
        })
      })
    )
  }
}

export const reducer: Reducer<State> = (state, action: any) => {
  switch (action.type) {
    case 'stations.getList':
      return { ...state, data: action.payload, reload: false }

    case 'stations.add':
      return { ...state, reload: true }

    case 'stations.del':
      return { ...state, reload: true }

    default:
      return state || { data: { columns: [], rows: [], total: 0 }, reload: false }
  }
}
