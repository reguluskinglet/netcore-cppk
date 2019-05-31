import * as React from 'react'
import InputField from '../fields/InputField'
import DropdownField from '../fields/DropdownField'


export default class Paginator extends React.Component<any, any> {
 setPage = pageNumber => {
     var { limit } = this.props
     var lim = limit
     limit = this.getSetting('limit') * 1 || limit
     if (lim != limit)
         this.props.onLimitChange(limit);
    const skip = (pageNumber - 1) * limit
    this.props.setPage(skip)
  }
    getSetting = settingName => {
        var j = JSON.parse(localStorage['Paginator'] || "null")
        if (j == null || j[location.pathname] == null || j[location.pathname][settingName] == null)
            return null;
        return  JSON.parse( localStorage['Paginator'])[location.pathname][settingName]
    }

    saveSetting = (settingName, settingValue) => {
        var settings = JSON.parse(localStorage['Paginator'] || '{}')
        settings[location.pathname] = settings[location.pathname] || {}
        settings[location.pathname][settingName] = settingValue
        localStorage['Paginator'] = JSON.stringify(settings)
    }
  render() {
      var { total, limit, skip } = this.props
      var lim = limit
      limit = this.getSetting('limit') * 1 || limit
      if (lim != limit)
          this.props.onLimitChange(limit);
   
      const pageCount = Math.ceil(total / limit)
    const pages = []
    for (var i = 1; i <= pageCount; i++) {
      pages.push(i)
    }
    const currentPage = (skip + limit) / limit
    return (
      <div className="layout horizontal center">
          {currentPage > 1 && (
          <div
            className="paginator-item layout horizontal center"
            onClick={() => {
                currentPage > 1 && this.setPage(currentPage - 1)
            }}
          >
            {<span className="icon-chevron-left" style={{ fontSize: '16px' }} />}
          </div>
        )}

          {pageCount >= 1 && (
          <div
            className={`paginator-item layout horizontal center ${currentPage == 1 && `current-page`}`}
            onClick={() => {
              this.setPage(1)
            }}
          >
            1
          </div>
        )}

          {currentPage > 2 &&
          currentPage <= (pageCount) && (
            <div
              className={`paginator-item layout horizontal center`}
            >
              {`...`}
            </div>
          )}

          {currentPage > 1 &&
          currentPage < pageCount && (
            <div
              className={`paginator-item layout horizontal center ${`current-page`}`}
              onClick={() => {
                this.setPage(currentPage)
              }}
            >
              {currentPage}
            </div>
          )}

          {currentPage >= 1 &&
          currentPage < (pageCount-1) && (
            <div
              className={`paginator-item layout horizontal center`}
            >
              {`...`}
            </div>
          )}

          {pageCount > 1 && (
          <div
            className={`paginator-item layout horizontal center ${currentPage == pageCount && `current-page`}`}
            onClick={() => {
              this.setPage(pageCount)
            }}
          >
            {pageCount}
          </div>
        )}

          {pageCount > currentPage && (
          <div
            className="paginator-item layout horizontal center"
            onClick={() => {
              pageCount > currentPage && this.setPage(currentPage + 1)
            }}
          >
            {<span className="icon-chevron-right" style={{ fontSize: '16px' }} />}
          </div>
        )}

          <div className="flex"/>

          <DropdownField
              hideClear
              className="paginator-item layout horizontal center"
              label="Кол-во строк на странице"
              type="number"
              onChange={e => {
                  this.props.onLimitChange(parseInt(e.target.value))
                  this.saveSetting('limit', e.target.value)
            //this.setPage(currentPage)
          }}
              value={limit}
              width={'36px'}
              list={[
            { value: 10, label: 10 },
            { value: 25, label: 25 },
            { value: 50, label: 50 },
            { value: 100, label: 100 }
          ]}/>
      </div>
    )
  }
}
