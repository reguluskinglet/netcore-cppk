import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from '../../store/ReportsStore'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, TextareaField, DropdownField } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'
import Tabs from '../../UI/tab/Tabs'
import { Redirect } from 'react-router-dom'
import { formatIsoToNorm } from '../../common/helper'
import { get as getV } from 'lodash'

const algoritms = ['ТО-1', 'ТО-2', 'Приемка']
export const faultTypes = [{ label: 'Санитарная', value: 1 }, { label: 'Задачи по ремонту', value: 2 }]
export const faultTypeLabels = ['Техническая']
const statuses = [
  { label: 'Новая', value: 0 },
  { label: 'В журнале', value: 1 },
  { label: 'В работе', value: 2 },
  { label: 'Просмотрено', value: 3 },
  { label: 'Принято к исполнению', value: 4 },
  { label: 'Не подтверждено', value: 5 },
  { label: 'Выполнено', value: 6 },
  { label: 'К проверке', value: 7 },
  { label: 'Не прошла проверку', value: 8 },
  { label: 'К подтверждению', value: 9 },
  { label: 'Закрыто', value: 99 }
]

interface State {
  skip: number
  limit: number
  loading: boolean
  report: number
}

class Reports extends React.Component<Props, State> {
  componentWillMount() {
    this.setState({
      skip: 0,
      limit: 10,
      loading: false,
      report: null
    })
    this.props.getReports()
  }

  componentWillReceiveProps(nextProps) {}

  setPage = (skip, l) => {
    const { limit, report } = this.state
    this.props.getReport(report, skip, (l > 0 && l) || limit)
    this.setState({ skip })
  }

  render() {
    const { result: { columns, rows, total }, reports } = this.props
    const { skip, limit, report } = this.state
    return (
      <div className="margin">
        <div className="add-item card margin-top">
          <div className="layout vertical">
            <div className="layout horizontal">
              <DropdownField
                label="Выбирете отчет"
                className=""
                value={report}
                onChange={event => {
                  const value = parseInt(event.currentTarget.value)
                  this.setState({ ...this.state, report: value })
                }}
                list={reports}
                showNull
              />
              <BlueButton
                label="Применить фильтр"
                className="margin-left"
                onClick={() => {
                  const { limit } = this.state
                  this.setState({ ...this.state, skip: 0 })
                  this.props.getReport(report, 0, limit)
                }}
              />
            </div>
          </div>
        </div>

        <div className="table-layout card  layout vertical margin-top">
          <Table>
            <HeaderRow>
              {columns && columns.map((col, colIndex) => <HeaderCell key={`col_${colIndex}`}>{col.name}</HeaderCell>)}
            </HeaderRow>

            {rows &&
              rows.map((row, index) => (
                <div key={`row_${index}${row.id}`}>
                  <Row>
                    {columns &&
                      columns.map((col, colIndex) => (
                        <Cell key={`cell_${colIndex}_${index}`}>{this.getValue(col, colIndex, row, index)}</Cell>
                      ))}
                  </Row>
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

  getValue = (c, ci, r, ri) => {
    if (!r.values) return ''
    if (ci === 1 && c.type === 'enum') return statuses[parseInt(r.values[ci])] && statuses[parseInt(r.values[ci])].label
    if (ci === 2 && c.type === 'enum') return faultTypeLabels[parseInt(r.values[ci])]
    return r.values[ci]
  }
}

const provider = provide((state: ApplicationState) => state.reports, Store.actionCreators).withExternalProps<
  RouteComponentProps<any>
>()

type Props = typeof provider.allProps

export default provider.connect(Reports)
