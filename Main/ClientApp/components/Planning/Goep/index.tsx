import './style.scss'
import * as React from 'react'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../../store';
import * as GridStore from '../../../store/GridStore';
import * as DialogStore from '../../../store/DialogStore';
import { GridType, journalTitleFilter } from '../../../common'
import { Loading } from '../../../UI/loading'
import { Table } from '../../../UI/tables';
import { Filter, DayOfWeek } from '../../Common'
import { RouteComponentProps } from 'react-router'
import { Button } from '../../../UI/button'
import * as classnames from 'classnames';
import { ToolBar, ToolBarButton } from '../../../UI/toolbar';
import * as store from './store';

class Goep extends React.Component<Props, undefined> {

    componentWillUnmount() {
        this.props.unloadState();
    }

    componentWillMount() {
        this.props.getGrid(GridType.goep)
    }

    mapRowValue = (row, index, value: string) => {

        if (index == 2) {

            if (!value)
                return <DayOfWeek selectedDays={null} />

            var result = value.split(',').map(x => parseInt(x, 10));
            return <DayOfWeek selectedDays={result} />
        }

        return value;
    }

    mapColumntClass = (row, index, value: string) => {

        if (index == 2)
            return 'days'

        return '';
    }

    showRow = (row, isEdit) => {
        if (isEdit)
            this.props.getTurnoverById(row.id, this.props.clearFilter)
        else 
            this.props.goToDocument('/planning/goep/' + row.id)
    }

    render() {
        if (this.props.isLoaded) {
            return <div>
                <ToolBar>
                    <ToolBarButton label="Добавить" onClick={() => this.props.getTurnoverById(null, this.props.clearFilter)} />
                    <ToolBarButton disabled={this.props.selectedRows.length===0} label="Удалить" red right onClick={() => this.props.deleteTurnoverWithDays(this.props.selectedRows, this.props.clearFilter)} />
                </ToolBar>
                <Table withColumnEdit showRow={this.showRow} mapRowValue={this.mapRowValue} mapColumntClass={this.mapColumntClass} />
            </div>
        } else {
            return <Loading />
        }
    }
}

const provider = provide(
    (state: ApplicationState) => state.grid,
    { ...GridStore.actionCreators, ...DialogStore.actionCreators, ...store.actionCreators }
).withExternalProps<RouteComponentProps<any>>()

type Props = typeof provider.allProps;

export default provider.connect(Goep);