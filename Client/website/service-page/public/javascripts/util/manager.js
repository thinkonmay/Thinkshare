import * as API from "./api.js"
import { append } from "./utils.js";
import { managerRegister, requestCluster } from "./api.js"


async function
checkManager() {
	return "true" === (await (await API.getRoles()).json()).isManager;
}



export async function
	clusterFormGen() {
	var isManager = await checkManager();
	if (isManager === true) {
		API.getClusters().then(async data => {
			var body = await data.json()
			if (body.length == 0)
				document.getElementById('clusterBoard').innerHTML = `<h4>You haven't any Cluster 😞</h4>`
			else
			body.forEach(element => {
				document.getElementById('clusterBoard').innerHTML +=
					`<div class="d-flex align-items-center justify-content-between py-2 border-bottom" style="padding-bottom: 0px !important">
					<div class="d-flex">
						<i class="menu-icon mdi mdi-desktop-tower" style="font-size: 32px;"></i>
						<div class="ms-3" style="margin-top: 5px;">
							<p class="ms-1 mb-1 fw-bold">Cluster Name: ${element.name}</p>
						</div>
					</div>
					<a href="${element.url}">
						<button type="button" class="btn btn-outline-primary btn-fw"
						style="margin-top: 0px">Access</button>
					</a>
				</div>
				`
			});
		})

		var form = document.getElementById("ClusterForm");
		form.innerHTML =
			`
		<label class="col-sm-3 col-form-label">
			ClusterName
		</label>
		<div class="col-sm-9">
		<input type="text" class="form-control" id="clusterNameCtrler"
			placeholder=" ClusterName ">
		</div>
		<label class="col-sm-3 col-form-label">
			Password
		</label>
		
		<div class="col-sm-9">
		<input type="password" class="form-control" id="passwordCtrler"
			placeholder=" Type your password here ">
		</div>
		<label class="col-sm-3 col-form-label">
			Region
		</label>
		<div class="col-sm-9">
			<select class="form-control" id="regionCtrler">
				<option value="US-West">US West</option>
				<option value="US-East">US East</option>
				<option value="Canada">Canada</option>
				<option value="Singapore">Singapore</option>
				<option value="India">India</option>
				<option value="South_Korea">South Korea</option>
				<option value="Australia">Australia</option>
				<option value="Tokyo">Tokyo</option>
			</select>
		</div>
		`;
	}
	else {
		form.innerHTML =
			`
		<label class="col-sm-3 col-form-label">
			<b>Description</b> 
			<br>
			<i>Is there any specific requirement that we can support? </i>
		</label>
		<div class="col-sm-9">
			<input type="text" class="form-control" id="descriptionCtrler"
				style="height: 100px;" placeholder=" Why do you want to host your own worker node ? ">
		</div>
		`;
		$('[id="submitClusterCtrler"]').click(managerRegister);
	}
}
