import * as React from 'react'

export default class Tabs extends React.Component<any, any> {
  render() {
    return (
      <div className={this.props.className + ' tabs layout horizontal'}>
        {this.props.tabs.map((tab, index) => (
          <div
            key={index}
            className={
              'tab flex-none layout horizontal center-center ' + (this.props.selectedTab === index ? ' selected ' : '')
            }
            onClick={() => {
              this.props.onTabChanged(index)
            }}
          >
            {tab.label}
          </div>
        ))}
      </div>
    )
  }
}
