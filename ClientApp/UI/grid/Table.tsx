import * as React from 'react'

export default class Table extends React.Component<any, any> {
  render() {
    const classes = this.props.className + ' table layout vertical'
    return <div className={classes}>{this.props.children}</div>
  }
}
