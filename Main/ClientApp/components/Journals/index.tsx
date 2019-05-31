import './index.scss'
import * as React from 'react'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../store';
import * as GridStore from '../../store/GridStore';
import * as DialogStore from '../../store/DialogStore';
import * as CommonStore from '../../store/CommonStore';
import { GridType, journalTitleFilter, ReportType } from '../../common'
import { Loading } from '../../UI/loading'
import { isEqual } from 'lodash'
import { Table } from '../../UI/tables';
import { Filter } from '../Common'
import { RouteComponentProps } from 'react-router'
import { Button } from '../../UI/button'
import * as classnames from 'classnames';
import * as JournalStore from './store'
import { ToolBar, ToolBarButton } from '../../UI/toolbar';
import { Storage } from '../../services'

import { changeArrayItem, swapRow, findRow, getRowById } from '../../UI/utils/utils'
import PagerHelper from 'ClientApp/UI/tables/Pager';

interface State
{
    inspectionIds: any[]
    taskIds: any[],
}

class Journals extends React.Component<Props, State> {

    constructor(props) {
        super(props)

        this.state = {
            inspectionIds: [],
            taskIds: []
        };
    }

    componentWillUnmount() {
        this.props.unloadState();
    }

    componentWillMount() {
        this.props.getGrid(GridType.journal)
    }

    componentWillReceiveProps(nextProps: Props) {

        var inspectionIds = [],
            taskIds = [];

        var selectedRows = nextProps.selectedRows

        if (selectedRows && selectedRows.length > 0) {
            for (var x = 0; x < selectedRows.length; x++) {
                var row = selectedRows[x]

                if(row.isSelected)
                    if (row.rowId.includes('M'))
                        inspectionIds.push(row.rowId.split('_')[1])
                    else if (row.rowId.includes('E'))
                        taskIds.push(row.rowId.split('_')[1])
            }
        }         

        this.setState({
            inspectionIds: inspectionIds,
            taskIds: taskIds
        })        
    }

    mapColumnClass = (row, index, value) => {

        var column = this.props.visibleColumns[index];

        if (column.name === 'hasInspection')
        {
            return row.hasInspection ? "col-m" : "col-i";
        }

        return '';
    }

    showRow = (row, isEdit) => {

        var requestId = row.id.split('_')[1];

        if (row.hasInspection) 
            this.props.getInspection(requestId);
        else
        {
            this.props.getTask(requestId, this.props.clearFilter);
        }
    }

    get getSelectedRows(): any[] {
        return this.props.selectedRows
    }

    get selectedTrain(): number {
        return this.props.filter.trainId;
    }

    additionalFilter = () => {
        return <Button label="Создать задачу" onClick={() => this.props.createNewTask(this.props.clearFilter)} />
    }

    get withCheckBox(): boolean {
        return !this.props.isEmptyFilter
    }

    get gridOptions(): any {
        const { isSelectedAll, sortOptions, filter } = this.props
        return {
            options: {
                selectedRows: this.getSelectedRows,
                isSelectedAll: isSelectedAll,
                sortOptions: sortOptions,
                filter: filter
            }
        }
    }

    render() {
        if (this.props.isLoaded) {

            const { inspectionIds, taskIds } = this.state
            
            var isAdmin = JSON.parse(localStorage["user_info"] || "{}").permissions === -1;
            return <div>
                <ToolBar>
                    <ToolBarButton label="ТУ 152" disabled={!inspectionIds.length && !taskIds.length} onClick={() => { this.props.getReport(ReportType.a, { inspectionIds: inspectionIds, taskIds: taskIds }) }} />
                    <ToolBarButton label="ТУ 308" disabled />
                    <ToolBarButton label="Акт приема" hide={!isAdmin} disabled={false/*!this.selectedTrain*/} onClick={() => { this.props.getReport(ReportType.b, this.selectedTrain) }} />
                    <ToolBarButton label="Печать задач" disabled={!taskIds.length && !inspectionIds.length} onClick={() => { this.props.getReport(ReportType.c, { inspections: inspectionIds, tasks: taskIds }) }} />
                    <ToolBarButton label="Печать таблицы" disabled={false} onClick={() => { this.props.getReport(ReportType.d, this.gridOptions) }} />
                </ToolBar>
                <br/>
                <Filter filterTitle={journalTitleFilter} additionalFilter={this.additionalFilter} />
                <Table mapColumntClass={this.mapColumnClass} showRow={this.showRow} isTree withCheckBox={this.withCheckBox} withSelectAllCheckBox={this.withCheckBox} />
            </div>
        } else {
            return <Loading />
        }
    }
}

const provider = provide(
    (state: ApplicationState) => state.grid,
    { ...GridStore.actionCreators, ...DialogStore.actionCreators, ...JournalStore.actionCreators}
).withExternalProps<RouteComponentProps<any>>()

type Props = typeof provider.allProps;

export default provider.connect(Journals);
