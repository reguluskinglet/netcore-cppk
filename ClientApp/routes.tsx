import * as React from 'react'
import { Route, Redirect, Switch, RouteComponentProps } from 'react-router-dom'
import { PrivateLayout } from './components/Layout'
import {
  Home,
  Tasks,
  Log,
  Incident,
  Login,
  Dicts,
  Task,
  //Journals,
  Reports,
  TaskCreate,
  Administration,
  Schedule,
  Tags,
  Tag,
  Templates
} from './components'

import Journal from './components/Journals'

import { UserService } from './services'
import { RoutePaths, AuthComponent } from './common'
import Planning from './components/Planning'
import Give from './components/Give'
import Parking from './components/Parking'

const Private = () => {
  return (
    <PrivateLayout>
      <Switch>
        <Route exact path={RoutePaths.home} render={props => <Redirect to={'/journals'} />} />
        {/* <Route exact path={RoutePaths.tasks} component={Tasks} /> */}
        {/* <Route exact path={`/tasks/create/new`} component={TaskCreate} /> */}
        {/* <Route exact path={`/tasks/:id/:attrId`} component={Task} /> */}
        <Route exact path={`${RoutePaths.dicts}/:dict/:id/:innerDict`} component={Dicts} />
        <Route path={RoutePaths.dicts} component={Dicts} />
        <Route exact path={`/journals`} component={Journal} />
        <Route exact path={RoutePaths.analyze} component={Reports} />
        <Route path={`/administration/:part?/:id?`} component={Administration} />
        {/* <Route path={`/administration/:part`} component={Administration} /> */}
        <Route path={RoutePaths.schedule} component={Schedule} />
        <Route path={'/tags/:type?/:id?'} component={Tags} />
        <Route exact path={'/planning/goep/:id'} component={Planning} />
        <Route path={RoutePaths.planning} component={Planning} />
      </Switch>
    </PrivateLayout>
  )
}

export const routes = (
  <Switch>
    <Route path={RoutePaths.login} component={Login} />
    <AuthComponent path="/" component={Private} />
  </Switch>
)
