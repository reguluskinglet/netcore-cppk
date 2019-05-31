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

import * as remove from 'lodash/remove'
import { InputNumber, Input } from '../../../../UI/input'
import { ScrollBarsVertical } from '../../../../UI/scrollbars'
import { AutocompliteDataSource } from '../../../../UI/dropdown'
import moment, { Moment } from 'moment'
import * as store from '../store';
import { DayOfWeek } from '../../../Common'
import union from 'lodash/union';
import classnames from 'classnames'
import { checkToValue } from '../../../../UI/utils/utils'

interface Props {
    currentStantions: TripStantion[]
    stantions: SelectItem[]
    onSave: (data) => void
}

interface State {
    stantionId?: number,
    stantionName?: string,
    start?: string,
    end?: string
    disabledEnd?: boolean

    dateStart?: Moment
    dateEnd?: Moment
}

export class StantionOnTrips extends React.Component<Props, State> {

    constructor(props) {
        super(props)

        this.state = {
            stantionId: null,
            stantionName: null,
            start: '00:00',
            end: '00:00',
            disabledEnd: true
        }
    }

    

    handleChange = (newValue, field) => {

        var state = {}
        state[field] = newValue;

        if (field === 'start') {
            var dateStart = this.getDate(newValue, '01'),
                dateEnd = this.getDate(newValue, '01');

            dateEnd.add(1, "millisecond");

            state['disabledEnd'] = false;
            state['end'] = dateEnd.format('HH:mm')
            state['dateEnd'] = dateEnd;
            state['dateStart'] = dateStart;
        }
        else if (field === 'end') {
            var dateEnd = this.getDate(newValue, '01');
            dateEnd.add(1, "millisecond");
            state['dateEnd'] = dateEnd;
            //var sStart = this.state.dateStart,
            //    sEnd = this.getDate(newValue, '01');

            ////   const hasCurrentDay = moment(sStart).isBefore(moment(sEnd))

            //if (sStart.isSameOrAfter(sEnd)) {

            //    var end = sStart.add(1, 'minutes');

            //    state['dateEnd'] = end;
            //    state['end'] = end.format('HH:mm');
            //}
        }

        this.setState(state);
    }

    getDate(time: string, day) {
        return moment(`0001-01-${day}T${time}`);
    }

    handleChangeStantion = (newValue: SelectItem, field) => {
        var state = {
            stantionId:newValue.value,
            stantionName:newValue.text
        }

        this.setState(state);
    }

    save = () => {

        var data = { ...this.state }

        const { stantionId, start, dateStart, end, dateEnd } = this.state;

        const hasCurrentDay = moment(dateStart).isBefore(moment(dateEnd))

        data.start = this.getDate(start, '01').format('YYYY-MM-DDTHH:mm:ss');
        data.end = this.getDate(end, hasCurrentDay ? '01' : '02').format('YYYY-MM-DDTHH:mm:ss');

        this.props.onSave(data)
    }

    get isValid(): boolean {

        const { stantionId, start, dateStart, end, dateEnd } = this.state;

        var stantions = this.props.currentStantions,
            lasStantion = stantions.length ? stantions[stantions.length - 1] : null;

        //var fixDateStart;

        //if (lasStantion !== null) {
        //    var inTime = moment(lasStantion.inTime, 'YYYY-MM-DDTHH:mm:ss'),
        //        ouTime = moment(lasStantion.outTime, 'YYYY-MM-DDTHH:mm:ss');

        //    var dateS = dateStart;

        //    var hDateStart = dateS.hours(),
        //        hOutTime = ouTime.hours();

        //    if (hOutTime > hDateStart)
        //        dateEnd.add(1, 'days')

        //    if (ouTime.isSameOrAfter(dateStart)) {
        //        return false;
        //    }

        //}

        if (dateStart === undefined || dateEnd === undefined)
            return false;

        var hourStart = dateStart.hours(),
            hourEnd = dateEnd.hours();

        const hasCurrentDay = moment(dateStart).isBefore(moment(dateEnd))
     
        if (!hasCurrentDay) {
            if (hourEnd > 3 || hourStart === hourEnd)
                return false;

        }
        else if (dateStart.toDate().getTime() > dateEnd.toDate().getTime()) {
            return false;
        }

        return checkToValue(stantionId)
    }

    render() {

        const { stantionId, start, end } = this.state

        return <div className="g-data">
            <div className="g-body">
                <AutocompliteDataSource name="stantionId" title="Станция" value={stantionId} dataSource={this.props.stantions} handleChange={this.handleChangeStantion} />
                <Input time isClear={false} title="Время прибытия" name="start" value={start} handleChange={this.handleChange} />
                <Input disabled={this.state.disabledEnd} time isClear={false} title="Время убытия" name="end" value={end} handleChange={this.handleChange} />
            </div>
            <div className="g-data-action">
                <Button label="Сохранить" onClick={this.save} disabled={!this.isValid} />
            </div>
        </div>
    }
}