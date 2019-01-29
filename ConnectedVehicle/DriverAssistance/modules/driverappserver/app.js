'use strict';

var Transport = require('azure-iot-device-mqtt').Mqtt;
var Client = require('azure-iot-device').ModuleClient;
var Message = require('azure-iot-device').Message;
const express = require("express");
const http = require("http");
const socketIo = require("socket.io");
const port = process.env.PORT || 4001;
const index = require("./routes/index");
const app = express();
app.use(index);
const server = http.createServer(app);
const io = socketIo(server); // < Interesting!

var temperatureThreshold = 25;

Client.fromEnvironment(Transport, function (err, client) {
  if (err) {
    throw err;
  } else {
    client.on('error', function (err) {
      throw err;
    });

    // connect to the Edge instance
    client.open(function (err) {
      if (err) {
        throw err;
      } else {
        console.log('IoT Hub module client initialized');

        io.on("connection", socket => {
          console.log("New client connected");
          // Act on input messages to the module.
          client.on('inputMessage', function (inputName, msg) {
            // console.log('Receiving message on ' + inputName);
            filterMessage(client, inputName, msg, socket);
          });

          //listen for disconnect
          socket.on("disconnect", () => console.log("Client disconnected"));
        });

        client.getTwin(function (err, twin) {
          if (err) {
            console.error('Error getting twin: ' + err.message);
          } else {
            console.log('Property update received');
            twin.on('properties.desired', function (delta) {
              if (delta.TemperatureThreshold) {
                temperatureThreshold = delta.TemperatureThreshold;
              }
            });
          }
        });
      }
    });
  }
});

server.listen(port, () => console.log(`Listening on port ${port}`));

const postPayload = async (socket, payload) => {
  try {
    socket.emit("FromAPI", payload);
  } catch (error) {
    console.error(`Error: ${error.code}`);
  }
};

// This function filters out messages that report temperatures below the temperature threshold.
// It also adds the MessageType property to the message with the value set to Alert.
function filterMessage(client, inputName, msg, socket) {
  client.complete(msg, printResultFor('Receiving message'));
  if (inputName === 'input1') {
    var message = msg.getBytes().toString('utf8');
    var messageBody = JSON.parse(message);

    //Post the payload to websocket
    postPayload(socket, { "temperature": Math.floor(messageBody.ambient.temperature), "humidity": Math.floor(messageBody.ambient.humidity), "speed": Math.floor(Math.random() * 100), "version": "1.2" });

    if (messageBody && messageBody.machine && messageBody.machine.temperature && messageBody.machine.temperature > temperatureThreshold) {
      console.log(`Machine temperature ${messageBody.machine.temperature} exceeds threshold ${temperatureThreshold}`);
      var outputMsg = new Message(message);
      outputMsg.properties.add('MessageType', 'Alert');
      client.sendOutputEvent('output1', outputMsg, printResultFor('Sending received message'));
    }
  }
}

// Helper function to print results in the console
function printResultFor(op) {
  return function printResult(err, res) {
    if (err) {
      console.log(op + ' error: ' + err.toString());
    }
    if (res) {
      console.log(op + ' status: ' + res.constructor.name);
    }
  };
}