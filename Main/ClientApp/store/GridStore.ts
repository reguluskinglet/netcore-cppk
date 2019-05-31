import { typeName, isActionType, Action, Reducer } from 'redux-typed';
import { ActionCreator } from './';
import { getGrid, getChild } from '../services'
import api from '../services/rest'
import { UpdateRow, GridType, InsertRow, ShowDialog, ToogleDialog, CommandType, FilterInfo } from '../common'
import * as remove from 'lodash/remove'
import * as union from 'lodash/union'
import * as find from "lodash/find"
import * as uniqBy from "lodash/uniqBy"
import { push, replace } from 'react-router-redux'
import { changeArrayItem, swapRow,findRow } from '../UI/utils/utils'

export interface GridState {
    dataSource: GridData<any>
    allColumns: GridColumn[]
    visibleColumns: GridColumn[]
    sortOptions: SortOptions
    filterInfo: FilterInfo[],
    filter: any,
    isEmptyFilter: boolean,

    isLoaded: boolean
    isLoadedPage: boolean,

    data: GridSourceData

    isExpandAll: boolean
    isSelectedAll: boolean

    expandRows: any[]
    selectedRows: any[]
   
    parentIds: any[]
    newRowOpen: boolean
    showRowId?: any
    isNewRow?: boolean

    loadedRows: any[]

    page?:number
}


@typeName("RequestGrid")
class RequestGrid extends Action {

    constructor(
        public isLoadPage: boolean,
        public isLoadedPage?: boolean
    ) {
        super();
    }
}

@typeName("RequestGridFilter")
class RequestGridFilter extends Action {

    constructor(
        public filter: any,
    ) {
        super();
    }
}

@typeName("ReceiveGrid")
class ReceiveGrid extends Action {

    public allColumns: GridColumn[]
    public visibleColumns: GridColumn[]
    public dataSource: GridData<any>
    public sortOptions: SortOptions
    public filterInfo: FilterInfo[]
    public filter: any
    public data: GridSourceData
    public isEmptyFilter: boolean

    constructor(data: any) {
        super();

        this.allColumns = data.allColumns
        this.visibleColumns = data.visibleColumns
        this.dataSource = data.dataSource
        this.sortOptions = data.sortOptions
        this.filterInfo = data.filterInfo
        this.filter = data.filter
        this.data = data.data
        this.isEmptyFilter = data.isEmptyFilter
    }
}

@typeName("ReceiveGridSelectedRows")
class ReceiveGridSelectedRows extends Action {

    public isSelectedAll: boolean
    public selectedRows: any[]
    public page: number

    constructor(data: any) {
        super();

        this.isSelectedAll = data.isSelectedAll
        this.selectedRows = data.selectedRows
        this.page = data.page
    }
}

@typeName("ReceiveGridPage")
class ReceiveGridPage extends Action {

    public page: number
    public dataSource: GridData<any>
    public sortOptions?: SortOptions

    constructor
        (
        data,
        sortOptions?,
        ) {
        super();

        this.dataSource = data.dataSource ? data.dataSource : data
        this.sortOptions = sortOptions
        this.page = data.page
    }
}

@typeName("ReorderColumns")
class ReorderColumns extends Action {
    constructor(
        public columns: GridColumn[],
        public isLoadedPage = false,
        public fromIndex: number,
        public toIndex: number
    ) {
        super();

    }
}

@typeName("ExpandRow")
class ExpandRow extends Action {
    constructor(
        public expandRows: any[],
    ) {
        super();

    }
}


@typeName("ExpandRowAll")
class ExpandRowAll extends Action {
    constructor(
        public isExpandAll?: boolean
    ) {
        super();

    }
}


@typeName("ChangeFilter")
class ChangeFilter extends Action {
    constructor
        (
        public newValue: any,
        public field: string
        ) {
        super();

    }
}

@typeName("ApplyFilter")
class ApplyFilter extends Action {
    constructor
        (
        public dataSource: any,
        public filter: any,
        public isEmptyFilter: boolean
        ) {
        super();

    }
}

@typeName("NewRow")
class NewRow extends Action {
    constructor(public open = true) {
        super();
    }
}

@typeName("SHOW_ROW_DATA")
class ShowRowData extends Action {
    constructor(public showRowId, public isNewRow: boolean) {
        super();
    }
}

@typeName("CHECK_COLUMNS")
class CheckColumns extends Action {
    constructor(public visibleColumns: GridColumn[]) {
        super();
    }
}

@typeName("RECIVE_LOADED_CHILD_ROWS")
class ReciveLoadedChildRows extends Action {
    constructor(
        public datasource: GridData<any>,
        public expandRows: any[],
        public loadRows: any[])
    {
        super();
    }
}

@typeName("UpdateDiffirenceField")
class UpdateDiffirenceField extends Action {
    constructor(public fields) {
        super();
    }
}


let currentRequestGrid: string;

export const actionCreators = {

    showRowData: (row): ActionCreator => (dispatch, getState) => {

        //dispatch(new ShowRowData(showRowId, isNewRow));
    },
    goToDocument: (url): ActionCreator => (dispatch, getState) => {
        dispatch(push(url));
    },

    unloadState: (): ActionCreator => (dispatch, getState) => {
        dispatch(new RequestGrid(false));
    },
    removeConfirm: (message, callback): ActionCreator => (dispatch, getState) => {

        dispatch(new ShowDialog(message, 0, 'confirmRemove', true, callback));
    },
    //removeRows: (gridType: GridType, data, callback): ActionCreator => (dispatch, getState) => {

    //    grud(gridType, RestAction.Remove, data)
    //        .then(data => {
    //            callback()
    //        })
    //        .fail((response) => {

    //            var status = response.status;

    //            if (status == 409)
    //                dispatch(new ShowDialog(response.responseJSON, status, GridType[gridType], true));
    //            else if (status == 406)
    //                dispatch(new ShowDialog(response.responseText, status, 'error', true));
    //            else if (status == 500)
    //                dispatch(new ShowDialog('Не удалось удалить записи, имеются связанные документы.', 406, 'error', true));

    //        })
    //},
    addRow: (): ActionCreator => (dispatch, getState) => {
        dispatch(new ShowRowData('new', true));
    },
    addNewRow: (open = true): ActionCreator => (dispatch, getState) => {
        dispatch(new NewRow(open));
    },
    getGrid: (gridType: GridType, withParameter?, callback?: (data) => void, parameters?): ActionCreator => (dispatch, getState) => {

        currentRequestGrid = GridType[gridType];

        dispatch(new RequestGrid(false));

        if (withParameter != undefined)
            currentRequestGrid += '/' + withParameter;
        getGrid(currentRequestGrid, parameters).then(data => {
            if (callback)
                callback(data);
            dispatch(new ReceiveGrid(data));
        }).fail(response => {
            var status = response.status;
           // var defaultRoute = permission.defaultRoute();
            //if (status == 403)
            //    dispatch(push(defaultRoute))
        })
    },
    pageSelected: (pageNumber): ActionCreator => (dispatch, getState) => {

        getGrid(currentRequestGrid, { page: pageNumber }, CommandType.Page).then(data => {
            dispatch(new ReceiveGridPage(data));
        })

        dispatch(new RequestGrid(true));
    },
    //updateGrid: (id?, hasSaveRequest?: boolean): ActionCreator => (dispatch, getState) => {

    //    getGrid(currentRequestGrid, null, CommandType.Page).then(data => {
    //        dispatch(new ReceiveGridPage(data, null, true));

    //        //if (id)
    //        dispatch(new ShowRowData(null, false));

    //        dispatch(new ToogleDialog(false))

    //        if (hasSaveRequest)
    //            dispatch(new SaveRequestEnd())
    //    })

    //    dispatch(new RequestGrid(true));
    //    dispatch(new ClearGridCache())

    //},
    changeFilter: (newValue, field): ActionCreator => (dispatch, getState) => {
        dispatch(new ChangeFilter(newValue, field))
    },
    applyFilter: (filter): ActionCreator => (dispatch, getState) => {

        getGrid(currentRequestGrid, { filter: filter }, CommandType.Page).then(data => {
            dispatch(new ApplyFilter(data.dataSource, filter, data.isEmptyFilter));
        })

        dispatch(new RequestGridFilter(filter));
    },
    sortColumn: (data): ActionCreator => (dispatch, getState) => {

        getGrid(currentRequestGrid, data, CommandType.Page).then(result => {
            dispatch(new ReceiveGridPage(result, data.sortOptions));
        })

        dispatch(new RequestGrid(true));
    },
    //dragEndColumn: (columns: GridColumn[], stopDrag: boolean, fromIndex?: number, toIndex?: number): ActionCreator => (dispatch, getState) => {

    //    if (stopDrag)
    //        requestGrid(currentRequestGrid, { visibleColumns: columns }, CommandType.Save)

    //    dispatch(new ReorderColumns(columns, true, fromIndex, toIndex))
    //},
    //resizeEndColumn: (columns: GridColumn[]): ActionCreator => (dispatch, getState) => {
    //    //saveGridState({ visibleColumns: columns })
    //},
    //toggleExpandAll: (): ActionCreator => (dispatch, getState) => {
    //    dispatch(new ExpandRowAll())
    //},
    //toggleSelectedAll: (): ActionCreator => (dispatch, getState) => {
    //    dispatch(new SelectedRowAll())
    //},
    selectAll: (isSelectedAll: boolean): ActionCreator => (dispatch, getState) => {
        getGrid(currentRequestGrid, { isSelectedAll: isSelectedAll, page: getState().grid.page }/*, CommandType.None*/).then(result => {
            dispatch(new ReceiveGridSelectedRows(result))
        })

        dispatch(new RequestGrid(true))
    },
    selectRow: (row, isSelected: boolean): ActionCreator => (dispatch, getState) => {

        getGrid(currentRequestGrid, { selectedRow: { rowId: row.id, parentId: row.parentId, isSelected: isSelected } }).then(result => {
            dispatch(new ReceiveGridSelectedRows(result))
        })

        dispatch(new RequestGrid(true));
    },
    onExpandRow: (currentExpandRows: any[], id: string): ActionCreator => (dispatch, getState) => {

        var gridState = getState().grid;

        var requestId = id.split('_')[1];

        if (gridState.loadedRows.indexOf(id) < 0) {
            var source: GridData<any> = { ...gridState.dataSource };
            var loadRows = gridState.loadedRows.concat(id)
            var expandRows = currentExpandRows.concat(id)

            getChild(currentRequestGrid, requestId).then((data) => {

                var row = find(source.rows, x => x.id === id);

                row.child = data;

                dispatch(new ReciveLoadedChildRows(source, expandRows, loadRows))
            })
        }
        else
        {
            var rows = changeArrayItem(currentExpandRows, id);

            dispatch(new ExpandRow(rows));
        }

        //saveGridState({ visibleColumns: columns })


    },
    clearFilter: (): ActionCreator => (dispatch, getState) => {

        getGrid(currentRequestGrid, null, CommandType.Clear).then(data => {
            dispatch(new ReceiveGrid(data));
        })

        dispatch(new RequestGrid(true));
    },

    checkColumns: (data: { column: GridColumn, visibleColumns: GridColumn[] }, checked: boolean): ActionCreator => (dispatch, getState) => {

        //if (checked)
        //    data.visibleColumns.push(data.column);
        //else
        //    remove(data.visibleColumns, x => x.name == data.column.name);

        //requestGrid(currentRequestGrid, { visibleColumns: data.visibleColumns }, CommandType.Page).then(data => {
        //    dispatch(new ReceiveGridPage(data));
        //})

        //dispatch(new CheckColumns(data.visibleColumns))
    },

    //changeFilter: (newValue, field): ActionCreator => (dispatch, getState) => {

    //    filterCache[field] = newValue;
    //    dispatch(new ChangeFilter(newValue, field))
    //},
}

const unloadedState: GridState = {
    sortOptions: null,
    dataSource: null,
    allColumns: [],
    visibleColumns: [],
    
    isLoaded: false,
    isLoadedPage: false,

    isEmptyFilter: false,
    filterInfo: null,
    filter: null,

    //  gridType: null,
    isSelectedAll: false,
    selectedRows: [],

    isExpandAll: false,
    expandRows: [],

    parentIds: [],

    data: null,
    newRowOpen: false,

    loadedRows:[]
};

export const reducer: Reducer<GridState> = (state, action: any) => {

    if (isActionType(action, RequestGrid))
        return { ...state, isLoaded: action.isLoadPage, isLoadedPage: !action.isLoadPage };
    else if (isActionType(action, RequestGridFilter)) {
        var filter = { ...state.filter, ...action.filter }

        return { ...state, filter: filter, isLoaded: true, isLoadedPage: false };
    }
    else if (isActionType(action, ShowRowData)) {
        var current = state.showRowId === action.showRowId ? null : action.showRowId;

        return { ...state, showRowId: current, isNewRow: action.isNewRow };
    }
    else if (isActionType(action, ReceiveGrid))
        return {
            sortOptions: action.sortOptions,
            dataSource: action.dataSource,
            allColumns: action.allColumns,
            visibleColumns: action.visibleColumns,
            filterInfo: action.filterInfo,
            filter: action.filter,
            isEmptyFilter: action.isEmptyFilter,
            data: action.data,
            isLoaded: true,
            isLoadedPage: true,
            isExpandAll: false,
            expandRows: [],
            selectedRows: [],
            isSelectedAll: false,
            parentIds: [],
            newRowOpen: false,
            loadedRows: [],
        };
    else if (isActionType(action, ReceiveGridSelectedRows))
        return { ...state, isSelectedAll: action.isSelectedAll, selectedRows: action.selectedRows, page: action.page, isLoaded: true, isLoadedPage: true };
    else if (isActionType(action, ReceiveGridPage)) {

        //if (action.clearSelectedRows)
        //    return { ...state, dataSource: action.dataSource, isLoadedPage: true, selectedRows: [], newRowOpen: false };

        if (!action.sortOptions)
            return { ...state, page: action.page, dataSource: action.dataSource, isLoadedPage: true, newRowOpen: false, loadedRows: [], expandRows: []  };

        return { ...state, page: action.page, dataSource: action.dataSource, sortOptions: action.sortOptions, isLoadedPage: true, newRowOpen: false, loadedRows: [], expandRows: []  };
    }
    else if (isActionType(action, ExpandRow))
        return { ...state, expandRows: [...action.expandRows] }
    else if (isActionType(action, ExpandRowAll))
        return { ...state, isExpandAll: !state.isExpandAll, expandRows: [] }
    else if (isActionType(action, ReorderColumns)) {

        var dataSource = { ...state.dataSource }
        var rows = dataSource.rows

        for (var i = 0; i < rows.length; i++)
            swapRow(rows[i], action.fromIndex, action.toIndex)

        return { ...state, visibleColumns: action.columns, dataSource: dataSource }
    }
    else if (isActionType(action, ChangeFilter)) {

        var newStateFilter = { ...state.filter, [action.field]: action.newValue }

        return { ...state, filter: newStateFilter }
    }
    else if (isActionType(action, UpdateRow)) {

        var dataSource = { ...state.dataSource };

        for (var i = 0; i < dataSource.rows.length; i++) {

            var row = findRow(dataSource.rows[i], action.row.id, action.row);

            if (row === true) {
                dataSource.rows[i] = action.row;
                break;
            }

            if (row === false)
                break;
        }
        return { ...state, dataSource: dataSource, showRowId: null }
    }
    else if (isActionType(action, ApplyFilter)) {

        var filter = { ...state.filter, ...action.filter };

        return { ...state, filter: filter, dataSource: action.dataSource, isEmptyFilter: action.isEmptyFilter, isLoadedPage: true, loadedRows: [], expandRows: [] };
    }
    else if (isActionType(action, NewRow)) {

        return { ...state, newRowOpen: action.open };
    }
    else if (isActionType(action, InsertRow)) {

        var dataSource = { ...state.dataSource };

        dataSource.rows.unshift(action.row);
        dataSource.pager.totalElementsCount++;

        return { ...state, dataSource: dataSource, newRowOpen: false };
    }
    else if (isActionType(action, CheckColumns)) {
        return { ...state, visibleColumns: action.visibleColumns };
    }
    else if (isActionType(action, ReciveLoadedChildRows))
        return { ...state, dataSource: action.datasource, loadedRows: action.loadRows, expandRows: action.expandRows };
    //else if (isActionType(action, UpdateDiffirenceField)) {
    //    var additionData = { ...state.additionalData, fields: action.fields }

    //    return { ...state, additionalData: additionData };
    //}
    else
        return state || unloadedState;
}




