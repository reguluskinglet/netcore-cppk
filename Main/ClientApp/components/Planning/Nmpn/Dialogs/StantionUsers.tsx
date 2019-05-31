import * as React from 'react'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../../../store';
//import * as InvItemState from '../../store/InvItem';

import * as DialogStore from '../../../../store/DialogStore';
import * as GridStore from '../../../../store/GridStore';
import { DialogForm } from '../../../../UI/dialog'
import { Button } from '../../../../UI/button'

import * as remove from 'lodash/remove'
import * as union from 'lodash/union'
import { Input } from '../../../../UI/input'
import { DayOfWeek } from '../../../Common'
import { AutocompliteDataSource } from '../../../../UI/dropdown'
import { checkToValue } from '../../../../UI/utils/utils'
import * as store from '../store'
import { ScrollBarsVertical } from '../../../../UI/scrollbars'

interface State {
    stantionUsers?: StantionUser[],
    showAddUser?: boolean
    removeUserIds?:[]

    currentSelectedUserId?: number
    currentSelectedStartStantionId?: number
    currentSelectedEndStantionId?: number
}

interface ExternalProps {
    users: SelectItem[]
    date: string
    planedRouteTrainId: number
    onChangeUsers: (stantionUsers: StantionUser[], removeUserId?: number) => void

    stantionUsers: StantionUser[]
}

class StantionUsers extends React.Component<Props, State> {

    constructor(props: Props) {
        super(props)

        this.state = {
            stantionUsers: props.stantionUsers,
            removeUserIds:[],
            showAddUser: props.stantionUsers.length === 0
        }
    }


    addUserByStantion = () => {
        this.setState({
            showAddUser: !this.state.showAddUser
        })
    }

    handleChange = (newValue: SelectItem, field) => {

        var state = {}
        state[field] = newValue.value;

        if (field === 'currentSelectedStartStantionId')
            this.props.getOutputStation(this.state.currentSelectedUserId, this.props.date, this.props.planedRouteTrainId, newValue.value);
        else if (field === 'currentSelectedUserId')
            this.props.getInputStation(newValue.value, this.props.date, this.props.planedRouteTrainId);

        this.setState(state);
    }

    save = () => {
        var stantionUsers = [...this.state.stantionUsers]

        const { currentSelectedEndStantionId, currentSelectedUserId, currentSelectedStartStantionId } = this.state
        const { stantionsEnd, stantionsStart, users } = this.props

        var start = stantionsStart.find(x => x.value === currentSelectedStartStantionId),
            end = stantionsEnd.find(x => x.value === currentSelectedEndStantionId),
            user = users.find(x => x.value === currentSelectedUserId);


        var stantion: StantionUser = stantionUsers.find(x => x.stantionStart.value === currentSelectedStartStantionId && x.stantionEnd.value === currentSelectedEndStantionId);

        if (!stantion) {
            stantion = {
                stantionStart: start,
                stantionEnd: end,
                users: [user]
            }

            stantionUsers.push(stantion)
        }
        else {

            var userStantion = stantion.users.find(x => x.value === currentSelectedUserId)

            if (!userStantion)
                stantion.users.push(user)
        }
        

        this.setState({
            stantionUsers: stantionUsers,
            showAddUser: false,
            currentSelectedUserId: undefined
        })

        this.props.onChangeUsers(stantionUsers)
    }

    get isValid() {

        const { currentSelectedUserId, currentSelectedEndStantionId, currentSelectedStartStantionId } = this.state

        return checkToValue(currentSelectedUserId) && checkToValue(currentSelectedEndStantionId) && checkToValue(currentSelectedStartStantionId) 
    }

    onRemoveUser = (item: StantionUser, user: SelectItemPlaneBrigadeTrain) => {


        var stantionUsers = [...this.state.stantionUsers]

        var stantion: StantionUser = stantionUsers.find(x => x.stantionStart.value === item.stantionStart.value && x.stantionEnd.value === item.stantionEnd.value);

        remove(stantion.users, x => x.value === user.value)

        if (stantion.users.length === 0)
            remove(stantionUsers, x => x.stantionStart.value === item.stantionStart.value && x.stantionEnd.value === item.stantionEnd.value)

        this.setState({
            stantionUsers: stantionUsers
        })

        this.props.onChangeUsers(stantionUsers, user.planeBrigadeTrainsId)
    }

    render() {

        var usersDataSource = this.props.users;
        const { currentSelectedEndStantionId, currentSelectedUserId, currentSelectedStartStantionId, stantionUsers } = this.state
        var stantion: StantionUser = stantionUsers.find(x => x.stantionStart.value === currentSelectedStartStantionId && x.stantionEnd.value === currentSelectedEndStantionId);

        if (stantion)
            usersDataSource = usersDataSource.filter(x => stantion.users.indexOf(x) < 0)

        return <div className="main-stantion-users">
            <Button label="Добавить сотрудника на станцию" onClick={this.addUserByStantion} style={{ marginBottom:10}} />

            <div className="g-dialog-form">
                <div className="g-trip">
                    <div className="flex-grid">
                        <div className="flex-grid-header">
                            <div className="item" style={{ paddingLeft: 5 }}>Станция</div>
                            <div className="item">Сотрудники</div>
                        </div>
                        <ScrollBarsVertical height={250} width={undefined}>
                            <div className="flex-container" style={{ height: '100%' }}>
                                {this.state.showAddUser && <div className="g-data">
                                    <div className="g-body">
                                        <AutocompliteDataSource handleChange={this.handleChange} title="Сотрудник" name="currentSelectedUserId" value={this.state.currentSelectedUserId} dataSource={usersDataSource} />

                                        <AutocompliteDataSource handleChange={this.handleChange} title="Станция отправления" name="currentSelectedStartStantionId" value={this.state.currentSelectedStartStantionId} dataSource={this.props.stantionsStart} />
                                        <AutocompliteDataSource handleChange={this.handleChange} title="Станция прибытия" name="currentSelectedEndStantionId" value={this.state.currentSelectedEndStantionId} dataSource={this.props.stantionsEnd} />
                                    </div>
                                    <div className="g-data-action">
                                        <Button label="Сохранить" onClick={this.save} disabled={!this.isValid} />
                                    </div>
                                </div>}
                                {this.state.stantionUsers.map((item, i) => <Td key={i} item={item} onRemove={this.onRemoveUser} />)}
                            </div>
                        </ScrollBarsVertical>
                    </div>
                </div>
            </div>
        </div>
    }
}

const provider = provide(
    (state: ApplicationState) => state.npmn,
    { ...DialogStore.actionCreators, ...store.actionCreators }
).withExternalProps<ExternalProps>();

type Props = typeof provider.allProps;

export default provider.connect(StantionUsers);


const Td = ({ item, onRemove }: { item: StantionUser, onRemove?: (item, userId) => void }) => {

    return <div className="flex-row">
        <div className="item" style={{ paddingLeft:5 }}>{item.stantionStart.text} → {item.stantionEnd.text}</div>
        <div className="item">
            {item.users.map(x => <div className="g-user">{x.text}<span onClick={() => onRemove(item, x)} className="icon-remov" title="Удалить"></span></div>)}
        </div>
    </div>
}