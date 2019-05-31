import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from '../../store/ScheduleEventsStore'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, TextareaField, DropdownField } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'
import Tabs from '../../UI/tab/Tabs'

export const days = ['Воскресение', 'Понедельник', 'Вторник', 'Среда', 'Четверг', 'Пятница', 'Суббота']

const algoritms = ['ТО-1', 'ТО-2', 'Приемка']
const algoritmsList = [{ label: 'ТО-1', value: 0 }, { label: 'ТО-2', value: 1 }, { label: 'Приемка', value: 2 }]

interface State {
  skip: number
  limit: number
  loading: boolean
  reload: boolean
  filter1: number
}

class ScheduleEvents extends React.Component<Props, State> {
  componentWillMount() {
    this.setState({
      skip: 0,
      limit: 10,
      loading: false,
      reload: false,
      filter1: null
    })
    this.props.getCategories(0, 10)
    this.props.getLinks()
  }

  componentWillReceiveProps(nextProps) {
    const { skip, limit } = this.state
    nextProps.reload && this.props.getCategories(skip, limit)
  }

  setPage = (skip, l) => {
    const { limit, filter1 } = this.state
    this.props.getCategories(skip, (l > 0 && l) || limit, filter1)
    this.setState({ skip })
  }

  getTime = time => {
    const m = time.split(':')
    if (m.length === 3) {
      const h = parseInt(m[0])
      const min = parseInt(m[1])
      return min > 0 ? h + 1 : h > 12 ? 12 : h
    }
    return
  }

  render() {
    const { result: { data, total }, stations, models, routes } = this.props
    const { skip, limit, filter1 } = this.state
    return (
      <div>
        <div className="add-item card">
          <div className="layout vertical">
            <div className=" layout horizontal">
              <DropdownField
                label="Маршрут"
                className="flex"
                value={filter1}
                onChange={event => {
                  const value = parseInt(event.currentTarget.value)
                  this.setState({ ...this.state, filter1: value })
                }}
                onEnter={() => {
                  const { limit, filter1 } = this.state
                  this.setState({ ...this.state, skip: 0 })
                  this.props.getCategories(0, limit, filter1)
                }}
                style={{ minWidth: '250px' }}
                list={routes}
                showNull
              />

              <BlueButton
                label="Применить фильтр"
                onClick={() => {
                  const { limit, filter1 } = this.state
                  this.setState({ ...this.state, skip: 0 })
                  this.props.getCategories(0, limit, filter1)
                }}
                className="margin-left"
              />
              <BlueButton
                label="Сбросить"
                onClick={() => {
                  const { limit } = this.state
                  this.setState({ ...this.state, skip: 0, filter1: null })
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
              <HeaderCell>Маршрут</HeaderCell>
              <HeaderCell>Рейс</HeaderCell>
              <HeaderCell>День недели</HeaderCell>
            </HeaderRow>

            {data &&
              data.map((row, index) => (
                <div key={`ct_${index}${row.id}`}>
                  <Row className={row.expanded || row.showEdit ? 'expanded' : ''}>
                    <Cell
                      className="first cell layout horizontal center-center"
                      onClick={() => {
                        if (!row.expanded) {
                          this.props.expandRow(row.id)
                        } else {
                          this.props.unexpandRow(row.id)
                        }
                      }}
                    >
                      {(row.expanded && <span className="icon-chevron-down" style={{ fontSize: '16px' }} />) || (
                        <span className="icon-chevron-right" style={{ fontSize: '16px' }} />
                      )}
                    </Cell>
                    <Cell>{row.routeName}</Cell>
                    <Cell>{row.tripName}</Cell>
                    <Cell>{days[row.day]}</Cell>
                  </Row>

                  {row.expanded && (
                    <Table className="inner-table layout vertical">
                      {row.stations &&
                        row.stations.map(innerRow => (
                          <div key={`eq_${index}${innerRow.id}`}>
                            <Row className={innerRow.expanded || innerRow.showEdit ? 'expanded' : ''}>
                              <Cell>{innerRow.name}</Cell>
                              <Cell>{innerRow.arrivalTime}</Cell>
                              <Cell>{innerRow.departmentTime}</Cell>
                              <Cell>
                                <div>{innerRow.downtime}</div>
                                <span className={`icon-t${this.getTime(innerRow.downtime)}`} />
                              </Cell>
                              <Cell>
                                <InputField
                                  type="radio"
                                  value={innerRow.checkList === 0}
                                  rightLabel
                                  label="ТО-1"
                                  onChange={event => {
                                    const value = event.currentTarget.checked
                                    value && this.props.setCheckList(row.id, innerRow.id, 0)
                                  }}
                                />
                                <InputField
                                  type="radio"
                                  value={innerRow.checkList === 1}
                                  rightLabel
                                  label="ТО-2"
                                  onChange={event => {
                                    const value = event.currentTarget.checked
                                    value && this.props.setCheckList(row.id, innerRow.id, 1)
                                  }}
                                />
                                {/* <InputField type="radio" value={innerRow.checkList === 2} rightLabel label="Приемка"/> */}
                                <InputField
                                  type="radio"
                                  value={innerRow.checkList === null}
                                  rightLabel
                                  label="Нет"
                                  onChange={event => {
                                    const value = event.currentTarget.checked
                                    value && this.props.setCheckList(row.id, innerRow.id, null)
                                  }}
                                />
                              </Cell>
                            </Row>
                          </div>
                        ))}
                    </Table>
                  )}
                </div>
              ))}
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

const provider = provide((state: ApplicationState) => ({ ...state.scheduleEvents }), Store.actionCreators)

type Props = typeof provider.allProps

export default provider.connect(ScheduleEvents)
