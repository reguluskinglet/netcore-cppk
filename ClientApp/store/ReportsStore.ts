import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, ShowTooltip, ShowDeleteDialog } from '../common'
import { Storage, UserService } from '../services'
import api from '../services/rest'
import { ActionCreator } from '.'
import store from '../main'

interface IData {
  columns: any
  rows: any
  total: number
}

export interface State {
  result: IData
  trains: Array<any>
  users: Array<any>
  brigades: Array<any>
  vagons: Array<any>
  inspection: any
  reports: any
}

@typeName('get8wed81')
class GetReports extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('getL88sdfsdf1')
class GetReport extends Action {
  constructor(public result) {
    super()
  }
}

export const actionCreators = {
  getReports: (): ActionCreator => (dispatch, getState) => {
    api.get(`Report/GetList`).then(result => {
      dispatch(new GetReports(result))
    })
  },
  getReport: (report, skip, limit): ActionCreator => (dispatch, getState) => {
    api.get(`Report/Get?id=${report}&skip=${skip}&limit=${limit}`).then(result => {
      dispatch(new GetReport(result))
    })
  }
}

export const reducer: Reducer<State> = (state, action: any) => {
  if (isActionType(action, GetReports)) {
    const reports = action.result.map(r => {
      return { label: r.name, value: r.id }
    })
    return { ...state, reports }
  }

  if (isActionType(action, GetReport)) {
    return { ...state, result: action.result }
  }

  return (
    state || {
      result: { data: [], total: 0 },
      trains: [],
      users: [],
      brigades: [],
      vagons: [],
      inspection: {},
      reports: []
    }
  )
}
