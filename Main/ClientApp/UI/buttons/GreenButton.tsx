import * as React from 'react'

export default class GreenButton extends React.Component<any, any> {
  render() {
    return (
      <div
        {...this.props}
        className={`${this.props.className} add-button flex-none layout horizontal center-center`}
        onClick={this.props.onClick}
      >
        <div
          className={`${this.props.icon || 'icon-plus'} white-icon`}
          style={{ color: 'white', fontSize: '18px', padding: '0px 4px 0 0px' }}
        />
        <div>{this.props.label}</div>
      </div>
    )
  }
}
