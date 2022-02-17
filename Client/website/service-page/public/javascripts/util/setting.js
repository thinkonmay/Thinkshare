import * as API from "./api.js"

export async function
prepare_setting()
{
	var body = await (await API.getSetting()).json()
	$(`[value=${Setting.DecodeCoreEngine(parseInt(body.engine))}]`).attr('checked', true);
	$(`[value=${Setting.DecodeCodec(parseInt(body.audioCodec))}]`).attr('checked', true);
	$(`[value=${Setting.DecodeCodec(parseInt(body.videoCodec))}]`).attr('checked', true);
	$(`[value=${Setting.DecodeDeviceType(parseInt(body.device))}]`).attr('checked', true);
	$(`[value=${Setting.DecodeQoeMode(parseInt(body.mode))}]`).attr('checked', true);
	$(`[value=${Setting.DecodeResolution(body)}]`).attr('checked', true);

	$('[name="download-app"]').click(function () {
		let value;
		fetch('https://api.github.com/repos/thinkonmay/Thinkremote/releases?page=1&per_page=100/assets/')
			.then(response => response.json())
			.then(data => {
				value = data
				let url = `https://github.com/thinkonmay/Thinkremote/releases/download/${value[0].tag_name}/Thinkremote.msi`
				window.location.href = url
			});
	})

	if (getCookie("show-tutorial") == "true") {
		$('#checkboxTutorial').attr("checked", true)
	}
	if (getCookie("show-tutorial") != "true") {
		$('#modal-btn3').attr('checked', true)
	}

	$('#showTutorial').click(function () {
	})
	$('#showSettings').click(function () {
		window.location = '/dashboard#settings-modal'
	})
	$('#registertoManager').click(function () {
		window.location = '/dashboard#manager-modal'
	})




	$('.modal__checkbox').click(function () {
		if ($('#checkboxTutorial').attr("checked") == 'checked') {
			// have been click this checkbox
			document.getElementById('checkboxTutorial').removeAttribute("checked")
			// $('#checkboxTutorial').removeAttr("checked")
			setCookie("show-tutorial", "false", 99999999999999)
		} else
			if ($('#checkboxTutorial').attr("checked") != 'checked') {
				// click checkbox
				$('#showTutorialState').html("Yes, I won\'t show again")
				$('#checkboxTutorial').attr("checked", true)
				setCookie("show-tutorial", "true", 99999999999999)
			}
	})



	// Remote Core
	$('[name="remote"]').click(async function () {
		var display = await (await API.getSetting()).json();
		var value = $(this).val();
		display.engine = Setting.CoreEngine(value);
		await Setting.updateSetting(display);
	})

	// Remote control bitrate
	$('[name="bitrate"]').click(async function () {
		var display = await (await API.getSetting()).json();
		var value = $(this).val();
		display.mode = Setting.QoEMode(value);
		await Setting.updateSetting(display);
	});

	$('[id="submitClusterCtrler"]').click(managerRegister);

	// Resolution
	$('[name="res"]').click(async function () {
		var display = await (await API.getSetting()).json();
		var value = $(this).attr("value");
		switch (value) {
			case "FullHD":
				display.screenWidth = 1920;
				display.screenHeight = 1080;
				break;
			case "2K":
				display.screenWidth = 2560;
				display.screenHeight = 1440;
				break;
			case "4K":
				display.screenWidth = 3840;
				display.screenHeight = 2160;
				break;
		}
		await Setting.updateSetting(display);

	})

	// VideoCodec
	$('[name="video"]').click(async function () {
		var display = await (await API.getSetting()).json();
		var value = $(this).val();
		display.videoCodec = Setting.Codec(value);
		await Setting.updateSetting(display);
	});

	// Remote control bitrate
	$('[name="bitrate"]').click(async function () {
		var display = await (await API.getSetting()).json();
		var value = $(this).val();
		display.mode = Setting.QoEMode(value);
		await Setting.updateSetting(display);
	});

	// AudioCodec
	$('[name="audio"]').click(async function () {
		var display = await (await API.getSetting()).json();
		var value = $(this).val();
		display.audioCodec = Setting.Codec(value)
		await Setting.updateSetting(display);
	});

	$(".next-tab").click(() => {
		let value = null;
		if ($('#NativeApp').attr('checked') == 'checked') {
			value = 'HowToUse'
			$("#NativeApp").attr('checked', false)
		}
		if ($("#HowToUse").attr('checked') == 'checked') {
			value = 'ShorcutKey';
			$("#HowToUse").attr('checked', false)
		}
		if ($("#ShorcutKey").attr('checked') == 'checked') {
			value = 'NativeApp';
			$("#ShorcutKey").attr('checked', false)
		}
		if (value != null) {
			value = '#' + value
			$(`${value}`).attr('checked', true)
		}
	})




	if (CheckDevice.isElectron()) {
		$('#downloadApp').css("display","none")
		$('#remoteApp2').removeAttr("disabled")
		$('#videoCodec3').removeAttr("disabled")
	}
}