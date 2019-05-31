import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as EquipmentStore from '../../store/EquipmentStore'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, TextareaField, DropdownField } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'
import Tabs from '../../UI/tab/Tabs'

interface State {
  skip: number
  limit: number
  loading: boolean
  catName: string
  catDesc: string
  reload: boolean
  eqName: string
  eqDesc: string
  editName: string
  editDesc: string
  innerEditName: string
  innerEditDesc: string
  selectedTab: number
  fail: number
  newFails: any
  showAdd: boolean
  filter1: string
  filter2: number
  catAkt: number
  newFailName: string
}

class Equipment extends React.Component<Props, State> {
  componentWillMount() {
    this.setState({
      skip: 0,
      limit: 10,
      loading: false,
      catName: null,
      catDesc: null,
      eqName: null,
      eqDesc: null,
      reload: false,
      editName: null,
      editDesc: null,
      innerEditName: null,
      innerEditDesc: null,
      selectedTab: 0,
      fail: null,
      newFails: [],
      showAdd: false,
      filter1: null,
      filter2: null,
      catAkt: null
    })
    this.props.getCategories(0, 10)
  }

  componentWillReceiveProps(nextProps) {
    const { skip, limit, filter1 } = this.state
    if (nextProps.reload) {
      this.props.getCategories(skip, limit, filter1)
    }
    const { result: { data }, expandedRows } = nextProps
    expandedRows.forEach(element => {
      const row = data.find(r => r.id === element)
      if (row && !row.expanded) {
        this.props.expandRow(row.id, this.state.filter2)
      }
    })
  }

  setCatName = event => {
    const catName = event.currentTarget.value
    this.setState({ catName })
  }

  setCatNDesc = event => {
    const catDesc = event.currentTarget.value
    this.setState({ catDesc })
  }

  setCatAkt = event => {
    const catAkt = parseInt(event.currentTarget.value)
    this.setState({ catAkt })
  }

  addCat = () => {
    const { catName, catDesc } = this.state
    if (catName) {
      this.props.addCat({ Name: catName, Description: catDesc })
    }
    this.setState({
      ...this.state,
      catName: null,
      catDesc: null,
      showAdd: null
    })
  }

  setEqName = event => {
    const eqName = event.currentTarget.value
    this.setState({ eqName })
  }

  setEqDesc = event => {
    const eqDesc = event.currentTarget.value
    this.setState({ eqDesc })
  }

  addEq = id => {
    const { eqName, eqDesc, newFails, filter2, catAkt } = this.state
    if (eqName) {
      this.props.addEq(
        { Name: eqName, Description: eqDesc, CategoryId: id, fails: newFails, EquipmentActCategoryId: catAkt },
        filter2
      )
    }
    this.setState({
      ...this.state,
      eqName: null,
      eqDesc: null
    })
  }

  editEq = (id, row) => {
    const { eqName, eqDesc, newFails, filter2, catAkt } = this.state
    if (eqName || row.name) {
      this.props.addEq(
        {
          Name: eqName === null ? row.name : eqName,
          Description: eqDesc === null ? row.description : eqDesc,
          CategoryId: id,
          id: row.id,
          EquipmentActCategoryId: catAkt
        },
        filter2
      )
    }
    this.setState({
      ...this.state,
      eqName: null,
      eqDesc: null
    })
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
    } else {
      this.props.hideEdit(row.id)
      this.setState({
        ...this.state,
        editName: null,
        editDesc: null
      })
    }
  }

  onInnerCellClick = (row, innerRow) => {
    if (!innerRow.showEdit) {
      this.setState({ ...this.state, showAdd: false, selectedTab: 0, catAkt: innerRow.equipmentActCategoryId })
      this.props.showInnerEdit(row.id, innerRow.id)
    } else {
      this.props.hideInnerEdit(row.id, innerRow.id)
      this.setState({
        ...this.state,
        innerEditName: null,
        innerEditDesc: null,
        catAkt: null
      })
    }
  }

  edit = row => {
    const { editName, editDesc } = this.state
    if (row.name || editName) {
      this.props.addCat({
        Name: editName === null ? row.name : editName,
        Description: editDesc === null ? row.description : editDesc,
        Id: row.id
      })
      this.setState({
        ...this.state,
        editName: null,
        editDesc: null
      })
    }
  }

  setEditName = event => {
    const editName = event.currentTarget.value
    this.setState({ editName })
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
    const { fails } = this.props
    const fail = fails.find(f => f.value === id)
    return (fail && fail.label) || '__'
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

  addFail = (cId, eId, fails) => {
    const { fail } = this.state
    const findFail = fails.findIndex(el => el.id === fail)
    if (fail !== null && findFail === -1) {
      this.props.addFail(cId, eId, fail)
      this.setState({ ...this.state, newFails: [], fail: null })
    }
  }

  createFail = (cId, eId, fails, name) => {
    this.props.createFail(cId, eId, name)
    this.setState({ ...this.state, newFails: [], newFailName: null })
  }

  delFail = (cId, eId, fId) => {
    const { fail } = this.state
    if (fId !== null) {
      this.props.delFail(cId, eId, fId)
    }
  }

  render() {
    const { result: { data, total }, fails, aktCategories } = this.props
    const {
      catName,
      catDesc,
      eqName,
      eqDesc,
      skip,
      limit,
      editName,
      editDesc,
      fail,
      newFails,
      showAdd,
      filter1,
      filter2,
      catAkt,
      newFailName
    } = this.state

    return (
      <div>
        <div className="add-item card">
          <div className="layout vertical">
            <div className="layout horizontal">
              <InputField
                label="Название категории"
                className="flex-2"
                value={filter1}
                onChange={event => {
                  this.setState({ ...this.state, filter1: event.currentTarget.value })
                }}
                onEnter={() => {
                  const { limit, filter1, filter2 } = this.state
                  this.setState({ ...this.state, skip: 0 })
                  this.props.getCategories(0, limit, filter1, filter2)
                }}
                style={{ maxWidth: '420px', minWidth: '320px' }}
              />

              <InputField
                label="Название оборудования"
                className="flex-2 margin-left"
                value={filter2}
                onChange={event => {
                  this.setState({ ...this.state, filter2: event.currentTarget.value })
                }}
                onEnter={() => {
                  const { limit, filter1, filter2 } = this.state
                  this.setState({ ...this.state, skip: 0 })
                  this.props.getCategories(0, limit, filter1, filter2)
                }}
                style={{ maxWidth: '420px', minWidth: '320px' }}
              />
              <div className="flex-1" />

              <BlueButton
                label="Применить фильтр"
                className="margin-left"
                onClick={() => {
                  const { limit, filter1, filter2 } = this.state
                  this.setState({ ...this.state, skip: 0 })
                  this.props.getCategories(0, limit, filter1, filter2)
                }}
              />
              <BlueButton
                label="Сбросить"
                className="margin-left"
                onClick={() => {
                  const { limit } = this.state
                  this.setState({ ...this.state, skip: 0, filter1: null, filter2: null })
                  this.props.getCategories(0, limit)
                }}
              />
            </div>
          </div>
        </div>

        <div className="table-layout card  layout vertical margin-top">
          <div className="layout horizontal margin-bottom">
            <GreenButton
              label="Добавить категорию"
              onClick={() => {
                this.props.hideEditors()
                this.setState({ ...this.state, showAdd: true })
              }}
            />
          </div>

          <Table>
            <HeaderRow>
              <HeaderCell className="first header-cell" />
              <HeaderCell>Категория</HeaderCell>
              <HeaderCell>Описание</HeaderCell>
              <HeaderCell className="last header-cell" />
              <HeaderCell className="last header-cell" />
            </HeaderRow>

            {showAdd && (
              <div
                className="layout vertical padding"
                style={{
                  borderLeft: '1px solid #666666',
                  borderTop: '1px solid #666666',
                  borderBottom: '1px solid #666666',
                  borderRight: '1px solid #666666'
                }}
              >
                <div className="layout horizontal margin-bottom">
                  <InputField label="Название категории" onChange={this.setCatName} value={catName} className="flex" />
                  <span
                    className="icon-save path1 path2 path3 margin-left"
                    onClick={this.addCat}
                    style={{ fontSize: '30px' }}
                  >
                    <span className="path1" />
                    <span className="path2" />
                    <span className="path3" />
                  </span>
                  <div className="flex" />
                </div>
                <TextareaField label="Описание категории" onChange={this.setCatNDesc} value={catDesc} className="" />
              </div>
            )}

            {data &&
              data.map((row, index) => (
                <div key={`ct_${index}${row.id}`}>
                  <Row className={row.expanded || row.showEdit ? 'expanded' : ''}>
                    <Cell
                      className="first cell layout horizontal center-center"
                      onClick={() => {
                        if (!row.expanded) {
                          this.props.expandRow(row.id, filter2)
                        } else {
                          this.props.unexpandRow(row.id)
                        }
                      }}
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
                      {row.description}
                    </Cell>
                    <Cell className="last cell layout horizontal center-center">
                      <div
                        className="icon-add"
                        onClick={() => {
                          this.setState({
                            ...this.state,
                            eqName: null,
                            eqDesc: null,
                            showAdd: false,
                            catAkt: null
                          })
                          if (!row.expanded) {
                            this.props.expandRow(row.id, filter2, true)
                          } else {
                            this.props.showAdd(row.id)
                          }
                        }}
                      />
                    </Cell>
                    <Cell className="last cell layout horizontal center-center">
                      <div
                        className="icon-delete"
                        onClick={() => {
                          this.props.delCat(row.id)
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
                      <div className="layout horizontal center">
                        <InputField
                          className="margin-top margin-right margin-left margin-bottom"
                          label="Наименование категории"
                          onChange={this.setEditName}
                          value={editName === null ? row.name : editName}
                        />

                        <span
                          className="icon-save path1 path2 path3 margin-top margin-right margin-bottom"
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
                      <TextareaField
                        className="margin-left margin-right margin-bottom"
                        label="Описание категории"
                        onChange={this.setEditDesc}
                        value={editDesc === null ? row.description : editDesc}
                      />
                    </div>
                  )}

                  {row.expanded && (
                    <Table className="inner-table layout vertical">
                      {/* <HeaderRow>
                        <HeaderCell>Оборудование</HeaderCell>
                      </HeaderRow> */}

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
                          <Tabs
                            tabs={[{ label: 'ОСНОВНЫЕ' }, { label: 'ТИПОВЫЕ НЕИСПРАВНОСТИ' }]}
                            selectedTab={this.state.selectedTab}
                            onTabChanged={id => {
                              this.setState({ ...this.state, selectedTab: id })
                            }}
                            className=""
                          />

                          {this.state.selectedTab === 0 && (
                            <div className="layout vertical">
                              <div className="layout horizontal margin-top margin-left margin-right">
                                <InputField label="Название оборудования" onChange={this.setEqName} value={eqName} />
                                <DropdownField
                                  label="Категория акта приёмщиков"
                                  value={catAkt}
                                  onChange={this.setCatAkt}
                                  className="margin-left"
                                  showNull
                                  list={aktCategories}
                                  maxWidth={'200px'}
                                />
                                <span
                                  className="icon-save path1 path2 path3 margin-left"
                                  onClick={() => {
                                    this.addEq(row.id)
                                  }}
                                  style={{ fontSize: '30px' }}
                                >
                                  <span className="path1" />
                                  <span className="path2" />
                                  <span className="path3" />
                                </span>
                              </div>
                              <TextareaField
                                label="Описание оборудования"
                                onChange={this.setEqDesc}
                                value={eqDesc}
                                className="margin"
                              />
                            </div>
                          )}

                          {this.state.selectedTab === 1 && (
                            <div className="layout vertical">
                              <div className="layout horizontal margin">
                                <DropdownField
                                  label="Неисправность"
                                  onChange={this.setFail}
                                  value={fail}
                                  list={fails}
                                  showNull
                                />
                                <span
                                  className="icon-add margin-left"
                                  onClick={() => {
                                    this.addNewFail()
                                  }}
                                  style={{ fontSize: '30px' }}
                                />
                                <span
                                  className="icon-save path1 path2 path3 margin-left"
                                  onClick={() => {
                                    this.addEq(row.id)
                                  }}
                                  style={{ fontSize: '30px' }}
                                >
                                  <span className="path1" />
                                  <span className="path2" />
                                  <span className="path3" />
                                </span>
                              </div>

                              {newFails.length > 0 && (
                                <Table className="table layout vertical margin-bottom margin-left margin-right">
                                  {newFails &&
                                    newFails.map(newFail => (
                                      <div>
                                        <Row>
                                          <Cell className="cell flex layout horizontal center">
                                            {this.getFailLabel(newFail)}
                                          </Cell>
                                          <Cell className="last cell layout horizontal center-center">
                                            <span
                                              className="icon-delete"
                                              onClick={() => {
                                                this.delNewFail(newFail)
                                              }}
                                            />
                                          </Cell>
                                        </Row>
                                      </div>
                                    ))}
                                </Table>
                              )}
                            </div>
                          )}
                        </div>
                      )}

                      {row.equipments &&
                        row.equipments.map(innerRow => (
                          <div key={`eq_${index}${innerRow.id}`}>
                            <Row className={innerRow.expanded || innerRow.showEdit ? 'expanded' : ''}>
                              <Cell
                                onClick={() => {
                                  this.onInnerCellClick(row, innerRow)
                                }}
                              >
                                {innerRow.name}
                              </Cell>
                              <Cell className="last cell layout horizontal center-center">
                                <div
                                  className={(true && 'icon-delete') || 'icon-delete-1'}
                                  onClick={() => {
                                    true && this.props.delEq(innerRow.id, row.id, filter2)
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
                                  tabs={[{ label: 'ОСНОВНЫЕ' }, { label: 'ТИПОВЫЕ НЕИСПРАВНОСТИ' }]}
                                  selectedTab={this.state.selectedTab}
                                  onTabChanged={id => {
                                    this.setState({ ...this.state, selectedTab: id })
                                  }}
                                  className=""
                                />

                                {this.state.selectedTab === 0 && (
                                  <div className="layout vertical">
                                    <div className="layout horizontal margin-top margin-left margin-right">
                                      <InputField
                                        label="Название оборудования"
                                        onChange={this.setEqName}
                                        value={eqName === null ? innerRow.name : eqName}
                                        width={'320px'}
                                      />
                                      <DropdownField
                                        label="Категория акта приёмщиков"
                                        value={catAkt}
                                        onChange={this.setCatAkt}
                                        className="margin-left"
                                        showNull
                                        list={aktCategories}
                                        maxWidth={'200px'}
                                      />
                                      <span
                                        className="icon-save path1 path2 path3 margin-left"
                                        onClick={() => {
                                          this.editEq(row.id, innerRow)
                                        }}
                                        style={{ fontSize: '30px' }}
                                      >
                                        <span className="path1" />
                                        <span className="path2" />
                                        <span className="path3" />
                                      </span>
                                    </div>
                                    <TextareaField
                                      label="Описание оборудования"
                                      onChange={this.setEqDesc}
                                      value={eqDesc === null ? innerRow.description : eqDesc}
                                      className="margin"
                                    />
                                  </div>
                                )}

                                {this.state.selectedTab === 1 && (
                                  <div className="layout vertical">
                                    <div className="layout horizontal margin">
                                      <DropdownField
                                        label="Неисправность"
                                        onChange={this.setFail}
                                        value={fail}
                                        list={fails}
                                        showNull
                                        maxWidth={'200px'}
                                      />
                                      <span
                                        className={`icon-add${fail ? '' : '-2'} margin-left`}
                                        onClick={() => {
                                          fail && this.addFail(row.id, innerRow.id, innerRow.fails)
                                        }}
                                        style={{ fontSize: '30px' }}
                                      />
                                      <InputField
                                        className="margin-left"
                                        label="Добавить новую неисправность"
                                        onChange={e => {
                                          const newFailName = e.target.value
                                          this.setState({ newFailName })
                                        }}
                                        value={newFailName}
                                      />
                                      <span
                                        className={`icon-add${newFailName ? '' : '-2'} margin-left`}
                                        onClick={() => {
                                          newFailName &&
                                            this.createFail(row.id, innerRow.id, innerRow.fails, newFailName)
                                        }}
                                        style={{ fontSize: '30px' }}
                                      />
                                      <span
                                        className="icon-save path1 path2 path3 margin-left"
                                        onClick={() => {
                                          this.editEq(row.id, innerRow)
                                        }}
                                        style={{ fontSize: '30px' }}
                                      >
                                        <span className="path1" />
                                        <span className="path2" />
                                        <span className="path3" />
                                      </span>
                                    </div>

                                    {innerRow.fails.length > 0 && (
                                      <Table className="table layout vertical margin-bottom margin-right">
                                        {innerRow.fails &&
                                          innerRow.fails.map(_fail => (
                                            <div>
                                              {_fail.id > 0 && (
                                                <Row>
                                                  <Cell className="cell flex layout horizontal center">
                                                    {this.getFailLabel(_fail.id)}
                                                  </Cell>
                                                  <Cell className="last cell layout horizontal center-center">
                                                    <span
                                                      className="icon-delete"
                                                      onClick={() => {
                                                        this.delFail(row.id, innerRow.id, _fail.id)
                                                      }}
                                                    />
                                                  </Cell>
                                                </Row>
                                              )}
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

const provider = provide((state: ApplicationState) => state.equipment, EquipmentStore.actionCreators)

type Props = typeof provider.allProps

export default provider.connect(Equipment)
