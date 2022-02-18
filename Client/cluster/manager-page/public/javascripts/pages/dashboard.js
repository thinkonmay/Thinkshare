import * as Utils from "../util/utils.js"
import { setCookie, deleteCookie } from "../util/cookie.js"
import { set_current_datetime } from "../util/config-field-datetime.js"
import { prepare_cluster_infor, setup_cluster_booking, set_worker_chart } from "../util/cluster-component.js"



$(document).ready(async () => {
	set_current_datetime();
	setup_cluster_booking();
	await prepare_cluster_infor();
	set_worker_chart();

	

	$('#DeleteCluster').click(() => {
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
	});

	$('#logout').click(() => {
		setCookie("logout", "true")
		setCookie("token", null, 1)
		deleteCookie("token", "/", document.domain)
		try {
			gapi.auth.signOut();
			window.location = "/login"
		} catch {
			window.location = "/login"
		}
	})
})


function append(id, html) { $(`#${id}`).append(html) }
(function ($) { })(jQuery);