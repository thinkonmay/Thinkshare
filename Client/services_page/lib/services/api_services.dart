import 'dart:convert';

import 'package:admin/models/ServingSession.dart';
import 'package:admin/models/Slave.dart';
import 'package:admin/screens/login/login_screen.dart';
import 'package:admin/utils/server_config.dart';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;

Future<List<Slave>> userFetchSlave() async {
   final response = await http.get(
      Uri.parse('$urlServer/User/FetchSlave?UserID=$clientID'),
      headers: <String, String>{
        'Content-Type': 'application/json; charset=UTF-8',
      },
    );
  return compute(parseSlaves, response.body);
}

List<Slave> parseSlaves(String responseBody) {
  final parsed = jsonDecode(responseBody).cast<Map<String, dynamic>>();
  return parsed.map<Slave>((json) => Slave.fromJson(json)).toList();
}

Future<List<Slave>> userFetchSession() async {
   final response = await http.get(
      Uri.parse('$urlServer/User/FetchSession?UserID=$clientID'),
      headers: <String, String>{
        'Content-Type': 'application/json; charset=UTF-8',
      },
    );
  return compute(parseSession, response.body);
}

List<Slave> parseSession(String responseBody) {
  final parsed = jsonDecode(responseBody).cast<Map<String, dynamic>>();
  return parsed.map<Slave>((json) => Slave.fromJson(json)).toList();
}