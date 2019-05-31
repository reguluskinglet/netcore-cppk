import * as React from 'react'
import * as classnames from 'classnames';

import { provide } from 'redux-typed';
import { ApplicationState } from '../../store';
import * as GridStore from '../../store/GridStore';
import * as remove from 'lodash/remove'
import * as without from 'lodash/without'
import Checkbox from '../checkbox/Checkbox'

interface RowItem {
    id: number
    item: any[]
    child?: RowItem[]
    disabled: boolean
}

interface RowState
{
    showRow?: number
    newRow?: boolean
}

interface ExternalProps {
    rowDataComponent?: React.ComponentClass<any>
    selectedParent: boolean
    isTree: boolean
    withcellTextAdd?: string
    showCellTextAdd?: boolean
    rowDataParameter?: any
    addByParent: boolean
    withCheckBox: boolean
    withColumnEdit: boolean

    handleShowData?: (id, withNew, e?, row?) => void
    mapRowClass: (rowClass, row) => void
    mapColumntClass?: (row, index, value) => void
    selectRow: (row, checked, e?) => void
    showRow: (row, isEdit) => void  
    mapRowValue: (row, index, value) => void    
}

class Row extends React.Component<Props, undefined> {

    render() {
        const { selectedRows, isSelectedAll, selectRow, expandRows, onExpandRow, showRowData, showRow, mapRowClass, mapColumntClass, mapRowValue, isTree, withCheckBox, withColumnEdit } = this.props;

        return <tbody>
            {this.props.dataSource.rows.map((row, i) => <RowItem
                key={`p_${row.id}_${i}`}
                row={row}
                mapRowValue={mapRowValue}
                selectedRows={selectedRows}
                isSelectedAll={isSelectedAll}
                expandRows={expandRows}
                onExpandRow={onExpandRow}
                mapColumntClass={mapColumntClass}
                showRowData={showRow ? showRow : showRowData}
                parentId={null}
                mapRowClass={mapRowClass}
                isTree={isTree}
                withColumnEdit={withColumnEdit}
                withCheckBox={withCheckBox}
                selectRow={selectRow}
            />)}
           
        </tbody>
    }
}

const provider = provide(
    (state: ApplicationState) => state.grid,
    GridStore.actionCreators
).withExternalProps<ExternalProps>();

type Props = typeof provider.allProps;

export default provider.connect(Row);

const RowItem = ({ row, selectedRows, isSelectedAll, expandRows, onExpandRow, showRowData, mapRowClass, mapColumntClass, mapRowValue, isTree, withColumnEdit, withCheckBox, selectRow, parentId }) => {
    var rowIndex = selectedRows.findIndex(r => r.rowId == row.id)

    var checked = rowIndex >= 0 ? selectedRows[rowIndex].isSelected : isSelectedAll

    var isExpand = expandRows.indexOf(row.id) >= 0
    // var isExpand = isExpandAll ? !expandRow : expandRow;

    var showData = row.id == '9999998'//this.props.showRowId;

    var disabled = row.disabled;

    if (row.child)
        disabled = row.child.every(x => x.disabled)

    //var action = this.props.handleShowData ? this.props.handleShowData : this.props.showRowData;
    var levels = [],
        clevel = 0,
        level = parentId ? 1 : 0; //TODO должны получать с сервера Level
    //  var clevel = maxLevel + 1 - row.level;

    if (isTree)
    {
        var clevel = 2 - level;

        for (var i = 0; i < level; i++) {
            levels.push(<td className="fix" />)
        }
    }


    var parent = row.childCount > 0;

    var rowClass = {
        'parent': parent,
        'child': !parent,
        'show-data': showData,
        'disabled': disabled,
        'checked': checked
    }


    if (mapRowClass)
        mapRowClass(rowClass, row);

    var className = classnames('tr-item', rowClass)

    var classNameCollaps = classnames('fix', {
        'parent-item': parent,
        'open': isExpand
    })

    return (
        <>
            <tr className={className} onClick={(e) => {

                if ($(e.target).closest('td').hasClass('fix'))
                    return;

                showRowData(row)
            }}>
                {levels}
                {isTree && <td className={classNameCollaps} onClick={() => onExpandRow(expandRows, row.id)} />}
                {withColumnEdit && <td className="fix edit" onClick={() => showRowData(row, true)} />}
                {withCheckBox ?
                    <td className="fix row-check" >
                        <Checkbox checked={disabled || checked} onChange={(checked, event) => selectRow(row, checked, event)} disabled={disabled} />
                    </td> : null
                }
                {row.item.map((value, index) => {

                    var classColumn = mapColumntClass ? mapColumntClass(row, index, value) : '';

                    if (index == 0)
                        return <td key={'data_' + index} colSpan={clevel} className={'cell-span ' + classColumn}><span>{value}</span></td>

                    return <td className={classColumn} key={'data_' + index}>{mapRowValue ? mapRowValue(row, index, value):value}</td>
                })}
            </tr>
            {row.child && isExpand ? <>{row.child.map((rowChild, index) => <RowItem
                isTree={isTree}
                mapRowValue={mapRowValue}
                key={`ch_${rowChild.id}`}
                row={rowChild}
                selectedRows={selectedRows}
                isSelectedAll={isSelectedAll}
                expandRows={expandRows}
                onExpandRow={onExpandRow}
                showRowData={showRowData}
                mapRowClass={mapRowClass}
                mapColumntClass={mapColumntClass}
                parentId={row.id}
                withColumnEdit={withColumnEdit}
                withCheckBox={withCheckBox}
                selectRow={selectRow}
            />)}</> : null}
        </>
    );
}