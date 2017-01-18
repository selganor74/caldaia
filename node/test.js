'use strict';
(function() {
	var moment = require("moment");
	var http = require("http");
	var serialport = require("serialport");
	var SerialPort = serialport.SerialPort;

	var timerInterval  = parseInt( process.argv[2] || 900000 ); // Tempo di acquisizione su Elastic search
	var innerCycle     = parseInt( process.argv[3] ||  30000 ); // Tempo di aggiornamento stato attuale.
	var serialPortName = process.argv[4] || "/dev/ttyUSB1"; // Porta seriale da utilizzare.

	var sp; 

	var response="";
	var fromArduino= {};
	var evalResponse="";
	var outerTimerInterval ;
	var whatIndexToFill;
	var elapsed=parseInt( "0" );
	var timeoutTimer; // Timer di timeout. Se la risposta non arriva prima che il timer scatti, bisogna resettare la seriale.

	startupSerial();
	//closeAndReopenSerial();

	function startupSerial() {

		console.log( 'Opening: ' + serialPortName );
		sp = new SerialPort(serialPortName, {
			baudrate: 9600,
			databits: 8,
			parity: 'none',
			stopbits: 1,
			parser: serialport.parsers.readline("\n")
		});
		sp.on( "open", function() {
			console.log(serialPortName + ' opened.');
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
						console.log ( "Inserting into Elastic Search for index: " + whatIndexToFill );
						try {
							if ( whatIndexToFill == "RA" ) {
								putDataIntoES( fromArduino, 'GET-RA' );
							} else {
								putDataIntoES( fromArduino, 'CURRENT' );
							}
						} catch (f) {
							console.log("Error sending data: " + f );
						}
						// Data has arrived so no need to reset serial !!!
						cancelTimeoutTimer();
					} catch	(e) {
						console.log("Error parsing response: " + e );	
					}
					response = "";
				}
			});
		});

	}	
	

	
	// Imposta il timer Principale
	function setUpMainTimer() {
		console.log('Setting up main interval to ' + timerInterval / 1000. + 's' );
		console.log('Setting up inner interval to ' + innerCycle / 1000. + 's' );
		outerTimerInterval = setInterval( askDataToArduino, innerCycle );
		askDataToArduino(); // La prima chiamata viene fatta appena aperta la seriale!
	}

	// Invia il comando "GET" ad arduino per recuperarne i dati
	function askDataToArduino() {
		elapsed += innerCycle;
		var command = "GET\r";
		whatIndexToFill = "CURRENT";
		if ( elapsed >= timerInterval ) {
			whatIndexToFill = "RA";
			command = "GET-RA\r";	
			elapsed = 0;	
		}
		startTimeoutTimer( 4000 );
		console.log( 'Asking for data with command: ' + command + ' elapsed: ' + elapsed );
		try {
			sp.write(command, function( err, results ) {
				if (err) {
					console.log( 'Error in writing to Serial: ' + err );
				}
				console.log( 'Bytes written ' + results );
			});
		} catch(e) {
			console.log( "Unable to write to Serial: " + e );
		}
	
	}
	
	// inserisce i dati in elasticsearch
	function putDataIntoES( dataToBePut, dataType ) {
		var indexPath = "/caldaia/raw-data";

		if (dataType) {
			if (dataType === "GET-RA" ){
				indexPath = "/caldaia-ra-"+moment(dataToBePut['@timestamp']).utc().format('YYYY-MM-DD')+"/raw-data";
			}
			if (dataType === "CURRENT" ) {
				indexPath = "/caldaia-current/current/1";
			}
		}

		var options = {
			host: 'es.casa.local',
			port: 9200,
			path: indexPath,
			method: dataType === "GET-RA" ? 'POST' : 'PUT'
		};
	
		var req = http.request(options, function(res) {
			console.log('METHOD: ' + options.method );
			console.log('URL: ' + options.host + options.path );
			console.log('STATUS: ' + res.statusCode);
			console.log('HEADERS: ' + JSON.stringify(res.headers));
			res.setEncoding('utf8');
			res.on('data', function (chunk) {
				console.log('BODY: ' + chunk);
				if (dataType === "CURRENT") saveDataToDisk( dataToBePut, dataType );
			});
			res.on('error', function (chunk) {
				console.log('Error: BODY: ' + chunk);
				if (dataType === "CURRENT") saveDataToDisk( dataToBePut, dataType );
			});
		});
	
		req.on('error', function(e) {
			console.log('problem with request: ' + e.message);
			saveDataToDisk( dataToBePut, dataType );
		});
	
		// write data to request body
		req.write(JSON.stringify(dataToBePut)+'\n');
		req.end();
	}  
	
	function saveDataToDisk( dataToBePut, dataType ) {
		console.log('saveDataToDisk');
		var tmpPath = "/tmp/";
		
		var fs = require('fs');
		var mkdirp = require('mkdirp');

		var saveDir = tmpPath + dataType;
		var saveTo;
		if (dataType === 'CURRENT' ) {
			saveTo = '/run/caldaia/CURRENT.json';
		} else {
			saveTo = saveDir + '/' + dataToBePut['@timestamp'] + '.json';
		}
		console.log('Saving data to ' + saveTo + ' for later use.' );

		mkdirp( saveDir, function( err ) {
			if ( err ) { 
				console.log ( "Trouble in creating directory " + saveTo + ": " + err );			
				return;
			}

			fs.writeFile( saveTo, JSON.stringify( dataToBePut ), function( err ) {
				if (err) {
					console.log( "Unable to save file! " + err );
				} else {
					console.log( "Saved!" );
				}
			} );
		} );
	
	}


	function closeAndReopenSerial() {
		function reopen() {
			console.log( "Reopening Serial..." );
			startupSerial();
		}
		console.log( "Closing Serial..." );
		try {
			outerTimerInterval.clearInterval();
		} catch(u) {
			console.log( 'Unable to cancel Timer... maybe not yet set ?' );
		}
		if (sp) {
			sp.close( reopen );
		} else {
			reopen();
		}
	}

	function startTimeoutTimer( tms ) {
		timeoutTimer = setTimeout( closeAndReopenSerial, tms );
		console.log( 'Countdown started ... ' + timeoutTimer );
	}

	function cancelTimeoutTimer() {
		console.log('Called in time... canceling timeout! Phew!');
		try {
			clearTimeout(timeoutTimer);
		} catch(e) {
			console.log( 'Error clearing timeout! ' + e );
		}
	}
	
})()
