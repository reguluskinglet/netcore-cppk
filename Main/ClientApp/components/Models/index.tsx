import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from '../../store/ModelsStore'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, TextareaField, DropdownField } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'
import { Redirect } from 'react-router-dom'

export const types = [
  { label: 'Головной вагон', value: 0 },
  { label: 'Моторный вагон', value: 1 },
  { label: 'Прицепной вагон', value: 2 }
]
export const typeLabels = ['Головной вагон', 'Моторный вагон', 'Прицепной вагон']

interface State {
  skip: number
  limit: number
  loading: boolean
  reload: boolean
  name: string
  type: number
  editName: string
  editType: number
  redirect: string
  showAdd: boolean
  filter1: string
  filter2: number
}

class Models extends React.Component<Props, State> {
  componentWillMount() {
    this.setState({
      skip: 0,
      limit: 10,
      loading: false,
      name: null,
      type: null,
      reload: false,
      editName: null,
      editType: null,
      redirect: null,
      showAdd: false,
      filter1: null,
      filter2: null
    })
    this.props.get(0, 10)
  }

  componentWillReceiveProps(nextProps) {
    const { skip, limit } = this.state
    nextProps.reload && this.props.get(skip, limit)
  }

  setName = event => {
    const name = event.currentTarget.value
    this.setState({ name })
  }

  setDesc = event => {
    const type = event.currentTarget.value
    this.setState({ type })
  }

  setEditName = event => {
    const editName = event.currentTarget.value
    this.setState({ editName })
  }

  setEditDesc = event => {
    const editType = event.currentTarget.value
    this.setState({ editType })
  }

  add = () => {
    const { name, type } = this.state
    if (name && type !== null) {
      this.props.add({ Name: name, modelType: type })
      this.setState({
        ...this.state,
        name: null,
        type: null,
        showAdd: false
      })
    }
  }

  edit = row => {
    const { editName, editType } = this.state
    if ((row.name || editName) && (row.modelType >= 0 || editType)) {
      this.props.add({
        Name: editName === null ? row.name : editName,
        modelType: editType === null ? row.modelType : editType,
        Id: row.id
      })
      this.setState({
        ...this.state,
        editName: null,
        editType: null
      })
    }
  }

  setPage = (skip, l) => {
    const { limit, filter1, filter2 } = this.state
    this.props.get(skip, (l > 0 && l) || limit, filter1, filter2)
    this.setState({ skip })
  }

  onCellClick = row => {
    if (!row.showEdit) {
      this.setState({ ...this.state, showAdd: false })
      this.props.showEdit(row.id)
    } else {
      this.props.hideEdit(row.id)
      this.setState({
        ...this.state,
        editName: null,
        editType: null
      })
    }
  }

  cloneModel = row => () => {
    const data = { ...row, name: row.name + '( копия )' }
    // console.log('data ', data)
    this.props.clone(data)
  }

  render() {
    const { result = { data: [], total: 0 } } = this.props
    const { data, total } = result
    const { name, type, skip, limit, editName, editType, redirect, showAdd, filter1, filter2 } = this.state
    if (redirect) return <Redirect to={redirect} push={true} />
    else
      return (
        <div>
          <div className="add-item card">
            <div className="layout vertical">
              <div className=" layout horizontal">
                <InputField
                  label="Наименование"
                  className="flex"
                  value={filter1}
                  onChange={event => {
                    this.setState({ ...this.state, filter1: event.currentTarget.value })
                  }}
                  onEnter={() => {
                    const { limit, filter1, filter2 } = this.state
                    this.setState({ ...this.state, skip: 0 })
                    this.props.get(0, limit, filter1, filter2)
                  }}
                />
                <DropdownField
                  label="Тип"
                  className="flex margin-left"
                  list={types}
                  showNull
                  value={filter2}
                  onChange={event => {
                    this.setState({ ...this.state, filter2: event.currentTarget.value })
                  }}
                />

                <BlueButton
                  label="Применить фильтр"
                  onClick={() => {
                    const { limit, filter1, filter2 } = this.state
                    this.setState({ ...this.state, skip: 0 })
                    this.props.get(0, limit, filter1, filter2)
                  }}
                  className="margin-left"
                />
                <BlueButton
                  label="Сбросить"
                  onClick={() => {
                    const { limit } = this.state
                    this.setState({ ...this.state, skip: 0, filter1: null, filter2: null })
                    this.props.get(0, limit)
                  }}
                  className="margin-left"
                />
              </div>
            </div>
          </div>

          <div className="table-layout card  layout vertical margin-top">
            <div className="layout horizontal margin-bottom">
              <GreenButton
                label="Добавить модель"
                onClick={() => {
                  this.props.hideEditors()
                  this.setState({ ...this.state, showAdd: true })
                }}
              />
            </div>

            <Table>
              <HeaderRow>
                <HeaderCell>Наименование</HeaderCell>
                <HeaderCell>Тип</HeaderCell>
                <HeaderCell className="last header-cell" />
                <HeaderCell className="last header-cell" />
                <HeaderCell className="last header-cell" />
              </HeaderRow>

              {showAdd && (
                <div
                  className="layout horizontal center"
                  style={{
                    borderLeft: '1px solid #666666',
                    borderTop: '1px solid #666666',
                    borderBottom: '1px solid #666666',
                    borderRight: '1px solid #666666'
                  }}
                >
                  <InputField
                    className="margin-top margin-right margin-left margin-bottom flex"
                    label="Наименование"
                    onChange={this.setName}
                    value={name}
                  />
                  <DropdownField
                    label="Тип"
                    onChange={this.setDesc}
                    value={type}
                    className="margin-top margin-right margin-bottom flex"
                    list={types}
                    showNull
                  />
                  <span
                    className="icon-save path1 path2 path3 margin-top margin-right margin-bottom"
                    onClick={this.add}
                    style={{ fontSize: '30px' }}
                  >
                    <span className="path1" />
                    <span className="path2" />
                    <span className="path3" />
                  </span>
                </div>
              )}

              {data &&
                data.map((row, index) => (
                  <div key={`ct_${index}${row.id}`}>
                    <Row className={row.expanded || row.showEdit ? 'expanded' : ''}>
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
                        {typeLabels[row.modelType]}
                      </Cell>
                      <Cell className="last cell layout horizontal center-center">
                        <div
                          className="icon-open"
                          style={{ fontSize: '18px' }}
                          onClick={() => {
                            this.setState({
                              ...this.state,
                              redirect: `/dicts/models/${row.id}/locations`,
                              showAdd: false
                            })
                          }}
                        />
                      </Cell>
                      <Cell className="last cell layout horizontal center-center">
                        <div className="icon-copy" onClick={this.cloneModel(row)} />
                      </Cell>
                      <Cell className="last cell layout horizontal center-center">
                        <div
                          className="icon-delete"
                          onClick={() => {
                            this.props.del(row.id)
                          }}
                        />
                      </Cell>
                    </Row>

                    {row.showEdit && (
                      <div
                        className="layout horizontal center"
                        style={{
                          borderLeft: '1px solid #666666',
                          borderTop: '1px solid #666666',
                          borderBottom: '1px solid #666666',
                          borderRight: '1px solid #666666'
                        }}
                      >
                        <InputField
                          className="margin-top margin-right margin-left margin-bottom flex"
                          label="Наименование"
                          onChange={this.setEditName}
                          value={editName === null ? row.name : editName}
                        />
                        <DropdownField
                          label="Тип"
                          onChange={this.setEditDesc}
                          value={editType === null ? row.modelType : editType}
                          className="margin-top margin-right margin-bottom flex"
                          list={types}
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

const provider = provide((state: ApplicationState) => state.models, Store.actionCreators)

type Props = typeof provider.allProps

export default provider.connect(Models)
