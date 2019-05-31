import * as React from 'react'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from './store'
import Table from '../../UI/table'
import { get, map } from 'lodash'
import { Tabs, Form, Row, Col, Button, Select, Input, DatePicker } from 'antd'
import "../../../node_modules/antd/lib/date-picker/style/index.css";
import "../../../node_modules/antd/lib/grid/style/index.css"
import { RouteComponentProps } from 'react-router-dom'
import '../../../node_modules/antd/lib/input/style/index.css'
import { history } from '../../main'
import moment from 'moment'

const TabPane = Tabs.TabPane

const { TextArea } = Input

class CLS extends React.Component<Props, any> {
  constructor(props) {
    super(props)
    this.state = {
      activeKey: '1'
    }
  }

  componentDidMount() {
    const isNew = get(this, 'props.match.params.id') === 'new'
    this.props
      .getLinks()
      .then(() => !isNew && this.props.get(this.props.match.params.id))
  }

  componentWillUnmount() {
    this.props.clear()
  }

  render() {
    return <div className="card padding-bottom">{this.renderFields()}</div>
  }

  private renderInputField(name, title, disabled = false) {
    return (
      <Row className="margin-top">
        <Col span={8}>{title}</Col>
        <Col span={16}>
          <Input
            disabled={disabled}
            value={get(this.props.element, name)}
            onChange={e => {
              this.props.setValue(name, e.target.value)
            }}
          />
        </Col>
      </Row>
    )
  }

  private renderTextField(name, title, disabled = false) {
    return (
      <Row className="margin-top">
        <Row>{title}</Row>
        <Row>
          <TextArea
            disabled={disabled}
            value={get(this.props.element, name)}
            onChange={e => {
              this.props.setValue(name, e.target.value)
            }}
          />
        </Row>
      </Row>
    )
  }

  private renderSelectField(
    name,
    title = '',
    list = [],
    { id = 'id', label = 'name' } = { id: 'id', label: 'name' },
    disabled = false
  ) {
    return (
      <Row className="margin-top">
        <Col span={8}>{title}</Col>
        <Col span={16}>
          <Select
            disabled={disabled}
            value={get(this.props.element, [name, 'id'])}
            onChange={e => {
              this.props.setValue(`${name}.id`, e as string)
            }}
          >
            {list.map(el => (
              <Select.Option key={get(el, id)} value={get(el, id)}>
                {get(el, label)}
              </Select.Option>
            ))}
          </Select>
        </Col>
      </Row>
    )
  }

  private renderDateField(name, title, disabled = false) {
    return (
      <Row className="margin-top">
        <Col span={8}>{title}</Col>
        <Col span={16}>
          <DatePicker
            showTime
            disabled={disabled}
            value={moment(get(this.props.element, name)).utcOffset(0)}
            format="DD-MM-YYYY HH:mm:ss"
            onOk={e => {
              this.props.setValue(name, e)
            }}
          />
        </Col>
      </Row>
    )
  }

  private renderFields() {
    const isNew = get(this, 'props.match.params.id') === 'new'
    return (
      <div className="padding-left padding-bottom">
        <Col span={12}>
          {!isNew && this.renderDateField('createDate', 'Дата', true)}
          {!isNew && this.renderInputField('user.name', 'Инициатор', true)}

          {this.renderSelectField(
            'device',
            'Устройство',
            this.props.devices,
            { id: 'id.id', label: 'col1' },
            !isNew
          )}
          {this.renderSelectField(
            'deviceFault',
            'Типовая неисправность',
            this.props.faults,
            undefined,
            !isNew
          )}
          {this.renderInputField('description', 'Описание', !isNew)}
          {isNew &&
            this.renderSelectField('status', 'Статус', this.props.statuses)}

          {isNew && (
            <Row className="margin-top">
              <Button
                type={'primary'}
                onClick={() => {
                  this.props.save().then(() => {
                    history.push('/administration/service')
                  })
                }}
              >
                Сохранить
              </Button>
            </Row>
          )}
        </Col>
        {!isNew && (
          <Col span={12} style={{ padding: '24px 24px 0 24px' }}>
            <h4>Комментарии:</h4>
            {map(get(this, 'props.element.comments'), comment => (
              <p>
                {`${moment(comment.date).utcOffset(0).format(
                  'DD.MM.YYYY HH:mm:ss'
                )} ${comment.user} ${comment.status || ''} ${comment.text ||
                ''}`}
              </p>
            ))}
            {this.renderSelectField('status', 'Статус', this.props.statuses)}
            {this.renderTextField('comment', 'Комментарий')}
            <Button
              className="margin-top"
              type={'primary'}
              onClick={() => {
                this.props.comment().then(() => {
                  // this.props.get(this.props.match.params.id)
                  history.push('/administration/service')
                })
              }}
            >
              Добавить
            </Button>
          </Col>
        )}
      </div>
    )
  }
}

const provider = provide(
  (state: ApplicationState) => state.serv,
  Store.actionCreators
).withExternalProps<RouteComponentProps<any>>()

type Props = typeof provider.allProps

export default provider.connect(CLS)
