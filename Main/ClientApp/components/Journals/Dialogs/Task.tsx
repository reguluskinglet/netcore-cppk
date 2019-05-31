import * as React from 'react'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../../store';
//import * as InvItemState from '../../store/InvItem';

import * as DialogStore from '../../../store/DialogStore';
import * as store from '../store';
import { DialogForm } from '../../../UI/dialog'
import { Button } from '../../../UI/button'
import { DropDown } from '../../../UI/dropdown'
import { ReadOnlyField } from '../../Common'
import * as remove from 'lodash/remove'
import { InputNumber, TextArea } from '../../../UI/input'
import { ScrollBarsVertical } from '../../../UI/scrollbars'
import Item from 'antd/lib/list/Item';
import { dataSourceSelect } from '../../../resource'
import moment from 'moment'
import api from '../../../services/rest'
import { DocumentType } from '../../../common'
import { FileBlock, HistoryBlock, FaultBlock, InspectionBlock, UploadFile } from './extensions'
import classnames from 'classnames'


interface State {
    commentText?: string
    files: JournalTaskFile[]
    tabIndex: Tabs
    brigadeType: number
    statusId: number
}

enum Tabs {
    home = 0,
    fault = 1,
    history = 2,
    inspection = 3
}

class Task extends React.Component<Props, State> {

    statuses: SelectItem[]

    constructor(props: Props) {
        super(props)

        this.state = {
            commentText: '',
            files: [],
            tabIndex: 0,
            brigadeType: props.data.brigadeType,
            statusId: props.data.statusId
        }

        this.statuses = props.data.possibleTaskStatuses.map(x => dataSourceSelect.status.find(s => x === s.value))
    }

    handleChange = (newValue, field) => {

        var state = {}
        state[field] = newValue;

        this.setState(state);
    }

    find = (sources: SelectItem[], value: number) => {
        var item = sources.find(x => x.value === value)

        return item ? item.text:''
    }

    tab = (text: string, icon: string, index: Tabs) => {

        var className = classnames('g-tab-menu-item', {
            'activ': index === this.state.tabIndex
        })

        return <div className={className} onClick={() => this.setState({ tabIndex: index })}>
            <div className={'g-icon ' + icon}></div>
            <div className="g-text">{text}</div>
        </div>
    }

    renderHistory = () => {
        return <div className="g-data">
            <h6 className="title">История изменения статусов, исполнителей и комментариев</h6>
            <ScrollBarsVertical width={710} height={479}>
                {this.props.data.history && this.props.data.history.map((x, i) => <HistoryBlock key={i} history={x} />)}
            </ScrollBarsVertical>
        </div>
    }

    renderFault = () => {
        return <div className="g-data">
            <h6 className="title">Неисправности</h6>
            <ScrollBarsVertical width={710} height={479}>
                {this.props.data.faults && this.props.data.faults.length
                    ? this.props.data.faults.map((x, i) => <FaultBlock key={i} fault={x} />)
                    : <p>Нет заведенных неисправностей</p>}
            </ScrollBarsVertical>
        </div>
    }

    renderInspection = () => {
        return <div className="g-data">
            <h6 className="title">Проведенные мероприятия</h6>

            <ScrollBarsVertical width={710} height={479}>
                {this.props.data.inspections && this.props.data.inspections.length
                    ? this.props.data.inspections.map((x, i) => <InspectionBlock key={i} inspection={x} />)
                    :<p>Нет проведенных мероприятий</p>}
            </ScrollBarsVertical>
        </div>
    }

    renderTask = () => {

        var data = this.props.data;

        return <div className="g-data">
            <h6 className="title">Инцидент</h6>
            <ReadOnlyField title="Создан" value={moment(data.data, 'YYYY-MM-DDTHH:mm:ss').format('DD.MM.YYYY HH:mm')} />
            <ReadOnlyField title="Поезд" value={data.trainName} />
            <ReadOnlyField title="Вагон" value={data.carriageSerial} />
            <ReadOnlyField title="Оборудование" value={data.equipmentName} />
            <ReadOnlyField title="Тип" value={this.find(dataSourceSelect.taskType, data.taskType)} />
            <ReadOnlyField title="Уровень критичности" value={this.find(dataSourceSelect.taskLevel, data.taskLevel)} />
            <DropDown
                isClear={false}
                title="Текущий исполнитель"
                dataSource={dataSourceSelect.brigadeType}
                name="brigadeType"
                value={this.state.brigadeType}
                handleChange={this.handleChange} />
            <DropDown
                isClear={false}
                title="Текущий статус"
                dataSource={this.statuses}
                name="statusId"
                value={this.state.statusId}
                handleChange={this.handleChange} />
            <TextArea height={190} title="Текст сообщения" name="commentText" value={this.state.commentText} handleChange={this.handleChange} />
            <UploadFile files={this.state.files} onChange={x => this.setState({ files: x})} />
        </div>
    }

    save = () => {

        const { id, statusId, brigadeType } = this.props.data

        var result = {
            traintaskId: this.props.data.id,
            commentText: this.state.commentText
        }

        if (statusId !== this.state.statusId)
            result['statusId'] = this.state.statusId;

        if (brigadeType !== this.state.brigadeType)
            result['trainTaskExecutorsId'] = this.state.brigadeType;

        if (this.state.files.length)
            result['filesId'] = this.state.files.map(x => x.id);

        this.props.updateTask(result, this.props.action)
    }

    render() {

        return (
            <div>
                <div className="dialog-body clear-x dialog-task" style={{ width: 700, height: 587 }}>
                    <div className="g-main-tabs">
                        <div className="g-tab-menu">
                            {this.tab('Основные', 'icon-home', Tabs.home)}
                            {this.tab('Неисправности', 'icon-steam', Tabs.fault)}
                            {this.tab('Мероприятия', 'icon-bell', Tabs.inspection)}
                            {this.tab('История', 'icon-history', Tabs.history)}
                        </div>
                        {this.state.tabIndex === Tabs.home
                            ? this.renderTask()
                            : this.state.tabIndex === Tabs.fault
                                ? this.renderFault()
                                : this.state.tabIndex === Tabs.inspection
                                    ? this.renderInspection()
                                    : this.renderHistory()
                            }
                    </div>
                </div>
                <nav className="dialog-actions">
                    {this.state.tabIndex === Tabs.home && <Button label="Сохранить" onClick={this.save} />}
                    <Button label="Закрыть" onClick={() => this.props.toggleDialog(false)} />
                </nav>
            </div>
        )
    }
}

const provider = provide(
    (state: ApplicationState) => ({ data: (state.dialog.responseMessage as JournalTask),action:state.dialog.action }),
    { ...DialogStore.actionCreators, ...store.actionCreators }
);

type Props = typeof provider.allProps;

export default provider.connect(Task);