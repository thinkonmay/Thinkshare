import * as API from "./api.js"

let sessionInfor;
let datasets = [];
export async function setDataForChart() {
	try {
		sessionInfor = await (await API.getSession()).json()
	} catch (error) {
		await setDataForChart();
	}
	for (let i = 0; i < 7; i++) {
		datasets[i] = 0;
	}
	for (let i = 0; i < sessionInfor.length; i++) {
		datasets[sessionInfor[i].dayofWeek] = sessionInfor[i].sessionTime;
	}
	var date = new Date();
	var day = date.getDay();
	let countDay = 0;
	let _lables = [];
	while (countDay <= 6) {
		switch (day) {
			case 0:
				_lables.unshift("SUN");
				break;
			case 1:
				_lables.unshift("MON");
				break;
			case 2:
				_lables.unshift("TUE");
				break;
			case 3:
				_lables.unshift("WED");
				break;
			case 4:
				_lables.unshift("THU");
				break;
			case 5:
				_lables.unshift("FRI");
				break;
			case 6:
				_lables.unshift("SAT");
				break;
		}
		day--;
		if (day < 0) {
			day = 6;
		}
		countDay++;
	}
	if ($("#performanceLine").length) {
		var graphGradient = document.getElementById("performanceLine").getContext('2d');
		var graphGradient2 = document.getElementById("performanceLine").getContext('2d');
		var saleGradientBg = graphGradient.createLinearGradient(5, 0, 5, 100);
		saleGradientBg.addColorStop(0, 'rgba(26, 115, 232, 0.18)');
		saleGradientBg.addColorStop(1, 'rgba(26, 115, 232, 0.02)');
		var saleGradientBg2 = graphGradient2.createLinearGradient(100, 0, 50, 150);
		saleGradientBg2.addColorStop(0, 'rgba(0, 208, 255, 0.19)');
		saleGradientBg2.addColorStop(1, 'rgba(0, 208, 255, 0.03)');
		var salesTopData = {
			labels: _lables,
			datasets: [{
				label: 'This week',
				data: datasets,
				backgroundColor: saleGradientBg,
				borderColor: [
					'#1F3BB3',
				],
				borderWidth: 1.5,
				fill: true, // 3: no fill
				pointBorderWidth: 1,
				pointRadius: [4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4],
				pointHoverRadius: [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2],
				pointBackgroundColor: ['#1F3BB3)', '#1F3BB3', '#1F3BB3', '#1F3BB3', '#1F3BB3)', '#1F3BB3', '#1F3BB3', '#1F3BB3', '#1F3BB3)', '#1F3BB3', '#1F3BB3', '#1F3BB3', '#1F3BB3)'],
				pointBorderColor: ['#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff',],
			},
				//  {
				// 	label: 'Last week',
				// 	data: [30, 150, 190, 250, 120, 150, 130],
				// 	backgroundColor: saleGradientBg2,
				// 	borderColor: [
				// 		'#52CDFF',
				// 	],
				// 	borderWidth: 1.5,
				// 	fill: true, // 3: no fill
				// 	pointBorderWidth: 1,
				// 	pointRadius: [4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4],
				// 	pointHoverRadius: [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2],
				// 	pointBackgroundColor: ['#52CDFF)', '#52CDFF', '#52CDFF', '#52CDFF', '#52CDFF)', '#52CDFF', '#52CDFF', '#52CDFF', '#52CDFF)', '#52CDFF', '#52CDFF', '#52CDFF', '#52CDFF)'],
				// 	pointBorderColor: ['#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff',],
				// }
			]
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
						beginAtZero: false,
						autoSkip: true,
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
						maxTicksLimit: 7,
						fontSize: 10,
						color: "#6B778C"
					}
				}],
			},
			legend: false,
			legendCallback: function (chart) {
				var text = [];
				text.push('<div class="chartjs-legend"><ul>');
				for (var i = 0; i < chart.data.datasets.length; i++) {
					text.push('<li>');
					text.push('<span style="background-color:' + chart.data.datasets[i].borderColor + '">' + '</span>');
					text.push(chart.data.datasets[i].label);
					text.push('</li>');
				}
				text.push('</ul></div>');
				return text.join("");
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
		var salesTop = new Chart(graphGradient, {
			type: 'line',
			data: salesTopData,
			options: salesTopOptions
		});
		document.getElementById('performance-line-legend').innerHTML = salesTop.generateLegend();
	}



	///////////////////////////////////////////////////////////////////////////////////////////////////////
	// do not delete this block, i intentionally reserve doughnutChart for future use
	if ($("#doughnutChart").length) {
		var doughnutChartCanvas = $("#doughnutChart").get(0).getContext("2d");
		var doughnutPieData = {
			datasets: [{
				data: [40, 20, 30, 10],
				backgroundColor: [
					"#1F3BB3",
					"#FDD0C7",
					"#52CDFF",
					"#81DADA"
				],
				borderColor: [
					"#1F3BB3",
					"#FDD0C7",
					"#52CDFF",
					"#81DADA"
				],
			}],

			// These labels appear in the legend and in the tooltips when hovering different arcs
			labels: [
				'Total',
				'Net',
				'Gross',
				'AVG',
			]
		};
		var doughnutPieOptions = {
			cutoutPercentage: 50,
			animationEasing: "easeOutBounce",
			animateRotate: true,
			animateScale: false,
			responsive: true,
			maintainAspectRatio: true,
			showScale: true,
			legend: false,
			legendCallback: function (chart) {
				var text = [];
				text.push('<div class="chartjs-legend"><ul class="justify-content-center">');
				for (var i = 0; i < chart.data.datasets[0].data.length; i++) {
					text.push('<li><span style="background-color:' + chart.data.datasets[0].backgroundColor[i] + '">');
					text.push('</span>');
					if (chart.data.labels[i]) {
						text.push(chart.data.labels[i]);
					}
					text.push('</li>');
				}
				text.push('</div></ul>');
				return text.join("");
			},

			layout: {
				padding: {
					left: 0,
					right: 0,
					top: 0,
					bottom: 0
				}
			},
			tooltips: {
				callbacks: {
					title: function (tooltipItem, data) {
						return data['labels'][tooltipItem[0]['index']];
					},
					label: function (tooltipItem, data) {
						return data['datasets'][0]['data'][tooltipItem['index']];
					}
				},

				backgroundColor: '#fff',
				titleFontSize: 14,
				titleFontColor: '#0B0F32',
				bodyFontColor: '#737F8B',
				bodyFontSize: 11,
				displayColors: false
			}
		};
		var doughnutChart = new Chart(doughnutChartCanvas, {
			type: 'doughnut',
			data: doughnutPieData,
			options: doughnutPieOptions
		});
		document.getElementById('doughnut-chart-legend').innerHTML = doughnutChart.generateLegend();
	}
	//////////////////////////////////////////////////////////////////////////////////////////////////
}