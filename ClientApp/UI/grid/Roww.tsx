import * as React from 'react'

export default class Row extends React.Component<any, any> {
  render() {
    const classes = this.props.className + ' layout horizontal'
    return (
      <div className={classes} onClick={this.props.onClick} style={this.props.style}>
        {this.props.children}
      </div>
    )
  }
}
