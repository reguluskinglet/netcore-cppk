import * as React from 'react'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../../../store';
import * as DialogStore from '../../../../store/DialogStore';
import * as CommonStore from '../../../../store/CommonStore';
import { GridType, journalTitleFilter, ReportType, dayOfWeek } from '../../../../common'
import { Loading } from '../../../../UI/loading'
import { isEqual } from 'lodash'
import { Table } from '../../../../UI/tables';
import { Filter, DayOfWeek } from '../../../Common'
import { RouteComponentProps } from 'react-router'
import { Button } from '../../../../UI/button'
import * as classnames from 'classnames';
import * as store from './store'
import moment from 'moment'
import Scrollbars from 'react-custom-scrollbars';
import { ToolBar, ToolBarButton } from '../../../../UI/toolbar';
import { Menu } from 'antd';
import { TimelineTypeEnum } from '../../../../common'
import { Timeline } from '../../extensions'
import ReactTooltip from 'react-tooltip';

interface State {
    width: number
    height: number

    contextMenu: ContextMenuProps
}

enum MenuEvents
{
    AddReis,
    AddTo2,
    AddSto,

    EditRoute,
    DeleteRoute,

    EditTimeLine,
    DeleteTimeLine
}

class Graph extends React.Component<Props, State> {

    constructor(props) {
        super(props)

        this.state = {
            width: $(window).width(),
            height: $(window).height(),
            contextMenu: {
                visible: false,
                visibleTwo: false,
                visibleTre: false
            }
        };
    }

    updateDimensions=()=> {
        this.setState({ width: $(window).width(), height: $(window).height() });
    }

    componentDidMount() {
        window.addEventListener("resize", this.updateDimensions);
    }

    componentWillMount() {
        this.props.get(this.props.match.params.id, 0, 9999)
    }

    componentWillUnmount() {
    window.removeEventListener("resize", this.updateDimensions);
    }



    componentWillReceiveProps(nextProps: Props) {

      

    }

    getRows = (data: GraphDataRoute) => {

        var hourLines = []

        for (var i = 0, hover = 3; i < 24; i++ , hover++) {

            var timeLine = null;

            if (hover > 23) 
                hover = 0;

            if (data.timeLines)
                timeLine = data.timeLines.map((t, index) => <Timeline size={2} key={index} isNight={i - hover > 0} contextMenu={this.contextMenuLine} tl={t} hover={hover} />)

            var result = {
                routeId: data.routeId,
                hover: i - hover > 0 ? hover * -1 : hover
            }

            hourLines.push(<Cell data={result} contextMenu={this.contextMenu} timeLine={timeLine} />)
        }

        return hourLines;
    }

    contextMenuLine = (e, data) => {

        e.preventDefault();

        this.setState({
            contextMenu: {
                visible: false,
                visibleTre: false,
                visibleTwo: true,
                clientY: e.clientY,
                clientX: e.clientX,
                data: { id: data.tl.id, enumType: data.tl.enumType, tripOnRouteId: data.tl.tripOnRouteId}
            }
        })
    }


    contextMenu = (e, data) => {
        if (!$(e.target).closest('.main-time-line').length) {
            e.preventDefault();

            this.setState({
                contextMenu: {
                    visible: true,
                    visibleTre: false,
                    clientY: e.clientY,
                    clientX: e.clientX,
                    data: data
                }
            })
        }
    }

    getGraph = () => {

        var hourLines = [

        ]

        var startTime = 3,
            endTime=23;


        for (var i = 3; i <= 23; i++) {

            var sep = i >= 10 ? i : '0' + i;
            var text = `${sep}:00`

            hourLines.push(<div className="g-hour">{text}</div>)
        }

        for (var i = 0; i < 3; i++) {

            var sep = i >= 10 ? i : '0' + i;
            var text = `${sep}:00`

            hourLines.push(<div className="g-hour">{text}</div>)
        }


        return <div className="g-graph-header">{hourLines}</div>
    }

    contextMenuHeder = (e, data) => {

        e.preventDefault();

        this.setState({
            contextMenu: {
                visible: false,
                visibleTre: true,
                clientY: e.clientY,
                clientX: e.clientX,
                data: data
            }
        })
    }

    handleSelectMenu = (event: MenuEvents) => {
        //routeId: data.routeId,
        //    hover: hover

        var data = this.state.contextMenu.data;

        switch (event) {
            case MenuEvents.EditRoute:
                this.props.getRouteById(this.state.contextMenu.data, this.reload)
                break;
            case MenuEvents.DeleteRoute:
                this.props.removeRoute(data, this.reload)
                break;
            case MenuEvents.AddReis:
                this.props.getTrips(this.props.match.params.id, data.routeId, this.reload)
                break;
            case MenuEvents.AddTo2:
            case MenuEvents.AddSto:
                this.props.showDialog('timlineEvent', {
                    checkListType: event == MenuEvents.AddTo2 ? TimelineTypeEnum.TimeRangeTo2 : TimelineTypeEnum.TimeSto,
                    routeId: data.routeId,
                    hover: data.hover
                }, this.reload)
                break;
            case MenuEvents.DeleteTimeLine:
                this.props.removeEvent(data, this.reload)
                break;
            default:
                console.log(this.state.contextMenu.data)
        }

        this.setState({
            contextMenu: {
                visible: false,
                visibleTre: false,
                visibleTwo: false
            }
        })
    }

    reload = () => {
        this.props.get(this.props.match.params.id, 0, 9999)
    }

    get turnoverId(): number {
        return this.props.match.params.id;
    }

    render() {

        if (this.props.isLoaded === GridType.goep) {

            var result = this.props.data;
            var rows = result.data;

            const { clientX, clientY, visible, visibleTwo, visibleTre } = this.state.contextMenu
            // <ContextMenuItem text="Изменить" handleSelect={() => this.handleSelectMenu(MenuEvents.EditTimeLine)} />
            return <div>
                <ContextMenu clientX={clientX} clientY={clientY} visible={visible}>
                    <ContextMenuItem text="Добавить Рейс" handleSelect={() => this.handleSelectMenu(MenuEvents.AddReis)} />
                    <ContextMenuItem text="Добавить ТО-2" handleSelect={() => this.handleSelectMenu(MenuEvents.AddTo2)} />
                    <ContextMenuItem text="Добавить СТО" handleSelect={() => this.handleSelectMenu(MenuEvents.AddSto)} />
                </ContextMenu>
                <ContextMenu clientX={clientX} clientY={clientY} visible={visibleTwo}>
                    <ContextMenuItem text="Удалить" handleSelect={() => this.handleSelectMenu(MenuEvents.DeleteTimeLine)} />
                </ContextMenu>
                <ContextMenu clientX={clientX} clientY={clientY} visible={visibleTre}>
                    <ContextMenuItem text="Изменить" handleSelect={() => this.handleSelectMenu(MenuEvents.EditRoute)} />
                    <ContextMenuItem text="Удалить" handleSelect={() => this.handleSelectMenu(MenuEvents.DeleteRoute)} />
                </ContextMenu>
                <div className="g-header" >
                    График-оборотов поездов {result.turnoverName}.Направление:{result.directionName}
                    <DayOfWeek selectedDays={result.days} />
                </div>
                <ToolBar>
                    <ToolBarButton label="Добавить маршрут" onClick={() => this.props.showDialog('graphaaddroute', { turnoverId: this.turnoverId }, this.reload)} />
                </ToolBar>
                <div className="g-main-graph" id="goep">
                    <Scrollbars style={{ width: this.state.width-20 , height: this.state.height -20  }}>
                    <div className="g-graph fix">
                        <div className="g-graph-header">
                            <div className="g-hour" style={{ width: 120 }}>Пробег</div>
                            <div className="g-hour" style={{ width: 120 }}>Номер</div>
                        </div>
                        {result.data.map((d, i) => <div key={i} className="g-row g-border" onContextMenu={(e) => this.contextMenuHeder(e, d.routeId)}>
                            <div className="g-hour">{d.mileage}</div>
                            <div className="g-hour">{d.routeName}</div>
                        </div>)}
                    </div>
                    
                        <div className="g-graph fix">
                            {this.getGraph()}
                            {rows.map((x, i) => <div key={i} className="g-row">{this.getRows(x)}</div>)}
                        </div>
                    </Scrollbars>
                </div>
                <ReactTooltip id='timeline' type="info" html={true} />
            </div>
        } else {
            return <Loading />
        }
    }
}

//interface TimelineProps
//{
//    tl: GraphTimeLine
//    hover: number
//    contextMenu: (e, data) => void
//    isNight: boolean
//}

//const Timeline = ({ tl, hover, isNight, contextMenu }: TimelineProps) => {
//    var dateStart = Globalize.parseDate(tl.starTime, 'yyyy-MM-ddTHH:mm:ssZ');
//    dateStart.setSeconds(0, 0);

//    var dateEnd = Globalize.parseDate(tl.endTime, 'yyyy-MM-ddTHH:mm:ssZ');
//    dateEnd.setSeconds(0, 0);

//    var hourDateStart = dateStart.getHours();
//    var hourDateEnd = dateEnd.getHours();

//    if (hourDateStart == hover && (!isNight || isNight && hourDateEnd != hover)) {
        
//        var endDateGraph = moment(`0001-01-02T04:00:00`); //Дата окончания графа

//        var xDateStart = moment(tl.starTime,'YYYY-MM-DDTHH:mm:ss');
//        var xDateEnd = moment(tl.endTime,'YYYY-MM-DDTHH:mm:ss');

//       // const hasCurrentDay = moment(xDateStart).isBefore(moment(xDateEnd))


//        if (!endDateGraph.isSameOrAfter(xDateEnd)) 
//            xDateEnd = endDateGraph;
//        else if (isNight && hourDateEnd > 3)
//            xDateEnd = moment(`0001-01-01T0${hourDateEnd > 4 ? 4 : hourDateEnd}:00:00`);
        
//        var minutes = moment.duration(xDateEnd.diff(xDateStart)).asMinutes();

//      //  var diff = (dateEnd.getTime() - dateStart.getTime()) / 1000;
//      //  diff /= 60;

//        var mStart = xDateStart.minutes();
//        var menuteStart = Globalize.format(dateStart, 'mm');
//        var minuteEnd = Globalize.format(dateEnd, 'mm');

//     //   var minutes = Math.abs(Math.round(diff));

//        return <div onContextMenu={(e) => contextMenu(e, { tl: tl, hover: hover })} className="main-time-line" style={{ width: minutes * 2, left: mStart * 2}}>
//            <div className="g-time-line" style={{ background: tl.color }}>
//                <div className="g-info">{tl.description}</div>
//                <div className="g-time-minute s">
//                    {tl.enumType == TimelineTypeEnum.TimeRangeTrip || tl.enumType == TimelineTypeEnum.TimeRangeTripTransfer ? <span className="g-time-value g-text fix">{tl.additionalTimeLineData.tripStartStationName}</span> : null}
//                    <span className="g-time-value">{menuteStart}</span>
//                    <span className="g-time-line-m" style={{ background: tl.color }}></span>
//                </div>
//                <div className="g-time-minute e">
//                    {tl.enumType == TimelineTypeEnum.TimeRangeTrip || tl.enumType == TimelineTypeEnum.TimeRangeTripTransfer ? <span className="g-time-value g-text fix">{tl.additionalTimeLineData.tripEndStationName}</span> : null}
//                    <span className="g-time-value">{minuteEnd}</span>
//                    <span className="g-time-line-m" style={{ background: tl.color }}></span>
//                </div>
//            </div>
//        </div>
//    }

//    return null;
//}


const provider = provide((state: ApplicationState) => ({ data: (state.common.data as GraphData), isLoaded: state.common.isLoaded }),
    { ...DialogStore.actionCreators, ...store.actionCreators }
).withExternalProps<RouteComponentProps<{ id: number }>>()

type Props = typeof provider.allProps;

export default provider.connect(Graph);

interface ContextMenuProps
{
    clientX?: number
    clientY?: number
    visible: boolean
    visibleTwo?: boolean
    visibleTre?: boolean
    data?: any
}

const Cell = ({ timeLine, contextMenu, data}) => {
    return <div className="g-hour" onContextMenu={(e) => contextMenu(e, data)}>{timeLine}</div>
}


const ContextMenuItem = ({ text, handleSelect, separator = false }) => {

    if (separator)
        return <div onClick={handleSelect} className="contextMenu--separator" />

    return <div onClick={handleSelect} className="contextMenu--option">{text}</div>
}

class ContextMenu extends React.Component<ContextMenuProps, any> {

    state = {
        visible: false,
    };

    componentDidMount() {
        document.addEventListener('click', this.handleClick);
        document.addEventListener('scroll', this.handleScroll);
    };

    componentWillUnmount() {
        document.removeEventListener('click', this.handleClick);
        document.removeEventListener('scroll', this.handleScroll);
    }

    componentWillReceiveProps(nextProps: ContextMenuProps) {

        if (nextProps.visible != this.state.visible) {
          
            this.setState({ visible: nextProps.visible })
        }

    }


    handleContextMenu = () => {

        const clickX = this.props.clientX;
        const clickY = this.props.clientY;
        const screenW = window.innerWidth;
        const screenH = window.innerHeight;
        const rootW = this.root.offsetWidth;
        const rootH = this.root.offsetHeight;

        const right = (screenW - clickX) > rootW;
        const left = !right;
        const top = (screenH - clickY) > rootH;
        const bottom = !top;

        if (right) {
            this.root.style.left = `${clickX + 5}px`;
        }

        if (left) {
            this.root.style.left = `${clickX - rootW - 5}px`;
        }

        if (top) {
            this.root.style.top = `${clickY + 5}px`;
        }

        if (bottom) {
            this.root.style.top = `${clickY - rootH - 5}px`;
        }
    };

    handleClick = (event) => {
        const { visible } = this.state;
        const wasOutside = !(event.target.contains === this.root);

        if (wasOutside && visible)
            this.setState({ visible: false, });
    };

    handleScroll = () => {
        const { visible } = this.state;

        if (visible) this.setState({ visible: false, });
    };



    root: any
    componentDidUpdate(prevProps, prevState) {
        this.handleContextMenu()
    }


    render() {

        //   setTimeout(this.handleContextMenu,10)

            return <div style={{ display: this.state.visible ? 'block' :'none'}} ref={ref => { this.root = ref }} className="contextMenu">
                {this.props.children}
            </div>
    };
}