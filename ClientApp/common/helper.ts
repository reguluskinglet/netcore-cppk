export const findRowIdx = (data, id, path?) => {
  const row = data.find(r => (path ? r[path].id === id : r.id === id))
  const idx = data.indexOf(row)
  return idx
}

export const formatIsoToNorm = iso => {
  var n = ''
  if (iso && iso.length >= 19) {
    n =
      iso.substring(8, 10) +
      '.' +
      iso.substring(5, 7) +
      '.' +
      iso.substring(0, 4) +
      ' ' +
      iso.substring(11, 13) +
      ':' +
      iso.substring(14, 16) +
      ':' +
      iso.substring(17, 19)
  }
  if (iso && iso.length === 10) {
    n =
      iso.substring(8, 10) +
      '.' +
      iso.substring(5, 7) +
      '.' +
      iso.substring(0, 4)
  }
  return n
}

export const formatDateToNorm = date => {
  var year = date.getFullYear(),
    month = (date.getMonth() + 1).toString(),
    formatedMonth = month.length === 1 ? '0' + month : month,
    day = date.getDate().toString(),
    formatedDay = day.length === 1 ? '0' + day : day,
    hour = date.getHours().toString(),
    formatedHour = hour.length === 1 ? '0' + hour : hour,
    minute = date.getMinutes().toString(),
    formatedMinute = minute.length === 1 ? '0' + minute : minute,
    second = date.getSeconds().toString(),
    formatedSecond = second.length === 1 ? '0' + second : second
  return (
    formatedDay +
    '.' +
    formatedMonth +
    '.' +
    year +
    ' ' +
    formatedHour +
    ':' +
    formatedMinute +
    ':' +
    formatedSecond
  )
}

export function hash(str = ''): number {
  var hash = 5381,
    i = str.length

  while (i) {
    hash = (hash * 33) ^ str.charCodeAt(--i)
  }

  return hash >>> 0
}
