var serialport = require("serialport");
var SerialPort = serialport.SerialPort;
var sp = new SerialPort("/dev/ttyUSB0", {
	baudrate: 9600,
	databits: 8,
	parity: 'none',
	stopbits: 1,
	parser: serialport.parsers.readline("\n")
});

var response="(";
var fromArduino= {};

sp.on( "open", function() {
	console.log('open');
	sp.on('data', function( data ) {
		response = response + "\n" + data;
		// console.log( "--" + data );
		if (data[0] == "}") {
			response += ")";
			try {
				fromArduino = eval( response );
				fromArduino.timeStamp = new Date();
				console.log ( fromArduino );
			} catch (e) {
				console.log("Errore nel parsing della risposta");	
			}
			response = "(";
		}
		// console.log(data + "");	
	});
	sp.write("GET\n", function( err, results ) {
		console.log( 'err ' + err );
		console.log( 'results ' + results );	
	});
});

