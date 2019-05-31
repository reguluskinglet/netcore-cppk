import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from '../../store/TaskCreateStore'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, TextareaField, DropdownField, CommentField, File } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'
import Tabs from '../../UI/tab/Tabs'
import { Redirect } from 'react-router-dom'
import { formatIsoToNorm, formatDateToNorm } from '../../common/helper'
import { get, find } from 'lodash'
import { taskLevelTypes } from './Task'

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

export const modelTypes = ['головной', 'моторный', 'прицепной']

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

interface State {
  loading: boolean
  id: number
  status: number
  comment: string
  br: any
  redirectPath: string
  inspectionId: number
  brigade: number
  vagon: number
  train: number
  equipment: number
  fail: number
  taskType: number
}

class TaskCreate extends React.Component<Props, any> {
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
      train: null,
      vagon: null,
      equipment: null,
      fail: null
    })
    this.props.getLinks(this.props.match.params.eventId)
    // this.props.get(parseInt(this.props.match.params.id))
  }

  componentWillReceiveProps(nextProps) {
    if (nextProps.data && nextProps.data.possibletaskStatuses) {
      statuses = nextProps.data.possibletaskStatuses.map(s => statusesAll.find(sa => s === sa.value))
      nextProps.data.statusId && statuses.push(statusesAll.find(sa => nextProps.data.statusId === sa.value))
      this.setState({ ...this.state, status: nextProps.data.statusId, brigade: nextProps.data.brigadeType })
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
    const { files } = this.props
    const { status, comment, id, brigade, train, equipment, pdza, vagon, fail, taskType, taskLevel } = this.state
    if (train && vagon && status >= 0 && brigade >= 0) {
      const data = {
        taskType: taskType || 0,
        train,
        vagon,
        equipment: pdza,
        status,
        comment,
        brigade,
        taskLevel,
        files: files.map(f => f.id),
        fail
      }

      this.props.addComment(data)
      this.setState({
        train: null,
        vagon: null,
        equipment: null,
        pdza: null,
        status: null,
        comment: null,
        brigade: null,
        taskLevel: null,
        fail: null
      })
    }
  }

  redirect = path => {
    this.setState({
      ...this.state,
      redirectPath: path
    })
  }

  getUserName = () => {
    const s = localStorage.getItem('user_info')
    const i = JSON.parse(s)
    return (i && i.name) || 'Без имени'
  }

  render() {
    const {
      data,
      files,
      inspection,
      brigades,
      trains,
      vagons,
      equipments,
      redirect,
      fails,
      executors,
      statuses
    } = this.props
    const {
      status,
      comment,
      id,
      br,
      redirectPath,
      inspectionId,
      brigade,
      train,
      vagon,
      equipment,
      fail,
      taskType
    } = this.state

    const ispolnitel = []
    executors &&
      executors.forEach(ex => {
        const br = brigadeTypes.find(b => b.value === ex)
        ispolnitel.push(br)
      })

    const statusi = []
    statuses &&
      statuses.forEach(ex => {
        const st = statusesAll.find(s => s.value === ex)
        statusi.push(st)
      })

    if (redirectPath || redirect) return <Redirect to={redirectPath || redirect} push={true} />
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
                      : `${algoritms[inspection.TypeId]} №${inspectionId} от ${formatIsoToNorm(
                          inspection.Date
                        )} поезд ${inspection.TrainName}`}
                  </span>
                  <span
                    className="icon-chevron-right"
                    style={{ fontSize: '16px', padding: '0 4px 1px 4px', cursor: 'pointer', color: '#3D496B' }}
                  />
                </div>
              ))}
            <span style={{ fontSize: '16px', textDecoration: 'underline', cursor: 'pointer', color: '#3D496B' }}>
              Создание задачи
            </span>
          </div>

          <div className="card padding layout horizontal">
            <div className="flex-1 layout vertical" style={{ minHeight: '261px' }}>
              <Table className="">
                <div>
                  <Row>
                    <Cell className="cell120px cell flex layout horizontal center">{'Дата1'}</Cell>
                    <Cell>{formatDateToNorm(new Date(Date.now()))}</Cell>
                  </Row>
                </div>
                <div>
                  <Row>
                    <Cell className="cell120px cell flex layout horizontal center">{'Инициатор'}</Cell>
                    <Cell>{this.getUserName()}</Cell>
                  </Row>
                </div>
                <div>
                  <Row>
                    <Cell className="cell120px cell flex layout horizontal center">{'Тип'}</Cell>
                    <Cell>
                      <select
                        className="flex no-border"
                        value={taskType}
                        onChange={event => {
                          const value = parseInt(event.currentTarget.value)
                          this.setState({ ...this.state, taskType: value })
                        }}
                      >
                        {types &&
                          types.map((t, index) => (
                            <option key={index} value={t.value}>
                              {t.label}
                            </option>
                          ))}
                      </select>
                    </Cell>
                  </Row>
                </div>
                <div>
                  <Row>
                    <Cell className="cell120px cell flex layout horizontal center">{'Поезд'}</Cell>
                    <Cell>
                      <select
                        className="flex no-border"
                        value={train}
                        onChange={event => {
                          const value = parseInt(event.currentTarget.value)
                          this.setState({ ...this.state, train: value, vagon: 0, equipment: 0, fail: 0 })
                          this.props.getVagons(value)
                        }}
                      >
                        <option key={'null'} value={0}>
                          {''}
                        </option>
                        {trains &&
                          trains.map((t, index) => (
                            <option key={index} value={t.id}>
                              {t.name}
                            </option>
                          ))}
                      </select>
                    </Cell>
                  </Row>
                </div>
                <div>
                  <Row>
                    <Cell className="cell120px cell flex layout horizontal center">{'Вагон'}</Cell>
                    <Cell>
                      <select
                        className="flex no-border"
                        value={vagon}
                        onChange={event => {
                          const value = parseInt(event.currentTarget.value)
                          this.setState({ ...this.state, vagon: value, equipment: 0, fail: 0 })
                          this.props.getEq(value)
                        }}
                      >
                        <option key={'null'} value={0}>
                          {''}
                        </option>
                        {vagons &&
                          vagons.map((t, index) => (
                            <option key={index} value={t.id}>
                              {/* {t.number} */}
                              {`${t.serial || ''} (вагон ${t.number || ''}, ${modelTypes[t.model.modelType] || ''})`}
                            </option>
                          ))}
                      </select>
                    </Cell>
                  </Row>
                </div>
                <div>
                  <Row>
                    <Cell className="cell120px cell flex layout horizontal center">{'Оборудование'}</Cell>
                    <Cell>
                      <select
                        className="flex no-border"
                        value={equipment}
                        onChange={event => {
                          const value = parseInt(event.currentTarget.value)
                          // console.log(find(equipments, { equipmentId: value }))
                          this.setState({
                            ...this.state,
                            equipment: value,
                            pdza: get(find(equipments, { equipmentId: value }), 'equipmentModelId'),
                            fail: 0
                          })
                          this.props.getFails(value)
                        }}
                      >
                        <option key={'null'} value={0}>
                          {''}
                        </option>
                        {equipments &&
                          equipments.map((t, index) => (
                            <option value={t.equipmentId}>
                              {t.parentId ? '----' + t.equipmentName : t.equipmentName}
                            </option>
                          ))}
                      </select>
                    </Cell>
                  </Row>
                </div>
                <div>
                  <Row>
                    <Cell className="cell120px cell flex layout horizontal center">{'Типовая неисправность'}</Cell>
                    <Cell>
                      <select
                        className="flex no-border"
                        value={fail}
                        onChange={event => {
                          const value = parseInt(event.currentTarget.value)
                          this.setState({ ...this.state, fail: value })
                        }}
                      >
                        <option key={'null'} value={0}>
                          {''}
                        </option>
                        {fails && fails.map((t, index) => <option value={t.id}>{t.name}</option>)}
                      </select>
                    </Cell>
                  </Row>
                </div>
              </Table>

              <DropdownField
                className="margin-top flex-none"
                label="Текущий исполнитель"
                labelWidth={'130px'}
                list={ispolnitel}
                value={brigade}
                onChange={event => {
                  const value = parseInt(event.currentTarget.value)
                  this.setState({ ...this.state, brigade: value, status: null })
                  this.props.getAvaibleStatuses(value)
                }}
                showNull
                style={{ width: '320px' }}
              />

              <DropdownField
                className="margin-top flex-none"
                label="Текущий статус"
                labelWidth={'130px'}
                list={statusi}
                value={status}
                onChange={event => {
                  const value = parseInt(event.currentTarget.value)
                  this.setState({ ...this.state, status: value })
                }}
                showNull
                style={{ width: '320px' }}
              />

              <DropdownField
                className="margin-top flex-none"
                label="Уровень критичности"
                labelWidth={'130px'}
                list={taskLevelTypes}
                // value={status}
                onChange={event => this.setState({ taskLevel: parseInt(event.currentTarget.value) })}
                showNull
                style={{ width: '320px' }}
              />
            </div>

            <div className="flex-2 layout vertical margin-left" style={{ minHeight: '261px' }}>
              <CommentField
                className=""
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
                marginTop={'0'}
              />

              <div className="layout vertical end margin-top">
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

const provider = provide((state: ApplicationState) => state.taskCreate, Store.actionCreators).withExternalProps<
  RouteComponentProps<any>
>()

type Props = typeof provider.allProps

export default provider.connect(TaskCreate)
