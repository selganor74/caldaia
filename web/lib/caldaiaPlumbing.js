var commonConnect = {
	connector: "Flowchart",
	endpoint: ["Dot", {
		radius: 5
	}]
};
var plumbConnections = {};
jsPlumb.ready(function () {
	jsPlumb.registerEndpointTypes({
		pompaSpenta: {
			endpoint: ["Dot", {
				radius: 5
			}],
			paintStyle: { fillStyle: "gray" }
		},
		pompaAccesa: {
			endpoint: ["Dot", {
				radius: 7
			}],
			paintStyle: { fillStyle: "red" }
		}
	});

	jsPlumb.registerConnectionTypes({
		pompaSpenta: {
			connector: "Flowchart",
			overlays: [
				["Arrow", {
					location: 30
				}]
			],
			paintStyle: {
				strokeStyle: "gray",
				lineWidth: 3
			}
		},
		pompaAccesa: {
			connector: "Flowchart",
			overlays: [
				["Arrow", {
					location: 30
				}]
			],
			paintStyle: {
				strokeStyle: "red",
				lineWidth: 7
			}
		},
		pompaAccesaConOverride: {
			connector: "Flowchart",
			overlays: [
				["Arrow", {
					location: 30
				}]
			],
			paintStyle: {
				strokeStyle: "orange",
				lineWidth: 7
			}
		}
	});
	jsPlumb.draggable($(".draggable"));
	plumbConnections.pompaCamino = jsPlumb.connect({
			source: "camino",
			target: "accumuloRotex",
			anchors: ["Bottom", "Top"]
		},
		commonConnect
	);
	plumbConnections.pompaCaldaia = jsPlumb.connect({
			source: "caldaiaMetano",
			target: "accumuloRotex",
			anchors: ["Bottom", "Right"]
		},
		commonConnect
	);
	plumbConnections.pompaPannelli = jsPlumb.connect({
			source: "pannelliSolari",
			target: "accumuloRotex",
			anchors: ["Bottom", "Left"]
		},
		commonConnect
	);
	plumbConnections.pompaRiscaldamento = jsPlumb.connect({
			source: "accumuloRotex",
			target: "riscaldamento",
			anchors: ["Bottom", "Top"]
		},
		commonConnect
	);
});
