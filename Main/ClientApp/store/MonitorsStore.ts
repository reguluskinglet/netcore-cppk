import { push } from 'react-router-redux'
import { Action, isActionType, Reducer, typeName } from 'redux-typed'
import { RequestEvent, ShowTooltip, ShowDeleteDialog, findRowIdx } from '../common'
import { Storage, UserService } from '../services'
import api from '../services/rest'
import { ActionCreator } from '.'
import store from '../main'
import _ from 'lodash'

interface IData {
    data: Array<any>;
    total: number;
}

export interface State {
    result: IData;
    reload: boolean;
    screenTypes: any;
}

@typeName('Monitors_GetCategoriesAction2')
class GetCategoriesAction extends Action {
    constructor(public result) {
    super();
    }
}

@typeName('Monitors_DelCategoryAction2')
class DelCategoryAction extends Action {
    constructor(public id) {
        super()
    }
}

@typeName('Monitors_AddPanelAction2')
class AddPanelAction extends Action {
    constructor(public data) {
    super();
    }
}

@typeName('Monitors_DelPanelAction2')
class DelPanelAction extends Action {
    constructor(public boxId, public displayId, public panelId, public dispatch = null) {
    super();
    }
}

@typeName('Monitors_ExpandCategoryAction2')
class ExpandCategoryAction extends Action {
    constructor(public id) {
    super();
    }
}

@typeName('Monitors_UnexpandCategoryAction2')
class UnexpandCategoryAction extends Action {
    constructor(public id) {
    super();
    }
}

@typeName('Monitors_ShowAddCategoryAction2')
class ShowAddCategoryAction extends Action {
    constructor(public rowId, public innerRowId) {
    super();
    }
}

@typeName('Monitors_HideAddCategoryAction2')
class HideAddCategoryAction extends Action {
    constructor(public rowId, public innerRowId) {
    super();
    }
}

@typeName('Monitors_AddEqCategoryAction2')
class AddEqCategoryAction extends Action {
    constructor(public id) {
    super();
    }
}

@typeName('Monitors_ShowEdit2')
class ShowEdit extends Action {
    constructor(public id) {
    super();
    }
}

@typeName('Monitors_HideEdit2')
class HideEdit extends Action {
    constructor(public id) {
    super();
    }
}

@typeName('Monitors_ShowInnerEdit2')
class ShowInnerEdit extends Action {
    constructor(public id, public innerId) {
    super();
    }
}

@typeName('Monitors_HideInnerEdit2')
class HideInnerEdit extends Action {
    constructor(public id, public innerId) {
    super();
    }
}

@typeName('Monitors_ExpandInnerAction')
class ExpandInnerAction extends Action {
    constructor(public id, public innerId) {
    super();
    }
}

@typeName('Monitors_UnexpandInnerAction')
class UnexpandInnerAction extends Action {
    constructor(public id, public innerId) {
    super();
    }
}

@typeName('Monitors_GetScreenTypes')
class GetScreenTypes extends Action {
    constructor(public result) {
    super();
    }
}

@typeName('Monitors_HideEditors2')
export class HideEditors extends Action {
    constructor() {
    super();
    }
}

export const actionCreators = {
    getCategories: (skip, limit, p1?, p2?): ActionCreator => (dispatch, getState) => {
        const filter =
            ((p1 || p2) &&
            '&filter=[' +
                (p1 ? `{"filter":"name","value":"${p1}"},` : '') +
                (p2 ? `{"filter":"brigadeType","value":${p2}}` : '') +
                ']') ||
            '';
        api.get(`TvPanelsSetup/GetAllBoxes?skip=${skip}&limit=${limit}${filter}`).then(result => {
            dispatch(new GetCategoriesAction(result));
            api.get(`TvPanelsSetup/GetAllScreenTypes`).then(res => {
                dispatch(new GetScreenTypes(res));
            });
        });
    },
    delCategory: (boxId): ActionCreator => (dispatch, getState) => {
        store.dispatch(
            new ShowDeleteDialog('', () => {
                api.delete(`TvPanelsSetup/DeleteBox?boxId=${boxId}`, {}).then(() => {
                    dispatch(new DelCategoryAction(boxId))
                })
            })
        )
    },
    expandRow: (id, showAdd?): ActionCreator => (dispatch, getState) => {
        dispatch(new ExpandCategoryAction(id));
    if (showAdd) dispatch(new ShowAddCategoryAction(id, null));
    },
    unexpandRow: (id): ActionCreator => (dispatch, getState) => {
        dispatch(new UnexpandCategoryAction(id));
    },
    showAdd: (rowId, innerRowId): ActionCreator => (dispatch, getState) => {
        dispatch(new ShowAddCategoryAction(rowId, innerRowId));
    },
    hideAdd: (rowId, innerRowId): ActionCreator => (dispatch, getState) => {
        dispatch(new HideAddCategoryAction(rowId, innerRowId));
    },
    addPanel: (data): ActionCreator => (dispatch, getState) => {
        api.post(`TvPanelsSetup/AddPanel`, data).then(resultAdd => {
            store.dispatch(new ShowTooltip('Данные успешно сохранены!', 0));
            data.id = resultAdd.id;
            dispatch(new AddPanelAction(data));
        });
    },
    delPanel: (boxId, displayId, panelId): ActionCreator => (dispatch, getState) => {
        store.dispatch(         
            new ShowDeleteDialog('', () => {
                api.delete(`TvPanelsSetup/DeletePanel?panelId=${panelId}`, {}).then(() => {
                dispatch(new DelPanelAction(boxId, displayId, panelId,dispatch));
            });
            })
        );
    },

    showEdit: (id): ActionCreator => (dispatch, getState) => {
        dispatch(new ShowEdit(id));
    },
    hideEdit: (id): ActionCreator => (dispatch, getState) => {
        dispatch(new HideEdit(id));
    },
    showInnerEdit: (id, innerId): ActionCreator => (dispatch, getState) => {
        dispatch(new ShowInnerEdit(id, innerId));
    },
    hideInnerEdit: (id, innerId): ActionCreator => (dispatch, getState) => {
        dispatch(new HideInnerEdit(id, innerId));
    },
    expandInner: (id, innerId): ActionCreator => (dispatch, getState) => {
        dispatch(new ExpandInnerAction(id, innerId));
    },
    unexpandInner: (id, innerId): ActionCreator => (dispatch, getState) => {
        dispatch(new UnexpandInnerAction(id, innerId));
    },
    hideEditors: (): ActionCreator => (dispatch, getState) => {
        dispatch(new HideEditors());
    }
};

export const reducer: Reducer<State> = (state, action: any) => {
    function findRowIdx(data, id) {
        const row = data.find(r => r.id === id);
        const idx = data.indexOf(row);
        return idx;
    }

    function hideEditors(data) {
        data.forEach(r => {
            r.showAdd = false;
            r.showEdit = false;
            r.equipments &&
            r.equipments.forEach(ir => {
                ir.showAdd = false;
                ir.showEdit = false;
            });
        });
        return data;
    }

    if (isActionType(action, GetCategoriesAction)) {
        const data = _.map(action.result.data, box => {
            const panels = _.values(_.mapValues(box.panels, (value, key) => ({ id: parseInt(key), monitors: value })));
            return { ...box, panels };
        });

        return { ...state, result: { data, total: action.result.total }, reload: false };
    }

    if (isActionType(action, DelCategoryAction)) {
        return { ...state, reload: true }
    }

    if (isActionType(action, GetScreenTypes)) {
        return { ...state, screenTypes: action.result.map(type => ({ value: type.id, label: type.name })), reload: false };
    }

    if (isActionType(action, AddPanelAction)) {
        const result = JSON.parse(JSON.stringify(state.result));
        const idx = findRowIdx(result.data, action.data.TvBoxId);
        const innerIdx = findRowIdx(result.data[idx].panels, action.data.Number);
        const type = _.find(state.screenTypes, type => type.value === action.data.type);
        const monitors = result.data[idx].panels[innerIdx].monitors;
        const maxId = _.get(_.maxBy(monitors, 'id'), 'id', 0);

        result.data[idx].panels[innerIdx].monitors = [
            ...monitors,
            { id: action.data.id || maxId + 1, type: { id: type.value, name: type.label } }
        ];
        result.data[idx].panels[innerIdx].showAdd = false;
        return { ...state, result };
    }

    if (isActionType(action, DelPanelAction)) {
       
        const result = JSON.parse(JSON.stringify(state.result));
        const idx = findRowIdx(result.data, action.boxId);
        const innerIdx = findRowIdx(result.data[idx].panels, action.displayId);
        const monitors = _.filter(result.data[idx].panels[innerIdx].monitors, monitor => monitor.id !== action.panelId);

        result.data[idx].panels[innerIdx].monitors = monitors;
        if (!monitors.length) {
            const type = _.find(state.screenTypes, type => type.value === 0);
            result.data[idx].panels[innerIdx].monitors = [
                ...monitors,
                { id: action.panelId, type: { id: type.value, name: type.label } }
            ];
        }
        return { ...state, result };
    }

    if (isActionType(action, ExpandCategoryAction)) {
        const result = JSON.parse(JSON.stringify(state.result));
        const idx = findRowIdx(result.data, action.id);
        result.data[idx].expanded = true;
        result.data[idx].showAdd = false;
        return { ...state, result: result, reload: false };
    }

    if (isActionType(action, UnexpandCategoryAction)) {
        const result = JSON.parse(JSON.stringify(state.result));
        const idx = findRowIdx(result.data, action.id);
        result.data[idx].expanded = false;
        return { ...state, result: result, reload: false };
    }

    if (isActionType(action, ShowAddCategoryAction)) {
        const result = JSON.parse(JSON.stringify(state.result));
        const idx = findRowIdx(result.data, action.rowId);
        const innerIdx = findRowIdx(result.data[idx].panels, action.innerRowId);
        result.data[idx].panels[innerIdx].showAdd = true;
        result.data[idx].panels[innerIdx].expanded = true;
        hideEditors(result.data);
        return { ...state, result: result, reload: false };
    }

    if (isActionType(action, HideAddCategoryAction)) {
        const result = JSON.parse(JSON.stringify(state.result));
        const idx = findRowIdx(result.data, action.rowId);
        const innerIdx = findRowIdx(result.data[idx].panels, action.innerRowId);
        result.data[idx].panels[innerIdx].showAdd = false;
        hideEditors(result.data);
        return { ...state, result: result, reload: false };
    }

    if (isActionType(action, ShowEdit)) {
        const result = JSON.parse(JSON.stringify(state.result));
        const idx = findRowIdx(result.data, action.id);
        hideEditors(result.data);
        result.data[idx].showEdit = true;
        return { ...state, result: result };
    }

    if (isActionType(action, HideEdit)) {
        const result = JSON.parse(JSON.stringify(state.result));
        const idx = findRowIdx(result.data, action.id);
        result.data[idx].showEdit = false;
        return { ...state, result: result };
    }

    if (isActionType(action, ShowInnerEdit)) {
        const result = JSON.parse(JSON.stringify(state.result));
        const idx = findRowIdx(result.data, action.id);
        const innerIdx = findRowIdx(result.data[idx].equipments, action.innerId);
        hideEditors(result.data);
        result.data[idx].equipments[innerIdx].showEdit = true;
        return { ...state, result: result };
    }

    if (isActionType(action, ExpandInnerAction)) {
        const result = JSON.parse(JSON.stringify(state.result));
        const idx = findRowIdx(result.data, action.id);
        const innerIdx = findRowIdx(result.data[idx].panels, action.innerId);
        result.data[idx].panels[innerIdx].expanded = true;
        result.data[idx].panels[innerIdx].showAdd = false;
        return { ...state, result: result };
    }

    if (isActionType(action, UnexpandInnerAction)) {
        const result = JSON.parse(JSON.stringify(state.result));
        const idx = findRowIdx(result.data, action.id);
        const innerIdx = findRowIdx(result.data[idx].panels, action.innerId);
        result.data[idx].panels[innerIdx].expanded = false;
        return { ...state, result: result };
    }

    if (isActionType(action, HideInnerEdit)) {
        const result = JSON.parse(JSON.stringify(state.result));
        const idx = findRowIdx(result.data, action.id);
        const innerIdx = findRowIdx(result.data[idx].equipments, action.innerId);
        result.data[idx].equipments[innerIdx].showEdit = false;
        return { ...state, result: result };
    }

    if (isActionType(action, HideEditors)) {
        const result = JSON.parse(JSON.stringify(state.result));
        hideEditors(result.data);
        return { ...state, result: result };
    }

    return state || { result: { data: [], total: 0 }, reload: false, screenTypes: [] };
};
