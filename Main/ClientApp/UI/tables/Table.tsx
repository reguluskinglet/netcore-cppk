import './theme.scss'
import * as React from 'react'
import Header from './Header'
import Row from './Row'
import PagerHelper from './Pager'

import { provide } from 'redux-typed';
import { ApplicationState } from '../../store';
import * as GridStore from '../../store/GridStore';

interface ExternalProps
{
    rowDataComponent?: React.ComponentClass<any>,
    rowDataParameter?: any
    selectedParent?: boolean
    withcellTextAdd?: string
    showCellTextAdd?: boolean
    addByParent?: boolean

    withSelectAllCheckBox?: boolean,
    withCheckBox?: boolean

    selectAll?: (checked, e?) => void
    selectRow?: (row, checked, e?) => void       

    mapRowClass?: (rowClass, row) => void
    mapColumntClass?: (row, index, value) => void

    invItem?: boolean
    showRow?: (row, isEdit) => void
    handleShowData?: (id, withNew, e?, row?) => void 
    isTree?: boolean
    mapRowValue?: (row, index, value) => void
    withColumnEdit?: boolean
}

class Table extends React.Component<Props, undefined> {

    pagerText = (pager: Pager) => {
        var data: number[] = pager.data;

        var single = data[0];
        var composition = data[1];
        var parent = data[2];
        var total = single + composition + parent;
        var invItems = total - parent;

        return 'Строк: ' + total + ' (Ед.: ' + single + '/ Корн.: ' + parent + '/ Сост.:' + composition + '/ ОУ: ' + invItems;
    }

    render() {

        var pagerText = this.props.invItem ? this.pagerText : null;

        return (
             this.props.dataSource.rows.length || this.props.isNewRow
                ? <div>
                    <table className="grid">
                        <thead>
                            <Header isTree={this.props.isTree} withColumnEdit={this.props.withColumnEdit} withSelectAllCheckBox={this.props.withSelectAllCheckBox} selectAll={this.props.selectAll}/>
                        </thead>
                        <Row
                            isTree={this.props.isTree}
                            mapRowValue={this.props.mapRowValue}
                            rowDataParameter={this.props.rowDataParameter}
                            withcellTextAdd={this.props.withcellTextAdd}
                            showCellTextAdd={this.props.showCellTextAdd}
                            rowDataComponent={this.props.rowDataComponent}
                            selectedParent={this.props.selectedParent}
                            addByParent={this.props.addByParent}
                            withCheckBox={this.props.withSelectAllCheckBox}
                            handleShowData={this.props.handleShowData}
                            mapRowClass={this.props.mapRowClass}
                            mapColumntClass={this.props.mapColumntClass}
                            showRow={this.props.showRow}
                            withColumnEdit={this.props.withColumnEdit}
                            selectRow={this.props.selectRow}
                        />
                    </table>
                    <PagerHelper pager={this.props.dataSource.pager}
                        onPageSelected={this.props.pageSelected}
                        pagerText={pagerText}
                    />
                </div>
                : <div className="rt-main-empty">нет данных</div>
        )
    }
}

const provider = provide(
    (state: ApplicationState) => state.grid,
    GridStore.actionCreators
).withExternalProps<ExternalProps>();

type Props = typeof provider.allProps;

export default provider.connect(Table);
