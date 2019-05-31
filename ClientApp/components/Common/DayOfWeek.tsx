import './dayofweek.scss'
import * as React from 'react'
import * as classnames from 'classnames';
import { dayOfWeekShort, dayOfWeek } from '../../common'

interface Props {
    selectedDays: any[]
    onSelectedDay?: (day) => void
    hasLongDays?: boolean
}

const DayOfWeek = ({ selectedDays, onSelectedDay, hasLongDays }: Props) => {

    var keys = Object.keys(dayOfWeekShort);

    const selectedDay = (day) => {

        if (onSelectedDay)
            onSelectedDay(parseInt(day))
    }

    return (
        <div className="g-day-of-week">
            {keys.rotateShift(0).map((key, i) => <span onClick={() => selectedDay(key)}
                className={classnames({ 'activ': selectedDays && selectedDays.indexOf(parseInt(key)) >= 0 })}
                key={i}>{hasLongDays ? dayOfWeek[key] : dayOfWeekShort[key]}</span>)}
        </div>
        )

}

export default DayOfWeek