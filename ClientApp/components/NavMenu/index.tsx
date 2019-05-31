import * as React from 'react'
import { NavLink, Link } from 'react-router-dom'
import { RoutePaths } from '../../common'
import './style.scss'
import { push } from 'react-router-redux'

export default class NavMenu extends React.Component<any, any> {
  getUserName = () => {
    const s = localStorage.getItem('user_info')
    const i = JSON.parse(s)
    return (i && i.name) || 'Без имени'
  }

  getUserPerm = () => {
    const s = localStorage.getItem('user_info')
    const i = JSON.parse(s)
    return (i && i.permissions) || 0
  }

  render() {
    const perm = this.getUserPerm()
    return (
      <div className="horizontal-menu">
        <div className="head">
          <div className="logo" />
        </div>

        <div className="menu">
          {((perm & 1) === 1 && (
            <NavLink to={RoutePaths.journals} activeClassName="menu-item selected" className="menu-item">
              События
            </NavLink>
          )) || <div className={`menu-item disabled`}>События</div>}

          {/* {((perm & 2) === 2 && (
            <NavLink to={RoutePaths.tasks} activeClassName="menu-item selected" className="menu-item">
              Задачи
            </NavLink>
          )) || <div className={`menu-item disabled`}>Задачи</div>} */}

          {((perm & 4) === 4 && (
          <NavLink to={RoutePaths.planning} activeClassName="menu-item selected" className="menu-item">
            Планирование
          </NavLink>
          )) || <div className={`menu-item disabled`}>Планирование</div>} 
      
          {/* {((perm & 4) === 4 && (
            <NavLink to={RoutePaths.schedule} activeClassName="menu-item selected" className="menu-item">
              Расписание
            </NavLink>
          )) || <div className={`menu-item disabled`}>Расписание</div>} */}

          {((perm & 8) === 8 && (
            <NavLink to={RoutePaths.dicts} activeClassName="menu-item selected" className="menu-item">
              Справочники
            </NavLink>
          )) || <div className={`menu-item disabled`}>Справочники</div>}

          {/* <NavLink exact to={RoutePaths.work} activeClassName="menu-item selected" className="menu-item">
            Работа
          </NavLink> */}

          {((perm & 16) === 16 && (
            <NavLink to={RoutePaths.tags} activeClassName="menu-item selected" className="menu-item">
              Метки
            </NavLink>
          )) || <div className={`menu-item disabled`}>Метки</div>}

          {((perm & 32) === 32 && (
            <NavLink exact to={RoutePaths.analyze} activeClassName="menu-item selected" className="menu-item">
              Отчеты
            </NavLink>
          )) || <div className={`menu-item disabled`}>Отчеты</div>}

          {((perm & -1) === -1 && (
            <NavLink to={RoutePaths.administration} activeClassName="menu-item selected" className="menu-item">
              Администрирование
            </NavLink>
          )) || <div className={`menu-item disabled`}>Администрирование</div>}

          <div className="user layout horizontal center">
            <div>{this.getUserName()}</div>
            <span
              className="icon-exit"
              style={{ fontSize: '16px', marginLeft: '8px' }}
              onClick={() => {
                localStorage.clear()
                window.location.reload()
              }}
            />
          </div>
            </div>
      </div>
    )
  }
}
