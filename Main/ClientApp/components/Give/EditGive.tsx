import * as React from 'react'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from './store'
import Table from '../../UI/table'
import { get, map } from 'lodash'
import { Tabs, Form, Row, Col, Button } from 'antd'
import '../../../node_modules/antd/lib/tabs/style/index.css'
import { RouteComponentProps } from 'react-router-dom'
import { history } from '../../main'

const TabPane = Tabs.TabPane

const cells = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27]

class CLS extends React.Component<Props, any> {
  constructor(props) {
    super(props)
    this.state = {
      activeKey: '1'
    }
  }

  componentDidMount() {
    const isNew = get(this, 'props.match.params.id') === 'new'
    !isNew && this.props.get(this.props.match.params.id)
  }

  componentWillUnmount() {
    this.props.clear()
  }

  render() {
    const isNew = get(this, 'props.match.params.id') === 'new'
    return (
      <div className="card">
        <Tabs
          type="card"
          activeKey={this.state.activeKey}
          onChange={activeKey => {
            this.setState({ activeKey })
            // debugger
            switch (activeKey) {
              case '2':
                !isNew && this.state.hist && this.props.getLinks('hist', this.state.hist)
                break
              case '3':
                !isNew && this.state.gps && this.props.getLinks('gps', this.state.gps)
                break
              case '4':
                !isNew && this.state.charge && this.props.getLinks('charge', this.state.charge)
                break
            }
          }}
        >
          <TabPane tab="Основное" key="1">
            {this.renderFields()}
          </TabPane>
          {!isNew && (
            <TabPane tab="История операций" key="2">
              <Table
                data={this.props.hist}
                widthOffset={320}
                reload={data => {
                  this.setState({ hist: data })
                  !isNew && this.props.getLinks('hist', data)
                }}
              />
            </TabPane>
          )}
          {!isNew && (
            <TabPane tab="GPS история" key="3">
              <Table
                data={this.props.gps}
                widthOffset={320}
                reload={data => {
                  this.setState({ gps: data })
                  !isNew && this.props.getLinks('gps', data)
                }}
              />
            </TabPane>
          )}
          {!isNew && (
            <TabPane tab="История заряда" key="4">
              <Table
                data={this.props.charge}
                widthOffset={320}
                reload={data => {
                  this.setState({ charge: data })
                  !isNew && this.props.getLinks('charge', data)
                }}
              />
            </TabPane>
          )}
        </Tabs>
      </div>
    )
  }

  private renderFields() {
    return (
      <div className="padding-left padding-bottom">
        <Row className="margin-top">
          <Col span={2}>Ячейка</Col>
          <Col span={6}>
            <select
              value={this.props.element.cellNumber}
              onChange={e => {
                this.props.setValue('cellNumber', parseInt(e.target.value))
              }}
            >
              <option value={0} />
              {map(cells, cell => (
                <option key={cell} value={cell}>
                  {cell}
                </option>
              ))}
            </select>
          </Col>
        </Row>
        <Row className="margin-top">
          <Col span={2}>Модель</Col>
          <Col span={6}>
            <input
              value={this.props.element.name}
              onChange={e => {
                this.props.setValue('name', e.target.value)
              }}
            />
          </Col>
        </Row>
        <Row className="margin-top">
          <Col span={2}>Серийный номер</Col>
          <Col span={6}>
            <input
              value={this.props.element.serial}
              onChange={e => {
                this.props.setValue('serial', e.target.value)
              }}
            />
          </Col>
        </Row>
        <Row className="margin-top">
          <Button
            type={'primary'}
            onClick={() => {
              this.props.save().then(() => {
                history.push('/administration/give')
              })
            }}
          >
            Сохранить
          </Button>
        </Row>
      </div>
    )
  }
}

const provider = provide((state: ApplicationState) => state.give, Store.actionCreators).withExternalProps<
  RouteComponentProps<any>
>()

type Props = typeof provider.allProps

export default provider.connect(CLS)
