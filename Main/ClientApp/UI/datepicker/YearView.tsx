import * as React from 'react'
import * as classnames from 'classnames'
import PickerThead from './PickerThead' 

interface YearViewProps
{
    date: Date
    onChangeYear: (date: Date) => void
}

interface YearViewState {
    date: Date
}

export default class YearView extends React.Component<YearViewProps, YearViewState> {

    constructor(props) {
        super(props)

        this.state = {
            date: props.date
        }
    }

    componentWillReceiveProps(nextProps: YearViewProps) {

        var year = this.state.date.getFullYear();

        this.setState({
            date: new Date(year, nextProps.date.getMonth())
        })
    }

    renderYear(year: number) {

        var className = classnames({
            'dp-current': this.props.date.getFullYear() == year
        })

        return <td className={className} onClick={() => this.props.onChangeYear(new Date(year, this.state.date.getMonth()))}>{year}</td>
    }

    nextOrPrevYear=(index: number)=>{
        this.setState({
            date: this.state.date.addYears(10 * index)
        })
    }

    render() {

        let currentYear = this.state.date.getFullYear() - 1;
        const lastYear = currentYear + 11;
        const head = currentYear + '-' + lastYear;

        var yearPrewiews = [];

        for (var i = 0; i < 2; i++) {
            yearPrewiews[i] = [];
            for (var j = 0; j < 5; j++) {
                yearPrewiews[i][j] = currentYear++;
            }
        }

        return (
            <div className="r-dp-years">
                <table className="db-year">
                    <tbody>
                    {yearPrewiews.map((i, index) =>
                            <tr>
                            {index == 0 ? <td rowSpan={2} onClick={()=>this.nextOrPrevYear(-1)}><span className="icon-angle-left" /></td> : null}
                            {i.map((year, jindex) => this.renderYear(year))}
                            {index == 0 ? <td rowSpan={2} onClick={() => this.nextOrPrevYear(1)}><span className="icon-angle-right" /></td> : null}
                        </tr>)}
                    </tbody>
                </table>
            </div>
        );
    }
}

//<table>
//    <PickerThead
//        head={head}
//        onNext={() => this.nextOrPrevYear(1)}
//        onPrev={() => this.nextOrPrevYear(-1)}
//    />
//</table>