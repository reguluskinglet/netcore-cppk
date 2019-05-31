import * as React from 'react'
import { NavLink, Link } from 'react-router-dom'
import { RoutePaths } from '../../common'
import Equipment from '../Equipment'
import Fails from '../Fails'
import Directions from '../Directions'
import Stations from '../Stations'
import Brigades from '../Groups/Brigades'
import Employees from '../Groups/Employees'
import Models from '../Models'
import Trains from '../Trains'
import Locations from '../Locations'
import Templates from '../Templates'
import { ApplicationState } from '../../store'
import { provide } from 'redux-typed'
import { RequestEvent } from '../../common'
import { RouteComponentProps } from 'react-router-dom'
import { Redirect } from 'react-router-dom'
import Parking from '../Parking'
import Routes from '../Routes'
import FailsMD from '../FailsMD'

class Dicts extends React.Component<any, any> {
    render() {
        return (
            <div className="content layout horizontal">
                <div className="menu-column">
                    <div className="left-menu card flex-none">
                        <NavLink
                            to={RoutePaths.trains}
                            activeClassName="left-menu-item selected"
                            className="left-menu-item layout horizontal center"
                        >
                            <div className="icon-train margin-icon" /> <div>Составы</div>
                        </NavLink>
                        <NavLink
                            to={RoutePaths.stations}
                            activeClassName="left-menu-item selected"
                            className="left-menu-item layout horizontal center"
                        >
                            <span className="icon-store margin-icon" />Станции
            </NavLink>
                        <NavLink
                            to={RoutePaths.brigades}
                            activeClassName="left-menu-item selected"
                            className="left-menu-item layout horizontal center"
                        >
                            <span className="icon-users margin-icon" />Бригады
            </NavLink>
                        <NavLink
                            to={RoutePaths.employees}
                            activeClassName="left-menu-item selected"
                            className="left-menu-item layout horizontal center"
                        >
                            <span className="icon-user-tie margin-icon" />Сотрудники
            </NavLink>
                        <NavLink
                            to={RoutePaths.models}
                            activeClassName="left-menu-item selected"
                            className="left-menu-item layout horizontal center"
                        >
                            <span className="icon-train-1 margin-icon" />Модели
            </NavLink>
                        <NavLink
                            to={RoutePaths.equipment}
                            activeClassName="left-menu-item selected"
                            className="left-menu-item layout horizontal center"
                        >
                            <span className="icon-settings margin-icon" />Оборудование
            </NavLink>
                        <NavLink
                            to={RoutePaths.fails}
                            activeClassName="left-menu-item selected"
                            className="left-menu-item layout horizontal center"
                        >
                            <span className="icon-fails margin-icon" />Неисправности
            </NavLink>

                        <NavLink
                            to={RoutePaths.directions}
                            activeClassName="left-menu-item selected"
                            className="left-menu-item layout horizontal center"
                        >
                            <span className="icon-compass margin-icon" />Направления
            </NavLink>

                        <NavLink
                            to={'/dicts/parking'}
                            activeClassName="left-menu-item selected"
                            className="left-menu-item layout horizontal center"
                        >
                            <span className="icon-route margin-icon" />Места постановки
            </NavLink>

                        {/* <NavLink
              to={'/dicts/routes'}
              activeClassName="left-menu-item selected"
              className="left-menu-item layout horizontal center"
            >
              <span className="directions-icon margin-icon" />Рейсы
            </NavLink> */}

                        <NavLink
                            to={'/dicts/failsmd'}
                            activeClassName="left-menu-item selected"
                            className="left-menu-item layout horizontal center"
                        >
                            <span className="icon-fails margin-icon" />Неисправности МУ
            </NavLink>

                        {/* <NavLink
              to={`/dicts/templates`}
              activeClassName="left-menu-item selected"
              className="left-menu-item layout horizontal center"
            >
              <span className="icon-fails margin-icon" />ZPL шаблоны
            </NavLink> */}
                    </div>
                </div>

                <div className="content-column flex layout vertical">
                    {this.props.location.pathname === '/dicts' && (
                        <Redirect to={'/dicts/trains'} />
                    )}
                    {this.props.location.pathname === '/dicts/equipment' && <Equipment />}
                    {this.props.location.pathname === '/dicts/fails' && <Fails />}
                    {this.props.location.pathname === '/dicts/directions' && (
                        <Directions />
                    )}
                    {this.props.location.pathname === '/dicts/stations' && <Stations />}
                    {this.props.location.pathname === '/dicts/parking' && <Parking />}
                    {this.props.location.pathname === '/dicts/failsmd' && <FailsMD />}
                    {this.props.location.pathname === '/dicts/routes' && <Routes />}
                    {this.props.location.pathname === '/dicts/brigades' && <Brigades />}
                    {this.props.location.pathname === '/dicts/employees' && <Employees />}
                    {this.props.location.pathname === '/dicts/models' && <Models />}
                    {this.props.location.pathname === '/dicts/trains' && <Trains />}
                    {/* {this.props.location.pathname === '/dicts/templates' && <Templates />} */}
                    {this.props.match.params &&
                        this.props.match.params.id &&
                        this.props.match.params.innerDict === 'locations' && (
                            <Locations
                                match={this.props.match}
                                location={this.props.location}
                                history={this.props.history}
                            /> //modelId={parseInt(this.props.match.params.id)} />
                        )}
                </div>
            </div>
        )
    }
}

const provider = provide(
    (state: ApplicationState) => ({ common: state.common }),
    undefined
).withExternalProps<RouteComponentProps<any>>()

type Props = typeof provider.allProps

export default provider.connect(Dicts)
