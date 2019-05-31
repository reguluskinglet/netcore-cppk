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
import * as remove from 'lodash/remove'

export const modelTypes = ['головной', 'моторный', 'прицепной']
interface IData {
    data: Array<any>
    total: number
}

export interface State {
    result: IData
    reload: boolean
    id: any
    templates: any
    tag: any
    trains: any
    vagons: any
    equips: any
    printers: any
    print1: any
    procent: any

    isStartPrint?: any
    isSelectedAll?: boolean
    selectedRows?: any[]
}

@typeName('TagPrint_Status_TagPrintes_PPC')
class TagPrint_Status_PRINT extends Action {
    constructor(public isStartPrint) {
        super()
    }
}

@typeName('Get444443saddfgdsfgasd3445')
class Get extends Action {
    constructor(public result) {
        super()
    }
}

@typeName('Add44sdfgdsfg4434asdasd344')
class Add extends Action {
    constructor(public result) {
        super()
    }
}

@typeName('Del5443asddfgdsfgasdas43434445')
class Del extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('ShowEdit3dsfgsdgfsd4asdasd3444445')
class ShowEdit extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('HideE34asdadsfgdsfgsdgsd34dit44445')
class HideEdit extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('HideEdi34asdasddsfgsdgfasd3434tors44445')
export class HideEditors extends Action {
    constructor() {
        super()
    }
}

@typeName('getL83434asdasdasds3dsfgsdfgsdfg4sdsds82')
class GetLinks extends Action {
    constructor(public result, public t) {
        super()
    }
}

@typeName('TagSelectAll')
class SelectAll extends Action {
    constructor(public val) {
        super()
    }
}

@typeName('getLasdxasxasxascsadcsdcdsc882')
class Select extends Action {
    constructor(public result, public val) {
        super()
    }
}

//@typeName('getsdcsdcsxasxasxasxassdacsdL88dasc2')
//class Unselect extends Action {
//    constructor(public result) {
//        super()
//    }
//}

@typeName('Print')
class Print extends Action {
    constructor() {
        super()
    }
}

@typeName('PrintState')
class PrintState extends Action {
    constructor(public result) {
        super()
    }
}

@typeName('StopPrint')
class StopPrint extends Action {
    constructor() {
        super()
    }
}

var timer;

export const actionCreators = {
    get: (id, skip, limit, p1?, p2?): ActionCreator => (dispatch, getState) => {
        const filter =
            ((p1 || p2) &&
                '&filter=[' +
                (p1 ? `{"filter":"DateFrom","value":"${p1}"},` : '') +
                (p2 ? `{"filter":"DateTo","value":"${p2}"}` : '') +
                ']') ||
            ''
        api
            .get(
                `Label/GetAllLabelsByTaskPrintId?taskPrintId=${id}&skip=${skip}&limit=${limit}${filter}`
            )
            .then(result => {
                dispatch(new Get(result))
            })
    },
    getLinks: (id): ActionCreator => (dispatch, getState) => {
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

        api.get(`Train/GetAll?skip=0&limit=1000`).then(res => {
            let m = []
            if (res.data) {
                m = res.data.map(el => {
                    return { label: el.name, value: el.id }
                })
                dispatch(new GetLinks(m, 'trains'))
            }
        })

        api
            .get(`Label/GetPrintTaskByIdGetPrintTaskById?taskPrintId=${id}`)
            .then(res => {
                dispatch(new GetLinks(res, 'tag'))
            })
    },
    getVagons: (id): ActionCreator => (dispatch, getState) => {
        api.get(`Carriage/GetByTrainId?train_id=${id}`).then(res => {
            let m = []
            if (res) {
                m = res.map(el => {
                    return {
                        label: `${el.serial || ''} (${el.number || ''}, ${modelTypes[
                            el.model.modelType
                        ] || ''})`,
                        value: el.id
                    }
                })
                dispatch(new GetLinks(m, 'vagons'))
            }
        })
    },
    getEq: (id): ActionCreator => (dispatch, getState) => {
        api
            .get(`Model/GetEquipmentsByCarriage?carriage_id=${id}&isMark=true`)
            .then(res => {
                let m = []
                if (res) {
                    m = res.map(el => {
                        return { label: el.equipmentName, value: el.equipmentModelId }
                    })
                    dispatch(new GetLinks(m, 'equips'))
                }
            })
    },
    add: (data): any => (dispatch, getState) => {
        return api.post(`Label/AddLabelWithTaskPtintsItem`, data).then(res => {
            store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
            dispatch(new Add(res))
        })
    },
    del: (id): ActionCreator => (dispatch, getState) => {
        store.dispatch(
            new ShowDeleteDialog('', () => {
                api.delete(`Label/DeleteTaskPrintItem`, { id: id }).then(() => {
                    dispatch(new Del(id))
                })
            })
        )
    },
    save: (data): any => (dispatch, getState) => {
        return api
            .post(`Label/AddOrUpdateTaskPrints`, {
                LabelType: data.type,
                TemplateLabelId: data.temp || null,
                id: data.id,
                Name: data.label
            })
            .then(() => {
                store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
            })
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
    print: (request, printData): ActionCreator => (dispatch, getState) => {
        api.post(
            `Label/GetSelectedLabelsByTaskPrintId`,
            request
            ).then(rows => {
                api.post(`http://localhost:5050/service/SavePrinterSettings`, {
                    ConnectionType: printData.connectionType || 0,
                    CorpTagCOMPort: printData.comPort || '',
                    RfidTagWriterDeviceType: printData.modelName || '0',
                    SelectedPrinter: printData.printerName || '',
                    TagWriterTxPower: printData.power || '0',
                    Template: printData.template || ''
                })
                    .then(res => {
                        if (printData.paper) {
                            api
                                .post(
                                    `http://localhost:5050/service/Print`,
                                    rows.map((row, index) => ({
                                        Id: index,
                                        Rfid: row.rfid,
                                        Name: row.equipmentName,
                                        RoomFullName: row.parentName || '',
                                        InvCode: row.trainName,
                                        VinCode: row.carriageNumber,
                                        CompanyName: row.carriageSerialNumber || ''
                                    }))
                                )
                                .then(res => {

                                    dispatch(new Print())

                                    //rows &&
                                    //    rows.forEach(row => {
                                    //        api
                                    //            .post(`Label/UpdateTimePrinted`, { id: row.id })
                                    //            .then(res => {
                                    //                dispatch(new Add(res))
                                    //            })
                                    //    })

                                    actionCreators.getPrintState(dispatch);
                                    dispatch(new TagPrint_Status_PRINT(true));
                                })
                        } else {
                            if (rows && rows.length) {
                                dispatch(new TagPrint_Status_PRINT(true))

                                var rfid = rows[0].rfid;

                                api.post(`http://localhost:5050/service/PrintCorpTagRfid`, rfid).then(x => {
                                    dispatch(new TagPrint_Status_PRINT(undefined))
                                }).fail(responce => {
                                    var errorMessage = responce.responseText
                                    dispatch(new ShowTooltip(errorMessage, 1))
                                    dispatch(new TagPrint_Status_PRINT(undefined))
                                })
                            }
                        }
                    })

                dispatch(new Add(rows))
            })
    },
    selectRowAll: (checked): ActionCreator => (dispatch, getState) => {
        dispatch(new SelectAll(checked))
    },
    selectRow: (row, checked): ActionCreator => (dispatch, getState) => {
        dispatch(new Select(row, checked))
    },
    //unselectRow: (row): ActionCreator => (dispatch, getState) => {
    //    dispatch(new Unselect(row))
    //},
    getPrintState: (dispatch) => {
        timer = setInterval(() => {

            api.post('http://localhost:5050/service/PrintProgress', {}).then(res => {

                if (res)
                    dispatch(new PrintState(res))
                else {
                    clearInterval(timer)
                    dispatch(new TagPrint_Status_PRINT(undefined))
                    dispatch(new ShowTooltip('Печать завершена', 0));
                }
            }).fail(() => {
                clearInterval(timer)
                dispatch(new TagPrint_Status_PRINT(undefined))
            })
        }, 500);
    },
    stopPrint: (): ActionCreator => (dispatch, getState) => {
        api.post('http://localhost:5050/service/StopPrinted', {}).then(res => {

            if (timer)
                clearInterval(timer)

            dispatch(new StopPrint())
            dispatch(new TagPrint_Status_PRINT(undefined))
        })
    }
}

export const reducer: Reducer<State> = (state, action: any) => {
    if (isActionType(action, TagPrint_Status_PRINT)) {
        return { ...state, isStartPrint: action.isStartPrint }
    }
    else
        if (isActionType(action, Get)) {

            if (state.isSelectedAll)
                action.result && action.result.data.map(x =>
                    x.selected = state.selectedRows.findIndex(s => s.id == x.id) >= 0 ?
                        state.selectedRows[state.selectedRows.findIndex(s => s.id == x.id)].selected
                        : x.selected || state.isSelectedAll
                )

            return { ...state, result: action.result, reload: false }
        }

    if (isActionType(action, GetLinks)) {
        return { ...state, [action.t]: action.result }
    }

    if (isActionType(action, Add)) {
        return {
            ...state,
            reload: true,
            vagons: [],
            equips: []
        }
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

    if (isActionType(action, SelectAll)) {
        const result = JSON.parse(JSON.stringify(state.result))
        result.data.map(x => x.selected = action.val)

        var selectedRows = [...state.selectedRows]
        if (!action.val)
            selectedRows = []

        return { ...state, result: result, isSelectedAll: action.val, selectedRows: [...selectedRows] }
    }

    if (isActionType(action, Select)) {
        const result = JSON.parse(JSON.stringify(state.result))
        const idx = findRowIdx(result.data, action.result.id)
        result.data[idx].selected = action.val
        const selectedRows = [...state.selectedRows]

        if (!action.val && !state.isSelectedAll)
            selectedRows.splice(selectedRows.findIndex(x => x.id == result.data[idx].id), 1)
        else
            selectedRows.push(result.data[idx])

        return { ...state, result: result, selectedRows: [...selectedRows] }
    }

    //if (isActionType(action, Unselect)) {
    //    const result = JSON.parse(JSON.stringify(state.result))
    //    const idx = findRowIdx(result.data, action.result.id)
    //    result.data[idx].selected = false

    //    const selectedRows = [...state.selectedRows]
    //    remove(selectedRows, x => x.id == action.result.id)

    //    return { ...state, result: result, selectedRows: [...selectedRows] }
    //}

    if (isActionType(action, Print)) {
        return { ...state, print1: true, procent: 0 }
    }

    if (isActionType(action, PrintState)) {
        let p = 100
        try {
            p = action.result.progress[0] / action.result.progress[1] * 100
        } catch (err) { }
        return { ...state, procent: p, print1: p < 100 }
    }

    if (isActionType(action, StopPrint)) {
        return { ...state, print1: false, procent: 999 }
    }

    return (
        state || {
            result: { data: [], total: 0 },
            reload: false,
            id: null,
            templates: [],
            tag: {},
            trains: [],
            vagons: [],
            equips: [],
            printers: null,
            print1: false,
            procent: 0,
            selectedRows: [],
            isSelectedAll: false
        }
    )
}
