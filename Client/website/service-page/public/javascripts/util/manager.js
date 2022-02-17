import * as API from "./api.js"





export async function
clusterFormGen() {
    var isManager = await API.isManager()


	if (isManager === true) {
        API.getClusters().then(async data =>{
            var body = await data.json()
            body.forEach(element => {
                append("clusterBoard",
                ` <h5>Cluster Name: ${element.name}, <a href="${element.url}">Access cluster dashboard here</a></h5> `)
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
	}
}