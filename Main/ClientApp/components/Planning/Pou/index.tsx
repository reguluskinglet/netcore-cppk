import * as React from 'react'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../../store';
import * as DialogStore from '../../../store/DialogStore';
import * as CommonStore from '../../../store/CommonStore';
import { GridType, journalTitleFilter, ReportType, dayOfWeek,getTimeLineDescription } from '../../../common'
import { Loading } from '../../../UI/loading'
import { isEqual } from 'lodash'
import { Table } from '../../../UI/tables';
import { DatePicker } from '../../../UI/datepicker';
import { Filter, DayOfWeek } from '../../Common'
import { RouteComponentProps } from 'react-router'
import { Button } from '../../../UI/button'
import * as classnames from 'classnames';
import * as store from './store'
import moment, { Moment } from 'moment'
import Scrollbars from 'react-custom-scrollbars';
import { positionValues } from 'react-custom-scrollbars';
import { ToolBar, ToolBarButton } from '../../../UI/toolbar';
import { Menu } from 'antd';
import { TimelineTypeEnum } from '../../../common'
import { Data } from '../../../UI/table';
import ReactTooltip from 'react-tooltip';
import { Timeline } from '../extensions'
import * as DispatcherStore from '../Dispatcher/store'

interface State {
    width: number
    height: number

    contextMenu: ContextMenuProps

    date?: any

    size?: number
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

const timeLineTypes = Object.keys(TimelineTypeEnum).filter(k => typeof TimelineTypeEnum[k as any] === "number");

const timeLineTypesText = {
    TimeRangeTo2:'ТО-2',
    TimeRangeTrip:'Рейс',
    TimeTaskCritical:'Критичный инцидент',
    TimeSto:'СТО',
    TimeRangeTo1:'ТО-1',
    Inspection:'Приемка поезда',
    Surrender:'Сдача поезда',
    Channeling:'Постановка на канал',
    EntryDepo:'Вход в Депо',
    TimeBrigade:'Посадка бригады',
    TimeRangeTripTransfer:'Перегонный рейс',
    TimeRangeCancelTrip:'Отмена Рейса',
    ChangeTrain:'Изменен поезд'
}

class Pou extends React.Component<Props, State> {

    constructor(props) {
        super(props)

        this.state = {
            width: $(window).width(),
            height: $(window).height(),
            date: this.getDate(new Date()),
            size:2,
            contextMenu: {
                visible: false,
                visibleTwo: false,
                visibleTre: false
            }
        };
    }

    getDate = (date) => {
        return moment(date).format('YYYY-MM-DDT00:00:00') + 'Z'
    }

    updateDimensions=()=> {
        this.setState({ width: $(window).width(), height: $(window).height() });
    }

    componentDidMount() {
        window.addEventListener("resize", this.updateDimensions);
    }

    componentWillMount() {
        this.props.get()
    }

    componentWillUnmount() {
    window.removeEventListener("resize", this.updateDimensions);
    }



    componentWillReceiveProps(nextProps: Props) {

      

    }

    getRows = (timeLines: TimeLine[], routeId: number) => {

        var hourLines = []

        var size = this.state.size;

        for (var i = 0, hover = 3; i < 24; i++ , hover++) {

            var timeLine = null;

            if (hover > 23) 
                hover = 0;

            if (timeLines)
                timeLine = timeLines.map((t, index) => <Timeline size={size} key={index} isNight={i - hover > 0} onClick={(tl) => this.handleSelectTimeLine(tl, routeId)} tl={t} hover={hover} />)

            var result = {
                //routeId: data.routeId,
                hover: i - hover > 0 ? hover * -1 : hover
            }

            hourLines.push(<Cell size={size} data={result} contextMenu={this.contextMenu} timeLine={timeLine} />)
        }

        return hourLines;
    }

    contextMenuLine = (e, data) => {

        //e.preventDefault();

        //this.setState({
        //    contextMenu: {
        //        visible: false,
        //        visibleTre: false,
        //        visibleTwo: true,
        //        clientY: e.clientY,
        //        clientX: e.clientX,
        //        data: data.tl.id
        //    }
        //})
    }


    contextMenu = (e, data) => {
        //if (!$(e.target).closest('.main-time-line').length) {
        //    e.preventDefault();

        //    this.setState({
        //        contextMenu: {
        //            visible: true,
        //            visibleTre: false,
        //            clientY: e.clientY,
        //            clientX: e.clientX,
        //            data: data
        //        }
        //    })
        //}
    }

    getGraph = () => {

        var hourLines = [

        ]

        var startTime = 3,
            endTime=23;

        var size = 60 * this.state.size;

        var style = {
            width: size
        }

        for (var i = 3; i <= 23; i++) {

            var sep = i >= 10 ? i : '0' + i;
            var text = `${sep}:00`

            hourLines.push(<div className="g-hour" style={style}>{text}</div>)
        }

        for (var i = 0; i < 3; i++) {

            var sep = i >= 10 ? i : '0' + i;
            var text = `${sep}:00`

            hourLines.push(<div className="g-hour" style={style}>{text}</div>)
        }

         

        return <div ref={(node) => { this._mainGraphHeader = node; }} className="g-graph-header" style={{ position: "absolute", top:-27 }}>{hourLines}</div>
    }

    _mainGraphHeader: HTMLDivElement

    handleScroll = (e: positionValues) => {
        this._mainGraphHeader.style.top = `${-27 + e.scrollTop}px`;
        this._mainHeaderGroup.scrollTo({ top: e.scrollTop+1 })
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

        this.props.addDispatcher(null, this.reload, data);

        //switch (event) {
        //    case MenuEvents.EditRoute:
        //        this.props.getRouteById(this.state.contextMenu.data, this.reload)
        //        break;
        //    case MenuEvents.DeleteRoute:
        //        this.props.removeRoute(data, this.reload)
        //        break;
        //    case MenuEvents.AddReis:
        //        this.props.getTrips(this.props.match.params.id, data.routeId, this.reload)
        //        break;
        //    case MenuEvents.AddTo2:
        //    case MenuEvents.AddSto:
        //        this.props.showDialog('timlineEvent', {
        //            checkListType: event == MenuEvents.AddTo2 ? TimelineTypeEnum.TimeRangeTo : TimelineTypeEnum.TimeSto,
        //            routeId: data.routeId,
        //            hover: data.hover
        //        }, this.reload)
        //        break;
        //    case MenuEvents.DeleteTimeLine:
        //        this.props.removeEvent(data, this.reload)
        //        break;
        //    default:
        //        console.log(this.state.contextMenu.data)
        //}

        this.setState({
            contextMenu: {
                visible: false,
                visibleTre: false,
                visibleTwo: false
            }
        })
    }

    reload = () => {
        this.props.get(this.props.data.date)
    }

    get turnoverId(): number {
        return this.props.match.params.id;
    }
//                    <div className="g-header" >
//    График-оборотов поездов {result.turnoverName}.Направление:{result.directionName}
//    <DayOfWeek selectedDays={result.days} />
//</div>

//                        <Scrollbars style={{ width: this.state.width - 243, height: this.state.height - 296 }}>
//    <div className="g-graph">
//        {this.getGraph()}
//        {rows.map((x, i) => <div key={i} className="g-row">{this.getRows(x)}</div>)}
//    </div>
//</Scrollbars>

    handleSelectTimeLine = (tl: TimeLine, routeId: number) => {

        var dialogName;

        switch (tl.enumType) {
            case TimelineTypeEnum.TimeRangeTo2:
            case TimelineTypeEnum.TimeSto:
                dialogName = 'inspectionPou'
                break;
            case TimelineTypeEnum.TimeBrigade:
                dialogName = 'brigadePou'
                break;
            case TimelineTypeEnum.TimeRangeTrip:
            case TimelineTypeEnum.TimeRangeTripTransfer:
                dialogName = 'tripDialogPou'
                break;
        }

        if (dialogName)
            this.props.getTimeRangeData(tl.id, tl.enumType, routeId, dialogName, this.reload)
    }

    handleChangeDate = (newValue, field) => {

        var date = moment(newValue,'DD.MM.YYYY').toDate();

        this.props.get(date)
    }



    renderSizeBtn = (text, size) => {
        var className = classnames({
            'active': this.state.size === size
        })

        return <span className={className} onClick={() => this.setState({ size: size })}>{text}</span>
    }



    handleScrollTop = (e) => {
        console.log(e)
    }

    _mainHeaderGroup: HTMLDivElement

    render() {

        if (this.props.isLoaded === GridType.pou) {

            var result = this.props.data.data;
            var rows = result.data;
            var date = moment(this.props.data.date).format('DD.MM.YYYY');
            const { clientX, clientY, visible, visibleTwo, visibleTre } = this.state.contextMenu
            // <ContextMenuItem text="Изменить" handleSelect={() => this.handleSelectMenu(MenuEvents.EditTimeLine)} />
            return <div>
                <ContextMenu clientX={clientX} clientY={clientY} visible={visibleTre}>
                    <ContextMenuItem text="Добавить данные о Ремонте в Депо" handleSelect={() => this.handleSelectMenu(MenuEvents.EditRoute)} />
                </ContextMenu>
                <div className="g-main-graph-info clear-x">
                    <div className="g-graph-caledar">
                        <DatePicker value={date} isLayout handleChange={this.handleChangeDate} />
                    </div>
                    <h1 className="title">График оборота поездов c изменениями на {date}</h1>
                    <div className="g-size">
                        <p>Масштаб:</p>
                        <div className="g-size-btn">
                            {this.renderSizeBtn('1 : 1', 2)}
                            {this.renderSizeBtn('1 : 2', 4)}
                            {this.renderSizeBtn('1 : 3', 6)}
                        </div> 
                    </div>
                    <div className="g-graph-legend">
                        {timeLineTypes.map((x, i) => <div key={i} className="main-legend">
                            <span className={`legend type-${TimelineTypeEnum[x]}`}></span>
                            <span className="g-text">{timeLineTypesText[x]}</span>
                        </div>)}
                    </div>
                </div>
                {result.data && result.data.length ? <div className="g-main-graph pou">
                    <div className="g-graph fix">
                        <div className="g-graph-header">
                            <div className="g-hour">Маршрут</div>
                            <div className="g-hour" style={{ minWidth: 55, width: 55 }}></div>
                            <div className="g-hour">Поезд</div>
                        </div>
                        <div ref={(node) => { this._mainHeaderGroup = node; }} className="main-g-group" style={{ height: this.state.height - 405 }}>
                        {result.data.map((d, i) => <div key={i} className="g-row-group" onContextMenu={(e) => this.contextMenuHeder(e, { routeName: d.routeName, trainName: d.trains.change.current })}>
                            <div className="g-info">{d.routeName}</div>
                            <div className="group-row">
                                <div className="g-row g-border">
                                    <div className="g-hour">План</div>
                                </div>
                                <div className="g-row">
                                    <div className="g-hour">Факт</div>
                                </div>
                            </div>
                            <div className="group-row">
                                <div className="g-row g-border">
                                    <div className="g-hour" style={{ width: 88 }}>{d.trains.planed}</div>
                                </div>
                                <div className="g-row">
                                    <div className="g-hour train" style={{ width: 88 }}>
                                        {d.trains.change.previous.map(x => <div className="prev">{x}</div>)}
                                        <div className="current">{d.trains.change.current}</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                            )}
                        </div>
                    </div>
                    <Scrollbars style={{ width: this.state.width - 260, height: this.state.height - 378 }} onUpdate={this.handleScroll}
                        renderThumbVertical={({ style, ...props }: any) => (
                            <div style={{ ...style, zIndex: 100, cursor: 'pointer', backgroundColor: '#0003' }}{...props} />
                        )}
                        renderThumbHorizontal={({ style, ...props }: any) => (
                            <div style={{ ...style, zIndex: 100, cursor: 'pointer', backgroundColor: '#0003' }}{...props} />
                        )}
                    >
                        <div className="g-graph" style={{ marginTop: 27 }}>
                            {this.getGraph()}
                            {rows.map((x, i) => <>
                                <div className="g-row">{this.getRows(x.planTimeLines, x.planedRouteId)}</div>
                                <div className="g-row">{this.getRows(x.changeTimeLines, x.planedRouteId)}</div>
                            </>)}
                        </div>
                    </Scrollbars>
                </div>
                    : <h6 className="g-no-data">На {date} НЕТ ЗАПЛАНИРОВАННЫХ МАРШРУТОВ</h6>}
                <ReactTooltip id='timeline' type="info" html={true}/>
            </div>
        } else {
            return <Loading />
        }
    }
}



const provider = provide((state: ApplicationState) => ({ data: (state.common.data as { data: PouData, date: any }), isLoaded: state.common.isLoaded }),
    { ...DialogStore.actionCreators, ...store.actionCreators, ...DispatcherStore.actionCreators }
).withExternalProps<RouteComponentProps<{ id: number }>>()

type Props = typeof provider.allProps;

export default provider.connect(Pou);

interface ContextMenuProps
{
    clientX?: number
    clientY?: number
    visible: boolean
    visibleTwo?: boolean
    visibleTre?: boolean
    data?: any
}

const Cell = ({ timeLine, contextMenu, data, size }) => {
    return <div className="g-hour" style={{ minWidth: 60 * size }} onContextMenu={(e) => contextMenu(e, data)}>{timeLine}</div>
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