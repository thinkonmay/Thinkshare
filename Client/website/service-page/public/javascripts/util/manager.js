/**
 * * In this component, I will:
 * TODO: Solve Problem about Cluster
 * ? + CheckManager
 * ? + Generate Cluster Form
 * ? + Handle Cluster Register
 * ? + Handle Manager Register
*/

import * as API from "./api.js"
import * as Utils from "./utils.js"
import { managerRegister, requestCluster } from "./api.js"

/**
 * * Check Manager
 * @returns true if role of user is manager
 */
async function checkManager() { return "true" === (await (await API.getRoles()).json()).isManager; }


/**
 * * In this funciton, I will
 * ? - Generate Cluster Form and Handle Cluster Register if is Manager
 * ? - Generate Register Form and Handle Manager Register if is Manager
 */
export async function
clusterFormGen() {
    var form = document.getElementById("ClusterForm");
    var board = document.getElementById('clusterBoard')
	var isManager = await checkManager();

	if (isManager === true) 
    {
		var body = await ( await API.getClusters()).json()
        form.innerHTML = clusterRegisterForm;

        if (body.length == 0)
            board.innerHTML = empty;
        else
            body.forEach(element => { board.innerHTML += device(element); });

        handleClusterRegister();
    }
    else {
        board.innerHTML = empty;
        form.innerHTML = managerRegisterForm;

        handleManagerRegister();
    }
}

var empty = `<h4>You don't have any cluster 😞</h4>`;

var clusterRegisterForm = 
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

var managerRegisterForm = 
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

function device (element) 
{
    var result = 
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
    `;
    return result;
}

/**
 * * In this function, I will:
 * ? get value of name, region, password for request Cluster
 * ? Response status with swal.fire
 */
function handleClusterRegister()
{
    $('[id="submitClusterCtrler"]').click(async () => {
        var name = $('#clusterNameCtrler').val();
        // var region = $('#regionCtrler').val();
        var region = 'Singapore' // Cause ssh don't connect to another region then we set hard variable region
        var password = $('#passwordCtrler').val();


        Utils.newSwal.fire({
            title: 'Processing',
            html: '...',
            didOpen: async () => {
                Swal.showLoading();
                var resp = await requestCluster(name, password, region);

                if (resp.ok) {
                    Utils.newSwal.fire({
                        icon: 'success',
                        title: `Register cluster ${name} successfully`,
                        showConfirmButton: false,
                        timer: 1500
                    })
                }
                else {
                    Utils.newSwal.fire({
                        icon: 'error',
                        title: 'Fail to register ',
                        showConfirmButton: false,
                        timer: 1500
                    })

                }
            }
        })
    });
}

/**
 * * In this function, I will:
 * ? - When submit Cluster was click, the response will get from managerRegister(value of description)
 */
function handleManagerRegister()
{
    $('[id="submitClusterCtrler"]').click(() => {
        Utils.newSwal.fire({
            title: 'Processing',
            html: '...',
            didOpen: async () => {
                Swal.showLoading();
                var resp = await managerRegister($('#descriptionCtrler').val());

                if (resp.ok) {
                    Utils.newSwal.fire({
                        icon: 'success',
                        title: `Now you can create cluster`,
                        showConfirmButton: false,
                        timer: 1500
                    })
                }
                else {
                    Utils.newSwal.fire({
                        icon: 'error',
                        title: `Fail to elevate to manager`,
                        showConfirmButton: false,
                        timer: 1500
                    })

                }
            }
        })
    });
}