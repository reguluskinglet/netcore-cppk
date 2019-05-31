import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from './LocationsStore'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, TextareaField, DropdownField } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'
import Tabs from '../../UI/tab/Tabs'
import _, { filter, map, get, find, findIndex } from 'lodash'

const algoritms = [{ label: 'ТО-1', value: 0 }, { label: 'ТО-2', value: 1 }, { label: 'Приемка ЛБ', value: 2 }]
const tasks = [{ label: 'Техническая', value: 0 }, { label: 'Санитарная', value: 1 }]
const controls = [{ label: 'Булево', value: 1 }, { label: 'Число', value: 2 }]

interface IAlgoritm {
    checkListType: number
    faultType: number
    nameTask: string
    value: number
    valueType: number | boolean
}

interface IEquipment {
    name: string
    id: number
}

interface ILocation {
    id: number
    modelId: number
    parentId: number
    equipment: IEquipment
    algoritm0: any
    algoritm1: any
    algoritm2: any
    algoritm3: any
    isMark: boolean
}

interface State {
    skip: number
    limit: number
    loading: boolean
    selectedTab: number
    showAdd: boolean
    name: string
    editName: string
    location: ILocation
    filter1: string
    filter2: number
    modelId: number
    algoritm: number
    expandedRows: any[]
}

class Locations extends React.Component<Props, State> {
    defaultLocation = () => {
        let l: ILocation = {
            id: 0,
            modelId: null,
            parentId: null,
            equipment: {
                id: null,
                name: ''
            },
            algoritm0: [],
            algoritm1: [],
            algoritm2: [],
            algoritm3: [],
            isMark: false
        }
        return l
    }

    componentWillMount() {
        this.setState({
            skip: 0,
            limit: 10,
            loading: false,
            selectedTab: 0,
            showAdd: false,
            name: null,
            editName: null,
            location: this.defaultLocation(),
            filter1: null,
            filter2: null,
            modelId: parseInt(this.props.match.params.id),
            algoritm: 0
        })
        this.props.getCategories(this.props.match.params.id, undefined, 0, 10)
    }

    componentWillReceiveProps(nextProps) {
        const { skip, limit, modelId } = this.state
        if (nextProps.reload) {
            this.props.getCategories(modelId, undefined, skip, limit)
        }
        const { result: { data }, expandedRows } = nextProps
        expandedRows.forEach(element => {
            const row = data.find(r => r.id === element)
            row && this.props.expandRow(row.id, this.state.modelId, row.id, this.state.filter1)
        })
    }

    add = id => {
        const { location } = this.state
        location.modelId = this.state.modelId
        if (true) {
            this.props.add(location, id)
        }
        this.setState({
            ...this.state,
            location: this.defaultLocation(),
            showAdd: false
        })
    }

    setPage = (skip, l) => {
        const { limit, filter1, filter2, modelId } = this.state
        this.props.getCategories(modelId, undefined, skip, (l > 0 && l) || limit, filter1, filter2)
        this.setState({ skip })
    }

    onCellClick = row => {
        if (!row.showEdit) {
            this.props.showEdit(row.id).then(algorithms => {
                const a0 = filter(algorithms, el => el.checkListType === 0)
                const a1 = filter(algorithms, el => el.checkListType === 1)
                const a2 = filter(algorithms, el => el.checkListType === 2)
                const a3 = filter(algorithms, el => el.checkListType === 3)
                this.setState({
                    ...this.state,
                    location: {
                        ...row,
                        algoritm0: a0 || [],
                        algoritm1: a1 || [],
                        algoritm2: a2 || [],
                        algoritm3: a3 || []
                    },
                    showAdd: false,
                    selectedTab: 0
                })
            })
        } else {
            this.props.hideEdit(row.id)
            this.setState({
                ...this.state,
                location: this.defaultLocation()
            })
        }
    }

    onInnerCellClick = (row, innerRow) => {
        if (!innerRow.showEdit) {
            const a0 = innerRow.algorithms.find(el => el.checkListType === 0)
            const a1 = innerRow.algorithms.find(el => el.checkListType === 1)
            const a2 = innerRow.algorithms.find(el => el.checkListType === 2)
            const a3 = innerRow.algorithms.find(el => el.checkListType === 3)
            this.setState({
                ...this.state,
                location: {
                    ...innerRow,
                    algoritm0: a0 || this.defaultLocation().algoritm0,
                    algoritm1: a1 || this.defaultLocation().algoritm1,
                    algoritm2: a2 || this.defaultLocation().algoritm2,
                    algoritm3: a3 || this.defaultLocation().algoritm3
                },
                showAdd: false,
                selectedTab: 0
            })
            this.props.showInnerEdit(row.id, innerRow.id)
        } else {
            this.props.hideInnerEdit(row.id, innerRow.id)
            this.setState({
                ...this.state,
                location: this.defaultLocation()
            })
        }
    }

    editElement = (id?) => {
        const { location, algoritm } = this.state
        const { equipments } = this.props
        return (
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
                    <Tabs
                        tabs={[{ label: 'ОСНОВНЫЕ' }, { label: 'ТО-1' }, { label: 'ТО-2' }, { label: 'Приёмка ЛБ' }, { label: 'Приёмка Пр' }]}
                        selectedTab={this.state.selectedTab}
                        onTabChanged={selectedTab => {
                            this.setState({ ...this.state, selectedTab, })
                        }}
                    />

                    {this.state.selectedTab === 0 && (
                        <div className="layout horizontal margin">
                            <DropdownField
                                className="margin-top flex-1"
                                label="Наименование"
                                onChange={event => {
                                    const value = parseInt(event.currentTarget.value)
                                    this.setState({
                                        ...this.state,
                                        location: { ...location, equipment: { id: value, name: null }, isMark: false }
                                    })
                                }}
                                value={location.equipment.id}
                                list={equipments}
                                showNull
                            />

                            <InputField
                                className="margin-top margin-left"
                                label="Подлежит маркировке"
                                value={location.isMark}
                                onChange={event => {
                                    const value = event.currentTarget.checked
                                    this.setState({
                                        ...this.state,
                                        location: { ...location, isMark: value }
                                    })
                                }}
                                type="checkbox"
                            />

                            <span
                                className="icon-save path1 path2 path3 margin-top margin-left margin-bottom"
                                onClick={() => {
                                    this.add(id)
                                }}
                                style={{ fontSize: '30px' }}
                            >
                                <span className="path1" />
                                <span className="path2" />
                                <span className="path3" />
                            </span>
                            <div className="flex" />
                        </div>
                    )}

                    {this.state.selectedTab >= 1 && (
                        <div className="layout vertical margin">
                            <div className="layout horizontal">
                                <DropdownField
                                    className="flex margin-right margin-bottom"
                                    label="Тип проверки"
                                    list={controls}
                                    value={location['algoritm' + (this.state.selectedTab - 1)].valueType >= 0
                                        ? location['algoritm' + (this.state.selectedTab - 1)].valueType
                                        : null}
                                    onChange={event => {
                                        if (algoritm >= 0) {
                                            const value = parseInt(event.currentTarget.value)
                                            const state = this.state
                                            state.location['algoritm' + (this.state.selectedTab - 1)].valueType = value
                                            value === 0 && (state.location['algoritm' + (this.state.selectedTab - 1)].value = 0)
                                            this.setState(state)
                                        }
                                    }}
                                    showNull
                                />
                                <DropdownField
                                    className="flex margin-bottom"
                                    label="Тип задачи"
                                    list={tasks}
                                    value={location['algoritm' + (this.state.selectedTab - 1)].faultType >= 0
                                        ? location['algoritm' + (this.state.selectedTab - 1)].faultType
                                        : null}
                                    onChange={event => {
                                        if (algoritm >= 0) {
                                            const value = parseInt(event.currentTarget.value)
                                            const state = this.state
                                            state.location['algoritm' + (this.state.selectedTab - 1)].faultType = value
                                            this.setState(state)
                                        }
                                    }}
                                />

                                <InputField
                                    className="flex margin-right margin-bottom"
                                    label="Наименование"
                                    value={location['algoritm' + (this.state.selectedTab - 1)].nameTask}
                                    onChange={event => {
                                        if (algoritm >= 0) {
                                            const value = event.currentTarget.value
                                            const state = this.state
                                            state.location['algoritm' + (this.state.selectedTab - 1)].nameTask = value
                                            this.setState(state)
                                        }
                                    }}
                                />
                                <InputField
                                    className="margin-right margin-bottom"
                                    label="Норма"
                                    value={location['algoritm' + (this.state.selectedTab - 1)].value}
                                    type={
                                        location['algoritm' + (this.state.selectedTab - 1)].valueType === 2
                                            ? 'number'
                                            : 'checkbox'
                                    }
                                    onChange={event => {
                                        if (algoritm >= 0) {
                                            const value =
                                                location['algoritm' + (this.state.selectedTab - 1)].valueType === 2
                                                    ? parseFloat(event.currentTarget.value)
                                                    : event.currentTarget.checked ? 1 : 0
                                            const state = this.state
                                            state.location['algoritm' + (this.state.selectedTab - 1)].value = value
                                            this.setState(state)
                                        }
                                    }}
                                />
                                <span
                                    className="icon-save path1 path2 path3"
                                    onClick={() => {
                                        const l = {
                                            checkListType: this.state.selectedTab - 1,
                                            faultType: location['algoritm' + (this.state.selectedTab - 1)].faultType,
                                            nameTask: location['algoritm' + (this.state.selectedTab - 1)].nameTask,
                                            value: location['algoritm' + (this.state.selectedTab - 1)].value,
                                            valueType: location['algoritm' + (this.state.selectedTab - 1)].valueType
                                        }
                                        this.props.addAlg(location.id, l).then(res => {
                                            location['algoritm' + (this.state.selectedTab - 1)].push(res)
                                            this.setState({
                                                location: {
                                                    ...location,
                                                    ['algoritm' + (this.state.selectedTab - 1)]: [...location['algoritm' + (this.state.selectedTab - 1)]]
                                                }
                                            })
                                        })
                                    }}
                                    style={{ fontSize: '30px' }}
                                >
                                    <span className="path1" />
                                    <span className="path2" />
                                    <span className="path3" />
                                </span>
                            </div>
                            <table>
                                <thead>
                                    <tr>
                                        <td>
                                            <b>Тип проверки</b>
                                        </td>
                                        <td>
                                            <b>Тип задачи</b>
                                        </td>
                                        <td>
                                            <b>Наименование</b>
                                        </td>
                                        <td>
                                            <b>Норма</b>
                                        </td>
                                        <td />
                                    </tr>
                                </thead>
                                <tbody>
                                    {map(get(location, 'algoritm' + (this.state.selectedTab - 1)), (alg: any, index) => (
                                        <tr key={index}>
                                            <td>{alg.valueType}</td>
                                            <td>{alg.faultType}</td>
                                            <td>{alg.nameTask}</td>
                                            <td>{alg.valueType === 'Число' ? alg.value : alg.value == 1 ? 'Да' : 'Нет'}</td>
                                            <td>
                                                <a
                                                    onClick={() => {
                                                        this.props.delAlg(alg.id).then(() => {
                                                            const idx = findIndex(
                                                                location['algoritm' + (this.state.selectedTab - 1)],
                                                                (el: any) => el.id === alg.id
                                                            )
                                                            idx >= 0 && location['algoritm' + (this.state.selectedTab - 1)].splice(idx, 1)
                                                            this.setState({ location })
                                                        })
                                                    }}
                                                >
                                                    Удалить
                        </a>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </div>
            </div>
        )
    }

    renderRow = ({ id }, index, level) => {
        const { filter2, modelId} = this.state
        const { equipments2, rows, showAddId, expandedRows } = this.props
        const row = _.get(rows, id, {})
        const { equipments = [] } = row

        const isExpanded = expandedRows && expandedRows.findIndex(x => x.id == id) >= 0

        return (
            <div key={`ct_${index}${row.id}`}>
                <Row className={ isExpanded || row.showEdit ? 'expanded' : ''}>
                    <Cell
                        className="first cell layout horizontal center-center"
                        onClick={() => {
                            if (!isExpanded) {
                                this.props.expandRow(row.id, modelId, row.id, filter2)
                            } else {
                                this.props.unexpandRow(row.id)
                            }
                        }}
                    >
                        {( isExpanded && <span className="icon-chevron-down" style={{ fontSize: '16px' }} />) || (
                            <span className="icon-chevron-right" style={{ fontSize: '16px' }} />
                        )}
                    </Cell>
                    <Cell
                        onClick={() => {
                            this.onCellClick(row)
                        }}
                    >
                        {row.equipment.name}
                    </Cell>
                    <Cell className="last cell layout horizontal center-center">
                        <div
                            className="icon-add"
                            onClick={() => {
                                const l = this.defaultLocation()
                                l.parentId = row.id
                                this.setState({
                                    location: l,
                                    showAdd: false
                                })
                                    ; (!row.expanded && this.props.expandRow(row.id, modelId, row.id, filter2, true)) ||
                                        this.props.showAdd(row.id)
                            }}
                        />
                    </Cell>
                    <Cell className="last cell layout horizontal center-center">
                        <div
                            className="icon-delete"
                            onClick={() => {
                                const { location } = this.state
                                location.modelId = this.state.modelId
                                location.parentId = row.parentId

                                this.props.del(location, row.id)
                            }}
                        />
                    </Cell>
                </Row>

                {row.showEdit && this.editElement()}

                {isExpanded && (
                    <Table className="inner-table layout vertical">
                        {row.id === showAddId && this.editElement(row.id)}
                        {equipments.map(this.renderRow)}

                        {/* {equipments.map(innerRow => (
              <div key={`eq_${index}${innerRow.id}`}>
                <Row className={innerRow.expanded || innerRow.showEdit ? 'expanded' : ''}>
                  <Cell
                    className="first cell layout horizontal center-center"
                    onClick={() => {
                      if (!innerRow.expanded) {
                        this.props.expandRow(innerRow.id, modelId, innerRow.id, filter2)
                      } else {
                        this.props.unexpandRow(innerRow.id)
                      }
                    }}
                  >
                    {(row.expanded && <span className="icon-chevron-down" style={{ fontSize: '16px' }} />) || (
                      <span className="icon-chevron-right" style={{ fontSize: '16px' }} />
                    )}
                  </Cell>
                  <Cell
                    onClick={() => {
                      this.onInnerCellClick(row, innerRow)
                    }}
                  >
                    {innerRow.equipment.name}
                  </Cell>
                  <Cell className="last cell layout horizontal center-center">
                    <div
                      className="icon-add"
                      onClick={() => {
                        const l = this.defaultLocation()
                        l.parentId = innerRow.id
                        this.setState({
                          ...this.state,
                          location: l,
                          showAdd: false
                        })
                        ;(!innerRow.expanded &&
                          this.props.expandRow(innerRow.id, modelId, innerRow.id, filter2, true)) ||
                          this.props.showAdd(innerRow.id)
                      }}
                    />
                  </Cell>
                  <Cell className="last cell layout horizontal center-center">
                    <div
                      className="icon-delete"
                      onClick={() => {
                        this.props.delEq(modelId, innerRow.id, row)
                      }}
                    />
                  </Cell>
                </Row>

                {innerRow.showEdit && this.editElement(row.id)}
              </div>
            ))} */}
                    </Table>
                )}
            </div>
        )
    }

    render() {
        const { result: { data, total, model }, equipments2, rows } = this.props
        const { name, skip, limit, editName, location, showAdd, filter1, filter2, modelId } = this.state
        const typeLabels = ['Головной вагон', 'Моторный вагон', 'Прицепной вагон']
        const rootRows = _.filter(rows, (row: any) => row.parentId === null)

        return (
            <div>
                <div className="layout horizontal end margin-bottom">
                    <span
                        style={{ fontSize: '16px', cursor: 'pointer', color: '#3D496B' }}
                        onClick={() => {
                            this.props.history.goBack()
                        }}
                    >
                        Модели
                    </span>
                    <span
                        className="icon-chevron-right"
                        style={{ fontSize: '16px', padding: '0 4px 1px 4px', cursor: 'pointer', color: '#3D496B' }}
                    />
                    <span style={{ textDecoration: 'underline', cursor: 'pointer', color: '#3D496B', fontSize: '16px' }}>
                        {`${model.name} (${model.modelType >= 0 && typeLabels[model.modelType]})`}
                    </span>
                </div>

                <div className="add-item card">
                    <div className="layout vertical">
                        <div className="layout horizontal">
                            <InputField
                                label="Местоположение"
                                className="flex"
                                value={filter1}
                                onChange={event => {
                                    this.setState({ ...this.state, filter1: event.currentTarget.value })
                                }}
                                onEnter={() => {
                                    const { limit, filter1, filter2 } = this.state
                                    this.setState({ ...this.state, skip: 0 })
                                    this.props.getCategories(modelId, undefined, 0, limit, filter1, filter2)
                                }}
                            />

                            <InputField
                                label="Оборудование"
                                className="flex margin-left"
                                value={filter2}
                                onChange={event => {
                                    this.setState({ ...this.state, filter2: event.currentTarget.value })
                                }}
                                onEnter={() => {
                                    const { limit, filter1, filter2 } = this.state
                                    this.setState({ ...this.state, skip: 0 })
                                    this.props.getCategories(modelId, undefined, 0, limit, filter1, filter2)
                                }}
                            />

                            <div className="flex " />
                            <BlueButton
                                label="Применить фильтр"
                                className="margin-left"
                                onClick={() => {
                                    const { limit, filter1, expandedRows } = this.state
                                    this.setState({ ...this.state, skip: 0 })
                                    this.props.getCategories(modelId, undefined, 0, limit, filter1, expandedRows)
                                }}
                            />
                            <BlueButton
                                label="Сбросить"
                                className="margin-left"
                                onClick={() => {
                                    const { limit, modelId } = this.state
                                    this.setState({ ...this.state, skip: 0, filter1: null, filter2: null })
                                    this.props.getCategories(modelId, undefined, 0, limit)
                                }}
                            />
                        </div>
                    </div>
                </div>

                <div className="table-layout card  layout vertical margin-top">
                    <div className="layout horizontal margin-bottom">
                        <GreenButton
                            label="Добавить местоположение"
                            onClick={() => {
                                this.props.hideEditors()
                                this.setState({ ...this.state, showAdd: true, location: this.defaultLocation() })
                            }}
                        />
                    </div>
                    <Table>
                        <HeaderRow>
                            <HeaderCell className="first header-cell" />
                            <HeaderCell>Наименование</HeaderCell>
                            <HeaderCell className="last header-cell" />
                            <HeaderCell className="last header-cell" />
                        </HeaderRow>

                        {showAdd && this.editElement()}

                        {rootRows &&
                            rootRows.map(
                            (row, index) => this.renderRow(row, index, 0)                            
                                // <div key={`ct_${index}${row.id}`}>
                                //   <Row className={row.expanded || row.showEdit ? 'expanded' : ''}>
                                //     <Cell
                                //       className="first cell layout horizontal center-center"
                                //       onClick={() => {
                                //         if (!row.expanded) {
                                //           this.props.expandRow(row.id, modelId, row.id, filter2)
                                //         } else {
                                //           this.props.unexpandRow(row.id)
                                //         }
                                //       }}
                                //     >
                                //       {(row.expanded && <span className="icon-chevron-down" style={{ fontSize: '16px' }} />) || (
                                //         <span className="icon-chevron-right" style={{ fontSize: '16px' }} />
                                //       )}
                                //     </Cell>
                                //     <Cell
                                //       onClick={() => {
                                //         this.onCellClick(row)
                                //       }}
                                //     >
                                //       {row.equipment.name}
                                //     </Cell>
                                //     <Cell className="last cell layout horizontal center-center">
                                //       <div
                                //         className="icon-add"
                                //         onClick={() => {
                                //           const l = this.defaultLocation()
                                //           l.parentId = row.id
                                //           this.setState({
                                //             ...this.state,
                                //             location: l,
                                //             showAdd: false
                                //           })
                                //           ;(!row.expanded && this.props.expandRow(row.id, modelId, row.id, filter2, true)) ||
                                //             this.props.showAdd(row.id)
                                //         }}
                                //       />
                                //     </Cell>
                                //     <Cell className="last cell layout horizontal center-center">
                                //       <div
                                //         className="icon-delete"
                                //         onClick={() => {
                                //           this.props.del(row.id)
                                //         }}
                                //       />
                                //     </Cell>
                                //   </Row>

                                //   {row.showEdit && this.editElement()}

                                //   {row.expanded && (
                                //     <Table className="inner-table layout vertical">
                                //       {/* <HeaderRow>
                                //         <HeaderCell>Оборудование</HeaderCell>
                                //       </HeaderRow> */}

                                //       {row.showAdd && this.editElement(row.id)}

                                //       {row.equipments &&
                                //         row.equipments.map(innerRow => (
                                //           <div key={`eq_${index}${innerRow.id}`}>
                                //             <Row className={innerRow.expanded || innerRow.showEdit ? 'expanded' : ''}>
                                //               <Cell
                                //                 className="first cell layout horizontal center-center"
                                //                 onClick={() => {
                                //                   if (!innerRow.expanded) {
                                //                     this.props.expandRow(innerRow.id, modelId, innerRow.id, filter2)
                                //                   } else {
                                //                     this.props.unexpandRow(innerRow.id)
                                //                   }
                                //                 }}
                                //               >
                                //                 {(row.expanded && (
                                //                   <span className="icon-chevron-down" style={{ fontSize: '16px' }} />
                                //                 )) || <span className="icon-chevron-right" style={{ fontSize: '16px' }} />}
                                //               </Cell>
                                //               <Cell
                                //                 onClick={() => {
                                //                   this.onInnerCellClick(row, innerRow)
                                //                 }}
                                //               >
                                //                 {innerRow.equipment.name}
                                //               </Cell>
                                //               <Cell className="last cell layout horizontal center-center">
                                //                 <div
                                //                   className="icon-add"
                                //                   onClick={() => {
                                //                     const l = this.defaultLocation()
                                //                     l.parentId = innerRow.id
                                //                     this.setState({
                                //                       ...this.state,
                                //                       location: l,
                                //                       showAdd: false
                                //                     })
                                //                     ;(!innerRow.expanded &&
                                //                       this.props.expandRow(innerRow.id, modelId, innerRow.id, filter2, true)) ||
                                //                       this.props.showAdd(innerRow.id)
                                //                   }}
                                //                 />
                                //               </Cell>
                                //               <Cell className="last cell layout horizontal center-center">
                                //                 <div
                                //                   className="icon-delete"
                                //                   onClick={() => {
                                //                     this.props.delEq(modelId, innerRow.id, row)
                                //                   }}
                                //                 />
                                //               </Cell>
                                //             </Row>

                                //             {innerRow.showEdit && this.editElement(row.id)}
                                //           </div>
                                //         ))}
                                //     </Table>
                                //   )}
                                // </div>
                            )}
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

const provider = provide((state: ApplicationState) => state.locations, Store.actionCreators).withExternalProps<
    RouteComponentProps<any>
    >()

type Props = typeof provider.allProps

export default provider.connect(Locations)
