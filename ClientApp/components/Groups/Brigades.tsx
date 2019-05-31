import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from '../../store/BrigadesStore'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, DropdownField } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'

export const types = [
    { label: 'Локомотивная бригада', value: 0 },
    { label: 'Бригада депо', value: 1 },
    { label: 'Приемщики', value: 2 }
]
export const typeLabels = ['Локомотивная бригада', 'Бригада депо', 'Приемщики']

interface State {
    skip: number
    limit: number
    loading: boolean
    name: string
    type: number
    reload: boolean
    //userId: number
    //eqDesc: string
    editName: string
    editType: number
    //editNum: string
    //innerEditName: string
    //innerEditDesc: string
    selectedTab: number
    showAdd: boolean
    filter1: string
    filter2: number
    //fio: string
    //dol: string
    //num: string
}

class Brigades extends React.Component<Props, State> {
    componentWillMount() {
        this.setState({
            skip: 0,
            limit: 10,
            loading: false,
            name: null,
            type: null,
            //userId: null,
            //eqDesc: null,
            reload: false,
            editName: null,
            editType: null,
            //editNum: null,
            //innerEditName: null,
            //innerEditDesc: null,
            selectedTab: 0,
            showAdd: false,
            filter1: null,
            filter2: null,
            //fio: null,
            //dol: null,
            //num: null
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

    setType = event => {
        const typeS = event.currentTarget.value
        const type = parseInt(typeS)
        this.setState({ type })
    }

    add = () => {
        const { name, type } = this.state
        if (name) {
            this.props.addCat({ Name: name, BrigadeType: type })
            this.setState({
                ...this.state,
                name: null,
                type: null,
                showAdd: null
            })
        }
    }

    /*setUserId = event => {
        const userIdS = event.currentTarget.value
        const userId = parseInt(userIdS)
        this.setState({ userId })
    }*/

    /*setEqDesc = event => {
        const eqDesc = event.currentTarget.value
        this.setState({ eqDesc })
    }*/

    /*addUser = (bId, id?) => {
        const { fio, dol, num } = this.state
        if (fio !== null) {
            this.props.addEq({
                name: fio,
                PersonPosition: dol,
                PersonNumber: num,
                BrigadeId: bId,
                id: id || undefined
            })
        }
        this.setState({
            ...this.state,
            userId: null,
            eqDesc: null,
            fio: null,
            dol: null,
            num: null
        })
    }*/

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
                editType: null,
                //fio: null,
                //dol: null,
                //num: null
            })
        }
    }

    /*onInnerCellClick = (row, innerRow) => {
        if (!innerRow.showEdit) {
            this.setState({
                ...this.state,
                showAdd: false,
                selectedTab: 0,
                fio: innerRow.name,
                dol: innerRow.personPosition,
                num: innerRow.personNumber
            })
            this.props.showInnerEdit(row.id, innerRow.id)
        } else {
            this.props.hideInnerEdit(row.id, innerRow.id)
            this.setState({
                ...this.state,
                innerEditName: null,
                innerEditDesc: null,
                fio: null,
                dol: null,
                num: null
            })
        }
    }*/

    edit = row => {
        const { editName, editType } = this.state
        if (row.name || editName) {
            this.props.addCat({
                Name: editName === null ? row.name : editName,
                BrigadeType: editType === null ? row.brigadeType : editType,
                Id: row.id
            })
            this.setState({
                ...this.state,
                editName: null,
                editType: null
            })
        }
    }

    setEditName = event => {
        const editName = event.currentTarget.value
        this.setState({ editName })
    }

    setEditType = event => {
        const editTypeS = event.currentTarget.value
        const editType = parseInt(editTypeS)
        this.setState({ editType })
    }

    /*setFio = event => {
        const fio = event.currentTarget.value
        this.setState({ fio })
    }
    setDol = event => {
        const dol = event.currentTarget.value
        this.setState({ dol })
    }
    setNum = event => {
        const num = event.currentTarget.value
        this.setState({ num })
    }*/

    render() {
        const data = this.props.result.data
        const total = this.props.result.total
        //const users = this.props.users
        const {
            name,
            type,
            //userId,
            //eqDesc,
            skip,
            limit,
            editName,
            editType,
            showAdd,
            filter1,
            filter2,
            //fio,
            //dol,
            //num
        } = this.state
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
                                    this.props.getCategories(0, limit, filter1, filter2)
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
                    <div className="margin-bottom layout horizontal ">
                        <GreenButton
                            label="Добавить бригаду"
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
                                <InputField label="Наименование" onChange={this.setName} value={name} className="flex margin-left" />
                                <DropdownField
                                    label="Тип"
                                    onChange={this.setType}
                                    value={type}
                                    className="flex margin-left"
                                    list={types}
                                    showNull
                                />
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
                                            {row.name}
                                        </Cell>
                                        <Cell
                                            onClick={() => {
                                                this.onCellClick(row)
                                            }}
                                        >
                                            {typeLabels[row.brigadeType]}
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
                                                    label="Наименование"
                                                    onChange={this.setEditName}
                                                    value={editName === null ? row.name : editName}
                                                    className="flex  margin-left"
                                                />
                                                <DropdownField
                                                    label="Тип"
                                                    onChange={this.setEditType}
                                                    value={editType === null ? row.brigadeType : editType}
                                                    className="flex margin-left"
                                                    list={types}
                                                    showNull
                                                />
                                                <span
                                                    className="icon-save path1 path2 path3 margin-top margin-right margin-bottom  margin-left"
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

const provider = provide((state: ApplicationState) => state.brigades, Store.actionCreators)

type Props = typeof provider.allProps

export default provider.connect(Brigades)
