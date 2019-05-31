import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, findRowIdx, ShowTooltip, ShowDeleteDialog } from '../../common'
import { Storage, UserService } from '../../services'
import api from '../../services/rest'
import { ActionCreator } from '../../store'
import store from '../../main'
import { get } from 'lodash';

interface IData {
  data: Array<any>
  total: number
}

export interface State {
    result: IData
    reload: boolean
    id: any
    templates: any
    users: any
    md: any
    printers: any[]

    isStartPrint?: any
}

@typeName('TagPrint_Status')
class TagPrint_Status extends Action {
    constructor(public isStartPrint) {
        super()
    }
}

@typeName('Get444443sadasd3445')
class Get extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('Add444434asdasd344')
class Add extends Action {
  constructor(public result) {
    super()
  }
}

@typeName('Del5443asdasdas43434445')
class Del extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('ShowEdit34asdasd3444445')
class ShowEdit extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('HideE34asdasd34dit44445')
class HideEdit extends Action {
  constructor(public id) {
    super()
  }
}

@typeName('HideEdi34asdasdasd3434tors44445')
export class HideEditors extends Action {
  constructor() {
    super()
  }
}

@typeName('getL83434asdasdasds34sdsds82')
class GetLinks extends Action {
  constructor(public result, public t) {
    super()
  }
}

var timerPrint;

export const actionCreators = {
  get: (skip, limit, p1?, p2?): ActionCreator => (dispatch, getState) => {
    const filter =
      ((p1 || p2) &&
        '&filter=[' +
          (p1 ? `{"filter":"DateFrom","value":"${p1}"},` : '') +
          (p2 ? `{"filter":"DateTo","value":"${p2}"}` : '') +
          ']') ||
      ''
    api.get(`Label/GetTaskPrints?skip=${skip}&limit=${limit}${filter}`).then(result => {
      dispatch(new Get(result))
    })
  },
  getLinks: id => (dispatch, getState) => {
    api
      .get(`http://localhost:5050/service/GetPrinters`)
      .then(res => {
        dispatch(new GetLinks(res, 'printers'))
      })
      .fail(res => {
        store.dispatch(new ShowTooltip('Сервис печати не доступен!', 1))
      })

    api.get(`Label/GetAllTemplateLabels`).then(res => {
      const roles =
        res &&
        res.map(r => {
          return { value: r.id, label: r.name, template: r.template }
        })
      dispatch(new GetLinks(roles, 'templates'))
    })

    // api.get(`Label/GetPrintTaskByIdGetPrintTaskById?taskPrintId=${id}`).then(res => {
    //   dispatch(new GetLinks(res, 'tag'))
    // })
  },
  add: (): any => (dispatch, getState) => {
    return api.post(`Label/AddOrUpdateTaskPrints`, {})
  },
  del: (id): ActionCreator => (dispatch, getState) => {
    store.dispatch(
      new ShowDeleteDialog('', () => {
        api.delete(`Label/DeleteTaskPrints`, { id: id }).then(() => {
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
  },
  getUserList: (data: any): ActionCreator => (dispatch, getState) => {
    api.post(`Label/GetUserLabels`, { ...data }).then(result => {
      dispatch({ type: 'tags.getUserList', payload: result })
    })
  },
  getMdList: (data: any): ActionCreator => (dispatch, getState) => {
    api.post(`Label/GetDeviceLabels`, { ...data }).then(result => {
      dispatch({ type: 'tags.getMdList', payload: result })
    })
  },
  print: (rows:any[], printData, t) => (dispatch, getState) => {
    api
      .post(`http://localhost:5050/service/SavePrinterSettings`, {
        ConnectionType: printData.connectionType || 0,
        CorpTagCOMPort: printData.comPort || '',
        RfidTagWriterDeviceType: printData.modelName || '0',
        SelectedPrinter: printData.printerName || '',
        TagWriterTxPower: printData.power || '0',
        Template: printData.template || ''
      })
      .then(res => {
        if (printData.paper) {
          debugger
          api
            .post(
              `http://localhost:5050/service/Print`,
              rows.map((row, index) => ({
                Id: get(row, 'id.id', ''),
                Rfid: get(row, t==='user'?'col2':'col1', ''),
                Name: get(row, t==='user'?'col0':'col0', ''),
                RoomFullName: get(row, t==='user'?'col3':'col2', ''),
                InvCode:'',
                VinCode: '',
                CompanyName: 'АО "Центральная ППК"'
              }))
            )
            .then(res => {
              dispatch({
                type: 'Tags.Print'
                })

                actionCreators.getPrintState(dispatch)
                dispatch(new TagPrint_Status(true))
            })
        } else {
            if (rows && rows.length) {
                dispatch(new TagPrint_Status(true))
                var rfid = get(rows[0], t === 'user' ? 'col2' : 'col1', '');//ЭТО ППЦ
                api.post(`http://localhost:5050/service/PrintCorpTagRfid`, rfid).then(x => {
                    dispatch(new TagPrint_Status(undefined))
                }).fail(responce => {
                    var errorMessage = responce.responseText
                    dispatch(new ShowTooltip(errorMessage, 1))
                    dispatch(new TagPrint_Status(undefined))
                })
            }
        }
      })
  },
    getPrintState: (dispatch)=> {
       timerPrint = setInterval(() => {

            api.post('http://localhost:5050/service/PrintProgress', {})
                .then(res => {

                    if (res)
                        dispatch({ type: 'Tags.PrintState', result: res })
                    else {
                        clearInterval(timerPrint)
                        dispatch(new ShowTooltip('Печать завершена',0))
                        dispatch(new TagPrint_Status(undefined))
                    }
                }).fail(() => {
                    clearInterval(timerPrint)
                    dispatch(new TagPrint_Status(undefined))
                })

        }, 500);

    },
  stopPrint: (): ActionCreator => (dispatch, getState) => {
      api.post('http://localhost:5050/service/StopPrinted', {}).then(res => {

          if (timerPrint)
              clearInterval(timerPrint)

          dispatch({ type: 'StopPrint' })
          dispatch(new TagPrint_Status(undefined))
    })
  },
  reset: () => dispatch => dispatch({type: 'tags.reset'})
}

export const reducer: Reducer<State> = (state, action: any) => {
    if (isActionType(action, TagPrint_Status)) {
        return { ...state, isStartPrint: action.isStartPrint }
    }
    else if (isActionType(action, Get)) {
        return { ...state, result: action.result, reload: false }
    }

    if (isActionType(action, GetLinks)) {
        return { ...state, [action.t]: action.result }
    }

    if (isActionType(action, Add)) {
        return { ...state }
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
            r.equipments &&
                r.equipments.forEach(ir => {
                    ir.showEdit = false
                })
        })
        return { ...state, result: result }
    }

    if (action.type === 'tags.getUserList') {
        return { ...state, users: action.payload }
    }

    if (action.type === 'tags.getMdList') {
        return { ...state, md: action.payload }
    }

    if (action.type === 'Tags.Print') {
        return { ...state, print1: true, procent: 0 }
    }

    if (action.type === 'Tags.PrintState') {
        let p = 100
        try {
            p = action.result.progress[0] / action.result.progress[1] * 100
        } catch (err) { }
        return { ...state, procent: p, print1: p < 100 }
    }

    if (action.type === 'Tags.StopPrint') {
        return { ...state, print1: false, procent: 999 }
    }

    if (action.type === 'tags.reset') {
        return {
            result: { data: [], total: 0 },
            reload: false,
            id: null,
            templates: [],
            md: {},
            users: {},
            printers: [],
            procent: 0,
            print1: false
        }
    }

    return (
        state || {
            result: { data: [], total: 0 },
            reload: false,
            id: null,
            templates: [],
            md: {},
            users: {},
            printers: [],
            procent: 0,
            print1: false
        }
    )
}
