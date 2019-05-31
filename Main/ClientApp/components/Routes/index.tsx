import * as React from 'react'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from './store'
import Table from '../../UI/table'
import { map, get, find, concat } from 'lodash'
import { Select, Modal, Button, Row, Col, TimePicker } from 'antd'
const Option = Select.Option
import moment from 'moment'
import { DropdownField, InputField } from '../../UI/fields'
import { GreenButton } from '../../UI/buttons'
import { HeaderRow, HeaderCell, Roww, Table as OldTable, Cell, Row as OldRow } from '../../UI/grid'
import Label from '../../UI/fields/Label'

class CLS extends React.Component<Props, any> {
  constructor(props) {
    super(props)
    this.state = {}
  }

  componentDidMount() {
    // this.props.getLinks()
    this.props.getStation('')
  }

  render() {
    const { data } = this.props
    const { columns = [], rows = [], total = 0 } = data
    const {} = this.state
    columns.length &&
      columns[0].type !== 'link' &&
      columns.splice(0, 0, {
        name: '',
        title: '',
        type: 'link',
        onClick: (row, col) => {
          const stations = []
          get(row, ['additionalProperty', 'stantionsWithTime']).forEach(s => {
            stations.push({
              station: {
                id: s.stantionId,
                label: s.stantionName
              },
              startTime: moment(s.inTime),
              endTime: moment(s.outTime)
            })
          })

          this.setState({
            modal2Visible: true,
            stations,
            newTrip: false,
            row,
            newName: row.col0
          })
        }
      })
    return (
      <div>
        <Table
          data={{ columns, rows, total }}
          widthOffset={320}
          reload={({ currentPage, pageSize, sortings, filters }) => {
            this.setState({ currentPage, pageSize, sortings, filters })
            this.props.getList({
              paging: { skip: currentPage * pageSize, limit: pageSize },
              sortings,
              filters
            })
          }}
          editing
          add
          addComponent={() => (
            <th
              {...{
                colspan: '1',
                style: {
                  position: 'relative',
                  userSelect: 'none',
                  cursor: 'pointer'
                }
              }}
            >
              <a
                onClick={() => {
                  this.setState({
                    modal2Visible: true,
                    newTrip: true,
                    newName: '',
                    stations: [],
                    d1: false,
                    d2: false,
                    d3: false,
                    d4: false,
                    d5: false,
                    d6: false,
                    d7: false
                  })
                }}
              >
                Создать
              </a>
            </th>
          )}
          del
          commitChanges={(changes, { currentPage, pageSize, sortings, filters }) => {
            Promise.all(
              concat(
                map(get(changes, 'changed'), row => {
                  return this.props.add({
                    TripWithDays: {
                      Name: row.col0,
                      Description: null,
                      Id: row.id.id
                    }
                  })
                }),
                map(get(changes, 'deleted'), row => {
                  return this.props.del(row.id.id)
                })
              )
            ).then(res => {
              this.props.getList({
                paging: { skip: currentPage * pageSize, limit: pageSize },
                sortings,
                filters
              })
            })
          }}
        />

        {this.renderModal2()}
      </div>
    )
  }

  private renderModal2() {
    // console.log('d', this.state.new_station)

    return (
      <Modal
        title={`${this.state.newTrip ? 'Добавить' : 'Изменить'} рейс`}
        wrapClassName="vertical-center-modal"
        visible={this.state.modal2Visible}
        closable={false}
        width={640}
        bodyStyle={{ overflow: 'auto', maxHeight: '70vh' }}
        footer={[
          <Button
            key="cancel"
            onClick={e => {
              e.stopPropagation()
              this.props.clearTrip()
              this.setState({ modal2Visible: false, newTrip: false })
            }}
          >
            Отмена
          </Button>,
          <Button
            key="submit"
            type="primary"
            disabled={
              !(
                get(this, 'state.stations.length') > 1 &&
                this.state.newName &&
                (this.state.d1 ||
                  this.state.d2 ||
                  this.state.d3 ||
                  this.state.d4 ||
                  this.state.d5 ||
                  this.state.d6 ||
                  this.state.d7)
              )
            }
            onClick={() => {
              const { d1, d2, d3, d4, d5, d6, d7 } = this.state
              const data = {
                tripWithDateTimeStations: {
                  stantionOnTripsWithStringTime: this.state.stations.map(s => ({
                    stantionId: s.station.value,
                    inTime: moment(s.startTime).format('YYYY-MM-DDTHH:mm:ss'),
                    outTime: moment(s.endTime).format('YYYY-MM-DDTHH:mm:ss')
                  })),
                  name: this.state.newName,
                  id: this.state.trip || undefined
                },
                // routeId: this.state.selectedRow.routeId,
                Days: [d7, d1, d2, d3, d4, d5, d6].map((e, i) => (e === true ? i : undefined)).filter(e => e >= 0)
              }

              if (this.state.newTrip) {
                this.props.addNewTrip(data).then(() => {
                  this.setState({ modal2Visible: false, newTrip: false })
                  const { currentPage, pageSize, sortings, filters } = this.state
                  this.props.getList({
                    paging: { skip: currentPage * pageSize, limit: pageSize },
                    sortings,
                    filters
                  })
                })
              } else {
                this.props
                  .add({
                    TripWithDays: {
                      Name: this.state.newName,
                      Description: null,
                      Id: this.state.row.id.id
                    }
                  })
                  .then(() => {
                    const { currentPage, pageSize, sortings, filters } = this.state
                    this.props.getList({
                      paging: { skip: currentPage * pageSize, limit: pageSize },
                      sortings,
                      filters
                    })
                    this.setState({ modal2Visible: false })
                  })
              }
            }}
          >
            {`${this.state.newTrip ? 'Добавить' : 'Изменить'}`}
          </Button>
        ]}
      >
        {!(this.state.newTrip && this.state.trip) && (
          <Row className="margin-bottom" gutter={16}>
            <Col span={15}>
              <InputField
                className="flex"
                value={this.state.newName}
                placeholder={'Название рейса'}
                onChange={e => {
                  const newName = e.target.value
                  this.setState({ newName })
                }}
                hideLabel
                disabled={this.state.trip}
              />
            </Col>

            <Col span={9}>
              {this.state.newTrip && (
                <GreenButton
                  label="Добавить станцию"
                  onClick={() => {
                    const showAdd = true
                    this.setState({ showAdd })
                  }}
                />
              )}
            </Col>
          </Row>
        )}
        {((this.state.showAdd || get(this, 'state.stations.length')) && (
          <Row>
            <Col span={24}>
              <OldTable>
                <HeaderRow>
                  <HeaderCell>Наименование</HeaderCell>
                  <HeaderCell>Время прибытия</HeaderCell>
                  <HeaderCell>Время убытия</HeaderCell>
                  <HeaderCell className="last header-cell" />
                </HeaderRow>

                {this.state.showAdd && (
                  <Col
                    className=""
                    style={{
                      borderLeft: '1px solid #666666',
                      borderTop: '1px solid #666666',
                      borderBottom: '1px solid #666666',
                      borderRight: '1px solid #666666'
                    }}
                  >
                    <div className="layout horizontal center padding-top padding-right padding-left">
                      <Select
                        className="flex"
                        placeholder="Станция"
                        showSearch
                        value={get(this, 'state.new_station.value')}
                        defaultActiveFirstOption={false}
                        showArrow={false}
                        filterOption={false}
                        onSearch={value => {
                          this.props.getStation(value)
                        }}
                        onChange={new_station => {
                          this.setState({
                            new_station: find(this.props.stations, {
                              value: new_station as any
                            })
                          })
                        }}
                      >
                        {this.props.stations &&
                          this.props.stations.map(station => <Option value={station.value}>{station.label}</Option>)}
                      </Select>

                      <span
                        className="icon-save path1 path2 path3 margin-left margin-right"
                        onClick={() => {
                          if (this.state.start && this.state.end) {
                            const row = {
                              station: this.state.new_station,
                              startTime: this.state.start,
                              endTime: this.state.end
                            }
                            const stations = this.state.stations || []
                            stations.push(row)
                            this.setState({
                              stations,
                              new_station: null,
                              start: null,
                              end: null,
                              showAdd: false
                            })
                          }
                        }}
                        style={{ fontSize: '30px' }}
                      >
                        <span className="path1" />
                        <span className="path2" />
                        <span className="path3" />
                      </span>
                    </div>

                    <Roww className="margin-top margin-bottom">
                      <Label className="margin-left" text={'Время прибытия'}>
                        <TimePicker
                          format={'HH:mm'}
                          value={this.state.start}
                          onChange={start => {
                            // console.log(start.format('HH:mm:ss'))
                            // console.log(
                            //   moment(
                            //     '0001-01-01T' + start.format('HH:mm:ss') + 'Z'
                            //   )
                            // )
                            this.setState({
                              start: moment('0001-01-01T' + start.format('HH:mm:ss') + 'Z').utcOffset(0)
                            })
                          }}
                          placeholder=""
                        />
                      </Label>

                      <Label className="margin-left" text={'Время убытия'}>
                        <TimePicker
                          format={'HH:mm'}
                          value={this.state.end}
                          onChange={end => {
                            const day = moment(this.state.start).isBefore(moment(end)) ? '0001-01-01' : '0001-01-02'
                            console.log(day)
                            this.setState({
                              end: moment(day + 'T' + end.format('HH:mm:ss') + 'Z').utcOffset(0)
                            })
                          }}
                          placeholder=""
                          // disabledHours={() => {
                          //   const h = moment(this.state.start).hour()
                          //   let a = []
                          //   for (let i = 0; i < h; i++) {
                          //     a.push(i)
                          //   }
                          //   return a
                          // }}
                          // disabledMinutes={selectedHour => {
                          //   const h = moment(this.state.start).hour()
                          //   if (h === selectedHour) {
                          //     const m = moment(this.state.start).minute()
                          //     let a = []
                          //     for (let i = 0; i < m; i++) {
                          //       a.push(i)
                          //     }
                          //     return a
                          //   }
                          // }}
                        />
                      </Label>
                    </Roww>
                  </Col>
                )}

                {this.state.stations &&
                  this.state.stations.map((row, index) => (
                    <div key={`ct_${index}${row.id}`}>
                      <OldRow className={row.expanded || row.showEdit ? 'expanded' : ''}>
                        <Cell
                          onClick={() => {
                            // this.onCellClick(row)
                          }}
                        >
                          {(row.station && (row.station.label || row.station.name)) || ''}
                        </Cell>
                        <Cell
                          onClick={() => {
                            // this.onCellClick(row)
                          }}
                        >
                          {moment(row.startTime)
                            .utcOffset(0)
                            .format('HH:mm')}
                        </Cell>
                        <Cell>
                          {moment(row.endTime)
                            .utcOffset(0)
                            .format('HH:mm')}
                        </Cell>
                        <Cell className="last cell layout horizontal center-center">
                          {this.state.newTrip && (
                            <div
                              className="icon-delete"
                              onClick={() => {
                                const stations = this.state.stations
                                stations.splice(index, 1)
                                this.setState({ stations })
                              }}
                            />
                          )}
                        </Cell>
                      </OldRow>
                    </div>
                  ))}
              </OldTable>
            </Col>
          </Row>
        )) ||
          ''}

        {this.state.newTrip && (
          <Row className="margin-top">
            <Col span={5}>
              <InputField
                label="Понедельник"
                type={'checkbox'}
                className="margin-top margin-left flex"
                rightLabel
                value={this.state.d1}
                onChange={e => {
                  const d1 = e.target.checked
                  this.setState({ d1 })
                }}
              />
            </Col>
            <Col span={5}>
              <InputField
                label="Вторник"
                type={'checkbox'}
                className="margin-top margin-left flex"
                rightLabel
                value={this.state.d2}
                onChange={e => {
                  const d2 = e.target.checked
                  this.setState({ d2 })
                }}
              />
            </Col>
            <Col span={5}>
              <InputField
                label="Среда"
                type={'checkbox'}
                className="margin-top margin-left flex"
                rightLabel
                value={this.state.d3}
                onChange={e => {
                  const d3 = e.target.checked
                  this.setState({ d3 })
                }}
              />
            </Col>
            <Col span={5}>
              <InputField
                label="Черверг"
                type={'checkbox'}
                className="margin-top margin-left flex"
                rightLabel
                value={this.state.d4}
                onChange={e => {
                  const d4 = e.target.checked
                  this.setState({ d4 })
                }}
              />
            </Col>
            <Col span={5}>
              <InputField
                label="Пятница"
                type={'checkbox'}
                className="margin-top margin-left flex"
                rightLabel
                value={this.state.d5}
                onChange={e => {
                  const d5 = e.target.checked
                  this.setState({ d5 })
                }}
              />
            </Col>
            <Col span={5}>
              <InputField
                label="Суббота"
                type={'checkbox'}
                className="margin-top margin-left flex"
                rightLabel
                value={this.state.d6}
                onChange={e => {
                  const d6 = e.target.checked
                  this.setState({ d6 })
                }}
              />
            </Col>
            <Col span={5}>
              <InputField
                label="Воскресенье"
                type={'checkbox'}
                className="margin-top margin-left flex"
                rightLabel
                value={this.state.d7}
                onChange={e => {
                  const d7 = e.target.checked
                  this.setState({ d7 })
                }}
              />
            </Col>
          </Row>
        )}
      </Modal>
    )
  }
}

const provider = provide((state: ApplicationState) => state.routes, Store.actionCreators)

type Props = typeof provider.allProps

export default provider.connect(CLS)
