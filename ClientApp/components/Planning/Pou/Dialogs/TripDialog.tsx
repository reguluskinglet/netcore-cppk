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
import Scrollbars from 'react-custom-scrollbars';
import { AutocompliteDataSource } from '../../../../UI/dropdown'
import * as classnames from 'classnames'

interface Data {
    checkListType: TimelineTypeEnum
    routeId: number
    hover: number
}

interface State {
    stantions: TimeRangeTipStantions[]
    isAllCanseled: boolean
}

class BrigadeDialog extends React.Component<Props, State> {

    constructor(props: Props) {
        super(props)

        var data = props.data;

        this.state = {
            stantions: data.stantions,
            isAllCanseled: this.isAllCanseled(data.stantions)
        }
    }

    handleChange = (index, value, field) => {

        var stantions = [...this.state.stantions];

        stantions[index][field] = value;

        this.setState({
            stantions: stantions,
            isAllCanseled: this.isAllCanseled(stantions)
        })
    }

    isAllCanseled = (stantions: TimeRangeTipStantions[]) => {
        return stantions.every(x => x.canceled === true);
    }

    get canselBtnText(): string {
        return this.state.isAllCanseled ? 'Возобновить все станции' : 'Отменить все станции' ;
    }

    save = () => {

        if (this.isValid) {

            var result = this.state.stantions.map(x => {
                return {
                    trainId: x.trainId,
                    startFact: x.startFact,
                    endFact: x.endFact,
                    canceled: x.canceled,
                    planeStationOnTripId: x.planeStationOnTripId
                }
            });

            this.props.changeTimeLine(result, this.props.timeLineType, this.props.action)
        }
    }

    get isValid(): boolean {

        for (var i = 0; i < this.state.stantions.length; i++) {
            var stantion = this.state.stantions[i];

            const isBefore = moment(stantion.startFact).isBefore(moment(stantion.endFact))

            if (!isBefore)
                return false;

        }

        return true;
    }

    renderThumb({ style, ...props }) {

        return (
            <div {...props} style={style} />
        );
    }

    getDate(time: string) {
        return moment(`0001-01-01T${time}:00`).format('YYYY-MM-DDTHH:mm:ss')+'Z';
    }

    canselAllStantions = () => {
        var stantions = [...this.state.stantions],
            isAllCanseled = !this.state.isAllCanseled;


        for (var i = 0; i < this.state.stantions.length; i++) {
            var stantion = this.state.stantions[i];

            stantion.canceled = isAllCanseled;
        }

        this.setState({
            stantions: stantions,
            isAllCanseled: isAllCanseled
        })
    }

    render() {

        var stantios = this.state.stantions;

        var dataSourses = this.props.data.dataSource;
        var item = stantios[0];

        return (
            <div>
                <div className="dialog-body clear-x" style={{ width: 599 }}>
                    <Scrollbars style={{ width: 607, height: 400 }}
                        renderTrackHorizontal={props => <div {...props} style={{ display: 'none' }} className="track-horizontal" />}
                        renderThumbVertical={({ style, ...props }: any) => (
                            <div style={{ ...style, width: 4, cursor: 'pointer', backgroundColor: '#0077977f' }}{...props} />
                        )}
                    >
                        <div style={{ width: 589 }}>
                            <h4 className="g-h-title g-title-trip">Рейс {this.props.data.trip}
                                <Button label={this.canselBtnText} onClick={this.canselAllStantions} red={!this.state.isAllCanseled} />
                            </h4>
                            {stantios.map((item, index) => <div className={classnames('g-main-block clear-x', {
                                'red': item.canceled
                            })}>
                                <h4 className="g-h-title g-blue">Станция {item.name}</h4>
                                <div className="g-block first">
                                    <h4 className="g-h-title g-pink">План</h4>
                                    <ReadOnlyField title="Поезд" value={item.train} />
                                    <ReadOnlyField title="Приход" value={item.startPlan} />
                                    <ReadOnlyField title="Уход" value={item.endPlan} />
                                </div>
                                <div className="g-block">
                                    <h4 className="g-h-title g-orange">Факт</h4>
                                    <AutocompliteDataSource dataSource={dataSourses.trains}
                                        name="trainId"
                                        value={item.trainId}
                                        handleChange={(newValue, field) => this.handleChange(index, newValue.value, field)} />

                                    <Input time isClear={false}
                                        name="startFact"
                                        className="g-pading"
                                        value={moment(item.startFact, 'YYYY-MM-DDTHH:mm:ss').format('HH:mm')}
                                        handleChange={(newValue, field) => this.handleChange(index, this.getDate(newValue), field)} />
                                    <Input time isClear={false}
                                        name="endFact"
                                        className="g-pading"
                                        value={moment(item.endFact, 'YYYY-MM-DDTHH:mm:ss').format('HH:mm')}
                                        handleChange={(newValue, field) => this.handleChange(index, this.getDate(newValue), field)} />

                                    <CheckBoxField className="g-check" fieldTitleWidth={115} invert name="canceled" title="Отменить станцию" checked={item.canceled}
                                        handleChange={(newValue, field) => this.handleChange(index, newValue, field)} />
                                </div>
                            </div>)}
                        </div>
                    </Scrollbars>
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
    (state: ApplicationState) => ({ data: (state.dialog.responseMessage.result as TimeRangeTrip), timeLineType: (state.dialog.responseMessage.timeLineType as TimelineTypeEnum), action: state.dialog.action }),
    { ...DialogStore.actionCreators, ...store.actionCreators }
);

type Props = typeof provider.allProps;

export default provider.connect(BrigadeDialog);
