import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from './TrainsStore'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, TextareaField, DropdownField } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'
import Tabs from '../../UI/tab/Tabs'
import Akts from './Akts';

export const modelTypeLabels = ['Головной вагон', 'Моторный вагон', 'Прицепной вагон']
export const modelTypes = ['головной', 'моторный', 'прицепной']

interface State {
  skip: number
  limit: number
  loading: boolean
  name: string
  depo: number
  desc: string
  reload: boolean
  model: number
  num: number
  z_num: string
  editName: string
  editDepo: number
  editDesc: string
  innerEditName: string
  innerEditDesc: string
  selectedTab: number
  fail: number
  newFails: any
  showAdd: boolean
  filter1: string
  filter2: number
}

class Trains extends React.Component<Props, State> {
  componentWillMount() {
    this.setState({
      skip: 0,
      limit: 10,
      loading: false,
      name: null,
      depo: null,
      desc: null,
      model: null,
      num: null,
      z_num: null,
      reload: false,
      editName: null,
      editDepo: null,
      editDesc: null,
      innerEditName: null,
      innerEditDesc: null,
      selectedTab: 0,
      fail: null,
      newFails: [],
      showAdd: false,
      filter1: null,
      filter2: null
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

  setDepo = event => {
    const depoS = event.currentTarget.value
    const depo = parseInt(depoS)
    this.setState({ depo })
  }

  setDesc = event => {
    const desc = event.currentTarget.value
    this.setState({ desc })
  }

  add = () => {
    const { name, depo, desc } = this.state
    if (name && depo != null) {
      this.props.addCat({ Name: name, Description: desc, StantionId: depo })

      this.setState({
        ...this.state,
        name: null,
        depo: null,
        desc: null,
        showAdd: false
      })
    }
  }

  setNum = event => {
    const numS = event.currentTarget.value
    const num = parseInt(numS)
    this.setState({ num })
  }

  setZNum = event => {
    const z_num = event.currentTarget.value
    this.setState({ z_num })
  }

  setModel = event => {
    const modelS = event.currentTarget.value
    const model = parseInt(modelS)
    this.setState({ model })
  }

  addVag = tid => {
    const { num, z_num, model } = this.state
    if (num !== null && z_num && model !== null) {
      this.props.addVag({ Number: num, Serial: z_num, ModelId: model, TrainId: tid })
      this.setState({
        ...this.state,
        num: null,
        z_num: null,
        model: null
      })
    }
  }

  editVag = (tid, row) => {
    const { num, z_num, model } = this.state
    if ((num !== null || row.number !== null) && (z_num || row.serial) && (model !== null || row.modelId !== null)) {
      this.props.addVag({
        Number: num === null ? row.number : num,
        Serial: z_num === null ? row.serial : z_num,
        TrainId: tid,
        ModelId: model === null ? row.modelId : model,
        id: row.id
      })
      this.setState({
        ...this.state,
        num: null,
        z_num: null,
        model: null
      })
    }
  }

  setPage = (skip, l) => {
    const { limit, filter1, filter2 } = this.state
    this.props.getCategories(skip, (l > 0 && l) || limit, filter1, filter2)
    this.setState({ skip })
  }

  onCellClick = row => {
    if (!row.showEdit) {
      this.setState({ ...this.state, showAdd: false, selectedTab: 0 })
      this.props.showEdit(row.id)
      this.props.getAkts(row.id)
    } else {
      this.props.hideEdit(row.id)
      this.setState({
        ...this.state,
        editName: null,
        editDepo: null
      })
    }
  }

  onInnerCellClick = (row, innerRow) => {
    if (!innerRow.showEdit) {
      this.setState({ ...this.state, showAdd: false, selectedTab: 0 })
      this.props.showInnerEdit(row.id, innerRow.id)
    } else {
      this.props.hideInnerEdit(row.id, innerRow.id)
      this.setState({
        ...this.state,
        innerEditName: null,
        innerEditDesc: null
      })
    }
  }

  edit = row => {
    const { editName, editDepo, editDesc } = this.state
    if ((row.name || editName) && ((row.stantion && row.stantion.id) || editDepo)) {
      this.props.addCat({
        Name: editName === null ? row.name : editName,
        Description: editDesc === null ? row.description : editDesc,
        Id: row.id,
        StantionId: editDepo === null && row.stantion ? row.stantion.id : editDepo
      })
      this.setState({
        ...this.state,
        editName: null,
        editDepo: null,
        editDesc: null
      })
    }
  }

  setEditName = event => {
    const editName = event.currentTarget.value
    this.setState({ editName })
  }

  setEditDepo = event => {
    const editDepoS = event.currentTarget.value
    const editDepo = parseInt(editDepoS)
    this.setState({ editDepo })
  }

  setEditDesc = event => {
    const editDesc = event.currentTarget.value
    this.setState({ editDesc })
  }

  setFail = event => {
    const failS = event.currentTarget.value
    const fail = parseInt(failS)
    this.setState({ fail })
  }

  addNewFail = () => {
    const { fail, newFails } = this.state
    if (fail !== null) {
      const newNewFails = newFails
      const findFail = newNewFails.find(el => el === fail)
      !findFail && newNewFails.push(fail)
      this.setState({ ...this.state, newFails: newNewFails, fail: null })
    }
  }

  getFailLabel = id => {
    const { stations } = this.props
    const station = stations.find(f => f.value === id)
    return (station && station.label) || '__'
  }

  delNewFail = id => {
    const { newFails } = this.state
    if (id !== null) {
      const newNewFails = newFails
      const findFail = newNewFails.findIndex(el => el === id)
      findFail !== -1 && newNewFails.splice(findFail, 1)
      this.setState({ ...this.state, newFails: newNewFails, fail: null })
    }
  }

  addFail = (cId, eId) => {
    const { fail } = this.state
    if (fail !== null) {
      this.props.addFail(cId, eId, fail)
      this.setState({ ...this.state, newFails: [], fail: null })
    }
  }

  delFail = (cId, eId, fId) => {
    const { fail } = this.state
    if (fId !== null) {
      this.props.delFail(cId, eId, fId)
    }
  }

  render() {
    const { result: { data, total }, stations, models } = this.props
    const {
      name,
      depo,
      num,
      z_num,
      skip,
      limit,
      editName,
      editDepo,
      editDesc,
      fail,
      newFails,
      showAdd,
      desc,
      model,
      filter1,
      filter2
    } = this.state
    return (
      <div>
        <div className="add-item card">
          <div className="layout vertical">
            <div className=" layout horizontal">
              <InputField
                label="Название поезда"
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
                style={{ minWidth: '250px' }}
              />
              <DropdownField
                label="Название депо"
                className="flex margin-left"
                list={stations}
                showNull
                value={filter2}
                onChange={event => {
                  this.setState({ ...this.state, filter2: event.currentTarget.value })
                }}
                style={{ minWidth: '250px' }}
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
          <div className=" layout horizontal margin-bottom">
            <GreenButton
              label="Добавить поезд"
              onClick={() => {
                this.props.hideEditors()
                this.setState({ ...this.state, showAdd: true })
              }}
            />
          </div>
          <Table>
            <HeaderRow>
              <HeaderCell className="first header-cell" />
              <HeaderCell>Наименование</HeaderCell>
              <HeaderCell>Депо</HeaderCell>
              <HeaderCell className="last header-cell" />
              {/* <HeaderCell className="last header-cell" /> */}
              <HeaderCell className="last header-cell" />
            </HeaderRow>

            {showAdd && (
              <div
                className="layout vertical"
                style={{
                  borderLeft: '1px solid #666666',
                  borderTop: '1px solid #666666',
                  borderBottom: '1px solid #666666',
                  borderRight: '1px solid #666666'
                }}
              >
                <div className="layout horizontal center">
                  <InputField
                    label="Название поезда"
                    onChange={this.setName}
                    value={name}
                    className="flex margin-left"
                  />
                  <DropdownField
                    label="Название депо"
                    onChange={this.setDepo}
                    value={depo}
                    className="flex margin-left"
                    list={stations}
                    showNull
                  />
                  <span className="icon-save path1 path2 path3 margin" onClick={this.add} style={{ fontSize: '30px' }}>
                    <span className="path1" />
                    <span className="path2" />
                    <span className="path3" />
                  </span>
                </div>
                <div>
                  <TextareaField
                    label="Описание"
                    onChange={this.setDesc}
                    value={desc}
                    className="flex margin-left margin-bottom margin-right"
                  />
                </div>
              </div>
            )}

            {data &&
              data.map((row, index) => (
                <div key={`ct_${index}${row.id}`}>
                  <Row className={row.expanded || row.showEdit ? 'expanded' : ''}>
                    <Cell
                      className="first cell layout horizontal center-center"
                      onClick={() => (!row.expanded && this.props.expandRow(row.id)) || this.props.unexpandRow(row.id)}
                    >
                      {(row.expanded && <span className="icon-chevron-down" style={{ fontSize: '16px' }} />) || (
                        <span className="icon-chevron-right" style={{ fontSize: '16px' }} />
                      )}
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
                      {row.stantion && row.stantion.name}
                    </Cell>
                    <Cell className="last cell layout horizontal center-center">
                      <div
                        className="icon-add"
                        onClick={() => {
                          this.setState({
                            ...this.state,
                            num: null,
                            z_num: null,
                            showAdd: false
                          })
                          ;(!row.expanded && this.props.expandRow(row.id, true)) || this.props.showAdd(row.id)
                        }}
                      />
                    </Cell>
                    {/* <Cell className="last cell layout horizontal center-center">
                      <div
                        className="icon-add-vagon-2"
                        onClick={() => {
                          // this.setState({ ...this.state, num: null, z_num: null })
                          // ;(!row.expanded && this.props.expandRow(row.id, true)) || this.props.showAdd(row.id)
                        }}
                      />
                    </Cell> */}
                    <Cell className="last cell layout horizontal center-center">
                      <div
                        className={(true && 'icon-delete') || 'icon-delete-1'}
                        onClick={() => {
                          true && this.props.delCat(row.id)
                        }}
                      />
                    </Cell>
                  </Row>

                  {row.showEdit && (
                    <div
                      className="layout vertical"
                      style={{
                        borderLeft: '1px solid #666666',
                        borderTop: '1px solid #666666',
                        borderBottom: '1px solid #666666',
                        borderRight: '1px solid #666666'
                      }}
                    >
                      <Tabs
                        tabs={[{ label: 'СОСТАВ' }, { label: 'АКТЫ ТЕХОСМОТРА' }]}
                        selectedTab={this.state.selectedTab}
                        onTabChanged={selectedTab => {
                          this.setState({ ...this.state, selectedTab })
                        }}
                      />

                      {this.state.selectedTab === 0 && (
                        <React.Fragment>
                          <div className="layout horizontal center">
                            <InputField
                              label="Название поезда"
                              onChange={this.setEditName}
                              value={editName === null ? row.name : editName}
                              className="flex margin-left"
                            />
                            <DropdownField
                              label="Название депо"
                              onChange={this.setEditDepo}
                              value={editDepo === null && row.stantion ? row.stantion.id : editDepo}
                              className="flex margin-left"
                              list={stations}
                              showNull
                            />
                            <span
                              className="icon-save path1 path2 path3 margin"
                              onClick={() => {
                                this.edit(row)
                              }}
                              style={{ fontSize: '30px' }}
                            >
                              <span className="path1" />
                              <span className="path2" />
                              <span className="path3" />
                            </span>
                          </div>
                          <div>
                            <TextareaField
                              label="Описание"
                              onChange={this.setEditDesc}
                              value={editDesc === null ? row.description : editDesc}
                              className="flex margin-left margin-bottom margin-right"
                            />
                          </div>
                        </React.Fragment>
                      )}

                      {this.state.selectedTab === 1 && (
                        <Akts 
                          trainId={row.id} 
                          onAdd={data=>{this.props.addAkt(data)}} 
                          onDelete={id=>{this.props.delAkt(id)}} 
                          akts={this.props.akts}
                        />
                      )}
                    </div>
                  )}

                  {row.expanded && (
                    <Table className="inner-table layout vertical">
                      <HeaderRow>
                        <HeaderCell>Модель</HeaderCell>
                        <HeaderCell>Тип</HeaderCell>
                        <HeaderCell>Заводской номер</HeaderCell>
                        <HeaderCell>Номер</HeaderCell>
                        {/* <HeaderCell className="last header-cell" /> */}
                        <HeaderCell className="last header-cell" />
                      </HeaderRow>

                      {row.showAdd && (
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
                              <div className="flex layout vertical">
                                <DropdownField
                                  label="Модель"
                                  onChange={this.setModel}
                                  value={model}
                                  list={models}
                                  showNull
                                />
                                <InputField
                                  className="margin-top"
                                  label="Заводской номер"
                                  onChange={this.setZNum}
                                  value={z_num}
                                />
                              </div>
                              <div className="flex layout vertical margin-left">
                                <InputField className="" label="Номер" onChange={this.setNum} value={num} />
                              </div>
                              <div className="flex-none layout horizontal center-center">
                                <span
                                  className="icon-save path1 path2 path3 margin-left"
                                  onClick={() => {
                                    this.addVag(row.id)
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
                        </div>
                      )}

                      {row.vagons &&
                        row.vagons.map(innerRow => (
                          <div key={`eq_${index}${innerRow.id}`}>
                            <Row className={innerRow.expanded || innerRow.showEdit ? 'expanded' : ''}>
                              <Cell
                                onClick={() => {
                                  this.onInnerCellClick(row, innerRow)
                                }}
                              >
                                {innerRow.model.name}
                              </Cell>
                              <Cell
                                onClick={() => {
                                  this.onInnerCellClick(row, innerRow)
                                }}
                              >
                                {modelTypeLabels[innerRow.model.modelType]}
                              </Cell>
                              <Cell
                                onClick={() => {
                                  this.onInnerCellClick(row, innerRow)
                                }}
                              >
                                {innerRow.serial}
                              </Cell>
                              <Cell
                                onClick={() => {
                                  this.onInnerCellClick(row, innerRow)
                                }}
                              >
                                {innerRow.number}
                              </Cell>
                              {/* <Cell className="last cell layout horizontal center-center">
                                <div
                                  className="icon-add-vagon-2"
                                  onClick={() => {
                                    // this.props.delEq(innerRow.id, row.id)
                                  }}
                                />
                              </Cell> */}
                              <Cell className="last cell layout horizontal center-center">
                                <div
                                  className={(true && 'icon-delete') || 'icon-delete-1'}
                                  onClick={() => {
                                    true && this.props.delEq(innerRow.id, row.id)
                                  }}
                                />
                              </Cell>
                            </Row>

                            {innerRow.showEdit && (
                              <div
                                className="layout vertical"
                                style={{
                                  borderLeft: '1px solid #666666',
                                  borderTop: '1px solid #666666',
                                  borderBottom: '1px solid #666666',
                                  borderRight: '1px solid #666666'
                                }}
                              >
                                <Tabs
                                  tabs={[{ label: 'ОСНОВНЫЕ' }, { label: 'ОБОРУДОВАНИЕ' }]}
                                  selectedTab={this.state.selectedTab}
                                  onTabChanged={id => {
                                    this.setState({ ...this.state, selectedTab: id })
                                  }}
                                />

                                {this.state.selectedTab === 0 && (
                                  <div className="layout vertical">
                                    <div className="layout horizontal margin">
                                      <div className="flex layout vertical">
                                        <DropdownField
                                          label="Модель"
                                          onChange={this.setModel}
                                          value={model === null ? innerRow.modelId : model}
                                          list={models}
                                        />
                                        <InputField
                                          className="margin-top"
                                          label="Заводской номер"
                                          onChange={this.setZNum}
                                          value={z_num === null ? innerRow.serial : z_num}
                                        />
                                      </div>
                                      <div className="flex layout vertical margin-left">
                                        <InputField
                                          className=""
                                          label="Номер"
                                          onChange={this.setNum}
                                          value={num === null ? innerRow.number : num}
                                        />
                                      </div>
                                      <div className="flex-none layout horizontal center-center">
                                        <span
                                          className="icon-save path1 path2 path3 margin-left"
                                          onClick={() => {
                                            this.editVag(row.id, innerRow)
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

                                {this.state.selectedTab === 1 && (
                                  <div className="layout vertical">
                                    {innerRow.equipments.length > 0 && (
                                      <Table className="table layout vertical margin">
                                        <Row>
                                          <HeaderCell>Местоположение</HeaderCell>
                                          <HeaderCell>Наименование</HeaderCell>
                                          <HeaderCell>Алгоритмы проверок</HeaderCell>
                                          <HeaderCell>Метки</HeaderCell>
                                        </Row>

                                        {innerRow.equipments &&
                                          innerRow.equipments.map(eq => (
                                            <div>
                                              <Row>
                                                <Cell>{eq.location}</Cell>
                                                <Cell>{eq.equipment}</Cell>
                                                <Cell>{eq.algorithm}</Cell>
                                                <Cell>{eq.labels && eq.labels.join()}</Cell>
                                              </Row>
                                            </div>
                                          ))}
                                      </Table>
                                    )}
                                  </div>
                                )}
                              </div>
                            )}
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

const provider = provide((state: ApplicationState) => ({ ...state.trains }), Store.actionCreators)

type Props = typeof provider.allProps

export default provider.connect(Trains)
