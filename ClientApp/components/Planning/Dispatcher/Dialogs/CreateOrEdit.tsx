import * as React from 'react'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../../../store';
//import * as InvItemState from '../../store/InvItem';

import * as DialogStore from '../../../../store/DialogStore';
import * as GridStore from '../../../../store/GridStore';
import { DialogForm } from '../../../../UI/dialog'
import { Button } from '../../../../UI/button'

import * as remove from 'lodash/remove'
import { Input, TextArea } from '../../../../UI/input'
import { DayOfWeek } from '../../../Common'
import { AutocompliteDataSource } from '../../../../UI/dropdown'
import { checkToValue } from '../../../../UI/utils/utils'
import * as store from '../store'
import { DatePicker } from '../../../../UI/datepicker'
import moment from 'moment'

interface State extends DepoEventDtoUi {
    stantionId?: any
}

class DispatcherCreate extends React.Component<Props, State> {

    constructor(props: Props) {
        super(props)


        var result = props.data.result,
            params = props.data.params;

        var trainId = null,
            routeId = null;

        if (params) {
            var train = result.depoEventDataSource.trains.find(x => x.text === params.trainName);

            if (train)
                trainId = train.value;

            var route = result.depoEventDataSource.routes.find(x => x.text === params.routeName);

            if (route)
                routeId = route.value;
        }

        if (result.id)
        {
            var parking = result.depoEventDataSource.parkings.find(x => x.value === result.parkingId);

            this.state = { ...result, stantionId: parking.dependentId }
        }
        else
            this.state = {
                id: null,
                parkingId: null,
                inspectionId: null,
                userId: null,
                inspectionTxt: null,
                trainId: trainId,
                routeId: routeId,
                inTime: null,
                outTime: null,
                parkingTime: null,
                repairStopTime: null,
                testStartTime: null,
                testStopTime: null,
                stantionId: 1
            }
    }

    handleChange = (newValue, field: string) => {

        var state = {}
        if (newValue && newValue.indexOf('Invalid dateT')>=0)
            newValue = null;
        state[field] = newValue;

        this.setState(state);
    }

    handleChangeAutocomplite = (newValue: SelectItem, field) => {

        if (field === 'trainId')
            this.props.getDispatcherInspectionByTrainId(newValue.value);

        var state = {}
        state[field] = newValue.value;

        this.setState(state);
    }

    get isValid() {
        const { trainId, parkingId, inTime } = this.state
        return checkToValue(trainId) && checkToValue(parkingId) && checkToValue(inTime);
    }

    save = () => {

        this.props.saveOrUpdateDispatcher(this.state, this.props.action);
    }

    getDate(date) {

        if (!date)
            date = new Date();

        return moment(date, 'YYYY-MM-DDTHH:mm:ss').format('DD.MM.YYYY')
    }

    getTime(time: string) {

        if (!time)
            return '00:00';

        var s = moment(time,'YYYY-MM-DDTHH:mm:ss').format('HH:mm')

        return s;
    }

    render() {

        const dataSource = this.props.data.result.depoEventDataSource;

        return (
            <div>
                <div className="dialog-body clear-x" style={{ width: 700 }}>
                    <div className="g-dialog-form g-dispetcher">
                        <AutocompliteDataSource
                            name="trainId"
                            title="Состав"
                            dataSource={dataSource.trains.filter(x => x.dependentId === this.state.stantionId)}
                            value={this.state.trainId}
                            handleChange={this.handleChangeAutocomplite} />

                        <AutocompliteDataSource
                            name="routeId"
                            title="Маршрут"
                            dataSource={dataSource.routes}
                            value={this.state.routeId}
                            handleChange={this.handleChangeAutocomplite} />


                        <AutocompliteDataSource
                            name="stantionId"
                            title="Депо"
                            dataSource={dataSource.stantions}
                            value={this.state.stantionId}
                            handleChange={this.handleChangeAutocomplite} />

                        <AutocompliteDataSource
                            name="parkingId"
                            title="Место постановки"
                            dataSource={dataSource.parkings.filter(x => x.dependentId === this.state.stantionId)}
                            value={this.state.parkingId}
                            handleChange={this.handleChangeAutocomplite} />

                        <DateTimePicker name="inTime" title="Время захода в депо" value={this.state.inTime} handleChange={this.handleChange} />
                        <DateTimePicker name="parkingTime" title="Время постановки на канаву" value={this.state.parkingTime} handleChange={this.handleChange} />
                        
                        <AutocompliteDataSource
                            name="userId"
                            title="Проверяющий"
                            dataSource={dataSource.users}
                            value={this.state.userId}
                            handleChange={this.handleChangeAutocomplite} />

                        {/*<DateTimePicker  name="testStartTime" title="Начало проверки под напряжением" value={this.state.testStartTime} handleChange={this.handleChange} />
                        <DateTimePicker  name="testStopTime" title="Окончания проверки под напряжением" value={this.state.testStopTime} handleChange={this.handleChange} />
                        */}
                        <DateTimePicker name="repairStopTime" title="Время окончания ремонта" value={this.state.repairStopTime} handleChange={this.handleChange} />
                        <DateTimePicker name="outTime" title="Время выхода из депо" value={this.state.outTime} handleChange={this.handleChange} />

                        {/*<AutocompliteDataSource
                            name="inspectionId"
                            title="Инспекция"
                            dataSource={this.props.inspections}
                            value={this.state.inspectionId}
                            handleChange={this.handleChangeAutocomplite} />*/}
                        <TextArea disabled={checkToValue(this.state.inspectionId)} title="Комментарий" height={120} name="inspectionTxt" value={this.state.inspectionTxt} handleChange={this.handleChange} />
                    </div>
                </div>
                <nav className="dialog-actions">
                    <Button disabled={!this.isValid} label="Сохранить" onClick={this.save} />
                    <Button label="Закрыть" onClick={() => this.props.toggleDialog(false)} />
                </nav>
            </div>
        )
    }
}

const provider = provide(
    (state: ApplicationState) => ({ data: (state.dialog.responseMessage as { result: DepoEventDtoUi, params: { routeName: string, trainName: string }}),inspections:state.dispetcher.inspections, action: state.dialog.action }),
    { ...DialogStore.actionCreators, ...store.actionCreators }
);

type Props = typeof provider.allProps;

export default provider.connect(DispatcherCreate);

interface DateTimePickerProps {
}

const DateTimePicker = ({ name, title, value, handleChange}) => {

    const getDate=(date) =>{

        if (!date) {
            return null;
        }

        return moment(date, 'YYYY-MM-DDTHH:mm:ss').format('DD.MM.YYYY')
    }

    const getTime=(time: string) =>{

        if (!time)
            return '00:00';

        var s = moment(time, 'YYYY-MM-DDTHH:mm:ss').format('HH:mm')

        return s;
    }

    const onChanhe = (newValue, field) => {

        var fieldName = field.split('_time'),
            field = fieldName[0];

        var dateCurrent = value ? value : new Date();

        var date = moment(dateCurrent, 'YYYY-MM-DDTHH:mm:ss');

        var val = fieldName.length === 1 ? moment(newValue, 'DD.MM.YYYY').format('YYYY-MM-DD') : date.format('YYYY-MM-DD');
        var time = fieldName.length > 1 ? newValue : date.format('HH:mm');

        newValue = `${val}T${time}:00`

        handleChange(newValue, field);
    }

    return <div className="date-time-picker clear-x">
        <DatePicker name={name} title={title} value={getDate(value)} handleChange={onChanhe}>
            <Input time isClear={false} name={name + '_time'} value={getTime(value)} handleChange={onChanhe} />
        </DatePicker>
    </div>
}