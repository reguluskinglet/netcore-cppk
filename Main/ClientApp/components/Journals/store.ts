import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, ShowTooltip, ShowDeleteDialog, ShowDialog, LoadingOverlay, ReportType, ToogleDialog } from '../../common'
import { Storage, UserService } from '../../services'
import api from '../../services/rest'
import { ActionCreator } from '../../store'
import store from '../../main'
import { find, get, set } from 'lodash'
import { dataSourceSelect } from '../../resource'

export interface State {
    statuses?: SelectItem[],
    stantions?: SelectItem[],
    trains?: SelectItem[],
    carriages?: SelectItem[],
    equipments?: SelectItem[],
    faults?: SelectItem[]
}


@typeName("Journal.TaskCreate")
export class JournalTaskCreate extends Action {
    constructor
        (
        public data: any
        ) {
        super();
    }
}

@typeName("Journal.UnloadState")
export class JournalUnloadState extends Action {
}

export const actionCreators = {
    unloadState: (): ActionCreator => (dispatch, getState) => {
        dispatch(new JournalUnloadState())
    },
    getReport: (type: ReportType, data: any): ActionCreator => (dispatch, getState) => {

        var url = '';

        switch (type) {
            case ReportType.a:
                url = 'ReportPdf/GetTu152Pdf';
                break;
            case ReportType.b:
                url = 'Task/GetActPdf';
                break;
            case ReportType.c:
                url = 'Task/GetJournalPdf';
                break;
            case ReportType.d:
                url = 'Task/GetJournalTablePdf';
                break;
        }

        api.post(url, data, false, true).then(result => {

        }).done(x => {
            dispatch(new LoadingOverlay(false));
        }).fail(x => { dispatch(new LoadingOverlay(false)); })

        dispatch(new LoadingOverlay(true));
    },
    getInspection: (id): ActionCreator => (dispatch, getState) => {
        api.get(`EventsTable/InspectionByIdForEventTable?id=${id}`).then(data => {
            dispatch(new ShowDialog(data, 0, 'journal.inspection', true));
        })
    },
    getTask: (id,action): ActionCreator => (dispatch, getState) => {
        //api.get(`Service/GetTask`).then(data => {
        //    dispatch(new ShowDialog(data, 0, 'journal.task', true));
        //})
        api.get(`Task/GetTaskById?id=${id}`,).then(data => {
            dispatch(new ShowDialog(data, 0, 'journal.task', true, action));
        })
    },
    updateTask: (data, action): ActionCreator => (dispatch, getState) => {
        api.post(`Task/Update`, data).then(x => {
            action();
            dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
            dispatch(new ToogleDialog(false))
            //traintaskId
            //api.get(`Service/GetTask`).then(result => {
            //    dispatch(new ShowDialog(result, 0, 'journal.task', true));
            //    dispatch(new ShowTooltip('Данные успешно сохранены!', 0))
            //    //action();
            //})
        })
    },
    createNewTask:(action): ActionCreator => (dispatch, getState) => {


        api.get('Train/GetAll?skip=0&limit=9999').then(trains=> {

            var trainSelectItem: SelectItem[] = trains.data.map(x => { return { value: x.id, text: x.name } })

            var user = Storage.getUserInfo().name;

            dispatch(new JournalTaskCreate({ trains: trainSelectItem }))
            dispatch(new ShowDialog(user, 0, 'journal.taskcreate', true, action));
        })
    },
    getCarriagesByTrainId: (id): ActionCreator => (dispatch, getState) => {

        const modelTypes = ['головной', 'моторный', 'прицепной']

        if (!id)
            dispatch(new JournalTaskCreate({ equipments: undefined, faults: undefined, carriages: undefined }))
        else
            api.get(`Carriage/GetByTrainId?train_id=${id}`).then((carriages: { serial: string, number: number, id: number, model: { modelType: number } }[]) => {

                var carriageSelectItem = carriages.map(x => {
                    return {
                        value: x.id,
                        text: `${x.serial} (вагон ${x.number},${modelTypes[x.model.modelType]})`
                    }
                })

                dispatch(new JournalTaskCreate({ carriages: carriageSelectItem }))
            })
    },
  
    getEquipmentsByCarriage: (id): ActionCreator => (dispatch, getState) => {
        
        if (!id)
            dispatch(new JournalTaskCreate({ equipments: undefined, faults: undefined }))
        else
            api.get(`Model/GetEquipmentsByCarriage?carriage_id=${id}&sortToTaskList=true`)
                .then((result: { equipmentModelId: number, equipmentId: number, equipmentName: string }[]) => {

                    var equipmentSelectItem = result.map(x => {
                        return {
                            value: x.equipmentModelId,
                            text: x.equipmentName
                        }
                    })

                    dispatch(new JournalTaskCreate({ equipments: equipmentSelectItem, faults: undefined }))
            })
    },
    getFaultByEquipmentId: (id): ActionCreator => (dispatch, getState) => {

        api.get(`Fault/GetByEquipmentModelId?id=${id}`)
            .then((result: { id: number, name: string }[]) => {

                var faultSelectItem = result.map(x => {
                    return {
                        value: x.id,
                        text: x.name
                    }
                })

                dispatch(new JournalTaskCreate({ faults: faultSelectItem }))
            })
    },
    getAvaibleStatuses: (brigadeType): ActionCreator => (dispatch, getState) => {

        api.get(`Task/GetAvaibleStatuses?taskStatus=null&executorBrigadeType=${brigadeType}`)
            .then((result: number[]) => {

                var statusSelectItem: SelectItem[] = result.map(x => dataSourceSelect.status.find(s => x === s.value))

                dispatch(new JournalTaskCreate({ statuses: statusSelectItem }))
            })
    },
    saveTask: (data, action): ActionCreator => (dispatch, getState) => {

        api.post(`Task/Add`, data).then(message => {

            if (message)
                dispatch(new ShowTooltip(message, 2))

            dispatch(new ToogleDialog(false))
            action();
        })
    },


    //getData: (data, reload = true): ActionCreator => (dispatch, getState) => {
    //    api.post(`EventsTable/EventTable`, data).then(result => {
    //        reload
    //            ? dispatch({ type: 'journals.get', payload: result })
    //            : dispatch({ type: 'journals.getChild', payload: result })
    //    })
    //},
    //getReports: (): ActionCreator => (dispatch, getState) => {
    //    api.post(`EventsTable/Reports`, {}).then(res => {
    //        let m = []
    //        if (res) {
    //            m = res.map(el => {
    //                return { label: el.name, value: el.id }
    //            })
    //            dispatch({ type: 'journals.getLinks', payload: { type: 'reports', data: m } })
    //        }
    //    })
    //},

}

export const reducer: Reducer<State> = (state, action: any) => {

    if (isActionType(action, JournalTaskCreate))
        return { ...state, ...action.data };

    if (isActionType(action, JournalUnloadState))
        return {};

    return state || {}
    //
    //else

    //switch (action.type) {
    //    case 'journals.get':
    //        return { ...state, data: action.payload }

    //    case 'journals.getChild':
    //        const { rows } = action.payload
    //        if (Array.isArray(rows) && rows.length) {
    //            const data = state.data
    //            const parentRow = find(data.rows, row => JSON.stringify(row.id) === JSON.stringify(get(rows, '0.parentId')))
    //            parentRow && set(parentRow, 'childLoaded', true)
    //            return { ...state, data: { ...state.data, rows: data.rows.concat(rows) } }
    //        }

    //    case 'journals.getLinks':
    //        return { ...state, [action.payload.type]: action.payload.data }
    //    case 'journals.getLinks':
    //        return { ...state, [action.payload.type]: action.payload.data }
    //    default:
    //        return state || { data: { columns: [], rows: [], total: 0 }, reports: [] }
    //}
}
