var indexPost = 0
import * as API from "./api.js"

export function appendWorkerNode(os, cpu, gpu, id, raMcapacity, register, state) {

    var date = new Date(register);


    append('WorkerNode-Table', `<tr>
    <td>
        <p>ID: ${id}</p>
        <b>Register: ${String(date).slice(0, 24)}</b>
    </td>
    <td>
        <p>CPU: ${cpu}</p>
        <b>GPU: ${gpu}</b>
    </td>
    <td>
        <p>RAM: ${raMcapacity} GB</p>
        <b>OS: ${os}</b>
    </td>
    <td>
        <b>${state}</b>
    </td>
    <td>
        <a onclick="showDataLog(${id})" href="#popup1" id="detailBtn${id}" class="bn11">Detail</a>
    </td>
    </tr>`);

    

}


export function append(id, html) {
    $(`#${id}`).append(html)
}
$(document).ready(async () => {

})