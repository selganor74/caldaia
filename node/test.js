(function() {
	var moment = require("moment");
	var http = require("http");
	var serialport = require("serialport");
	var SerialPort = serialport.SerialPort;

	var serialPortName = process.argv[3] || "/dev/ttyUSB0";
	var timerInterval = process.argv[2] || 900000;
	
	function startupSerial( serialPortName ) {

		console.log( 'Opening: ' + serialPortName );
		return new SerialPort(serialPortName, {
			baudrate: 9600,
			databits: 8,
			parity: 'none',
			stopbits: 1,
			parser: serialport.parsers.readline("\n")
		});
	}	
	
	var sp = startupSerial( serialPortName );

	var response="";
	var fromArduino= {};
	var evalResponse="";
	var outerTimerInterval 
	
	sp.on( "open", function() {
		console.log(serialPortName + ' opened.');
		console.log('Setting up main interval to ' + timerInterval / 1000. + 's' );
		// Diamo ad Arduino il tempo di "svegliarsi" dopo un reset
		console.log( "Waiting for Arduino to become ready." );
		setTimeout( setUpMainTimer, 5000 ) ;

		sp.on("data", function( data ) {
			response = response + "\n" + data;
			// console.log( "--" + data );
			if (data[0] == "}") {
				evalResponse = "(" + response + ")";
				try {
					fromArduino = eval( evalResponse );
					fromArduino["@timestamp"] = new Date();
					console.log ( fromArduino );
					try {
						putDataIntoES( fromArduino, 'GET-RA' );
					} catch (f) {
						console.log("Errore nell'invio dai dati: " + f );
					}
				} catch (e) {
					console.log("Errore nel parsing della risposta");	
				}
				response = "";
			}
		});
	});
	
	// Imposta il timer Principale
	function setUpMainTimer() {
		outerTimerInterval = setInterval( askDataToArduino, timerInterval );
		askDataToArduino(); // La prima chiamata viene fatta appena aperta la seriale!
	}

	// Invia il comando "GET" ad arduino per recuperarne i dati
	function askDataToArduino() {
		console.log( 'Asking for data' );
		sp.write("GET-RA\r", function( err, results ) {
			if (err) {
				console.log( 'Errore nella scrittura su Seriale ' + err );
			}
			console.log( 'Bytes written ' + results );
		});
	
	}
	
	// inserisce i dati in elasticsearch
	function putDataIntoES( dataToBePut, dataType ) {
		var indexPath = "/caldaia/raw-data";

		if (dataType) {
			if (dataType === "GET-RA" ){
				indexPath = "/caldaia-ra-"+moment().utc().format('YYYY-MM-DD')+"/raw-data";
			}
		}

		var options = {
			host: 'es.casa.local',
			port: 9200,
			path: indexPath,
			method: 'POST'
		};
	
		var req = http.request(options, function(res) {
			console.log('METHOD: ' + options.method );
			console.log('URL: ' + options.host + options.path );
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
	
})()
