import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from './store'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, TextareaField, DropdownField } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'
import { Redirect } from 'react-router-dom'
import moment from 'moment'

const tagTypes = [{ value: 1, label: 'Бумажные метки' }, { value: 2, label: 'Корпусные метки' }]
const tagTypesLabels = ['Бумажные метки', 'Корпусные метки']

class Tags extends React.Component<Props, any> {
  componentWillMount() {
    this.setState({
      skip: 0,
      limit: 10,
      loading: false,
      reload: false,
      filter1: null,
      filter2: null
    })
    this.props.get(0, 10)
    this.props.getLinks(undefined)
  }

  componentWillReceiveProps(nextProps) {
    const { skip, limit } = this.state
    nextProps.reload && this.props.get(skip, limit)
  }

  add = () => {}

  setPage = (skip, l) => {
    const { limit, filter1, filter2 } = this.state
    this.props.get(skip, (l > 0 && l) || limit, filter1, filter2)
    this.setState({ skip })
  }

  onCellClick = row => {
    this.setState({ ...this.state, redirect: row.id })
  }

  render() {
    const { result = { data: [], total: 0 }, id } = this.props
    const { data, total } = result
    const { skip, limit, filter1, filter2, redirect } = this.state
    // debugger
    if (redirect) return <Redirect to={'/tags/eq/' + redirect} push={true} />
    else
      return (
        <div>
          <div className="add-item card">
            <div className="layout vertical">
              <div className="layout horizontal">
                <div className="layout horizontal">
                  <InputField
                    label="Дата с"
                    className="flex"
                    type={'date'}
                    value={filter1}
                    onChange={event => {
                      const value = event.currentTarget.value
                      this.setState({ ...this.state, filter1: value })
                    }}
                  />
                  <InputField
                    label="по"
                    className="flex margin-left"
                    type={'date'}
                    value={filter2}
                    onChange={event => {
                      const value = event.currentTarget.value
                      this.setState({ ...this.state, filter2: value })
                    }}
                  />
                </div>

                <BlueButton
                  label="Применить фильтр"
                  className="margin-left"
                  onClick={() => {
                    const { limit, filter1, filter2 } = this.state
                    this.setState({ ...this.state, skip: 0 })
                    this.props.get(0, limit, filter1, filter2)
                  }}
                />
                <BlueButton
                  label="Сбросить"
                  className="margin-left"
                  onClick={() => {
                    const { limit } = this.state
                    this.setState({
                      ...this.state,
                      skip: 0,
                      filter1: null,
                      filter2: null
                    })
                    this.props.get(0, limit)
                  }}
                />
              </div>
            </div>
          </div>

          <div className="table-layout card  layout vertical margin-top">
            <div className="layout horizontal  margin-bottom">
              <GreenButton
                label="Добавить"
                onClick={() => {
                  this.props.add().then(result => {
                    this.setState({ ...this.state, redirect: result.id })
                  })
                }}
              />
            </div>

            <Table>
              <HeaderRow>
                <HeaderCell>Номер документа</HeaderCell>
                <HeaderCell>Наименование</HeaderCell>
                <HeaderCell>Создал</HeaderCell>
                <HeaderCell>Тип меток</HeaderCell>
                <HeaderCell>Шаблон</HeaderCell>
                <HeaderCell>Кол-во ОУ</HeaderCell>
                <HeaderCell>Создано</HeaderCell>
                <HeaderCell className="last header-cell" />
              </HeaderRow>

              {data &&
                data.map((row, index) => (
                  <div key={`ct_${index}${row.id}`}>
                    <Row className={row.expanded || row.showEdit ? 'expanded' : ''}>
                      <Cell
                        onClick={() => {
                          this.onCellClick(row)
                        }}
                      >
                        {row.id}
                      </Cell>
                      <Cell
                        onClick={() => {
                          this.onCellClick(row)
                        }}
                      >
                        {row.name}
                      </Cell>
                      <Cell
                        onClick={() => {
                          this.onCellClick(row)
                        }}
                      >
                        {row.userName}
                      </Cell>
                      <Cell
                        onClick={() => {
                          this.onCellClick(row)
                        }}
                      >
                        {(row.labelType > 0 && tagTypesLabels[row.labelType - 1]) || ''}
                      </Cell>
                      <Cell
                        onClick={() => {
                          this.onCellClick(row)
                        }}
                      >
                        {row.templateLabelsName}
                      </Cell>
                      <Cell
                        onClick={() => {
                          this.onCellClick(row)
                        }}
                      >
                        {row.taskPrintItemsCount}
                      </Cell>
                      <Cell
                        onClick={() => {
                          this.onCellClick(row)
                        }}
                      >
                        {row.createDate}
                      </Cell>
                      <Cell className="last cell layout horizontal center-center">
                        <div
                          className={(!row.taskPrintItemsCount && 'icon-delete') || 'icon-delete-1'}
                          onClick={() => {
                            !row.taskPrintItemsCount && this.props.del(row.id)
                          }}
                        />
                      </Cell>
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
}

const provider = provide((state: ApplicationState) => state.tags, Store.actionCreators).withExternalProps<
  RouteComponentProps<any>
>()

type Props = typeof provider.allProps

export default provider.connect(Tags)
