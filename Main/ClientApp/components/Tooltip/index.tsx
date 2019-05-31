import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from '../../store/CommonStore'
import classnames from 'classnames'

interface State {
  hidden?: boolean
  timeout: number
}

class Tooltip extends React.Component<Props, State> {

    componentWillMount() {

        this.state = {
            timeout: 3000
        }
    }

    componentWillReceiveProps(props) {
        const { common: { show } } = this.props
        if (show !== props.common.show && props.common.show) {
            setTimeout(() => {
                this.props.hideTooltip()
            }, this.state.timeout)
        }
    }

    render() {
        const { common: { message, t, show } } = this.props

        var className = classnames('tooltip layout horizontal center-center', {
            'info': t === 0,
            'error': t === 1,
            'warning':t === 2 
        })

        return (
            <div
                className={className}
                hidden={!show}
                onClick={() => {
                    this.props.hideTooltip()
                }}
            >
                <div className="tooltip-message">{message}</div>
            </div>
        )
    }
}

const provider = provide((state: ApplicationState) => ({ common: state.common }), Store.actionCreators)

type Props = typeof provider.allProps

export default provider.connect(Tooltip)
