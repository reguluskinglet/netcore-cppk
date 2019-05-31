import React, { Component } from 'react'
import { Row, Col } from 'antd'
import { InputField } from '../../UI/fields'
import { map } from 'lodash'
import moment from 'moment'

export default class Akts extends Component<any, any> {
  state = {
    f1: '',
    f2: '',
    f3: ''
  }
  render() {
    return (
      <div className="padding">
        <Row>
          <Col span={7}>
            <InputField
              label="Номер"
              onChange={e => {
                this.setState({ f1: e.target.value })
              }}
              value={this.state.f1}
              className="margin-left"
            />
          </Col>
          <Col span={7}>
            <InputField
              label="Дата"
              onChange={e => {
                this.setState({ f2: e.target.value })
              }}
              value={this.state.f2}
              className="margin-left"
              type="date"
            />
          </Col>
          <Col span={7}>
            <InputField
              label="Место"
              onChange={e => {
                this.setState({ f3: e.target.value })
              }}
              value={this.state.f3}
              className="margin-left"
            />
          </Col>
          <Col span={3}>
            <span
              className="icon-save path1 path2 path3 margin"
              onClick={() => {
                this.props.onAdd({
                  number: this.state.f1,
                  placeDrawUp: this.state.f3,
                  dateDrawUp: this.state.f2,
                  trainId: this.props.trainId
                })
                this.setState({ f1: '', f2: '', f3: '' })
              }}
              style={{ fontSize: '30px' }}
            >
              <span className="path1" />
              <span className="path2" />
              <span className="path3" />
            </span>
          </Col>
        </Row>
        <Row>
          <Col span={24}>
            <table style={{ width: '100%' }}>
              <thead>
                <tr>
                  <td>
                    <b>Номер</b>
                  </td>
                  <td>
                    <b>Дата</b>
                  </td>
                  <td>
                    <b>Место</b>
                  </td>
                  <td />
                </tr>
              </thead>
              <tbody>
                {map(this.props.akts, akt => (
                  <tr key={akt.id}>
                    <td>{akt.number}</td>
                    <td>{moment(akt.dateDrawUp).format('DD.MM.YYYY')}</td>
                    <td>{akt.placeDrawUp}</td>
                    <td>
                      <div
                        className={(true && 'icon-delete') || 'icon-delete-1'}
                        onClick={() => {
                          true && this.props.onDelete(akt.id)
                        }}
                      />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </Col>
        </Row>
      </div>
    )
  }
}
