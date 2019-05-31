import * as React from 'react';
import { Route, Redirect } from 'react-router-dom';
import { UserService } from '../services'
import { RoutePaths } from '.'

export const AuthComponent = ({ component: Component, ...rest }: { component: any, path: string, exact?: boolean }) => (
    <Route {...rest} render={props => (
        UserService.IsAuthenticated
            ? <Component {...props} />
            : <Redirect to={{
                pathname: RoutePaths.login,
                state: { from: props.location }
            }} />
    )} />
);