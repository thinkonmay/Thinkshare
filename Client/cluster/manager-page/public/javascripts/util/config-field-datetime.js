export function set_current_datetime() {
	let maxDate = String(new Date((new Date()).getTime() +  + 86400000000).toISOString()).slice(0, 16)
	let minDate = String(new Date((new Date()).getTime()).toISOString()).slice(0, 16)
	let defaultDate = String(new Date((new Date()).getTime()).toISOString()).slice(0, 16)
	$('#stamp-time-from').attr({
		"max": maxDate,
		"min": minDate,
		"value": defaultDate
	});
	// $('#stamp-time-to').attr({
	// 	"max": maxDate,
	// 	"min": minDate,
	// 	"value": defaultDate
	// });
}