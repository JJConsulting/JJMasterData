interface JQuery{
    flatpickr(options: FlatpickrOptions)
}

interface FlatpickrOptions{
    enableTime?: boolean,
    wrap?: boolean,
    noCalendar?: boolean,
    allowInput?: boolean,
    altInput?: boolean,
    dateFormat?: string,
    altFormat?: string,
    onOpen?: Function,
    locale?: string,
    time_24hr?: boolean
}