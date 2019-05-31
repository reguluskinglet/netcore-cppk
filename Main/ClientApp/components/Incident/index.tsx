import * as React from 'react'
import { ApplicationState } from '../../store'
import * as IncidentStore from '../../store/IncidentStore'
import { provide } from 'redux-typed'
import { RequestEvent } from '../../common'
import { RouteComponentProps } from 'react-router-dom'

class Incident extends React.Component<Props, any> {
  componentWillMount() {
    this.props.getData()
  }

  render() {
    return this.props.message && this.props.common.requestEvent === RequestEvent.End ? (
      <div>
        <h2>Incident</h2>
        <p>{this.props.message}</p>
      </div>
    ) : null
  }
}

const provider = provide(
  (state: ApplicationState) => ({ ...state.incident, common: state.common }),
  IncidentStore.actionCreators
).withExternalProps<RouteComponentProps<{}>>()

type Props = typeof provider.allProps

export default provider.connect(Incident)
