import * as React from 'react'

export default class Row extends React.Component<any, any> {
  render() {
    const classes = this.props.className + ' row layout horizontal'
    return (
      <div className={classes} onClick={this.props.onClick}>
        {this.props.children}
      </div>
    )
  }
}
