import store from '../main'
import storage from './storage'
import { RequestEvent, RequestAction, ShowTooltip, ShowDeleteDialog } from '../common'
import { push } from 'react-router-redux'

const _base = '/api/'

export default class RestWebService {
  static get(url: string, isDownloadFile?: boolean): JQueryPromise<any> {
    return this.request('GET', url, null, undefined, undefined, false, isDownloadFile)
  }

  static post(url: string, data: Object | string, isUploadFile?, isDownloadFile?: boolean): JQueryPromise<any> {
    return this.request('POST', url, data, undefined, undefined, isUploadFile, isDownloadFile)
  }

  static put(url: string, data: Object | string, isUploadFile?, isDownloadFile?: boolean): JQueryPromise<any> {
    return this.request('PUT', url, data, undefined, undefined, isUploadFile, isDownloadFile)
  }

  static delete(url: string, data: Object | string): JQueryPromise<any> {
    return this.request('DELETE', url, data)
  }

  static postNoHeader(url: string, data?): JQueryPromise<any> {
    return $.post(url, data)
  }

  static request(
    type: 'POST' | 'GET' | 'DELETE' | 'PUT',
    url,
    data?,
    hasGlobal = true,
    base = _base,
    isUploadFile = false,
    isDownloadFile = false,
    contentTypeDefault = false
  ): JQueryPromise<any> {
    var token = storage.getToken()

    let hhhh

    const hhh = $.ajax({
      beforeSend: () => {
        //if (store && hasGlobal) store.dispatch(new RequestAction(RequestEvent.Start))
      },
      type: type,
      headers: { Authorization: 'Bearer ' + token },
      contentType: isUploadFile ? false : contentTypeDefault ? undefined : 'application/json; charset=utf-8',
      //dataType: isUploadFile || isDownloadFile ? undefined : 'json',
      url: url.includes('http://') || url.includes('https://') ? url : base + url,
      data: isUploadFile || contentTypeDefault ? data : data && JSON.stringify(data),
      processData: isUploadFile ? false : undefined,
      mimeType: isUploadFile ? 'multipart/form-data' : undefined,
      cache: false,
      xhr: () => {
        hhhh = new XMLHttpRequest()
        return hhhh
      },
      xhrFields:
        (isDownloadFile && {
          responseType: 'blob'
        }) ||
        undefined,
      complete: (...params) => {
        // if (hasGlobal) store.dispatch(new RequestAction(RequestEvent.End))
        //  debugger
      },
      success: function(data) {
        if (isDownloadFile) {
          var blob = new Blob([data])
          // console.log(blob.size)
          var link = document.createElement('a')
          link.href = window.URL.createObjectURL(blob)
          link.download = '' + new Date() + '.pdf'
          link.click()
        }
      }
    }).fail((xhr: JQueryXHR, ...args) => {
      //console.log(hhhh)

      if (hhhh.responseType === 'blob') {
        var b = new Blob([hhhh.response]),
          fr = new FileReader()

        fr.onload = function() {
          try {
            const res = JSON.parse(this.result.toString())
            store.dispatch(new ShowTooltip(`Ошибка: ${res.Message}`, 1))
          } catch (err) {}
        }

        fr.readAsText(b)
      } else {
        var status = xhr.status

        switch (status) {
          case 401:
            store.dispatch(push('/login'))
            break
          case 404:
            store.dispatch(push('/404'))
            break
        }

        if (xhr.responseJSON && typeof xhr.responseJSON === 'string') {
          store.dispatch(new ShowTooltip(`Ошибка: ${xhr.responseJSON}`, 1))
        } else if (xhr.responseJSON && typeof xhr.responseJSON === 'object' && xhr.responseJSON['error']) {
          store.dispatch(new ShowTooltip(`Ошибка: ${xhr.responseJSON['error']}`, 1))
        } else if (xhr.responseJSON && typeof xhr.responseJSON === 'object' && xhr.responseJSON['Message']) {
          store.dispatch(new ShowTooltip(`Ошибка: ${xhr.responseJSON['Message']}`, 1))
        } else {
          store.dispatch(new ShowTooltip(`Ошибка: ${xhr.status} ${xhr.statusText}`, 1))
        }
      }
    })
    return hhh
  }
}
