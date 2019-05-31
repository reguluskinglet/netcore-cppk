import * as React from 'react'
import { provide } from 'redux-typed';
import TimeField from 'react-simple-timefield';
import { ApplicationState } from '../../../../store';
//import * as InvItemState from '../../store/InvItem';
import { TimelineTypeEnum } from '../../../../common'
import * as DialogStore from '../../../../store/DialogStore';
import * as GridStore from '../../../../store/GridStore';
import { DialogForm } from '../../../../UI/dialog'
import { Button } from '../../../../UI/button'
import { ReadOnlyField } from '../../../Common'
import * as remove from 'lodash/remove'
import { InputNumber, Input } from '../../../../UI/input'
import CheckBoxField from '../../../../UI/checkbox/CheckBoxField'
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
    droped: boolean
}

class EventDialog extends React.Component<Props, State> {

    constructor(props: Props) {
        super(props)

        var data = props.data;

        this.state = {
            start: this.getTime(data.fact.dateStart),
            end: this.getTime(data.fact.dateEnd),
            droped: data.fact.canseled
        }
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

        if (this.isValid) {
            var fact = this.props.data.fact;

            var result = {
                droped: this.state.droped,
                planedInspectionRouteId: this.props.data.id,
                start: this.getDate(fact.dateStart, this.state.start),
                end: this.getDate(fact.dateEnd, this.state.end),
                trainId: 0
            }

            this.props.changeTimeLine(result, this.props.timeLineType, this.props.action)
        }
       
    }

    getTime(date: string) {
        return moment(date,'YYYY-MM-DDTHH:mm:ss').format('HH:mm');
    }

    getDate(date: string, time: string) {
        var currentDate = moment(date,'YYYY-MM-DDTHH:mm:ss'),
            times = time.split(':');

        return moment({
            year: currentDate.year(),
            month: currentDate.month(),
            day: currentDate.day(),
            hour: parseInt(times[0]),
            minute: parseInt(times[1])

        }).format('YYYY-MM-DDTHH:mm:ss')+'Z';
    }

    get isValid(): boolean {

        var start = this.parceTime(this.state.start, '01'),
            end = this.parceTime(this.state.end, '01');

        const isBefore = moment(start).isBefore(moment(end))

        return isBefore;
    }


    getDateTime(time: string, day) {
        return moment(`0001-01-${day}T${time}:00+00:00`);
    }

    parceTime(time: string, day) {
        return moment(`0001-01-${day}T${time}:00+00:00`);
    }

    render() {
        const { start, end } = this.state;
        var data = this.props.data;

        return (
            <div>
                <div className="dialog-body clear-x" style={{ width: 599 }}>
                    <div className="g-main-block">
                        <ReadOnlyField title="Маршрут" value={data.routeName} />
                        <ReadOnlyField title="Состав" value={data.trainName} />
                        <div className="g-block first">
                            <h4 className="g-h-title g-pink">План</h4>
                            <ReadOnlyField title="Начало" value={this.getTime(data.plan.dateStart)} />
                            <ReadOnlyField title="Окончание" value={this.getTime(data.plan.dateEnd)} />
                        </div>
                        <div className="g-block">
                            <h4 className="g-h-title g-orange">Факт</h4>
                            <Input time isClear={false} name="start" value={start} handleChange={this.handleChange} className="g-pading" />
                            <Input time isClear={false} name="end" value={end} handleChange={this.handleChange} className="g-pading" />
                            <CheckBoxField className="g-check" invert fieldTitleWidth={115} name="droped" title="Отменен" checked={this.state.droped} handleChange={this.handleChange} />
                        </div>
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
    (state: ApplicationState) => ({ data: (state.dialog.responseMessage.result as TimeRangeData), timeLineType: (state.dialog.responseMessage.timeLineType as TimelineTypeEnum), action: state.dialog.action }),
    { ...DialogStore.actionCreators, ...store.actionCreators }
);

type Props = typeof provider.allProps;

export default provider.connect(EventDialog);
 