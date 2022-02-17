
export function setDataForChart(color, nameLabel, checkStateChange) {
	if (checkStateChange) {
		document.getElementById('stateChange').innerHTML = 
		"<canvas id=\"performanceLine\"></canvas>"
	}
	let datasetRAM = []
	let datasetCPU = []
	let datasetGPU = []
	let datasetNetwork = []
	switch (nameLabel) {
		case 'RAM':
			setDataForChart('52CDFF', 'FixState', true)
			datasetRAM = [11, 51, 22, 40, 95, 43, 2, 30, 22, 2, 30, 1, 40, 95, 1, 16, 14, 49, 21, 12, 95, 1, 16, 54, 61, 25, 95, 43, 2, 30, 1, 40, 95, 1, 16, 1, 1, 40, 95, 1, 32, 5, 32, 40, 95, 43, 2, 30, 1, 40, 95, 1, 16, 14, 49, 21, 29, 45, 61, 25, 3, 1]
			break;
		case 'CPU':
			setDataForChart('52CDFF', 'FixState', true)
			datasetCPU = [11, 51, 22, 40, 95, 43, 2, 30, 1, 40, 95, 1, 16, 14, 49, 21, 29, 15, 43, 2, 30, 1, 40, 95, 1, 16, 54, 61, 25, 95, 43, 2, 30, 1, 40, 95, 1, 16, 1, 1, 40, 95, 1, 32, 5, 32, 40, 95, 43, 2, 30, 1, 40, 95, 1, 16, 14, 49, 21, 29, 45, 61, 25, 3, 1]
			break;
		case 'GPU':
			setDataForChart('52CDFF', 'FixState', true)
			datasetGPU = [11, 1, 40, 95, 1, 16, 14, 49, 21, 29, 15, 43, 2, 30, 1, 40, 11, 51, 22, 40, 95, 43, 51, 22, 40, 95, 43, 2, 30, 1, 40, 95, 1, 16, 14, 49, 21, 29, 15, 43, 2, 30, 1, 40, 95, 1, 16, 54, 61, 25, 95, 43, 2, 30, 1, 40, 95, 1, 16, 1, 1, 40, 95, 1, 32, 5, 32, 40, 95, 43, 2, 30, 1, 40, 95, 1, 16, 14, 49, 21, 29, 45, 61, 25, 3, 1]
			break;
		case 'Network':
			setDataForChart('52CDFF', 'FixState', true)
			datasetNetwork = [29, 15, 43, 2, 30, 1, 40, 95, 1, 16, 54, 61, 25, 95, 43, 2, 30, 1, 40, 95, 1, 16, 1, 1, 40, 95, 1, 32, 5, 32, 40, 95, 43, 2, 30, 1, 40, 95, 1, 16, 14, 49, 21, 29, 45, 61, 25, 3, 1]
			break;
	}
	let isSetElement = false;
	let _lables = [];
	for (let index = 0; index <= 60; index++) {
		_lables.unshift(index);
	}
	var salesTopData = null;
	if ($("#performanceLine").length) {
		var graphGradient = document.getElementById("performanceLine").getContext('2d');
		var saleGradientBg = graphGradient.createLinearGradient(5, 0, 5, 100);
		saleGradientBg.addColorStop(0, 'rgba(0, 0, 0, 0)');
		saleGradientBg.addColorStop(1, 'rgba(0, 0, 0, 0)');
		salesTopData = {
			labels: _lables,
			datasets: [{
				label: nameLabel,
				data: nameLabel == "RAM" ? datasetRAM : nameLabel == "CPU" ? datasetCPU : nameLabel == "GPU" ? datasetGPU : nameLabel == "Network" ? datasetNetwork : [],
				backgroundColor: saleGradientBg,
				borderColor: [
					color,
				],
				borderWidth: 1.5,
				fill: true, // 3: no fill
				pointBorderWidth: 1,
				tension: 0.1
				//pointRadius: [4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4],
				//pointHoverRadius: [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2],
				//pointBackgroundColor: ['#e65555', '#e65555', '#e65555', '#e65555', '#e65555', '#e65555', '#e65555', '#e65555', '#e65555', '#e65555', '#e65555', '#e65555', '#e65555'],
				//pointBorderColor: ['#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff',],
			}]
		};
		var salesTopOptions = {
			responsive: true,
			maintainAspectRatio: false,
			scales: {
				yAxes: [{
					gridLines: {
						display: true,
						drawBorder: false,
						color: "#F0F0F0",
						zeroLineColor: '#F0F0F0',
					},
					ticks: {
						beginAtZero: true,
						autoSkip: false,
						maxTicksLimit: 4,
						fontSize: 10,
						color: "#6B778C"
					}
				}],
				xAxes: [{
					gridLines: {
						display: false,
						drawBorder: false,
					},
					ticks: {
						beginAtZero: false,
						autoSkip: true,
						maxTicksLimit: 1,
						fontSize: 10,
						color: "#6B778C"
					}
				}],
			},
			legend: false,
			legendCallback: function (chart) {
				if (!isSetElement) {
					isSetElement = true;
					var text = [];
					return text.join("");
				}
			},
			elements: {
				line: {
					tension: 0.4,
				}
			},
			tooltips: {
				backgroundColor: 'rgba(31, 59, 179, 1)',
			}
		}
		var salesTop =
			new Chart(graphGradient, {
				type: 'line',
				data: salesTopData,
				options: salesTopOptions
			})
		document.getElementById('performance-line-legend').innerHTML = salesTop.generateLegend();
	}
}
