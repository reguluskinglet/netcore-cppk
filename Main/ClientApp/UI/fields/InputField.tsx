import * as React from 'react'
import * as classnames from 'classnames'
import { DatePicker } from 'antd'
import moment from 'moment'
import 'moment/locale/ru'
import locale from 'antd/lib/date-picker/locale/ru_RU'

export default class InputField extends React.Component<any, any> {
  render() {
    const { className } = this.props
    const classes = classnames('', 'field', 'layout', 'horizontal', 'center-center', className)
    return (
      <div
        className={classes}
        style={{
          ...this.props.style,
          width: this.props.width || (this.props.style && this.props.style.width) || undefined
        }}
      >
        {!this.props.hideLabel &&
          !this.props.rightLabel && (
            <div className="label flex-none" style={{ width: this.props.labelWidth || undefined }}>
              {this.props.label}
            </div>
          )}

        {this.props.type !== 'date' && (
          <input
            className={!this.props.rightLabel ? 'input flex' : 'input'}
            type={this.props.type || 'text'}
            onChange={this.props.onChange}
            value={this.props.value || ''}
            checked={
                this.props.type === 'checkbox' || this.props.type === 'radio'
                    ? this.props.value !== null ? this.props.value : false
                    : undefined
            }
            onKeyUp={event => {
                if (event.keyCode == 13) {
                    this.props.onEnter()
                }
            }}
            disabled={this.props.disabled}
            style={{ width: this.props.width || undefined, height: this.props.height || undefined }}
            placeholder={this.props.placeholder}
            autoComplete={this.props.autoComplete}
          />
        )}

        {this.props.type === 'date' && (
          <DatePicker
            placeholder={this.props.placeholder}
            disabled={this.props.disabled}
            format={'DD.MM.YYYY'}
            onChange={value => {
              const iso = (value && value.toISOString()) || null
              this.props.onChange({ currentTarget: { value: iso }, target: { value: iso } })
            }}
            value={this.props.value ? moment(this.props.value) : undefined}
            // showToday={false}
            locale={locale}
          />
        )}

        {this.props.rightLabel && (
          <div className="label flex" style={{ width: this.props.labelWidth || undefined, marginLeft: '8px' }}>
            {this.props.label}
          </div>
        )}
        {!this.props.hideClear &&
          this.props.type !== 'date' &&
          this.props.type !== 'checkbox' &&
          this.props.type !== 'radio' && (
            <span
              className="icon-clear clear layout horizontal center-center"
              onClick={() => {
                this.props.onChange({ currentTarget: { value: '' }, target: { value: '' } })
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
