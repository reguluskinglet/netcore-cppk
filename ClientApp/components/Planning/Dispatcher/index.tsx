import * as React from 'react'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../../store';
import * as GridStore from '../../../store/GridStore';
import * as DialogStore from '../../../store/DialogStore';
import { GridType, journalTitleFilter, dispatcherTitleFilter } from '../../../common'
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
        this.props.getGrid(GridType.dispatcher);
    }


    showRow = (row, isEdit) => {
        this.props.addDispatcher(row.id, this.props.clearFilter);
    };

    get getSelectedRows(): any[] {
        const { selectedRows, isSelectedAll } = this.props
        return selectedRows &&
            selectedRows
            .filter(x => isSelectedAll == false ? x.isSelected == true : x.isSelected == false)
                .map(x => x.rowId)
    }

    get isInDepo(): number {
        return this.props.filter.isInDepo;
    }

    get deleteOptions(): any {
        return {
            ids: this.getSelectedRows,
            isSelectedAll: this.props.isSelectedAll,
            isInDepo: this.isInDepo
        }
    }

    render() {
        if (this.props.isLoaded) {
            return <div>

                       <ToolBar>
                           <ToolBarButton label="Добавить" onClick={() => this.props.addDispatcher(null,
                        this.props.clearFilter)} />
                    <ToolBarButton disabled={this.getSelectedRows.length === 0 && !this.props.isSelectedAll} label="Удалить" red right onClick={
                        () => this.props.deleteDispatcher(this.deleteOptions, this.props.clearFilter)}/>
                       </ToolBar>
                       <div className="filter-right">
                           <Filter filterTitle={dispatcherTitleFilter}/>
                       </div>
                       <Table showRow={this.showRow} withSelectAllCheckBox/>
                   </div>;
        } else {
            return <Loading />;
        }
    }
}

const provider = provide(
    (state: ApplicationState) => state.grid,
    { ...GridStore.actionCreators, ...DialogStore.actionCreators, ...store.actionCreators }
).withExternalProps<RouteComponentProps<any>>();

type Props = typeof provider.allProps;

export default provider.connect(Goep);