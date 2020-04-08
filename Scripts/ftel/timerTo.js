function timerTo(eventTime, id, str_day, str_hour, str_minute, str_second) {
    var currentTime = Math.round(new Date().getTime() / 1000);
    var diffTime = eventTime - currentTime;
    var duration = moment.duration(diffTime * 1000, 'milliseconds');
    var interval = 1000;

    setInterval(function () {
        duration = moment.duration(duration - interval, 'milliseconds');
        var days = parseInt(duration.asDays());
        var hours = duration.hours();
        var minutes = duration.minutes();
        var seconds = duration.seconds();

        var full_str_days = "";
        if (days > 0) {
            full_str_days += days + str_day;
        }

        var full_str_hours = "";
        if (days > 0 || hours > 0) {
            full_str_hours += hours + str_hour;
        }

        var full_str_minutes = "";
        if (days > 0 || hours > 0 || minutes > 0) {
            full_str_minutes += minutes + str_minute;
        }

        var full_str_seconds = "";
        if (days > 0 || hours > 0 || minutes > 0 || seconds > 0) {
            full_str_seconds += seconds + str_second;
        }

        var str = "";
        if (full_str_days) {
            str += " " + full_str_days;
        }
        if (full_str_hours) {
            str += " " + full_str_hours;
        }
        if (full_str_minutes) {
            str += " " + full_str_minutes;
        }
        if (full_str_seconds) {
            str += " " + full_str_seconds;
        }

        $('#' + id).html(str);
    }, interval);
}