import React, { Component } from "react";
import socketIOClient from "socket.io-client";
import logo from './images/contosologo.jpg';
import './App.css';
import Chart from 'react-google-charts';

class App extends Component {
  constructor() {
    super();
    this.state = {
      response: false,
      endpoint: "http://127.0.0.1:4001"
    };
  }
  componentDidMount() {
    const { endpoint } = this.state;
    const socket = socketIOClient(endpoint);
    socket.on("FromAPI", data => {
      console.log(data);
      this.setState({
        response: data
      });
    });
  }
  render() {
    const { response } = this.state;
    return (
      <div className="App">
        <header className="App-header">
          <table className="App-header-table">
            <tbody>
              <tr>
                <td align="left">
                  <img src={volvologo} className="App-logo" alt="logo" />
                  <p style={{ fontSize: 24, fontStyle: "bold" }}>Contoso Drive</p>
                </td>
                <td>
                  <p>Driver Assistance</p>
                </td>
                <td>{response
                  ? <p>Version {response.version}</p> 
                  : <p>Loading...</p>}
                </td>
              </tr>
            </tbody>
          </table>
          {response
            ?
            <div style={{ display: 'flex', width: "90%", alignItems: "center", justifyContent: "center" }}>
              <Chart
                width={200}
                height={120}
                chartType="Gauge"
                loader={<div>Loading Chart</div>}
                data={[
                  ['Label', 'Value'],
                  ['Temperature', response.temperature]
                ]}
                options={{
                  redFrom: 35,
                  redTo: 50,
                  yellowFrom: 25,
                  yellowTo: 35,
                  minorTicks: 5,
                }}
                rootProps={{ 'data-testid': '1' }}
              />
              <Chart
                width={200}
                height={120}
                chartType="Gauge"
                loader={<div>Loading Chart</div>}
                data={[
                  ['Label', 'Value'],
                  ['Humidity', response.humidity]
                ]}
                options={{
                  redFrom: 90,
                  redTo: 100,
                  yellowFrom: 80,
                  yellowTo: 90,
                  minorTicks: 5,
                }}
                rootProps={{ 'data-testid': '2' }}
              />
              <Chart
                width={200}
                height={120}
                chartType="Gauge"
                loader={<div>Loading Chart</div>}
                data={[
                  ['Label', 'Value'],
                  ['Speed', response.speed]
                ]}
                options={{
                  redFrom: 90,
                  redTo: 100,
                  yellowFrom: 80,
                  yellowTo: 90,
                  minorTicks: 5,
                }}
                rootProps={{ 'data-testid': '3' }}
              />
            </div>
            : <p>Loading...</p>}
          <br />
        </header>
      </div>
    );
  }
}
export default App;