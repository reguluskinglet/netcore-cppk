import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from '../../store/TemplatesStore'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, TextareaField, DropdownField } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'

export const types = [{ label: 'Санитарная', value: 1 }, { label: 'Задачи по ремонту', value: 2 }]
export const typeLabels = ['', 'Санитарная', 'Задачи по ремонту']

interface State {
  skip: number
  limit: number
  loading: boolean
  reload: boolean
  name: string
  type: number
  editName: string
  editType: number
  showAdd: boolean
  filter1: string
  filter2: number
}

class Templates extends React.Component<Props, State> {
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
      this.props.add({ Name: name, Template: type })
      this.setState({
        ...this.state,
        name: null,
        type: null,
        showAdd: null
      })
    }
  }

  edit = row => {
    const { editName, editType } = this.state
    if ((row.name || editName) && (row.template || editType)) {
      this.props.add({
        Name: editName === null ? row.name : editName,
        Template: editType === null ? row.template : editType,
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

  render() {
    const { result = { data: [], total: 0 } } = this.props
    const { data, total } = result
    const { name, type, skip, limit, editName, editType, showAdd, filter1, filter2 } = this.state
    return (
      <div>
        <div className="table-layout card  layout vertical margin-top">
          <div className="margin-bottom layout horizontal ">
            <GreenButton
              label="Добавить шаблон"
              onClick={() => {
                this.props.hideEditors()
                this.setState({ ...this.state, showAdd: true })
              }}
            />
          </div>
          <Table>
            <HeaderRow>
              <HeaderCell>Наименование</HeaderCell>
              {/* <HeaderCell>Тип</HeaderCell> */}
              <HeaderCell className="last header-cell" />
            </HeaderRow>

            {showAdd && (
              <div
                className="layout vertical"
                style={{
                  borderLeft: '1px solid #666666',
                  borderTop: '1px solid #666666', borderBottom: '1px solid #666666',
                  borderRight: '1px solid #666666'
                }}
              >
                <div className="layout horizontal center margin">
                  <InputField label="Наименование" onChange={this.setName} value={name} className="flex" />

                  <span
                    className="icon-save path1 path2 path3 margin-left"
                    onClick={this.add}
                    style={{ fontSize: '30px' }}
                  >
                    <span className="path1" />
                    <span className="path2" />
                    <span className="path3" />
                  </span>

                  <div className="flex" />
                </div>

                <TextareaField
                  label="Шаблон"
                  onChange={this.setDesc}
                  value={type}
                  className="margin-left margin-right margin-bottom"
                  height="200px"
                  rows="10"
                />
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
                    {/* <Cell
                      onClick={() => {
                        this.onCellClick(row)
                      }}
                    >
                      {typeLabels[row.faultType]}
                    </Cell> */}
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
                      className="layout vertical"
                      style={{
                        borderLeft: '1px solid #666666',
                        borderTop: '1px solid #666666', borderBottom: '1px solid #666666',
                        borderRight: '1px solid #666666'
                      }}
                    >
                      <div className="layout horizontal center">
                        <InputField
                          className="margin-top margin-right margin-left margin-bottom flex"
                          label="Наименование"
                          onChange={this.setEditName}
                          value={editName === null ? row.name : editName}
                        />
                        {/* <DropdownField
                        label="Тип"
                        onChange={this.setEditDesc}
                        value={editType === null ? row.faultType : editType}
                        className="margin-top margin-right margin-bottom flex"
                        list={types}
                      /> */}
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
                        <div className="flex" />
                      </div>

                      <TextareaField
                        label="Шаблон"
                        onChange={this.setEditDesc}
                        value={editType === null ? row.template : editType}
                        className="margin-left margin-right margin-bottom"
                        height="200px"
                        rows="10"
                      />
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

const provider = provide((state: ApplicationState) => state.templates, Store.actionCreators)

type Props = typeof provider.allProps

export default provider.connect(Templates)
