import * as React from 'react'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../../store';
//import * as InvItemState from '../../store/InvItem';

import * as DialogStore from '../../../store/DialogStore';
import * as GridStore from '../../../store/GridStore';
import * as store from '../store';
import { DialogForm } from '../../../UI/dialog'
import { Button } from '../../../UI/button'
import { DropDown, AutocompliteDataSource } from '../../../UI/dropdown'
import { ReadOnlyField } from '../../Common'
import * as remove from 'lodash/remove'
import { InputNumber, TextArea, Input } from '../../../UI/input'
import { ScrollBarsVertical } from '../../../UI/scrollbars'
import Item from 'antd/lib/list/Item';
import { dataSourceSelect } from '../../../resource'
import moment from 'moment'
import api from '../../../services/rest'
import { DocumentType } from '../../../common'
import classnames from 'classnames'
import { UploadFile } from './extensions'
import { isValidData, checkToValue } from '../../../UI/utils/utils'

interface State {
    carriageId?: number
    equipmentId?: number
    executor?: number
    faultId?: number
    filesId?: number[]
    taskStatus?: number
    taskType?: number
    text?: string
    trainId?: number
    taskLevel?: number
    files?: any
}

class TaskCreate extends React.Component<Props, State> {

    constructor(props) {
        super(props)

        this.state = {
            files: [],
            taskLevel:1
        }
    }

    componentWillUnmount = () => {
        this.props.unloadState();
    }

    handleChange = (newValue, field) => {

        var state: State = {}

        switch (field) {
            case 'trainId':
                this.props.getCarriagesByTrainId(newValue)
                state = { carriageId: undefined, faultId: undefined, equipmentId: undefined }
                break;
            case 'carriageId':
                this.props.getEquipmentsByCarriage(newValue)
                state = { faultId: undefined, equipmentId: undefined }
                break;
            case 'equipmentId':
                this.props.getFaultByEquipmentId(newValue)
                state = { faultId: undefined }
                break;
            case 'executor':
                this.props.getAvaibleStatuses(newValue)
                state = { taskStatus: undefined }
                break;
        }

        state[field] = newValue;

        this.setState(state);
    }

    save = () => {

        const {
            carriageId,
            equipmentId,
            executor,
            faultId,
            taskStatus,
            taskType,
            text,
            trainId,
            taskLevel
        } = this.state;

        var result = {
            carriageId: carriageId,
            equipmentId: equipmentId,
            executor: executor,
            faultId: faultId,
            filesId: this.state.files.length ? this.state.files.map(x => x.id) : null,
            taskStatus: taskStatus,
            taskType: taskType,
            text: text,
            trainId: trainId,
            taskLevel: taskLevel
        }

        this.props.saveTask(result, () => {
            this.props.reloadGrid();
        })
    }

    get isValid(): boolean {
        const {
            carriageId,
            equipmentId,
            executor,
            faultId,
            taskStatus,
            taskType,
            text,
            taskLevel,
            trainId } = this.state;

        var isValid = isValidData({
            carriageId: carriageId,
            equipmentId: equipmentId,
            executor: executor,
            taskStatus: taskStatus,
            taskType: taskType,
            trainId: trainId,
            taskLevel: taskLevel
        })

        return isValid;
    }

    render() {

        return (
            <div>
                <div className="dialog-body clear-x dialog-task" style={{ width: 700, height: 584 }}>
                    <div className="g-data" style={{ border: 'none'}}>
                        <Input disabled value={this.props.user} name="user" title="Инициатор" />
                        <DropDown
                            isClear={false}
                            title="Исполнитель"
                            dataSource={dataSourceSelect.brigadeType}
                            name="executor"
                            value={this.state.executor}
                            handleChange={this.handleChange} />
                        <DropDown
                            isClear={false}
                            title="Статус"
                            dataSource={this.props.data.statuses}
                            name="taskStatus"
                            value={this.state.taskStatus}
                            handleChange={this.handleChange} />
                        <DropDown
                            isClear={false}
                            title="Уровень критичности"
                            dataSource={dataSourceSelect.taskLevel}
                            name="taskLevel"
                            value={this.state.taskLevel}
                            handleChange={this.handleChange} />
                        <DropDown
                            title="Тип"
                            dataSource={dataSourceSelect.taskType}
                            name="taskType"
                            value={this.state.taskType}
                            handleChange={this.handleChange} />
                        <DropDown
                            title="Поезд"
                            dataSource={this.props.data.trains}
                            name="trainId"
                            value={this.state.trainId}
                            handleChange={this.handleChange} />
                        <AutocompliteDataSource
                            title="Вагон"
                            dataSource={this.props.data.carriages}
                            name="carriageId"
                            value={this.state.carriageId}
                            handleChange={(newValue, field) => this.handleChange(newValue.value, field)} />
                        <AutocompliteDataSource
                            title="Оборудование"
                            dataSource={this.props.data.equipments}
                            name="equipmentId"
                            value={this.state.equipmentId}
                            handleChange={(newValue, field) => this.handleChange(newValue.value, field)} />
                        <AutocompliteDataSource
                            title="Типовая неисправность"
                            dataSource={this.props.data.faults}
                            name="faultId"
                            value={this.state.faultId}
                            handleChange={(newValue, field) => this.handleChange(newValue.value, field)} />
                        <TextArea height={190} title="Текст сообщения" name="text" value={this.state.text} handleChange={this.handleChange} />
                        <UploadFile files={this.state.files} onChange={x => this.setState({ files: x })} />
                    </div>
                </div>
                <nav className="dialog-actions">
                    <Button label="Сохранить" disabled={!this.isValid} onClick={this.save} />
                    <Button label="Закрыть" onClick={() => this.props.toggleDialog(false)} />
                </nav>
            </div>
        )
    }
}
const provider = provide(
    (state: ApplicationState) => ({ user: state.dialog.responseMessage, data: state.journals, reloadGrid: state.dialog.action }),
    { ...DialogStore.actionCreators, ...store.actionCreators }
);

type Props = typeof provider.allProps;

export default provider.connect(TaskCreate);