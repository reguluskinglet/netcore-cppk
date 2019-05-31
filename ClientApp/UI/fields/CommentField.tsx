import * as React from 'react'
import * as classnames from 'classnames'
import { File } from '.'
import './style.scss'

const folders = ['project/docs/', 'project/images/', 'project/sounds/']

export default class CommentField extends React.Component<any, any> {
  render() {
    const { className } = this.props
    const classes = classnames('', 'flex', 'layout', 'vertical', '', className)
    return (
      <div className={classes} style={this.props.style}>
        <div className="label flex-none" style={{ marginTop: this.props.marginTop || '8px' }}>
          {this.props.label}
        </div>
        <div className="comment flex layout vertical">
          <textarea
            className="text flex"
            onChange={this.props.textChange}
            value={this.props.text || ''}
            rows={4}
            style={{ height: '90px' }}
          />
          <div className="flex-none layout horizontal" style={{ borderTop: '1px solid #666666', borderBottom: '1px solid #666666', marginTop: '4px' }}>
            <div className="flex layout horizontal wrap center" style={{ maxHeight: '140px', overflow: 'auto' }}>
              {this.props.files &&
                this.props.files.map((file, index) => (
                  <File
                    key={index}
                    fullName={file.name}
                    previewPath={'project/images/thumbs/' + file.thumbPath}
                    path={folders[file.documentType] + file.path}
                    canDelete
                    onDelete={() => {
                      this.props.fileDelete(file)
                    }}
                  />
                ))}
            </div>
            <span
              className="icon-attach self-end"
              style={{ fontSize: '18px', color: '#5090cd', margin: '4px' }}
              onClick={() => {
                this.props.fileAdd()
              }}
            />
          </div>
        </div>
      </div>
    )
  }
}
