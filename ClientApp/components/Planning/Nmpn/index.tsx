import './index.scss'
import * as React from 'react'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../../store';
import * as GridStore from '../../../store/GridStore';
import * as DialogStore from '../../../store/DialogStore';
import { GridType, journalTitleFilter } from '../../../common'
import { Loading } from '../../../UI/loading'
import { Table } from '../../../UI/tables';
import { Filter, DayOfWeek } from '../../Common'
import { RouteComponentProps } from 'react-router'
import { Button } from '../../../UI/button'
import { DatePickerRange } from '../../../UI/datepicker'
import * as classnames from 'classnames';
import { ToolBar, ToolBarButton } from '../../../UI/toolbar';
import * as store from './store';
import Scrollbars from 'react-custom-scrollbars';
import moment from 'moment'

interface State {
    width: number
    height: number
}

class XUI extends React.Component<Props, State> {

    constructor(props) {
        super(props)

        this.state = {
            width: $(window).width(),
            height: $(window).height(),
        }
    }

    updateDimensions = () => {
        this.setState({ width: $(window).width(), height: $(window).height() });
    }

    componentDidMount() {
        window.addEventListener("resize", this.updateDimensions);
    }

    componentWillUnmount() {
        window.removeEventListener("resize", this.updateDimensions);
    }

    componentWillMount() {

        var startDate = new Date();
        //var endDate = new Date();
        //endDate=endDate.addDays(31)

        this.props.get(this.parseDate(startDate), null);
    }

    getDateRange = () => {
    }

    mapRowValue = (row, index, value: string) => {

        if (index == 2) {

            if (!value)
                return <DayOfWeek selectedDays={null} />

            var result = value.split(',').map(x => parseInt(x, 10));
            return <DayOfWeek selectedDays={result} />
        }

        return value;
    }

    mapColumntClass = (row, index, value: string) => {

        if (index == 2)
            return 'days'

        return '';
    }

    //showRow = (row, isEdit) => {
    //    if (isEdit)
    //        this.props.getTurnoverById(row.id, this.props.clearFilter)
    //    else
    //        this.props.goToDocument('/planning/goep/' + row.id)
    //}

    //clearFilter = () => {
    //    this.props.clearFilter();
    //}

    //applyFilter = (e) => {
    //    e.preventDefault();

    //    this.props.applyFilter(this.props.grid.filter);
    //}

    parseDate = (date) => {
        return moment(date,'YYYY-MM-DDTHH:mm:ss').format('DD.MM.YYYY')
    }

    handleChange = (newValue, field) => {

       // var date = moment(newValue, 'DD.MM.YYYY').toDate()

        var startDate = null,
            endDate = null;

        if (field === 'startDate')
            startDate = newValue;
        else
            endDate = newValue;

        this.props.get(startDate, endDate)

    }

    renderRowHeader = () => {

        const { startDate, endDate } = this.props.data;

        var td = [
            <th>Маршрут</th>,
            <th>Дни недели</th>
        ];


        var startDateRes = moment(startDate, 'YYYY-MM-DDTHH:mm:ss').toDate();
        var endDateRes = moment(endDate, 'YYYY-MM-DDTHH:mm:ss').toDate();

        for (var date = startDateRes; date <= endDateRes; date = date.addDays(1)) {

            var week = date.getUTCDay();

            var thDate = date.getDate();

            td.push(<th className={classnames({
                'hday': (week == 6 || week == 5) 
            })}>{thDate}</th>);
        }

        return <tr>{td}</tr>
    }

    renderRowsData = (date: Date, daysData: NmpnDaysData[], routeDays: number[], route: NmpnRoute) => {

        var week = date.getDay();

        var isSelectedDay = routeDays.indexOf(week) >= 0;

        var data = this.renderData(daysData, date);

        var onClick = null;

        if (isSelectedDay) {
            onClick = () => {

                var requestDate = moment(date).format('YYYY-MM-DDT00:00:00');

                this.onSelectedDay(requestDate, data.data, route)
            }
        }

        return <td onClick={onClick} className={classnames('g-item', {
            'g-green': !isSelectedDay
        })}>{data ? data.component : null}</td>
    }

    onSelectedDay = (date, dayDate, route) => {
        this.props.addTrainToRoute(date, dayDate, route, this.reload)
    }

    reload = () => {
        var startDate = new Date();

        this.props.get(this.parseDate(startDate), null);
    }

    renderData = (daysDatas: NmpnDaysData[], date: Date) => {

        var index = null

        for (var i = 0; i < daysDatas.length; i++) {

            var dayData = daysDatas[i];

            var dateMoment = moment(dayData.date, 'YYYY-MM-DDTHH:mm:ss');
            var r = moment(date).startOf('day').format('YYYY-MM-DD');
            var hasDate = dateMoment.isSame(r);

            if (hasDate) {
                index = i;
                break;
            }

        }

        if (index === null)
            return null;
        else {
            var data = daysDatas[index]
            var countUsers = data.users ? data.users.length : 0;


            return {
                data: data,
                component:<div className="g-npm-info">
                    {data.train && <span className="icon-train" />}
                    {countUsers === 0 ? null : countUsers > 1 ? <span className="icon-users" /> : <span className="icon-user" />}
                </div>
            }
        }
    }
    //this.renderRowsData(x.daysData, x.routeDays)
    render() {
        if (this.props.isLoaded === GridType.nmpn) {

            const { startDate, endDate, result } = this.props.data;

        //    var s = this.props.data.result.map(x=>x.)

            var rows = [];

            var startDateRes = moment(startDate, 'YYYY-MM-DDTHH:mm:ss').toDate();
            var endDateRes = moment(endDate, 'YYYY-MM-DDTHH:mm:ss').toDate();

            for (var i = 0; i < result.length; ++i) {

                var route = result[i];
                var tds = [
                    <td>{route.route.name}</td>,
                    <td className="days"><DayOfWeek selectedDays={route.routeDays} /></td>
                ];

                for (var date = startDateRes; date <= endDateRes; date = date.addDays(1)) {

                    tds.push(this.renderRowsData(date, route.daysData, route.routeDays, route.route))
                }


               // var tdDays=result[i].daysData.map(x => this.renderRowsData(x, route.routeDays))

               // var resultTds = tds.concat(tdDays)

                rows.push(<tr>{tds}</tr>)
            }

         

            return <div className="g-nmpn">
                <h2 className="title">Назначение поездов и бригад на маршруты</h2>
                <div className="filter filter-fix">
                    <div className="filter-center">
                        <DatePickerRange
                            title="Период назначения"
                            valueStart={this.parseDate(startDate)}
                            nameStart="startDate"
                            valueEnd={this.parseDate(endDate)}
                            nameEnd="endDate"
                            handleChange={this.handleChange}
                        />
                    </div>
                </div>
                <Scrollbars style={{ width: this.state.width - 25, height: this.state.height-290 }}>
                    <table className="grid">
                        {this.renderRowHeader()}
                        <tbody>{rows}</tbody>
                    </table>
                </Scrollbars>
            </div>
        } else {
            return <Loading />
        }
    }
}

const provider = provide(
    (state: ApplicationState) => ({ data: (state.common.data as { result: Nmpn[], startDate: Date, endDate: Date}), isLoaded: state.common.isLoaded }),
    {...DialogStore.actionCreators, ...store.actionCreators }
).withExternalProps<RouteComponentProps<any>>()

type Props = typeof provider.allProps;

export default provider.connect(XUI);