import * as React from 'react'

export default class BlueButton extends React.Component<any, any> {
  render() {
    return (
      <div
        className={`${this.props.className} blue-button flex-none layout horizontal center-center`}
        onClick={this.props.onClick}
        style={{ width: this.props.width }}
      >
        <div>{this.props.label}</div>
      </div>
    )
  }
}
