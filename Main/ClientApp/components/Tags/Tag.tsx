import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from './TagStore'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import {
    InputField,
    TextareaField,
    DropdownField,
    DropdownMultiField
} from '../../UI/fields'
import { GreenButton, BlueButton, PrinterButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'
import { Redirect } from 'react-router-dom'
import moment from 'moment'

const tagTypes = [
    { value: 1, label: 'Бумажные метки' },
    { value: 2, label: 'Корпусные метки' }
]

export const typeLabels = [
    'Головной вагон',
    'Моторный вагон',
    'Прицепной вагон'
]

export const modelTypes = ['головной', 'моторный', 'прицепной']

class Tag extends React.Component<Props, any> {
    componentWillMount() {
        const id = parseInt(this.props.match.params.id)
        this.setState({
            skip: 0,
            limit: 10,
            loading: false,
            reload: false,
            filter1: null,
            filter2: null,
            id: id
        })
        this.props.get(id, 0, 10)
        this.props.getLinks(id)
    }

    componentWillReceiveProps(nextProps) {
        const { skip, limit, id } = this.state
        if (nextProps.reload) {
            this.props.get(id, skip, limit)
            this.setState({ ...this.state, train: null, vagon: null, equip: [] })
        }
        if (nextProps.tag !== this.props.tag) {
            this.setState({
                ...this.state,
                type: nextProps.tag.labelType,
                temp: nextProps.tag.templateLabelsId,
                label: nextProps.tag.name
            })
        }
    }

    add = () => { }

    setPage = (skip, l) => {
        const { limit, filter1, filter2, id } = this.state
        this.props.get(id, skip, (l > 0 && l) || limit, filter1, filter2)
        this.setState({ skip })
    }

    onCellClick = row => { }

    render() {
        const {
            result = { data: [], total: 0 },
            templates,
            trains,
            vagons,
            equips,
            tag,
            printers,
            print1,
            procent,
            isStartPrint,
            isSelectedAll,
            selectedRows
        } = this.props
        const { data, total } = result
        const {
            skip,
            limit,
            filter1,
            filter2,
            redirect,
            type,
            temp,
            label,
            id,
            train,
            vagon,
            equip,
            connectionType,
            printerName,
            comPort,
            modelName,
            power
        } = this.state
        var enabledPrintButton = isStartPrint === undefined && (selectedRows && selectedRows.length > 0) || this.props.isSelectedAll

        if (enabledPrintButton) {
            if (type === 1)
                enabledPrintButton = connectionType && printerName && temp;
            else
                enabledPrintButton = modelName != undefined && comPort;
        }

        const connectionTypes = []
        printers &&
            printers.paperTag &&
            printers.paperTag.connectedTypes &&
            printers.paperTag.connectedTypes.forEach(element => {
                connectionTypes.push({
                    value: parseInt(element.value),
                    label: element.text
                })
            })

        const printerNames = []
        const a =
            printers &&
            printers.paperTag &&
            printers.paperTag.connectionTypeItems &&
            printers.paperTag.connectionTypeItems.find(
                e => e.connectionType === connectionType
            )

        a &&
            a.printers &&
            a.printers.forEach(element => {
                printerNames.push({ value: element.value, label: element.text })
            })

        const modelNames = []
        printers &&
            printers.corpTag &&
            printers.corpTag.deviceModels &&
            printers.corpTag.deviceModels.forEach(element => {
                modelNames.push({ value: parseInt(element.value), label: element.text })
            })

        const comPorts = []
        printers &&
            printers.corpTag &&
            printers.corpTag.comPorts.forEach(element => {
                comPorts.push({ value: element.value, label: element.text })
            })

        //if (print1 === true)
        //  // setTimeout(() => {
        //  this.props.getPrintState()
        // }, 1000)

        if (redirect) return <Redirect to={redirect} push={true} />
        else
            return (
                <div>
                    <div className="layout horizontal end">
                        <span
                            style={{ fontSize: '16px', cursor: 'pointer', color: '#3D496B' }}
                            onClick={() => {
                                this.props.history.goBack()
                            }}
                        >
                            Оборудование
            </span>
                        <span
                            className="icon-chevron-right"
                            style={{
                                fontSize: '16px',
                                padding: '0 4px 1px 4px',
                                cursor: 'pointer',
                                color: '#3D496B'
                            }}
                        />
                        <span
                            style={{
                                textDecoration: 'underline',
                                cursor: 'pointer',
                                color: '#3D496B',
                                fontSize: '16px'
                            }}
                        >
                            {`Задание № ${id}`}
                        </span>
                    </div>

                    <div className="add-item card margin-top">
                        <div className="layout vertical">
                            <div className="layout horizontal">
                                <div className="layout horizontal center">
                                    <div className="label flex-none margin-right">{`${tag.userName}, ${moment(tag.createDate).utcOffset(0).format('DD.MM.YYYY')}`}</div>

                                    <DropdownField
                                        label="Тип"
                                        className="flex"
                                        value={type}
                                        onChange={event => {
                                            const value = parseInt(event.currentTarget.value)
                                            this.setState({ ...this.state, type: value })
                                        }}
                                        list={tagTypes}
                                        showNull
                                    />

                                    <InputField
                                        label="Наименование"
                                        className="flex margin-left"
                                        value={label}
                                        onChange={event => {
                                            const value = event.currentTarget.value
                                            this.setState({ ...this.state, label: value })
                                        }}
                                    />

                                    <DropdownField
                                        label="Шаблон"
                                        className="flex margin-left"
                                        value={temp}
                                        onChange={event => {
                                            const value = parseInt(event.currentTarget.value)
                                            this.setState({ ...this.state, temp: value })
                                        }}
                                        showNull
                                        list={templates}
                                    />

                                    <span
                                        className={`${label
                                            ? 'icon-save'
                                            : 'icon-save-1'} path1 path2 path3 margin-left`}
                                        onClick={() => {
                                            label && this.props.save({ type, temp, label, id })
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

                    {printers && (
                        <div className="add-item card margin-top">
                            <div className="layout vertical">
                                <div className="layout horizontal">
                                    <div className="layout horizontal center">
                                        <div className="label flex-none margin-right">
                                            Настройки печати:
                    </div>

                                        {type === 1 && (
                                            <DropdownField
                                                label="Тип соединения"
                                                className="flex margin-left"
                                                value={connectionType}
                                                onChange={event => {
                                                    const value = parseInt(event.currentTarget.value)
                                                    this.setState({
                                                        ...this.state,
                                                        connectionType: value
                                                    })
                                                }}
                                                list={connectionTypes}
                                                showNull
                                            />
                                        )}

                                        {type === 1 &&
                                            connectionType === 2 && (
                                                <DropdownField
                                                    label="Имя принтера"
                                                    className="flex margin-left"
                                                    value={printerName}
                                                    onChange={event => {
                                                        const value = event.currentTarget.value
                                                        this.setState({ ...this.state, printerName: value })
                                                    }}
                                                    showNull
                                                    list={printerNames}
                                                />
                                            )}

                                        {type === 1 &&
                                            connectionType === 0 && (
                                                <DropdownField
                                                    label="COM порт"
                                                    className="flex margin-left"
                                                    value={comPort}
                                                    onChange={event => {
                                                        const value = event.currentTarget.value
                                                        this.setState({ ...this.state, comPort: value })
                                                    }}
                                                    showNull
                                                    list={comPorts}
                                                />
                                            )}

                                        {type === 2 && (
                                            <DropdownField
                                                label="Модель"
                                                className="flex margin-left"
                                                value={modelName}
                                                onChange={event => {
                                                    const value = parseInt(event.currentTarget.value)
                                                    this.setState({ ...this.state, modelName: value })
                                                }}
                                                showNull
                                                list={modelNames}
                                            />
                                        )}

                                        {type === 2 && (
                                            <DropdownField
                                                label="COM порт"
                                                className="flex margin-left"
                                                value={comPort}
                                                onChange={event => {
                                                    const value = event.currentTarget.value
                                                    this.setState({ ...this.state, comPort: value })
                                                }}
                                                showNull
                                                list={comPorts}
                                            />
                                        )}

                                        {type === 2 && (
                                            <InputField
                                                label="Мощность"
                                                className="flex margin-left"
                                                type="number"
                                                value={power}
                                                onChange={event => {
                                                    const value = parseInt(event.currentTarget.value)
                                                    this.setState({ ...this.state, power: value })
                                                }}
                                            />
                                        )}

                                        <PrinterButton
                                            label="Печать"
                                            enabled={enabledPrintButton}
                                            onClick={() => {
                                                const rows = this.props.selectedRows 
                                                const request = {
                                                    id: this.state.id,
                                                    isSelectedAll: this.props.isSelectedAll,
                                                    selectedRows: rows && rows.filter(x => x.selected == !this.props.isSelectedAll).map(x => x.id)
                                                }
                                                 this.props.print(request, {
                                                    connectionType,
                                                    printerName,
                                                    comPort,
                                                    power,
                                                    modelName,
                                                    paper: type === 1,
                                                    template:
                                                        templates.find(e => e.value === temp) &&
                                                        templates.find(e => e.value === temp).template
                                                })
                                            }}
                                            className="margin-left"
                                        />

                                        {print1 === true && (
                                            <div className="margin-left label layout horizontal center">
                                                <div>{`Печать меток: ${procent} %`}</div>
                                                <span
                                                    className="icon-close"
                                                    onClick={() => {
                                                        this.props.stopPrint()
                                                    }}
                                                />
                                            </div>
                                        )}
                                    </div>
                                </div>
                            </div>
                        </div>
                    )}

                    <div className="table-layout card  layout vertical margin-top">
                        <div className="layout horizontal center margin-bottom">
                            <DropdownField
                                label="Поезд"
                                className="flex"
                                value={train}
                                onChange={event => {
                                    const value = parseInt(event.currentTarget.value)
                                    this.setState({
                                        ...this.state,
                                        train: value,
                                        vagon: null,
                                        equip: []
                                    })
                                    this.props.getVagons(value)
                                }}
                                showNull
                                list={trains}
                            />

                            <DropdownField
                                label="Вагон"
                                className="flex margin-left"
                                value={vagon}
                                onChange={event => {
                                    const value = parseInt(event.currentTarget.value)
                                    this.setState({ ...this.state, vagon: value, equip: [] })
                                    this.props.getEq(value)
                                }}
                                showNull
                                list={vagons}
                            />

                            <DropdownMultiField
                                label="Оборудование"
                                className="flex margin-left margin-right"
                                value={equip}
                                onChange={value => {
                                    this.setState({ ...this.state, equip: value })
                                }}
                                showNull
                                list={equips}
                            />

                            <GreenButton
                                label="Добавить"
                                onClick={() => {
                                    vagon &&
                                        equip.length &&
                                        id &&
                                        equip.forEach(element => {
                                            this.props.add({
                                                CarriageId: vagon,
                                                EquipmentModelId: element,
                                                TaskPrintId: id
                                            })
                                        })
                                }}
                            />
                        </div>

                        <Table>
                            <HeaderRow>
                                <HeaderCell className="header-cell">
                                    <InputField
                                        type="checkbox"
                                        value={this.props.isSelectedAll}
                                        onChange={event => {
                                            this.props.selectRowAll(
                                                event.currentTarget.checked
                                            )
                                        }}
                                        hideLabel
                                    />
                                </HeaderCell>
                                <HeaderCell>Наименование</HeaderCell>
                                <HeaderCell>Поезд</HeaderCell>
                                <HeaderCell>Вагон</HeaderCell>
                                <HeaderCell>ИД метки</HeaderCell>
                                <HeaderCell>Время печати</HeaderCell>
                                <HeaderCell className="last header-cell" />
                            </HeaderRow>

                            {data &&
                                data.map((row, index) => (
                                    <div key={`ct_${index}${row.id}`}>
                                        <Row
                                            className={row.expanded || row.showEdit ? 'expanded' : ''}
                                        >
                                            <Cell
                                                className="cell layout horizontal center-center"
                                                onClick={() => { }}
                                            >
                                                <InputField
                                                type="checkbox"
                                                    value={row.selected}
                                                    onChange={event => {
                                                        this.props.selectRow(
                                                            row,
                                                            event.currentTarget.checked
                                                        )
                                                    }}
                                                    hideLabel
                                                />
                                            </Cell>
                                            <Cell
                                                onClick={() => {
                                                    this.onCellClick(row)
                                                }}
                                            >
                                                {row.equipmentName}
                                            </Cell>
                                            <Cell
                                                onClick={() => {
                                                    this.onCellClick(row)
                                                }}
                                            >
                                                {row.trainName}
                                            </Cell>
                                            <Cell
                                                onClick={() => {
                                                    this.onCellClick(row)
                                                }}
                                            >
                                                {/* {`${row.carriageNumber} (${(row.modelType >= 0 && typeLabels[row.modelType]) || ''})`} */}
                                                {`${row.carriageSerialNumber ||
                                                    ''} (${row.carriageNumber || ''}, ${modelTypes[
                                                    row.modelType
                                                    ] || ''})`}
                                            </Cell>
                                            <Cell
                                                onClick={() => {
                                                    this.onCellClick(row)
                                                }}
                                            >
                                                {row.rfid}
                                            </Cell>
                                            <Cell
                                                onClick={() => {
                                                    this.onCellClick(row)
                                                }}
                                            >
                                                {row.timePrintedDateTime}
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

const provider = provide(
    (state: ApplicationState) => state.tag,
    Store.actionCreators
).withExternalProps<RouteComponentProps<any>>()

type Props = typeof provider.allProps

export default provider.connect(Tag)
