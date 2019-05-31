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
  users: Array<any>
  brigades: Array<any>
  vagons: Array<any>
  files: Array<any>
  inspection: any
  trains: any
  equipments: any
  redirect: string
  fails: any
  executors: any
  statuses: any
}

@typeName('getdfg88')
class Get extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('gdfgfsdfetL88')
class GetLinks extends Action {
  constructor(public result, public t) {
    super()
  }
}

@typeName('Uplddfgfdoad')
class Upload extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('DelFdfddfdsdffdsdffdile')
class DelFile extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('AddCodfddfgfdsdfdsdfgmment')
class AddComment extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('AddCodfddfgfdsdfdsascascascdfgmment')
class Redirect extends Action {
  constructor(public result) {
    super()
  }
}

export const actionCreators = {
  get: (id): ActionCreator => (dispatch, getState) => {
    api.get(`Task/GetTaskById?id=${id}`).then(result => {
      dispatch(new Get(result))
    })
  },
  getAvaibleStatuses: (brId): ActionCreator => (dispatch, getState) => {
    api.get(`Task/GetAvaibleStatuses?taskStatus=null&executorBrigadeType=${brId}`).then(res => {
      dispatch(new GetLinks(res, 'statuses'))
    })
  },
  getLinks: (inspectionId?): ActionCreator => (dispatch, getState) => {
    api.get(`Task/GetAvaibleExecutors`).then(res => {
      dispatch(new GetLinks(res, 'executors'))
    })

    api.get(`Train/GetAll?skip=0&limit=1000`).then(res => {
      // let m = []
      // if (res.data) {
      //   m = res.data.map(el => {
      //     return { label: el.name, value: el.id }
      //   })
      dispatch(new GetLinks(res.data, 'trains'))
      // }
    })
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
      TaskType: data.taskType,
      TrainId: data.train,
      CarriageId: data.vagon,
      EquipmentId: data.equipment,
      TaskStatus: data.status,
      Text: data.comment,
      Executor: data.brigade,
      FilesId: data.files,
      FaultId: data.fail,
      TaskLevel: data.taskLevel
    }
    api.post(`Task/Add`, newData).then(result => {
      store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
      // api.get(`Task/GetTaskById?id=${data.id}`).then(result => {
      dispatch(new Redirect('/journals'))
      dispatch(new AddComment(result))
      // })
    })
  },
  getVagons: (id): ActionCreator => (dispatch, getState) => {
    api.get(`Carriage/GetByTrainId?train_id=${id}`).then(res => {
      // let m = []
      // if (res.data) {
      //   m = res.data.map(el => {
      //     return { label: el.name, value: el.id }
      //   })
      dispatch(new GetLinks(res, 'vagons'))
      // }
    })
  },
  getEq: (id): ActionCreator => (dispatch, getState) => {
    api.get(`Model/GetEquipmentsByCarriage?carriage_id=${id}&sortToTaskList=true`).then(res => {
      // let m = []
      // if (res.data) {
      //   m = res.data.map(el => {
      //     return { label: el.name, value: el.id }
      //   })
      dispatch(new GetLinks(res, 'equipments'))
      // }
    })
  },
  getFails: (id): ActionCreator => (dispatch, getState) => {
    api.get(`Fault/GetByEquipmentId?id=${id}`).then(res => {
      dispatch(new GetLinks(res, 'fails'))
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
    return { ...state, files: [], redirect: null }
  }

  if (isActionType(action, Redirect)) {
    return { ...state, redirect: action.result }
  }

  return (
    state || {
      data: {},
      trains: [],
      users: [],
      brigades: [],
      files: [],
      inspection: {},
      vagons: [],
      equipments: [],
      redirect: null,
      fails: [],
      executors: [],
      statuses: []
    }
  )
}
