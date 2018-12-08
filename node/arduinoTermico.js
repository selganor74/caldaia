'use strict'
var events = require('events');
// Arduino serial port manager
function ArduinoSerialPort(
	serialPortName
) {
	var SerialPort = require("serialport").SerialPort;
	var sp; // Rappresenta la porta seriale "fisicamente" in uso.

	var serialPortStatus = "not initialized";

	var serialBuffer = "";

	events.EventEmitter.call(this); // Chiama il costruttore di EventEmitter

	this.init = function()	{
		sp = new SerialPort(serialPortName, {
                        baudrate: 9600,
                        databits: 8,
                        parity: 'none',
                        stopbits: 1,
                        parser: serialport.parsers.readline("\n")
		});

		serialPortStatus = "Initializing";

		sp.on( "open", function() {
			serialPortStatus = "initialized";
			this.emit('sp_initialized');
		});

		sp.on( "", this.serialDataHandler );
	}

	this.serialDataHandler = function( data ) {
			
	}

	this.askData = function() {

		if( serialPortStatus != 'initialized" ) {
			console.log('ArduinoSerialPort.askData: serial port not initialized!');
			return;
		}

		var command = "GET\r";
		
		sp.write( command, function( err, bytesWritten ) {
			if (err) {
				console.log( 'ArduinoSerialPort.askData: errors writing command ' + command + ' to serial port ' + serialPortName + ' - ' + JSON.stringify(err);
				return;
			}	
			console.log( 'ArduinoSerialPort.askData: command ' + command + ' correctly sent. ' + bytesWritten + ' bytes written.' );
		});		
	}
}

// ArduinoSerialPort eredita da event emitter... in typescript sarebbe più leggibile.
ArduinoSerialPort.prototype.__proto__ = events.EventEmitter.prototype;
