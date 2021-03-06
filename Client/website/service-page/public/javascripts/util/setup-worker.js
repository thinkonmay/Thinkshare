import { isElectron } from "./checkdevice.js";

export async function 
prepare_host_ui()
{
    if(!isElectron()) {
        document.getElementById('runWorker').innerHTML = '';
        return;
    }

    $('#agentLAN').click(() => {
        run_agent_localhost()
    });
    $('#remoteLAN').click(() => {
        run_remote_localhost();
    });
    $('#agentProduction').click(() => {
        run_agent_production();
    });
}

function
run_agent_production()
{
    window.location.assign(`agent://agent/`);
}

function
run_agent_localhost()
{
    window.location.assign(`localagent://agent/`);
}

function
run_remote_localhost()
{
    window.location.assign(`localremote://remote/`);
}