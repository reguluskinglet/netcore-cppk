import * as React from 'react'

export default class Row extends React.Component<any, any> {
  render() {
    return <div className="header-row layout horizontal">{this.props.children}</div>
  }
}
