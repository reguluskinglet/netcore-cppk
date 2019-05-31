import * as React from 'react'
import { NavLink, Link } from 'react-router-dom'
import { RoutePaths } from '../../common'
import { ApplicationState } from '../../store'
import { provide } from 'redux-typed'
import { RequestEvent } from '../../common'
import { RouteComponentProps } from 'react-router-dom'
import { Redirect } from 'react-router-dom'
import Templates from '../Templates'
import Monitors from '../Monitors'
import Give from '../Give'
import EditGive from '../Give/EditGive'
import Service from '../Service'
import EditService from '../Service/EditService'
import Tags from './Tags'
import Tag from './Tag'
import Users from './Users'
import MobileDevices from './MobileDevices'

class Administration extends React.Component<any, any> {
  render() {
    return (
      <div className="content layout horizontal">
        <div className="menu-column">
          <div className="left-menu card flex-none">
            <NavLink
              to={'/tags/eq'}
              activeClassName="left-menu-item selected"
              className="left-menu-item layout horizontal center"
            >
              <div>Оборудование</div>
            </NavLink>

            <NavLink
              to={'/tags/users'}
              activeClassName="left-menu-item selected"
              className="left-menu-item layout horizontal center"
            >
              Сотрудники
            </NavLink>

            <NavLink
              to={`/tags/md`}
              activeClassName="left-menu-item selected"
              className="left-menu-item layout horizontal center"
            >
              Мобильные устройства
            </NavLink>
          </div>
        </div>

        <div className="content-column flex layout vertical">
          {this.props.location.pathname === '/tags' && (
            <Redirect to={'/tags/eq'} />
          )}
          {this.props.location.pathname === '/tags/eq' && (
            <Tags
              match={this.props.match}
              location={this.props.location}
              history={this.props.history}
            />
          )}
          {this.props.location.pathname.includes('/tags/eq/') && (
            <Tag
              match={this.props.match}
              location={this.props.location}
              history={this.props.history}
            />
          )}
          {this.props.location.pathname === '/tags/users' && <Users />}
          {this.props.location.pathname === '/tags/md' && <MobileDevices />}
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
