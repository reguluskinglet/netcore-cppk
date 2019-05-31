import * as React from 'react'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../../../store';
//import * as InvItemState from '../../store/InvItem';

import * as DialogStore from '../../../../store/DialogStore';
import * as GridStore from '../../../../store/GridStore';
import { DialogForm } from '../../../../UI/dialog'
import { Button } from '../../../../UI/button'

import * as remove from 'lodash/remove'
import { Input } from '../../../../UI/input'
import { DayOfWeek } from '../../../Common'
import { AutocompliteDataSource } from '../../../../UI/dropdown'
import { checkToValue } from '../../../../UI/utils/utils'
import StantionUsers from './StantionUsers'
import * as store from '../store'
import moment from 'moment'


interface State {
    trainId?: number,
    users?:any[]
}


class TurnoverCreate extends React.Component<Props, State> {

    constructor(props: Props) {
        super(props)


        var result = props.data;

        var state = {
            users: []
        }
       
        if (result.dayDate.train)
            state['trainId'] = result.dayDate.train.id

        var users = result.dayDate.users;

        if (users && result.dayDate.users.length) {

            var stantionUsers: StantionUser[] = [];

            for (var i = 0; i < users.length; i++) {

                var user = users[i],
                    userId = user.name,
                    userSelect: SelectItemPlaneBrigadeTrain = { value: userId, planeBrigadeTrainsId: user.planeBrigadeTrainsId, text: user.name },
                    stantionStartId = user.userStations.inputName,
                    stantionEndId = user.userStations.outputName;


                var stantion = stantionUsers.find(x => x.stantionStart.value === stantionStartId
                    && x.stantionEnd.value === stantionEndId);


                if (!stantion) {
                    stantion = {
                        stantionStart: { value: stantionStartId, text: this.getStantionName(user.userStations.inputName, user.userStations.inputTime) },
                        stantionEnd: { value: stantionEndId, text: this.getStantionName(user.userStations.outputName, user.userStations.outputTime) },
                        users: [userSelect]
                    }

                    stantionUsers.push(stantion)
                }
                else {

                    var userStantion = stantion.users.find(x => x.value === userId)

                    if (!userStantion)
                        stantion.users.push(userSelect)
                }

            }

            this.currentStantionUsers = stantionUsers
        }

        this.state = state;
    }

    componentWillUnmount = () => {
        this.props.nmpnUnloadState();
    }

    getStantionName = (name: string, dateTime: string) => {

        return `${name} (${moment(dateTime, 'YYYY-MM-DDTHH:mm:ss').format('HH:mm')})`;
    }

    handleChange = (newValue, field) => {

        var state = {}
        state[field] = newValue;

        this.setState(state);
    }



    get isValid() {

        //const { days, name, directionId } = this.state

        return false;
    }

    save = () => {

        const { date, route } = this.props.data

        this.props.addPlaneBrigadeTrain(date, this.props.source.planedRouteTrainId, this.newStantionUsers, this.removeUserIds, this.props.action);
    }

    saveTrainToRoute = () => {

        const { date, route } = this.props.data

        this.props.saveTrainToRoute(date, this.state.trainId, route.id, this.props.action);
    }

    newStantionUsers = []
    currentStantionUsers = []
    removeUserIds=[]

    changeUsers = (stantions: StantionUser[], removeUserId?: number) => {
        this.newStantionUsers = stantions;

        if (removeUserId)
            this.removeUserIds.push(removeUserId);
    }

    render() {

        var { users, trains, date } = this.props.data;
        const { planedRouteTrainId } = this.props.source;

        return (
            <div>
                <div className="dialog-body clear-x" style={{ width: 700 }}>
                    <div className="g-dialog-form">
                        <AutocompliteDataSource
                            name="trainId"
                            title="Поезд"
                            dataSource={trains}
                            value={this.state.trainId}
                            readonly={planedRouteTrainId !== undefined}
                            handleChange={(n, f) => this.handleChange(n.value, f)} />
                        {planedRouteTrainId && <Button style={{
                            position: 'absolute',
                            right: 25,
                            top: 58
                        }} red label="Убрать поезд с маршрута" onClick={() => this.props.removeTrains(planedRouteTrainId, this.props.action)} />}
                        {planedRouteTrainId === undefined
                            ? <p>Для добавления сотрудников на станцию необходимо добавить поезд на маршрут</p>
                            : <StantionUsers stantionUsers={this.currentStantionUsers} onChangeUsers={this.changeUsers} users={users} date={date} planedRouteTrainId={planedRouteTrainId} />}
                    </div>
                </div>
                <nav className="dialog-actions">
                    {planedRouteTrainId === undefined
                        ? <Button disabled={!this.state.trainId} label="Добавить поезд на маршрут" onClick={this.saveTrainToRoute} />
                        : <Button label="Сохранить" onClick={this.save} />}
                    <Button label="Закрыть" onClick={() => this.props.toggleDialog(false)} />
                </nav>
            </div>
        )
    }
}

const provider = provide(
    (state: ApplicationState) => ({
        source: state.npmn,
        data: (state.dialog.responseMessage as {
            users: SelectItem[]
            trains: SelectItem[]
            date: string,
            dayDate: NmpnDaysData,
            route: NmpnRoute
        }),
        action: state.dialog.action
    }),
    { ...DialogStore.actionCreators, ...store.actionCreators }
);

type Props = typeof provider.allProps;

export default provider.connect(TurnoverCreate);



//interface AddStantionAndUserProps {
//}
//const AddStantionAndUser = ({ users }: { users: SelectItem[] }) => {

//}

//interface StantionUsersProps {
//    stantionName: string
//    users: any[]
//}

//const StantionUsers = () => {

//    return <div className="g-stantion-users">
//        <Button label="Добавить сотрудника" onClick={this.addUser} />
//        <div></div>
//    </div>
//}
