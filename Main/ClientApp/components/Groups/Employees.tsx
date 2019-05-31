import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from '../../store/EmployeesStore'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, DropdownField } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'

interface State {
    skip: number
    limit: number
    loading: boolean
    name: string,
    brigade: number,
    personNumber: string,
    personPosition: string,
    reload: boolean
    editName: string
    editBrigade: number
    editPersonNumber: string
    editPersonPosition: string
    selectedTab: number
    showAdd: boolean
    showEdit: boolean,
    filter1: string
    filter2: number
}

class Employees extends React.Component<Props, State> {
    componentWillMount() {
        this.setState({
            skip: 0,
            limit: 10,
            loading: false,
            name: null,
            brigade: null,
            personNumber: null,
            personPosition: null,
            reload: false,
            editName: null,
            editBrigade: null,
            editPersonNumber: null,
            editPersonPosition: null,
            selectedTab: 0,
            showAdd: false,
            showEdit: false,
            filter1: null,
            filter2: null,
        })
        this.props.getEmployees(0, 10)
    }

    componentWillReceiveProps(nextProps) {
        const { skip, limit } = this.state
        nextProps.reload && this.props.getEmployees(skip, limit)
    }

    setName = event => {
        const name = event.currentTarget.value
        this.setState({ name })
    }

    setBrigade = event => {
        const brigades = event.currentTarget.value
        const brigade = parseInt(brigades)
        this.setState({ brigade })
    }

    setPersonNumber = event => {
        const personNumber = event.currentTarget.value
        this.setState({ personNumber })
    }

    setPersonPosition = event => {
        const personPosition = event.currentTarget.value
        this.setState({ personPosition })
    }

    setEditName = event => {
        const editName = event.currentTarget.value
        this.setState({ editName })
    }

    setEditPersonPosition = event => {
        const editPersonPosition = event.currentTarget.value
        this.setState({ editPersonPosition })
    }

    setEditPersonNumber = event => {
        const editPersonNumber = event.currentTarget.value
        this.setState({ editPersonNumber })
    }

    setEditBrigade = event => {
        const editBrigadeS = event.currentTarget.value
        const editBrigade = parseInt(editBrigadeS)
        this.setState({ editBrigade })
    }

    addEmployee = () => {
        const { name, personPosition, personNumber, brigade } = this.state
        if (name !== null) {
            this.props.addEmployee({
                Name: name,
                PersonPosition: personPosition,
                PersonNumber: personNumber,
                BrigadeId: brigade,
            })
        }
        this.setState({
            ...this.state,
            name: null,
            personPosition: null,
            personNumber: null,
            brigade: null
        })
    }

    editEmployee = row => {
        const { editName, editPersonPosition, editPersonNumber, editBrigade } = this.state
        if (row.name || editName) {
            this.props.addEmployee({
                Id: row.id,
                Name: editName === null ? row.name : editName,
                PersonPosition: editPersonPosition === null ? row.personPosition : editPersonPosition,
                PersonNumber: editPersonNumber === null ? row.personNumber : editPersonNumber,
                BrigadeId: editBrigade === null && row.brigade ? row.brigade.id : editBrigade,
            })
            this.setState({
                ...this.state,
                editName: null,
                editBrigade: null,
                editPersonNumber: null,
                editPersonPosition: null
            })
        }
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
                editPersonNumber: null,
                editPersonPosition: null,
                editBrigade: null,
            })
        }
    }

    setPage = (skip, l) => {
        const { limit, filter1, filter2 } = this.state
        this.props.getEmployees(skip, (l > 0 && l) || limit, filter1, filter2)
        this.setState({ skip })
    }

    render() {
        const data = this.props.result.data
        const total = this.props.result.total
        const brigades = this.props.brigades
        const {
            name,
            brigade,
            personNumber,
            personPosition,
            skip,
            limit,
            editName,
            editBrigade,
            editPersonNumber,
            editPersonPosition,
            showAdd,
            showEdit,
            filter1,
            filter2,
        } = this.state
        return (
            <div>
                <div className="add-item card">
                    <div className="layout vertical">
                        <div className=" layout horizontal">
                            <InputField
                                label="ФИО"
                                className="flex"
                                value={filter1}
                                onChange={event => {
                                    this.setState({ ...this.state, filter1: event.currentTarget.value })
                                }}
                                onEnter={() => {
                                    const { limit, filter1, filter2 } = this.state
                                    this.setState({ ...this.state, skip: 0 })
                                    this.props.getEmployees(0, limit, filter1, filter2)
                                }}
                            />
                            <InputField
                                label="Номер"
                                className="flex"
                                value={filter2}
                                onChange={event => {
                                    this.setState({ ...this.state, filter2: event.currentTarget.value })
                                }}
                                onEnter={() => {
                                    const { limit, filter1, filter2 } = this.state
                                    this.setState({ ...this.state, skip: 0 })
                                    this.props.getEmployees(0, limit, filter1, filter2)
                                }}
                            />

                            <BlueButton
                                label="Применить фильтр"
                                onClick={() => {
                                    const { limit, filter1, filter2 } = this.state
                                    this.setState({ ...this.state, skip: 0 })
                                    this.props.getEmployees(0, limit, filter1, filter2)
                                }}
                                className="margin-left"
                            />
                            <BlueButton
                                label="Сбросить"
                                onClick={() => {
                                    const { limit } = this.state
                                    this.setState({ ...this.state, skip: 0, filter1: null, filter2: null })
                                    this.props.getEmployees(0, limit)
                                }}
                                className="margin-left"
                            />
                        </div>
                    </div>
                </div>

                <div className="table-layout card  layout vertical margin-top">
                    <div className="margin-bottom layout horizontal ">
                        <GreenButton
                            label="Добавить сотрудника"
                            onClick={() => {
                                this.props.hideEditors()
                                this.setState({ ...this.state, showAdd: true })
                            }}
                        />
                    </div>

                    <Table>
                        <HeaderRow>
                            <HeaderCell>ФИО</HeaderCell>
                            <HeaderCell>Табельный номер</HeaderCell>
                            <HeaderCell>Бригада</HeaderCell>
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
                                <div className="flex layout vertical margin-top margin-bottom">
                                    <InputField label="ФИО" onChange={this.setName} value={name} className="margin-left"/>
                                    <InputField
                                        label="Должность"
                                        onChange={this.setPersonPosition}
                                        value={personPosition}
                                        className="margin-top margin-left"
                                    />
                                </div>
                                <div className="flex layout vertical margin-top margin-bottom">
                                    <InputField
                                        label="Табельный номер"
                                        onChange={this.setPersonNumber}
                                        value={personNumber}
                                        className="margin-left"
                                    />
                                    <DropdownField
                                        label="Бригада"
                                        onChange={this.setBrigade}
                                        value={brigade}
                                        className="margin-top margin-left"
                                        list={brigades}
                                        showNull
                                    />                                    
                                </div>
                                <span
                                    className="icon-save path1 path2 path3 margin-left margin-top margin-right margin-bottom"
                                    onClick={() => {
                                        this.addEmployee()
                                    }}
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
                                            {row.personNumber}
                                        </Cell>
                                        <Cell
                                            onClick={() => {
                                                this.onCellClick(row)
                                            }}
                                        >
                                            {row.brigade && row.brigade.name}
                                        </Cell>
                                        <Cell className="last cell layout horizontal center-center">
                                            <div
                                                className="icon-delete"
                                                onClick={() => {
                                                    this.props.deleteEmployee(row.id)
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
                                            <div className="flex layout vertical margin-top margin-bottom">
                                                <InputField label="ФИО" onChange={this.setEditName} value={editName === null ? row.name : editName} className="margin-left"/>
                                                <InputField
                                                    label="Должность"
                                                    onChange={this.setEditPersonPosition}
                                                    value={editPersonPosition === null ? row.personPosition : editPersonPosition}
                                                    className="margin-top margin-left"
                                                />
                                            </div>
                                            <div className="flex layout vertical margin-top margin-bottom">
                                                <InputField
                                                    label="Табельный номер"
                                                    onChange={this.setEditPersonNumber}
                                                    value={editPersonNumber === null ? row.personNumber : editPersonNumber}
                                                    className="margin-left"
                                                />
                                                <DropdownField
                                                    label="Бригада"
                                                    onChange={this.setEditBrigade}
                                                    value={editBrigade === null && row.brigade ? row.brigade.id : editBrigade}
                                                    className="margin-top margin-left"
                                                    list={brigades}
                                                    showNull
                                                />                                                
                                            </div>
                                            <span
                                                className="icon-save path1 path2 path3 margin-top margin-right margin-bottom margin-left"
                                                onClick={() => {
                                                    this.editEmployee(row)
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

const provider = provide((state: ApplicationState) => state.employees, Store.actionCreators)

type Props = typeof provider.allProps

export default provider.connect(Employees)
