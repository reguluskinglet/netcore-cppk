import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from '../../store/TasksStore'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, TextareaField, DropdownField } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'
import Tabs from '../../UI/tab/Tabs'
import { Redirect } from 'react-router-dom'
import { formatIsoToNorm } from '../../common/helper'
import { get, find } from 'lodash'

const algoritms = ['ТО-1', 'ТО-2', 'Приемка']
export const faultTypes = [{ label: 'Санитарная', value: 1 }, { label: 'Задачи по ремонту', value: 2 }]
export const faultTypeLabels = ['Техническая']
const labelWidth = '75px'

export const brigadeTypes = [
  { label: 'Локомотивная бригада', value: 0 },
  { label: 'Бригада депо', value: 1 },
  { label: 'Приемщики', value: 2 }
]
export const brigadeTypesLabels = ['Локомотивная бригада', 'Бригада депо', 'Приемщики']

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

const algoritmsList = [
  { label: 'ТО-1', value: 0 },
  { label: 'ТО-2', value: 1 },
  { label: 'Приемка поезда', value: 2 },
  { label: 'Сдача поезда', value: 3 },
  { label: 'Приёмка поезда ПР', value: 30 },
  { label: 'Выпуск поезда ПР', value: 31 },
  { label: 'Приёмка поезда ЛБ', value: 32 },
  { label: 'Сдача поезда ЛБ', value: 33 },
  { label: 'Инцидент Технический', value: 40 },
  { label: 'Инцидент СТО', value: 41 }
]

interface ITask {
  statusId: number
  taskTypeId: number
  trainName: string
  carriageNum: number
  modelName: string
  equipmentName: string
  faultName: string
  ownerName: string
  createDate: string
}

interface IFilter {
  TrainId: number
  CarriageId: number
  OwnerId: number
  BrigadeId: number
  StatusId: number
  InspectionId: number
  DateFrom: string
  DateTo: string
}

interface State {
  skip: number
  limit: number
  loading: boolean
  filter: IFilter
  redirect: string
  inspectionId: number
  selectedTab: number
}

class Tasks extends React.Component<Props, State> {
  defaultFilter(inspectionId = null): IFilter {
    return {
      TrainId: null,
      CarriageId: null,
      OwnerId: null,
      BrigadeId: null,
      StatusId: null,
      InspectionId: inspectionId,
      DateFrom: null,
      DateTo: null
    }
  }

  componentWillMount() {
    const a = this.props.match.url.split('/')
    let inspectionId = null
    if (a[2] === 'inspection' && a[3]) {
      inspectionId = parseInt(a[3])
    }
    this.setState({
      skip: 0,
      limit: 10,
      loading: false,
      filter: this.defaultFilter(inspectionId),
      redirect: null,
      inspectionId: inspectionId,
      selectedTab: 0
    })
    this.props.get(0, 10, this.defaultFilter(inspectionId))
    this.props.getLinks(inspectionId)
  }

  componentWillReceiveProps(nextProps) {
    const a = nextProps.match.url.split('/')
    let inspectionId = null
    if (a[2] === 'inspection' && a[3]) {
      inspectionId = parseInt(a[3])
    }
    if (inspectionId !== this.state.inspectionId) {
      this.setState({
        skip: 0,
        limit: 10,
        loading: false,
        filter: this.defaultFilter(inspectionId),
        redirect: null,
        inspectionId: inspectionId
      })
      this.props.get(0, 10, this.defaultFilter(inspectionId))
      this.props.getLinks(inspectionId)
    }
  }

  setPage = (skip, l) => {
    const { limit, filter } = this.state
    this.props.get(skip, (l > 0 && l) || limit, filter)
    this.setState({ skip })
  }

  onCellClick = row => {
    this.setState({
      ...this.state,
      redirect: this.state.inspectionId
        ? `/journals/inspection/${this.state.inspectionId}/tasks/${row.id}`
        : `/tasks/${row.id}/${row.TaskAttributeId}`
    })
  }

  getUserPerm = () => {
    const s = localStorage.getItem('user_info')
    const i = JSON.parse(s)
    return (i && i.permissions) || 0
  }

  render() {
    const { result: { data, total }, trains, users, brigades, vagons, inspection } = this.props
    const { skip, limit, filter, redirect, inspectionId } = this.state
    const perm = this.getUserPerm()
    if (redirect) return <Redirect to={redirect} push={true} />
    else
      return (
        <div
        //className="padding card"
        >
          {/* {inspectionId && (
            <div className="layout horizontal margin-bottom">
              <span
                style={{ fontSize: '16px', cursor: 'pointer', color: '#3D496B' }}
                onClick={() => {
                  this.setState({
                    ...this.state,
                    redirect: `/journals`
                  })
                }}
              >
                События
              </span>
              <span
                className="icon-chevron-right"
                style={{ fontSize: '16px', padding: '0 4px 1px 4px', cursor: 'pointer', color: '#3D496B' }}
              />
              <span style={{ fontSize: '16px', textDecoration: 'underline', cursor: 'pointer', color: '#3D496B' }}>
                {`${get(
                  find(algoritmsList, { value: inspection.TypeId }),
                  'label'
                )} №${inspectionId} от ${formatIsoToNorm(inspection.Date)} поезд ${inspection.TrainName}`}
              </span>
            </div>
          )} */}

          <div>
            <Tabs
              tabs={[{ label: 'ОПИСАНИЕ' }, { label: 'МЕТКИ' }]}
              selectedTab={this.state.selectedTab}
              onTabChanged={id => {
                this.setState({ selectedTab: id })
              }}
              className=""
            />

            {this.state.selectedTab === 0 && (
              <div className="layout vertical">
                <div className="layout horizontal margin">
                  <Table className="flex">
                    <div>
                      <Row>
                        <Cell style={{ maxWidth: '100px', fontWeight: '600' }}>{'Тип'}</Cell>
                        <Cell>{get(find(algoritmsList, { value: inspection.TypeId }), 'label')}</Cell>
                      </Row>
                    </div>
                    <div>
                      <Row>
                        <Cell style={{ maxWidth: '100px', fontWeight: '600' }}>{'Состав'}</Cell>
                        <Cell>{inspection.TrainName}</Cell>
                      </Row>
                    </div>
                    <div>
                      <Row>
                        <Cell style={{ maxWidth: '100px', fontWeight: '600' }}>{'Номер'}</Cell>
                        <Cell>{inspection.InspectionId}</Cell>
                      </Row>
                    </div>
                    <div>
                      <Row>
                        <Cell style={{ maxWidth: '100px', fontWeight: '600' }}>{'Статус'}</Cell>
                        <Cell>
                          {statuses.find(sa => sa.value === inspection.StatusId) &&
                            statuses.find(sa => sa.value === inspection.StatusId).label}
                        </Cell>
                      </Row>
                    </div>
                    <div>
                      <Row>
                        <Cell style={{ maxWidth: '100px', fontWeight: '600' }}>{'Исполнитель'}</Cell>
                        <Cell>{inspection.UserName}</Cell>
                      </Row>
                    </div>
                    <div>
                      <Row>
                        <Cell style={{ maxWidth: '100px', fontWeight: '600' }}>{'Начато'}</Cell>
                        <Cell>{formatIsoToNorm(inspection.Date)}</Cell>
                      </Row>
                    </div>
                    <div>
                      <Row>
                        <Cell style={{ maxWidth: '100px', fontWeight: '600' }}>{'Закончено'}</Cell>
                        <Cell>{formatIsoToNorm(get(inspection, 'Tabs.Description.DataEnd'))}</Cell>
                      </Row>
                    </div>
                  </Table>

                  <div className="flex layout vertical margin-left">
                    <Table className="">
                      <div>
                        <Row>
                          <Cell style={{ maxWidth: '140px', fontWeight: '600' }}>{'Задач'}</Cell>
                          <Cell>{get(inspection, 'Tabs.Description.TasksCount')}</Cell>
                        </Row>
                      </div>
                      <div>
                        <Row>
                          <Cell style={{ maxWidth: '140px', fontWeight: '600' }}>{'Меток всего'}</Cell>
                          <Cell>{get(inspection, 'Tabs.Description.AllLabelCount')}</Cell>
                        </Row>
                      </div>
                      <div>
                        <Row>
                          <Cell style={{ maxWidth: '140px', fontWeight: '600' }}>{'Км за смену'}</Cell>
                          <Cell>{get(inspection, 'Tabs.Description.KmPerShift')}</Cell>
                        </Row>
                      </div>
                      <div>
                        <Row>
                          <Cell style={{ maxWidth: '140px', fontWeight: '600' }}>{'Км общий'}</Cell>
                          <Cell>{get(inspection, 'Tabs.Description.KmTotal')}</Cell>
                        </Row>
                      </div>
                      <div>
                        <Row>
                          <Cell style={{ maxWidth: '140px', fontWeight: '600' }}>{'Квт часы'}</Cell>
                          <Cell>{get(inspection, 'Tabs.Description.KwHours')}</Cell>
                        </Row>
                      </div>
                      <div>
                        <Row>
                          <Cell style={{ maxWidth: '140px', fontWeight: '600' }}>{'Тормозные башмаки'}</Cell>
                          <Cell>{get(inspection, 'Tabs.BrakeShoesSerial')}</Cell>
                        </Row>
                      </div>
                    </Table>
                  </div>

                  <div className="flex" />
                </div>
              </div>
            )}

            {this.state.selectedTab === 1 && (
              <div className="layout vertical">
                <div className="layout horizontal margin">
                  {inspection.Tabs &&
                    inspection.Tabs.Labels.length > 0 && (
                      <Table className="flex-2">
                        <Row>
                          <HeaderCell>Вагон</HeaderCell>
                          <HeaderCell>Оборудование</HeaderCell>
                          <HeaderCell>Время</HeaderCell>
                          <HeaderCell>ИД метки</HeaderCell>
                        </Row>
                        {inspection.Tabs &&
                          inspection.Tabs.Labels &&
                          inspection.Tabs.Labels.map((label, indexL) => (
                            <div>
                              <Row key={'sd' + indexL}>
                                <Cell>{label.CarriageName}</Cell>
                                <Cell>{label.EquipmentName}</Cell>
                                <Cell>{formatIsoToNorm(label.TimeStamp)}</Cell>
                                <Cell>{label.LabelSerial}</Cell>
                              </Row>
                            </div>
                          ))}
                      </Table>
                    )}
                  <div className="flex-2" />
                </div>
              </div>
            )}

            {this.state.selectedTab === 2 && (
              <div className="layout vertical">
                <div className="layout horizontal margin">
                  {inspection.Tabs &&
                    inspection.Tabs.Temps.length > 0 && (
                      <Table className="flex-1">
                        <Row>
                          <HeaderCell>Температура</HeaderCell>
                          <HeaderCell>Время</HeaderCell>
                        </Row>
                        {inspection.Tabs &&
                          inspection.Tabs.Temps &&
                          inspection.Tabs.Temps.map((temp, indexT) => (
                            <div>
                              <Row key={'sd' + indexT}>
                                <Cell>{temp.Value}</Cell>
                                <Cell>{formatIsoToNorm(temp.TimeStamp)}</Cell>
                              </Row>
                            </div>
                          ))}
                      </Table>
                    )}
                  <div className="flex-4" />
                </div>
              </div>
            )}
          </div>

          {/* <div className="add-item card margin-top">
            <div className="layout vertical">
              {!inspectionId && (
                <div className="layout horizontal">
                  <div className="layout vertical flex">
                    <div className="layout horizontal">
                      <InputField
                        label="Дата с"
                        labelWidth={'40px'}
                        className="flex"
                        type={'date'}
                        value={filter.DateFrom}
                        onChange={event => {
                          const value = event.currentTarget.value
                          this.setState({ ...this.state, filter: { ...filter, DateFrom: value } })
                        }}
                      />
                      <InputField
                        label="по"
                        className="flex margin-left"
                        type={'date'}
                        value={filter.DateTo}
                        onChange={event => {
                          const value = event.currentTarget.value
                          this.setState({ ...this.state, filter: { ...filter, DateTo: value } })
                        }}
                      />
                    </div>
                    <InputField
                      label="Поезд"
                      labelWidth={'40px'}
                      className="margin-top"
                      value={filter.TrainId}
                      onChange={event => {
                        const value = event.currentTarget.value
                        this.setState({ ...this.state, filter: { ...filter, TrainId: value } })
                      }}
                      list={trains}
                      showNull
                    />
                  </div>

                  <div className="layout vertical flex margin-left">
                    <InputField
                      label="Инициатор"
                      labelWidth={labelWidth}
                      className=""
                      value={filter.OwnerId}
                      onChange={event => {
                        const value = event.currentTarget.value
                        this.setState({ ...this.state, filter: { ...filter, OwnerId: value } })
                      }}
                      list={users}
                      showNull
                    />
                    <InputField
                      label="Вагон"
                      labelWidth={labelWidth}
                      className="margin-top"
                      value={filter.CarriageId}
                      onChange={event => {
                        const value = event.currentTarget.value
                        this.setState({ ...this.state, filter: { ...filter, CarriageId: value } })
                      }}
                      list={vagons}
                      showNull
                    />
                  </div>

                  <div className="layout vertical flex margin-left">
                    <DropdownField
                      label="Статус"
                      labelWidth={labelWidth}
                      className=""
                      value={filter.StatusId}
                      onChange={event => {
                        const value = parseInt(event.currentTarget.value)
                        this.setState({ ...this.state, filter: { ...filter, StatusId: value } })
                      }}
                      list={statuses}
                      showNull
                    />
                    <DropdownField
                      label="Исполнитель"
                      labelWidth={labelWidth}
                      className="margin-top"
                      value={filter.BrigadeId}
                      onChange={event => {
                        const value = event.currentTarget.value
                        this.setState({ ...this.state, filter: { ...filter, BrigadeId: value } })
                      }}
                      list={brigadeTypes}
                      showNull
                    />
                  </div>
                </div>
              )}

              {inspectionId && (
                <div className="layout horizontal">
                  <DropdownField
                    label="Статус"
                    className="flex"
                    value={filter.StatusId}
                    onChange={event => {
                      const value = parseInt(event.currentTarget.value)
                      this.setState({ ...this.state, filter: { ...filter, StatusId: value } })
                    }}
                    list={statuses}
                    showNull
                  />
                  <DropdownField
                    label="Исполнитель"
                    className="flex margin-left"
                    value={filter.BrigadeId}
                    onChange={event => {
                      const value = event.currentTarget.value
                      this.setState({ ...this.state, filter: { ...filter, BrigadeId: value } })
                    }}
                    list={brigadeTypes}
                    showNull
                  />
                  <InputField
                    label="Вагон"
                    className="flex margin-left"
                    value={filter.CarriageId}
                    onChange={event => {
                      const value = event.currentTarget.value
                      this.setState({ ...this.state, filter: { ...filter, CarriageId: value } })
                    }}
                    list={vagons}
                    showNull
                  />
                </div>
              )}

              <div className="layout horizontal margin-top">
                <BlueButton
                  label="Применить фильтр"
                  className=""
                  onClick={() => {
                    const { limit, filter } = this.state
                    this.setState({ ...this.state, skip: 0 })
                    this.props.get(0, limit, filter)
                  }}
                />
                <BlueButton
                  label="Сбросить"
                  className="margin-left"
                  onClick={() => {
                    const { limit } = this.state
                    this.setState({ ...this.state, skip: 0, filter: this.defaultFilter(inspectionId) })
                    this.props.get(0, limit)
                  }}
                />
              </div>
            </div>
          </div>

          <div className="table-layout card  layout vertical margin-top">
            {(65536 & perm) === 65536 && (
              <div className="layout horizontal margin-bottom">
                <GreenButton
                  label="Добавить"
                  onClick={() => {
                    this.setState({ ...this.state, redirect: '/tasks/create/new' })
                  }}
                />
              </div>
            )}
            <Table>
              <HeaderRow>
                <HeaderCell>Номер</HeaderCell>
                <HeaderCell>Статус</HeaderCell>
                <HeaderCell>Тип</HeaderCell>
                <HeaderCell>Поезд</HeaderCell>
                <HeaderCell>Вагон</HeaderCell>
                <HeaderCell>Оборудование</HeaderCell>
                <HeaderCell>Типовая неисправность</HeaderCell>
                <HeaderCell>Инициатор</HeaderCell>
                <HeaderCell>Исполнитель</HeaderCell>
                <HeaderCell>Дата</HeaderCell>
              </HeaderRow>

              {data &&
                data.map((row, index) => (
                  <div key={`ct_${index}${row.id}`}>
                    <Row
                      className={row.expanded || row.showEdit ? 'expanded' : ''}
                      onClick={() => {
                        this.onCellClick(row)
                      }}
                    >
                      <Cell>{row.id}</Cell>
                      <Cell>
                        {statuses.find(sa => sa.value === row.statusId) &&
                          statuses.find(sa => sa.value === row.statusId).label}
                      </Cell>
                      <Cell>{faultTypeLabels[row.taskTypeId]}</Cell>
                      <Cell>{row.trainName}</Cell>
                      <Cell>{row.carriageName}</Cell>
                      <Cell>{row.equipmentName}</Cell>
                      <Cell>{row.faultName}</Cell>
                      <Cell>{row.ownerName}</Cell>
                      <Cell>{row.executor >= 0 && brigadeTypesLabels[row.executor]}</Cell>
                      <Cell>{row.createDate}</Cell>
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
          </div> */}
        </div>
      )
  }
}

const provider = provide((state: ApplicationState) => state.tasks, Store.actionCreators).withExternalProps<
  RouteComponentProps<any>
>()

type Props = typeof provider.allProps

export default provider.connect(Tasks)
