


export async function 
prepare_download_button()
{
    let versionList = await (await fetch('https://api.github.com/repos/thinkonmay/Thinkremote/releases?page=1&per_page=100/assets/')).json()
    let versionListForm = document.getElementById('versionDownload')
    for (let i in versionList) {
        if(i == 8) break;
        let url = `https://github.com/thinkonmay/Thinkremote/releases/download/${versionList[i].tag_name}/Thinkremote.msi`
        versionListForm.innerHTML +=
            `
            <li>
                <div class="d-flex justify-content-between">
                <div><span class="text-light-green">${versionList[i].tag_name}</span> ${versionList[i].name}</div>
                <a href="${url}" ><button type="button" class="btn btn-outline-success btn-fw" style="margin-top: 0px">Download</button></a>
                </div>
            </li>
        `
    }

    $('[name="download-app"]').click(function () {
		let url = `https://github.com/thinkonmay/Thinkremote/releases/download/${versionList[0].tag_name}/Thinkremote.msi`
		window.location.href = url
	})
}