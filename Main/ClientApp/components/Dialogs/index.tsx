import './dialog.scss'
import * as React from 'react'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../store';
import * as GridStore from '../../store/GridStore';
import { DialogForm } from '../../UI/dialog'
import { GridType, TimelineTypeEnum } from '../../common'
import * as DialogStore from '../../store/DialogStore';
import { JournalTask, JournalInspection, JournalTaskCreate } from '../Journals/Dialogs'

import { AddRoute, EventDialog, AddTripRoute } from '../Planning/Goep/Graph/Dialogs'
import { PouEventDialog, BrigadeDialog, PouTripDialog } from '../Planning/Pou/Dialogs'
import TripCreate from '../Planning/Routes/Dialogs/Create'
import GoepCreate from '../Planning/Goep/Dialogs/Create'
import CreateOrEdit from '../Planning/Dispatcher/Dialogs/CreateOrEdit'

import NmpnCreate from '../Planning/Nmpn/Dialogs/Create'

class Dialog extends React.Component<Props, undefined> {

    render() {

        var title;

        switch (this.props.status) {
            case 409:
                title = 'Предупреждения по удалению выбранных записей:'
        }

        var Component;

        var className = 'fix-body';
        let closeAction: any = () => this.props.toggleDialog(false);
        var fixed = false;

        switch (this.props.type) {
            case 'nmpn.create':
                title = `Назначение бригад и поезда`
                Component = NmpnCreate
                break;
            case 'dispatcher.createOrEdit':
                title = `Ремонт в депо`
                Component = CreateOrEdit
                break;
            case 'planning.goepCreate':
                title = `Оборот поездов`
                Component = GoepCreate
                break;
            case 'planning.tripCreate':
                title = `Рейс`
                Component = TripCreate
                className += ' o-700';
                break;
            case 'journal.inspection':
                var data = this.props.responseMessage;
                title = `Мероприятие ${data.type} №${data.id}`
                Component = JournalInspection
                className += ' o-700';
                break;
            case 'journal.task':
                Component = JournalTask
                className += ' o-700';
                break;
            case 'journal.taskcreate':
                title='Создание задачи'
                Component = JournalTaskCreate
                className += ' o-700';
                break;
            case 'inspectionPou':
                var data = this.props.responseMessage,
                    type = data.timeLineType,
                    event = type == TimelineTypeEnum.TimeRangeTo2 ? `ТО-2` : 'СТО';

                title = `Мероприятие ${event}`
                Component = PouEventDialog
                className += ' o-700';
                break;
            case 'graphaaddroute':
                title = `Маршрут`
                Component = AddRoute
                break;
            case 'timlineEvent':
                var type = this.props.responseMessage.checkListType;
                title = type == TimelineTypeEnum.TimeRangeTo2 ? `ТО-2` : 'СТО';
                Component = EventDialog
                break;
            case 'addTripRoute':
                title = `Рейс`
                Component = AddTripRoute
                break;
            case 'brigadePou':
                title = `Бригады`
                Component = BrigadeDialog
                break;
            case 'tripDialogPou':
                title = `Изменить рейс`
                Component = PouTripDialog
                break;
        }

        return (
            <DialogForm
                className="normal"
                active={this.props.open}
                onEscKeyDown={closeAction}
                onOverlayClick={closeAction}
                title={title}
                classBody={className}
                fixed={fixed}
            >
                {Component ? <Component /> : null}
            </DialogForm>
        )
    }
}

const provider = provide(
    (state: ApplicationState) => state.dialog,
    { ...DialogStore.actionCreators, ...GridStore.actionCreators }
)

type Props = typeof provider.allProps;

export default provider.connect(Dialog);