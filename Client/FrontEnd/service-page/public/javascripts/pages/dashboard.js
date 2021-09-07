import * as API from "../util/api.js";
import { getCookie } from "../util/cookie.js"

$(document).ready(async () => {
    const slaves = await getSlaves()
    console.log(slaves)
    for (const slave of slaves) {
        append(createSlave(slave))
    }
})

async function getSlaves() {
    const token = getCookie("token")
    if (!token) return []
    try {
        const data = await fetch(API.FetchSlave, {
            headers: API.genHeaders(token)
        })
        const res = (await data.text()).replace(/(?:\\[rn])+/g, '')
        window.abc = res;
        const obj = JSON.parse(JSON.parse(res))
        console.log(obj)
        console.log(typeof obj)
        return JSON.parse(text)
    } catch(e) {
      console.log(e)
        return []
    }
}

function createSlave(slave) {
    return `
    <div class="col-12 col-sm-6 col-md-3 d-flex align-items-stretch flex-column">
      <div class="card bg-light d-flex flex-fill">
        <div class="card-header text-muted border-bottom-0">
          <br>
        </div>
        <div class="card-body pt-0">
          <div class="row">
            <div class="col-7">
              <h2 class="lead"><b>${slave.id}</b></h2>
              <ul class="ml-4 mb-0 fa-ul text-muted">
                <li class="small"><span class="fa-li"><i class="fas fa-lg fa-building"></i></span>OS: ${slave.OS}</li>
                <li class="small"><span class="fa-li"><i class="fas fa-lg fa-phone"></i></span>RAM: ${slave.RAMcapacity}</li>
                <li class="small"><span class="fa-li"><i class="fas fa-lg fa-phone"></i></span>CPU: ${slave.CPU}</li>
                <li class="small"><span class="fa-li"><i class="fas fa-lg fa-phone"></i></span>GPU: ${slave.GPU}</li>
              </ul>
            </div>
            <div class="col-5 text-center">
              <img src="images/pc.png" alt="user-avatar" class="img-circle img-fluid">
            </div>
          </div>
        </div>
        <div class="overlay" align="center">
            <div class="row">
              <div class="col m-50"><input class="btn btn-secondary" type="submit" name="" value="Reject device"></div>
              <div class="col-sm"><input class="btn btn-secondary" type="submit" name="" value="Connect device"></div>
              <div class="w-100"></div>
              <br>
              <div class="col-sm"><input class="btn btn-secondary" type="submit" name="" value="AMOGUS"></div>
              <div class="col-sm"><input class="btn btn-secondary" type="submit" name="" value="SUSSY BAKA"></div>
            </div>
          </div>
      </div>
    </div>`
}

function append(id, html) {
    $(`#${id}`).append(html)
}