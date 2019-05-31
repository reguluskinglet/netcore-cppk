import * as React from 'react'
import * as classnames from 'classnames'

export default class Label extends React.Component<any, any> {
  render() {
    const { className } = this.props
    const classes = classnames('layout', 'horizontal', 'center-center', className)
    return (
      <div
        className={classes}
        style={{
          ...this.props.style,
          width: this.props.width || (this.props.style && this.props.style.width) || undefined
        }}
      >
        <div className="label flex-none" style={{ width: this.props.labelWidth || undefined }}>
          {this.props.text}
        </div>
        {this.props.children}
      </div>
    )
  }
}
