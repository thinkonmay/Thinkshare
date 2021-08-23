import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:thinkmay_admin_client/models/Slave.dart';

class CreateSlave{
  Future createSlave(String id) async {
  final response = await http.post(
    Uri.parse('https://192.168.1.2:49169/Admin/AddSlaved'),
    headers: <String, String>{
      'Content-Type': 'application/json; charset=UTF-8',
    },
    body: jsonEncode(<String, String>{
      'ID': id,
    }),
  );

  if (response.statusCode == 200) {
    // If the server did return a 200 CREATED response,
    // then parse the JSON.
    print("Success for send reponse");
    return true;
    // Slave.fromJson(jsonDecode(response.body));
  } else {
    // If the server did not return a 201 CREATED response,
    // then throw an exception.
    print('Failed to create slave');
    throw Exception('Failed to create slave.');
  }
}
  FutureBuilder<Slave> buildFutureBuilder(Future<Slave> _futureSlave) {
    return FutureBuilder<Slave>(
      future: _futureSlave,
      builder: (context, snapshot) {
        if (snapshot.hasData) {
          return Container(
            child: ListView(children: <Widget>[
              Text("${snapshot.data!.ID}"),
              Text(snapshot.data!.CPU),
              Text(snapshot.data!.GPU),
              Text("${snapshot.data!.RAMcapacity}"),
              Text(snapshot.data!.OS),
            ]),
          );
        } else if (snapshot.hasError) {
          return Text('${snapshot.error}');
        }
        return const CircularProgressIndicator();
      },
    );
  }
}