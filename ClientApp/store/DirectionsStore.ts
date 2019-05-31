import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, findRowIdx, ShowTooltip, ShowDeleteDialog } from '../common'
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
}

@typeName('Get')
class Get extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('Add')
class Add extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('Del')
class Del extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ShowEdit')
class ShowEdit extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('HideEdit')
class HideEdit extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('HideEditors')
export class HideEditors extends Action {
  constructor() {
    super()
  }
}

export const actionCreators = {
  get: (skip, limit, p1?, p2?): ActionCreator => (dispatch, getState) => {
    const filter =
      ((p1 || p2) &&
        '&filter=[' +
          (p1 ? `{"filter":"name","value":"${p1}"},` : '') +
          (p2 ? `{"filter":"faultType","value":${p2}}` : '') +
          ']') ||
      ''
    api.get(`Direction/GetAll?skip=${skip}&limit=${limit}${filter}`).then(result => {
      dispatch(new Get(result))
    })
  },
  add: (data): ActionCreator => (dispatch, getState) => {
    api.post(`Direction/AddOrUpdate`, data).then(result => {
      dispatch(new Add(result))
      store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
    })
  },
  del: (id): ActionCreator => (dispatch, getState) => {
    store.dispatch(
      new ShowDeleteDialog('', () => {
        api.get(`Direction/delete?id=${id}`).then(() => {
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
    })
    return { ...state, result: result }
  }

  return state || { result: { data: [], total: 0 }, reload: false }
}
