import * as React from 'react'
import * as classnames from 'classnames'
import PickerThead from './PickerThead' 
import YearView from './YearView' 
import { Button } from '../button'

var months = Globalize.culture().calendar.months.namesAbbr;
var monthNames = Globalize.culture().calendar.months.names;

interface MonthViewProps
{
    date?: Date
    onChangeMonth: (newDate: Date) => any
    onCloseMonth: () => void
}


interface MonthViewState {
    date: Date
}

export default class MonthView extends React.Component<MonthViewProps, MonthViewState> {

    constructor(props: MonthViewProps) {
        super(props)

        this.state = {
            date: props.date
        }
    }

    //componentWillUnmount = () =>{
    //    document.removeEventListener('click', this.handleClick, false);
    //}

    //handleClick = (e) => {
    //    if (!this._node.contains(e.target)) {
    //       // this.props.onCloseMonth();
    //    }
    //}

    handleChangeMonth= (monthIndex: number) => {
        var newDate = new Date(this.state.date.getFullYear(), monthIndex)

        this.setState({
            date: newDate
        })
    }

    renderMonth(monthIndex: number) {

        const className = classnames({
            'dp-current': this.state.date.getMonth() == monthIndex
        })

        return <td className={className} onClick={() => this.handleChangeMonth(monthIndex)}>{months[monthIndex]}</td>
    }

    nextOrPrevYear(index: number) {
        this.setState({
            date: this.state.date.addYears(index)
        })
    }

    handleChangeYear = (date) => {
        this.setState({
            date: date
        })
    }

    render() {

        var date = this.state.date;
        const year = this.state.date.getFullYear();
        const monthPrewiews = [[0, 1, 2,3], [4, 5, 6,7], [8, 9, 10,11]]

        var curent = monthNames[date.getMonth()] + ' ' + date.getFullYear();

        return (
            <div className="r-dp-months">
                <div className="r-dp-months-main"  >
                    <table className="db-month">
                        <tbody>
                            <tr><td colSpan={4}>{curent}</td></tr>
                            {monthPrewiews.map((i) => <tr>{i.map(month => this.renderMonth(month))}</tr>)}
                        </tbody>
                    </table>
                    <YearView date={this.state.date} onChangeYear={this.handleChangeYear} /> 
                </div>
                <div className="db-action">
                    <span onClick={() => this.props.onChangeMonth(this.state.date)}>ОК</span>
                    <span onClick={this.props.onCloseMonth}>Закрыть</span>
                </div>
            </div> 
        );
    }
}
//<table>
//    <PickerThead
//        head={year}
//        onNext={() => this.nextOrPrevYear(1)}
//        onPrev={() => this.nextOrPrevYear(-1)}
//        onClick={() => this.setState({ showYear: true })}
//    />
//</table>