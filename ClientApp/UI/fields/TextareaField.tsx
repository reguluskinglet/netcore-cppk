import * as React from 'react'
import * as classnames from 'classnames'

export default class TextareaField extends React.Component<any, any> {
  render() {
    const { className } = this.props
    const classes = classnames('', 'field', 'layout', 'vertical', '', className)
    return (
      <div className={classes} style={{ height: this.props.height || '90px' }}>
        <div className="label flex-none" style={{ marginTop: '8px' }}>
          {this.props.label}
        </div>
        <textarea
          className="textarea flex"
          onChange={this.props.onChange}
          value={this.props.value || ''}
          rows={this.props.rows || 4}
        />
      </div>
    )
  }
}
