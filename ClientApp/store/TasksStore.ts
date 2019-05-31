import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, ShowTooltip, ShowDeleteDialog } from '../common'
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
  trains: Array<any>
  users: Array<any>
  brigades: Array<any>
  vagons: Array<any>
  inspection: any
}

@typeName('get881')
class Get extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('getL881')
class GetLinks extends Action {
  constructor(public result, public t) {
    super()
  }
}

export const actionCreators = {
  get: (skip, limit, filter?): ActionCreator => (dispatch, getState) => {
    const af = []
    for (const key in filter) {
      if (filter.hasOwnProperty(key)) {
        const element = filter[key]
        const f = { filter: key, value: element }
        element && af.push(f)
      }
    }
    const s = (af.length > 0 && `&filter=${JSON.stringify(af)}`) || ``
    api.get(`Task/GetAll?skip=${skip}&limit=${limit}${s}`).then(result => {
      dispatch(new Get(result))
    })
  },
  getLinks: (inspectionId?): ActionCreator => (dispatch, getState) => {
    // api.get(`Train/GetAll?skip=0&limit=1000`).then(res => {
    //   let m = []
    //   if (res.data) {
    //     m = res.data.map(el => {
    //       return { label: el.name, value: el.id }
    //     })
    //     dispatch(new GetLinks(m, 'trains'))
    //   }
    // })

    // api.get(`User/GetAll?skip=0&limit=1000`).then(res => {
    //   let m = []
    //   if (res.data) {
    //     m = res.data.map(el => {
    //       return { label: el.name, value: el.id }
    //     })
    //     dispatch(new GetLinks(m, 'users'))
    //   }
    // })

    api.get(`Brigade/GetAll?skip=0&limit=1000`).then(res => {
      let m = []
      if (res.data) {
        m = res.data.map(el => {
          return { label: el.name, value: el.id }
        })
        dispatch(new GetLinks(m, 'brigades'))
      }
    })

    // api.get(`Carriage/GetAll?skip=0&limit=1000`).then(res => {
    //   dispatch(new GetLinks(res, 'vagons'))
    // })

    if (inspectionId) {
      api
        .get(`Journal/GetAll?skip=0&limit=1&filter=[{"filter":"InspectionId", "value": ${inspectionId}}]`)
        .then(result => {
          const data = result.data.map(el => JSON.parse(el))
          data[0] && dispatch(new GetLinks(data[0], 'inspection'))
        })
    }
  }
}

export const reducer: Reducer<State> = (state, action: any) => {
  if (isActionType(action, Get)) {
    return { ...state, result: action.result }
  }

  if (isActionType(action, GetLinks)) {
    return { ...state, [action.t]: action.result }
  }

  return state || { result: { data: [], total: 0 }, trains: [], users: [], brigades: [], vagons: [], inspection: {} }
}
