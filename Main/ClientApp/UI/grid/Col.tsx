import * as React from 'react'
import * as classnames from 'classnames'

export default class Col extends React.Component<any, any> {
  render() {
    const { className } = this.props
    const classes = className + ' layout vertical'
    return (
      <div className={classes} onClick={this.props.onClick} style={this.props.style || {}}>
        {this.props.children}
      </div>
    )
  }
}
