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
        await http.get(Uri.parse("http://125.212.237.45:81/Admin/System"));
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
                                  title: new Text(),
                                  content: Text("Slave ID: ${randomNumber}"),
                                  actions: <Widget>[
                                    ElevatedButton(
                                      child: Text('Add'),
                                      onPressed: () async {
                                        Navigator.of(context).pop();
                                        final response = await http.post(
                                          // Uri.parse('https://localhost:port/Admin/AddSlave'),
                                          Uri.parse(
                                              'http://125.212.237.45:81/Admin/AddSlave?ID=$randomNumber'),
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
