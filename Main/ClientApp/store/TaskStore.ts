import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, ShowTooltip, ShowDeleteDialog } from '../common'
import { Storage, UserService } from '../services'
import api from '../services/rest'
import { ActionCreator } from '.'
import store from '../main'

interface IData {
  data: any
}

export interface State {
  data: any
  trains: Array<any>
  users: Array<any>
  brigades: Array<any>
  vagons: Array<any>
  files: Array<any>
  inspection: any
}

@typeName('get88')
class Get extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('getL88')
class GetLinks extends Action {
  constructor(public result, public t) {
    super()
  }
}

@typeName('Upload')
class Upload extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('DelFile')
class DelFile extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('AddComment')
class AddComment extends Action {
  constructor(public result) {
    super()
  }
}

export const actionCreators = {
  get: (id, attrId): ActionCreator => (dispatch, getState) => {
    api.get(`Task/GetTaskById?id=${id}&attributeId=${attrId}`).then(result => {
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
    api.get(`Brigade/GetAll?skip=0&limit=1000`).then(res => {
      let m = []
      if (res.data) {
        m = res.data.map(el => {
          return { label: el.name, value: el.id }
        })
        dispatch(new GetLinks(m, 'brigades'))
      }
    })

    if (inspectionId) {
      api
        .get(`Journal/GetAll?skip=0&limit=1&filter=[{"filter":"InspectionId", "value": ${inspectionId}}]`)
        .then(result => {
          const data = result.data.map(el => JSON.parse(el))
          data[0] && dispatch(new GetLinks(data[0], 'inspection'))
        })
    }
  },
  upload: (formData): ActionCreator => (dispatch, getState) => {
    api.post(`Document/Add`, formData, true).then(result => {
      dispatch(new Upload(JSON.parse(result).files))
    })
  },
  delFile: (id): ActionCreator => (dispatch, getState) => {
    store.dispatch(
      new ShowDeleteDialog('', () => {
        api.delete(`Document/Delete`, { id }).then(result => {
          dispatch(new DelFile(id))
        })
      })
    )
  },
  addComment: (data): ActionCreator => (dispatch, getState) => {
    const newData = {
      TraintaskId: data.id,
      StatusId: data.status,
      CommentText: data.comment === null && data.files.length ? '' : data.comment,
      FilesId: data.files.map(file => file.id),
      TrainTaskExecutorsId: data.brigade,
      TaskLevel: data.taskLevel
    }
    api.post(`Task/Update`, newData).then(result => {
      dispatch(new AddComment(result))
      store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
      api.get(`Task/GetTaskById?id=${data.id}&attributeId=${data.attrId}`).then(result => {
        dispatch(new Get(result))
      })
    })
  }
}

export const reducer: Reducer<State> = (state, action: any) => {
  function findRowIdx(data, id) {
    const row = data.find(r => r.id === id)
    const idx = data.indexOf(row)
    return idx
  }

  if (isActionType(action, Get)) {
    return { ...state, data: action.result }
  }

  if (isActionType(action, GetLinks)) {
    return { ...state, [action.t]: action.result }
  }

  if (isActionType(action, Upload)) {
    const files = JSON.parse(JSON.stringify(state.files))
    return { ...state, files: files.concat(action.result) }
  }

  if (isActionType(action, DelFile)) {
    const files = JSON.parse(JSON.stringify(state.files))
    const idx = findRowIdx(files, action.id)
    files.splice(idx, 1)
    return { ...state, files: files }
  }

  if (isActionType(action, AddComment)) {
    return { ...state, files: [] }
  }

  return state || { data: {}, trains: [], users: [], brigades: [], vagons: [], files: [], inspection: {} }
}
