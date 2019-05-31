import * as React from 'react'

export default class PrinterButton extends React.Component<any, any> {
  render() {
    return (
      <div
        className={`${this.props.className} printer-button ${this.props.enabled
          ? 'enabled'
          : ''} flex-none layout horizontal center-center`}
        onClick={event => {
          this.props.enabled && this.props.onClick(event)
        }}
      >
        <div className="icon-printer" style={{ color: 'white', fontSize: '18px', padding: '0px 8px 0 0px' }} />
        <div>{this.props.label}</div>
      </div>
    )
  }
}
