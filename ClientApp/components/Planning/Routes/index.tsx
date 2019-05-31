import './index.scss'
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


class Route extends React.Component<Props, undefined> {

    componentWillUnmount() {
        this.props.unloadState();
    }

    componentWillMount() {
        this.props.getGrid(GridType.route)
    }

    mapRowValue = (row, index, value: string) => {

        if (index == 3) {

            if (!value)
                return <DayOfWeek selectedDays={null} />

            var result = value.split(',').map(x => parseInt(x, 10));
            return <DayOfWeek selectedDays={result} />
        }

        return value;
    }

    mapColumntClass = (row, index, value: string) => {

        if (index == 3)
            return 'days'

        return '';
    }

    showRow = (row, isEdit) => {
        this.props.getTripById(row.id, this.props.clearFilter)
    }

    render() {
        if (this.props.isLoaded) {
            return <div>
                <ToolBar>
                    <ToolBarButton label="Добавить" onClick={() => this.props.getTripById(null, this.props.clearFilter)} />
                    <ToolBarButton red right disabled={this.props.selectedRows.length === 0} label="Удалить" onClick={() => this.props.deleteTrip(this.props.selectedRows, this.props.clearFilter)} />
                </ToolBar>
                <Table showRow={this.showRow} mapRowValue={this.mapRowValue} mapColumntClass={this.mapColumntClass} />
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

export default provider.connect(Route);