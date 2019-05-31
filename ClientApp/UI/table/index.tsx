import * as React from 'react'
import {
  Column,
  TreeDataState,
  CustomTreeData,
  SortingState,
  SelectionState,
  PagingState,
  CustomPaging,
  FilteringState,
  DataTypeProvider,
  IntegratedFiltering,
  EditingState
} from '@devexpress/dx-react-grid'
import {
  Grid,
  Table as DxTable,
  TableHeaderRow,
  VirtualTable,
  TableTreeColumn,
  TableSelection,
  PagingPanel,
  TableFilterRow,
  DragDropProvider,
  TableColumnReordering,
  TableColumnResizing,
  TableColumnVisibility,
  Toolbar,
  ColumnChooser,
  TableEditRow,
  TableEditColumn
} from '@devexpress/dx-react-grid-bootstrap3'
import { get, filter, map, last, find, keys } from 'lodash'
import moment from 'moment'
import DateEditor from './DateEditor'
import BooleanEditor from './BooleanEditor'
import { history as HISTORY } from '../../main'
import { hash } from '../../common'
import { Icon, DatePicker } from 'antd'
import 'moment/locale/ru'
import locale from 'antd/lib/date-picker/locale/ru_RU'

const getChildRows = (row, rootRows) => {
  const childRows = rootRows.filter(r => {
    return JSON.stringify(r.parentId) === (row ? JSON.stringify(row.id) : 'null')
  })
  if (childRows.length) {
    return childRows
  }
  return row && row.hasItems === 'True' ? [] : null
}

const DateFilterCell = ({ filter, onFilter, children, ...other }) => {
  return (
    <th style={{ fontWeight: 'normal' }}>
      <div className="input-group">
        {(children.length && children[0]) || ''}

        <DatePicker
          className="form-control"
          placeholder=""
          showTime
          format={'DD.MM.YYYY HH:mm:ss'}
          onChange={value => {
            const iso = (value && value.toISOString()) || null
            onFilter({ currentTarget: { value: iso }, target: { value: iso } })
          }}
          value={filter ? moment(filter.value) : undefined}
          locale={locale}
        />
      </div>
    </th>
  )
}

const BoolFilterCell = ({ filter, onFilter, children, ...other }) => {
  return (
    <th style={{ fontWeight: 'normal' }}>
      <input
        type="checkbox"
        onChange={e => {
          onFilter(e)
        }}
        checked={filter ? filter.value : undefined}
      />
    </th>
  )
}

const FilterCell = props => {
  const { column } = props
  if (column.type === 'link') {
    return <th />
  }
  if (column.type === 'date') {
    return <DateFilterCell {...props} />
  }
  if (column.type === 'bool') {
    return <BoolFilterCell {...props} />
  }
  return <TableFilterRow.Cell {...props} />
}

export interface Col {
  name: string
  title: string
  type: string
  disableEdit?: boolean
  getCellValue?: (row: any) => string
  getLink?: (row: any, col: Col) => string
}

export interface Data {
  columns: Col[]
  rows: any[]
  total: number
}

export interface Props {
  data: Data
  add?: boolean
  edit?: boolean
  del?: boolean
  selectable?: boolean
  tree?: boolean
  commitChanges?: (
    changes: { added: any[]; changed: any[]; deleted: any[] },
    state: {
      currentPage: number
      pageSize: number
      sortings: any[]
      filters: any[]
    }
  ) => void
  load?: (params: { parentId: any }) => void
  reload?: (
    state: {
      currentPage: number
      pageSize: number
      sortings: any[]
      filters: any[]
    }
  ) => void
  widthOffset?: number
  editors?: any[]
  editing?: boolean
  addComponent?: any
  toolbar?: () => JSX.Element
  onSelect?: (rows: any[] | undefined) => void
}

export default class Table extends React.Component<Props, any> {
  localization = {
    TableFilterRow: {
      filterPlaceholder: '',
      contains: 'Содержит',
      notContains: 'Не содержит',
      startsWith: 'Начинается с',
      endsWith: 'Кончается на',
      equal: 'Равно',
      notEqual: 'Не равно',
      greaterThan: 'Больше',
      greaterThanOrEqual: 'Больше или равно',
      lessThan: 'Меньше',
      lessThanOrEqual: 'Меньше или равно'
    },
    TableEditColumn: {
      addCommand: 'Создать',
      editCommand: 'Изменить',
      deleteCommand: 'Удалить',
      commitCommand: 'Сохранить',
      cancelCommand: 'Отмена'
    },
    VirtualTable: {
      noData: 'Нет данных'
    }
  }

  constructor(props) {
    super(props)

    this.state = {
      tableColumnExtensions: [],
      expandedRowIds: this.getSettings('expandedRowIds') || [],
      loading: false,
      sortings: this.getSettings('sortings') || [],
      currentPage: 0,//this.getSettings('currentPage') || 0,
      pageSize: 10,//this.getSettings('pageSize') || 10,
      pageSizes: [
        5,
        10,
        50,
        100
        // , 0
      ],
      selection: this.getSettings('selection') || [],
      filters: this.getSettings('filters') || [],
      dateColumns: [],
      dateFilterOperations: ['equal', 'notEqual', 'greaterThan', 'greaterThanOrEqual', 'lessThan', 'lessThanOrEqual'],

      numberColumns: [],
      numberFilterOperations: ['equal', 'notEqual', 'greaterThan', 'greaterThanOrEqual', 'lessThan', 'lessThanOrEqual'],

      boolColumns: [],
      boolFilterOperations: ['equal', 'notEqual'],

      stringColumns: [],
      stringFilterOperations: ['contains', 'notContains'],

      columnOrder: this.getSettings('columnOrder') || [],
      columnWidths: this.getSettings('columnWidths') || [],
      hiddenColumnNames: this.getSettings('hiddenColumnNames') || [],
      editingRowIds: [],
      addedRows: [],
      rowChanges: {},
      editingStateColumnExtensions: [],
      showFilter: this.getSettings('showFilter') || false
    }
  }

  getSettings(name, props?) {
    const h = hash(JSON.stringify(get(props || this.props, 'data.columns')))
    const s = JSON.parse(localStorage.getItem(h.toString()) || '{}')
    return s[name]
  }

  saveSettings(name, value, props?) {
    const h = hash(JSON.stringify(get(props || this.props, 'data.columns')))
    const s = JSON.parse(localStorage.getItem(h.toString()) || '{}')
    s[name] = value
    localStorage.setItem(h.toString(), JSON.stringify(s))
  }

  prepareColumns(props: Props) {
    const columns: any = map(get(props, 'data.columns'), (col: any) => {
      let getCellValue
      switch (col.type) {
        case 'date':
          getCellValue = row =>
            (row[col.name] &&
              moment(row[col.name])
                .utcOffset(0)
                .format('DD.MM.YYYY HH:mm')) ||
            ''
          break

        case 'bool':
          getCellValue = row => {
            return (
              <input
                type="checkbox"
                checked={row[col.name] === 'False' ? false : row[col.name] === 'True' ? true : !!row[col.name]}
                disabled
              />
            )
          }
          break

        case 'link':
          getCellValue = row => {
            const link = get(col, 'getLink', () => {})(row, col)
            const onClick = get(col, 'onClick', undefined)
            return (
              <a
                onClick={() => {
                  if (onClick) {
                    onClick(row, col)
                  } else {
                    HISTORY.push(link)
                  }
                }}
              >
                {col.text || 'Перейти'}
              </a>
            )
          }
          break
      }

      col.getCellValue = col.getCellValue || getCellValue
      return col
    })

    const tableColumnExtensions = map(filter(columns, { type: 'number' }), (col: any) => ({
      columnName: col.name,
      align: 'right'
    }))

    const dateColumns = map(filter(columns, { type: 'date' }), (col: any) => col.name)
    const numberColumns = map(filter(columns, { type: 'number' }), (col: any) => col.name)
    const boolColumns = map(filter(columns, { type: 'bool' }), (col: any) => col.name)
    const stringColumns = map(filter(columns, col => !col.type || col.type === 'string'), (col: any) => col.name)

    const columnOrder = !this.getSettings('columnOrder', props)
      ? map(columns, (col: any) => col.name)
      : this.getSettings('columnOrder', props)

    const columnWidth = columns.length
      ? (window.innerWidth - get(this.props, 'widthOffset', 0) - 120) / columns.length
      : 120

    const columnWidths = !this.getSettings('columnWidths', props)
      ? map(columns, (col: any) => ({
          columnName: col.name,
          width: col.type === 'link' ? 80 : columnWidth
        }))
      : this.getSettings('columnWidths', props)

    const editingStateColumnExtensions = map(columns, (col: any) => ({
      columnName: col.name,
      editingEnabled: !col.disableEdit
    }))

    const sortings = this.getSettings('sortings', props) || this.state.sortings
    const expandedRowIds = this.getSettings('expandedRowIds', props) || this.state.expandedRowIds
    // const currentPage = this.getSettings('currentPage', props) || this.state.currentPage
     const pageSize = this.getSettings('pageSize', props) || this.state.pageSize
    // const selection = this.getSettings('selection', props) || this.state.selection

    // const selected = map(selection, idx => get(this, `props.data.rows[${idx}]`) )
    // this.props.onSelect && this.props.onSelect(selected)

    const filters = this.getSettings('filters', props) || this.state.filters
    const hiddenColumnNames = this.getSettings('hiddenColumnNames', props) || this.state.hiddenColumnNames
    const showFilter = this.getSettings('showFilter', props) || this.state.showFilter

    this.setState({
      columns,
      tableColumnExtensions,
      columnOrder,
      columnWidths,
      editingStateColumnExtensions,
      sortings,
      expandedRowIds,
      // currentPage,
       pageSize,
      // selection,
      filters,
      hiddenColumnNames,
      showFilter,
      dateColumns,
      numberColumns,
      boolColumns,
      stringColumns
    })
  }

  componentDidMount() {
    // debugger
    if (get(this.props, 'data.columns.length')) {
      this.prepareColumns(this.props)
    } else {
      const { currentPage, pageSize, sortings, filters } = this.state
      this.props.reload && this.props.reload({ currentPage, pageSize, sortings, filters })
    }
  }

  componentWillReceiveProps(props) {
    if (get(props, 'data.columns') !== get(this.props, 'data.columns')) {
      this.prepareColumns(props)
    }
  }

  render() {
    const {
      columns = [],
      tableColumnExtensions,
      expandedRowIds,
      loading,
      sortings,
      pageSize,
      pageSizes,
      currentPage,
      selection,
      dateColumns,
      dateFilterOperations,
      columnOrder,
      columnWidths,
      hiddenColumnNames,
      editingRowIds,
      addedRows,
      rowChanges,
      editingStateColumnExtensions,
      filters,
      showFilter,
      numberColumns,
      numberFilterOperations,
      boolColumns,
      boolFilterOperations,
      stringColumns,
      stringFilterOperations
    } = this.state

    const { data, add = false, edit = false, del = false, selectable, addComponent = undefined } = this.props
    const { rows = [], total = 0 } = data || {}

    const treeColumn = get(columns, ['0', 'name']) || 'col0'

    const editing = add || edit || del

    return (
      <Grid rows={rows} columns={columns}>
        <DragDropProvider />
        <DataTypeProvider for={dateColumns} availableFilterOperations={dateFilterOperations} />
        <DataTypeProvider for={numberColumns} availableFilterOperations={numberFilterOperations} />
        <DataTypeProvider for={boolColumns} availableFilterOperations={boolFilterOperations} />
        <DataTypeProvider for={stringColumns} availableFilterOperations={stringFilterOperations} />
        <FilteringState filters={filters} onFiltersChange={this.changeFilters} />

        {selectable && <SelectionState selection={selection} onSelectionChange={this.changeSelection} />}

        <PagingState
          currentPage={currentPage}
          onCurrentPageChange={this.changeCurrentPage}
          pageSize={pageSize}
          onPageSizeChange={this.changePageSize}
        />
        <CustomPaging totalCount={total || rows.length || 0} />
        <SortingState sorting={sortings} onSortingChange={this.changeSorting} />
        {this.props.tree && (
          <TreeDataState expandedRowIds={expandedRowIds} onExpandedRowIdsChange={this.changeExpandedRowIds} />
        )}
        {this.props.tree && <CustomTreeData getChildRows={getChildRows} />}

        {editing && (
          <EditingState
            // editingRowIds={editingRowIds}
            // onEditingRowIdsChange={this.changeEditingRowIds}
            // rowChanges={rowChanges}
            // onRowChangesChange={this.changeRowChanges}
            // addedRows={addedRows}
            // onAddedRowsChange={this.changeAddedRows}
            onCommitChanges={this.commitChanges}
            columnExtensions={editingStateColumnExtensions}
          />
        )}

        <VirtualTable columnExtensions={tableColumnExtensions} messages={this.localization.VirtualTable} />
        <TableColumnResizing columnWidths={columnWidths} onColumnWidthsChange={this.changeColumnWidths} />
        <TableColumnReordering order={columnOrder} onOrderChange={this.changeColumnOrder} />
        <TableHeaderRow showSortingControls />

        {editing && (
          <TableEditRow
            cellComponent={cellProps => {
              const extEditor = get(
                find(get(this.props, 'editors') as any, {
                  columnName: cellProps.column.name
                }),
                'editor'
              )
              return typeof extEditor === 'function' ? (
                extEditor(cellProps)
              ) : cellProps.column['type'] === 'date' ? (
                <DateEditor {...cellProps} />
              ) : cellProps.column['type'] === 'bool' ? (
                <BooleanEditor {...cellProps} />
              ) : cellProps.column['type'] === 'link' ? (
                <td {...{ colspan: '1' }} style={{ verticalAlign: 'middle', padding: '1px' }} />
              ) : (
                <TableEditRow.Cell {...cellProps} />
              )
            }}
          />
        )}
        {editing && (
          <TableEditColumn
            headerCellComponent={addComponent || TableEditColumn.HeaderCell}
            showAddCommand={!addedRows.length && add}
            showEditCommand={edit}
            showDeleteCommand={del}
            messages={this.localization.TableEditColumn}
            width={170}
          />
        )}

        <TableColumnVisibility
          hiddenColumnNames={hiddenColumnNames}
          onHiddenColumnNamesChange={this.hiddenColumnNamesChange}
        />
        {showFilter && (
          <TableFilterRow showFilterSelector messages={this.localization.TableFilterRow} cellComponent={FilterCell} />
        )}
        {this.props.tree && <TableTreeColumn for={treeColumn} showSelectionControls={selectable} />}
        {selectable && (
          <TableSelection selectByRowClick highlightRow showSelectionColumn={this.props.tree ? false : true} />
        )}
        <Toolbar rootComponent={this.toolbar} />
        <ColumnChooser />
        <PagingPanel pageSizes={pageSizes} />
      </Grid>
    )
  }

  toolbar = props => {
    return (
      <Toolbar.Root>
        <div style={{ flex: 'auto' }}>{(this.props.toolbar && this.props.toolbar()) || ''}</div>
        <button type="button" className="btn btn-link">
          <Icon
            type="filter"
            onClick={() => {
              this.saveSettings('showFilter', !this.state.showFilter)
              this.setState({ showFilter: !this.state.showFilter })
            }}
          />
        </button>

        <button type="button" className="btn btn-link">
          <Icon
            type="close-circle"
            onClick={() => {
              this.saveSettings('filters', [])
              this.setState({ filters: [] })
            }}
          />
        </button>

        {props.children}
      </Toolbar.Root>
    )
  }

  commitChanges = (params: any) => {
    let { rows } = get(this.props, 'data')
    if (params.added) {
      rows = [
        ...params.added.map(row => ({
          ...row
        }))
      ]
    }
    if (params.changed) {
      rows = rows.map((row, index) => (params.changed[index] ? { ...row, ...params.changed[index] } : row))
    }
    if (params.deleted) {
      const deletedSet = new Set(params.deleted)
      rows = rows.filter((row, index) => deletedSet.has(index))
    }

    const added = map(keys(get(params, 'added')), key => get(rows, [parseInt(key)]))
    const changed = map(keys(get(params, 'changed')), key => get(rows, [parseInt(key)]))
    const deleted = map(keys(get(params, 'deleted')), key => get(rows, [parseInt(key)]))

    const { currentPage, pageSize, filters, sortings } = this.state

    this.props.commitChanges &&
      this.props.commitChanges({ added, changed, deleted }, { currentPage, pageSize, sortings, filters })
  }

  changeAddedRows = addedRows => {
    // debugger
    const initialized = addedRows.map(row => (Object.keys(row).length ? row : {}))
    this.setState({ addedRows: initialized })
  }

  changeEditingRowIds = editingRowIds => {
    this.setState({ editingRowIds })
  }

  changeRowChanges = rowChanges => {
    // debugger
    this.setState({ rowChanges })
  }

  changeExpandedRowIds = expandedRowIds => {
    this.saveSettings('expandedRowIds', expandedRowIds)
    this.setState({ expandedRowIds })
    if (last(expandedRowIds) >= 0) {
      const childLoaded = get(this.props, ['data', 'rows', last(expandedRowIds).toString(), 'childLoaded'])
      if (!childLoaded) {
        const parentId = get(this.props, ['data', 'rows', last(expandedRowIds).toString(), 'id'])
        this.props.load && this.props.load({ parentId })
      }
    }
  }

  changeSorting = sortings => {
    this.saveSettings('sortings', sortings)
    this.setState({ sortings })
    const { currentPage, pageSize, filters } = this.state
    this.props.reload && this.props.reload({ currentPage, pageSize, sortings, filters })
  }

  changeCurrentPage = (currentPage: number) => {
    this.saveSettings('currentPage', currentPage)
    this.setState({ currentPage })
    const { pageSize, sortings, filters } = this.state
    this.props.reload && this.props.reload({ currentPage, pageSize, sortings, filters })
  }

  changePageSize = (pageSize: number) => {
    this.saveSettings('pageSize', pageSize)
    this.setState({ pageSize })
    const { sortings, filters } = this.state
    this.props.reload && this.props.reload({ currentPage: 0, pageSize, sortings, filters })
  }

  changeFilters = filters => {
    this.saveSettings('filters', filters)
    this.setState({ filters })
    const { currentPage, pageSize, sortings } = this.state
    this.props.reload && this.props.reload({ currentPage, pageSize, sortings, filters })
  }

  changeSelection = (selection: any) => {
    const selected = map(selection, idx => get(this, `props.data.rows[${idx}]`) )
    this.props.onSelect && this.props.onSelect(selected)
    this.saveSettings('selection', selection)
    this.setState({ selection })
  }

  changeColumnOrder = columnOrder => {
    this.saveSettings('columnOrder', columnOrder)
    this.setState({ columnOrder })
  }

  changeColumnWidths = columnWidths => {
    this.saveSettings('columnWidths', columnWidths)
    this.setState({ columnWidths })
  }

  hiddenColumnNamesChange = hiddenColumnNames => {
    this.saveSettings('hiddenColumnNames', hiddenColumnNames)
    this.setState({ hiddenColumnNames })
  }
}
