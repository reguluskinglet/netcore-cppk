import * as React from 'react'
import { NavLink, Link } from 'react-router-dom'
import { RoutePaths } from '../../common'
import { ApplicationState } from '../../store'
import { provide } from 'redux-typed'
import { RequestEvent } from '../../common'
import { RouteComponentProps } from 'react-router-dom'
import Users from './Users'
import Roles from './Roles'
import { Redirect } from 'react-router-dom'
import Templates from '../Templates'
import Monitors from '../Monitors'
import Give from '../Give'
import EditGive from '../Give/EditGive'
import Service from '../Service'
import EditService from '../Service/EditService'

class Administration extends React.Component<any, any> {
  render() {
    return (
      <div className="content layout horizontal">
        <div className="menu-column">
          <div className="left-menu card flex-none">
            <NavLink
              to={RoutePaths.users}
              activeClassName="left-menu-item selected"
              className="left-menu-item layout horizontal center"
            >
              <div>Пользователи</div>
            </NavLink>
            <NavLink
              to={RoutePaths.roles}
              activeClassName="left-menu-item selected"
              className="left-menu-item layout horizontal center"
            >
              Роли
            </NavLink>
            <NavLink
              to={`/administration/templates`}
              activeClassName="left-menu-item selected"
              className="left-menu-item layout horizontal center"
            >
              ZPL шаблоны
            </NavLink>
            <NavLink
              to={`/administration/monitors`}
              activeClassName="left-menu-item selected"
              className="left-menu-item layout horizontal center"
            >
              Управление ИП
            </NavLink>

            <NavLink
              to={`/administration/give`}
              activeClassName="left-menu-item selected"
              className="left-menu-item layout horizontal center"
            >
              Выдача устройств
            </NavLink>

            <NavLink
              to={`/administration/service`}
              activeClassName="left-menu-item selected"
              className="left-menu-item layout horizontal center"
            >
              Сервисный раздел
            </NavLink>
          </div>
        </div>

        <div className="content-column flex layout vertical">
          {this.props.location.pathname === '/administration' && <Redirect to={'/administration/users'} />}
          {this.props.location.pathname === '/administration/users' && <Users />}
          {this.props.location.pathname === '/administration/roles' && <Roles />}
          {this.props.location.pathname === '/administration/templates' && <Templates />}
          {this.props.location.pathname === '/administration/monitors' && <Monitors />}
          {this.props.location.pathname === '/administration/give' && <Give />}
          {this.props.location.pathname.includes('/administration/give/') && (
            <EditGive match={this.props.match} location={this.props.location} history={this.props.history} />
          )}
          {this.props.location.pathname === '/administration/service' && <Service />}
          {this.props.location.pathname.includes('/administration/service/') && (
            <EditService match={this.props.match} location={this.props.location} history={this.props.history} />
          )}
        </div>
      </div>
    )
  }
}

const provider = provide(
  (state: ApplicationState) => ({ ...state.incident, common: state.common }),
  undefined
).withExternalProps<RouteComponentProps<any>>()

type Props = typeof provider.allProps

export default provider.connect(Administration)
