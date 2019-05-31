import * as React from 'react'
import { NavLink, Link } from 'react-router-dom'
import { RoutePaths } from '../../common'
import { ApplicationState } from '../../store'
import { provide } from 'redux-typed'
import { RouteComponentProps, Switch, Route, Redirect} from 'react-router-dom'
import Goep from './Goep'
import EditGoep from './Goep/Graph'
import Dispatcher from './Dispatcher'
import Nmpn from './Nmpn'
import Pou from './Pou'
import Routes from './Routes'

class Planning extends React.Component<any, any> {

    state = {
        collapseMenu: false
    }

    render() {
        return (
            <div>
                <div className="g-nav">
                    <NavLink to={RoutePaths.goep} activeClassName="active">График оборота поездов</NavLink>
                    <NavLink to={RoutePaths.nmpn} activeClassName="active">Назначение поездов на маршруты</NavLink>
                    <NavLink to={RoutePaths.pou} activeClassName="active">Планово-оперативное управление</NavLink>
                    <NavLink to={RoutePaths.dispatcher} activeClassName="active">Ремонт в депо</NavLink>
                    <NavLink to={RoutePaths.routes} activeClassName="active">Рейсы</NavLink>
                </div>
                <div className="content-column flex layout vertical">
                    <Switch>
                        <Route exact path={RoutePaths.routes} component={Routes} />
                        <Route exact path={RoutePaths.dispatcher} component={Dispatcher} />
                        <Route exact path={RoutePaths.pou} component={Pou} />
                        <Route exact path={RoutePaths.nmpn} component={Nmpn} />
                        <Route exact path={RoutePaths.goep} component={Goep} />
                        <Route path='/planning/goep/:id' component={EditGoep} />
                        <Redirect path='/planning' exact to={'/planning/goep'} />
                    </Switch>
                </div>
            </div>
        )
    }
}

const provider = provide((state: ApplicationState) => ({ common: state.common }), undefined).withExternalProps<
  RouteComponentProps<any>
>()

type Props = typeof provider.allProps

export default provider.connect(Planning)
