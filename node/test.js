var http = require("http");
var serialport = require("serialport");
var SerialPort = serialport.SerialPort;
var sp = new SerialPort("/dev/ttyUSB0", {
	baudrate: 9600,
	databits: 8,
	parity: 'none',
	stopbits: 1,
	parser: serialport.parsers.readline("\n")
});

var response="";
var fromArduino= {};
var evalResponse="";
sp.on( "open", function() {
	console.log('open');
	sp.on('data', function( data ) {
		response = response + "\n" + data;
		// console.log( "--" + data );
		if (data[0] == "}") {
			evalResponse = "(" + response + ")";
			try {
				fromArduino = eval( evalResponse );
				fromArduino["@timestamp"] = new Date();
				console.log ( fromArduino );
			} catch (e) {
				console.log("Errore nel parsing della risposta");	
			}
			try {
				putDataIntoES( fromArduino );
			} catch (e) {
				console.log("Errore nell'inviod dai dati: " + e );
			}
			response = "";
		}
		// console.log(data + "");	
	});
	sp.write("GET\n", function( err, results ) {
		console.log( 'err ' + err );
		console.log( 'results ' + results );	
	});
});

// inserisce i dati in elasticsearch
function putDataIntoES( dataToBePut ) {
	var options = {
		host: 'es.casa.local',
		port: 9200,
		path: '/caldaia/raw-data',
		method: 'POST'
	};

	var req = http.request(options, function(res) {
		console.log('STATUS: ' + res.statusCode);
		console.log('HEADERS: ' + JSON.stringify(res.headers));
		res.setEncoding('utf8');
		res.on('data', function (chunk) {
		console.log('BODY: ' + chunk);
		});
	});

	req.on('error', function(e) {
		console.log('problem with request: ' + e.message);
	});

	// write data to request body
	req.write(JSON.stringify(dataToBePut)+'\n');
	req.end();
}  

