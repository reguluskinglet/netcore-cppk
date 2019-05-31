import * as React from 'react'
import { NavLink, Link } from 'react-router-dom'

interface Props {
  fullName: string
  path: string
  previewPath: string
}

export default class File extends React.Component<any, any> {
  getIcon = type => {
    switch (type) {
      case 'mp3':
      case 'wav':
        return 'icon-audio'

      case 'mp4':
      case 'wmv':
      case 'avi':
        return 'icon-video'

      default:
        return 'icon-doc'
    }
  }

  render() {
    const { fullName, path, previewPath, canDelete } = this.props
    const a = fullName.split('.')
    const name = (a && a.length > 0 && a[0]) || ''
    const type = (a && a.length > 1 && a[a.length - 1]) || ''
    const icon = this.getIcon(type)
    const hasPreview = type === 'png' || type === 'bmp' || type === 'jpg'
    return (
      <div className={`file layout horizontal center ${canDelete ? 'grey' : ''}`}>
        {hasPreview && (
          <div className="layout horizontal">
            <img
              src={previewPath}
              style={{ width: '64px', height: '48px', cursor: 'pointer' }}
              onClick={() => {
                window.open(path, '_blank')
              }}
            />
            {canDelete && (
              <span
                className="icon-close"
                style={{ fontSize: '14px', color: '#111111', marginLeft: '4px' }}
                onClick={() => {
                  this.props.onDelete()
                }}
              />
            )}
          </div>
        )}
        {!hasPreview && (
          <div className="layout horizontal center">
            <span
              className={icon}
              style={{ fontSize: '14px', color: '#5090cd', marginRight: '4px', cursor: 'pointer' }}
              onClick={() => {
                window.open(path, '_blank')
              }}
            />
            <div
              className="file-name"
              onClick={() => {
                window.open(path, '_blank')
              }}
              style={{ cursor: 'pointer' }}
            >
              {name}
            </div>
            {canDelete && (
              <span
                className="icon-close"
                style={{ fontSize: '14px', color: '#111111', marginLeft: '4px' }}
                onClick={() => {
                  this.props.onDelete()
                }}
              />
            )}
          </div>
        )}
      </div>
    )
  }
}
