import * as Utils from "../util/utils.js"
import * as API from "../util/api.js"
import { setCookie, deleteCookie } from "../util/cookie.js"
import { set_current_datetime } from "../util/config-field-datetime.js"
import { prepare_booker_infor, prepare_cluster_infor, setup_cluster_booking, set_worker_chart } from "../util/cluster-component.js"



$(document).ready(async () => {
	await set_current_datetime();
	await setup_cluster_booking();
	await prepare_cluster_infor();
	await set_worker_chart();
	await prepare_booker_infor();

	$('#DeleteCluster').click(() => { DeleteCluster(); });
	$('#logout').click(() => { API.Logout(); })
})

function DeleteCluster()
{
	Utils.newSwal.fire({
		title: 'Are you sure?',
		text: "You won't be able to revert this!",
		icon: 'warning',
		showCancelButton: true,
		confirmButtonColor: '#3085d6',
		cancelButtonColor: '#d33',
		confirmButtonText: 'Yes, delete it!'
	}).then(async (result) => {
		if (result.isConfirmed) {
			var res = await API.UnRegister();

			if (res.ok)
				Swal.fire('Success!', 'Cluster deleted.', 'success')
			else
				Swal.fire('Fail!', 'Cluster deletion failed.', 'error')
		}
	})
}


function append(id, html) { $(`#${id}`).append(html) }
(function ($) { })(jQuery);