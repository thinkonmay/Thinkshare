export async function set_current_datetime() {
	var maxDate = 				new Date()			
	var minDate = 				new Date()			

	maxDate.setMinutes(maxDate.getMinutes() + 5)
	minDate.setMonth(minDate.getMonth())
	

	$('#stamp-time-from').attr({
		"value": 							(minDate.toISOString().slice(0, 16))
	});
	$('#stamp-time-to').attr({
		"value": 							(maxDate.toISOString()).slice(0, 16)
	});
}