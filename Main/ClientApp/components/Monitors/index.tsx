import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from '../../store/MonitorsStore'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, TextareaField, DropdownField } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'
import Tabs from '../../UI/tab/Tabs'
import * as _ from 'lodash'

interface State {
  skip: number
  limit: number
  loading: boolean
  name: string
  type: number
  reload: boolean
  userId: number
  eqDesc: string
  editName: string
  editType: number
  editNum: string
  innerEditName: string
  innerEditDesc: string
  selectedTab: number
  showAdd: boolean
  filter1: string
  filter2: number
  fio: string
  dol: string
  num: string
}

class Monitors extends React.Component<Props, State> {
  componentWillMount() {
    this.setState({
      skip: 0,
      limit: 10,
      loading: false,
      name: null,
      type: null,
      userId: null,
      eqDesc: null,
      reload: false,
      editName: null,
      editType: null,
      editNum: null,
      innerEditName: null,
      innerEditDesc: null,
      selectedTab: 0,
      showAdd: false,
      filter1: null,
      filter2: null,
      fio: null,
      dol: null,
      num: null
    })
    this.props.getCategories(0, 10)
  }

  componentWillReceiveProps(nextProps) {
    const { skip, limit } = this.state
    nextProps.reload && this.props.getCategories(skip, limit)
  }

  setName = event => {
    const name = event.currentTarget.value
    this.setState({ name })
  }

  setType = event => {
    const typeS = event.currentTarget.value
    const type = parseInt(typeS)
    this.setState({ type })
  }

  setEqDesc = event => {
    const eqDesc = event.currentTarget.value
    this.setState({ eqDesc })
  }

  addPanel = (rowId, innerRowId) => {
    const { type } = this.state
    const data = {
      TvBoxId: rowId,
      Number: parseInt(innerRowId),
      type: type
    }
    this.props.addPanel(data)
    this.setState({
      type: null
    })
  }

  setPage = (skip, l) => {
    const { limit, filter1, filter2 } = this.state
    this.props.getCategories(skip, (l > 0 && l) || limit, filter1, filter2)
    this.setState({ skip })
  }

  onRowClick = row => () => {
    row.expanded ? this.props.unexpandRow(row.id) : this.props.expandRow(row.id)
  }
  onInnerRowClick = (row, innerRow) => () => {
    innerRow.expanded ? this.props.unexpandInner(row.id, innerRow.id) : this.props.expandInner(row.id, innerRow.id)
  }

  render() {
    const { result: { data, total }, screenTypes } = this.props
    const {
      name,
      type,
      userId,
      eqDesc,
      skip,
      limit,
      editName,
      editType,
      showAdd,
      filter1,
      filter2,
      fio,
      dol,
      num
    } = this.state

    return (
      <div>
        <div className="add-item card">
          <div className="layout vertical">
            <div className=" layout horizontal">
              <InputField
                label="Наименование"
                className="flex"
                value={filter1}
                onChange={event => {
                  this.setState({ ...this.state, filter1: event.currentTarget.value })
                }}
                onEnter={() => {
                  const { limit, filter1, filter2 } = this.state
                  this.setState({ ...this.state, skip: 0 })
                  this.props.getCategories(0, limit, filter1, filter2)
                }}
              />

              <BlueButton
                label="Применить фильтр"
                onClick={() => {
                  const { limit, filter1, filter2 } = this.state
                  this.setState({ ...this.state, skip: 0 })
                  this.props.getCategories(0, limit, filter1, filter2)
                }}
                className="margin-left"
              />
              <BlueButton
                label="Сбросить"
                onClick={() => {
                  const { limit } = this.state
                  this.setState({ ...this.state, skip: 0, filter1: null, filter2: null })
                  this.props.getCategories(0, limit)
                }}
                className="margin-left"
              />
            </div>
          </div>
        </div>

        <div className="table-layout card  layout vertical margin-top">
          <Table>
            <HeaderRow>
                <HeaderCell className="first header-cell" />
                <HeaderCell>Наименование</HeaderCell>
                <HeaderCell className="last header-cell" ></HeaderCell>
            </HeaderRow>

            {data &&
              data.map((row, index) => {
                return (
                  <div key={`ct_${index}${row.id}`}>
                    <Row className={row.expanded || row.showEdit ? 'expanded' : ''}>
                        <Cell className="first cell layout horizontal center-center" onClick={this.onRowClick(row)}>
                        {row.expanded ? (
                            <span className="icon-chevron-down" style={{ fontSize: '16px' }} />
                        ) : (
                            <span className="icon-chevron-right" style={{ fontSize: '16px' }} />
                        )}
                        </Cell>
                        <Cell>{row.name}</Cell>
                            <Cell className="last cell layout horizontal center-center">
                                {row.panels[0].monitors && row.panels[0].monitors.length > 0 && (
                                    <div
                                        className={(true && 'icon-delete') || 'icon-delete-1'}
                                        onClick={() => {
                                            true && this.props.delCategory(row.id)
                                        }}
                                    />
                                )}
                        </Cell>
                    </Row>

                    {row.expanded && (
                      <Table className="inner-table layout vertical">
                        {row.panels &&
                          _.map(row.panels, innerRow => {
                            return (
                              <div key={`eq_${row.id}${innerRow.id}`}>
                                <Row className={innerRow.expanded || innerRow.showEdit ? 'expanded' : ''}>
                                  <Cell
                                    className="first cell layout horizontal center-center"
                                    onClick={this.onInnerRowClick(row, innerRow)}
                                  >
                                    {innerRow.expanded ? (
                                      <span className="icon-chevron-down" style={{ fontSize: '16px' }} />
                                    ) : (
                                      <span className="icon-chevron-right" style={{ fontSize: '16px' }} />
                                    )}
                                  </Cell>
                                  <Cell>{`Экран${innerRow.id}`}</Cell>
                                  <Cell className="last cell layout horizontal center-center">
                                    <div
                                      className="icon-add"
                                      onClick={() => {
                                        this.setState({ ...this.state, userId: null, eqDesc: null, showAdd: false })
                                        row.expanded
                                          ? this.props.showAdd(row.id, innerRow.id)
                                          : this.props.expandRow(row.id, true)
                                      }}
                                    />
                                  </Cell>
                                </Row>

                                {innerRow.showAdd && (
                                  <div
                                    className="layout vertical"
                                    style={{
                                      borderLeft: '1px solid #666666',
                                      borderTop: '1px solid #666666',
                                      borderBottom: '1px solid #666666',
                                      borderRight: '1px solid #666666'
                                    }}
                                  >
                                    <div className="layout vertical">
                                      <div className="layout horizontal margin">
                                        <DropdownField
                                          label="Тип"
                                          onChange={this.setType}
                                          value={type}
                                          className=" margin-left"
                                          list={screenTypes}
                                          showNull
                                        />
                                        <span
                                          className="icon-save path1 path2 path3 margin-left"
                                          onClick={() => {
                                            this.addPanel(row.id, innerRow.id)
                                          }}
                                          style={{ fontSize: '30px' }}
                                        >
                                          <span className="path1" />
                                          <span className="path2" />
                                          <span className="path3" />
                                        </span>
                                      </div>
                                    </div>
                                  </div>
                                )}

                                {innerRow.expanded && (
                                  <Table className="inner-table layout vertical">
                                    {innerRow.monitors &&
                                      _.map(innerRow.monitors, monitor => {
                                        return (
                                          <div key={`eq_${row.id}${innerRow.id}${monitor.id}`}>
                                            <Row className={monitor.expanded || monitor.showEdit ? 'expanded' : ''}>
                                              <Cell>{monitor.type.name}</Cell>

                                              <Cell className="last cell layout horizontal center-center">
                                                <div
                                                  className={(true && 'icon-delete') || 'icon-delete-1'}
                                                  onClick={() => {
                                                    true && this.props.delPanel(row.id, innerRow.id, monitor.id)
                                                  }}
                                                />
                                              </Cell>
                                            </Row>
                                          </div>
                                        )
                                      })}
                                  </Table>
                                )}
                              </div>
                            )
                          })}
                      </Table>
                    )}
                  </div>
                )
              })}
          </Table>

          <Paginator
            skip={skip}
            limit={limit}
            total={total}
            setPage={this.setPage}
            onLimitChange={l => {
              this.setState({ ...this.state, limit: l })
              l > 0 && this.setPage(0, l)
            }}
          />
        </div>
      </div>
    )
  }
}

const provider = provide((state: ApplicationState) => state.monitors, Store.actionCreators)

type Props = typeof provider.allProps

export default provider.connect(Monitors)
