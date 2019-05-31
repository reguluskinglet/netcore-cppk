import * as React from 'react'
import * as classnames from 'classnames'
import './style.scss'
import Select from 'antd/lib/select'
const Option = Select.Option

export default class DropdownMultiField extends React.Component<any, any> {
  render() {
    const { className } = this.props
    const classes = classnames('', '', 'layout', 'horizontal', 'center-center', className)
    return (
      <div className={classes} style={this.props.style}>
        <div className="label flex-none" style={{ width: this.props.labelWidth || undefined }}>
          {this.props.label}
        </div>
        <Select className="flex" mode="multiple" onChange={this.props.onChange} value={this.props.value}>
          {this.props.list &&
            this.props.list.map((item, index) => (
              <Option key={index} value={item.value}>
                {item.label}
              </Option>
            ))}
        </Select>
      </div>
    )
  }
}
