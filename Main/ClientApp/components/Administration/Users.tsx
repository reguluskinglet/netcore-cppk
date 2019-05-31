import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from '../../store/UsersStore'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, TextareaField, DropdownField } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'

class Users extends React.Component<Props, any> {
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
            sortParams: null
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
        const { name, login, pass, roleId, sotrId } = this.state
        if (login && pass && roleId) {
            this.props.add({
                login: login,
                passwordHash: pass,
                isBlocked: false,
                roleId: roleId,
                id: sotrId || undefined
            })

            this.props.getLinks()

            this.setState({
                ...this.state,
                name: null,
                login: null,
                pass: null,
                hash: null,
                roleId: null,
                sotrId: null,
                showAdd: false
            })
        }
    }

    edit = row => {
        const { name, login, pass, hash, roleId } = this.state
        if (login && roleId) {
            this.props.add({
                // ...row,
                login: login,
                passwordHash: pass || undefined,
                roleId: roleId,
                id: row.id
            })

            this.props.getLinks()

            this.setState({
                ...this.state,
                name: null,
                login: null,
                pass: null,
                hash: null,
                roleId: null,
                sotrId: null,
                showAdd: false
            })
        }
    }

    setPage = (skip, l) => {
        const { limit, filter1, sortParams } = this.state
        this.props.get(skip, (l > 0 && l) || limit, filter1, sortParams)
        this.setState({ skip })
    }

    onCellClick = row => {
        if (!row.showEdit) {
            this.setState({
                ...this.state,
                showAdd: false,
                name: row.name,
                login: row.login,
                pass: null,
                hash: row.passwordHash,
                roleId: row.roleId
            })
            this.props.showEdit(row.id)
        } else {
            this.props.hideEdit(row.id)
            this.setState({
                ...this.state,
                name: null,
                login: null,
                pass: null,
                hash: null,
                roleId: null,
                sotrId: null,
                showAdd: false
            })
        }
    }

    getRoleName = id => {
        const role = this.props.roles.find(r => r.value === id)
        return (role && role.label) || ''
    }

    render() {
        const { result = { data: [], total: 0, sortOptions: undefined  }, roles, sotr} = this.props
        const { data, total, sortOptions = { column: '', direction: 0 } } = result
        const { column, direction } = sortOptions
        const { name, skip, limit, showAdd, login, pass, hash, roleId, sotrId, filter1, sortParams } = this.state
        const permissionsArray = []
        const labelWidth = '60px'

        return (
            <div>
                <div className="add-item card">
                    <div className="layout vertical">
                        <div className=" layout horizontal">                            
                            <InputField
                                label="Пользователь"
                                className="flex"
                                value={filter1}
                                onChange={event => {
                                    this.setState({ ...this.state, filter1: event.currentTarget.value })
                                }}
                                onEnter={() => {
                                    const { limit, filter1 } = this.state
                                    this.setState({ ...this.state, skip: 0 })
                                    this.props.get(0, limit, filter1, sortParams)
                                }}
                            />

                            <BlueButton
                                label="Применить фильтр"
                                onClick={() => {
                                    const { limit, filter1 } = this.state
                                    this.setState({ ...this.state, skip: 0 })
                                    this.props.get(0, limit, filter1, sortParams)
                                }}
                                className="margin-left"
                            />
                            <BlueButton
                                label="Сбросить"
                                onClick={() => {
                                    const { limit } = this.state
                                    this.setState({ ...this.state, skip: 0, filter1: null, sortParams })
                                    this.props.get(0, limit)
                                }}
                                className="margin-left"
                            />
                        </div>
                    </div>
                </div>

                <div className="table-layout card  layout vertical margin-top">
                    <div className="margin-bottom layout horizontal ">
                        <GreenButton
                            label="Добавить пользователя"
                            onClick={() => {
                                this.props.hideEditors()
                                this.setState({ ...this.state, showAdd: true })
                            }}
                        />
                    </div>

                    <Table>
                        <HeaderRow>
                            <HeaderCell
                                column={'login'}
                                className="header-cell flex layout horizontal center sortable"
                                sortDirection={column == 'login' ? direction : null}
                                onSortColumn={(sortOptions) => {
                                    const { skip, limit, filter1 } = this.state
                                    this.props.get(skip, limit, filter1, sortOptions)
                                    this.setState({ ...this.state, sortParams: sortOptions })
                                }}
                            >
                                Логин
                            </HeaderCell>
                            <HeaderCell
                                column={'name'}
                                className="header-cell flex layout horizontal center sortable"
                                sortDirection={column == 'name' ? direction : null}
                                onSortColumn={(sortOptions) => {
                                    const { skip, limit, filter1 } = this.state
                                    this.setState({ ...this.state, sortParams: sortOptions })
                                    this.props.get(skip, limit, filter1, sortOptions)
                                }}
                            >
                                ФИО сотрудника
                            </HeaderCell>
                            <HeaderCell
                                column={'roleId'}
                                className="header-cell flex layout horizontal center sortable"
                                sortDirection={column == 'roleId' ? direction : null}
                                onSortColumn={(sortOptions) => {
                                    const { skip, limit, filter1 } = this.state
                                    this.setState({ ...this.state, sortParams: sortOptions })
                                    this.props.get(skip, limit, filter1, sortOptions)
                                }}
                            >
                                Роль
                            </HeaderCell>
                            <HeaderCell className="last header-cell" />
                        </HeaderRow>

                        {showAdd && (
                            <div
                                className="layout horizontal center"
                                style={{
                                    borderLeft: '1px solid #666666',
                                    borderTop: '1px solid #666666', borderBottom: '1px solid #666666',
                                    borderRight: '1px solid #666666'
                                }}
                            >
                                <div className="flex layout vertical margin-top margin-bottom">
                                    <InputField
                                        labelWidth={labelWidth}
                                        label="Логин"
                                        onChange={event => {
                                            this.setState({ ...this.state, login: event.currentTarget.value })
                                        }}
                                        value={login}
                                        className="margin-left"
                                        autoComplete="new-password"
                                    />

                                    <InputField
                                        labelWidth={labelWidth}
                                        label="Пароль"
                                        type="password"
                                        onChange={event => {
                                            this.setState({
                                                ...this.state,
                                                pass: event.currentTarget.value,
                                                hash: event.currentTarget.value
                                            })
                                        }}
                                        value={pass}
                                        className="margin-top margin-left"
                                        autoComplete="new-password"
                                    />
                                </div>

                                <div className="flex layout vertical margin-top margin-bottom">
                                    <DropdownField
                                        labelWidth={labelWidth}
                                        label="Сотрудник"
                                        onChange={event => {
                                            const value = parseInt(event.currentTarget.value)
                                            this.setState({ ...this.state, sotrId: value })
                                        }}
                                        value={sotrId}
                                        list={sotr}
                                        className="margin-left"
                                        showNull
                                    />

                                    <DropdownField
                                        labelWidth={labelWidth}
                                        label="Роль"
                                        type="password"
                                        onChange={event => {
                                            const value = parseInt(event.currentTarget.value)
                                            this.setState({ ...this.state, roleId: value })
                                        }}
                                        value={roleId}
                                        list={roles}
                                        className="margin-top margin-left"
                                        showNull
                                    />
                                </div>

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
                                            {row.login}
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
                                            {this.getRoleName(row.roleId)}
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
                                            className="layout horizontal"
                                            style={{
                                                borderLeft: '1px solid #666666',
                                                borderTop: '1px solid #666666', borderBottom: '1px solid #666666',
                                                borderRight: '1px solid #666666'
                                            }}
                                        >
                                            <div className="flex layout vertical margin-top margin-bottom">
                                                <InputField
                                                    labelWidth={labelWidth}
                                                    label="Логин"
                                                    onChange={event => {
                                                        this.setState({ ...this.state, login: event.currentTarget.value })
                                                    }}
                                                    value={login}
                                                    className="margin-left"
                                                />

                                                <InputField
                                                    labelWidth={labelWidth}
                                                    label="Пароль"
                                                    type="password"
                                                    onChange={event => {
                                                        this.setState({
                                                            ...this.state,
                                                            pass: event.currentTarget.value,
                                                            hash: event.currentTarget.value
                                                        })
                                                    }}
                                                    value={pass}
                                                    className="margin-top margin-left"
                                                />
                                            </div>

                                            <div className="flex layout vertical margin-top margin-bottom">
                                                <InputField
                                                    labelWidth={labelWidth}
                                                    label="Сотрудник"
                                                    value={name}
                                                    className="margin-left"
                                                    disabled
                                                />

                                                <DropdownField
                                                    labelWidth={labelWidth}
                                                    label="Роль"
                                                    type="password"
                                                    onChange={event => {
                                                        const value = parseInt(event.currentTarget.value)
                                                        this.setState({ ...this.state, roleId: value })
                                                    }}
                                                    value={roleId}
                                                    list={roles}
                                                    className="margin-top margin-left"
                                                    showNull
                                                />
                                            </div>

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

const provider = provide((state: ApplicationState) => state.users, Store.actionCreators)

type Props = typeof provider.allProps

export default provider.connect(Users)
