import * as React from 'react'
import * as classnames from 'classnames'

export default class DropdownField extends React.Component<any, any> {
  render() {
    const { className, disabled } = this.props
    const classes = classnames('', 'field', 'layout', 'horizontal', 'center-center', className, disabled && 'disabled')
    return (
      <div className={classes} style={this.props.style}>
        {!this.props.hideLabel && (
          <div className="label flex-none" style={{ width: this.props.labelWidth || undefined }}>
            {this.props.label}
          </div>
        )}
        <select
          className="input dropdown flex"
          name="list"
          onChange={this.props.onChange}
          value={this.props.value === null ? -1 : this.props.value}
          multiple={this.props.multiple || false}
          style={{ maxWidth: this.props.maxWidth || undefined }}
          disabled={disabled}
        >
          {this.props.showNull && <option value={-1}> </option>}
          {this.props.placeholder && <option value={-1}>{this.props.placeholder}</option>}
          {this.props.list &&
            this.props.list.map((item, index) => (
              <option key={index} value={item.value}>
                {item.label}
              </option>
            ))}
        </select>
        {!this.props.hideClear && (
          <span
            className="icon-clear clear layout horizontal center-center"
            onClick={() => {
              !disabled && this.props.onChange({ currentTarget: { value: null }, target: { value: null } })
            }}
          >
            <span className="path1" />
            <span className="path2" />
            <span className="path3" />
          </span>
        )}
      </div>
    )
  }
}
