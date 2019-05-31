import * as React from 'react'
import { NavLink, Link } from 'react-router-dom'
import { RoutePaths } from '../../common'
import { ApplicationState } from '../../store'
import { provide } from 'redux-typed'
import { RequestEvent } from '../../common'
import { RouteComponentProps } from 'react-router-dom'
import ScheduleEvents from './ScheduleEvents'
import ScheduleBrigades from './ScheduleBrigades'
import { Redirect } from 'react-router-dom'

class Schedule extends React.Component<any, any> {
  render() {
    return (
      <div className="content layout horizontal">
        <div className="menu-column">
          <div className="left-menu card flex-none">
            <NavLink
              to={RoutePaths.scheduleEvents}
              activeClassName="left-menu-item selected"
              className="left-menu-item layout horizontal center"
            >
              Мероприятия
            </NavLink>

            <NavLink
              to={RoutePaths.scheduleBrigades}
              activeClassName="left-menu-item selected"
              className="left-menu-item layout horizontal center"
            >
              Бригады
            </NavLink>
          </div>
        </div>

        <div className="content-column flex layout vertical">
          {this.props.location.pathname === '/schedule' && <Redirect to={'/schedule/events'} />}
          {this.props.location.pathname === RoutePaths.scheduleEvents && <ScheduleEvents />}
          {this.props.location.pathname === RoutePaths.scheduleBrigades && <ScheduleBrigades />}
        </div>
      </div>
    )
  }
}

const provider = provide((state: ApplicationState) => ({ common: state.common }), undefined).withExternalProps<
  RouteComponentProps<any>
>()

type Props = typeof provider.allProps

export default provider.connect(Schedule)
