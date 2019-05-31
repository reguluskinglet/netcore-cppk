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
import { AutocompliteDataSource, DropDown } from '../../../../UI/dropdown'
import CheckBoxField from '../../../../UI/checkbox/CheckBoxField'
import moment from 'moment'
import * as store from '../store';
import { DayOfWeek } from '../../../Common'
import union from 'lodash/union';
import classnames from 'classnames'
import { StantionOnTrips } from './extensions'
import { checkToValue } from '../../../../UI/utils/utils'


interface State {
    showNewStantion?: boolean
    name?: string
    days?: number[]
    tripType?: boolean
    stantions?: TripStantion[]
}

interface Stantion {
    inTime: any
    outTime: any
    stantionId: number
}


class TripCreate extends React.Component<Props, State> {

    constructor(props: Props) {
        super(props)

        var data = props.data;

        this.state = {
            name: data.name,
            stantions: data.stantions,
            days: data.days,
            tripType: data.tripType,
            showNewStantion: !data.stantions.length
        }
    }

    getTime = (hover, minute, endTime?) => {
        if (hover < 10) {
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

        const { days, name, stantions, tripType } = this.state

        var id = this.props.data.id;

        var data;

        var tType = tripType === true ? 1 : 0;

        if (id !== null) {
            data = {
                TripWithDays: {  
                    id: id,
                    name: name,
                    tripType: tType
                }
            }
        }
        else
            data = {
                days: days,
                tripWithDateTimeStations: {
                    name: name,
                    tripType: tType,
                    stantionOnTripsWithStringTime: stantions.map(x => {
                        return {
                            stantionId: x.id,
                            inTime: x.inTime,
                            outTime: x.outTime
                        }
                    })
                }
            }

        this.props.addOrUpdateTripToRoute(data, id !== null, this.props.action)
    }

    getDate(time: string, day) {
        return moment(`0001-01-${day}T${time}:00+00:00`);
    }

    addNewStantion = () => {
        this.setState({
            showNewStantion: !this.state.showNewStantion,
        })

    }

    selectedDay = (day: number) => {

        if (this.isEdit)
            return;

        var days = [...this.state.days],
            index = days.indexOf(day);

        if (index >= 0)
            days.splice(index, 1)
        else
            days.push(day);

        this.setState({
            days: days
        })
    }

    onSaveStantion = (stantion) => {

        var curentStantions = [...this.state.stantions];
        
        curentStantions.push({
            id: stantion.stantionId,
            name: stantion.stantionName,
            inTime: stantion.start,
            outTime: stantion.end
        })

        this.setState({
            stantions: curentStantions,
            showNewStantion: false
        })
    }

    onRemoveStantion = (id) => {

        var curentStantions = [...this.state.stantions];

        remove(curentStantions, x => x.id === id);

        this.setState({
            stantions: curentStantions,
        })
    }

    get isValid() {
        return this.state.stantions.length >= 2 && this.state.days.length > 0 && checkToValue(this.state.name)
    }

    get isEdit() {
        return this.props.data.id !== null;
    }

    render() {
        const { name, stantions, days, tripType } = this.state;
        var stantionSelectItem = this.props.data.dataSource.stantions.filter(x => !stantions.find(s => s.id === x.value))

        return (
            <div>
                <div className="dialog-body clear-x" style={{ width: 700 }}>
                    <div className="g-dialog-form">
                        <Input title="Наименование" name="name" value={name} handleChange={this.handleChange} />
                        <CheckBoxField name="tripType" checked={this.state.tripType} title="Перегонный рейс" handleChange={this.handleChange} />
                        <div className="day-of-week-field clear-x" style={{ marginBottom: 8 }}>
                            <b>Дни недели маршрута</b>
                            <DayOfWeek selectedDays={this.state.days} onSelectedDay={this.selectedDay} />
                        </div>
                        <div className="g-trip" style={{ marginTop:8 }}>
                            {!this.isEdit && <Button label="Добавить станцию" onClick={this.addNewStantion} style={{ marginBottom: 8 }} />}
                            <div className="flex-grid">
                                <div className="flex-grid-header">
                                    <div className="item">Станция</div>
                                    <div className="item">Время прибытия</div>
                                    <div className="item">Время убытия</div>
                                </div>
                                <ScrollBarsVertical height={250} width={undefined}>
                                    <div className="flex-container" style={{ height: '100%' }}>
                                        {this.state.showNewStantion
                                            && <StantionOnTrips
                                            currentStantions={this.state.stantions}
                                            stantions={stantionSelectItem}
                                                onSave={this.onSaveStantion} />}
                                        {stantions.map(item => <Td key={item.id} item={item} onRemove={this.onRemoveStantion} showRemove={!this.isEdit && !this.state.showNewStantion} />)}
                                    </div>
                                </ScrollBarsVertical>
                            </div>
                        </div>
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
    (state: ApplicationState) => ({ data: (state.dialog.responseMessage as TripUi), action: state.dialog.action }),
    { ...DialogStore.actionCreators, ...store.actionCreators }
);

type Props = typeof provider.allProps;

export default provider.connect(TripCreate);

const Td = ({ item, onRemove, showRemove }: { item: TripStantion, showRemove: boolean, onRemove: (id) => void }) => {

    var className = classnames('flex-row')

    return <div className={className}>
        <div className="item">{item.name}</div>
        <div className="item">{moment(item.inTime,'YYYY-MM-DDTHH:mm:ss').format('HH:mm')}</div>
        <div className="item">
            {moment(item.outTime, 'YYYY-MM-DDTHH:mm:ss').format('HH:mm')}
            {showRemove && <div className="g-item-remove" title="УДАЛИТЬ" onClick={() => onRemove(item.id)}><span className="icon-remov" /></div>}
        </div>
    </div>
}