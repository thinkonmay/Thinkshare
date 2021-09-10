import * as API from "../util/api.js"
import { getCookie } from "../util/cookie.js"

$(document).ready(async () => {
	$("[data-toggle=\"tooltip\"]").tooltip()
	$("#addslave").click(() => {
		Swal.fire({
			title: "Submit your slave ID",
			input: "text",
			showCancelButton: true,
			confirmButtonText: "Add",
			showLoaderOnConfirm: true,
			preConfirm: id => {
				// fetch goes here
				return id
			},
			allowOutsideClick: () => !Swal.isLoading()
		}).then(result => {
			if (result.isConfirmed) {
				Swal.fire({
					title: `Added slave ID ${result.value}`
				})
			}
		})
	})
	$(document).on("click", ".rejectbtn", function () {
		const slaveId = $(this).attr("id")
		// await API.rejectDevice()
	})
	const slaves = await getSlaves()
	for (let i = 0; i < 10; i++) {
		slaves.push({})
	}
	for (const slave of slaves) {
		append("onlineSlaves", createSlave(slave))
	}
	for (const slave of slaves) {
		append("availableSlaves", createSlave(slave))
	}
	const connection = new signalR.HubConnectionBuilder().withUrl("https://conductor.thinkmay.net/ChatHub").build();
	//Disable send button until connection is established
	document.getElementById("sendButton").disabled = true;

	connection.on("ReceiveMessage", function (user, message) {
		var li = document.createElement("li");
		document.getElementById("messagesList").appendChild(li);
		// We can assign user-supplied strings to an element's textContent because it
		// is not interpreted as markup. If you're assigning in any other way, you 
		// should be aware of possible script injection concerns.
		li.textContent = `${user} says ${message}`;
	});

	connection.start().then(function () {
		document.getElementById("sendButton").disabled = false;
	}).catch(function (err) {
		return console.error(err.toString());
	});

	document.getElementById("sendButton").addEventListener("click", function (event) {
		var user = document.getElementById("userInput").value;
		var message = document.getElementById("messageInput").value;
		connection.invoke("SendMessage", user, message).catch(function (err) {
			return console.error(err.toString());
		});
		event.preventDefault();
	});
})

async function getSlaves() {
	const token = getCookie("token")
	if (!token) return []
	try {
		const data = await API.fetchSlave()
		return await data.json()
	} catch (e) {
		console.log(e)
		return []
	}
}

function getURL(slaveId) {
	return `${API.Initialize}?${serialize({
		slaveId,
		cap: {
			...Quality("best"),
			mode: 1,
			screenWidth: window.innerWidth,
			screenHeight: window.innerHeight
		}
	})}`
}

function createSlave(slave) {
	const URL = getURL(slave.id)
	return `
    <div class="col-12 col-sm-6 col-md-3 d-flex align-items-stretch flex-column slave">
      <div class="card bg-light d-flex flex-fill">
        <div class="card-header text-muted border-bottom-0">
          <br>
        </div>
        <div class="card-body pt-0">
          <div class="row">
            <div class="col-7">
              <h2 class="lead"><b>${slave.cpu}</b></h2>
              <ul class="ml-4 mb-0 fa-ul text-muted">
                <li class="small"><span class="fa-li"><i class="fab fa-windows"></i></span>OS: ${
	slave.os
}</li>
                <li class="small"><span class="fa-li"><i class="fas fa-memory"></i></span>RAM: ${Math.round(
		slave.raMcapacity / 1024
	)}GB</li>
                <li class="small"><span class="fa-li"><i class="fas fa-tv"></i></span>GPU: ${
	slave.gpu
}</li>
              </ul>
            </div>
            <div class="col-5 text-center">
              <img src="images/avtFounder.png" alt="user-avatar" class="img-circle img-fluid">
            </div>
          </div>
        </div>
        <div class="overlay">
          <div class="row">
            <div class="col colbut rejectbtn" id="${
	slave.id
}"><input class="btn btn-secondary" type="submit" name="" value="Reject device"></div>
            <div class="col colbut"><a href="${URL}" target="_blank"><input class="btn btn-secondary" type="submit" value="Connect device"></div></a>
            <div class="w-100"></div>
            <div class="col colbut"><input class="btn btn-secondary" type="submit" name="" value="AMOGUS"></div>
            <div class="col colbut"><input class="btn btn-secondary" type="submit" name="" value="SUSSY BAKA"></div>
          </div>
        </div>
      </div>
    </div>`
}

function append(id, html) {
	$(`#${id}`).append(html)
}

function Quality(qual) {
	switch (qual) {
	case "best":
		return {
			audioCodec: 4,
			videoCodec: 4
		}
	case "good":
		return {
			audioCodec: 2,
			videoCodec: 2
		}
	default:
		return {
			audioCodec: 0,
			videoCodec: 0
		}
	}
}

function serialize(obj, prefix) {
	var str = [],
		p
	for (p in obj) {
		if (obj.hasOwnProperty(p)) {
			var k = prefix ? prefix + "[" + p + "]" : p,
				v = obj[p]
			str.push(
				v !== null && typeof v === "object" ?
				serialize(v, k) :
				encodeURIComponent(k) + "=" + encodeURIComponent(v)
			)
		}
	}
	return str.join("&")
}

async function rejectDevice(sessionClientId) {
	const data = await API.rejectDevice(sessionClientId)
	console.log(await data.text())
}
