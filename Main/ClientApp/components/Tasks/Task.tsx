import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from '../../store/TaskStore'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, TextareaField, DropdownField, CommentField, File } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'
import Tabs from '../../UI/tab/Tabs'
import { Redirect } from 'react-router-dom'
import { formatIsoToNorm } from '../../common/helper'
import { get, find } from 'lodash'
import moment from 'moment'

const folders = ['project/docs/', 'project/images/', 'project/sounds/']

const algoritms = ['ТО-1', 'ТО-2', 'Приемка']
const types = [{ label: 'Техническая', value: 0 }, { label: 'Санитарная', value: 1 }, { label: 'Сервисная', value: 2 }]
const typeLabels = ['Техническая', 'Санитарная', 'Сервисная']

export const faultTypes = [{ label: 'Санитарная', value: 1 }, { label: 'Задачи по ремонту', value: 2 }]
export const faultTypeLabels = ['Техническая']

export const brigadeTypes = [
  { label: 'Локомотивная бригада', value: 0 },
  { label: 'Бригада депо', value: 1 },
  { label: 'Приемщики', value: 2 }
]
export const brigadeTypesLabels = ['Локомотивная бригада', 'Бригада депо', 'Приемщики']

const statusesAll = [
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

let statuses = []

export const modelTypes = ['головной', 'моторный', 'прицепной']

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

export const taskLevelTypes = [
  { label: 'Низкий', value: 1 },
  { label: 'Нормальный', value: 2 },
  { label: 'Высокий', value: 3 }
]

interface State {
  loading: boolean
  id: number
  status: number
  comment: string
  br: any
  redirectPath: string
  inspectionId: number
  brigade: number
  taskLevel: number
}

class Task extends React.Component<Props, State> {
  componentWillMount() {
    const a = this.props.match.url.split('/')
    const b = []
    if (a[1] && a[1] === 'journals') {
      b.push('Журналы')
      if (a[2] && a[2] === 'inspection' && this.props.match.params.eventId) {
        b.push(this.props.match.params.eventId)
      }
    }
    if (a[1] && a[1] === 'tasks') {
      b.push('Задачи')
    }
    this.setState({
      loading: false,
      id: parseInt(this.props.match.params.id),
      status: null,
      comment: null,
      br: b,
      redirectPath: null,
      inspectionId: this.props.match.params.eventId,
      brigade: null,
      taskLevel: null
    })
    this.props.getLinks(this.props.match.params.eventId)
      this.props.get(1, 1507)
  }

  componentWillReceiveProps(nextProps) {
    if (nextProps.data && nextProps.data.possibletaskStatuses) {
      statuses = nextProps.data.possibletaskStatuses.map(s => statusesAll.find(sa => s === sa.value))

      if (nextProps.data.statusId) {
        const cs = nextProps.data.possibletaskStatuses.find(s => nextProps.data.statusId == s)
        !cs && statuses.push(statusesAll.find(sa => nextProps.data.statusId === sa.value))
      }
      this.setState({
        ...this.state,
        status: nextProps.data.statusId,
        brigade: nextProps.data.brigadeType,
        taskLevel: nextProps.data.taskLevel
      })
    } else {
      statuses = []
    }
  }

  addFile = () => {
    const input = document.createElement('input')
    input.setAttribute('type', 'file')
    input.setAttribute('multiple', 'true')
    input.click()
    input.onchange = function(params) {
      if (params && params.currentTarget && params.currentTarget.files && params.currentTarget.files.length) {
        var formData = new FormData()
        Array.from(params.currentTarget.files).forEach((element, index) => {
          if (index > 8 || this.props.files.length > 8) return
          formData.append('files', params.currentTarget.files[index])
        })
        if (this.props.files.length < 8) this.props.upload(formData)
      }
    }.bind(this)
  }

  addComment = () => {
    const { data, files } = this.props
    const { status, comment, id, brigade, taskLevel } = this.state
    this.props.addComment({
      id: id,
      status: status === data.statusId ? undefined : status,
      comment: comment,
      files: files,
      brigade: brigade === data.brigadeType ? undefined : brigade,
      taskLevel: taskLevel === data.taskLevelType ? undefined : taskLevel,
      attrId: parseInt(this.props.match.params.attrId)
    })
    this.setState({
      ...this.state,
      status: null,
      comment: null,
      brigade: null,
      taskLevel: null
    })
  }
  //
  redirect = path => {
    this.setState({
      ...this.state,
      redirectPath: path
    })
  }

  getUserPerm = () => {
    const s = localStorage.getItem('user_info')
    const i = JSON.parse(s)
    return (i && i.permissions) || 0
  }

  render() {
    const { data, files, inspection, brigades } = this.props
    const { status, comment, id, br, redirectPath, inspectionId, brigade, taskLevel } = this.state
    const perm = this.getUserPerm()
    if (redirectPath) return <Redirect to={redirectPath} push={true} />
    else
      return (
        <div
          className="flex layout vertical"
          style={{
            height: 'calc(100vh - 180px)',
            padding: '18px'
          }}
        >
          <div className="layout horizontal end margin-bottom">
            {br &&
              br.map(b => (
                <div className="layout horizontal">
                  <span
                    style={{ fontSize: '16px', cursor: 'pointer', color: '#3D496B' }}
                    onClick={() => {
                      if (b === 'Журналы' || b === 'Задачи') this.redirect('/journals')
                      else
                        // else if (b === 'Задачи') this.redirect('/tasks')
                        this.redirect('/journals/inspection/' + inspectionId)
                    }}
                  >
                    {b === 'Журналы' || b === 'Задачи'
                      ? 'События'
                      : `${get(
                          find(algoritmsList, { value: inspection.TypeId }),
                          'label'
                        )} №${inspectionId} от ${formatIsoToNorm(inspection.Date)} поезд ${inspection.TrainName}`}
                  </span>
                  <span
                    className="icon-chevron-right"
                    style={{ fontSize: '16px', padding: '0 4px 1px 4px', cursor: 'pointer', color: '#3D496B' }}
                  />
                </div>
              ))}
            <span style={{ fontSize: '16px', textDecoration: 'underline', cursor: 'pointer', color: '#3D496B' }}>
              Задача №{id}
            </span>
          </div>

          <div className="flex layout horizontal">
            <div className="card padding flex layout vertical">
              <Table className="grey-back flex-none">
                <div>
                  <Row>
                    <Cell className="cell120px cell flex layout horizontal center">{'Дата'}</Cell>
                    <Cell>{moment(data.data).format('DD.MM.YYYY HH:mm')}</Cell>
                  </Row>
                </div>
                <div>
                  <Row>
                    <Cell className="cell120px cell flex layout horizontal center">{'Инициатор'}</Cell>
                    <Cell>{data.initiatorName}</Cell>
                  </Row>
                </div>
                <div>
                  <Row>
                    <Cell className="cell120px cell flex layout horizontal center">{'Тип'}</Cell>
                    <Cell>{data.taskType >= 0 && typeLabels[data.taskType]}</Cell>
                  </Row>
                </div>
                <div>
                  <Row>
                    <Cell className="cell120px cell flex layout horizontal center">{'Поезд'}</Cell>
                    <Cell>{data.trainName}</Cell>
                  </Row>
                </div>
                <div>
                  <Row>
                    <Cell className="cell120px cell flex layout horizontal center">{'Вагон'}</Cell>
                    <Cell>{`${data.carriageSerial || ''} (вагон ${data.сarriageNumber || ''}, ${modelTypes[
                      data.carriageModelTypeId
                    ] || ''})`}</Cell>
                  </Row>
                </div>
                <div>
                  <Row>
                    <Cell className="cell120px cell flex layout horizontal center">{'Оборудование'}</Cell>
                    <Cell>{data.equipmentName}</Cell>
                  </Row>
                </div>
                <div>
                  <Row>
                    <Cell className="cell120px cell flex layout horizontal center">{'Описание'}</Cell>
                    <Cell>{data.description}</Cell>
                  </Row>
                </div>
                <div>
                  <Row>
                    <Cell className="cell120px cell flex layout horizontal center">{'Мероприятие'}</Cell>
                    <Cell
                      onClick={() => {
                        data.inspection &&
                          data.inspection.id &&
                          this.setState({ ...this.state, redirectPath: '/journals/inspection/' + data.inspection.id })
                      }}
                      style={{ cursor: 'pointer', textDecoration: 'underline' }}
                    >
                      {/* {data.eventName} */}
                      {data.inspection &&
                        data.inspection.id &&
                        data.inspection.dateStart &&
                        `${algoritms[data.inspection.checkListType]} №${data.inspection.id} от ${data.inspection
                          .dateStart} поезд ${data.trainName}`}
                    </Cell>
                  </Row>
                </div>
                <div>
                  <Row>
                    <Cell className="cell120px cell flex layout horizontal center">{'Типовая неисправность'}</Cell>
                    <Cell>{data.faultName}</Cell>
                  </Row>
                </div>
                <div>
                  <Row className="heightAuto">
                    <Cell>
                      {data.files &&
                        data.files.map((file, index) => (
                          <File
                            key={index}
                            fullName={file.name}
                            previewPath={'project/images/thumbs/' + file.thumbPath}
                            path={folders[file.documentType] + file.path}
                          />
                        ))}
                    </Cell>
                  </Row>
                </div>
                {data.value > 0 && (
                  <div>
                    <Row>
                      <Cell className="cell120px cell flex layout horizontal center">{'Результат проверки'}</Cell>
                      <Cell>{data.value}</Cell>
                    </Row>
                  </div>
                )}
              </Table>

              <div className="flex layout vertical">
                {/* {br &&
                  !(br.length >= 1 && br[0] === 'Журналы') && ( */}
                <DropdownField
                  className="margin-top flex-none"
                  label="Текущий статус"
                  labelWidth={'130px'}
                  list={statuses}
                  value={status}
                  onChange={event => {
                    const value = parseInt(event.currentTarget.value)
                    this.setState({ ...this.state, status: value })
                  }}
                  showNull
                  style={{ width: '320px' }}
                />
                {/* )} */}

                {/* {br &&
                  !(br.length >= 1 && br[0] === 'Журналы') && */}
                {(65536 & perm) === 65536 && (
                  <DropdownField
                    className="margin-top flex-none"
                    label="Текущий исполнитель"
                    labelWidth={'130px'}
                    list={brigadeTypes}
                    value={brigade}
                    onChange={event => {
                      const value = parseInt(event.currentTarget.value)
                      this.setState({ ...this.state, brigade: value })
                    }}
                    showNull
                    style={{ width: '320px' }}
                  />
                )}

                <DropdownField
                  className="margin-top flex-none"
                  label="Уровень критичности"
                  labelWidth={'130px'}
                  list={taskLevelTypes}
                  value={taskLevel}
                  onChange={event => this.setState({ taskLevel: parseInt(event.currentTarget.value) })}
                  showNull
                  disabled
                  style={{ width: '320px' }}
                />

                <CommentField
                  className="margin-top flex"
                  label="Текст сообщения"
                  text={comment}
                  textChange={event => {
                    const value = event.currentTarget.value
                    this.setState({ ...this.state, comment: value })
                  }}
                  files={files}
                  fileAdd={this.addFile}
                  fileDelete={file => {
                    this.props.delFile(file.id)
                  }}
                  maxFiles={8}
                  style={{
                    minHeight: '109px'
                  }}
                />

                <div className="flex-none layout vertical end margin-top">
                  <span
                    className="icon-save path1 path2 path3"
                    style={{ fontSize: '30px' }}
                    onClick={() => {
                      this.addComment()
                    }}
                  >
                    <span className="path1" />
                    <span className="path2" />
                    <span className="path3" />
                  </span>
                </div>
              </div>
            </div>

            <div className="card margin-left padding flex layout vertical">
              <div className="layout horizontal center-center" style={{ height: '32px', marginBottom: '8px' }}>
                ИСТОРИЯ ИЗМЕНЕНИЯ СТАТУСОВ, ИСПОЛНИТЕЛЕЙ И КОММЕНТАРИЕВ
              </div>
              <div
                className="layout vertical"
                style={{
                  overflow: 'auto'
                }}
              >
                {data.history &&
                  data.history.map(h => (
                    <div className="blue-card margin-top">
                      <div>{`${h.date} | ${h.user} (${h.userBrigadeType})`}</div>
                      <div style={{ paddingTop: '4px' }}>
                        <b>{`${this.getRusType(h.type)}: `}</b>
                        {this.getRusType(h.type) === 'Сообщение' && h.text}

                        {this.getRusType(h.type) === 'Смена статуса' &&
                          `"${(statusesAll.find(sa => sa.value === h.oldStatus) &&
                            statusesAll.find(sa => sa.value === h.oldStatus).label) ||
                            ''}" > "${(statusesAll.find(sa => sa.value === h.newStatus) &&
                            statusesAll.find(sa => sa.value === h.newStatus).label) ||
                            ''}"`}

                        {this.getRusType(h.type) === 'Смена исполнителя' &&
                          `${brigadeTypesLabels[h.oldExecutorBrigadeType] || ''} > ${brigadeTypesLabels[
                            h.newExecutorBrigadeType
                          ] || ''}`}
                      </div>
                      <div className="layout horizontal wrap">
                        {this.getRusType(h.type) === 'Сообщение' &&
                          h.files &&
                          h.files.map((file, index) => (
                            <File
                              key={index}
                              fullName={file.name}
                              previewPath={'project/images/thumbs/' + file.name}
                              path={folders[file.documentType] + file.name}
                            />
                          ))}
                      </div>
                    </div>
                  ))}
              </div>
            </div>
          </div>
        </div>
      )
  }

  getRusType = t => {
    switch (t) {
      case 'Comment':
        return 'Сообщение'
      case 'Status':
        return 'Смена статуса'
      case 'Executor':
        return 'Смена исполнителя'
      default:
        break
    }
  }
}

const provider = provide((state: ApplicationState) => state.task, Store.actionCreators).withExternalProps<
  RouteComponentProps<any>
>()

type Props = typeof provider.allProps

export default provider.connect(Task)
