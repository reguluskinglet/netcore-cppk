import * as React from 'react'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'

import * as ParkingStore from './ParkingStore'

import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, TextareaField, DropdownField } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'
import { Select } from 'antd'

interface State {
    skip: number
    limit: number
    loading: boolean
    name: string
    station: number
    desc: string
    reload: boolean
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

class CLS extends React.Component<Props, State> {
    componentWillMount() {
        this.setState({
            skip: 0,
            limit: 10,
            loading: false,
            name: null,
            station: null,
            desc: null,
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
        this.props.getParkings(0, 10)
    }

    componentWillReceiveProps(nextProps) {
        const { skip, limit } = this.state
        nextProps.reload && this.props.getParkings(skip, limit)
    }

    setName = event => {
        const name = event.currentTarget.value
        this.setState({ name })
    }

    setStation = event => {
        const stations = event.currentTarget.value
        const station = parseInt(stations)
        this.setState({ station })
    }

    setDesc = event => {
        const desc = event.currentTarget.value
        this.setState({ desc })
    }

    add = () => {
        const { name, station, desc } = this.state
        if (name && station != null) {
            this.props.addParking({ Name: name, Description: desc, StantionId: station })

            this.setState({
                ...this.state,
                name: null,
                station: null,
                desc: null,
                showAdd: false
            })
        }
    }

    edit = row => {
        const { editName, editDepo, editDesc } = this.state
        if ((row.name || editName) && ((row.stantion && row.stantion.id) || editDepo)) {
            this.props.addParking({
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

    onCellClick = row => {
        if (!row.showEdit) {
            this.setState({ ...this.state, showAdd: false })
            this.props.showEdit(row.id)
        } else {
            this.props.hideEdit(row.id)
            this.setState({
                ...this.state,
                editName: null,
                editDepo: null,
                editDesc: null
            })
        }
    }

    setPage = (skip, l) => {
        const { limit, filter1, filter2 } = this.state
        this.props.getParkings(skip, (l > 0 && l) || limit, filter1, filter2)
        this.setState({ skip })
    }

    render() {
        const { result: { data, total }, stations } = this.props
        const {
            name,
            station,
            skip,
            limit,
            editName,
            editDepo,
            editDesc,
            showAdd,
            desc,
            filter1,
            filter2
        } = this.state
        return (
            <div>
                <div className="add-item card">
                    <div className="layout vertical">
                        <div className=" layout horizontal">
                            <InputField
                                label="Название места"
                                className="flex"
                                value={filter1}
                                onChange={event => {
                                    this.setState({ ...this.state, filter1: event.currentTarget.value })
                                }}
                                onEnter={() => {
                                    const { limit, filter1, filter2 } = this.state
                                    this.setState({ ...this.state, skip: 0 })
                                    this.props.getParkings(0, limit, filter1, filter2)
                                }}
                                style={{ minWidth: '250px' }}
                            />

                            <DropdownField
                                label="Депо"
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
                                    this.props.getParkings(0, limit, filter1, filter2)
                                }}
                                className="margin-left"
                            />
                            <BlueButton
                                label="Сбросить"
                                onClick={() => {
                                    const { limit } = this.state
                                    this.setState({ ...this.state, skip: 0, filter1: null, filter2: null })
                                    this.props.getParkings(0, limit)
                                }}
                                className="margin-left"
                            />
                        </div>
                    </div>
                </div>

                <div className="table-layout card  layout vertical margin-top">
                    <div className=" layout horizontal margin-bottom">
                        <GreenButton
                            label="Добавить место постановки"
                            onClick={() => {
                                this.props.hideEditors()
                                this.setState({ ...this.state, showAdd: true })
                            }}
                        />
                    </div>
                    <Table>
                        <HeaderRow>
                            <HeaderCell>Наименование</HeaderCell>
                            <HeaderCell>Описание</HeaderCell>
                            <HeaderCell>Депо</HeaderCell>
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
                                        label="Название места постановки"
                                        onChange={this.setName}
                                        value={name}
                                        className="flex margin-left"
                                    />

                                    <DropdownField
                                        label="Станция"
                                        onChange={this.setStation}
                                        value={station}
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
                                        label="Описание места постановки"
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
                                        <Row
                                            className={row.expanded || row.showEdit ? 'expanded' : ''}
                                            onClick={() => {
                                                this.onCellClick(row)
                                            }}
                                        >
                                        <Cell>
                                            {row.name}
                                        </Cell>
                                        <Cell>
                                            {row.description}
                                        </Cell>
                                        <Cell>
                                            {row.stantion && row.stantion.name}
                                        </Cell>
                                        <Cell className="last cell layout horizontal center-center">
                                            <div
                                                className={(true && 'icon-delete') || 'icon-delete-1'}
                                                onClick={() => {
                                                    true && this.props.delParking(row.id)
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
                                        </div>
                                    )}
                                </div>
                            ))
                        }
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

const provider = provide((state: ApplicationState) => state.parking, ParkingStore.actionCreators)

type Props = typeof provider.allProps

export default provider.connect(CLS)
