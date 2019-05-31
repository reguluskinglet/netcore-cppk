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
  permissions: any
}

@typeName('Get44445')
class Get extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('Add44444')
class Add extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('Del544445')
class Del extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ShowEdit44445')
class ShowEdit extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('HideEdit44445')
class HideEdit extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('HideEditors44445')
export class HideEditors extends Action {
  constructor() {
    super()
  }
}

@typeName('getL8sdsds82')
class GetLinks extends Action {
  constructor(public result, public t) {
    super()
  }
}

export const actionCreators = {
  get: (skip, limit, p1?, p2?): ActionCreator => (dispatch, getState) => {
    api.get(`UserRole/GetAll?skip=${skip}&limit=${limit}`).then(result => {
      dispatch(new Get(result))
    })
  },
  getLinks: (): ActionCreator => (dispatch, getState) => {
    api.get(`UserRole/GetAuthorityArray`).then(res => {
      dispatch(new GetLinks(res, 'permissions'))
    })
  },
  add: (data): ActionCreator => (dispatch, getState) => {
    api.post(`UserRole/AddOrUpdate`, data).then(result => {
      dispatch(new Add(result))
      store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
    })
  },
  del: (id): ActionCreator => (dispatch, getState) => {
    store.dispatch(
      new ShowDeleteDialog('', () => {
        api.delete(`UserRole/Delete`, { id: id }).then(() => {
          dispatch(new Del(id))
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
    const idx = findRowIdx(result.data, action.id, 'role')
    result.data.forEach(r => {
      r.showEdit = false
    })
    result.data[idx].showEdit = true
    return { ...state, result: result }
  }

  if (isActionType(action, HideEdit)) {
    const result = JSON.parse(JSON.stringify(state.result))
    const idx = findRowIdx(result.data, action.id, 'role')
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

  return state || { result: { data: [], total: 0 }, reload: false, permissions: [] }
}
