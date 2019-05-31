import * as React from 'react'
import * as classnames from 'classnames'

export default class Cell extends React.Component<any, any> {
  render() {
    const { className } = this.props
    const classes = className || 'cell flex layout horizontal center'
    return (
      <div className={classes} onClick={this.props.onClick} style={this.props.style || {}}>
        {this.props.children}
      </div>
    )
  }
}
