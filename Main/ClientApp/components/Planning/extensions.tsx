import * as React from 'react'
import * as classnames from 'classnames';
import moment, { Moment } from 'moment'
import { TimelineTypeEnum, getTimeLineDescription } from '../../common'


interface TimelineProps {
    tl: TimeLine
    hover: number
    contextMenu?: (e, data) => void
    onClick?: (tl: TimeLine) => void
    isNight: boolean

    size: number
}

export const Timeline = ({ tl, hover, isNight, contextMenu, onClick, size }: TimelineProps) => {

    var dateStart = moment(tl.starTime, 'YYYY-MM-DDTHH:mm:ss')
    dateStart.seconds(0)


    var dateEnd: Moment = null;
    var isPoint = false;

    if (tl.endTime != null) {
        dateEnd = moment(tl.endTime, 'YYYY-MM-DDTHH:mm:ss')

        var dateS = dateStart.toDate();
        var dateE = dateEnd.toDate();

        if (dateS > dateE)
            dateEnd.add(1, 'days')
    }
    else {
        isPoint = true;
        dateEnd = moment(tl.starTime, 'YYYY-MM-DDTHH:mm:ss')
        dateEnd.add(1, 'minutes');
    }

    dateEnd.seconds(0);

    var hourDateStart = dateStart.hours();
    var hourDateEnd = dateEnd.hours();
  //  var currentDate = moment(`0001-01-01T${hover > 9 ? hover : '0' + hover}:00:00`);

    //hourDateStart == hover && (!isNight || isNight && hourDateEnd != hover)

    var isStartNextDay;// = hover === 3 && (hourDateStart < hover && hourDateEnd > hover);

    if (hover === 3) {



        const hasCurrentDay = dateEnd.days() === 1;

        var currentDate = moment(`0001-01-${hasCurrentDay ? '01' :'02'}T03:00:00`);

        isStartNextDay = currentDate.isBetween(dateStart, dateEnd);
    }

    if (hourDateStart == hover && (!isNight || isNight && hourDateEnd != hover) || isStartNextDay) {

        var endDateGraph = moment(`0001-01-02T03:00:00`); //Дата окончания графа
        var currentDateEnd = dateEnd;
        var currentDateStart = isStartNextDay ? moment(`0001-01-01T03:00:00`) : dateStart;

        if (isStartNextDay)
            currentDateEnd.days(1);

        if (!endDateGraph.isSameOrAfter(currentDateEnd))
            currentDateEnd = endDateGraph;
        else if (isNight && hourDateEnd > 3)
            currentDateEnd = moment(`0001-01-01T0${hourDateEnd > 3 ? 3 : hourDateEnd}:00:00`);


        var minutes = moment.duration(currentDateEnd.diff(currentDateStart)).asMinutes();

        var mStart = currentDateStart.minutes();
        var menuteStart = currentDateStart.format('mm');
        var minuteEnd = currentDateEnd.format('mm')

        var className = classnames('main-time-line', `type-${tl.enumType}`, {
            'point': isPoint,
            'changed': tl.changed
        })

        var timeBorder = null;

        if (tl.enumType == TimelineTypeEnum.TimeRangeTo2
            || tl.enumType == TimelineTypeEnum.TimeSto
            || tl.enumType == TimelineTypeEnum.TimeRangeTrip
            || tl.enumType == TimelineTypeEnum.TimeRangeTripTransfer
        ) {
            timeBorder = <>
                <div className="g-time-minute s">
                    {tl.enumType == TimelineTypeEnum.TimeRangeTrip || tl.enumType == TimelineTypeEnum.TimeRangeTripTransfer ? <span className="g-time-value g-text">{tl.additionalTimeLineData.tripStartStationName}</span> : null}
                    <span className="g-time-value">{menuteStart}</span>
                    <span className="g-time-line-m" style={{ background: tl.color }}></span>
                </div>
                <div className="g-time-minute e">
                    {tl.enumType == TimelineTypeEnum.TimeRangeTrip || tl.enumType == TimelineTypeEnum.TimeRangeTripTransfer ? <span className="g-time-value g-text">{tl.additionalTimeLineData.tripEndStationName}</span> : null}
                    <span className="g-time-value">{minuteEnd}</span>
                    <span className="g-time-line-m" style={{ background: tl.color }}></span>
                </div>
            </>
        }

        var dateTip = isPoint ? dateStart.format('HH:mm') : `${dateStart.format('HH:mm')} - ${dateEnd.format('HH:mm')}`;

        var tip = "<div><b>" + getTimeLineDescription(tl.enumType) + "</b><p>" + dateTip + "</p></div>"
        var data = tl.additionalTimeLineData;

        if (tl.enumType == TimelineTypeEnum.TimeRangeTrip || tl.enumType == TimelineTypeEnum.TimeRangeTripTransfer) {

            var stantions = data && data.stantions
                ? data.stantions.map(x => x.canseled ? `<i>${x.name} ${x.time}</i>` : `<p>${x.name} ${x.time}</p>`).join('')
                : '';

            tip = `<div><b>${getTimeLineDescription(tl.enumType)}</b><p>${dateTip}</p><p>${data.tripStartStationName} - ${data.tripEndStationName}</p>${stantions}</div>`
        }
        else if (tl.enumType == TimelineTypeEnum.ChangeTrain) {
            tip = `<div><b>${getTimeLineDescription(tl.enumType)}</b><p>${dateTip}</p><p>Станция: ${data.tripStartStationName}</p><p>Поезд: ${data.description}</p></div>`
        }
        else if (tl.enumType == TimelineTypeEnum.TimeTaskCritical) {
            tip = `<div><b>${getTimeLineDescription(tl.enumType)}</b><p>${data.description}</p></div>`
        }
        else if (tl.enumType == TimelineTypeEnum.Surrender || tl.enumType == TimelineTypeEnum.Inspection) {
            tip = `<div><b>${data.description}</b></div>`
        }
        else if (tl.enumType == TimelineTypeEnum.TimeRangeCancelTrip) {

            var stantions = data.stantions.map(x => `<p>${x.name} ${x.time}</p>`).join('');

            tip = `<div><b>${getTimeLineDescription(tl.enumType)}</b><p>${dateTip}</p>${stantions}</div>`
        }
        else if (tl.enumType == TimelineTypeEnum.TimeRangeTo1
            || tl.enumType == TimelineTypeEnum.TimeRangeTo2
            || tl.enumType == TimelineTypeEnum.TimeSto
            || tl.enumType == TimelineTypeEnum.TimeBrigade
        ) {

            var description = data ? tl.additionalTimeLineData.description : '';

            tip = `<div><b>${getTimeLineDescription(tl.enumType)}</b><p>${dateTip}</p>${description}</div>`
        }
        //onContextMenu={(e) => contextMenu(e, { tl: tl, hover: hover })}
        //onClick

        const onClickTimeLine = (e) => {
            onClick(tl)
        }

        const onContextMenuTimeLine = (e) => {
            contextMenu(e, { tl: tl, hover: hover })
        }

        var props: any = {};

        if (onClick)
            props.onClick = onClickTimeLine;

        if (contextMenu)
            props.onContextMenu = onContextMenuTimeLine;

        return <div {...props} data-for='timeline' data-tip={tip} onClick={(e) => onClick(tl)} className={className} style={{ width: minutes * size, left: mStart * size }}>
            <div className="g-time-line" style={{ background: tl.color }}>
                <div className="g-info">{tl.description}</div>
                {timeBorder}
            </div>
        </div>
    }

    return null;
}