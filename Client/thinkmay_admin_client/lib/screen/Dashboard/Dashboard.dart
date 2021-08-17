import 'dart:convert';
import 'dart:io';
import 'dart:math';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_vector_icons/flutter_vector_icons.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:http/io_client.dart';
import 'package:signalr_core/signalr_core.dart';
import 'package:thinkmay_admin_client/components/app_tools.dart';
import 'package:thinkmay_admin_client/components/responsive_layout.dart';
import 'package:thinkmay_admin_client/models/Slave.dart';
import 'package:thinkmay_admin_client/screen/Dashboard/src/ProjectProgressCard.dart';
import 'package:http/http.dart' as http;

class Dashboard extends StatefulWidget {
  @override
  _DashboardState createState() => _DashboardState();
}

class _DashboardState extends State<Dashboard> {
  Future<void> main(List<String> arguments) async {
    final connection = HubConnectionBuilder()
        .withUrl(
            'http://localhost:5000/chatHub',
            HttpConnectionOptions(
              client: IOClient(
                  HttpClient()..badCertificateCallback = (x, y, z) => true),
              logging: (level, message) => print(message),
            ))
        .build();

    await connection.start();

    connection.on('ReceiveMessage', (message) {
      print(message.toString());
      // set data for slave
    });

    await connection.invoke('SendMessage', args: ['Bob', 'Says hi!']);
  }

  Future<List<Slave>> getData() async {
    http.Response response =
        await http.get(Uri.parse("http://192.168.1.6:81/Admin/System"));
    return compute(parseSlaves, response.body);
  }

  List<Slave> parseSlaves(String responseBody) {
    final parsed = jsonDecode(responseBody).cast<Map<String, dynamic>>();

    return parsed.map<Slave>((json) => Slave.fromJson(json)).toList();
  }

  @override
  void initState() {
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return Positioned(
      left: ResponsiveLayout.isMacbook(context) ? 100 : 0.0,
      child: Container(
        height: MediaQuery.of(context).size.height,
        width: ResponsiveLayout.isMacbook(context)
            ? MediaQuery.of(context).size.width * 0.63
            : MediaQuery.of(context).size.width,
        color: Colors.white,
        child: SingleChildScrollView(
          child: Column(
            crossAxisAlignment: ResponsiveLayout.isMacbook(context)
                ? CrossAxisAlignment.start
                : CrossAxisAlignment.center,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: <Widget>[
                  Container(
                    margin:
                        EdgeInsets.only(left: 30.0, top: 25.0, bottom: 10.0),
                    child: Text('ThinkMay - Admin Hub',
                        style: GoogleFonts.quicksand(
                            fontWeight: FontWeight.bold, fontSize: 20.0)),
                  ),
                  Padding(
                    padding: new EdgeInsets.all(20.0),
                    child: new RaisedButton(
                      color: Colors.greenAccent,
                      shape: new RoundedRectangleBorder(
                          borderRadius:
                              new BorderRadius.all(new Radius.circular(15.0))),
                      onPressed: () {
                        Random random = new Random();
                        int randomNumber = random.nextInt(999999) + 100000;
                        showDialog(
                            context: context,
                            builder: (_) => new AlertDialog(
                                  title: new Text(''' 
                                  <!DOCTYPE html>
<html>

<head>
    <link href="https://fonts.googleapis.com/css?family=Roboto:100,300,400,500,700,900|Material+Icons" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/vuetify/1.5.14/vuetify.min.css" rel="stylesheet" />
    <style>
        html {
            font-family: Roboto, Arial, sans;
            overflow: hidden;
        }

        .scrolly textarea {
            min-height: 300px;
            white-space: pre;
            overflow: scroll;
        }

        .fab-container {
            top: 50%;
            right: -25px;
        }

        .video-container {
            background-color: black;
            width: 100%;
            height: 100%;
        }

            .video-container video {
                width: 100%;
                height: 100%;
                position: absolute;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
            }

        .loading {
            position: absolute;
            top: 50%;
            width: 100%;
            text-align: center;
            color: #E0E0E0;
        }

        .loading-text {
            margin-top: 1em;
        }

        canvas {
            background: black;
        }

        ul {
            border: 1px solid #000000;
            border-radius: 8px;
        }

        .small-size {
            font-size: 10px;
        }

        .wrapper {
            display: flex;
            flex-direction: row;
            align-items: center;
        }

            .wrapper .button {
                border: 1px solid #000000;
                border-radius: 8px;
                margin: 0.5rem 2rem;
                padding: 0.5rem 1rem;
                background: pink;
            }

                .wrapper .button:hover {
                    opacity: 0.7;
                    cursor: pointer;
                }

            .wrapper input {
                border: 1px solid #595959;
                border-radius: 8px;
                height: 2.5rem;
            }

                .wrapper input:focus {
                    outline: none;
                }
    </style>
    <title>WebRTC</title>
</head>

<body>
    <div id="app">
        <v-app>
            <v-navigation-drawer v-model="showDrawer" app fixed right temporary width="600">
                <v-container fluid grid-list-lg>
                    <v-layout row wrap>
                        <v-flex xs12>
                            <p>
                                <v-toolbar>
                                    <v-tooltip bottom>
                                        <template v-slot:activator="{on}">
                                            <v-btn icon v-on:click="enterFullscreen()">
                                                <v-icon color="black" v-on="on">fullscreen</v-icon>
                                            </v-btn>
                                        </template>
                                        <span>Enter fullscreen mode</span>
                                    </v-tooltip>


                                    <v-tooltip bottom>
                                        <template v-slot:activator="{on}">
                                            <v-btn icon v-on:click="resetStream()">
                                                <v-icon color="black" v-on="on">gavel</v-icon>
                                            </v-btn>
                                        </template>
                                        <span>Reset Remote Control connection</span>
                                    </v-tooltip>


                                    <v-tooltip bottom>
                                        <template v-slot:activator="{on}">
                                            <v-btn icon v-on:click="connectServer()">
                                                <v-icon color="black" v-on="on">widgets</v-icon>
                                            </v-btn>
                                        </template>
                                        <span>Connect to Server</span>
                                    </v-tooltip>


                                    <v-tooltip bottom>
                                        <template v-slot:activator="{on}">
                                            <v-btn icon v-on:click="enterFullscreen()">
                                                <v-icon color="black" v-on="on">fullscreen</v-icon>
                                            </v-btn>
                                        </template>
                                        <span>Enter fullscreen mode</span>
                                    </v-tooltip>


                                </v-toolbar>



                            <p></p>
                            <ul>
                                <li>Peer connection state: <b>{{ status }}</b></li>
                                <li>Packets lost: <b>{{ adaptivePacketsLost }}</b></li>
                            </ul>
                            Bandwidth Stats:
                            <ul>
                                <li>Video receive rate: <b>{{ adaptiveVideoBitrate }}</b></li>
                                <li>Available receive bandwith: <b>{{ adaptiveTotalBandwidth }}</b></li>
                            </ul>
                            Video Stats
                            <ul>
                                <li>Video Latency: <b>{{ adaptiveVideoLatency }} ms</b></li>
                                <li>Audio Latency: <b>{{ adaptiveAudioLatency }} ms</b></li>
                                <li>Video Codec: <b>{{ connectionVideoCodecName }} </b></li>
                                <li>Received Screen Size: <b>{{ connectionResolution }} </b></li>
                                <li>Display Screen size: <b>{{ windowResolution }} </b></li>
                                <li>Video decoder: <b>{{ connectionVideoDecoder }}</b></li>
                                <li>Frame rate: <b>{{ adaptiveFramerate }} fps</b></li>
                                <li>Bit rate: <b>{{ adaptiveVideoBitrate }}</b></li>
                            </ul>
                            Audio Stats
                            <ul>
                                <li>Latency: <b>{{ adaptiveAudioLatency }} ms</b></li>
                                <li>Codec: <b>{{ connectionAudioCodecName }}</b></li>
                                <li>Bit rate: <b>{{ adaptiveAudioBitrate }} kbps</b></li>
                            </ul>





                            <hr />
                            <v-textarea bottom class="scrolly small-size" label="Debug Logs" readonly :value="debugEntries.join('\n\n')">
                            </v-textarea>
                            <v-textarea bottom class="scrolly small-size" label="Status Log" readonly :value="logEntries.join('\n\n')">
                            </v-textarea>


                        </v-flex>
                    </v-layout>
                </v-container>
            </v-navigation-drawer>

            <div id="video_container" class="video-container">
                <video id="stream" preload="none" playsinline>
                    Your browser doesn't support video
                </video>
            </div>

            <canvas id="capture"></canvas>

            <v-btn class="fab-container" v-on:click="showDrawer=!showDrawer" color="grey" fab dark fixed right>
                <v-icon>chevron_left</v-icon>
            </v-btn>




            <div class="loading">



                <div v-if="status === 'failed'">
                    <v-btn v-on:click="location.reload()" color="#E0E0E0">
                        reload
                    </v-btn>
                    <div class="loading-text">Connection failed.</div>
                </div>



                <div v-else>
                    <scale-loader size="200px" :loading="(status !== 'connected')" color="#E0E0E0"></scale-loader>
                    <div v-if="(status !== 'connected')" class="loading-text">{{ loadingText }}</div>
                </div>
            </div>
        </v-app>
    </div>
</body>

<script src="https://webrtc.github.io/adapter/adapter-latest.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/vue/2.6.9/vue.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/vuetify/1.5.14/vuetify.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/vue-spinner@1.0.3/dist/vue-spinner.min.js"></script>

<script src="src/enum.js?ts=1"></script>
<script src="src/input.js?ts=1"></script>
<script src="src/signalling.js?ts=1"></script>
<script src="src/webrtc.js?ts=1"></script>
<script src="src/app.js?ts=1"></script>



<script type="text/javascript">
    var clientSession =
    {
        "SessionClientID":  456985200 ,
        "SignallingUrl": "ws://192.168.1.6:82",
        "ClientOffer": False,

        "QoE":
        {
            "ScreenWidth": 0,
            "ScreenHeight": 0,
            "Framerate": 0,
            "Bitrate": 0,
            "AudioCodec": 0,
            "VideoCodec": 0,
            "QoEMode": 0
        }
    }

    var RTPconfig =     
    {"iceServers":    
        [
            {
                "urls": "stun:stun.l.google.com:19302" 
            }
        ]
    }

    var HostUrl = "";

    var ClientID = 299381905
</script>

</html>
                                  '''),
                                  content: Text("Slave ID: ${randomNumber}"),
                                  actions: <Widget>[
                                    ElevatedButton(
                                      child: Text('Add'),
                                      onPressed: () async {
                                        Navigator.of(context).pop();
                                        final response = await http.post(
                                          // Uri.parse('https://localhost:port/Admin/AddSlave'),
                                          Uri.parse(
                                              'http://192.168.1.6:81/Admin/AddSlave?ID=$randomNumber'),
                                          headers: <String, String>{
                                            'Content-Type':
                                                'application/json; charset=UTF-8',
                                          },
                                        );

                                        if (response.statusCode == 200) {
                                          // If the server did return a 201 CREATED response,
                                          // then parse the JSON.
                                          showMaterialDialog(
                                              context: context,
                                              title: "Add Slave Success",
                                              content:
                                                  "Your Slave have generated",
                                              confirmText: "Exit");
                                        } else if (response.statusCode == 400) {
                                          showMaterialDialog(
                                              context: context,
                                              title: "Add Slave Fail",
                                              content:
                                                  "Bad request! \n Try again!",
                                              confirmText: "Exit");
                                        } else {
                                          // If the server did not return a 201 CREATED response,
                                          // then throw an exception.
                                          showMaterialDialog(
                                              context: context,
                                              title: "Add Slave Fail",
                                              content:
                                                  "Your ID have been existed \n Try again!",
                                              confirmText: "Exit");
                                          print('Failed to create slave');
                                          throw Exception(
                                              'Failed to create slave.');
                                        }
                                      },
                                    ),
                                    ElevatedButton(
                                      child: Text('Cancer'),
                                      onPressed: () {
                                        Navigator.of(context).pop();
                                      },
                                    ),
                                  ],
                                ));
                      },
                      child: Container(
                        height: 50.0,
                        width: 100.0,
                        padding: const EdgeInsets.all(8.0),
                        child: Center(
                          child: new Text(
                            "Add Slave",
                            style:
                                TextStyle(color: Colors.black, fontSize: 18.0),
                          ),
                        ),
                      ),
                    ),
                  ),
                ],
              ),

              // Tabs(),
              FutureBuilder<List<Slave>>(
                  future: getData(),
                  builder: (context, snapshot) {
                    if (snapshot.hasData) {
                      print(snapshot.data);
                      List<Slave>? slaves = snapshot.data;
                      return GridView.count(
                        shrinkWrap: true,
                        crossAxisCount: 3,
                        mainAxisSpacing: 4.0,
                        crossAxisSpacing: 4.0,
                        childAspectRatio: 1.0,
                        padding: const EdgeInsets.all(4.0),
                        children: slaves!.map<Widget>((Slave slave) {
                          // return Text("${slave.ID}");
                          return Container(
                            margin: EdgeInsets.only(top: 5.0),
                            height: ResponsiveLayout.isMacbook(context)
                                ? 225.0
                                : MediaQuery.of(context).size.height * 0.6,
                            width: ResponsiveLayout.isMacbook(context)
                                ? MediaQuery.of(context).size.width * 0.62
                                : 305.0,
                            child: Padding(
                              padding: EdgeInsets.all(10.0),
                              child: ProjectProgressCard(
                                  color: Color(0xffFF4C60),
                                  progressIndicatorColor: Colors.blue[200],
                                  slaveName: "Device 1",
                                  state: true,
                                  id: slave.ID,
                                  CPU: slave.CPU,
                                  GPU: slave.GPU,
                                  RAMcapacity: slave.RAMcapacity,
                                  OS: slave.OS,
                                  CommandLogs: "slave.CommandLogs",
                                  ServingSession: "slave.ServingSession",
                                  GeneralErrors: "slave.GeneralErrors",
                                  SessionCoreExits: "slave.SessionCoreExits",
                                  icon: Icons.computer,
                                  date: '10 Nov 2020'),
                            ),
                          );
                        }).toList(),
                      );
                    } else {
                      return Center(
                        child: CircularProgressIndicator(),
                      );
                    }
                  }), //         // return GridView.count(
              ResponsiveLayout.isMacbook(context)
                  ? Container(
                      margin: EdgeInsets.only(top: 5.0),
                      width: ResponsiveLayout.isMacbook(context)
                          ? MediaQuery.of(context).size.width * 0.62
                          : MediaQuery.of(context).size.width,
                      child: Column(
                        children: [],
                      ))
                  : Container(),
            ],
          ),
        ),
      ),
    );
  }
}
