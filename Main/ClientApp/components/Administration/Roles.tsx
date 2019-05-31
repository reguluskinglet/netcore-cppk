import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from '../../store/RolesStore'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, TextareaField, DropdownField } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'

class Roles extends React.Component<Props, any> {
  defaultPerm() {
    return {
      0: 0,
      1: 0,
      2: 0,
      3: 0,
      4: 0,
      5: 0,
      6: 0,
      7: 0,
      8: 0,
      9: 0,
      10: 0,
      11: 0,
      12: 0,
      13: 0,
      14: 0,
      15: 0,
      16: 0,
      17: 0,
      18: 0,
      19: 0,
      20: 0,
      21: 0,
      22: 0,
      23: 0,
      24: 0,
      25: 0,
      26: 0,
      27: 0,
      28: 0,
      29: 0,
      30: 0,
      31: 0
    }
  }

  componentWillMount() {
    this.setState({
      skip: 0,
      limit: 10,
      loading: false,
      name: '',
      type: null,
      reload: false,
      editName: null,
      editType: null,
      showAdd: false,
      filter1: null,
      filter2: null,
      perm: this.defaultPerm()
    })
    this.props.get(0, 10)
    this.props.getLinks()
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
    const editTypeS = event.currentTarget.value
    const editType = parseInt(editTypeS)
    this.setState({ editType })
  }

  add = () => {
    const { name, perm } = this.state
    if (name) {
      this.props.add({ role: { name }, permissionsArray: perm })
      this.setState({
        ...this.state,
        name: null,
        perm: this.defaultPerm(),
        showAdd: false
      })
    }
  }

  edit = row => {
    const { editName, editType, perm } = this.state
    if (row.role.name || editName) {
      this.props.add({
        role: { name: (editName === null && row.role.name) || editName, id: row.role.id },
        permissionsArray: { ...row.permissionsArray, ...perm }
      })
      this.setState({
        ...this.state,
        editName: null,
        editType: null,
        perm: this.defaultPerm()
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
      this.setState({ ...this.state, showAdd: false, perm: row.permissionsArray })
      this.props.showEdit(row.role.id)
    } else {
      this.props.hideEdit(row.role.id)
      this.setState({
        ...this.state,
        editName: null,
        editType: null,
        perm: this.defaultPerm()
      })
    }
  }

  render() {
    const { result = { data: [], total: 0 }, permissions } = this.props
    const { data, total } = result
    const { name, type, skip, limit, editName, editType, showAdd, filter1, filter2, perm } = this.state
    const permissionsArray = []
    for (const key in permissions) {
      if (permissions.hasOwnProperty(key)) {
        const element = permissions[key]
        permissionsArray.push(element)
      }
    }
    return (
      <div>
        <div className="table-layout card  layout vertical margin-top">
          <div className="layout horizontal  margin-bottom">
            <GreenButton
              label="Добавить роль"
              onClick={() => {
                this.props.hideEditors()
                this.setState({ ...this.state, showAdd: true })
              }}
            />
          </div>

          <Table>
            <HeaderRow>
              <HeaderCell>Название роли</HeaderCell>
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
                <div className="layout horizontal center">
                  <InputField label="Название роли" onChange={this.setName} value={name} className="margin-left" />

                  <span
                    className="icon-save path1 path2 path3 margin-left margin-top margin-right margin-bottom"
                    onClick={this.add}
                    style={{ fontSize: '30px' }}
                  >
                    <span className="path1" />
                    <span className="path2" />
                    <span className="path3" />
                  </span>
                </div>

                <div
                  className="layout vertical wrap"
                  style={{
                    maxHeight: '250px',
                    overflowX: 'auto',
                    overflowY: 'hidden'
                  }}
                >
                  {permissionsArray &&
                    permissionsArray.map((p, index) => {
                      return index !== 1 && index !== 9 && index !== 10 && index !== 11 && index !== 12 && index !== 13 && index !== 14 && index !== 15 && <InputField
                        key={index}
                        className="margin-left"
                        rightLabel
                        type="checkbox"
                        label={p}
                        value={!!perm[index]}
                        onChange={event => {
                          this.setState({
                            ...this.state,
                            perm: { ...perm, [index]: event.currentTarget.checked ? 1 : 0 }
                          })
                        }}
                      />
                    }
                    )}
                </div>
              </div>
            )}

            {data &&
              data.map((row, index) => (
                <div key={`ct_${index}${row.role.id}`}>
                  <Row className={row.expanded || row.showEdit ? 'expanded' : ''}>
                    <Cell
                      onClick={() => {
                        this.onCellClick(row)
                      }}
                    >
                      {row.role.name}
                    </Cell>
                    <Cell className="last cell layout horizontal center-center">
                      <div
                        className="icon-delete"
                        onClick={() => {
                          this.props.del(row.role.id)
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
                          label="Название роли"
                          onChange={this.setEditName}
                          value={editName === null ? row.role.name : editName}
                          className="margin-left"
                        />

                        <span
                          className="icon-save path1 path2 path3 margin-left margin-top margin-right margin-bottom"
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

                      <div
                        className="layout vertical wrap"
                        style={{
                          maxHeight: '250px',
                          overflowX: 'auto',
                          overflowY: 'hidden'
                        }}
                      >
                        {permissionsArray &&
                          permissionsArray.map((p, index) => (
                            index !== 1 && index !== 9 && index !== 10 && index !== 11 && index !== 12 && index !== 13 && index !== 14 && index !== 15 && <InputField
                              key={index}
                              className="margin-left"
                              rightLabel
                              type="checkbox"
                              label={p}
                              value={!!perm[index]}
                              onChange={event => {
                                this.setState({
                                  ...this.state,
                                  perm: { ...perm, [index]: event.currentTarget.checked ? 1 : 0 }
                                })
                              }}
                            />
                          ))}
                      </div>
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

const provider = provide((state: ApplicationState) => state.roles, Store.actionCreators)

type Props = typeof provider.allProps

export default provider.connect(Roles)
