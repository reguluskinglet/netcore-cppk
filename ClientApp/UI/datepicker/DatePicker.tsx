import './theme.scss'
import * as React from 'react'

import * as classnames from 'classnames'
import PickerThead from './PickerThead'
import MonthView from './MonthView'
import '../utils/date'
import * as ReactDOM from "react-dom";
import Portal from '../hoc/Portal';
import events from '../utils/events';
import { Button } from '../button'

var dayNamesShort = Globalize.culture().calendar.days.namesShort.rotateShift(0);
var monthNames = Globalize.culture().calendar.months.names;

interface DaysProps {
    updateSelectedDate: (event, t) => void
}

interface Month
{
    monthNumber: number
    year: number
    weeks: Week[]
}

interface Week {
    days: Day[]
}

interface Day
{
    date: Date
    //Номер дня
    dayNubmer?: number

    //Является выходным днем
    isWeekend?: boolean

    //Признак, является днем другого месяца
    isAnotherMonth?: boolean

    //Является текущем выбранным днем
    isCurentDate?: boolean

    isToday?: boolean
}


interface DatePickerState
{
    currentSelectedDate: Date
    renderDate?: Date
    monthShow: boolean
    isOpen: boolean
    value: string
}

interface DatePickerProps
{
    value?: string
    title?: string
    name?: string
    disabled?: boolean
    handleChange?: (newValue, field) => void
    hide?: boolean

    isLayout?: boolean

    withTime?: boolean
}


export default class DatePicker extends React.Component<DatePickerProps, DatePickerState> {

    constructor(props) {
        super(props)

        var date = Globalize.parseDate(props.value);

        var currentDate = date ? date : new Date()

        this.state = {
            currentSelectedDate: currentDate,
            renderDate: currentDate,
            monthShow: false,
            isOpen: false,
            value: props.value
        };
    }

    componentWillReceiveProps(nextProps: DatePickerProps) {

        var date = Globalize.parseDate(nextProps.value);

        var currentDate = date ? date : new Date()

        this.setState({
            currentSelectedDate: currentDate,
            renderDate: currentDate,
            value: nextProps.value
        })
    }

    getCalendarData(countMonth: number = 1): Month {

        const {renderDate, currentSelectedDate } = this.state;

        var year = renderDate.getFullYear();
        var currentMonth = renderDate.getMonth();

        var firstDay = new Date(year, currentMonth, 1);
        var lastDay = new Date(year, currentMonth + 1, 0);

        //Если первый день недели не Понедельник
        if (firstDay.getDay() != 1) {
            do
                firstDay = firstDay.addDays(-1)
            while (firstDay.getDay() != 1)
        }


        var month: Month = {
            monthNumber: currentMonth,
            year: year,
            weeks: new Array<Week>(),
        }

        //Проходим по циклу всех недель месяца
        for (var week = 0; week <= 5; week++) {

            var xWeek: Week =
                {
                    days: new Array<Day>()
                };

            month.weeks.push(xWeek);

            // Определяем последний день текущей недели
            lastDay = firstDay.addDays(6);

            // Записываем все даты для текущей недели
            for (var date = firstDay; date <= lastDay; date = date.addDays(1)) {

                var xDay: Day = {
                    date: date,
                    dayNubmer: date.getDate(),
                    isWeekend: date.getDay() == 0 || date.getDay() == 6,
                    isAnotherMonth: date.getMonth() != renderDate.getMonth(),
                    isCurentDate: date.isSameDate(currentSelectedDate),
                    isToday: date.isSameDate(new Date())
                };

                xWeek.days.push(xDay);
            }

            // Определяем первый день следующей недели
            firstDay = lastDay.addDays(1);
        }

        return month
    }

    showMonthView = (show)=> {
        this.setState({ monthShow: show })
    }

//             <PickerThead
//    onClick={this.showMonthView.bind(this)}
//    onNext={() => this.nextOrPrevSelected(1)}
//    onPrev={() => this.nextOrPrevSelected(-1)}
//    head={monthNames[monthNumber] + ' ' + year} >
//    {dayNamesShort.map(name => <th className="dp-week-names">{name}</th>)}
//</PickerThead>

    renderHeader(monthNumber, year)
    {
        return (
            <thead>
                <tr>
                    {dayNamesShort.map(name => <th className="dp-week-names">{name}</th>)}
                </tr>
            </thead>
        )
        //(
        //    <thead>
        //        <tr>
        //            <th className="dp-prev"><span>‹</span></th>
        //            <th className="dp-switch" colSpan={5}>{monthNames[monthNumber]} {year}</th>
        //            <th className="dp-next"><span>›</span></th>
        //        </tr>
        //        <tr>
        //            {dayNamesShort.map(name => <th className="dp-week-names">{name}</th>)}
        //        </tr>
        //    </thead>
        //)
    }

    nextOrPrevSelected = (index) => {
        this.setState({
            renderDate: this.state.renderDate.addMonths(index)
        })
    }

    daySelected = (date) =>
    {
        var value = Globalize.format(date, 'd');

        setTimeout(() => {
            this.setState({
                currentSelectedDate: date,
                renderDate: date,
                isOpen: false,
                monthShow: false,
                value: Globalize.format(date, 'd')
            })
            this._imputNode.blur()
        },10)

        this.props.handleChange(value, this.props.name)
    }

    monthSelected = (newDate) => {
        this.setState({ renderDate: newDate, monthShow: false })
    }

    renderDay(day: Day)
    {
        const className = classnames('dp-day', {
            'dp-week': day.isWeekend,
            'dp-a-month': day.isAnotherMonth,
            'dp-tody': day.isToday,
            'dp-current': day.isCurentDate
        })

        return <td className={className} onClick={() => this.daySelected(day.date)}>{day.dayNubmer}</td>
    }

    renderDays(weeks: Week[]) {

        return (
            <tbody>
                {weeks.map(x => <tr>{x.days.map(day => this.renderDay(day))}</tr>)}
            </tbody>
        )
    }

    handleClickBody= (e) => {
        if (this.state.monthShow && this._monthNode && !this._monthNode.contains(e.target)) {
            this.setState({ monthShow: false })
        }
    }

    //handleToDayClick = () => {
    //    var date = new Date();

    //}

    handleClear = () => {

        setTimeout(() => {
            this.setState({
                isOpen: false,
                monthShow: false,
                value: ''
            })
            this._imputNode.blur()
        }, 50)

        this.props.handleChange('', this.props.name);
    }

    renderCalendar()
    {
      //  var day = daySelected ? Globalize.parseDate(daySelected) : new Date();

        //this.setState({
        //    day: day.getDate(),
        //    year: day.getFullYear(),
        //    month: day.getMonth()
        //})

        var month = this.getCalendarData();

        return (
            <div className="r-dp-days" onClick={this.handleClickBody}>
                <div className="db-main-header">
                    <table className="dp dp-header">
                        <tr>
                            <th className="dp-prev" onClick={() => this.nextOrPrevSelected(-1)}><span className="icon-angle-left"></span></th>
                            <th className="dp-switch" colSpan={5} onClick={() => this.showMonthView(true)}>
                                {monthNames[month.monthNumber] + ' ' + month.year}
                            </th>
                            <th className="dp-next" onClick={() => this.nextOrPrevSelected(1)}><span className="icon-angle-right"></span></th>
                        </tr>
                    </table>
                    {!this.props.isLayout && this.state.monthShow ? <div ref={(node) => { this._monthNode = node; }}>
                        <MonthView
                        onChangeMonth={this.monthSelected}
                        date={this.state.renderDate}
                        onCloseMonth={() => this.showMonthView(false)} /></div> : null}
                </div>
                <div className="main-db-days">
                    <table className="dp">
                        {this.renderHeader(month.monthNumber, month.year)}
                        {this.renderDays(month.weeks)}
                    </table>
                </div>
                {!this.props.isLayout && <div className="db-action">
                    <span onClick={() => this.daySelected(new Date())}>Сегодня</span>
                    <span onClick={this.handleClear} >Очистить</span>
                </div>}
            </div>
        )
    }

    openCalendar = (open) =>
    {
        this.setState({
            isOpen: open ? open : true
        })
    }

    _monthNode: any;
    _node: any
    _imputNode: any;

    onFocusOut = (e) => {
        if (this.state.isOpen)
        {
            var target = e.relatedTarget || document.activeElement

            if (!this._node.contains(target))
            {
                this.setState({ isOpen: false, monthShow: false })

                var date = Globalize.parseDate(this.state.value);

                if (!date)
                    this.props.handleChange('', this.props.name);
            }

            var date = Globalize.parseDate(this.state.value);

            if (!date)
            {
                this.setState({ value: '' })
              //  this.props.handleChange('', this.props.name);
            }
        }

       
    }

    handleChangeInpur = (value) => {
        var date = Globalize.parseDate(value);

        if (date) {
            this.setState({
                currentSelectedDate: date,
                renderDate: date,
                value: value
            })

            this.props.handleChange(value, this.props.name);
        }
        else
        {
            this.setState({
                value: value
            })
        }
    }

    render() {

        if (this.props.hide)
            return null;

        if (this.props.isLayout)
            return <div className="r-dp-main"><div className="r-dp">{this.renderCalendar()}</div></div>

        return (<div className="r-dp-main">
            <div className="main-field">
                <label className="field-label" ref={(node) => { this._node = node; }} tabIndex={1} onBlur={this.onFocusOut}>
                    {this.props.title && <span className="field-title">{this.props.title}</span>}
                    <div className="field-input">
                        <input type="text"
                            onFocus={this.openCalendar}
                            value={this.state.value}
                            onChange={(e) => this.handleChangeInpur(e.target.value)}
                            disabled={this.props.disabled}
                            ref={(node) => { this._imputNode = node; }} />
                        <span className="icon-calendar" />
                        <div className="r-dp">
                            {this.state.isOpen ? this.renderCalendar() : null}
                        </div>
                    </div>
                    {this.props.children}
                </label>
            </div>
            </div>
        )
    }
}


const Input = () => {
    return (
        <div className="main-field-range">
            <div className="field-title">Дата загрузки из УС</div>
            <div className="field-input">
                <input
                    type="text"
                />
                <span className="icon-calendar"  />
            </div>
            <span className="range-title">-</span>
            <div className="field-input">
                <input
                    type="text"
                />
                <span className="icon-calendar" />
            </div>
        </div>
    )
}