export { }

declare global {
    interface Date {
        getWeekNumber(): number;
        addDays(days: number): Date
        isSameDate(date: Date): boolean
        addMonths(value: number): Date
        addYears(value: number): Date
        //addHours(hour: number, minute: number): Date
        //addMinutes(value: number): Date
    }
    interface Array<T>
    {
        rotateShift(index: number): Array<T>
        insert(index: number, item: any)
    }
}

Date.prototype.getWeekNumber = function () {
    var d: any = new Date(+this);
    d.setHours(0, 0, 0, 0);
    d.setDate(d.getDate() + 4 - (d.getDay() || 7));
    var s: any = new Date(d.getFullYear(), 0, 1);
    return Math.ceil((((d - s) / 8.64e7) + 1) / 7);
};

Date.prototype.addDays = function (days: number): Date {

    var date = new Date(this.valueOf());
    date.setDate(date.getDate() + days);

    return date;
};

//Date.prototype.addHours = function (hour, minute) {
//    this.setHours(this.getHours() + hour);
//    this.(this.getHours() + hour);
//    return this;
//}

//Date.prototype.addMinutes = function (minute) {
//    this.setHours(this.getMinutes() + minute);
//    return this;
//}

Date.prototype.isSameDate = function(date: Date): boolean {
    return date && this.getFullYear() === date.getFullYear() && this.getMonth() === date.getMonth() && this.getDate() === date.getDate();
};

Date.prototype.addMonths = function (value: number) {
    var date = new Date(this.valueOf());
    date.setMonth(date.getMonth() + value);

    return date;
};

Date.prototype.addYears = function (value: number) {
    var date = new Date(this.valueOf());
    date.setFullYear(date.getFullYear() + value);

    return date;
};

Array.prototype.rotateShift = function (index: number)
{
    var array = this;

    var limit = array.length - 1;
    var element = array[index];

    while (index < limit)
        array[index] = array[++index];
    array[index] = element;
    return array;
}

Array.prototype.insert = function (index: number, item: any) {
    this.splice(index, 0, item);
}