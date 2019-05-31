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

interface Data {
    checkListType: TimelineTypeEnum
    routeId: number
    hover: number
}

interface State {
    users: TimeRangeBrigadeUsers[]
}

class BrigadeDialog extends React.Component<Props, State> {

    constructor(props: Props) {
        super(props)

        var data = props.data;

        this.state = {
            users: data.users,
        }
    }

    handleChange = (index, value, field) => {

        var users = [...this.state.users];

        users[index][field] = value;

        this.setState({
            users: users
        })
    }

    save = () => {

        var result = this.state.users.map(x => {
            return {
                id: x.id,
                startId: x.startId,
                endId: x.endId,
                userId: x.userId,
                planeBrigadeTrainId: x.planeBrigadeTrainId,
                canseled: x.canseled,
            }
        });

        this.props.changeTimeLine(result, this.props.timeLineType, this.props.action)
    }

    renderThumb({ style, ...props }) {
 
        return (
            <div {...props} style={style} />
        );
    }

    render() {

        var users = this.state.users;

        var dataSourses = this.props.data.dataSource;

        return (
            <div>
                <div className="dialog-body clear-x" style={{ width: 599 }}>
                    <Scrollbars style={{ width: 607, height: 400 }}
                        renderTrackHorizontal={props => <div {...props} style={{ display: 'none' }} className="track-horizontal" />}
                        renderThumbVertical={({ style, ...props }: any) => (
                            <div style={{ ...style, width: 4,cursor: 'pointer', backgroundColor: '#0077977f'}}{...props} />
                        )}
                    >
                        {users.map((item, index) => <div className="g-main-block clear-x">
                            <div className="g-block">
                                <h4 className="g-h-title g-pink">План</h4>
                                <ReadOnlyField title="ФИО" value={item.user} />
                                <ReadOnlyField title="Станция посадки" value={item.start} />
                                <ReadOnlyField title="Станция высадки" value={item.end} />
                            </div>
                            <div className="g-block">
                                <h4 className="g-h-title g-orange">Факт</h4>
                                <AutocompliteDataSource dataSource={dataSourses.users}
                                    name="userId"
                                    value={item.userId}
                                    handleChange={(newValue, field) => this.handleChange(index, newValue.value, field)} />
                                <AutocompliteDataSource dataSource={dataSourses.stantions}
                                    name="startId"
                                    value={item.startId}
                                    handleChange={(newValue, field) => this.handleChange(index, newValue.value, field)} />
                                <AutocompliteDataSource dataSource={dataSourses.stantions}
                                    name="endId"
                                    value={item.endId}
                                    handleChange={(newValue, field) => this.handleChange(index, newValue.value, field)} />
                                <CheckBoxField className="g-check" fieldTitleWidth={115} invert name="canseled" title="Отменен" checked={item.canseled}
                                    handleChange={(newValue, field) => this.handleChange(index, newValue, field)} />
                            </div>
                            </div>)}
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
    (state: ApplicationState) => ({ data: (state.dialog.responseMessage.result as TimeRangeBrigade), timeLineType: (state.dialog.responseMessage.timeLineType as TimelineTypeEnum), action: state.dialog.action }),
    { ...DialogStore.actionCreators, ...store.actionCreators }
);

type Props = typeof provider.allProps;

export default provider.connect(BrigadeDialog);
