import './login.scss'
import * as React from 'react'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as UserStore from '../../store/UserStore'
import { RouteComponentProps } from 'react-router-dom'
import { RequestEvent } from '../../common'
import InputField from '../../UI/fields/InputField'
import BlueButton from '../../UI/buttons/BlueButton'

interface ExternalProps extends RouteComponentProps<any> {
  exampel: any
}

class Login extends React.Component<Props, any> {
  constructor(props) {
    super(props)
    const redirectRoute = this.props.location.state ? this.props.location.state.from.pathname : '/'

    this.state = {
      login: { value: '', isValid: true },
      password: { value: '', isValid: true },
      redirectTo: redirectRoute
    }
  }

  validate(value) {
    if (value && value.length > 0) return true
    return false
  }

  onChange = (name, value) => {
    var isValid = this.validate(value)

    this.setState({
      [name]: {
        value: value,
        isValid: isValid
      }
    })

    return isValid
  }

  isValidForm = () => {
    const { login, password } = this.state

    var isValidLogin = this.onChange('login', login.value)
    var isValidPassword = this.onChange('password', password.value)

    return isValidLogin && isValidPassword
  }

  onSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()

    if (this.isValidForm()) {
      this.props.logIn(this.state.login.value, this.state.password.value, '/') //this.state.redirectTo)
    }
  }

  render() {
    const { login, password } = this.state
    const disabled =
      this.props.requestEvent === RequestEvent.Start ||
      !(login.value && login.isValid && password.value && password.isValid)

    return (
      <div className="login-container">
        <div className="main-login">
          <form id="form" onSubmit={this.onSubmit}>
            <div className="main-form">
              <div className="main-form-body layout vertical center">
                <div className="logo" />
                <span className="title">Требуется авторизация</span>
                <InputField
                  className="margin-bottom"
                  placeholder="Логин"
                  value={login.value}
                  name="login"
                  onChange={e => {
                    this.onChange('login', e.target.value)
                  }}
                  isValid={login.isValid}
                  hideClear
                  hideLabel
                  width={'320px'}
                />
                <InputField
                  className=""
                  placeholder="Пароль"
                  type="password"
                  value={password.value}
                  name="password"
                  onChange={e => {
                    this.onChange('password', e.target.value)
                  }}
                  isValid={password.isValid}
                  hideClear
                  hideLabel
                  width={'320px'}
                />
                <p className="error-message">{this.props.errorMessage}</p>
                <div className="form-btn layout vertical center">
                  {this.props.requestEvent === RequestEvent.Start ? <div className="loading" /> : null}
                  <BlueButton label="Войти" onClick={this.onSubmit} width={'320px'} />

                  {/* <input type="submit" disabled={disabled} value="Войти" /> */}
                </div>
              </div>
            </div>
          </form>
          <div className="bottom-text layout vertical center">{`АСПОЖС, ${new Date().getFullYear()} г.`}</div>
        </div>
      </div>
    )
  }
}

const provider = provide((state: ApplicationState) => state.user, UserStore.actionCreators).withExternalProps<
  ExternalProps
>()

type Props = typeof provider.allProps

export default provider.connect(Login)
