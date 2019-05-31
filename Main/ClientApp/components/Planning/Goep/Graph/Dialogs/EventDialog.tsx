import * as React from 'react'
import { provide } from 'redux-typed';
import TimeField from 'react-simple-timefield';
import { ApplicationState } from '../../../../../store';
//import * as InvItemState from '../../store/InvItem';
import { TimelineTypeEnum } from '../../../../../common'
import * as DialogStore from '../../../../../store/DialogStore';
import * as GridStore from '../../../../../store/GridStore';
import { DialogForm } from '../../../../../UI/dialog'
import { Button } from '../../../../../UI/button'

import * as remove from 'lodash/remove'
import { InputNumber, Input } from '../../../../../UI/input'
import moment from 'moment'
import * as store from '../store';

interface Data {
    checkListType: TimelineTypeEnum
    routeId: number
    hover: number
}

interface State {
    start: string
    end: string
}

class EventDialog extends React.Component<Props, State> {

    constructor(props: Props) {
        super(props)

        var data = props.data;

        this.state = {
            start: this.getTime(data.hover,'00'),
            end: this.getTime(data.hover, '00',1),
        }
    }

    getTime = (hover, minute, endTime?) => {
        if (hover < 10)
        {
            hover = hover < 0 ? hover * -1 : hover;

            if (endTime)
                hover += endTime;

            if (hover == 10)
                return `${hover}:${minute}`;

            return `0${hover}:${minute}`;
        }

        if (endTime) {
            if (hover == 23)
                hover = '00'
            else
                hover += endTime;
        }

        return `${hover}:${minute}`;
    }

    //handleChange = (newValue, field) => {

    //    this.data[field] = newValue;
    //}

    handleChange = (newValue, field) => {

        var state = {}
        state[field] = newValue;

        this.setState(state);
    }

    save = () => {

        var data = this.props.data,
            start = this.getDate(this.state.start, '01'),
             end = this.getDate(this.state.end, '01');

        const hasCurrentDay = moment(start).isBefore(moment(end))
      
        end = this.getDate(this.state.end, hasCurrentDay ? '01' : '02');

        var result = {
            routeId: data.routeId,
            start: start,
            end: end,
            checkListType: data.checkListType,
        }

        this.props.addEvent(result, this.props.action)
    }

    getDate(time: string,day) {
        return moment(`0001-01-${day}T${time}:00+00:00`);
    }

    render() {
        const { start, end} = this.state;

        return (
            <div>
                <div className="dialog-body clear-x" style={{ width: 400 }}>
                    <div className="g-dialog-form">
                        <Input time isClear={false} title="Время начала" name="start" value={start} handleChange={this.handleChange} />
                        <Input time isClear={false} title="Время окончания" name="end" value={end} handleChange={this.handleChange} />
                    </div>
                </div>
                <nav className="dialog-actions">
                    <Button label="Сохранить" onClick={this.save} />
                    <Button label="Закрыть" onClick={() => this.props.toggleDialog(false)} />
                </nav>
            </div>
        )
    }
}

const provider = provide(
    (state: ApplicationState) => ({ data: (state.dialog.responseMessage as Data), action: state.dialog.action }),
    { ...DialogStore.actionCreators, ...store.actionCreators }
);

type Props = typeof provider.allProps;

export default provider.connect(EventDialog);
 